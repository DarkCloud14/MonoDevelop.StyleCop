//-----------------------------------------------------------------------
// <copyright file="StyleCopOptionsPanel.cs">
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
  using System;
  using MonoDevelop.Components;
  using MonoDevelop.Ide.Gui.Dialogs;
  using MonoDevelop.StyleCop.Gui.OptionPanelWidgets;

  /// <summary>
  /// StyleCop options panel base class.
  /// </summary>
  internal abstract class StyleCopOptionsPanel : OptionsPanel
  {
    #region Private Fields

    /// <summary>
    /// The options panel widget.
    /// </summary>
    private StyleCopOptionsPanelWidget widget;

    #endregion Private Fields

    #region Protected Properties

    /// <summary>
    /// Gets or sets the widget.
    /// </summary>
    /// <value>The widget.</value>
    protected StyleCopOptionsPanelWidget Widget
    {
      get
      {
        return this.widget;
      }

      set
      {
        this.widget = value;
      }
    }

    #endregion Protected Properties

    #region Public  Override Methods

    /// <summary>
    /// Applies the changes.
    /// </summary>
    public override void ApplyChanges()
    {
      this.widget.ApplyChanges();
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
    /// Initializes the OptionsPanel.
    /// </summary>
    /// <param name="dialog">Parent dialog.</param>
    /// <param name="dataObject">Data object (should be the settings handler object in our case).</param>
    public override void Initialize(OptionsDialog dialog, object dataObject)
    {
      base.Initialize(dialog, dataObject);
      this.widget.Initialize(dataObject as StyleCopSettingsHandler);
    }

    /// <summary>
    /// Determines whether this instance is visible.
    /// </summary>
    /// <returns><c>true</c> if this instance is visible; otherwise, <c>false</c>.</returns>
    public override bool IsVisible()
    {
      return true;
    }

    /// <summary>
    /// Validates the changes.
    /// </summary>
    /// <returns><c>true</c>, if changes was validated, <c>false</c> otherwise.</returns>
    public override bool ValidateChanges()
    {
      return this.widget.ValidateChanges();
    }

    #endregion Public Override Methods
  }
}