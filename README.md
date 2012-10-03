MonoDevelop.StyleCop
=============

MonoDevelop.StyleCop is an addin for MonoDevelop 3.0.3 and above.

It integrates the source code analyzer [StyleCop](http://stylecop.codeplex.com/) into MonoDevelop.

Installation
-----------

Get the latest addin package from the [Downloads section](https://github.com/DarkCloud14/MonoDevelop.StyleCop/downloads) and use MonoDevelops Addin-Manager to install.

###Remarks

The following is necessary if you're not using MonoDevelop on Windows! The spell checker is only available on Windows too!
	
Before you install the latest addin package from the [Downloads section](https://github.com/DarkCloud14/MonoDevelop.StyleCop/downloads)
make sure that the following directories exists in your Mono installation.
	
	<Mono path>/etc/mono/registry
	<Mono path>/etc/mono/registry/LocalMachine
	
Create both directories if necessary.
Once you've done this it shouldn't be necessary anymore for new versions of the addin, only if you install a new version of Mono.

You can now proceed with the installation of the addin in MonoDevelop.

Usage
-----

After the installation you can find menu entries under Tools, by right clicking on a file, folder, project,
solution in Projectpad, by right clicking in an opened document or it's document tabulator.

By right clicking on a project you can also open the StyleCop settings editor to enable/disable StyleCop rules and so on.

The StyleCop entries only show up if the file, folder, project or solution are known by StyleCop!