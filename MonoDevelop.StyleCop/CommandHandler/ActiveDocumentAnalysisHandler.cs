//-----------------------------------------------------------------------
// <copyright file="ActiveDocumentAnalysisHandler.cs">
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
  using MonoDevelop.Components.Commands;
  using MonoDevelop.Ide;

  /// <summary>
  /// Class which handles the analysis type ActiveDocument.
  /// </summary>
  internal sealed class ActiveDocumentAnalysisHandler : BaseAnalysisHandler
  {
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ActiveDocumentAnalysisHandler"/> class.
    /// </summary>
    public ActiveDocumentAnalysisHandler()
      : base(AnalysisType.ActiveDocument)
    {
    }

    #endregion Constructor

    #region Protected Override Methods

    /// <summary>
    /// Update availability of the StyleCop command for the active document.
    /// </summary>
    /// <param name="info">A <see cref="CommandInfo"/></param>
    protected override void Update(CommandInfo info)
    {
      if (IdeApp.Workbench.ActiveDocument != null)
      {
        base.Update(info);
      }
      else
      {
        info.Visible = false;
      }
    }

    #endregion Protected Override Methods
  }
}