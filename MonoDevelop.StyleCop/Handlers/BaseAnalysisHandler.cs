//-----------------------------------------------------------------------
// <copyright file="BaseAnalysisHandler.cs">
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
    /// Text of cancel StyleCop context menu entry.
    /// </summary>
    private string styleCopCancelText = "Cancel StyleCop";

    /// <summary>
    /// Text of full StyleCop analysis context menu entry.
    /// </summary>
    private string styleCopFullAnalysisText = "Run StyleCop (Rescan All)";

    /// <summary>
    /// Text of default StyleCop analysis context menu entry.
    /// </summary>
    private string styleCopRunText = "Run StyleCop";

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

    #region Protected Properties

    /// <summary>
    /// Gets the text of cancel StyleCop context menu entry.
    /// </summary>
    protected string StyleCopCancelText
    {
      get { return this.styleCopCancelText; }
    }

    /// <summary>
    /// Gets the text of full StyleCop analysis context menu entry.
    /// </summary>
    protected string StyleCopFullAnalysisText
    {
      get { return this.styleCopFullAnalysisText; }
    }

    /// <summary>
    /// Gets the text of default StyleCop analysis context menu entry.
    /// </summary>
    protected string StyleCopRunText
    {
      get { return this.styleCopRunText; }
    }

    #endregion Protected Properties

    #region Private Static Properties

    /// <summary>
    /// Gets or sets a previously active document.
    /// </summary>
    private static Document CachedActiveDocument
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a previous ProjectPad node selection.
    /// </summary>
    private static ITreeNavigator[] CachedNodeSelection
    {
      get;
      set;
    }
    #endregion Private Static Propertiess

    #region Protected Override Methods

    /// <summary>
    /// Starts or cancels a StyleCop analysis with the settings of it's child class.
    /// </summary>
    protected override void Run()
    {
      base.Run();

      if (this.cancelStypeCopRun)
      {
        this.CancelAnalysis();
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
        if (string.IsNullOrEmpty(info.Text))
        {
          info.Text = this.styleCopRunText;
        }

        bool cacheCurrentSelection = true;

        // The check for active document is a bit different.
        if (this.TypeOfAnalysis != AnalysisType.ActiveDocument)
        {
          CachedActiveDocument = null;

          // Check if the current selection is the same as the previous selection and if it's the same don't call CacheKnownFiles again.
          if (CachedNodeSelection != null && CachedNodeSelection.Length > 0)
          {
            // Get the active ProjectPad tree selection
            ITreeNavigator[] currentSelection = ProjectUtilities.Instance.ProjectPad.TreeView.GetSelectedNodes();
            if (currentSelection != null && currentSelection.Length == CachedNodeSelection.Length)
            {
              cacheCurrentSelection = false;

              // Now check if the nodes are the same
              foreach (var currentNode in currentSelection)
              {
                object currentSolutionItem = currentNode.DataItem;
                var nodeDataItemInPreviousSelection = CachedNodeSelection.Select(node => node.DataItem as object).FirstOrDefault(
                  value => value != null && currentSolutionItem != null && value.GetHashCode() == currentSolutionItem.GetHashCode());

                if (nodeDataItemInPreviousSelection == null)
                {
                  CachedNodeSelection = currentSelection;
                  cacheCurrentSelection = true;
                  break;
                }
              }
            }
            else
            {
              CachedNodeSelection = currentSelection;
            }
          }
          else
          {
            ITreeNavigator[] currentSelection = ProjectUtilities.Instance.ProjectPad.TreeView.GetSelectedNodes();
            CachedNodeSelection = currentSelection;
          }
        }
        else
        {
          CachedNodeSelection = null;

          var activeDocument = IdeApp.Workbench.ActiveDocument;
          if (CachedActiveDocument != null && CachedActiveDocument.GetHashCode() == activeDocument.GetHashCode())
          {
            cacheCurrentSelection = false;
          }
          else
          {
            CachedActiveDocument = activeDocument;
          }
        }

        if (cacheCurrentSelection)
        {
          if (ProjectUtilities.Instance.CacheKnownFiles(this.TypeOfAnalysis))
          {
            info.Visible = true;
          }
        }
        else
        {
          if (ProjectUtilities.Instance.CachedFiles != null && ProjectUtilities.Instance.CachedFiles.Count > 0)
          {
            info.Visible = true;
          }
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

          info.Text = this.styleCopCancelText;
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
        IList<CodeProject> projects = ProjectUtilities.Instance.GetProjectList();
        IdeApp.ProjectOperations.StyleCopAnalysis(IdeApp.ProjectOperations.CurrentSelectedBuildTarget, this.FullAnalysis, projects);
      }
    }

    /// <summary>
    /// Cancel a previously started StyleCop analysis.
    /// </summary>
    private void CancelAnalysis()
    {
      IdeApp.ProjectOperations.CancelStyleCopAnalysis();
    }

    #endregion Private Methods
  }
}