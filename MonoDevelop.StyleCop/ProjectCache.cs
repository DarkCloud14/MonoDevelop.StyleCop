//-----------------------------------------------------------------------
// <copyright file="ProjectCache.cs">
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
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using MonoDevelop.Ide.Gui.Pads.ProjectPad;
  using MonoDevelop.Projects;
  using global::StyleCop;

  /// <summary>
  /// This class is used for project caching.
  /// Once a project got completely cached this should speed everything up a bit.
  /// </summary>
  internal class ProjectCache
  {
    #region Private Fields

    /// <summary>
    /// Dictionary with all known projects and project files added to the cache.
    /// </summary>
    private Dictionary<Project, HashSet<ProjectFile>> projectCache = new Dictionary<Project, HashSet<ProjectFile>>();

    /// <summary>
    /// HashSet which holds every project that got completely cached.
    /// </summary>
    private HashSet<Project> completelyCachedProjects = new HashSet<Project>();

    #endregion Private Fields

    #region Public Methods

    /// <summary>
    /// Adds a project file to the cache if it has a known file extension
    /// and exists in the project specified during object creation.
    /// </summary>
    /// <param name="fileToAdd">File to add to cache</param>
    public void AddFile(ProjectFile fileToAdd)
    {
      Param.AssertNotNull(fileToAdd, "fileToAdd");

      if (fileToAdd != null && fileToAdd.Project != null && this.AddProject(fileToAdd.Project))
      {
        if (!this.projectCache[fileToAdd.Project].Contains(fileToAdd) && ProjectUtilities.Instance.HasKnownFileExtension(fileToAdd))
        {
          List<ProjectFile> enumeratedFiles = ProjectUtilities.Instance.EnumerateFile(fileToAdd);

          foreach (var file in enumeratedFiles)
          {
            if (!this.projectCache[fileToAdd.Project].Contains(file))
            {
              this.projectCache[fileToAdd.Project].Add(file);
            }
          }
        }
      }
    }

    /// <summary>
    /// Enumerate the given folder and add all compatible files to the cache.
    /// </summary>
    /// <param name="projectFolder">Project folder to enumerate.</param>
    public void AddFolder(ProjectFolder projectFolder)
    {
      if (projectFolder.Project != null && this.AddProject(projectFolder.Project))
      {
        ProjectUtilities.Instance.EnumerateFolder(projectFolder).ForEach(file => this.AddFile(file));
      }
    }

    /// <summary>
    /// Add a project to cache if it has a known project type.
    /// </summary>
    /// <param name="projectToAdd">Project that should be cached.</param>
    /// <returns>True if the project was successfully added to the cache, false otherwise.</returns>
    public bool AddProject(Project projectToAdd)
    {
      Param.AssertNotNull(projectToAdd, "projectToAdd");

      if (projectToAdd != null && ProjectUtilities.Instance.IsKnownProjectType(projectToAdd))
      {
        if (!this.projectCache.ContainsKey(projectToAdd))
        {
          this.projectCache.Add(projectToAdd, new HashSet<ProjectFile>());
        }

        return true;
      }

      return false;
    }

    /// <summary>
    /// A call to this function will go through all project files and cache all compatible files.
    /// </summary>
    /// <param name="projectToCache">Project that should get completely cached.</param>
    public void CacheWholeProject(Project projectToCache)
    {
      Param.AssertNotNull(projectToCache, "projectToCache");

      // Only do the whole project caching if not already done.
      if (projectToCache != null && !this.IsWholeProjectCached(projectToCache) && this.AddProject(projectToCache))
      {
        foreach (var file in projectToCache.Files)
        {
          this.AddFile(file);
        }

        this.completelyCachedProjects.Add(projectToCache);
      }
    }

    /// <summary>
    /// Get all cached files of the <paramref name="associatedProject"/>.
    /// </summary>
    /// <param name="associatedProject">Project to get the cached files of.</param>
    /// <returns>HashSet with all cached files, null if project wasn't cached yet.</returns>
    public HashSet<ProjectFile> GetCachedProjectFiles(Project associatedProject)
    {
      Param.AssertNotNull(associatedProject, "associatedProject");

      if (associatedProject != null && this.projectCache.ContainsKey(associatedProject))
      {
        return this.projectCache[associatedProject];
      }

      return null;
    }

    /// <summary>
    /// Get MonoDevelop project based on <paramref name="fileName"/>
    /// </summary>
    /// <param name="fileName">Filename to use during project lookup.</param>
    /// <returns>MonoDevelop project which contains the file <paramref name="fileName"/>, null otherwise.</returns>
    public Project GetProjectForFile(string fileName)
    {
      foreach (var project in this.projectCache.Keys)
      {
        if (project.GetProjectFile(fileName) != null)
        {
          return project;
        }
      }

      return null;
    }

    /// <summary>
    /// Create a StyleCop compatible code project from MonoDevelop project.
    /// </summary>
    /// <param name="projectToUse">MonoDevelop project to use for StyleCop code project creation.</param>
    /// <returns>A StyleCop code project or null if something failed or the MonoDevelop project type is incompatible.</returns>
    public CodeProject CreateStyleCopCodeProject(Project projectToUse)
    {
      Param.AssertNotNull(projectToUse, "projectToUse");

      if (projectToUse != null)
      {
        this.CacheWholeProject(projectToUse);

        // Only go on if the project was cached successfully.
        if (this.IsWholeProjectCached(projectToUse))
        {
          CodeProject newStyleCopProject = new CodeProject(projectToUse.BaseDirectory.GetHashCode(), projectToUse.BaseDirectory, new Configuration(null));
          foreach (var currentFile in this.projectCache[projectToUse])
          {
            ProjectUtilities.Instance.Core.Environment.AddSourceCode(newStyleCopProject, currentFile.FilePath, null);
          }

          return newStyleCopProject;
        }
      }

      return null;
    }

    /// <summary>
    /// Get caching status of a project.
    /// </summary>
    /// <param name="projectToCheck">Project to check caching status.</param>
    /// <returns>True if the project got completely cached, false otherwise.</returns>
    public bool IsWholeProjectCached(Project projectToCheck)
    {
      Param.AssertNotNull(projectToCheck, "projectToCheck");

      // Make sure the project also in our cache else remove it from the completely cached projects!
      if (!this.projectCache.ContainsKey(projectToCheck))
      {
        this.completelyCachedProjects.Remove(projectToCheck);
      }

      return this.completelyCachedProjects.Contains(projectToCheck);
    }

    /// <summary>
    /// Removes a file with it's children from cache.
    /// </summary>
    /// <param name="fileToRemove">File to remove.</param>
    public void RemoveFile(ProjectFile fileToRemove)
    {
      this.RemoveFile(fileToRemove, fileToRemove.Project);
    }

    /// <summary>
    /// Removes a file with it's children from cache.
    /// </summary>
    /// <param name="fileToRemove">File to remove.</param>
    /// <param name="projectOfFile">Project of the file.</param>
    public void RemoveFile(ProjectFile fileToRemove, Project projectOfFile)
    {
      Param.AssertNotNull(fileToRemove, "fileToRemove");

      if (fileToRemove != null)
      {
        // If for some reason (e.g. OnNodeRemoved event) the project is null try to find the project cache which contains the file.
        if (projectOfFile == null)
        {
          foreach (var cacheKvP in this.projectCache)
          {
            if (cacheKvP.Value.Contains(fileToRemove))
            {
              projectOfFile = cacheKvP.Key;
              break;
            }
          }
        }

        if (projectOfFile != null && this.projectCache.ContainsKey(projectOfFile) && this.projectCache[projectOfFile].Contains(fileToRemove))
        {
          this.projectCache[projectOfFile].Remove(fileToRemove);

          // Cache the children files too
          if (fileToRemove.HasChildren)
          {
            foreach (var childFile in fileToRemove.DependentChildren)
            {
              this.RemoveFile(childFile, projectOfFile);
            }
          }
        }
      }
    }

    /// <summary>
    /// Enumerate the given folder and remove all files from cache.
    /// </summary>
    /// <param name="projectFolder">Project folder to enumerate.</param>
    public void RemoveFolder(ProjectFolder projectFolder)
    {
      if (projectFolder.Project != null && this.projectCache.ContainsKey(projectFolder.Project))
      {
        this.projectCache[projectFolder.Project].Where(
          file => file.FilePath.ParentDirectory.ToString().Contains(projectFolder.Path)).ToList().ForEach(file => this.RemoveFile(file, projectFolder.Project));
      }
    }

    /// <summary>
    /// Remove project and all it's files from cache.
    /// </summary>
    /// <param name="projectToRemove">Project to remove.</param>
    public void RemoveProject(Project projectToRemove)
    {
      Param.AssertNotNull(projectToRemove, "projectToRemove");

      if (projectToRemove != null && this.projectCache.ContainsKey(projectToRemove))
      {
        this.projectCache.Remove(projectToRemove);

        if (this.completelyCachedProjects.Contains(projectToRemove))
        {
          this.completelyCachedProjects.Remove(projectToRemove);
        }
      }
    }

    #endregion Public Methods

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
  }
}