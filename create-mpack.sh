#!/bin/bash
MonoDevelopTool=/Applications/MonoDevelop.app/Contents/MacOS/mdtool
StyleCopFilesDirectory=./StyleCop.Files
AddinBuildDirectory=./MonoDevelop.StyleCop/bin/Release
AddinFileName=MonoDevelop.StyleCop.dll
AddinFullFileName="$AddinBuildDirectory/$AddinFileName"

if [ -f "$AddinFullFileName" ]; then
	if [ -d "$StyleCopFilesDirectory" ]; then
		if [ -f "$MonoDevelopTool" ]; then
			if [ -f "$StyleCopFilesDirectory/$AddinFileName" ]; then
				rm "$StyleCopFilesDirectory/$AddinFileName"	
			fi
			
			cp -f LICENSE "$StyleCopFilesDirectory"
			cp -f LICENSE_Ms-PL.txt "$StyleCopFilesDirectory"
			cp -f NOTICE "$StyleCopFilesDirectory"
	    	cp -f "$AddinFullFileName" "$StyleCopFilesDirectory"
	    	$MonoDevelopTool setup pack "$StyleCopFilesDirectory/$AddinFileName"
	    else
	    	echo "Couldn't find the necessary MonoDevelop tool mdtool. Please make sure the path in this script is set correctly."
	    fi
	else
		echo "Make sure the StyleCop files are in the directory $StyleCopFilesDirectory/ !"
	fi
else
	echo "Couldn't find $AddinFileName in $AddinBuildDirectory"
fi