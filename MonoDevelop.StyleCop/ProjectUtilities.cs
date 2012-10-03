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
    /// Gets a dictionary which contains for each cached project a list with files to scan.
    /// </summary>
    internal Dictionary<Project, Dictionary<int, ProjectFile>> CachedFiles
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
    /// Determines whether the selected analysis type contains files to cache.
    /// </summary>
    /// <param name="analysisType">The analyze type being performed.</param>
    /// <returns>Returns true if there are cached files, or false otherwise.</returns>
    internal bool CacheKnownFiles(AnalysisType analysisType)
    {
      Param.Ignore(analysisType);

      this.CachedFiles = this.GetKnownFilesOfSelection(analysisType);
      if (this.CachedFiles.Count > 0)
      {
        return true;
      }

      return false;
    }

    /// <summary>
    /// Get the project associated with the file.
    /// </summary>
    /// <param name="fileName">Full path of the file.</param>
    /// <returns>Returns the project associated with the file or null if no project is associated.</returns>
    internal Project GetCachedProjectOfFile(string fileName)
    {
      if (this.CachedFiles != null && this.CachedFiles.Count > 0)
      {
        return this.CachedFiles.Select(kvp => kvp.Value.ContainsKey(fileName.GetHashCode()) ? kvp.Key : null).Where(value => value != null).FirstOrDefault();
      }

      return null;
    }

    /// <summary>
    /// Creates the list of StyleCop CodeProjects to be analyzed of the cached MonoDevelop files.
    /// </summary>
    /// <returns>Returns the list of StyleCop CodeProjects to be analyzed.</returns>
    internal IList<CodeProject> GetProjectList()
    {
      List<CodeProject> codeProjects = new List<CodeProject>();

      if (this.CachedFiles != null && this.CachedFiles.Count > 0)
      {
        foreach (var kvp in this.CachedFiles)
        {
          Project currentProject = kvp.Key;
          var projectFiles = kvp.Value;

          Configuration activeProjectConfiguration = new Configuration(null);
          if (projectFiles.Values.Count > 0)
          {
            CodeProject codeProject = new CodeProject(currentProject.BaseDirectory.GetHashCode(), currentProject.BaseDirectory, activeProjectConfiguration);

            foreach (var currentFile in projectFiles.Values)
            {
              this.Core.Environment.AddSourceCode(codeProject, currentFile.FilePath, null);
            }

            codeProjects.Add(codeProject);
          }
        }
      }

      return codeProjects;
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
    /// Enumerate through all childs of the given file and return all known files (including <paramref name="projectFile"/> if it's a known file type!).
    /// </summary>
    /// <param name="projectFile">Project file to enumerate.</param>
    /// <returns>A list with all known files.</returns>
    private Dictionary<int, ProjectFile> EnumerateFile(ProjectFile projectFile)
    {
      return this.EnumerateFile(projectFile, true);
    }

    /// <summary>
    /// Enumerate through all childs of the given file and return all known files (including <paramref name="projectFile"/> if it's a known file type!).
    /// </summary>
    /// <param name="projectFile">Project file to enumerate.</param>
    /// <param name="getParentFileFirst">Start enumeration from parent file.</param>
    /// <returns>A list with all known files.</returns>
    private Dictionary<int, ProjectFile> EnumerateFile(ProjectFile projectFile, bool getParentFileFirst)
    {
      Dictionary<int, ProjectFile> results = new Dictionary<int, ProjectFile>();

      if (projectFile != null)
      {
        ProjectFile fileToCheck = projectFile;
        if (getParentFileFirst)
        {
          fileToCheck = this.GetParentProjectFile(fileToCheck);
        }

        if (this.IsKnownFileExtension(fileToCheck.FilePath.Extension))
        {
          results.Add(fileToCheck.FilePath.ToString().GetHashCode(), fileToCheck);
        }

        if (fileToCheck.HasChildren)
        {
          foreach (var currentChild in fileToCheck.DependentChildren)
          {
            results = results.Concat(this.EnumerateFile(currentChild, false)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
          }
        }
      }

      return results;
    }

    /// <summary>
    /// Enumerate the given folder and return all known files.
    /// </summary>
    /// <param name="projectFolder">Project folder to enumerate.</param>
    /// <returns>A list with all known files.</returns>
    private Dictionary<int, ProjectFile> EnumerateFolder(ProjectFolder projectFolder)
    {
      Dictionary<int, ProjectFile> results = new Dictionary<int, ProjectFile>();

      if (projectFolder.Project != null)
      {
        results = projectFolder.Project.Files.GetFilesInPath(projectFolder.Path).Where(
          value => !IsDirectory(value.FilePath) && this.IsKnownFileExtension(value.FilePath.Extension)).ToDictionary(file => file.FilePath.ToString().GetHashCode());
      }

      return results;
    }

    /// <summary>
    /// Enumerate the give project and return all known files.
    /// </summary>
    /// <param name="project">Project to enumerate.</param>
    /// <returns>A list with all known files.</returns>
    private Dictionary<int, ProjectFile> EnumerateProject(Project project)
    {
      Dictionary<int, ProjectFile> results = new Dictionary<int, ProjectFile>();

      if (project != null)
      {
        results = project.Items.Select(item => item is ProjectFile ? item as ProjectFile : null).Where(
          value => value != null && !IsDirectory(value.FilePath) && this.IsKnownFileExtension(value.FilePath.Extension)).ToDictionary(file => file.FilePath.ToString().GetHashCode());
      }

      return results;
    }

    /// <summary>
    /// Get all known StyleCop files from the current selection.
    /// </summary>
    /// <param name="analysisType">The analyze type being performed.</param>
    /// <returns>Returns a dictionary which contains all known projects with a list of files or none.</returns>
    private Dictionary<Project, Dictionary<int, ProjectFile>> GetKnownFilesOfSelection(AnalysisType analysisType)
    {
      Dictionary<Project, Dictionary<int, ProjectFile>> resultDictionary = new Dictionary<Project, Dictionary<int, ProjectFile>>();

      if (analysisType == AnalysisType.Solution)
      {
        var allKnownProjectsInSolution = IdeApp.ProjectOperations.CurrentSelectedSolution.GetAllProjects().Select(
          project => this.IsKnownProjectType(project) ? project : null).Where(value => value != null).ToList();

        foreach (var currentProject in allKnownProjectsInSolution)
        {
          resultDictionary.Add(currentProject, this.EnumerateProject(currentProject));
        }
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
            resultDictionary.Add(projectFile.Project, enumeratedFiles);
          }
        }
      }
      else
      {
        if (this.ProjectPad.TreeView.MultipleNodesSelected())
        {
          var selectedNodes = this.ProjectPad.TreeView.GetSelectedNodes();

          // First get all projects of the selection and use the hash code as a key in dictionary.
          var knownProjectsInSelection = selectedNodes.Select(node => this.IsKnownProjectType(node.DataItem as Project) ? node.DataItem as Project : null).Where(
            value => value != null).ToList();

          // Go through all selected projects and add there files to a dictionary with there HashCode as key.
          // That will allow us to add each file just once even if it is selected seperatly.
          foreach (var currentProject in knownProjectsInSelection)
          {
            resultDictionary.Add(currentProject, new Dictionary<int, ProjectFile>());
            resultDictionary[currentProject] = resultDictionary[currentProject].Concat(this.EnumerateProject(currentProject)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
          }

          // Get all selected folders which have known file types and are not in the above project.
          var foldersWithKnownFileTypes = selectedNodes.Select(node => (node.DataItem as ProjectFolder) != null ? node.DataItem as ProjectFolder : null).Where(
            value => value != null && value.Project != null && !resultDictionary.ContainsKey(value.Project)
            && this.EnumerateFolder(value).Count > 0).ToList();

          foreach (var currentFolder in foldersWithKnownFileTypes)
          {
            if (!resultDictionary.ContainsKey(currentFolder.Project))
            {
              resultDictionary.Add(currentFolder.Project, new Dictionary<int, ProjectFile>());
            }

            resultDictionary[currentFolder.Project] = resultDictionary[currentFolder.Project].Concat(this.EnumerateFolder(currentFolder)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
          }

          // Next we get all known file types of the selection and add them if they are not already in the enumerated files list.
          var knownFilesInSelection = selectedNodes.Select(
            node => this.IsKnownFileExtension(node.DataItem as ProjectFile) ? node.DataItem as ProjectFile : null).Where(
            value => value != null && value.Project != null &&
            (!resultDictionary.ContainsKey(value.Project) || !resultDictionary[value.Project].ContainsKey(value.GetHashCode()))).ToList();

          // Add each file to the dictonary if it's not already in it, i.e. a selected child could be added through the EnumerateFile method.
          foreach (var currentFile in knownFilesInSelection)
          {
            if (!resultDictionary.ContainsKey(currentFile.Project))
            {
              resultDictionary.Add(currentFile.Project, new Dictionary<int, ProjectFile>());
            }

            if (!resultDictionary[currentFile.Project].ContainsKey(currentFile.GetHashCode()))
            {
              resultDictionary[currentFile.Project] = resultDictionary[currentFile.Project].Concat(this.EnumerateFile(currentFile)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
          }
        }
        else
        {
          Dictionary<int, ProjectFile> enumeratedFiles = new Dictionary<int, ProjectFile>();
          switch (analysisType)
          {
          case AnalysisType.File:
            enumeratedFiles = this.EnumerateFile(IdeApp.ProjectOperations.CurrentSelectedItem as ProjectFile);
            if (enumeratedFiles.Count > 0)
            {
              resultDictionary.Add(enumeratedFiles.Values.First().Project, enumeratedFiles);
            }

            break;

          case AnalysisType.Folder:
            enumeratedFiles = this.EnumerateFolder(IdeApp.ProjectOperations.CurrentSelectedItem as ProjectFolder);
            if (enumeratedFiles.Count > 0)
            {
              resultDictionary.Add(enumeratedFiles.Values.First().Project, enumeratedFiles);
            }

            break;

          case AnalysisType.Project:
            enumeratedFiles = this.EnumerateProject(IdeApp.ProjectOperations.CurrentSelectedProject);
            if (enumeratedFiles.Count > 0)
            {
              resultDictionary.Add(enumeratedFiles.Values.First().Project, enumeratedFiles);
            }

            break;
          }
        }
      }

      return resultDictionary;
    }

    /// <summary>
    /// Get the parent project file.
    /// </summary>
    /// <param name="projectFile">Project file to analyze.</param>
    /// <returns>Returns the parent project file, null if <paramref name="projectFile"/> is null.</returns>
    /// <remarks>The parent project file is the one which doesn't depend on another file.</remarks>
    private ProjectFile GetParentProjectFile(ProjectFile projectFile)
    {
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