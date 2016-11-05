﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Localization.Core;

namespace Localization.iOS
{
	public class LocalizationIOSTask : BaseLocalizationTask
	{
		protected override void Generate(Dictionary<string, List<ResxFile>> files)
		{
			List<string> keys = new List<string>();

			foreach (string directory in files.Keys)
			{
				GenerateStrings(directory, files[directory]);

				keys.AddRange(files[directory].SelectMany(x => x.Content.Keys.Select(y => y.SimplifyKey())));
			}

			keys = keys.Distinct().ToList();

			GenerateLocalizationService(keys);
			GenerateLocalizedStrings(keys);

			base.Generate(files);
		}

		protected virtual void GenerateStrings(string directory, List<ResxFile> files)
		{
			Dictionary<string, string> content = new Dictionary<string, string>();
			Dictionary<string, string> platformSpecificContent = new Dictionary<string, string>();

			foreach (var item in files.SelectMany(x => x.Content))
			{
				if (item.Key.IsPlatformSpecificString())
				{
					if (item.Key.IsIosString())
					{
						platformSpecificContent.Add(item.Key, ProcessValue(item.Value));
					}
				}
				else
				{
					content.Add(item.Key, ProcessValue(item.Value));
				}
			}

			foreach (var item in platformSpecificContent)
			{
				string key = item.Key.SimplifyKey();
				if (content.ContainsKey(key))
				{
					content[key] = item.Value;
				}
				else
				{
					content.Add(key, item.Value);
				}
			}
			string outputFile = Path.Combine(directory, "Localizable.strings");
			File.WriteAllLines(outputFile, content.Select(x => $"\"{x.Key}\" = \"{x.Value}\";"));
			OutputResourceFilePath.Add(outputFile);
		}

		protected virtual void GenerateLocalizationService(List<string> keys)
		{
			CodeCompileUnit codeUnit = new CodeCompileUnit();

			// for class declaration
			CodeNamespace codeNamespace = new CodeNamespace(GenerationNamespace);
			codeUnit.Namespaces.Add(codeNamespace);

			codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
			codeNamespace.Imports.Add(new CodeNamespaceImport("Foundation"));

			// create class
			CodeTypeDeclaration classDeclaration = new CodeTypeDeclaration(Constants.IMPLEMENTATION_SERVICE_NAME)
			{
				IsClass = true,
				TypeAttributes = TypeAttributes.Public,
			};
			classDeclaration.BaseTypes.Add(Constants.INTERFACE_SERVICE_NAME);
			codeNamespace.Types.Add(classDeclaration);
			
			//methode
			var method = new CodeMemberMethod
			{
				Name = Constants.SERVICE_METHOD_NAME,
				ReturnType = new CodeTypeReference(typeof(string)),
				Attributes = MemberAttributes.Public
			};
			method.Parameters.Add(new CodeParameterDeclarationExpression(Constants.ENUM_NAME, "key"));
			classDeclaration.Members.Add(method);

			CodeVariableReferenceExpression methodParam = new CodeVariableReferenceExpression("key");
			CodeMethodReferenceExpression localizedStringsMethodReference = new CodeMethodReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("NSBundle"), "MainBundle"), "LocalizedString");

			foreach (string key in keys)
			{
				CodeConditionStatement condition = new CodeConditionStatement(
					new CodeBinaryOperatorExpression(
						methodParam,
						CodeBinaryOperatorType.IdentityEquality,
						new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(Constants.ENUM_NAME), key)
						),
					new CodeMethodReturnStatement(
						new CodeMethodInvokeExpression(localizedStringsMethodReference, new CodePrimitiveExpression(key), new CodePrimitiveExpression(null))
						)
					);

				method.Statements.Add(condition);
			}

			method.Statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(ArgumentOutOfRangeException))));

			codeUnit.WriteToFile(Constants.IMPLEMENTATION_SERVICE_FILE_PATH, "This file was generated by Localization task for iOS");
			OutputCompileFilePath.Add(Constants.IMPLEMENTATION_SERVICE_FILE_PATH);
		}

		protected virtual void GenerateLocalizedStrings(List<string> keys)
		{
			CodeCompileUnit codeUnit = new CodeCompileUnit();

			// for class declaration
			CodeNamespace codeNamespace = new CodeNamespace(GenerationNamespace);
			codeUnit.Namespaces.Add(codeNamespace);

			codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
			codeNamespace.Imports.Add(new CodeNamespaceImport("Foundation"));

			// create class
			CodeTypeDeclaration classDeclaration = new CodeTypeDeclaration(Constants.LOCALIZED_STRINGS_NAME)
			{
				IsClass = true,
				TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed,
			};
			codeNamespace.Types.Add(classDeclaration);
			
			//constructor
			CodeConstructor constructor = new CodeConstructor
			{
				Attributes = MemberAttributes.Private
			};
			classDeclaration.Members.Add(constructor);
			
			CodeMethodReferenceExpression localizedStringsMethodReference = new CodeMethodReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("NSBundle"), "MainBundle"), "LocalizedString");

			foreach (string key in keys)
			{
				CodeMemberProperty property = new CodeMemberProperty
				{
					Name = key,
					Type = new CodeTypeReference(typeof(string)),
					Attributes = MemberAttributes.Public | MemberAttributes.Static
				};

				property.GetStatements.Add(new CodeMethodReturnStatement(
					new CodeMethodInvokeExpression(localizedStringsMethodReference, new CodePrimitiveExpression(key), new CodePrimitiveExpression(null))
					));
				classDeclaration.Members.Add(property);
			}
			
			codeUnit.WriteToFile(Constants.LOCALIZED_STRINGS_FILE_PATH, "This file was generated by Localization task for iOS");
			OutputCompileFilePath.Add(Constants.LOCALIZED_STRINGS_FILE_PATH);
		}

		private string ProcessValue(string value)
		{
			return value.Replace("\\", "\\\\")
				.Replace("\"", "\\\"")
				.Replace("\n", "\\n")
				;
		}
	}
}
