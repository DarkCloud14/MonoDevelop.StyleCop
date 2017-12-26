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
  using MonoDevelop.Ide.Gui.Dialogs;

  /// <summary>
  /// StyleCop options panel base class.
  /// </summary>
  internal abstract class StyleCopOptionsPanel : OptionsPanel
  {
    #region Private Fields

    /// <summary>
    /// The settings handler object.
    /// </summary>
    private StyleCopSettingsHandler settingsHandler;

    #endregion Private Fields

    #region Protected Properties

    /// <summary>
    /// Gets the settings handler object.
    /// </summary>
    /// <value>The settings handler object.</value>
    protected StyleCopSettingsHandler SettingsHandler
    {
      get
      {
        return this.settingsHandler;
      }
    }

    #endregion Protected Properties

    #region Public Override Methods

    /// <summary>
    /// Initializes the OptionsPanel.
    /// </summary>
    /// <param name="dialog">Parent dialog.</param>
    /// <param name="dataObject">Data object (should be the project in our case).</param>
    public override void Initialize(OptionsDialog dialog, object dataObject)
    {
      base.Initialize(dialog, dataObject);

      this.settingsHandler = dataObject as StyleCopSettingsHandler;
      if (this.settingsHandler == null)
      {
        throw new InvalidCastException("dataObject is not of type StyleCopSettingsHandler");
      }
    }

    #endregion Public Override Methods
  }
}