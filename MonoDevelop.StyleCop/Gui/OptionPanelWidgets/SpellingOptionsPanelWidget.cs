//-----------------------------------------------------------------------
// <copyright file="SpellingOptionsPanelWidget.cs">
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
  using System.Collections.Generic;
  using MonoDevelop.Ide;
  using global::StyleCop;

  /// <summary>
  /// StyleCop spelling options panel widget.
  /// </summary>
  [CLSCompliant(false)]
  public partial class SpellingOptionsPanelWidget : StyleCopOptionsPanelWidget
  {
    #region Constants

    /// <summary>
    /// The deprecated words property name.
    /// </summary>
    private const string DeprecatedWordsPropertyName = "DeprecatedWords";

    /// <summary>
    /// The dictionary folders property name.
    /// </summary>
    private const string DictionaryFoldersPropertyName = "DictionaryFolders";

    /// <summary>
    /// The recognized words property name.
    /// </summary>
    private const string RecognizedWordsPropertyName = "RecognizedWords";

    #endregion Constants

    #region Private Fields

    /// <summary>
    /// Stores the deprecated words.
    /// </summary>
    private Gtk.ListStore deprecatedWordsListStore;

    /// <summary>
    /// Stores the custom dictionary folders.
    /// </summary>
    private Gtk.ListStore foldersListStore;

    /// <summary>
    /// List which contains all local deprecated words.
    /// </summary>
    private List<string> localDeprecatedWordsList = new List<string>();

    /// <summary>
    /// List which contains all local folders.
    /// </summary>
    private List<string> localFoldersList = new List<string>();

    /// <summary>
    /// List which contains all local recognized words.
    /// </summary>
    private List<string> localRecognizedWordsList = new List<string>();

    /// <summary>
    /// List which contains all parent deprecated words.
    /// </summary>
    private List<string> parentDeprecatedWordsList = new List<string>();

    /// <summary>
    /// List which contains all parent folders.
    /// </summary>
    private List<string> parentFoldersList = new List<string>();

    /// <summary>
    /// List which contains all parent recognized words.
    /// </summary>
    private List<string> parentRecognizedWordsList = new List<string>();

    /// <summary>
    /// Stores the recognized words.
    /// </summary>
    private Gtk.ListStore recognizedWordsListStore;

    #endregion Private Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="MonoDevelop.StyleCop.Gui.OptionPanelWidgets.SpellingOptionsPanelWidget"/> class.
    /// </summary>
    public SpellingOptionsPanelWidget()
    {
      this.Build();
    }

    #endregion Constructor

    #region Private Enumerators

    /// <summary>
    /// Contains all ListStore columns index numbers
    /// </summary>
    private enum ListStoreColumns : int
    {
      /// <summary>
      /// Column index number of the value item
      /// </summary>
      Value = 0,

      /// <summary>
      /// Column index number of the removable indicator
      /// </summary>
      Removable = 1,

      /// <summary>
      /// Column index number of the weight indicator
      /// </summary>
      Weight = 2,
    }

    #endregion Private Enumerators

    #region Public Override Methods

    /// <summary>
    /// Applies the changes.
    /// </summary>
    public override void ApplyChanges()
    {
      CollectionProperty list = new CollectionProperty(this.SettingsHandler.Core, DeprecatedWordsPropertyName, this.localDeprecatedWordsList);
      this.SettingsHandler.LocalSettings.GlobalSettings.SetProperty(list);

      list = new CollectionProperty(this.SettingsHandler.Core, DictionaryFoldersPropertyName, this.localFoldersList);
      this.SettingsHandler.LocalSettings.GlobalSettings.SetProperty(list);

      list = new CollectionProperty(this.SettingsHandler.Core, RecognizedWordsPropertyName, this.localRecognizedWordsList);
      this.SettingsHandler.LocalSettings.GlobalSettings.SetProperty(list);
    }

    /// <summary>
    /// Initializes the options panel widget values.
    /// </summary>
    /// <param name="settingsHandler">Settings handler object.</param>
    public override void Initialize(StyleCopSettingsHandler settingsHandler)
    {
      base.Initialize(settingsHandler);
      this.InitializeNodeViews();
      this.FillNodeViews();
      this.removeDeprecatedWordButton.Sensitive = false;
      this.removeFolderButton.Sensitive = false;
      this.removeRecognizedWordButton.Sensitive = false;
    }

    /// <summary>
    /// Refreshes the merged override state of properties on the panel widget.
    /// </summary>
    public override void RefreshMergedSettingsOverrideState()
    {
      this.deprecatedWordsListStore.Clear();
      this.foldersListStore.Clear();
      this.recognizedWordsListStore.Clear();

      // Add new parent items
      this.AddParentSpellingValues();

      // Now add all locally added items which aren't in the parent list yet.
      List<string> newLocalDeprecatedWordsList = new List<string>();
      foreach (var currentWord in this.localDeprecatedWordsList)
      {
        if (!this.parentDeprecatedWordsList.Contains(currentWord))
        {
          this.deprecatedWordsListStore.AppendValues(currentWord, true, Pango.Weight.Heavy);
          newLocalDeprecatedWordsList.Add(currentWord);
        }
      }

      this.localDeprecatedWordsList.Clear();
      this.localDeprecatedWordsList.AddRange(newLocalDeprecatedWordsList);

      List<string> newLocalRecognizedWordsList = new List<string>();
      foreach (var currentWord in this.localRecognizedWordsList)
      {
        if (!this.parentRecognizedWordsList.Contains(currentWord))
        {
          this.recognizedWordsListStore.AppendValues(currentWord, true, Pango.Weight.Heavy);
          newLocalRecognizedWordsList.Add(currentWord);
        }
      }

      this.localRecognizedWordsList.Clear();
      this.localRecognizedWordsList.AddRange(newLocalRecognizedWordsList);

      List<string> newLocalFoldersList = new List<string>();
      foreach (var currentFolder in this.localFoldersList)
      {
        if (!this.parentFoldersList.Contains(currentFolder))
        {
          this.foldersListStore.AppendValues(currentFolder, true, Pango.Weight.Heavy);
          newLocalFoldersList.Add(currentFolder);
        }
      }

      this.localFoldersList.Clear();
      this.localFoldersList.AddRange(newLocalFoldersList);
    }

    #endregion Public Override Methods

    #region Protected Signal Methods

    /// <summary>
    /// Called when a key is released in the addAlternateWordEntry control.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void AddAlternateWordEntryReleaseEvent(object sender, Gtk.KeyReleaseEventArgs e)
    {
      Param.AssertNotNull(sender, "sender");
      Param.AssertNotNull(e, "e");

      if (e.Event.Key == Gdk.Key.Return || e.Event.Key == Gdk.Key.KP_Enter)
      {
        if (!string.IsNullOrEmpty(this.addAlternateWordEntry.Text) && !string.IsNullOrEmpty(this.addDeprecatedWordEntry.Text))
        {
          // Simulate a click of the add button.
          this.AddDeprecatedWordButtonClicked(sender, e);
        }
      }
    }

    /// <summary>
    /// Called when the addDeprecatedWordButton is clicked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void AddDeprecatedWordButtonClicked(object sender, EventArgs e)
    {
      Param.Ignore(sender, e);

      string deprecatedWord = this.addDeprecatedWordEntry.Text;
      string alternateWord = this.addAlternateWordEntry.Text;

      if (string.IsNullOrEmpty(deprecatedWord) || deprecatedWord.Length < 2 || deprecatedWord.Contains(" ") ||
          string.IsNullOrEmpty(alternateWord) || alternateWord.Length < 2 || alternateWord.Contains(" "))
      {
        MessageService.ShowError("Please enter a valid deprecated word and it's alternative before clicking the Add button");
        this.addDeprecatedWordEntry.IsFocus = true;
        return;
      }

      string newDeprecatedAndAlternateWord = string.Concat(deprecatedWord.Trim(), ", ", alternateWord.Trim());

      Gtk.TreeIter deprecatedWordsIter;

      // Check if the word combination is already in our list and if so select it instead of adding it.
      if (this.deprecatedWordsListStore.GetIterFirst(out deprecatedWordsIter))
      {
        do
        {
          string currentValue = this.deprecatedWordsListStore.GetValue(deprecatedWordsIter, (int)ListStoreColumns.Value) as string;
          if (!string.IsNullOrEmpty(currentValue) && currentValue == newDeprecatedAndAlternateWord)
          {
            this.deprecatedWordsNodeView.Selection.SelectIter(deprecatedWordsIter);
            this.addDeprecatedWordEntry.Text = string.Empty;
            this.addAlternateWordEntry.Text = string.Empty;
            this.addDeprecatedWordEntry.IsFocus = true;
            return;
          }
        }
        while (this.deprecatedWordsListStore.IterNext(ref deprecatedWordsIter));
      }

      deprecatedWordsIter = this.deprecatedWordsListStore.AppendValues(newDeprecatedAndAlternateWord, true, Pango.Weight.Heavy);
      this.deprecatedWordsNodeView.Selection.SelectIter(deprecatedWordsIter);
      this.addDeprecatedWordEntry.Text = string.Empty;
      this.addAlternateWordEntry.Text = string.Empty;
      this.addDeprecatedWordEntry.IsFocus = true;
      this.localDeprecatedWordsList.Add(newDeprecatedAndAlternateWord);
    }

    /// <summary>
    /// Called when a key is released in the addDeprecatedWordEntry control.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void AddDeprecatedWordEntryReleaseEvent(object sender, Gtk.KeyReleaseEventArgs e)
    {
      Param.AssertNotNull(sender, "sender");
      Param.AssertNotNull(e, "e");

      if (e.Event.Key == Gdk.Key.Return || e.Event.Key == Gdk.Key.KP_Enter)
      {
        if (!string.IsNullOrEmpty(this.addAlternateWordEntry.Text) && !string.IsNullOrEmpty(this.addDeprecatedWordEntry.Text))
        {
          // Simulate a click of the add button.
          this.AddDeprecatedWordButtonClicked(sender, e);
        }
      }
    }

    /// <summary>
    /// Called when the addFolderButton is clicked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void AddFolderButtonClicked(object sender, EventArgs e)
    {
      Param.Ignore(sender, e);

      string folderEntry = this.addFolderEntry.Text;

      if (folderEntry.Length == 0 || folderEntry.Length < 2)
      {
        MessageService.ShowError("Enter a valid folder path before clicking Add");
        this.addFolderEntry.IsFocus = true;
        return;
      }

      Gtk.TreeIter folderIter;

      // Check if the folder is already in our list and if so select it instead of adding it.
      if (this.foldersListStore.GetIterFirst(out folderIter))
      {
        do
        {
          string currentValue = this.foldersListStore.GetValue(folderIter, (int)ListStoreColumns.Value) as string;
          if (!string.IsNullOrEmpty(currentValue) && currentValue == folderEntry)
          {
            this.foldersNodeView.Selection.SelectIter(folderIter);
            this.addFolderEntry.Text = string.Empty;
            this.addFolderEntry.IsFocus = true;
            return;
          }
        }
        while (this.foldersListStore.IterNext(ref folderIter));
      }

      folderIter = this.foldersListStore.AppendValues(folderEntry, true, Pango.Weight.Heavy);
      this.foldersNodeView.Selection.SelectIter(folderIter);
      this.addFolderEntry.Text = string.Empty;
      this.addFolderEntry.IsFocus = true;
      this.localFoldersList.Add(folderEntry);
    }

    /// <summary>
    /// Called when a key is released in the addFolderEntry control.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void AddFolderEntryReleaseEvent(object sender, Gtk.KeyReleaseEventArgs e)
    {
      Param.AssertNotNull(sender, "sender");
      Param.AssertNotNull(e, "e");

      if (e.Event.Key == Gdk.Key.Return || e.Event.Key == Gdk.Key.KP_Enter)
      {
        if (!string.IsNullOrEmpty(this.addFolderEntry.Text))
        {
          // Simulate a click of the add button.
          this.AddFolderButtonClicked(sender, e);
        }
      }
    }

    /// <summary>
    /// Called when the addRecognizedWordButton is clicked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void AddRecognizedWordButtonClicked(object sender, EventArgs e)
    {
      Param.Ignore(sender, e);

      string recognizedWord = this.addRecognizedWordEntry.Text;

      if (recognizedWord.Length == 0 || recognizedWord.Length < 2 || recognizedWord.Contains(" "))
      {
        MessageService.ShowError("Please enter a valid word before clicking the Add button");
        this.addRecognizedWordEntry.IsFocus = true;
        return;
      }

      Gtk.TreeIter recognizedWordIter;

      // Check if the folder is already in our list and if so select it instead of adding it.
      if (this.recognizedWordsListStore.GetIterFirst(out recognizedWordIter))
      {
        do
        {
          string currentValue = this.recognizedWordsListStore.GetValue(recognizedWordIter, (int)ListStoreColumns.Value) as string;
          if (!string.IsNullOrEmpty(currentValue) && currentValue == recognizedWord)
          {
            this.recognizedWordsNodeView.Selection.SelectIter(recognizedWordIter);
            this.addRecognizedWordEntry.Text = string.Empty;
            this.addRecognizedWordEntry.IsFocus = true;
            return;
          }
        }
        while (this.recognizedWordsListStore.IterNext(ref recognizedWordIter));
      }

      recognizedWordIter = this.recognizedWordsListStore.AppendValues(recognizedWord, true, Pango.Weight.Heavy);
      this.recognizedWordsNodeView.Selection.SelectIter(recognizedWordIter);
      this.addRecognizedWordEntry.Text = string.Empty;
      this.addRecognizedWordEntry.IsFocus = true;
      this.localRecognizedWordsList.Add(recognizedWord);
    }

    /// <summary>
    /// Called when a key is released in the addRecognizedWordEntry control.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void AddRecognizedWordEntryReleaseEvent(object sender, Gtk.KeyReleaseEventArgs e)
    {
      Param.AssertNotNull(sender, "sender");
      Param.AssertNotNull(e, "e");

      if (e.Event.Key == Gdk.Key.Return || e.Event.Key == Gdk.Key.KP_Enter)
      {
        if (!string.IsNullOrEmpty(this.addRecognizedWordEntry.Text))
        {
          // Simulate a click of the add button.
          this.AddRecognizedWordButtonClicked(sender, e);
        }
      }
    }

    /// <summary>
    /// Called when the removeDeprecatedWordButton is clicked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void RemoveDeprecatedWordButtonClicked(object sender, EventArgs e)
    {
      Param.Ignore(sender, e);

      Gtk.TreeIter selectedNodeIter;

      if (this.deprecatedWordsNodeView.Selection.GetSelected(out selectedNodeIter))
      {
        string deprecatedWordToRemove = (string)this.deprecatedWordsListStore.GetValue(selectedNodeIter, (int)ListStoreColumns.Value);
        if (this.deprecatedWordsListStore.Remove(ref selectedNodeIter))
        {
          this.deprecatedWordsNodeView.Selection.SelectIter(selectedNodeIter);
        }

        this.localDeprecatedWordsList.Remove(deprecatedWordToRemove);
      }
    }

    /// <summary>
    /// Called when the removeFolderButton is clicked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void RemoveFolderButtonClicked(object sender, EventArgs e)
    {
      Param.Ignore(sender, e);

      Gtk.TreeIter selectedNodeIter;

      if (this.foldersNodeView.Selection.GetSelected(out selectedNodeIter))
      {
        string folderToRemove = (string)this.foldersListStore.GetValue(selectedNodeIter, (int)ListStoreColumns.Value);
        if (this.foldersListStore.Remove(ref selectedNodeIter))
        {
          this.foldersNodeView.Selection.SelectIter(selectedNodeIter);
        }

        this.localFoldersList.Remove(folderToRemove);
      }
    }

    /// <summary>
    /// Called when the removeRecognizedWordButton is clicked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void RemoveRecognizedWordButtonClicked(object sender, EventArgs e)
    {
      Param.Ignore(sender, e);

      Gtk.TreeIter selectedNodeIter;

      if (this.recognizedWordsNodeView.Selection.GetSelected(out selectedNodeIter))
      {
        string recognizedWordToRemove = (string)this.recognizedWordsListStore.GetValue(selectedNodeIter, (int)ListStoreColumns.Value);
        if (this.recognizedWordsListStore.Remove(ref selectedNodeIter))
        {
          this.recognizedWordsNodeView.Selection.SelectIter(selectedNodeIter);
        }

        this.localRecognizedWordsList.Remove(recognizedWordToRemove);
      }
    }

    #endregion Protected Signal Methods

    #region Private Methods

    /// <summary>
    /// Adds spelling values from the parent settings.
    /// </summary>
    private void AddParentSpellingValues()
    {
      this.parentDeprecatedWordsList.Clear();
      this.parentFoldersList.Clear();
      this.parentRecognizedWordsList.Clear();

      if (this.SettingsHandler.ParentSettings != null)
      {
        CollectionProperty parentProperty = this.SettingsHandler.ParentSettings.GlobalSettings.GetProperty(RecognizedWordsPropertyName) as CollectionProperty;
        if (parentProperty != null && parentProperty.Values.Count > 0)
        {
          foreach (string value in parentProperty)
          {
            if (!string.IsNullOrEmpty(value))
            {
              this.recognizedWordsListStore.AppendValues(value, false, Pango.Weight.Normal);
              this.parentRecognizedWordsList.Add(value);
            }
          }
        }

        parentProperty = this.SettingsHandler.ParentSettings.GlobalSettings.GetProperty(DeprecatedWordsPropertyName) as CollectionProperty;
        if (parentProperty != null && parentProperty.Values.Count > 0)
        {
          foreach (string value in parentProperty)
          {
            if (!string.IsNullOrEmpty(value))
            {
              string[] splitValue = value.Split(',');
              if (splitValue.Length == 2)
              {
                this.deprecatedWordsListStore.AppendValues(splitValue[0].Trim() + ", " + splitValue[1].Trim(), false, Pango.Weight.Normal);
                this.parentDeprecatedWordsList.Add(value);
              }
            }
          }
        }

        parentProperty = this.SettingsHandler.ParentSettings.GlobalSettings.GetProperty(DictionaryFoldersPropertyName) as CollectionProperty;
        if (parentProperty != null && parentProperty.Values.Count > 0)
        {
          foreach (string value in parentProperty)
          {
            if (!string.IsNullOrEmpty(value))
            {
              this.foldersListStore.AppendValues(value, false, Pango.Weight.Normal);
              this.parentFoldersList.Add(value);
            }
          }
        }
      }
    }

    /// <summary>
    /// Fills all the node views with all spelling values.
    /// </summary>
    private void FillNodeViews()
    {
      if (this.deprecatedWordsListStore != null && this.foldersListStore != null && this.recognizedWordsListStore != null)
      {
        this.deprecatedWordsListStore.Clear();
        this.foldersListStore.Clear();
        this.recognizedWordsListStore.Clear();

        // Get the list of allowed words from the parent settings.
        this.AddParentSpellingValues();

        // Get the list of allowed words from the local settings.
        CollectionProperty localProperty = this.SettingsHandler.LocalSettings.GlobalSettings.GetProperty(RecognizedWordsPropertyName) as CollectionProperty;
        if (localProperty != null && localProperty.Values.Count > 0)
        {
          foreach (string value in localProperty)
          {
            if (!string.IsNullOrEmpty(value))
            {
              this.recognizedWordsListStore.AppendValues(value, true, Pango.Weight.Heavy);
              this.localRecognizedWordsList.Add(value);
            }
          }
        }

        localProperty = this.SettingsHandler.LocalSettings.GlobalSettings.GetProperty(DeprecatedWordsPropertyName) as CollectionProperty;
        if (localProperty != null && localProperty.Values.Count > 0)
        {
          foreach (string value in localProperty)
          {
            if (!string.IsNullOrEmpty(value))
            {
              string[] splitValue = value.Split(',');
              if (splitValue.Length == 2)
              {
                this.deprecatedWordsListStore.AppendValues(splitValue[0].Trim() + ", " + splitValue[1].Trim(), true, Pango.Weight.Heavy);
                this.localDeprecatedWordsList.Add(splitValue[0].Trim() + ", " + splitValue[1].Trim());
              }
            }
          }
        }

        localProperty = this.SettingsHandler.LocalSettings.GlobalSettings.GetProperty(DictionaryFoldersPropertyName) as CollectionProperty;
        if (localProperty != null && localProperty.Values.Count > 0)
        {
          foreach (string value in localProperty)
          {
            if (!string.IsNullOrEmpty(value))
            {
              this.foldersListStore.AppendValues(value, true, Pango.Weight.Heavy);
              this.localFoldersList.Add(value);
            }
          }
        }
      }
    }

    /// <summary>
    /// Initializes all node views.
    /// </summary>
    private void InitializeNodeViews()
    {
      Gtk.CellRendererText textRenderer = new Gtk.CellRendererText();
      textRenderer.Mode = Gtk.CellRendererMode.Activatable;

      Gtk.TreeViewColumn deprecatedWordsColumn = new Gtk.TreeViewColumn();
      deprecatedWordsColumn.Clickable = false;
      deprecatedWordsColumn.Reorderable = false;
      deprecatedWordsColumn.PackStart(textRenderer, false);
      deprecatedWordsColumn.AddAttribute(textRenderer, "text", (int)ListStoreColumns.Value);
      deprecatedWordsColumn.AddAttribute(textRenderer, "weight", (int)ListStoreColumns.Weight);
      this.deprecatedWordsNodeView.AppendColumn(deprecatedWordsColumn);
      this.deprecatedWordsNodeView.HeadersVisible = false;

      this.deprecatedWordsListStore = new Gtk.ListStore(typeof(string), typeof(bool), typeof(Pango.Weight));
      this.deprecatedWordsNodeView.Model = this.deprecatedWordsListStore;
      this.deprecatedWordsNodeView.Selection.Changed += this.OnDeprecatedWordsNodeViewSelectionChanged;

      Gtk.TreeViewColumn foldersColumn = new Gtk.TreeViewColumn();
      foldersColumn.Clickable = false;
      foldersColumn.Reorderable = false;
      foldersColumn.PackStart(textRenderer, false);
      foldersColumn.AddAttribute(textRenderer, "text", (int)ListStoreColumns.Value);
      foldersColumn.AddAttribute(textRenderer, "weight", (int)ListStoreColumns.Weight);
      this.foldersNodeView.AppendColumn(foldersColumn);
      this.foldersNodeView.HeadersVisible = false;

      this.foldersListStore = new Gtk.ListStore(typeof(string), typeof(bool), typeof(Pango.Weight));
      this.foldersNodeView.Model = this.foldersListStore;
      this.foldersNodeView.Selection.Changed += this.OnFoldersNodeViewSelectionChanged;

      Gtk.TreeViewColumn recognizedWordsColumn = new Gtk.TreeViewColumn();
      recognizedWordsColumn.Clickable = false;
      recognizedWordsColumn.Reorderable = false;
      recognizedWordsColumn.PackStart(textRenderer, false);
      recognizedWordsColumn.AddAttribute(textRenderer, "text", (int)ListStoreColumns.Value);
      recognizedWordsColumn.AddAttribute(textRenderer, "weight", (int)ListStoreColumns.Weight);
      this.recognizedWordsNodeView.AppendColumn(recognizedWordsColumn);
      this.recognizedWordsNodeView.HeadersVisible = false;

      this.recognizedWordsListStore = new Gtk.ListStore(typeof(string), typeof(bool), typeof(Pango.Weight));
      this.recognizedWordsNodeView.Model = this.recognizedWordsListStore;
      this.recognizedWordsNodeView.Selection.Changed += this.OnRecognizedWordsNodeViewSelectionChanged;
    }

    /// <summary>
    /// Called when deprecated word node view selection changed.
    /// </summary>
    /// <param name="sender">Object sender.</param>
    /// <param name="args">Event arguments.</param>
    private void OnDeprecatedWordsNodeViewSelectionChanged(object sender, EventArgs args)
    {
      Param.AssertNotNull(sender, "sender");
      Param.AssertNotNull(args, "args");

      Gtk.TreeIter selectedNodeIter;

      if (this.deprecatedWordsNodeView.Selection.GetSelected(out selectedNodeIter))
      {
        this.removeDeprecatedWordButton.Sensitive = (bool)this.deprecatedWordsListStore.GetValue(selectedNodeIter, (int)ListStoreColumns.Removable);
      }
      else
      {
        this.removeDeprecatedWordButton.Sensitive = false;
      }
    }

    /// <summary>
    /// Called when folders node view selection changed.
    /// </summary>
    /// <param name="sender">Object sender.</param>
    /// <param name="args">Event arguments.</param>
    private void OnFoldersNodeViewSelectionChanged(object sender, EventArgs args)
    {
      Param.AssertNotNull(sender, "sender");
      Param.AssertNotNull(args, "args");

      Gtk.TreeIter selectedNodeIter;

      if (this.foldersNodeView.Selection.GetSelected(out selectedNodeIter))
      {
        this.removeFolderButton.Sensitive = (bool)this.foldersListStore.GetValue(selectedNodeIter, (int)ListStoreColumns.Removable);
      }
      else
      {
        this.removeFolderButton.Sensitive = false;
      }
    }

    /// <summary>
    /// Called when recognized word node view selection changed.
    /// </summary>
    /// <param name="sender">Object sender.</param>
    /// <param name="args">Event arguments.</param>
    private void OnRecognizedWordsNodeViewSelectionChanged(object sender, EventArgs args)
    {
      Param.AssertNotNull(sender, "sender");
      Param.AssertNotNull(args, "args");

      Gtk.TreeIter selectedNodeIter;

      if (this.recognizedWordsNodeView.Selection.GetSelected(out selectedNodeIter))
      {
        this.removeRecognizedWordButton.Sensitive = (bool)this.recognizedWordsListStore.GetValue(selectedNodeIter, (int)ListStoreColumns.Removable);
      }
      else
      {
        this.removeRecognizedWordButton.Sensitive = false;
      }
    }

    #endregion Private Methods
  }
}