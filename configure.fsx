//-----------------------------------------------------------------------
// <copyright file="configure.fsx">
//   APL 2.0
// </copyright>
// <license>
//   Copyright 2014-2016 Alexander Jochum
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </license>
//-----------------------------------------------------------------------

// Configuration script to create the following files:
//
// MonoDevelop.StyleCop/MonoDevelop.StyleCop.csproj
// MonoDevelop.StyleCop/MonoDevelop.StyleCop.addin.xml
// MonoDevelop.StyleCop/Properties/AssemblyInfo.cs

open System
open System.Collections.Generic
open System.Linq
open System.Text
open System.IO
open System.Diagnostics
open System.Text.RegularExpressions

let mutable TargetFrameWorkVersion = "v4.5"
let MonoDevelopStyleCopVersion = File.ReadAllText("version-addin")
Console.WriteLine ("MonoDevelop.StyleCop version: {0}", MonoDevelopStyleCopVersion)

let UnixPaths = 
    [ "/usr/lib/monodevelop"
      "/usr/local/monodevelop/lib/monodevelop"
      "/usr/local/lib/monodevelop"
      "/Applications/MonoDevelop.app/Contents/MacOS/lib/"
      "monodevelop"
      "/opt/mono/lib/monodevelop"
      "/Applications/Xamarin Studio.app/Contents/MacOS/lib/monodevelop"
      "/Applications/Xamarin Studio.app/Contents/Resources/lib/monodevelop" ]

let WindowsPaths = 
    [ @"C:\Program Files\Xamarin Studio"
      @"C:\Program Files\MonoDevelop"
      @"C:\Program Files (x86)\Xamarin Studio"
      @"C:\Program Files (x86)\MonoDevelop" ]

let MdCheckFile = "bin/MonoDevelop.Core.dll"
let isWindows = (Path.DirectorySeparatorChar = '\\')

let GetPath (str: string list) =
    Path.GetFullPath (String.Join (Path.DirectorySeparatorChar.ToString (), str.Select(fun (s:string) -> s.Replace ('/', Path.DirectorySeparatorChar))))

let Grep (file, regex, group:string) =
    let m = Regex.Match (File.ReadAllText (GetPath [file]), regex)
    m.Groups.[group].Value

let FileReplace (file, outFile, toReplace:string, replacement:string) =
    File.WriteAllText (GetPath [outFile], File.ReadAllText(GetPath [file]).Replace(toReplace, replacement))

let Run (file, args) =
    let currentProcess = new Process ()
    currentProcess.StartInfo.FileName <- file
    currentProcess.StartInfo.Arguments <- args
    currentProcess.StartInfo.RedirectStandardOutput <- true
    currentProcess.StartInfo.UseShellExecute <- false
    currentProcess.StartInfo.WindowStyle <- ProcessWindowStyle.Hidden
    currentProcess.Start () |> ignore
    currentProcess.StandardOutput

let paths = if isWindows then WindowsPaths else UnixPaths

Console.WriteLine "MonoDevelop.StyleCop add-in configuration script"
Console.WriteLine "------------------------------------------"

let args = fsi.CommandLineArgs.[1..]
if Array.exists ((=) "--help") args then
  Console.WriteLine "Options:\n"
  Console.WriteLine "--prefix=PATH\n"
  Console.WriteLine "  MonoDevelop library directory. Currently searched:\n"
  for p in paths do Console.WriteLine("  {0}", p)
  exit 0

let searchPaths =
  let getPrefix (s: string) =
    let xs = s.Split('=')
    if xs.Length = 2 && xs.[0] = "--prefix" then Some xs.[1]
    else None
  match Array.tryPick getPrefix args with
  | None -> paths
  | Some p -> p :: paths

let mutable mdDir = null
let mutable mdVersion = "5.0"

// Look for the installation directory
if (File.Exists (GetPath ["../../../monodevelop.pc.in"])) then
    // Local MonoDevelop build directory
    mdDir <- GetPath [Environment.CurrentDirectory + "/../../../build"]
    if (File.Exists (GetPath [mdDir;  "../../main/configure.in"])) then 
        mdVersion <- Grep (GetPath [mdDir; "../../main/configure.in"], @"AC_INIT.*?(?<ver>([0-9]|\.)+)", "ver")
