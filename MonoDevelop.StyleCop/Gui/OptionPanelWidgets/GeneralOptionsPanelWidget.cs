﻿//-----------------------------------------------------------------------
// <copyright file="GeneralOptionsPanelWidget.cs">
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
namespace MonoDevelop.StyleCop.Gui.OptionPanelWidgets
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using System.IO;
  using System.Reflection;
  using MonoDevelop.Ide.Fonts;
  using global::StyleCop;

  /// <summary>
  /// StyleCop general options panel widget.
  /// </summary>
  public partial class GeneralOptionsPanelWidget : StyleCopOptionsPanelWidget
  {
    #region Private Fields

    /// <summary>
    /// Contains the culture combo box values.
    /// </summary>
    private List<string> cultureComboBoxValues;

    /// <summary>
    /// The global value of the property.
    /// </summary>
    private StringProperty cultureParentProperty;

    /// <summary>
    /// Property culturePropertyDescriptor.
    /// </summary>
    private PropertyDescriptor<string> culturePropertyDescriptor;

    /// <summary>
    /// The global value of the property.
    /// </summary>
    private IntProperty maxViolationCountParentProperty;

    /// <summary>
    /// Property maxViolationCountPropertyDescriptor.
    /// </summary>
    private PropertyDescriptor<int> maxViolationCountPropertyDescriptor;

    /// <summary>
    /// The global value of the property.
    /// </summary>
    private BooleanProperty violationsAsErrorsParentProperty;

    /// <summary>
    /// Property violationsAsErrorsPropertyDescriptor.
    /// </summary>
    private PropertyDescriptor<bool> violationsAsErrorsPropertyDescriptor;

    /// <summary>
    /// The global value of the property.
    /// </summary>
    private BooleanProperty writeCacheParentProperty;

    /// <summary>
    /// Property writeCachePropertyDescriptor.
    /// </summary>
    private PropertyDescriptor<bool> writeCachePropertyDescriptor;

    #endregion Private Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="MonoDevelop.StyleCop.Gui.OptionPanelWidgets.GeneralOptionsPanelWidget"/> class.
    /// </summary>
    public GeneralOptionsPanelWidget()
    {
      this.Build();
      this.cultureComboBoxValues = new List<string>();
      this.cultureComboBoxValues.Add("en-US");
      this.cultureComboBox.AppendText("en-US");

      List<CultureInfo> cultures = new List<CultureInfo>(EnumStyleCopCultureInfos());
      foreach (CultureInfo cultureInfo in cultures)
      {
        this.cultureComboBoxValues.Add(cultureInfo.IetfLanguageTag);
        this.cultureComboBox.AppendText(cultureInfo.IetfLanguageTag);
      }
    }

    #endregion Constructor

    #region Public Override Methods

    /// <summary>
    /// Applies the changes.
    /// </summary>
    public override void ApplyChanges()
    {
      this.SettingsHandler.LocalSettings.GlobalSettings.SetProperty(new BooleanProperty(this.SettingsHandler.Core, this.writeCachePropertyDescriptor.PropertyName, this.enableCacheCheckBox.Active));
      this.SettingsHandler.LocalSettings.GlobalSettings.SetProperty(new BooleanProperty(this.SettingsHandler.Core, this.violationsAsErrorsPropertyDescriptor.PropertyName, this.violationsAsErrorsCheckBox.Active));
      this.SettingsHandler.LocalSettings.GlobalSettings.SetProperty(new IntProperty(this.SettingsHandler.Core, this.maxViolationCountPropertyDescriptor.PropertyName, this.maxViolationCountSpinButton.ValueAsInt));
      this.SettingsHandler.LocalSettings.GlobalSettings.SetProperty(new StringProperty(this.SettingsHandler.Core, this.culturePropertyDescriptor.PropertyName, this.cultureComboBox.ActiveText));
    }

    /// <summary>
    /// Initializes the options panel widget values.
    /// </summary>
    /// <param name="settingsHandler">Settings handler object.</param>
    public override void Initialize(StyleCopSettingsHandler settingsHandler)
    {
      base.Initialize(settingsHandler);

      // Get the write cache setting.
      this.writeCachePropertyDescriptor = this.SettingsHandler.Core.PropertyDescriptors["WriteCache"] as PropertyDescriptor<bool>;

      this.writeCacheParentProperty = this.SettingsHandler.ParentSettings == null
                                        ? null
                                        : this.SettingsHandler.ParentSettings.GlobalSettings.GetProperty(this.writeCachePropertyDescriptor.PropertyName) as BooleanProperty;

      BooleanProperty mergedWriteCacheProperty = this.SettingsHandler.MergedSettings == null
                                                  ? null
                                                  : this.SettingsHandler.MergedSettings.GlobalSettings.GetProperty(this.writeCachePropertyDescriptor.PropertyName) as BooleanProperty;

      this.enableCacheCheckBox.Active = mergedWriteCacheProperty == null ? this.writeCachePropertyDescriptor.DefaultValue : mergedWriteCacheProperty.Value;

      // Errors As Warnings
      this.violationsAsErrorsPropertyDescriptor = this.SettingsHandler.Core.PropertyDescriptors["ViolationsAsErrors"] as PropertyDescriptor<bool>;

      this.violationsAsErrorsParentProperty = this.SettingsHandler.ParentSettings == null
                                          ? null
                                          : this.SettingsHandler.ParentSettings.GlobalSettings.GetProperty(this.violationsAsErrorsPropertyDescriptor.PropertyName) as BooleanProperty;

      BooleanProperty mergedViolationsAsErrorsProperty = this.SettingsHandler.MergedSettings == null
                                                          ? null
                                                          : this.SettingsHandler.MergedSettings.GlobalSettings.GetProperty(
                                                           this.violationsAsErrorsPropertyDescriptor.PropertyName) as BooleanProperty;

      this.violationsAsErrorsCheckBox.Active = mergedViolationsAsErrorsProperty == null
                                                    ? this.violationsAsErrorsPropertyDescriptor.DefaultValue
                                                    : mergedViolationsAsErrorsProperty.Value;

      // Get the max violation count setting
      this.maxViolationCountPropertyDescriptor = this.SettingsHandler.Core.PropertyDescriptors["MaxViolationCount"] as PropertyDescriptor<int>;

      this.maxViolationCountParentProperty = this.SettingsHandler.ParentSettings == null
                                              ? null
                                              : this.SettingsHandler.ParentSettings.GlobalSettings.GetProperty(this.maxViolationCountPropertyDescriptor.PropertyName) as IntProperty;

      IntProperty mergedMaxViolationCountProperty = this.SettingsHandler.MergedSettings == null
                                                      ? null
                                                      : this.SettingsHandler.MergedSettings.GlobalSettings.GetProperty(
                                                      this.maxViolationCountPropertyDescriptor.PropertyName) as IntProperty;

      this.maxViolationCountSpinButton.Value = Convert.ToDouble(mergedMaxViolationCountProperty == null
                                                ? this.maxViolationCountPropertyDescriptor.DefaultValue.ToString(CultureInfo.InvariantCulture)
                                                : mergedMaxViolationCountProperty.Value.ToString(CultureInfo.InvariantCulture));

      // Get the culture setting
      this.culturePropertyDescriptor = this.SettingsHandler.Core.PropertyDescriptors["Culture"] as PropertyDescriptor<string>;

      this.cultureParentProperty = this.SettingsHandler.ParentSettings == null
                                    ? null
                                    : this.SettingsHandler.ParentSettings.GlobalSettings.GetProperty(this.culturePropertyDescriptor.PropertyName) as StringProperty;

      StringProperty mergedCultureProperty = this.SettingsHandler.MergedSettings == null
                                              ? null
                                              : this.SettingsHandler.MergedSettings.GlobalSettings.GetProperty(this.culturePropertyDescriptor.PropertyName) as StringProperty;

      int cultureComboBoxIndex = this.cultureComboBoxValues.IndexOf(mergedCultureProperty == null
                                  ? this.culturePropertyDescriptor.DefaultValue
                                  : mergedCultureProperty.Value);
      
      Gtk.TreeIter cultureIter;
      this.cultureComboBox.Model.IterNthChild(out cultureIter, cultureComboBoxIndex);
      this.cultureComboBox.SetActiveIter(cultureIter);
    }

    /// <summary>
    /// Refreshes the merged override state of properties on the panel widget.
    /// </summary>
    public override void RefreshMergedSettingsOverrideState()
    {
      this.writeCacheParentProperty = this.SettingsHandler.ParentSettings == null
                                       ? null
                                       : this.SettingsHandler.ParentSettings.GlobalSettings.GetProperty(this.writeCachePropertyDescriptor.PropertyName) as BooleanProperty;

      this.maxViolationCountParentProperty = this.SettingsHandler.ParentSettings == null
                                              ? null
                                              : this.SettingsHandler.ParentSettings.GlobalSettings.GetProperty(this.maxViolationCountPropertyDescriptor.PropertyName) as IntProperty;

      this.cultureParentProperty = this.SettingsHandler.ParentSettings == null
                                    ? null
                                    : this.SettingsHandler.ParentSettings.GlobalSettings.GetProperty(this.culturePropertyDescriptor.PropertyName) as StringProperty;

      this.violationsAsErrorsParentProperty = this.SettingsHandler.ParentSettings == null
                                               ? null
                                               : this.SettingsHandler.ParentSettings.GlobalSettings.GetProperty(this.violationsAsErrorsPropertyDescriptor.PropertyName) as BooleanProperty;
      
      this.SetBoldState();
    }

    #endregion Public Override Methods

    #region Protected Signal Methods

    /// <summary>
    /// Called when the cultureComboBox value changed.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void CultureComboBoxChanged(object sender, EventArgs e)
    {
      this.SetBoldState();
    }

    /// <summary>
    /// Called when the enableCacheCheckBox is checked or unchecked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void EnableCacheCheckBoxToggled(object sender, EventArgs e)
    {
      this.SetBoldState();
    }

    /// <summary>
    /// Called when the maxViolationCountSpinButton value changed.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void MaxViolationCountSpinButtonValueChanged(object sender, EventArgs e)
    {
      this.SetBoldState();
    }

    /// <summary>
    /// Called when the violationsAsErrorsCheckBox is checked or unchecked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void ViolationsAsErrorsCheckBoxToggled(object sender, EventArgs e)
    {
      this.SetBoldState();
    }

    #endregion Protected Signal Methods

    #region Private Static Methods

    /// <summary>
    /// Enumerates StyleCop culture information available in sub directories of the executing assembly.
    /// </summary>
    /// <returns>All available and valid StyleCop culture information.</returns>
    private static IEnumerable<CultureInfo> EnumStyleCopCultureInfos()
    {
      string directoryName = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

      foreach (string directory in Directory.GetDirectories(directoryName))
      {
        string name = System.IO.Path.GetFileNameWithoutExtension(directory);

        // Culture format is XX-YY, we discard directories that can't match.
        if (name.Length > 5)
        {
          continue;
        }

        CultureInfo culture = null;
        try
        {
          culture = CultureInfo.GetCultureInfo(name);
        }
        catch (ArgumentNullException)
        {
          continue;
        }
        catch (ArgumentException)
        {
          continue;
        }

        string resName = "StyleCop.CSharp.Rules.resources.dll";
        if (File.Exists(System.IO.Path.Combine(System.IO.Path.Combine(directoryName, name), resName)))
        {
          yield return culture;
        }
      }
    }

    #endregion Private Static Methods

    #region Private Methods

    /// <summary>
    /// Sets the bold state of the controls.
    /// </summary>
    private void SetBoldState()
    {
      bool bold;

      if (this.culturePropertyDescriptor != null)
      {
        if (this.cultureParentProperty == null)
        {
          bold = this.cultureComboBox.ActiveText != this.culturePropertyDescriptor.DefaultValue.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
          bold = this.cultureComboBox.ActiveText != this.cultureParentProperty.Value.ToString(CultureInfo.InvariantCulture);
        }

        var fontDescription = Ide.Gui.Styles.DefaultFont.CopyModified(weight: bold ? Pango.Weight.Heavy : Pango.Weight.Normal);
        this.cultureComboBox.Child.ModifyFont(fontDescription);
      }

      if (this.maxViolationCountPropertyDescriptor != null)
      {
        bold = this.maxViolationCountParentProperty == null
                   ? this.maxViolationCountSpinButton.Text != this.maxViolationCountPropertyDescriptor.DefaultValue.ToString(CultureInfo.InvariantCulture)
                   : this.maxViolationCountSpinButton.Text != this.maxViolationCountParentProperty.Value.ToString(CultureInfo.InvariantCulture);

        var fontDescription = Ide.Gui.Styles.DefaultFont.CopyModified(weight: bold ? Pango.Weight.Heavy : Pango.Weight.Normal);
        this.maxViolationCountSpinButton.ModifyFont(fontDescription);
      }

      if (this.violationsAsErrorsPropertyDescriptor != null)
      {
        bold = this.violationsAsErrorsParentProperty == null
                   ? this.violationsAsErrorsCheckBox.Active != this.violationsAsErrorsPropertyDescriptor.DefaultValue
                   : this.violationsAsErrorsCheckBox.Active != this.violationsAsErrorsParentProperty.Value;

        var fontDescription = Ide.Gui.Styles.DefaultFont.CopyModified(weight: bold ? Pango.Weight.Heavy : Pango.Weight.Normal);
        this.violationsAsErrorsCheckBox.Child.ModifyFont(fontDescription);
      }

      if (this.writeCachePropertyDescriptor != null)
      {
        bold = this.writeCacheParentProperty == null
                   ? this.enableCacheCheckBox.Active != this.writeCachePropertyDescriptor.DefaultValue
                   : this.enableCacheCheckBox.Active != this.writeCacheParentProperty.Value;

        var fontDescription = Ide.Gui.Styles.DefaultFont.CopyModified(weight: bold ? Pango.Weight.Heavy : Pango.Weight.Normal);
        this.enableCacheCheckBox.Child.ModifyFont(fontDescription);
      }
    }

    #endregion Private Methods
  }
}