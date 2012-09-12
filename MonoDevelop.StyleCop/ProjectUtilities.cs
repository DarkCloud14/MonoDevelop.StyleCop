//-----------------------------------------------------------------------
// <copyright file="ProjectUtilities.cs">
//   APL 2.0
// </copyright>
// <license>
//   Copyright 2012 Alexander Jochum
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
namespace MonoDevelop.StyleCop
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Xml.Linq;
  using MonoDevelop.Projects;
  using global::StyleCop;

  /// <summary>
  /// Utility class for project related stuff.
  /// </summary>
  internal static class ProjectUtilities
  {
    #region Private Static Fields

    /// <summary>
    /// The collection of known StyleCop source code parsers.
    /// </summary>
    private static readonly HashSet<string> AvailableParsers = new HashSet<string>();

    /// <summary>
    /// The StyleCop core object.
    /// </summary>
    private static readonly StyleCopCore Core = new StyleCopCore();

    #endregion Private Static Fields

    #region Constructor

    /// <summary>
    /// Initializes static members of the <see cref="ProjectUtilities"/> class.
    /// </summary>
    static ProjectUtilities()
    {
      Core.Initialize(null, true);
      RetrieveAvailableStyleCopParsers();
    }

    #endregion Constructor

    #region Internal Static Methods

    /// <summary>
    /// Function which gets called at MonoDevelop startup so the static constructor gets called.
    /// </summary>
    internal static void Initialize()
    {
    }

    /// <summary>
    /// Determines whether the project is a known project type.
    /// </summary>
    /// <param name="project">The project to analyse.</param>
    /// <returns>Returns true if its a known project type, or false otherwise.</returns>
    internal static bool IsKnownProjectType(Project project)
    {
      Param.AssertNotNull(project, "project");

      if (AvailableParsers != null && AvailableParsers.Contains(GetProjectKindOfProjectType(project)))
      {
        return true;
      }

      return false;
    }

    #endregion Internal Static Methods

    #region Private Static Methods

    /// <summary>
    /// Get the official project GUID/Type of MonoDevelops project kind.
    /// </summary>
    /// <param name="project">The MonoDevelop project to retrieve the project GUID/Type for.</param>
    /// <returns>Returns the official project GUID/Type.</returns>
    private static string GetProjectKindOfProjectType(Project project)
    {
      Param.AssertNotNull(project, "projectType");
      DotNetAssemblyProject assemblyProject = project as DotNetAssemblyProject;
      Debug.Assert(assemblyProject != null, "assemblyProject is not typeof DotNetAssemblyProject");

      if (project.ProjectType.Equals("AspNetApp", StringComparison.OrdinalIgnoreCase) && assemblyProject.LanguageName.Equals("C#", StringComparison.OrdinalIgnoreCase))
      {
        return "{E24C65DC-7377-472b-9ABA-BC803B73C61A}";
      }

      if (project.ProjectType.Equals("DotNet", StringComparison.OrdinalIgnoreCase) && assemblyProject.LanguageName.Equals("C#", StringComparison.OrdinalIgnoreCase))
      {
        return "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
      }

      return "Unknown";
    }

    /// <summary>
    /// Retrieve all available StyleCop source code parsers using the parsers configuration xml.
    /// </summary>
    /// <remarks>The necessary format of parsers configuration xml can be found in StyleCop source code.</remarks>
    private static void RetrieveAvailableStyleCopParsers()
    {
      Debug.Assert(Core != null, "Core has not been initialized");
      Debug.Assert(Core.Parsers != null, "Core source parsers has not been initialized.");

      foreach (SourceParser parser in Core.Parsers)
      {
        XDocument translationDocument = StyleCopCore.LoadAddInResourceXml(parser.GetType(), null).ToXDocument();
        foreach (var node in translationDocument.Elements("SourceParser").Elements("VsProjectTypes").Elements("VsProjectType"))
        {
          string projectKind = Convert.ToString(node.Attribute("ProjectKind").Value);
          if (!string.IsNullOrEmpty(projectKind))
          {
            projectKind = projectKind.Trim();
            if (!AvailableParsers.Add(projectKind))
            {
              // Allow this to succeed at runtime.
              Debug.Fail("A previously loaded parser already handles the project kind: " + projectKind);
            }
          }
        }
      }
    }

    #endregion Private Static Methods
  }
}