﻿//-----------------------------------------------------------------------
// <copyright file="StyleCopNodeCommandHandler.cs">
//   APL 2.0
// </copyright>
// <license>
//   Copyright 2013, 2018 Alexander Jochum
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
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using MonoDevelop.Components.Commands;
  using MonoDevelop.Ide;
  using MonoDevelop.Ide.Gui.Components;
  using MonoDevelop.Ide.Gui.Pads.ProjectPad;
  using MonoDevelop.Projects;
  using global::StyleCop;

  /// <summary>
  /// This class extends NodeCommandHandler class and adds the StyleCop analysis functionality to each compatible node.
  /// </summary>
  internal sealed class StyleCopNodeCommandHandler : NodeCommandHandler
  {
    #region Private Static Fields

    /// <summary>
    /// Holds the last tree selection.
    /// </summary>
    private static ITreeNavigator[] cachedTreeSelection = null;

    /// <summary>
    /// Dictionary holds the projects and files of the current/previous selection.
    /// </summary>
    private static Dictionary<Project, HashSet<ProjectFile>> temporaryProjectSelectionCache = new Dictionary<Project, HashSet<ProjectFile>>();

    /// <summary>
    /// If zero the data type count of the current selection gets determined and if necessary a new code project cache created.
    /// The data type counter is also used to execute StyleCop analysis only if the counter reaches zero. This is necessary as
    /// the OnNodeAnalysis function gets called for each data type but we want to execute StyleCop just once for the whole selection.
    /// </summary>
    private static int dataTypeCounter = 0;

    #endregion Private Static Fields

    #region Public Static Properties

    /// <summary>
    /// Gets or sets a value indicating whether a previously started StyleCop run should be canceled.
    /// </summary>
    public static bool CancelStypeCopRun
    {
      get;
      set;
    }

    #endregion Public Static Properties

    #region Private Methods

    /// <summary>
    /// Checks if it's necessary to parse and cache the current selection.
    /// </summary>
    /// <param name="currentTreeSelection">Current tree selection.</param>
    /// <param name="forceCaching">Overrides the current logic and forces the caching of the current selection.</param>
    /// <returns>True if it's necessary to parse and cache the current selection, false otherwise.</returns>
    private bool MustParseAndCacheCurrentSelection(ITreeNavigator[] currentTreeSelection, bool forceCaching)
    {
      bool parseAndCacheCurrentSelection = false;

      // Check if the cached tree selection and the current one are still the same.
      if (cachedTreeSelection != null && currentTreeSelection != null && cachedTreeSelection.Length == currentTreeSelection.Length && !forceCaching)
      {
        // Check if the selection is still the same.
        for (int i = 0; i < cachedTreeSelection.Length; i++)
        {
          if (!cachedTreeSelection[i].DataItem.Equals(currentTreeSelection[i].DataItem))
          {
            cachedTreeSelection = currentTreeSelection;
            parseAndCacheCurrentSelection = true;
            break;
          }
        }
      }
      else
      {
        cachedTreeSelection = currentTreeSelection;
        parseAndCacheCurrentSelection = true;
      }

      return parseAndCacheCurrentSelection;
    }

    /// <summary>
    /// Excludes or includes selected project items from StyleCop analysis.
    /// </summary>
    [AllowMultiSelection, NodeAnalysisCommand]
    [CommandHandler(AnalysisType.ExcludeProjectFile)]
    private void OnExcludeProjectFile()
    {
      if (!CancelStypeCopRun && !IdeApp.ProjectOperations.IsStyleCopRunning())
      {
        if (temporaryProjectSelectionCache.Count == 1)
        {
          foreach (var projectKvP in temporaryProjectSelectionCache)
          {
            if (projectKvP.Value != null)
            {
              foreach (var projectFile in projectKvP.Value)
              {
                bool excludeFromStyleCop = false;

                if (projectFile.Metadata.HasProperty("ExcludeFromStyleCop"))
                {
                  excludeFromStyleCop = !projectFile.Metadata.GetValue("ExcludeFromStyleCop", false);
                  projectFile.Metadata.SetValue("ExcludeFromStyleCop", excludeFromStyleCop);
                }
                else if (projectFile.Metadata.HasProperty("ExcludeFromSourceAnalysis"))
                {
                  excludeFromStyleCop = !projectFile.Metadata.GetValue("ExcludeFromSourceAnalysis", false);
                  projectFile.Metadata.SetValue("ExcludeFromSourceAnalysis", excludeFromStyleCop);
                }
                else
                {
                  projectFile.Metadata.SetValue("ExcludeFromStyleCop", excludeFromStyleCop);
                }
              }
            }

            // Now we save each project so that the modified metadata is stored in the project file.
            if (projectKvP.Key.ParentSolution != null && IdeApp.ProjectOperations != null)
            {
              IdeApp.ProjectOperations.SaveAsync(projectKvP.Key);
            }
          }
        }
      }
    }

    /// <summary>
    /// Starts a full StyleCop analysis.
    /// </summary>
    [AllowMultiSelection, FullNodeAnalysisCommandAttribute]
    [CommandHandler(AnalysisType.FullNodeAnalysis)]
    private void OnFullNodeAnalysis()
    {
      if (CancelStypeCopRun && IdeApp.ProjectOperations.IsStyleCopRunning())
      {
        dataTypeCounter = 0;
        IdeApp.ProjectOperations.CancelStyleCopAnalysis();
      }
      else
      {
        IList<CodeProject> projects = ProjectUtilities.Instance.CreateStyleCopCodeProjects(temporaryProjectSelectionCache);
        IdeApp.ProjectOperations.StyleCopAnalysis(IdeApp.ProjectOperations.CurrentSelectedBuildTarget, true, projects);
      }
    }

    /// <summary>
    /// Starts a standard StyleCop analysis.
    /// </summary>
    [AllowMultiSelection, NodeAnalysisCommand]
    [CommandHandler(AnalysisType.NodeAnalysis)]
    private void OnNodeAnalysis()
    {
      if (CancelStypeCopRun && IdeApp.ProjectOperations.IsStyleCopRunning())
      {
        dataTypeCounter = 0;
        IdeApp.ProjectOperations.CancelStyleCopAnalysis();
      }
      else
      {
        IList<CodeProject> projects = ProjectUtilities.Instance.CreateStyleCopCodeProjects(temporaryProjectSelectionCache);
        IdeApp.ProjectOperations.StyleCopAnalysis(IdeApp.ProjectOperations.CurrentSelectedBuildTarget, false, projects);
      }
    }

    /// <summary>
    /// Updates the node analysis command and hides it if necessary.
    /// </summary>
    /// <param name="info">A <see cref="CommandInfo"/>.</param>
    [CommandUpdateHandler(AnalysisType.ExcludeProjectFile)]
    private void OnUpdateExcludeProjectFile(CommandInfo info)
    {
      if (!CancelStypeCopRun && !IdeApp.ProjectOperations.IsStyleCopRunning())
      {
        info.Visible = false;

        var currentTreeSelection = Tree.GetSelectedNodes();

        // Only do the caching stuff once and not for each data type (function is called for each data type!)
        if (dataTypeCounter <= 0)
        {
          try
          {
            if (IdeApp.Workbench != null)
            {
              IdeApp.Workbench.SaveAll();
            }

            List<ITreeNavigator> projectFilesInSelection = currentTreeSelection.Where(treeNav => treeNav.DataItem is ProjectFile).ToList();
            this.SetDataTypeCounterForCurrentSelection(projectFilesInSelection: projectFilesInSelection);

            if (this.MustParseAndCacheCurrentSelection(currentTreeSelection, false))
            {
              this.ParseAndCacheCurrentSelection(projectFilesInSelection: projectFilesInSelection);
            }

            if (projectFilesInSelection.Count == 1)
            {
              foreach (var currentFile in projectFilesInSelection)
              {
                var projectFile = currentFile.DataItem as ProjectFile;
                if (projectFile != null)
                {
                  if (projectFile.Metadata.GetValue("ExcludeFromStyleCop", false) || projectFile.Metadata.GetValue("ExcludeFromSourceAnalysis", false))
                  {
                    info.Text = StaticStringResources.StyleCopIncludeProjectItemText;
                  }
                }
              }
            }

            dataTypeCounter--;
          }
          catch (Exception ex)
          {
            dataTypeCounter = 0;
            throw ex;
          }
        }
        else
        {
          dataTypeCounter--;
        }

        if (temporaryProjectSelectionCache.Count > 0)
        {
          info.Visible = true;
        }
      }
    }

    /// <summary>
    /// Updates the full node analysis command and hides it if necessary.
    /// </summary>
    /// <param name="info">A <see cref="CommandInfo"/>.</param>
    [CommandUpdateHandler(AnalysisType.FullNodeAnalysis)]
    private void OnUpdateFullNodeAnalysis(CommandInfo info)
    {
      if (!CancelStypeCopRun && !IdeApp.ProjectOperations.IsStyleCopRunning())
      {
        info.Visible = false;

        var currentTreeSelection = Tree.GetSelectedNodes();

        // Only do the caching stuff once and not for each data type (function is called for each data type!)
        if (dataTypeCounter <= 0)
        {
          try
          {
            if (IdeApp.Workbench != null)
            {
              IdeApp.Workbench.SaveAll();
            }

            List<ITreeNavigator> selectedSolutions = currentTreeSelection.Where(treeNav => treeNav.DataItem is Solution).ToList();
            List<ITreeNavigator> selectedProjects = currentTreeSelection.Where(treeNav => treeNav.DataItem is Project).ToList();
            this.SetDataTypeCounterForCurrentSelection(solutionsInSelection: selectedSolutions, projectsInSelection: selectedProjects);

            if (this.MustParseAndCacheCurrentSelection(currentTreeSelection, true))
            {
              this.ParseAndCacheCurrentSelection(solutionsInSelection: selectedSolutions, projectsInSelection: selectedProjects);
            }

            dataTypeCounter--;
          }
          catch (Exception ex)
          {
            dataTypeCounter = 0;
            throw ex;
          }
        }
        else
        {
          dataTypeCounter--;
        }

        if (temporaryProjectSelectionCache.Count > 0)
        {
          info.Visible = true;
        }
      }
    }

    /// <summary>
    /// Updates the node analysis command and hides it if necessary.
    /// </summary>
    /// <param name="info">A <see cref="CommandInfo"/>.</param>
    [CommandUpdateHandler(AnalysisType.NodeAnalysis)]
    private void OnUpdateNodeAnalysis(CommandInfo info)
    {
      if (!CancelStypeCopRun && !IdeApp.ProjectOperations.IsStyleCopRunning())
      {
        info.Visible = false;

        var currentTreeSelection = Tree.GetSelectedNodes();

        // Only do the caching stuff once and not for each data type (function is called for each data type!)
        if (dataTypeCounter <= 0)
        {
          try
          {
            if (IdeApp.Workbench != null)
            {
              IdeApp.Workbench.SaveAll();
            }

            List<ITreeNavigator> selectedProjects = currentTreeSelection.Where(treeNav => treeNav.DataItem is Project).ToList();
            List<ITreeNavigator> selectedProjectFiles = currentTreeSelection.Where(treeNav => treeNav.DataItem is ProjectFile).ToList();
            List<ITreeNavigator> selectedProjectFolders = currentTreeSelection.Where(treeNav => treeNav.DataItem is ProjectFolder).ToList();
            List<ITreeNavigator> selectedSolutions = currentTreeSelection.Where(treeNav => treeNav.DataItem is Solution).ToList();
            this.SetDataTypeCounterForCurrentSelection(selectedSolutions, selectedProjects, selectedProjectFolders, selectedProjectFiles);

            if (this.MustParseAndCacheCurrentSelection(currentTreeSelection, false))
            {
              this.ParseAndCacheCurrentSelection(selectedSolutions, selectedProjects, selectedProjectFolders, selectedProjectFiles);
            }

            dataTypeCounter--;
          }
          catch (Exception ex)
          {
            dataTypeCounter = 0;
            throw ex;
          }
        }
        else
        {
          dataTypeCounter--;
        }

        if (temporaryProjectSelectionCache.Count > 0)
        {
          if (temporaryProjectSelectionCache.Count == 1 && temporaryProjectSelectionCache.First().Value != null && temporaryProjectSelectionCache.First().Value.Count == 1)
          {
            // Ok we've just one item selected so we check if that item is excluded from StyleCop analysis and show or hide the StyleCop entry.
            var projectItem = temporaryProjectSelectionCache.First().Value.First();
            if (!projectItem.Metadata.GetValue("ExcludeFromStyleCop", false) && !projectItem.Metadata.GetValue("ExcludeFromSourceAnalysis", false))
            {
              info.Visible = true;
            }
          }
          else
          {
            info.Visible = true;
          }
        }
      }
    }

    /// <summary>
    /// Function will go through the whole selection and creates StyleCop code project for each valid item.
    /// It'll also cache the code projects in case the next call is done with the same selection..
    /// </summary>
    /// <param name="solutionsInSelection">List with all solutions in current selection.</param>
    /// <param name="projectsInSelection">List with all projects in current selection.</param>
    /// <param name="projectFoldersInSelection">List with all project folders in current selection.</param>
    /// <param name="projectFilesInSelection">List with all project files in current selection.</param>
    private void ParseAndCacheCurrentSelection(List<ITreeNavigator> solutionsInSelection = null, List<ITreeNavigator> projectsInSelection = null, List<ITreeNavigator> projectFoldersInSelection = null, List<ITreeNavigator> projectFilesInSelection = null)
    {
      temporaryProjectSelectionCache.Clear();

      if (solutionsInSelection != null)
      {
        foreach (var solution in solutionsInSelection)
        {
          foreach (var project in ((Solution)solution.DataItem).GetAllProjects())
          {
            if (!temporaryProjectSelectionCache.ContainsKey(project))
            {
              if (ProjectUtilities.Instance.IsKnownProjectType(project))
              {
                temporaryProjectSelectionCache.Add(project, null);
              }
            }
          }
        }
      }

      if (projectsInSelection != null)
      {
        foreach (var projectNode in projectsInSelection)
        {
          Project project = projectNode.DataItem as Project;
          if (!temporaryProjectSelectionCache.ContainsKey(project))
          {
            if (ProjectUtilities.Instance.IsKnownProjectType(project))
            {
              temporaryProjectSelectionCache.Add(project, null);
            }
          }
        }
      }

      // For ProjectFolder and ProjectFiles we need a different handling
      // as we only want to create a code project for files and folders which projects are not already in cache.
      List<ProjectFile> allKnownFilesInSelection = new List<ProjectFile>();

      if (projectFoldersInSelection != null)
      {
        foreach (var folder in projectFoldersInSelection)
        {
          allKnownFilesInSelection.AddRange(ProjectUtilities.Instance.EnumerateFolder(folder.DataItem as ProjectFolder));
        }
      }

      if (projectFilesInSelection != null)
      {
        foreach (var file in projectFilesInSelection)
        {
          allKnownFilesInSelection.AddRange(ProjectUtilities.Instance.EnumerateFile(file.DataItem as ProjectFile));
        }
      }

      // Now that we've all files from the file and folder selection go through them and filter files which are not already added before.
      foreach (var file in allKnownFilesInSelection)
      {
        if (file.Project != null)
        {
          if (!temporaryProjectSelectionCache.ContainsKey(file.Project))
          {
            temporaryProjectSelectionCache.Add(file.Project, new HashSet<ProjectFile>());
          }

          if (temporaryProjectSelectionCache[file.Project] != null && !temporaryProjectSelectionCache[file.Project].Contains(file))
          {
            temporaryProjectSelectionCache[file.Project].Add(file);
          }
        }
      }
    }

    /// <summary>
    /// Goes through each data type in current selection and increases the data type counter if necessary.
    /// </summary>
    /// <param name="solutionsInSelection">List with all solutions in current selection.</param>
    /// <param name="projectsInSelection">List with all projects in current selection.</param>
    /// <param name="projectFoldersInSelection">List with all project folders in current selection.</param>
    /// <param name="projectFilesInSelection">List with all project files in current selection.</param>
    private void SetDataTypeCounterForCurrentSelection(List<ITreeNavigator> solutionsInSelection = null, List<ITreeNavigator> projectsInSelection = null, List<ITreeNavigator> projectFoldersInSelection = null, List<ITreeNavigator> projectFilesInSelection = null)
    {
      dataTypeCounter = 0;

      // Find out the data type count
      if (solutionsInSelection != null && solutionsInSelection.Count > 0)
      {
        dataTypeCounter++;
      }

      if (projectsInSelection != null && projectsInSelection.Count > 0)
      {
        dataTypeCounter++;
      }

      if (projectFoldersInSelection != null && projectFoldersInSelection.Count > 0)
      {
        dataTypeCounter++;
      }

      if (projectFilesInSelection != null && projectFilesInSelection.Count > 0)
      {
        dataTypeCounter++;
      }
    }

    #endregion Private Methods
  }
}