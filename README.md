MonoDevelop.StyleCop
=============

MonoDevelop.StyleCop is an addin for MonoDevelop 8.3 and Visual Studio For Mac 8.3.

It integrates the source code analyzer [StyleCop](https://github.com/StyleCop/StyleCop/) into MonoDevelop/Visual Studio For Mac.

## DEPRECATION NOTICE
As I don't have much time anymore to work on the project and also [StyleCop](https://github.com/StyleCop/StyleCop/) is more or less obsolete as there is the Roslyn-based [StyleCopAnalyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)
I'll stop working on this project and also highly recommend to switch to [StyleCopAnalyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers) instead.

I tested it with VS for Mac + a .Net Core project and after adding the StyleCop.Analyzers NuGet package it worked out of the box, for old .Net projects I had to add the following manually to the project file after adding the StyleCop.Analyzers NuGet package:
```
<ItemGroup>
  <Analyzer Visible="False" Include="..\packages\StyleCop.Analyzers.1.1.118\analyzers\dotnet\cs\StyleCop.Analyzers.CodeFixes.dll" />
  <Analyzer Visible="False" Include="..\packages\StyleCop.Analyzers.1.1.118\analyzers\dotnet\cs\StyleCop.Analyzers.dll" />
</ItemGroup>
```
The version number must be corrected of course and maybe some other parts of the path so please check and correct the path to the packages.
Another thing you've to do is write a new configuration file as the old StyleCop configuraiton file won't work anymore, for more information about the configuration etc. of StyleCopAnalyzers goto the [projects GitHub website](https://github.com/DotNetAnalyzers/StyleCopAnalyzers) and look everything up you want to know.
### Issues are closed for MonoDevelop.StyleCop project

Installation
-----------

Get the latest addin package build or source code from GitHub [releases site](https://github.com/DarkCloud14/MonoDevelop.StyleCop/releases) and use MonoDevelops Addin-Manager to install.<br/>

You may have to enable the addin after installation.

### Remarks

The spell checker is only available on Windows!

Check the [StyleCop](https://github.com/StyleCop/StyleCop/) website for more informations about StyleCop or [StyleCop documentation](https://github.com/Visual-Stylecop/Visual-StyleCop/wiki#documentation-for-rules) for it's rules.

Usage
-----

After the installation you can find menu entries under Tools, by right clicking on a file, folder, project,<br/>
solution in Projectpad, by right clicking in an opened document or it's document tabulator.

By right clicking on a project you can also open the StyleCop settings editor to enable/disable StyleCop rules and so on.

The StyleCop entries only show up if the file, folder, project or solution are known by StyleCop!
