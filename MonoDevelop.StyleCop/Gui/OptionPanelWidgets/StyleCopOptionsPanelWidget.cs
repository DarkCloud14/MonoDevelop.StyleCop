//-----------------------------------------------------------------------
// <copyright file="StyleCopOptionsPanelWidget.cs">
//   APL 2.0
// </copyright>
// <license>
//   Copyright 2017, 2018 Alexander Jochum
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
namespace MonoDevelop.StyleCop.Gui.OptionPanelWidgets
{
  using System;

  /// <summary>
  /// StyleCop options panel widget base class.
  /// </summary>
  [CLSCompliant(false)]
  public abstract class StyleCopOptionsPanelWidget : Gtk.Bin
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

    #region Public Abstract Methods

    /// <summary>
    /// Applies the changes.
    /// </summary>
    public abstract void ApplyChanges();

    #endregion Public Abstract Methods

    #region Public Virtual Methods

    /// <summary>
    /// Initializes the options panel widget values.
    /// </summary>
    /// <param name="settingsHandler">Settings handler object.</param>
    public virtual void Initialize(StyleCopSettingsHandler settingsHandler)
    {
      this.settingsHandler = settingsHandler;
      if (this.settingsHandler == null)
      {
        throw new InvalidCastException("settingsHandler cannot be null");
      }
    }

    /// <summary>
    /// Refreshes the merged override state of properties on the page widget.
    /// </summary>
    public virtual void RefreshMergedSettingsOverrideState()
    {
    }

    /// <summary>
    /// Validates the changes.
    /// </summary>
    /// <returns><c>true</c>, if changes was validated, <c>false</c> otherwise.</returns>
    public virtual bool ValidateChanges()
    {
      return true;
    }

    #endregion Public Virtual Methods
  }
}