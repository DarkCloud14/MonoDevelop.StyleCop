//-----------------------------------------------------------------------
// <copyright file="CompanyInformationOptionsPanel.cs">
//   APL 2.0
// </copyright>
// <license>
//   Copyright 2013-2016 Alexander Jochum
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
  using System;
  using System.Reflection;
  using MonoDevelop.Components;
  using global::StyleCop;
  using global::StyleCop.CSharp;

  /// <summary>
  /// StyleCop company information options panel.
  /// </summary>
  internal partial class CompanyInformationOptionsPanel : ProjectOptionsPanelBase
  {
    #region Private Fields

    /// <summary>
    /// The analyzer that this settings page is attached to.
    /// </summary>
    private readonly SourceAnalyzer analyzer;

    /// <summary>
    /// The name of the property contains the company name.
    /// </summary>
    private string companyNameProperty = "CompanyName";

    /// <summary>
    /// The name of the property which contains the copyright.
    /// </summary>
    private string copyrightProperty = "Copyright";

    #endregion Private Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="MonoDevelop.StyleCop.CompanyInformationOptionsPanel"/> class.
    /// </summary>
    public CompanyInformationOptionsPanel()
    {
      this.Build();
      this.analyzer = ProjectUtilities.Instance.Core.GetAnalyzer("StyleCop.CSharp.DocumentationRules");
    }

    #endregion Constructor

    #region Public Override Methods

    /// <summary>
    /// Initializes the options panel values just before the panel is shown to user for the first time.
    /// </summary>
    /// <returns>The options panel widget.</returns>
    /// <remarks>Will only be called if the user really gets to see the options panel.</remarks>
    public override Control CreatePanelWidget()
    {
      if (this.analyzer != null)
      {
        // We use reflection and try to get the DocumentationRules CompanyNameProperty and CopyrightProperty values.
        FieldInfo field = typeof(DocumentationRules).GetField("CompanyNameProperty", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (field != null)
        {
          this.companyNameProperty = field.GetValue(null) as string;
        }

        field = typeof(DocumentationRules).GetField("CopyrightProperty", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (field != null)
        {
          this.copyrightProperty = field.GetValue(null) as string;
        }

        // Get the properties.
        StringProperty companyNameProperty = this.analyzer.GetSetting(this.SettingsHandler.MergedSettings, this.companyNameProperty) as StringProperty;

        if (companyNameProperty != null)
        {
          this.companyNameEntry.Text = companyNameProperty.Value;
        }

        StringProperty copyrightProperty = this.analyzer.GetSetting(this.SettingsHandler.MergedSettings, this.copyrightProperty) as StringProperty;

        if (copyrightProperty != null)
        {
          this.copyrightTextView.Buffer.Text = copyrightProperty.Value;
        }

        this.checkBox.Active = companyNameProperty != null || copyrightProperty != null;
        this.CheckBoxToggled(this.checkBox, EventArgs.Empty);
      }

      return base.CreatePanelWidget();
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