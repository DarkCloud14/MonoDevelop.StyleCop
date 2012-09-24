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
  using System.IO;
  using System.Linq;
  using System.Xml.Linq;
  using MonoDevelop.Ide;
  using MonoDevelop.Ide.Gui;
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

    /// <summary>
    /// Use MonoDevelops project pad to get detailed information about selected files and more.
    /// </summary>
    private readonly ProjectSolutionPad projectPad = null;

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

      Pad temporaryPad = IdeApp.Workbench.GetPad<ProjectSolutionPad>();
      if (temporaryPad != null)
      {
        this.projectPad = temporaryPad.Content as ProjectSolutionPad;
      }

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
    /// Gets the StyleCop core object.
    /// </summary>
    internal StyleCopCore Core
    {
      get { return this.core; }
    }

    /// <summary>
    /// Gets MonoDevelops project pad.
    /// </summary>
    internal ProjectSolutionPad ProjectPad
    {
      get { return this.projectPad; }
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
    /// Creates the list of project to be analyzed.
    /// </summary>
    /// <param name="analysisType">The analyze type being performed.</param>
    /// <returns>Returns the list of projects to be analyzed.</returns>
    internal IList<CodeProject> GetProjectList(AnalysisType analysisType)
    {
      Param.Ignore(analysisType);

      List<CodeProject> codeProjects = new List<CodeProject>();

      switch (analysisType)
      {
      case AnalysisType.ActiveDocument:
        var activeDocument = IdeApp.Workbench.ActiveDocument;
        if (activeDocument.HasProject)
        {
          var projectOfActiveDocument = activeDocument.Project;
          var projectConfiguration = projectOfActiveDocument.GetConfiguration(IdeApp.Workspace.ActiveConfiguration);
          var activeProjectConfiguration = new Configuration(new string[] { projectConfiguration.Name });

          CodeProject codeProject = new CodeProject(projectOfActiveDocument.BaseDirectory.GetHashCode(), projectOfActiveDocument.BaseDirectory, activeProjectConfiguration);
          this.Core.Environment.AddSourceCode(codeProject, activeDocument.FileName, null);

          codeProjects.Add(codeProject);
        }

        break;

      case AnalysisType.File:
      case AnalysisType.Folder:
        break;

      case AnalysisType.Project:
        break;

      case AnalysisType.Solution:
        break;
      }

      return codeProjects;
    }

    /// <summary>
    /// Determines whether the StyleCop menu items should be shown.
    /// </summary>
    /// <param name="analysisType">The analyze type being performed.</param>
    /// <returns>Returns true if the menu item should be shown, or false otherwise.</returns>
    internal bool SupportsStyleCop(AnalysisType analysisType)
    {
      Param.Ignore(analysisType);

      if (this.GetKnownProjectsAndFilesOfSelection(analysisType).Count > 0)
      {
        return true;
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
      return (File.GetAttributes(pathToCheck) & FileAttributes.Directory) == FileAttributes.Directory;
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
    /// Enumerate through all childs of the given file and return all known files (including <paramref name="projectFile"/> if it's a known file type!).
    /// </summary>
    /// <param name="projectFile">Project file to enumerate.</param>
    /// <returns>A list with all known files.</returns>
    private List<ProjectFile> EnumerateFile(ProjectFile projectFile)
    {
      List<ProjectFile> results = new List<ProjectFile>();

      if (projectFile != null)
      {
        if (this.IsKnownFileExtension(projectFile.FilePath.Extension))
        {
          results.Add(projectFile);
        }

        if (projectFile.HasChildren)
        {
          results.AddRange(projectFile.DependentChildren.Where(file => this.IsKnownFileExtension(file.FilePath.Extension)).ToList());
        }
      }

      return results;
    }

    /// <summary>
    /// Enumerate the given folder and return all known files.
    /// </summary>
    /// <param name="projectFolder">Project folder to enumerate.</param>
    /// <returns>A list with all known files.</returns>
    private List<ProjectFile> EnumerateFolder(ProjectFolder projectFolder)
    {
      List<ProjectFile> results = new List<ProjectFile>();

      if (projectFolder.Project != null)
      {
        results = projectFolder.Project.Files.GetFilesInPath(projectFolder.Path).Where(
          value => !IsDirectory(value.FilePath) && this.IsKnownFileExtension(value.FilePath.Extension)).ToList();
      }

      return results;
    }

    /// <summary>
    /// Enumerate the give project and return all known files.
    /// </summary>
    /// <param name="project">Project to enumerate.</param>
    /// <returns>A list with all known files.</returns>
    private List<ProjectFile> EnumerateProject(Project project)
    {
      List<ProjectFile> results = new List<ProjectFile>();

      if (project != null)
      {
        results = project.Items.Select(item => item is ProjectFile ? item as ProjectFile : null).Where(value => value != null && !IsDirectory(value.FilePath)).ToList();
      }

      return results;
    }

    /// <summary>
    /// Get all projects, folders and files of a known project/file type from the current selection.
    /// </summary>
    /// <param name="analysisType">The analyze type being performed.</param>
    /// <returns>Returns a list which contains all known projects and files or none.</returns>
    private List<object> GetKnownProjectsAndFilesOfSelection(AnalysisType analysisType)
    {
      Dictionary<int, Project> knownProjectsDictionary = new Dictionary<int, Project>();
      Dictionary<int, ProjectFile> knownFilesInFoldersDictionary = new Dictionary<int, ProjectFile>();
      List<object> resultList = new List<object>();
      var selectedNodes = this.ProjectPad.TreeView.GetSelectedNodes();

      if (analysisType == AnalysisType.Solution)
      {
        resultList = IdeApp.ProjectOperations.CurrentSelectedSolution.GetAllProjects().Select(
          project => this.IsKnownProjectType(project) ? (object)project : null).Where(value => value != null).ToList();
      }
      else if (analysisType == AnalysisType.ActiveDocument)
      {
        var activeDocument = IdeApp.Workbench.ActiveDocument;
        if (activeDocument.HasProject)
        {
          ProjectFile projectFile = activeDocument.Project.GetProjectFile(activeDocument.FileName);
          var enumeratedFiles = this.EnumerateFile(projectFile);
          if (enumeratedFiles.Count > 0)
          {
            resultList.AddRange(enumeratedFiles);
          }
        }
      }
      else
      {
        if (this.ProjectPad.TreeView.MultipleNodesSelected())
        {
          // First get all projects of the selection and use the hash code as a key in dictionary.
          var knownProjectsInSelection = selectedNodes.Select(node => this.IsKnownProjectType(node.DataItem as Project) ? node.DataItem as Project : null).Where(
            value => value != null);

          if (knownProjectsInSelection.Count() > 0)
          {
            knownProjectsDictionary = knownProjectsInSelection.ToDictionary(project => project.GetHashCode());
          }

          // Get all selected folders which have known file types and are not in the above project.
          var foldersWithKnownFileTypes = selectedNodes.Select(node => (node.DataItem as ProjectFolder) != null ? node.DataItem as ProjectFolder : null).Where(
            value => value != null && value.Project != null && !knownProjectsDictionary.ContainsKey(value.Project.GetHashCode())
            && this.EnumerateFolder(value).Count > 0).ToList();

          if (foldersWithKnownFileTypes.Count > 0)
          {
            foreach (var currentFolder in foldersWithKnownFileTypes)
            {
              var knownFilesInFolder = this.EnumerateFolder(currentFolder);
              foreach (var currentFile in knownFilesInFolder)
              {
                knownFilesInFoldersDictionary.Add(currentFile.GetHashCode(), currentFile);
              }
            }
          }

          // Next we get all known file types of the selection and add them if they don't have one of the above projects or folders as parent.
          var knownFilesInSelection = selectedNodes.Select(
            node => this.IsKnownFileExtension(node.DataItem as ProjectFile) ? node.DataItem as ProjectFile : null).Where(
            value => value != null && value.Project != null && !knownProjectsDictionary.ContainsKey(value.Project.GetHashCode())
            && !knownFilesInFoldersDictionary.ContainsKey(value.GetHashCode())).ToList();

          resultList.AddRange(knownProjectsDictionary.Values);

          // Add each file as a single project
          foreach (var currentFile in knownFilesInSelection)
          {
            resultList.Add(currentFile);
          }
        }
        else
        {
          List<ProjectFile> enumeratedFiles = new List<ProjectFile>();
          switch (analysisType)
          {
          case AnalysisType.File:
            enumeratedFiles = this.EnumerateFile(IdeApp.ProjectOperations.CurrentSelectedItem as ProjectFile);
            if (enumeratedFiles.Count > 0)
            {
              resultList.AddRange(enumeratedFiles);
            }

            break;

          case AnalysisType.Folder:
            enumeratedFiles = this.EnumerateFolder(IdeApp.ProjectOperations.CurrentSelectedItem as ProjectFolder);
            if (enumeratedFiles.Count > 0)
            {
              resultList.AddRange(enumeratedFiles);
            }

            break;

          case AnalysisType.Project:
            if (this.IsKnownProjectType(IdeApp.ProjectOperations.CurrentSelectedProject))
            {
              resultList.Add(IdeApp.ProjectOperations.CurrentSelectedProject);
            }

            break;
          }
        }
      }

      return resultList;
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
    /// <param name="projectFile">The <see cref="Document"/> class to analyze.</param>
    /// <returns>Returns true if its a known file extension, or false otherwise.</returns>
    private bool IsKnownFileExtension(ProjectFile projectFile)
    {
      if (projectFile != null && !string.IsNullOrEmpty(projectFile.FilePath))
      {
        return this.IsKnownFileExtension(projectFile.FilePath.Extension);
      }

      return false;
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
    /// Determines whether the project is a known project type.
    /// </summary>
    /// <param name="project">The project to analyze.</param>
    /// <returns>Returns true if its a known project type, or false otherwise.</returns>
    private bool IsKnownProjectType(Project project)
    {
      if (project != null)
      {
        if (this.availableParsers != null && this.availableParsers.Contains(this.GetProjectKindOfProjectType(project)))
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