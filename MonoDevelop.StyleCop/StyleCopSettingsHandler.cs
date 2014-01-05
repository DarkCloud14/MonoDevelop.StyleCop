//-----------------------------------------------------------------------
// <copyright file="StyleCopSettingsHandler.cs">
//   APL 2.0
// </copyright>
// <license>
//   Copyright 2014 Alexander Jochum
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
  using System.Globalization;
  using System.Reflection;
  using global::StyleCop;

  /// <summary>
  /// This class handles the given StyleCop settings file.
  /// </summary>
  internal sealed class StyleCopSettingsHandler
  {
    #region Private Fields

    /// <summary>
    /// The StyleCop core instance.
    /// </summary>
    private StyleCopCore core;

    /// <summary>
    /// The settings file to read from and write to.
    /// </summary>
    private WritableSettings localSettings;

    /// <summary>
    /// Local settings lock object.
    /// </summary>
    private object localSettingsLockObject = new object();

    /// <summary>
    /// The local settings merged with parent settings.
    /// </summary>
    private Settings mergedSettings;

    /// <summary>
    /// The settings one level up from the local settings file.
    /// </summary>
    private Settings parentSettings;

    /// <summary>
    /// Compares the local settings with the merged settings.
    /// </summary>
    private SettingsComparer settingsComparer;

    #endregion Private Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="StyleCopSettingsHandler"/> class.
    /// </summary>
    /// <param name="settingsFilePath">The path to the StyleCop settings file.</param>
    /// <param name="coreInstance">The StyleCop core instance.</param>
    internal StyleCopSettingsHandler(string settingsFilePath, StyleCopCore coreInstance)
    {
      Param.AssertValidString(settingsFilePath, "settingsFilePath");
      Param.AssertNotNull(coreInstance, "coreInstance");

      this.core = coreInstance;

      // Load the local settings.
      Exception exception = null;
      this.localSettings = this.core.Environment.GetWritableSettings(settingsFilePath, out exception);

      if (exception != null)
      {
        Gtk.MessageDialog messageDialog = new Gtk.MessageDialog(
          null,
          Gtk.DialogFlags.Modal,
          Gtk.MessageType.Error,
          Gtk.ButtonsType.Ok,
          string.Format(CultureInfo.CurrentUICulture, StaticStringResources.ProjectSettingsFileNotLoadedOrCreated, exception.Message));

        messageDialog.Title = StaticStringResources.Title;
        messageDialog.Run();
        messageDialog.Destroy();

        throw exception;
      }
      else if (this.localSettings != null)
      {
        if (string.Compare(settingsFilePath, this.core.Environment.GetDefaultSettingsPath(), true) == 0)
        {
          // We must use reflection at the moment to set the DefaultSettings property (in case it exists).
          PropertyInfo prop = this.localSettings.GetType().GetProperty("DefaultSettings", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
          if (prop != null && prop.CanWrite)
          {
            prop.SetValue(this.localSettings, true, null);
          }
        }

        // Set the contents of the parent settings file.
        SettingsMerger merger = new SettingsMerger(this.localSettings, this.core.Environment);
        this.parentSettings = merger.ParentMergedSettings;
        this.mergedSettings = merger.MergedSettings;

        // Set up the settings comparer.
        this.settingsComparer = new SettingsComparer(this.localSettings, this.parentSettings);
      }
      else
      {
        throw new InvalidOperationException("StyleCop settings couldn't be loaded!");
      }
    }

    #endregion Constructor

    #region Internal Properties

    /// <summary>
    /// Gets the StyleCop core instance.
    /// </summary>
    internal StyleCopCore Core
    {
      get
      {
        return this.core;
      }
    }

    /// <summary>
    /// Gets the local settings file to read from and write to.
    /// </summary>
    internal WritableSettings LocalSettings
    {
      get
      {
        return this.localSettings;
      }
    }

    /// <summary>
    /// Gets the local settings merged with all parent settings.
    /// </summary>
    internal Settings MergedSettings
    {
      get
      {
        return this.mergedSettings;
      }
    }

    /// <summary>
    /// Gets the settings which the local settings are merged with at runtime, or null if there are
    /// no settings to merge.
    /// </summary>
    internal Settings ParentSettings
    {
      get
      {
        return this.parentSettings;
      }
    }

    /// <summary>
    /// Gets a comparer that can be used to determine whether local settings are overriding parent settings.
    /// </summary>
    internal SettingsComparer SettingsComparer
    {
      get
      {
        return this.settingsComparer;
      }
    }

    #endregion Internal Properties
  }
}