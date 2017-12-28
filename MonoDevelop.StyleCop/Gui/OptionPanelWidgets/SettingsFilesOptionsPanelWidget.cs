//-----------------------------------------------------------------------
// <copyright file="SettingsFilesOptionsPanelWidget.cs">
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
  using System.Security;
  using System.Xml;
  using MonoDevelop.Components;
  using MonoDevelop.Core;
  using MonoDevelop.Ide;
  using MonoDevelop.StyleCop.Gui.Dialogs;
  using global::StyleCop;

  /// <summary>
  /// StyleCop settings files options panel widget.
  /// </summary>
  public partial class SettingsFilesOptionsPanelWidget : StyleCopOptionsPanelWidget
  {
    #region Constants

    /// <summary>
    /// The edit default settings warning message.
    /// </summary>
    private const string EditDefaultSettingsWarning = "You are about to edit the default settings which are applied to all code on which this installation of StyleCop is run. You may need special permissions to write to the file. Are you sure you want to do this?";

    /// <summary>
    /// The name of the linked settings property.
    /// </summary>
    private const string LinkedSettingsProperty = "LinkedSettingsFile";

    /// <summary>
    /// The name of the merge settings property.
    /// </summary>
    private const string MergeSettingsFilesProperty = "MergeSettingsFiles";

    /// <summary>
    /// Merge with a linked settings file.
    /// </summary>
    private const string MergeStyleLinked = "Linked";

    /// <summary>
    /// Do not merge the settings file.
    /// </summary>
    private const string MergeStyleNone = "NoMerge";

    /// <summary>
    /// Merge with a parent settings file.
    /// </summary>
    private const string MergeStyleParent = "Parent";

    #endregion Constants

    #region Private Fields

    /// <summary>
    /// Indicates whether the disable the linked settings options.
    /// </summary>
    private bool disableLinking;

    #endregion Private Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="MonoDevelop.StyleCop.Gui.OptionPanelWidgets.SettingsFilesOptionsPanelWidget"/> class.
    /// </summary>
    public SettingsFilesOptionsPanelWidget()
    {
      this.Build();
    }

    #endregion Constructor

    #region Public Override Methods

    /// <summary>
    /// Applies the changes.
    /// </summary>
    public override void ApplyChanges()
    {
      // Save the path to the linked settings file if necessary.
      if (this.mergeWithLinkedFileRadioButton.Active)
      {
        bool valid = false;

        // Validate the contents of the global file path textbox.
        try
        {
          // Make sure the file exists.
          if (System.IO.File.Exists(this.linkedFilePathEntry.Text))
          {
            // Make sure the file can be loaded and contains valid Xml.
            XmlDocument document = new XmlDocument();
            document.Load(this.linkedFilePathEntry.Text);

            // Make sure the file contains the correct type of root node.
            if (document.DocumentElement.Name == "StyleCopSettings" || document.DocumentElement.Name == "SourceAnalysisSettings")
            {
              valid = true;
            }
          }
        }
        catch (ArgumentException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (System.IO.IOException)
        {
        }
        catch (XmlException)
        {
        }

        if (valid)
        {
          string relativePath = this.linkedFilePathEntry.Text;
          if (!relativePath.StartsWith(".", StringComparison.Ordinal))
          {
            // Create a URI pointing to the local project folder.
            string localFolderPath = System.IO.Path.GetDirectoryName(this.SettingsHandler.LocalSettings.Location);
            if (!localFolderPath.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
              localFolderPath += System.IO.Path.DirectorySeparatorChar.ToString();
            }

            Uri uri = new Uri(localFolderPath);

            // Create the relative path to the global file folder.
            Uri relative = uri.MakeRelativeUri(new Uri(this.linkedFilePathEntry.Text));
            relativePath = relative.OriginalString;
          }

          this.SettingsHandler.LocalSettings.GlobalSettings.SetProperty(new StringProperty(this.SettingsHandler.Core, LinkedSettingsProperty, relativePath));
          this.SettingsHandler.LocalSettings.GlobalSettings.SetProperty(new StringProperty(this.SettingsHandler.Core, MergeSettingsFilesProperty, MergeStyleLinked));
        }
        else
        {
          MessageService.ShowError("The linked settings file does not exist or cannot be loaded.");
        }
      }
      else
      {
        this.SettingsHandler.LocalSettings.GlobalSettings.SetProperty(new StringProperty(this.SettingsHandler.Core, MergeSettingsFilesProperty, this.noMergeRadioButton.Active ? MergeStyleNone : MergeStyleParent));
        this.SettingsHandler.LocalSettings.GlobalSettings.Remove(LinkedSettingsProperty);
      }
    }

    /// <summary>
    /// Initializes the options panel widget values.
    /// </summary>
    /// <param name="settingsHandler">Settings handler object.</param>
    public override void Initialize(StyleCopSettingsHandler settingsHandler)
    {
      base.Initialize(settingsHandler);

      // Get the merge style setting.
      StringProperty mergeTypeProperty = this.SettingsHandler.LocalSettings.GlobalSettings.GetProperty(MergeSettingsFilesProperty) as StringProperty;
      string mergeType = mergeTypeProperty == null ? MergeStyleParent : mergeTypeProperty.Value;

      // If the merge style is set to link but the current environment doesn't support linking, change it to parent.
      if (!this.SettingsHandler.Core.Environment.SupportsLinkedSettings && string.CompareOrdinal(mergeType, MergeStyleLinked) == 0)
      {
        mergeType = MergeStyleParent;
        this.disableLinking = true;
      }

      if (string.CompareOrdinal(mergeType, MergeStyleNone) == 0)
      {
        this.noMergeRadioButton.Active = true;
      }
      else if (string.CompareOrdinal(mergeType, MergeStyleLinked) == 0)
      {
        this.mergeWithLinkedFileRadioButton.Active = true;

        StringProperty linkedSettingsFileProperty = this.SettingsHandler.LocalSettings.GlobalSettings.GetProperty(LinkedSettingsProperty) as StringProperty;
        if (linkedSettingsFileProperty != null && !string.IsNullOrEmpty(linkedSettingsFileProperty.Value))
        {
          // This mode assumes that StyleCop is running in a file-based environment.
          string linkedSettingsFile = Environment.ExpandEnvironmentVariables(linkedSettingsFileProperty.Value);
          if (linkedSettingsFile.StartsWith(".", StringComparison.Ordinal))
          {
            linkedSettingsFile = Utils.CorrectPathSeparators(linkedSettingsFile);
            linkedSettingsFile = Utils.MakeAbsolutePath(System.IO.Path.GetDirectoryName(this.SettingsHandler.LocalSettings.Location), linkedSettingsFile);
          }

          this.linkedFilePathEntry.Text = linkedSettingsFile;
        }
      }
      else
      {
        this.mergeWithParentsRadioButton.Active = true;
      }

      this.EnableDisable();

      bool defaultSettings = this.SettingsHandler.LocalSettingsAreDefaultSettings;

      // Disable the parent link controls if this is the default settings file.
      if (defaultSettings)
      {
        this.browseButton.Sensitive = false;
        this.editLinkedSettingsFileButton.Sensitive = false;
        this.editParentSettingsFileButton.Sensitive = false;
        this.linkedFilePathEntry.Sensitive = false;
        this.mergeWithParentsRadioButton.Sensitive = false;
        this.mergeWithLinkedFileRadioButton.Sensitive = false;
      }

      if (!this.noMergeRadioButton.Active && defaultSettings)
      {
        this.noMergeRadioButton.Active = true;
      }
    }

    #endregion Public Override Methods

    #region Protected Signal Methods

    #endregion Protected Signal Methods

    /// <summary>
    /// Called when the browseButton is clicked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void BrowseButtonClicked(object sender, EventArgs e)
    {
      Param.Ignore(sender, e);

      var dlg = new SelectFileDialog(GettextCatalog.GetString("Select File"))
      {
        CurrentFolder = string.IsNullOrEmpty(this.linkedFilePathEntry.Text) ? System.IO.Path.GetDirectoryName(this.SettingsHandler.LocalSettings.Location) : System.IO.Path.GetDirectoryName(this.linkedFilePathEntry.Text),
        InitialFileName = string.IsNullOrEmpty(this.linkedFilePathEntry.Text) ? global::StyleCop.Settings.DefaultFileName : System.IO.Path.GetFileName(this.linkedFilePathEntry.Text),
        SelectMultiple = false,
        TransientFor = this.Toplevel as Gtk.Window,
      };

      dlg.AddFilter("Settings Files", "*.StyleCop");
      dlg.AddAllFilesFilter();

      if (string.IsNullOrEmpty(dlg.InitialFileName))
      {
        dlg.InitialFileName = global::StyleCop.Settings.DefaultFileName;
      }

      if (!dlg.Run())
      {
        return;
      }

      linkedFilePathEntry.Text = dlg.SelectedFile;
    }

    /// <summary>
    /// Called when the editLinkedSettingsFileButton is clicked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void EditLinkedSettingsFileButtonClicked(object sender, EventArgs e)
    {
      Param.Ignore(sender, e);

      if (string.IsNullOrEmpty(this.linkedFilePathEntry.Text))
      {
        MessageService.ShowError("The settings file path must not be empty.");
      }
      else
      {
        string expandedPath = Environment.ExpandEnvironmentVariables(this.linkedFilePathEntry.Text);

        if (expandedPath.StartsWith(".", StringComparison.Ordinal) || !expandedPath.Contains(System.IO.Path.DirectorySeparatorChar.ToString()))
        {
          expandedPath = Utils.CorrectPathSeparators(expandedPath);
          expandedPath = Utils.MakeAbsolutePath(System.IO.Path.GetDirectoryName(this.SettingsHandler.LocalSettings.Location), expandedPath);
        }

        // Check if there is a file at the given path. Create the settings file if needed.
        if (!System.IO.File.Exists(expandedPath))
        {
          // Create a new settings file at the given path.
          Exception exception;

          Settings createdSettingsFile = this.SettingsHandler.Core.Environment.GetWritableSettings(expandedPath, out exception);
          if (createdSettingsFile == null)
          {
            MessageService.ShowError(string.Format("The settings file at this path cannot be loaded: {0}", exception == null ? string.Empty : exception.Message));

            expandedPath = null;
          }
        }

        if (!string.IsNullOrEmpty(expandedPath))
        {
          this.EditSettingsFile(expandedPath);
        }
      }
    }

    /// <summary>
    /// Called when the editParentSettingsButton is clicked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void EditParentSettingsFileButtonClicked(object sender, EventArgs e)
    {
      Param.Ignore(sender, e);

      bool defaultSettings = false;
      string settingsFile = this.SettingsHandler.Core.Environment.GetParentSettingsPath(this.SettingsHandler.LocalSettings.Location);
      if (string.IsNullOrEmpty(settingsFile))
      {
        defaultSettings = true;
        settingsFile = this.SettingsHandler.Core.Environment.GetDefaultSettingsPath();
      }

      if (string.IsNullOrEmpty(settingsFile))
      {
        MessageService.ShowMessage("There is no settings file located in a parent folder above the location of this settings file.");
      }
      else
      {
        if (defaultSettings)
        {
          if (!MessageService.Confirm(EditDefaultSettingsWarning, AlertButton.Ok, true))
          {
            return;
          }
        }

        this.EditSettingsFile(settingsFile);
      }
    }

    /// <summary>
    /// Called when the text of the linkedFilePathEntry control was changed.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void LinkedFilePathEntryChanged(object sender, EventArgs e)
    {
      Param.Ignore(sender, e);

      this.EnableDisable();
    }

    /// <summary>
    /// Called when the noMergeRadioButton is clicked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void NoMergeRadioButtonToggled(object sender, EventArgs e)
    {
      Param.Ignore(sender, e);

      this.EnableDisable();
    }

    /// <summary>
    /// Called when the mergeWithLinkedFileRadioButton is clicked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void MergeWithLinkedFileRadioButtonToggled(object sender, EventArgs e)
    {
      Param.Ignore(sender, e);

      this.EnableDisable();
    }

    /// <summary>
    /// Called when the mergeWithParentsRadioButton is clicked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void MergeWithParentsRadioButtonToggled(object sender, EventArgs e)
    {
      Param.Ignore(sender, e);

      this.EnableDisable();
    }

    #region Private Methods

    /// <summary>
    /// Confirms and edits a settings file.
    /// </summary>
    /// <param name="settingsFile">The path to the settings file.</param>
    private void EditSettingsFile(string settingsFile)
    {
      Param.AssertValidString(settingsFile, "settingsFile");

      StyleCopSettingsHandler settingsHandler;
      try
      {
        settingsHandler = new StyleCopSettingsHandler(settingsFile, ProjectUtilities.Instance.Core);
      }
      catch
      {
        return;
      }

      var styleCopOptionsDialog = new StyleCopOptionsDialog(IdeApp.Workbench.RootWindow, settingsHandler);
      try
      {
        if (MessageService.RunCustomDialog(styleCopOptionsDialog) == (int)Gtk.ResponseType.Ok)
        {
          Exception exception = null;
          settingsHandler.Core.Environment.SaveSettings(settingsHandler.LocalSettings, out exception);

          if (exception != null)
          {
            MessageService.ShowError("There was a problem while trying to save the StyleCop settings.", exception);
          }
        }
      }
      finally
      {
        styleCopOptionsDialog.Destroy();
        styleCopOptionsDialog.Dispose();
      }
    }

    /// <summary>
    /// Enables or disables the file controls.
    /// </summary>
    private void EnableDisable()
    {
      this.browseButton.Sensitive = this.mergeWithLinkedFileRadioButton.Active && !this.disableLinking;
      this.editLinkedSettingsFileButton.Sensitive = this.mergeWithLinkedFileRadioButton.Active && this.linkedFilePathEntry.Text.Length > 0 && !this.disableLinking;
      this.editParentSettingsFileButton.Sensitive = this.mergeWithParentsRadioButton.Active;
      this.linkedFilePathEntry.Sensitive = this.mergeWithLinkedFileRadioButton.Active && !this.disableLinking;

      // If linking is diabled, hide the controls which are related to linking.
      this.browseButton.Visible = !this.disableLinking;
      this.linkedFilePathEntry.Visible = !this.disableLinking;
      this.editLinkedSettingsFileButton.Visible = !this.disableLinking;
    }

    #endregion Private Methods
  }
}