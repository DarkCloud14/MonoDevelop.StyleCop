//-----------------------------------------------------------------------
// <copyright file="CompanyInformationOptionsPanelWidget.cs">
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
  using MonoDevelop.Ide;
  using global::StyleCop;

  /// <summary>
  /// StyleCop company information options panel widget.
  /// </summary>
  public partial class CompanyInformationOptionsPanelWidget : StyleCopOptionsPanelWidget
  {
    #region Constants

    /// <summary>
    /// The name of the property contains the company name.
    /// </summary>
    private const string CompanyNameProperty = "CompanyName";

    /// <summary>
    /// The name of the property which contains the copyright.
    /// </summary>
    private const string CopyrightProperty = "Copyright";

    #endregion Constants

    #region Private Fields

    /// <summary>
    /// The analyzer that this settings page is attached to.
    /// </summary>
    private readonly SourceAnalyzer analyzer;

    #endregion Private Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="MonoDevelop.StyleCop.Gui.OptionPanelWidgets.CompanyInformationOptionsPanelWidget"/> class.
    /// </summary>
    public CompanyInformationOptionsPanelWidget()
    {
      this.Build();
      this.analyzer = ProjectUtilities.Instance.Core.GetAnalyzer("StyleCop.CSharp.DocumentationRules");
    }

    #endregion Constructor

    #region Public Override Methods

    /// <summary>
    /// Applies the changes.
    /// </summary>
    public override void ApplyChanges()
    {
      if (this.analyzer != null)
      {
        if (!this.checkBox.Active)
        {
          this.analyzer.ClearSetting(this.SettingsHandler.LocalSettings, CompanyNameProperty);
          this.analyzer.ClearSetting(this.SettingsHandler.LocalSettings, CopyrightProperty);
        }
        else
        {
          if (this.companyNameEntry.Text.Length > 0 && this.copyrightTextView.Buffer.CharCount > 0)
          {
            this.analyzer.SetSetting(this.SettingsHandler.LocalSettings, new StringProperty(this.analyzer, CompanyNameProperty, this.companyNameEntry.Text));
            this.analyzer.SetSetting(this.SettingsHandler.LocalSettings, new StringProperty(this.analyzer, CopyrightProperty, this.copyrightTextView.Buffer.Text));
          }
        }
      }
    }

    /// <summary>
    /// Initializes the options panel widget values.
    /// </summary>
    /// <param name="settingsHandler">Settings handler object.</param>
    public override void Initialize(StyleCopSettingsHandler settingsHandler)
    {
      base.Initialize(settingsHandler);

      if (this.analyzer != null)
      {
        // Get the properties.
        StringProperty companyNameProperty = this.analyzer.GetSetting(this.SettingsHandler.MergedSettings, CompanyNameProperty) as StringProperty;

        if (companyNameProperty != null)
        {
          this.companyNameEntry.Text = companyNameProperty.Value;
        }

        StringProperty copyrightProperty = this.analyzer.GetSetting(this.SettingsHandler.MergedSettings, CopyrightProperty) as StringProperty;

        if (copyrightProperty != null)
        {
          this.copyrightTextView.Buffer.Text = copyrightProperty.Value;
        }

        this.checkBox.Active = companyNameProperty != null || copyrightProperty != null;
        this.CheckBoxToggled(this.checkBox, EventArgs.Empty);
      }
    }

    /// <summary>
    /// Validates the changes.
    /// </summary>
    /// <returns><c>true</c>, if changes was validated, <c>false</c> otherwise.</returns>
    public override bool ValidateChanges()
    {
      if (this.checkBox.Active)
      {
        if (this.companyNameEntry.Text.Length == 0 || this.copyrightTextView.Buffer.CharCount == 0)
        {
          MessageService.ShowError("The company name and copyright boxes must be filled in.");
          return false;
        }
      }

      return true;
    }

    #endregion Public Override Methods

    #region Protected Signal Methods

    /// <summary>
    /// Called when the checkbox is checked or unchecked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void CheckBoxToggled(object sender, EventArgs e)
    {
      this.companyNameEntry.Sensitive = this.checkBox.Active;
      this.copyrightTextView.Sensitive = this.checkBox.Active;
    }

    #endregion Protected Signal Methods
  }
}