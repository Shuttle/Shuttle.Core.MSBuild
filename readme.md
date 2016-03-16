Shuttle.Core.MSBuild
====================

Does not add any references but rather adds the assemblies to the `Tools` NuGet package folder.

This assembly contains the following MSBuild tasks that are used by [Shuttle](https://github.com/Shuttle) components:

### Prompt

Prompts the user for input that is saved in the given output parameter.

| Parameter | Required | Description |
| --- | --- | --- |
| Text | yes | The text to display on the console. |
| UserInput | output | The value entered on the console. |

``` xml
<Prompt Text="Enter semantic version:" Condition="$(SemanticVersion) == ''">
	<Output TaskParameter="UserInput" PropertyName="SemanticVersion" />
</Prompt>
```

### RegexFindAndReplace

Performs a regular expression find/replace on the given files.

| Parameter | Required | Description |
| --- | --- | --- |
| Files | yes | The files that the find/replace operation should be performed on. |
| FindExpression | yes | The Regex that should be used to find the text to be replaced. |
| ReplacementText | no | The text to replace the located expression with. |
| IgnoreCase | no | Defaults to false. |
| Multiline | no | Defaults to false. |
| Singleline | no | Defaults to false. |

``` xml
<RegexFindAndReplace Files="@(Files)" FindExpression="regex" ReplacementText="new-text" />
```

### SetNugetPackageVersions

Retrieves the package names and version from the given package folder and replaces all tags with the relevant version number. A tag has to be in the format `{OpenTag}{PackageName}-version{ClosingTag}`.

| Parameter | Required | Description |
| --- | --- | --- |
| Files | yes | The files that contain package version tags. |
| PackageFolder | yes | The folder that contains all the packages. |
| OpenTag | no | Defaults to `{`. |
| CloseTag | no | Defaults to `}`. |

``` xml
<SetNugetPackageVersions Files="@(Files)" PackageFolder="nuget-package-folder" />
```

### Zip

Creates an archive that contains the given files.

| Parameter | Required | Description |
| --- | --- | --- |
| Files | yes | The files that should be added to the zip archive. |
| RelativeFolder | yes | The 'base' folder that the zip entries should be created from.  e.g. if there is a file `c:\folder\another\file.txt` and the `RelativeFolder` is `c:\folder\` then the entry in the zip archive will be `another\file.txt`.	 |
| ZipFilePath | yes | The path to the zip archive that will be created.  Any existing file will be overwritten. |

``` xml
<Zip Files="@(Files)" RelativeFolder="$(OutputPath)" ZipFilePath="$(OutputPath).zip" />
```
