# Storm.CrossLocalization [![nuget](https://img.shields.io/nuget/v/Storm.CrossLocalization.svg)](https://www.nuget.org/packages/Storm.CrossLocalization)

This library is available on [nuget](https://www.nuget.org/packages/Storm.CrossLocalization) 

```
Install-Package Storm.CrossLocalization
```

This will help you to share strings and translation for cross-platform applications (**Windows Phone**, **Xamarin.iOS**, **Xamarin.Android**) by transforming resx or resw file to platform specific files and generating some helpers class.

## Detailed blog posts
- [Part 1/2](http://blog.julienmialon.com/2017/01/part-one-storm-crosslocalization-how-to-share-your-strings-on-xamarin-applications/)
- [Part 2/2](http://blog.julienmialon.com/2017/01/part-two-storm-crosslocalization-how-to-share-your-strings-on-xamarin-applications/)

## How to ?
- Reference this package in your project (**PCL**, **WindowsPhone**, **Xamarin.iOS** or **Xamarin.Android**)
- Add your resx or resw file to the appropriate location (for easy sharing, I recommend putting the resx file at the root directory of the solution and add it as link in all projects)
 - **PCL** : anywhere you want
 - **Android** : Resources/values/ (or any specific values directory: value-fr, value-en...)
 - **iOS** : Resources/Base.lproj/ (or any region specific directory: fr.lproj, en.lproj...)
 - **Windows Phone** : Strings/ (or any region specific directory: Strings/fr, Strings/en...)
- For **PCL** and **Windows Phone** projects, remove Custom tool in file properties 
- For all resx/resw file, set its build action to *LocalizationFile* (if you don't have this in the list, unload/reload your project, if still not, look directly in the csproj to check if there is a line like `<Import Project="..\packages\Storm.CrossLocalization.0.0.9\build\wpa81\Storm.CrossLocalization.targets" />` at the end of the file. If not try uninstall/reinstall the nuget package)
- Build all projects, some files are supposed to be generated : 
 - **PCL** : three cs files on project root : EnumStrings.Localization.cs, LocalizationService.Localization.cs, LocalizedStrings.Localization.cs. Those are only helpers to be able to use strings in the PCL project. You can include them in your project but it's not mandatory.
 - **Android** : two cs files on project root : LocalizationService.Localization.cs, LocalizedStrings.Localization.cs, the first need you to have added files in PCL to work, the second is just a helper to easily access strings (you have to initialize it with ApplicationContext before using it). The task generated one strings.xml file for each directory where you have one of resx/resw, don't forget to include them in your project.
 - **iOS** : two cs files on project root : LocalizationService.Localization.cs, LocalizedStrings.Localization.cs, the first need you to have added files in PCL to work, the second is just a helper to easily access strings. The task generated one Localizable.strings file for each directory where you have one of resx/resw, don't forget to include them in your project.
 - **Windows Phone** : two cs files on project root : LocalizationService.Localization.cs, LocalizedStrings.Localization.cs, the first need you to have added files in PCL to work, the second is just a helper to easily access strings (you have to initialize it with a ResourceLoader you can obtain via ResourceLoader.GetForViewIndependentUse() before using it). The task generated one Resources.resw file for each directory where you have one of resx/resw, don't forget to include them in your project.
- add _\*.Localization.cs_, *strings.xml*, *Localizable.strings*, *Resources.resw* to your source control ignore files to avoid to commit generated files.
 
## Warnings

- If you have a Windows Phone project, don't name your resx/resw file Resources, it will conflict with Windows Phone Resources.resw generated file.
- All cs files will be generated in the first part of the RootNamespace of your project (example: if your project RootNamespace is MyApplication.Core, files will be generated in MyApplication namespace), this could cause issue if generated files are not in the same namespace accross your solution because LocalizationService.Localization.cs files need to be able to use PCL generated files without using. If you want everything to work without issue, juste have the first part of namespace in common accross your projects. For instance: 
 - MyApplication.Core => PCL project
 - MyApplication.Droid => Android project
 - MyApplication.iOS => iOS project
 - MyApplication.WindowsPhone => Windows Phone project 
 
## Advanced

- What if I want some strings to be different accross platforms ?
> Very easy, just suffix the key of the string with \_WP, \_Android or \_iOS this will generate the item only for the selected platform with the key without the suffix.

## Contributions

- You want to add support for another platform ? 
- You found an issue ?
- You have any idea to improve this project ?

Do not hesitate to contribute to it, opening issue or creating pull requests are very welcome !
