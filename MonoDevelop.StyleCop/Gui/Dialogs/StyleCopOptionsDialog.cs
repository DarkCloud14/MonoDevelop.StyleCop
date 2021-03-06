﻿//-----------------------------------------------------------------------
// <copyright file="StyleCopOptionsDialog.cs">
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
namespace MonoDevelop.StyleCop.Gui.Dialogs
{
  using System;
  using MonoDevelop.Components;
  using MonoDevelop.Ide.Gui.Dialogs;
  using MonoDevelop.StyleCop.Gui.OptionPanels;

  /// <summary>
  /// StyleCop options dialog.
  /// </summary>
  [CLSCompliant(false)]
  public class StyleCopOptionsDialog : OptionsDialog
  {
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="T:MonoDevelop.StyleCop.Gui.Dialogs.StyleCopOptionsDialog"/> class.
    /// </summary>
    /// <param name="parentWindow">Parent window.</param>
    /// <param name="dataObject">Data object.</param>
    public StyleCopOptionsDialog(Window parentWindow, object dataObject)
      : base(parentWindow, dataObject, "/MonoDevelop/Gui/StyleCopOptionPanels")
    {
      StyleCopSettingsHandler settingsHandler = dataObject as StyleCopSettingsHandler;
      if (settingsHandler != null)
      {
        string styleCopTitleText = settingsHandler.LocalSettingsAreDefaultSettings ? "StyleCop {0} Default Settings" : "StyleCop {0} Project Settings";
        styleCopTitleText = string.Format(styleCopTitleText, StyleCopVersion.VersionNumberMajorMinor + " (" + StyleCopVersion.VersionNumberFull + ")");

        // Set the dialog title.
        this.Title = string.Format("{0} - {1}", styleCopTitleText, settingsHandler.LocalSettings.Location);
      }
    }

    #endregion Constructor

    #region Internal Methods

    /// <summary>
    /// Applies the setting file changes.
    /// </summary>
    internal void ApplySettingFileChanges()
    {
      StyleCopSettingsHandler settingsHandler = this.DataObject as StyleCopSettingsHandler;
      if (settingsHandler != null)
      {
        settingsHandler.RefreshMergedSettings();

        var rulesOptionsPanel = this.GetPanel<RulesOptionsPanel>("RulesOptionsPanel");
        rulesOptionsPanel.RefreshMergedSettingsOverrideState();

        var companyInformationOptionsPanel = this.GetPanel<CompanyInformationOptionsPanel>("CompanyInformationOptionsPanel");
        companyInformationOptionsPanel.RefreshMergedSettingsOverrideState();

        var spellingOptionsPanel = this.GetPanel<SpellingOptionsPanel>("SpellingOptionsPanel");
        spellingOptionsPanel.RefreshMergedSettingsOverrideState();

        var validPrefixesOptionsPanel = this.GetPanel<ValidPrefixesOptionsPanel>("ValidPrefixesOptionsPanel");
        validPrefixesOptionsPanel.RefreshMergedSettingsOverrideState();

        var generalOptionsPanel = this.GetPanel<GeneralOptionsPanel>("GeneralOptionsPanel");
        generalOptionsPanel.RefreshMergedSettingsOverrideState();
      }
    }

    #endregion Internal Methods
  }
}