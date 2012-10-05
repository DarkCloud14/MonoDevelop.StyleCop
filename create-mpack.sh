#!/bin/bash
MonoDevelopTool=/Applications/MonoDevelop.app/Contents/MacOS/mdtool
AddinBuildDirectory=./MonoDevelop.StyleCop/bin/Release
AddinFileName=MonoDevelop.StyleCop.dll
AddinFullFileName="$AddinBuildDirectory/$AddinFileName"

if [ -f "$MonoDevelopTool" ]; then
  $MonoDevelopTool build -t:Clean -c:Release
  $MonoDevelopTool build -c:Release

  if [ -f "$AddinFullFileName" ]; then
      $MonoDevelopTool setup pack "$AddinFullFileName"
    else
    echo "Couldn't find $AddinFileName in $AddinBuildDirectory"
  fi
else
    echo "Couldn't find the necessary MonoDevelop tool mdtool. Please make sure the path in this script is set correctly."
fi