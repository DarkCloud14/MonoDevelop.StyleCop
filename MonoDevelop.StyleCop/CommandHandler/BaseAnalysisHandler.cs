//-----------------------------------------------------------------------
// <copyright file="BaseAnalysisHandler.cs">
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
  using System.Linq;
  using MonoDevelop.Components.Commands;
  using MonoDevelop.Core.Serialization;
  using MonoDevelop.Ide;
  using MonoDevelop.Ide.Gui;
  using MonoDevelop.Ide.Gui.Components;
  using MonoDevelop.Projects;
  using global::StyleCop;

  /// <summary>
  /// This class will manage the basic StyleCop functionality.
  /// </summary>
  internal abstract class BaseAnalysisHandler : CommandHandler
  {
    #region Protected Readonly Fields

    /// <summary>
    /// The value indicates whether a full analysis should be performed.
    /// </summary>
    protected readonly bool FullAnalysis;

    /// <summary>
    /// The type of the analysis. See <see cref="AnalysisType"/> for more information.
    /// </summary>
    protected readonly AnalysisType TypeOfAnalysis;

    #endregion Protected Readonly Fields

    #region Private Fields

    /// <summary>
    /// Indicates if a previously started StyleCop run should be canceled.
    /// </summary>
    private bool cancelStypeCopRun = false;

    /// <summary>
    /// Holds all StyleCop compatible project files of the active document.
    /// </summary>
    private List<ProjectFile> enumeratedActiveDocument = null;

    #endregion Private Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseAnalysisHandler"/> class.
    /// </summary>
    /// <param name="analysisType">The type of analysis this class will run.</param>
    protected BaseAnalysisHandler(AnalysisType analysisType)
      : this(false, analysisType)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseAnalysisHandler"/> class.
    /// </summary>
    /// <param name="fullAnalysis">Set to true if StyleCop should run a full analysis, false otherwise.</param>
    /// <param name="analysisType">The type of analysis this class will run.</param>
    protected BaseAnalysisHandler(bool fullAnalysis, AnalysisType analysisType)
    {
      this.FullAnalysis = fullAnalysis;
      this.TypeOfAnalysis = analysisType;
    }

    #endregion Constructor

    #region Protected Override Methods

    /// <summary>
    /// Starts or cancels a StyleCop analysis with the settings of it's child class.
    /// </summary>
    protected override void Run()
    {
      base.Run();

      if (this.cancelStypeCopRun)
      {
        IdeApp.ProjectOperations.CancelStyleCopAnalysis();
      }
      else
      {
        if (IdeApp.Workbench != null)
        {
          IdeApp.Workbench.SaveAll();
        }

        this.Analyze();
      }
    }

    /// <summary>
    /// Update availability of the StyleCop command for the selected project/projects in ProjectPad.
    /// </summary>
    /// <param name="info">A <see cref="CommandInfo"/></param>
    protected override void Update(CommandInfo info)
    {
      base.Update(info);

      info.Visible = false;
      this.cancelStypeCopRun = false;

      if (IdeApp.ProjectOperations.CurrentRunOperation.IsCompleted)
      {
        // Set the default StyleCop run text.
        info.Text = StaticStringResources.StyleCopRunText;

        // The check for active document is a bit different.
        if (this.TypeOfAnalysis == AnalysisType.ActiveDocument)
        {
          var activeDocument = IdeApp.Workbench.ActiveDocument;
          
          if (activeDocument.HasProject)
          {
            ProjectFile projectFile = activeDocument.Project.GetProjectFile(activeDocument.FileName);
            this.enumeratedActiveDocument = ProjectUtilities.Instance.EnumerateFile(projectFile);
            
            if (this.enumeratedActiveDocument.Count > 0)
            {
              info.Visible = true;
            }
          }
        }
        else
        {
          // Always show the entry for the solution analyser handlers
          info.Visible = true;
        }
      }
      else
      {
        if (IdeApp.ProjectOperations.IsStyleCopRunning())
        {
          // Only show one cancel entry.
          if (this.FullAnalysis)
          {
            info.Visible = false;
          }
          else
          {
            info.Visible = true;
          }

          info.Text = StaticStringResources.StyleCopCancelText;
          this.cancelStypeCopRun = true;
        }
      }
    }

    #endregion Protected Override Methods

    #region Private Methods

    /// <summary>
    /// Gathers the list of files to analyze and hands it to a ProjectOperation method which will kick off the worker thread.
    /// </summary>
    private void Analyze()
    {
      if (!IdeApp.ProjectOperations.IsStyleCopRunning())
      {
        Dictionary<Project, HashSet<ProjectFile>> tempProjectDictionary = new Dictionary<Project, HashSet<ProjectFile>>();

        if (this.TypeOfAnalysis == AnalysisType.ActiveDocument)
        {
          foreach (var file in this.enumeratedActiveDocument)
          {
            if (file != null)
            {
              if (file.Project != null && !tempProjectDictionary.ContainsKey(file.Project))
              {
                tempProjectDictionary.Add(file.Project, new HashSet<ProjectFile>());
              }

              if (file.Project != null && !tempProjectDictionary[file.Project].Contains(file))
              {
                tempProjectDictionary[file.Project].Add(file);
              }
            }
          }
        }
        else
        {
          foreach (var project in IdeApp.ProjectOperations.CurrentSelectedSolution.GetAllProjects())
          {
            if (!tempProjectDictionary.ContainsKey(project))
            {
              tempProjectDictionary.Add(project, null);
            }
          }
        }

        IList<CodeProject> projects = ProjectUtilities.Instance.CreateStyleCopCodeProjects(tempProjectDictionary);
        IdeApp.ProjectOperations.StyleCopAnalysis(IdeApp.ProjectOperations.CurrentSelectedBuildTarget, this.FullAnalysis, projects);
      }
    }

    #endregion Private Methods
  }
}