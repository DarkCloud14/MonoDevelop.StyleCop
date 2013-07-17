//-----------------------------------------------------------------------
// <copyright file="ProjectUtilities.cs">
//   APL 2.0
// </copyright>
// <license>
//   Copyright 2013 Alexander Jochum
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
  using System.IO;
  using System.Linq;
  using System.Xml.Linq;
  using MonoDevelop.Ide;
  using MonoDevelop.Ide.Gui;
  using MonoDevelop.Ide.Gui.Pads;
  using MonoDevelop.Ide.Gui.Pads.ProjectPad;
  using MonoDevelop.Projects;
  using global::StyleCop;

  /// <summary>
  /// Utility class for project related stuff.
  /// </summary>
  internal sealed class ProjectUtilities : IDisposable
  {
    #region Private Readonly Fields

    /// <summary>
    /// The collection of known StyleCop source code parsers.
    /// </summary>
    private readonly HashSet<string> availableParsers = new HashSet<string>();

    #endregion Private Readonly Fields

    #region Private Fields

    /// <summary>
    /// The StyleCop core object.
    /// </summary>
    private StyleCopCore core = new StyleCopCore();

    #endregion Private Fields

    #region Constructors and Destructors

    /// <summary>
    /// Initializes static members of the <see cref="ProjectUtilities"/> class.
    /// </summary>
    static ProjectUtilities()
    {
      Instance = new ProjectUtilities();
    }

    /// <summary>
    /// Prevents a default instance of the <see cref="ProjectUtilities"/> class from being created.
    /// Initializes members of the <see cref="ProjectUtilities"/> class.
    /// </summary>
    private ProjectUtilities()
    {
      this.core.Initialize(null, true);
      this.RetrieveAvailableStyleCopParsers();

      // Register StyleCop events.
      this.core.OutputGenerated += ProjectOperationsExtensions.StyleCopCoreOutputGenerated;
      this.core.ViolationEncountered += ProjectOperationsExtensions.StyleCopCoreViolationEncountered;

      this.CachedProjects = new ProjectCache();

      Debug.Assert(this.projectPad != null, "ProjectPad not initialized.");
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="ProjectUtilities"/> class.
    /// </summary>
    ~ProjectUtilities()
    {
      this.Dispose(true);
    }

    #endregion Constructors and Destructors

    #region Internal Static Properties

    /// <summary>
    /// Gets an Instance of ProjectUtilities to call it's functions.
    /// </summary>
    public static ProjectUtilities Instance
    {
      get;
      private set;
    }

    #endregion Internal Static Properties

    #region Internal Properties

    /// <summary>
    /// Gets all cached Projects
    /// </summary>
    internal ProjectCache CachedProjects
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the StyleCop core object.
    /// </summary>
    internal StyleCopCore Core
    {
      get { return this.core; }
    }

    #endregion Internal Properties

    #region IDisposable Methods

    /// <summary>
    /// Dispose the object.
    /// </summary>
    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    #endregion IDisposable Methods

    #region Internal Methods

    /// <summary>
    /// Create a list with StyleCop code project based on the MonoDevelop projects and files in <paramref name="fromMonoDevelopProjects"/>.
    /// </summary>
    /// <param name="fromMonoDevelopProjects">MonoDevelop projects with files.</param>
    /// <returns>A list containing all possible StyleCop code projects.</returns>
    internal List<CodeProject> CreateStyleCopCodeProjects(Dictionary<Project, HashSet<ProjectFile>> fromMonoDevelopProjects)
    {
      List<CodeProject> resultList = new List<CodeProject>();

      if (fromMonoDevelopProjects != null)
      {
        foreach (var projectKvP in fromMonoDevelopProjects)
        {
          // If the value is null get the code project from our cached projects.
          if (projectKvP.Value == null)
          {
            resultList.Add(this.CachedProjects.CreateStyleCopCodeProject(projectKvP.Key));
          }
          else
          {
            CodeProject newStyleCopProject = new CodeProject(projectKvP.Key.BaseDirectory.GetHashCode(), projectKvP.Key.BaseDirectory, new Configuration(null));
            foreach (var file in projectKvP.Value)
            {
              ProjectUtilities.Instance.Core.Environment.AddSourceCode(newStyleCopProject, file.FilePath, null);
            }

            resultList.Add(newStyleCopProject);
          }
        }
      }

      return resultList;
    }

    /// <summary>
    /// Enumerate through all childs of the given file and return all known files (including <paramref name="projectFile"/> if it's a known file type!).
    /// </summary>
    /// <param name="projectFile">Project file to enumerate.</param>
    /// <returns>A list with all known files.</returns>
    internal List<ProjectFile> EnumerateFile(ProjectFile projectFile)
    {
      List<ProjectFile> results = new List<ProjectFile>();

      if (projectFile != null)
      {
        ProjectFile fileToEnumerate = this.GetParentProjectFile(projectFile);

        if (this.HasKnownFileExtension(fileToEnumerate))
        {
          results.Add(fileToEnumerate);

          if (fileToEnumerate.HasChildren)
          {
            foreach (var currentChild in fileToEnumerate.DependentChildren)
            {
              if (this.HasKnownFileExtension(currentChild))
              {
                results.Add(currentChild);
              }
            }
          }
        }
      }

      return results;
    }
    
    /// <summary>
    /// Enumerate the given folder and return all files known by StyleCop.
    /// </summary>
    /// <param name="projectFolder">Project folder to enumerate.</param>
    /// <returns>A list with all known files.</returns>
    internal List<ProjectFile> EnumerateFolder(ProjectFolder projectFolder)
    {
      List<ProjectFile> results = new List<ProjectFile>();

      if (projectFolder.Project != null)
      {
        results = projectFolder.Project.Files.GetFilesInPath(projectFolder.Path).Where(value => this.HasKnownFileExtension(value)).ToList();
      }

      return results;
    }

    /// <summary>
    /// Determines whether the file has a known file extension.
    /// </summary>
    /// <param name="projectFile">The <see cref="Document"/> class to analyze.</param>
    /// <returns>Returns true if its a known file extension, or false otherwise.</returns>
    internal bool HasKnownFileExtension(ProjectFile projectFile)
    {
      Param.AssertNotNull(projectFile, "projectFile");

      if (projectFile != null && !string.IsNullOrEmpty(projectFile.FilePath))
      {
        return this.IsKnownFileExtension(projectFile.FilePath.Extension);
      }

      return false;
    }

    /// <summary>
    /// Determines whether the project is a known project type.
    /// </summary>
    /// <param name="project">The project to analyze.</param>
    /// <returns>Returns true if its a known project type, or false otherwise.</returns>
    internal bool IsKnownProjectType(Project project)
    {
      Param.AssertNotNull(project, "project");

      if (project != null)
      {
        if (this.availableParsers != null && this.availableParsers.Contains(this.GetProjectKindOfProjectType(project)))
        {
          return true;
        }
      }

      return false;
    }

    #endregion Internal Methods

    #region Private Static Methods

    /// <summary>
    /// Analyzes <paramref name="pathToCheck"/> and checks if it is just a directory.
    /// </summary>
    /// <param name="pathToCheck">Path that should be checked.</param>
    /// <returns>Returns true if the given path is a directory, false otherwise.</returns>
    private static bool IsDirectory(string pathToCheck)
    {
      return Directory.Exists(pathToCheck);
    }

    #endregion Private Static Methods

    #region Private Methods

    /// <summary>
    /// Disposes the contents of the class.
    /// </summary>
    /// <param name="disposing">Indicates whether to dispose unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
      if (disposing && this.core != null)
      {
        // Unregister StyleCop events again.
        this.core.ViolationEncountered -= ProjectOperationsExtensions.StyleCopCoreViolationEncountered;
        this.core.OutputGenerated -= ProjectOperationsExtensions.StyleCopCoreOutputGenerated;
        this.core = null;
      }
    }

    /// <summary>
    /// Get the parent project file.
    /// </summary>
    /// <param name="projectFile">Project file to analyze.</param>
    /// <returns>Returns the parent project file, null if <paramref name="projectFile"/> is null.</returns>
    /// <remarks>The parent project file is the one which doesn't depend on another file.</remarks>
    private ProjectFile GetParentProjectFile(ProjectFile projectFile)
    {
      Param.AssertNotNull(projectFile, "projectFile");

      if (projectFile != null)
      {
        if (!string.IsNullOrEmpty(projectFile.DependsOn))
        {
          return this.GetParentProjectFile(projectFile.DependsOnFile);
        }
        else
        {
          return projectFile;
        }
      }

      return null;
    }

    /// <summary>
    /// Get the official project GUID/Type of MonoDevelops project kind.
    /// </summary>
    /// <param name="project">The MonoDevelop project to retrieve the project GUID/Type for.</param>
    /// <returns>Returns the official project GUID/Type.</returns>
    private string GetProjectKindOfProjectType(Project project)
    {
      Param.AssertNotNull(project, "projectType");
      DotNetAssemblyProject assemblyProject = project as DotNetAssemblyProject;

      if (assemblyProject != null)
      {
        if (project.ProjectType.Equals("AspNetApp", StringComparison.OrdinalIgnoreCase) && assemblyProject.LanguageName.Equals("C#", StringComparison.OrdinalIgnoreCase))
        {
          return "{E24C65DC-7377-472b-9ABA-BC803B73C61A}";
        }

        if (project.ProjectType.Equals("DotNet", StringComparison.OrdinalIgnoreCase) && assemblyProject.LanguageName.Equals("C#", StringComparison.OrdinalIgnoreCase))
        {
          return "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
        }
      }

      return "Unknown";
    }

    /// <summary>
    /// Determines whether the file extension is a known file extension.
    /// </summary>
    /// <param name="fileExtension">The file extension to analyze.</param>
    /// <returns>Returns true if its a known file extension, or false otherwise.</returns>
    private bool IsKnownFileExtension(string fileExtension)
    {
      Param.AssertNotNull(fileExtension, "fileExtension");

      if (fileExtension.Length > 0)
      {
        // Check if there is a dot in the extension and remove it
        if (fileExtension.StartsWith("."))
        {
          fileExtension = fileExtension.Remove(0, 1);
        }

        var knownExtension = this.core.Parsers.FirstOrDefault(parser => !string.IsNullOrEmpty(parser.FileTypes.FirstOrDefault(fileType => fileType.Equals(fileExtension, StringComparison.OrdinalIgnoreCase))));
        if (knownExtension != null)
        {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Retrieve all available StyleCop source code parsers using the parsers configuration xml.
    /// </summary>
    /// <remarks>The necessary format of parsers configuration xml can be found in StyleCop source code.</remarks>
    private void RetrieveAvailableStyleCopParsers()
    {
      Debug.Assert(this.core != null, "this.core has not been initialized");
      Debug.Assert(this.core.Parsers != null, "this.core source parsers has not been initialized.");

      foreach (SourceParser parser in this.core.Parsers)
      {
        XDocument translationDocument = StyleCopCore.LoadAddInResourceXml(parser.GetType(), null).ToXDocument();
        foreach (var node in translationDocument.Elements("SourceParser").Elements("VsProjectTypes").Elements("VsProjectType"))
        {
          string projectKind = Convert.ToString(node.Attribute("ProjectKind").Value);
          if (!string.IsNullOrEmpty(projectKind))
          {
            projectKind = projectKind.Trim();
            if (!this.availableParsers.Add(projectKind))
            {
              // Allow this to succeed at runtime.
              Debug.Fail("A previously loaded parser already handles the project kind: " + projectKind);
            }
          }
        }
      }
    }

    #endregion Private Methods
  }
}