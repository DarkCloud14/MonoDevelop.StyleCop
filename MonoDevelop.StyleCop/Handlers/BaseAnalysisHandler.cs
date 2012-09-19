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
  using MonoDevelop.Components.Commands;
  using MonoDevelop.Ide;
  using global::StyleCop;

  /// <summary>
  /// This class will manage the basic StyleCop functionality.
  /// </summary>
  internal abstract class BaseAnalysisHandler : CommandHandler
  {
    #region Protected Properties

    /// <summary>
    /// Gets or sets a value indicating whether a full analysis should be performed.
    /// </summary>
    protected bool FullAnalysis
    {
      get;
      set;
    }

    #endregion Protected Properties

    #region Protected Methods

    /// <summary>
    /// Gathers the list of files to analyze and hands it to a ProjectOperation method which will kick off the worker thread.
    /// </summary>
    /// <param name="analysisType">The analyze type being performed.</param>
    protected void Analyze(AnalysisType analysisType)
    {
      if (!IdeApp.ProjectOperations.IsStyleCopRunning())
      {
        IList<CodeProject> projects = ProjectUtilities.Instance.GetProjectList(analysisType);
        IdeApp.ProjectOperations.StyleCopAnalysis(IdeApp.ProjectOperations.CurrentSelectedBuildTarget, this.FullAnalysis, projects);
      }
    }

    /// <summary>
    /// Cancel a previously started StyleCop analysis.
    /// </summary>
    protected void CancelAnalysis()
    {
      if (IdeApp.ProjectOperations.IsStyleCopRunning())
      {
        IdeApp.ProjectOperations.CancelStyleCopAnalysis();
      }
    }

    #endregion Protected Methods
  }
}