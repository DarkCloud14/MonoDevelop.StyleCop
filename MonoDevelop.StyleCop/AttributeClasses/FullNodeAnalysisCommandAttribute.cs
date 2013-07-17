//-----------------------------------------------------------------------
// <copyright file="FullNodeAnalysisCommandAttribute.cs">
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
  /// NodeCommandHandler attribute for full StyleCop analysis command handling.
  /// </summary>
  internal class FullNodeAnalysisCommandAttribute : CustomCommandUpdaterAttribute
  {
    /// <summary>
    /// Updates the visibility of each command with this attribute.
    /// </summary>
    /// <param name="target">Target node handler.</param>
    /// <param name="cinfo">Command info.</param>
    protected override void CommandUpdate(object target, CommandInfo cinfo)
    {
      if (cinfo != null)
      {
        cinfo.Visible = true;

        base.CommandUpdate(target, cinfo);

        if (IdeApp.ProjectOperations.IsStyleCopRunning())
        {
          // Hide this entry if StyleCop is running.
          cinfo.Visible = false;
        }
      }
    }
  }
}