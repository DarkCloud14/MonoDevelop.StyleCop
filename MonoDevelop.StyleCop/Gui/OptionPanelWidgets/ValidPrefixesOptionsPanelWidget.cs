//-----------------------------------------------------------------------
// <copyright file="ValidPrefixesOptionsPanelWidget.cs">
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
  using MonoDevelop.Core;
  using MonoDevelop.Ide;
  using global::StyleCop;

  /// <summary>
  /// StyleCop valid prefixes options panel widget.
  /// </summary>
  public partial class ValidPrefixesOptionsPanelWidget : StyleCopOptionsPanelWidget
  {
    #region Constants

    /// <summary>
    /// The name of the property containing the list of allowable prefixes.
    /// </summary>
    private const string AllowedPrefixesProperty = "Hungarian";

    #endregion Constants

    #region Private Fields

    /// <summary>
    /// The analyzer that this settings page is attached to.
    /// </summary>
    private readonly SourceAnalyzer analyzer;

    /// <summary>
    /// List which contains all local prefix values.
    /// </summary>
    private List<string> localPrefixValueList = new List<string>();

    /// <summary>
    /// Stores the current prefixes.
    /// </summary>
    private Gtk.ListStore prefixListStore;

    #endregion Private Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="MonoDevelop.StyleCop.Gui.OptionPanelWidgets.ValidPrefixesOptionsPanelWidget"/> class.
    /// </summary>
    public ValidPrefixesOptionsPanelWidget()
    {
      this.Build();
      this.removePrefixButton.Sensitive = false;
      this.analyzer = ProjectUtilities.Instance.Core.GetAnalyzer("StyleCop.CSharp.NamingRules");
    }

    #endregion Constructor

    #region Private Enumerators

    /// <summary>
    /// Contains all ListStore columns index numbers
    /// </summary>
    private enum ListStoreColumns : int
    {
      /// <summary>
      /// Column index number of the prefix item
      /// </summary>
      Prefix = 0,

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
      CollectionProperty list = new CollectionProperty(this.analyzer, AllowedPrefixesProperty, this.localPrefixValueList);
      this.SettingsHandler.LocalSettings.SetAddInSetting(this.analyzer, list);
    }

    /// <summary>
    /// Initializes the options panel widget values.
    /// </summary>
    /// <param name="settingsHandler">Settings handler object.</param>
    public override void Initialize(StyleCopSettingsHandler settingsHandler)
    {
      base.Initialize(settingsHandler);
      this.InitializeNodeView();
      this.FillNodeView();
    }

    #endregion Public Override Methods

    #region Protected Signal Methods

    /// <summary>
    /// Called when the addPrefixButton is clicked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void AddPrefixButtonClicked(object sender, EventArgs e)
    {
      Param.Ignore(sender, e);

      string prefixToAdd = this.addPrefixEntry.Text;

      if (string.IsNullOrEmpty(prefixToAdd) || prefixToAdd.Length > 2)
      {
        MessageService.ShowError("Please enter a one or two digit prefix before clicking the Add button.");
        this.addPrefixEntry.IsFocus = true;
        return;
      }

      Gtk.TreeIter prefixIter;

      // Check if prefix is already in our list and if so select it instead of adding it.
      if (this.prefixListStore.GetIterFirst(out prefixIter))
      {
        do
        {
          string currentPrefix = this.prefixListStore.GetValue(prefixIter, (int)ListStoreColumns.Prefix) as string;
          if (!string.IsNullOrEmpty(currentPrefix) && currentPrefix == prefixToAdd)
          {
            this.prefixNodeView.Selection.SelectIter(prefixIter);
            this.addPrefixEntry.Text = string.Empty;
            this.addPrefixEntry.IsFocus = true;
            return;
          }
        }
        while (this.prefixListStore.IterNext(ref prefixIter));
      }

      prefixIter = this.prefixListStore.AppendValues(prefixToAdd, true, Pango.Weight.Bold);
      this.prefixNodeView.Selection.SelectIter(prefixIter);
      this.addPrefixEntry.Text = string.Empty;
      this.addPrefixEntry.IsFocus = true;
      this.localPrefixValueList.Add(prefixToAdd);
    }

    /// <summary>
    /// Called when the removePrefixButton is clicked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    protected void RemovePrefixButtonClicked(object sender, EventArgs e)
    {
      Gtk.TreeIter selectedNodeIter;

      if (this.prefixNodeView.Selection.GetSelected(out selectedNodeIter))
      {
        string prefixToRemove = (string)this.prefixListStore.GetValue(selectedNodeIter, (int)ListStoreColumns.Prefix);
        if (this.prefixListStore.Remove(ref selectedNodeIter))
        {
          this.localPrefixValueList.Remove(prefixToRemove);
        }
      }
    }

    #endregion Protected Signal Methods

    #region Private Methods

    /// <summary>
    /// Add prefixes from the parent settings.
    /// </summary>
    private void AddParentPrefixes()
    {
      CollectionProperty parentPrefixesProperty = null;

      if (this.SettingsHandler.ParentSettings != null)
      {
        parentPrefixesProperty = this.SettingsHandler.ParentSettings.GetAddInSetting(this.analyzer, AllowedPrefixesProperty /*NamingRules.AllowedPrefixesProperty*/) as CollectionProperty;

        if (parentPrefixesProperty != null && parentPrefixesProperty.Values.Count > 0)
        {
          foreach (string value in parentPrefixesProperty)
          {
            if (!string.IsNullOrEmpty(value))
            {
              var iter = this.prefixListStore.AppendValues(value, false, Pango.Weight.Light);
            }
          }
        }
      }
    }

    /// <summary>
    /// Fills the node view with all prefixes.
    /// </summary>
    private void FillNodeView()
    {
      if (this.analyzer != null && this.prefixListStore != null)
      {
        this.prefixListStore.Clear();

        // Get the list of allowed prefixes from the parent settings.
        this.AddParentPrefixes();

        // Get the list of allowed prefixes from the local settings.
        CollectionProperty localPrefixesProperty = this.SettingsHandler.LocalSettings.GetAddInSetting(this.analyzer, AllowedPrefixesProperty) as CollectionProperty;

        if (localPrefixesProperty != null && localPrefixesProperty.Values.Count > 0)
        {
          foreach (string value in localPrefixesProperty)
          {
            if (!string.IsNullOrEmpty(value))
            {
              var iter = this.prefixListStore.AppendValues(value, true, Pango.Weight.Bold);
              this.localPrefixValueList.Add(value);
            }
          }
        }
      }
    }

    /// <summary>
    /// Initializes the node view.
    /// </summary>
    private void InitializeNodeView()
    {
      Gtk.CellRendererText prefixTextRenderer = new Gtk.CellRendererText();
      prefixTextRenderer.Mode = Gtk.CellRendererMode.Activatable;

      Gtk.TreeViewColumn prefixColumn = new Gtk.TreeViewColumn();
      prefixColumn.Clickable = false;
      prefixColumn.Reorderable = false;
      prefixColumn.SortColumnId = (int)ListStoreColumns.Prefix;
      prefixColumn.Title = GettextCatalog.GetString("Currently excluded prefixes:");
      prefixColumn.PackStart(prefixTextRenderer, false);
      prefixColumn.AddAttribute(prefixTextRenderer, "text", (int)ListStoreColumns.Prefix);
      prefixColumn.AddAttribute(prefixTextRenderer, "weight", (int)ListStoreColumns.Weight);
      this.prefixNodeView.AppendColumn(prefixColumn);

      this.prefixListStore = new Gtk.ListStore(typeof(string), typeof(bool), typeof(Pango.Weight));
      this.prefixListStore.SetSortColumnId((int)ListStoreColumns.Prefix, Gtk.SortType.Ascending);
      this.prefixNodeView.Model = this.prefixListStore;
      this.prefixNodeView.Selection.Changed += new EventHandler(this.OnNodeViewSelectionChanged);
    }

    /// <summary>
    /// Called when NodeView selection changed.
    /// Fills the description if a detailed setting was selected.
    /// </summary>
    /// <param name="sender">Object sender.</param>
    /// <param name="args">Event arguments.</param>
    private void OnNodeViewSelectionChanged(object sender, EventArgs args)
    {
      Gtk.TreeIter selectedNodeIter;

      if (this.prefixNodeView.Selection.GetSelected(out selectedNodeIter))
      {
        this.removePrefixButton.Sensitive = (bool)this.prefixListStore.GetValue(selectedNodeIter, (int)ListStoreColumns.Removable);
      }
      else
      {
        this.removePrefixButton.Sensitive = false;
      }
    }

    #endregion Private Methods
  }
}