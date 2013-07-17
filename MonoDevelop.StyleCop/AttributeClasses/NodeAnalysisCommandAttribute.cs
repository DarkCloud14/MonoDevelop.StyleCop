//-----------------------------------------------------------------------
// <copyright file="NodeAnalysisCommandAttribute.cs">
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
  /// NodeCommandHandler attribute for StyleCop analysis command handling.
  /// </summary>
  internal class NodeAnalysisCommandAttribute : CustomCommandUpdaterAttribute
  {
    /// <summary>
    /// Updates the text and visibility of each command with this attribute.
    /// </summary>
    /// <param name="target">Target node handler.</param>
    /// <param name="cinfo">Command info.</param>
    protected override void CommandUpdate(object target, CommandInfo cinfo)
    {
      if (cinfo != null)
      {
        cinfo.Visible = true;
        StyleCopNodeCommandHandler.CancelStypeCopRun = false;

        base.CommandUpdate(target, cinfo);

        if (IdeApp.ProjectOperations.CurrentRunOperation.IsCompleted)
        {
          // Set the default StyleCop run text.
          if (string.IsNullOrEmpty(cinfo.Text))
          {
            cinfo.Text = StaticStringResources.StyleCopRunText;
          }
        }
        else
        {
          if (IdeApp.ProjectOperations.IsStyleCopRunning())
          {
            cinfo.Text = StaticStringResources.StyleCopCancelText;
            StyleCopNodeCommandHandler.CancelStypeCopRun = true;
          }
        }
      }
    }
  }
}