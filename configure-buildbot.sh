#!/bin/bash
#
#   Copyright 2014 Alexander Jochum
#
#   Licensed under the Apache License, Version 2.0 (the "License");
#   you may not use this file except in compliance with the License.
#   You may obtain a copy of the License at
#
#       http://www.apache.org/licenses/LICENSE-2.0
#
#   Unless required by applicable law or agreed to in writing, software
#   distributed under the License is distributed on an "AS IS" BASIS,
#   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
#   See the License for the specific language governing permissions and
#   limitations under the License.

echo "Configuring..."
cd $(dirname $0)

MDAPPVERSION="4.0"
TARGETFRAMEWORKVERSION="v4.5"
PROJUTILSSTRINGTOLOOKUP="project.GetProjectTypes().Where(name => name.Equals(\"AspNetApp\", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault() != null"
PROJUTILSSTRINGTOINSERT="project.ProjectType.Equals(\"AspNetApp\", StringComparison.OrdinalIgnoreCase)"

if [ ! -z "$1" ]; then
  MDAPPVERSION=$1
fi

if [ -f version-addin ]; then
  VERSION=`cat version-addin`
else
  echo "Version file not found!"
  exit -1
fi

echo "Project is build for MonoDevelop version is $MDAPPVERSION"
echo "MonoDevelop.StyleCop version is $VERSION"

if [ "$MDAPPVERSION" == "4.0" ]; then
  TARGETFRAMEWORKVERSION="v4.0"
  echo "Patching files for MonoDevelop $MDAPPVERSION"
  STRINGTOLOOKUP="Pad errorsPad = IdeApp.Workbench.Pads.ErrorsPad;"
  STRINGTOINSERT="Pad errorsPad = IdeApp.Workbench.GetPad<MonoDevelop.Ide.Gui.Pads.ErrorListPad>();"
  sed -i.bak "s/$STRINGTOLOOKUP/$STRINGTOINSERT/g" ./MonoDevelop.StyleCop/ClassExtensions/ProjectOperationsExtensions.cs
  sed -i.bak "s/$PROJUTILSSTRINGTOLOOKUP/$PROJUTILSSTRINGTOINSERT/g" ./MonoDevelop.StyleCop/ProjectUtilities.cs
fi

if [ "$MDAPPVERSION" == "5.0" ]; then
  TARGETFRAMEWORKVERSION="v4.0"
  sed -i.bak "s/$PROJUTILSSTRINGTOLOOKUP/$PROJUTILSSTRINGTOINSERT/g" ./MonoDevelop.StyleCop/ProjectUtilities.cs
fi

echo "Creating files necessary to build the project."

sed "s/INSERT_CSPROJ_VERSION/$VERSION/g" ./MonoDevelop.StyleCop/MonoDevelop.StyleCop.addin.xml.orig > ./MonoDevelop.StyleCop/MonoDevelop.StyleCop.addin.xml
sed -i.bak "s/INSERT_MAJORAPP_VERSION/$MDAPPVERSION/g" ./MonoDevelop.StyleCop/MonoDevelop.StyleCop.addin.xml

sed -i.bak "s/INSERT_CSPROJ_VERSION/$VERSION/g" ./MonoDevelop.StyleCop/MonoDevelop.StyleCop-BuildBot.csproj
sed -i.bak "s/INSERT_TARGET_FRAMEWORKVERSION/$TARGETFRAMEWORKVERSION/g" ./MonoDevelop.StyleCop/MonoDevelop.StyleCop-BuildBot.csproj
sed -i.bak "s/INSERT_CSPROJ_MDROOT/./g" ./MonoDevelop.StyleCop/MonoDevelop.StyleCop-BuildBot.csproj

sed "s/INSERT_CSPROJ_VERSION/$VERSION/g" ./MonoDevelop.StyleCop/Properties/AssemblyInfo.cs.orig > ./MonoDevelop.StyleCop/Properties/AssemblyInfo.cs

sed "s/INSERT_CSPROJ_MDROOT/./g" ./MonoDevelop.StyleCop/gtk-gui/gui.stetic.orig > ./MonoDevelop.StyleCop/gtk-gui/gui.stetic

cp -f addin-project-$MDAPPVERSION.xml addin-project.xml

echo "File creation was successful."