else
    // Using installed MonoDevelop
    mdDir <- searchPaths.FirstOrDefault (fun p -> File.Exists (GetPath [p; MdCheckFile]))
    if (mdDir <> null) then
        let mdExe = 
            if (File.Exists (GetPath[mdDir; "../../XamarinStudio"])) then
                GetPath[mdDir; "../../XamarinStudio"]
            elif (File.Exists (GetPath [mdDir; "../../MonoDevelop"])) then
                GetPath [mdDir; "../../MonoDevelop"]
            elif (File.Exists (GetPath[mdDir; "bin/XamarinStudio.exe"])) then
                GetPath[mdDir; "bin/XamarinStudio.exe"]
            elif (File.Exists (GetPath [mdDir; "bin/MonoDevelop.exe"])) then
                GetPath [mdDir; "bin/MonoDevelop.exe"]
            else
                null
        if (mdExe <> null) then
            let outp = Run(mdExe, "/?").ReadLine()
            mdVersion <- outp.Split([| ' ' |], StringSplitOptions.RemoveEmptyEntries).Last()

if not isWindows then
    // Update the makefile. We don't use that on windows
    FileReplace ("Makefile.orig", "Makefile", "INSERT_MDROOT", mdDir)
    FileReplace ("Makefile", "Makefile", "INSERT_MDVERSION4", mdVersion)
    FileReplace ("Makefile", "Makefile", "INSERT_VERSION", MonoDevelopStyleCopVersion)
    
if (mdDir = null) then
    Console.WriteLine ("MonoDevelop binaries not found. Continuing anyway")
else
    Console.WriteLine ("MonoDevelop binaries found at: {0}", mdDir)

Console.WriteLine ("Detected version: {0}", mdVersion)

let currentMDVersion = Version(mdVersion)
let mutable majorAppVersion = String.Format("{0}.{1}", currentMDVersion.Major, currentMDVersion.Minor)
let MinMDVersionForFramework45 = Version(5, 7)

if currentMDVersion < MinMDVersionForFramework45 then
    majorAppVersion <- mdVersion.Split('.').First() + ".0";
    TargetFrameWorkVersion <- "v4.0"

Console.WriteLine ("Major app version: {0}", majorAppVersion)

let addinProjectFile = "addin-project.xml"
let assemblyFile = "MonoDevelop.StyleCop/Properties/AssemblyInfo.cs"
let csprojFile = "MonoDevelop.StyleCop/MonoDevelop.StyleCop.csproj"
let guiSteticFile = "MonoDevelop.StyleCop/gtk-gui/gui.stetic"
let xmlFile = "MonoDevelop.StyleCop/MonoDevelop.StyleCop.addin.xml"

FileReplace ("addin-project.xml.orig", addinProjectFile, "INSERT_MAJORAPP_VERSION", majorAppVersion)
FileReplace ("MonoDevelop.StyleCop/Properties/AssemblyInfo.cs.orig", assemblyFile, "INSERT_CSPROJ_VERSION", MonoDevelopStyleCopVersion)
FileReplace ("MonoDevelop.StyleCop/MonoDevelop.StyleCop.csproj.orig", csprojFile, "INSERT_CSPROJ_MDROOT", mdDir)
FileReplace (csprojFile, csprojFile, "INSERT_CSPROJ_VERSION", MonoDevelopStyleCopVersion)
FileReplace (csprojFile, csprojFile, "INSERT_TARGET_FRAMEWORKVERSION", TargetFrameWorkVersion)
FileReplace ("MonoDevelop.StyleCop/gtk-gui/gui.stetic.orig", guiSteticFile, "INSERT_CSPROJ_MDROOT", mdDir)
FileReplace ("MonoDevelop.StyleCop/MonoDevelop.StyleCop.addin.xml.orig", xmlFile, "INSERT_CSPROJ_VERSION", MonoDevelopStyleCopVersion)
FileReplace (xmlFile, xmlFile, "INSERT_MAJORAPP_VERSION", majorAppVersion)