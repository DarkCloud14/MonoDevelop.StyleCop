//-----------------------------------------------------------------------
// <copyright file="SpellingOptionsPanel.cs">
//   APL 2.0
// </copyright>
// <license>
//   Copyright 2017 Alexander Jochum
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
namespace MonoDevelop.StyleCop.Gui.OptionPanels
{
  using MonoDevelop.Components;
  using MonoDevelop.StyleCop.Gui.OptionPanelWidgets;

  /// <summary>
  /// StyleCop spelling options panel.
  /// </summary>
  internal class SpellingOptionsPanel : StyleCopOptionsPanel
  {
    #region Private Fields

    /// <summary>
    /// The spelling options panel widget.
    /// </summary>
    private SpellingOptionsPanelWidget widget = new SpellingOptionsPanelWidget();

    #endregion Private Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="MonoDevelop.StyleCop.Gui.OptionPanels.SpellingOptionsPanel"/> class.
    /// </summary>
    internal SpellingOptionsPanel()
    {
    }

    #endregion Constructor

    #region Public  Override Methods

    /// <summary>
    /// Applies the changes.
    /// </summary>
    public override void ApplyChanges()
    {
    }

    /// <summary>
    /// Creates the options panel widget.
    /// </summary>
    /// <returns>The options panel widget.</returns>
    public override Control CreatePanelWidget()
    {
      return this.widget;
    }

    /// <summary>
    /// Determines whether this instance is visible.
    /// </summary>
    /// <returns><c>true</c> if this instance is visible; otherwise, <c>false</c>.</returns>
    public override bool IsVisible()
    {
      return true;
    }

    #endregion Public Override Methods
  }
}