//-----------------------------------------------------------------------
// <copyright file="ProjectOptionsPanelBase.cs">
//   APL 2.0
// </copyright>
// <license>
//   Copyright 2013-2014 Alexander Jochum
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
  using Gtk;
  using MonoDevelop.Ide;
  using MonoDevelop.Ide.Gui.Dialogs;
  using MonoDevelop.Projects;
  using global::StyleCop;

  /// <summary>
  /// StyleCop Project options panel base class.
  /// </summary>
  [System.ComponentModel.ToolboxItem(false)]
  internal abstract class ProjectOptionsPanelBase : Bin, IOptionsPanel
  {
    #region Private Static Fields

    /// <summary>
    /// Settings handler to get and set StyleCop settings values.
    /// </summary>
    private static StyleCopSettingsHandler settingsHandler;

    #endregion Private Static Fields

    #region Private Fields

    /// <summary>
    /// The parent project.
    /// </summary>
    private Project parentProject;

    /// <summary>
    /// The options panel visibility.
    /// </summary>
    private bool optionsPanelVisible;

    #endregion Private Fields

    #region Protected Properties

    /// <summary>
    /// Gets the StyleCop settings handler object.
    /// </summary>
    protected StyleCopSettingsHandler SettingsHandler
    {
      get { return settingsHandler; }
    }

    #endregion Protected Properties

    #region IOptionsPanel implementation

    /// <summary>
    /// Applies the changes.
    /// </summary>
    public virtual void ApplyChanges()
    {
    }

    /// <summary>
    /// Creates the options panel widget.
    /// </summary>
    /// <returns>The options panel widget.</returns>
    public virtual Widget CreatePanelWidget()
    {
      return this;
    }

    /// <summary>
    /// Initializes the OptionsPanel.
    /// </summary>
    /// <param name="dialog">Parent dialog.</param>
    /// <param name="dataObject">Data object (should be the project in our case).</param>
    public virtual void Initialize(OptionsDialog dialog, object dataObject)
    {
      this.optionsPanelVisible = false;
      this.parentProject = dataObject as Project;

      // If for some reason the dataObject isn't our project get it over the selection in case that isn't null.
      if (this.parentProject == null && IdeApp.ProjectOperations.CurrentSelectedProject != null)
      {
        this.parentProject = IdeApp.ProjectOperations.CurrentSelectedProject;
      }

      this.optionsPanelVisible = ProjectUtilities.Instance.IsKnownProjectType(this.parentProject);

      if (this.optionsPanelVisible)
      {
        string localSettingsFileFolder = this.parentProject.BaseDirectory;
        string settingsFilePath = System.IO.Path.Combine(localSettingsFileFolder, global::StyleCop.Settings.DefaultFileName);

        if (!System.IO.File.Exists(settingsFilePath))
        {
          string deprecatedSettingsFile = System.IO.Path.Combine(localSettingsFileFolder, global::StyleCop.Settings.AlternateFileName);
          if (System.IO.File.Exists(deprecatedSettingsFile))
          {
            settingsFilePath = deprecatedSettingsFile;
          }
          else
          {
            deprecatedSettingsFile = System.IO.Path.Combine(localSettingsFileFolder, V101Settings.DefaultFileName);
            if (System.IO.File.Exists(deprecatedSettingsFile))
            {
              settingsFilePath = deprecatedSettingsFile;
            }
          }
        }

        if (settingsHandler == null)
        {
          try
          {
            settingsHandler = new StyleCopSettingsHandler(settingsFilePath, ProjectUtilities.Instance.Core);
          }
          catch
          {
            this.optionsPanelVisible = false;
          }
        }
      }
    }

    /// <summary>
    /// Determines whether this instance is visible.
    /// </summary>
    /// <returns><c>true</c> if this instance is visible; otherwise, <c>false</c>.</returns>
    public virtual bool IsVisible()
    {
      return this.optionsPanelVisible;
    }

    /// <summary>
    /// Validates the changes.
    /// </summary>
    /// <returns><c>true</c>, if changes was validated, <c>false</c> otherwise.</returns>
    public virtual bool ValidateChanges()
    {
      return true;
    }

    #endregion
  }
}