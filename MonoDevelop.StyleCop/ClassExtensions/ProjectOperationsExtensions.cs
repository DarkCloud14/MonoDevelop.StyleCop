//-----------------------------------------------------------------------
// <copyright file="ProjectOperationsExtensions.cs">
//   APL 2.0
// </copyright>
// <license>
//   Copyright 2013-2016 Alexander Jochum
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
  using System.Globalization;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using MonoDevelop.Core;
  using MonoDevelop.Core.Instrumentation;
  using MonoDevelop.Core.ProgressMonitoring;
  using MonoDevelop.Ide;
  using MonoDevelop.Ide.Gui;
  using MonoDevelop.Ide.Tasks;
  using MonoDevelop.Projects;
  using global::StyleCop;

  /// <summary>
  /// Extension and helper methods for the <see cref="ProjectOperations"/> class to run the StyleCop analysis.
  /// </summary>
  internal static class ProjectOperationsExtensions
  {
    #region Private Static Fields

    /// <summary>
    /// AsyncOperation object which is used to check if a StyleCop analysis is still running.
    /// </summary>
    private static AsyncOperation currentStyleCopOperation = AsyncOperation.CompleteOperation;

    /// <summary>
    /// Object which is used to remove all warnings produced by StyleCop from the ErrorPad.
    /// </summary>
    private static object ownerObject = new object();

    /// <summary>
    /// List which will contain all violations as MonoDevelop TaskEntries reported by StyleCop.
    /// </summary>
    private static List<TaskListEntry> styleCopAnalysisResultList = new List<TaskListEntry>();

    /// <summary>
    /// MonoDevelop progress monitor which is used to output several messages to the output pane of ErrorPad.
    /// </summary>
    private static ProgressMonitor styleCopProgressMonitor = null;

    /// <summary>
    /// This is used to monitor the StyleCop analysis run with mdmonitor tool of MonoDevelop.
    /// </summary>
    private static TimerCounter styleCopTimer = InstrumentationService.CreateTimerCounter("StyleCop run in IDE", "IDE");

    #endregion Private Static Fields

    #region Internal Static Extension Methods

    /// <summary>
    /// Cancel a previously started StyleCop analysis.
    /// </summary>
    /// <param name="projectOperations">The <see cref="ProjectOperations"/> object being used.</param>
    internal static void CancelStyleCopAnalysis(this ProjectOperations projectOperations)
    {
      ProjectUtilities.Instance.Core.Cancel = true;
    }

    /// <summary>
    /// Returns true if a StyleCop analysis is still running.
    /// </summary>
    /// <param name="projectOperations">The <see cref="ProjectOperations"/> object being used.</param>
    /// <returns>Returns true if a StyleCop analysis is still running, or false otherwise.</returns>
    internal static bool IsStyleCopRunning(this ProjectOperations projectOperations)
    {
      return currentStyleCopOperation != null ? !currentStyleCopOperation.IsCompleted : true;
    }

    /// <summary>
    /// Sets up everything that is necessary to start the StyleCop analysis and then kicks off the worker thread.
    /// </summary>
    /// <param name="projectOperations">The <see cref="ProjectOperations"/> object being used.</param>
    /// <param name="entry">MonoDevelop build target i.e. a Solution or Project</param>
    /// <param name="fullAnalysis">True if a full analysis should be performed.</param>
    /// <param name="styleCopProjects">Collection of StyleCop projects to analyze.</param>
    /// <returns>AsyncOperation object which can be used to i.e. listen to the Completed event which is invoked when the operation is completed.</returns>
    internal static AsyncOperation StyleCopAnalysis(this ProjectOperations projectOperations, IBuildTarget entry, bool fullAnalysis, IList<CodeProject> styleCopProjects)
    {
      if (projectOperations.CurrentRunOperation != null && !projectOperations.CurrentRunOperation.IsCompleted)
      {
        return projectOperations.CurrentRunOperation;
      }

      if (currentStyleCopOperation != null && !currentStyleCopOperation.IsCompleted)
      {
        return currentStyleCopOperation;
      }

      CancellationTokenSource cs = new CancellationTokenSource();
      styleCopTimer.BeginTiming("Starting StyleCop analysis on " + entry.Name);
      styleCopProgressMonitor = IdeApp.Workbench.ProgressMonitors.GetBuildProgressMonitor();
      projectOperations.CurrentRunOperation = currentStyleCopOperation = new AsyncOperation(RunStyleCopAnalysisAsync(fullAnalysis, styleCopProjects), cs);

      return currentStyleCopOperation;
    }

    #endregion Internal Static Extension Methods

    #region Internal Static Methods

    /// <summary>
    /// Called when output should be added to the output pane of ErrorPad.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    internal static void StyleCopCoreOutputGenerated(object sender, OutputEventArgs e)
    {
      Param.Ignore(sender, e);

      if (styleCopProgressMonitor != null)
      {
        styleCopProgressMonitor.Log.WriteLine(e.Output);
      }
    }

    /// <summary>
    /// Called when a violation is found.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    internal static void StyleCopCoreViolationEncountered(object sender, ViolationEventArgs e)
    {
      Param.Ignore(sender, e);

      // Check the violation count. At some point we don't allow any more violations so we cancel the analyze run.
      if (e.SourceCode.Project.MaxViolationCount > 0 && styleCopAnalysisResultList.Count == e.SourceCode.Project.MaxViolationCount)
      {
        IdeApp.ProjectOperations.CancelStyleCopAnalysis();
      }

      string trimmedNamespace = e.Violation.Rule.Namespace;
      string searchValue = "StyleCop.";

      int indexOfProductName = trimmedNamespace.IndexOf(searchValue, StringComparison.Ordinal);
      if (indexOfProductName != -1 && indexOfProductName < trimmedNamespace.Length - 1)
      {
        trimmedNamespace = trimmedNamespace.Substring(indexOfProductName + searchValue.Length);

        int indexOfRulesString = trimmedNamespace.LastIndexOf("Rules", StringComparison.Ordinal);
        trimmedNamespace = indexOfRulesString != -1 ? trimmedNamespace.Substring(0, indexOfRulesString) : trimmedNamespace;
      }
      else
      {
        trimmedNamespace = string.Empty;
      }

      string fileName = string.Empty;
      if (e.Element != null)
      {
        fileName = e.Element.Document.SourceCode.Path;
      }
      else
      {
        if (e.SourceCode != null)
        {
          fileName = e.SourceCode.Path;
        }
      }

      TaskSeverity severity = TaskSeverity.Error;
      if (e.Warning || !e.SourceCode.Project.ViolationsAsErrors)
      {
        severity = TaskSeverity.Warning;
      }

      TaskListEntry styleCopWarning = new TaskListEntry(
                               fileName,
                               string.Concat(e.Violation.Rule.CheckId, " : ", trimmedNamespace, " : ", e.Message),
                               e.Location != null ? e.Location.Value.StartPoint.IndexOnLine : 1,
                               e.LineNumber,
                               severity,
                               TaskPriority.Normal,
                               ProjectUtilities.Instance.CachedProjects.GetProjectForFile(fileName),
                               ProjectOperationsExtensions.ownerObject);

      styleCopAnalysisResultList.Add(styleCopWarning);
    }

    #endregion Internal Static Methods

    #region Private Static Methods

    /// <summary>
    /// Clears the environment prior to analysis.
    /// </summary>
    private static void ClearEnvironmentPriorToAnalysis()
    {
      // Clear any items of StyleCop from ErrorPad
      IdeServices.TaskService.Errors.ClearByOwner(ProjectOperationsExtensions.ownerObject);

      // Clear any items from the analysis result list.
      styleCopAnalysisResultList.Clear();
    }

    /// <summary>
    /// Runs the StyleCop analysis on a second thread.
    /// </summary>
    /// <param name="fullAnalysis">True if a full analysis should be performed.</param>
    /// <param name="styleCopProjects">Collection of StyleCop projects to analyze.</param>
    /// <returns>As this is an async method returning Task is more or less equal to void but a caller method can than use the await keyword.</returns>
    private static async Task RunStyleCopAnalysisAsync(bool fullAnalysis, IList<CodeProject> styleCopProjects)
    {
      ClearEnvironmentPriorToAnalysis();
      SignalAnalysisStarted();

      try
      {
        await Task.Run(() =>
        {
          if (fullAnalysis)
          {
            ProjectUtilities.Instance.Core.FullAnalyze(styleCopProjects);
          }
          else
          {
            ProjectUtilities.Instance.Core.Analyze(styleCopProjects);
          }
        });
      }
      finally
      {
        // StyleCopAnalysisDone disposes the styleCopProgressMonitor
        StyleCopAnalysisDone();
      }
    }

    /// <summary>
    /// Signals to output that analysis has begun.
    /// </summary>
    private static void SignalAnalysisStarted()
    {
      if (styleCopProgressMonitor != null)
      {
        styleCopProgressMonitor.Log.WriteLine(string.Format(CultureInfo.InvariantCulture, "------ StyleCop {0} (build {1}) started ------\n", StyleCopVersion.VersionNumberMajorMinor, StyleCopVersion.VersionNumberFull));
      }
    }

    /// <summary>
    /// Called when the StyleCop analysis is done.
    /// </summary>
    private static void StyleCopAnalysisDone()
    {
      styleCopTimer.Trace("Begin reporting StyleCop result");

      try
      {
        if (styleCopAnalysisResultList != null)
        {
          styleCopProgressMonitor.Log.WriteLine();
          styleCopProgressMonitor.Log.WriteLine("------ StyleCop completed ------");

          styleCopTimer.Trace("Updating task service");
          IdeServices.TaskService.Errors.AddRange(styleCopAnalysisResultList);
          IdeServices.TaskService.Errors.ResetLocationList();
          IdeApp.Workbench.ActiveLocationList = IdeServices.TaskService.Errors;

          styleCopTimer.Trace("Reporting result");

          styleCopProgressMonitor.ReportWarning(string.Format(CultureInfo.InvariantCulture, "========== Violation Count: {0} ==========", styleCopAnalysisResultList.Count));
        }

        styleCopTimer.Trace("Showing results pad");

        try
        {
          Pad errorsPad = IdeApp.Workbench.Pads.ErrorsPad;
          switch (IdeApp.Preferences.ShowErrorPadAfterBuild.Value)
          {
          case BuildResultStates.Always:
            if (!errorsPad.Visible)
            {
              errorsPad.IsOpenedAutomatically = true;
            }

            errorsPad.Visible = true;
            errorsPad.BringToFront();
            break;

          case BuildResultStates.OnErrorsOrWarnings:
            if (IdeServices.TaskService.Errors.Any(task => task.Severity == TaskSeverity.Warning))
            {
              goto case BuildResultStates.Always;
            }

            break;
          }
        }
        catch
        {
        }

        if (styleCopAnalysisResultList != null)
        {
          TaskListEntry jumpTask = null;
          switch (IdeApp.Preferences.JumpToFirstErrorOrWarning.Value)
          {
          case JumpToFirst.ErrorOrWarning:
            jumpTask = styleCopAnalysisResultList.FirstOrDefault(t => t.Severity == TaskSeverity.Warning && TaskStore.IsProjectTaskFile(t));
            break;
          }

          if (jumpTask != null)
          {
            styleCopTimer.Trace("Jumping to first result position");
            jumpTask.JumpToPosition();
          }
        }
      }
      finally
      {
        styleCopProgressMonitor.Dispose();
        styleCopTimer.EndTiming();
      }
    }

    #endregion Private Static Methods
  }
}