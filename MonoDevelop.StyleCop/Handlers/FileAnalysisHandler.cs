//-----------------------------------------------------------------------
// <copyright file="FileAnalysisHandler.cs">
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
  using System.Linq;
  using System.Text;
  using MonoDevelop.Components.Commands;
  using MonoDevelop.Ide;

  /// <summary>
  /// Class which handles the analysis type File.
  /// </summary>
  internal sealed class FileAnalysisHandler : BaseAnalysisHandler
  {
    #region Protected Override Methods

    /// <summary>
    /// Starts a full StyleCop analysis of type File.
    /// </summary>
    protected override void Run()
    {
      base.Run();

      this.FullAnalysis = true;
      this.Analyze(AnalysisType.File);
    }

    /// <summary>
    /// Update availability of the StyleCop command for the selected file/s in ProjectPad.
    /// </summary>
    /// <param name="info">A <see cref="CommandInfo"/></param>
    protected override void Update(CommandInfo info)
    {
      base.Update(info);

      if (IdeApp.ProjectOperations.CurrentRunOperation.IsCompleted)
      {
        // TODO correct the this check! Check the selected files and not the active document..
        if (ProjectUtilities.Instance.SupportsStyleCop(AnalysisType.File))
        {
          info.Visible = true;
        }
        else
        {
          info.Visible = false;
        }
      }
    }

    #endregion Protected Override Methods
  }
}
