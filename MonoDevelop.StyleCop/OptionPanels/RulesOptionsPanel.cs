//-----------------------------------------------------------------------
// <copyright file="RulesOptionsPanel.cs">
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
  using System;
  using System.Collections.Generic;
  using System.Reflection;
  using MonoDevelop.Core;
  using global::StyleCop;

  /// <summary>
  /// StyleCop rules options panel.
  /// </summary>
  internal partial class RulesOptionsPanel : ProjectOptionsPanelBase
  {
    #region Constants

    /// <summary>
    /// The name of an analyzer node.
    /// </summary>
    private const string AnalyzerNode = "AnalyzerNode";

    /// <summary>
    /// The name of a parser node.
    /// </summary>
    private const string ParserNode = "ParserNode";

    /// <summary>
    /// The name of a rule group node.
    /// </summary>
    private const string RuleGroupNode = "RuleGroupNode";

    /// <summary>
    /// The name of a rule node.
    /// </summary>
    private const string RuleNode = "RuleNode";

    #endregion

    #region Private Fields

    /// <summary>
    /// Stores the properties for each analyzer and parser.
    /// </summary>
    private readonly Dictionary<StyleCopAddIn, ICollection<BooleanProperty>> properties = new Dictionary<StyleCopAddIn, ICollection<BooleanProperty>>();

    /// <summary>
    /// Stores the detailed settings.
    /// </summary>
    private Gtk.ListStore detailedSettingsStore;

    /// <summary>
    /// Model of TreeView which stores all analyzer rules.
    /// </summary>
    private Gtk.TreeStore rulesStore;

    #endregion Private Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="MonoDevelop.StyleCop.RulesOptionsPanel"/> class.
    /// </summary>
    public RulesOptionsPanel()
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
      /// Column index number of the toggle cell renderer
      /// </summary>
      Toggle = 0,

      /// <summary>
      /// Column index number of the text cell renderer markup property
      /// </summary>
      Markup = 1,

      /// <summary>
      /// Column index number of the column which can contain any object
      /// </summary>
      Object = 2
    }

    /// <summary>
    /// Contains all TreeStore columns index numbers
    /// </summary>
    private enum TreeStoreColumns : int
    {
      /// <summary>
      /// Column index number of the toggle cell renderer
      /// </summary>
      Toggle = 0,

      /// <summary>
      /// Column index number of the pixel buffer cell renderer
      /// </summary>
      PixBuf = 1,

      /// <summary>
      /// Column index number of the text cell renderer markup property
      /// </summary>
      Markup = 2,

      /// <summary>
      /// Column index number of the column which can contain any object
      /// </summary>
      Object = 3
    }

    #endregion Private Enumerators

    #region Public Override Methods

    /// <summary>
    /// Initializes the options panel values just before the panel is shown to user for the first time.
    /// </summary>
    /// <returns>The options panel widget.</returns>
    /// <remarks>Will only be called if the user really gets to see the options panel.</remarks>
    public override Gtk.Widget CreatePanelWidget()
    {
      this.InitializeNodeView();
      this.InitializeAndFillTreeView();

      return base.CreatePanelWidget();
    }

    #endregion Public Override Methods

    #region Private Static Methods

    /// <summary>
    /// Depending on <paramref name="bold"/> parameter the function returns <paramref name="inputText"/> with or without bold markup.
    /// </summary>
    /// <param name="inputText">Input text.</param>
    /// <param name="bold">True if bold markup should be added, false otherwise.</param>
    /// <returns>Returns <paramref name="inputText"/> with or without bold markup.</returns>
    private static string GetBoldMarkup(string inputText, bool bold)
    {
      return bold ? string.Format("<b>{0}</b>", inputText) : inputText;
    }

    #endregion Private Static Methods

    #region Private Methods

    /// <summary>
    /// Checks if the given detailed settings property is overridden 
    /// </summary>
    /// <param name="iter">Tree iterator to use.</param>
    /// <param name="propertyAddInPair">Property addin pair to check.</param>
    /// <param name="isChecked">Is checked value.</param>
    /// <returns>Property friendly name with appropriate markup.</returns>
    private string DetectDetailsSettingsBoldState(Gtk.TreeIter iter, PropertyAddInPair propertyAddInPair, bool isChecked)
    {
      // Create a property representing the current value of the selection.
      BooleanProperty localValue = new BooleanProperty((PropertyDescriptor<bool>)propertyAddInPair.Property.PropertyDescriptor, isChecked);

      // Compare this with the parent value.
      bool overridden = this.SettingsHandler.SettingsComparer.IsAddInSettingOverwritten(propertyAddInPair.AddIn, propertyAddInPair.Property.PropertyName, localValue);

      return GetBoldMarkup(propertyAddInPair.Property.FriendlyName, overridden);
    }

    /// <summary>
    /// Fills the give analyzer node with it's rules
    /// </summary>
    /// <param name="analyzer">Source analyzer</param>
    /// <param name="analyzerIter">Analyzer node iterator.</param>
    private void FillAnalyzerRules(SourceAnalyzer analyzer, Gtk.TreeIter analyzerIter)
    {
      IEnumerable<Rule> addinRules = null;

      // We must use reflection at the moment to get all analyzer rules.
      PropertyInfo prop = analyzer.GetType().GetProperty("AddInRules", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      if (prop != null && prop.CanRead)
      {
        addinRules = prop.GetValue(analyzer, null) as IEnumerable<Rule>;
      }

      if (addinRules != null)
      {
        Dictionary<string, Gtk.TreeIter> tempRuleGroups = new Dictionary<string, Gtk.TreeIter>();

        foreach (Rule rule in addinRules)
        {
          // Only show rules which can be disabled.
          if (rule.CanDisable)
          {
            // Get or create the rule group node for this rule, if necessary.
            Gtk.TreeIter ruleParentIter = analyzerIter;
            if (!string.IsNullOrEmpty(rule.RuleGroup))
            {
              if (!tempRuleGroups.TryGetValue(rule.RuleGroup, out ruleParentIter))
              {
                ruleParentIter = this.rulesStore.AppendValues(analyzerIter, false, null, rule.RuleGroup, rule);
                tempRuleGroups.Add(rule.RuleGroup, ruleParentIter);
              }
            }

            this.rulesStore.AppendValues(ruleParentIter, false, null, string.Format("{0}: {1}", rule.CheckId, rule.Name), rule);
          }
        }
      }
    }

    /// <summary>
    /// Fills the details node view with detailed settings if available.
    /// </summary>
    private void FillDetailsNodeView()
    {
      Gtk.TreeIter selectedNodeIter;

      this.detailedSettingsStore.Clear();

      if (this.treeview1.Selection.GetSelected(out selectedNodeIter))
      {
        StyleCopAddIn addIn = this.rulesStore.GetValue(selectedNodeIter, (int)TreeStoreColumns.Object) as StyleCopAddIn;
        if (addIn != null)
        {
          // Get the properties for this addin.
          ICollection<BooleanProperty> addInProperties = null;
          if (this.properties.TryGetValue(addIn, out addInProperties))
          {
            foreach (BooleanProperty property in addInProperties)
            {
              PropertyAddInPair propertyAddInPair = new PropertyAddInPair();
              propertyAddInPair.Property = property;
              propertyAddInPair.AddIn = addIn;

              string propertyFriendlyName = this.DetectDetailsSettingsBoldState(selectedNodeIter, propertyAddInPair, property.Value);
              this.detailedSettingsStore.AppendValues(property.Value, propertyFriendlyName, propertyAddInPair);
            }
          }
        }
      }
    }

    /// <summary>
    /// Initializes the node view
    /// </summary>
    private void InitializeNodeView()
    {
      Gtk.CellRendererText detailedSettingsTextRenderer = new Gtk.CellRendererText();
      detailedSettingsTextRenderer.Mode = Gtk.CellRendererMode.Activatable;
      Gtk.CellRendererToggle detailedSettingsToggleRenderer = new Gtk.CellRendererToggle();
      detailedSettingsToggleRenderer.Activatable = true;
      detailedSettingsToggleRenderer.Toggled += new Gtk.ToggledHandler(this.OnDetailedSettingsToggled);

      Gtk.TreeViewColumn detailedSettingsColumn = new Gtk.TreeViewColumn();
      detailedSettingsColumn.Clickable = false;
      detailedSettingsColumn.Reorderable = false;
      detailedSettingsColumn.Title = GettextCatalog.GetString("Detailed settings");
      detailedSettingsColumn.PackStart(detailedSettingsToggleRenderer, false);
      detailedSettingsColumn.PackStart(detailedSettingsTextRenderer, false);
      detailedSettingsColumn.AddAttribute(detailedSettingsToggleRenderer, "active", (int)ListStoreColumns.Toggle);
      detailedSettingsColumn.AddAttribute(detailedSettingsTextRenderer, "markup", (int)ListStoreColumns.Markup);
      this.nodeview4.AppendColumn(detailedSettingsColumn);

      this.detailedSettingsStore = new Gtk.ListStore(typeof(bool), typeof(string), typeof(PropertyAddInPair));
      this.nodeview4.Model = this.detailedSettingsStore;
      this.nodeview4.Selection.Changed += new EventHandler(this.OnNodeViewSelectionChanged);
    }

    /// <summary>
    /// Initializes and fills the tree view
    /// </summary>
    private void InitializeAndFillTreeView()
    {
      Gtk.CellRendererPixbuf rulePixBufRenderer = new Gtk.CellRendererPixbuf();
      rulePixBufRenderer.Mode = Gtk.CellRendererMode.Activatable;
      Gtk.CellRendererText ruleTextRenderer = new Gtk.CellRendererText();
      ruleTextRenderer.Mode = Gtk.CellRendererMode.Activatable;
      Gtk.CellRendererToggle ruleToggleRenderer = new Gtk.CellRendererToggle();
      ruleToggleRenderer.Activatable = true;
      ruleToggleRenderer.Toggled += new Gtk.ToggledHandler(this.OnRuleToggled);

      Gtk.TreeViewColumn rulesColumn = new Gtk.TreeViewColumn();
      rulesColumn.Clickable = false;
      rulesColumn.Reorderable = false;
      rulesColumn.Title = GettextCatalog.GetString("Enabled rules");
      rulesColumn.PackStart(ruleToggleRenderer, false);
      rulesColumn.PackStart(rulePixBufRenderer, false);
      rulesColumn.PackStart(ruleTextRenderer, false);
      rulesColumn.AddAttribute(ruleToggleRenderer, "active", (int)TreeStoreColumns.Toggle);
      rulesColumn.AddAttribute(rulePixBufRenderer, "pixbuf", (int)TreeStoreColumns.PixBuf);
      rulesColumn.AddAttribute(ruleTextRenderer, "markup", (int)TreeStoreColumns.Markup);
      this.treeview1.AppendColumn(rulesColumn);

      this.rulesStore = new Gtk.TreeStore(typeof(bool), typeof(Gdk.Pixbuf), typeof(string), typeof(object));
      this.treeview1.Model = this.rulesStore;
      this.treeview1.Selection.Changed += new EventHandler(this.OnTreeViewSelectionChanged);

      if (this.rulesStore != null)
      {
        // Add each of the parsers and analyzers to the tree.
        foreach (SourceParser parser in this.SettingsHandler.Core.Parsers)
        {
          Gtk.TreeIter parserIter = this.rulesStore.AppendValues(false, null, parser.Name, parser);

          // Add each of the boolean properties exposed by the parser.
          this.StoreAddinProperties(parser);

          // Iterate through each of the analyzers and add a checkbox for each.
          foreach (SourceAnalyzer analyzer in parser.Analyzers)
          {
            Gtk.TreeIter analyzerIter = this.rulesStore.AppendValues(parserIter, false, null, analyzer.Name, analyzer);

            // Add each of the boolean properties exposed by the analyzer.
            this.StoreAddinProperties(analyzer);

            this.FillAnalyzerRules(analyzer, analyzerIter);
          }
        }
      }
    }

    /// <summary>
    /// Called when the CellRendererToggle checkbox of node view is checked or unchecked.
    /// </summary>
    /// <param name="sender">Object sender.</param>
    /// <param name="toggledArgs">The toggled event arguments.</param>
    private void OnDetailedSettingsToggled(object sender, Gtk.ToggledArgs toggledArgs)
    {
      Gtk.TreeIter iter;

      if (this.detailedSettingsStore.GetIter(out iter, new Gtk.TreePath(toggledArgs.Path)))
      {
        bool newValue = !(bool)this.detailedSettingsStore.GetValue(iter, (int)ListStoreColumns.Toggle);
        this.detailedSettingsStore.SetValue(iter, (int)ListStoreColumns.Toggle, newValue);

        PropertyAddInPair propertyAddInPair = (PropertyAddInPair)this.detailedSettingsStore.GetValue(iter, (int)ListStoreColumns.Object);
        propertyAddInPair.Property.Value = newValue;

        string newPropertyFriendlyName = this.DetectDetailsSettingsBoldState(iter, propertyAddInPair, newValue);
        this.detailedSettingsStore.SetValue(iter, (int)ListStoreColumns.Markup, newPropertyFriendlyName);
      }
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

      this.textview1.Buffer.Clear();
      if (this.nodeview4.Selection.GetSelected(out selectedNodeIter))
      {
        PropertyAddInPair propertyAddInPair = (PropertyAddInPair)this.detailedSettingsStore.GetValue(selectedNodeIter, (int)ListStoreColumns.Object);
        this.textview1.Buffer.Text = propertyAddInPair.Property.Description;
      }
    }

    /// <summary>
    /// Called when the CellRendererToggle checkbox of tree view is checked or unchecked.
    /// </summary>
    /// <param name="sender">Object sender.</param>
    /// <param name="toggledArgs">The toggled event arguments.</param>
    private void OnRuleToggled(object sender, Gtk.ToggledArgs toggledArgs)
    {
      Gtk.TreeIter iter;

      if (this.rulesStore.GetIter(out iter, new Gtk.TreePath(toggledArgs.Path)))
      {
        bool newValue = !(bool)this.rulesStore.GetValue(iter, (int)TreeStoreColumns.Toggle);
        this.rulesStore.SetValue(iter, (int)TreeStoreColumns.Toggle, newValue);
        this.SetChildIterValues(iter, newValue);
        this.SetParentIterValues(iter, newValue);
      }
    }

    /// <summary>
    /// Called when TreeView selection changed.
    /// Fills the details node view if there are any detailed settings for the selected node.
    /// </summary>
    /// <param name="sender">Object sender.</param>
    /// <param name="args">Event arguments.</param>
    private void OnTreeViewSelectionChanged(object sender, EventArgs args)
    {
      Gtk.TreeIter selectedNodeIter;

      this.textview1.Buffer.Clear();
      this.FillDetailsNodeView();
      if (this.treeview1.Selection.GetSelected(out selectedNodeIter))
      {
        object objectOfSelectedNode = this.rulesStore.GetValue(selectedNodeIter, (int)TreeStoreColumns.Object);
        StyleCopAddIn addIn = objectOfSelectedNode as StyleCopAddIn;

        if (addIn != null)
        {
          this.textview1.Buffer.Text = addIn.Description;
        }
        else
        {
          Rule rule = objectOfSelectedNode as Rule;
          if (rule != null)
          {
            this.textview1.Buffer.Text = rule.Description;
          }
          else
          {
            this.textview1.Buffer.Clear();
          }
        }
      }
    }

    /// <summary>
    /// Goes through all children of the parent iterator and sets the given value.
    /// </summary>
    /// <param name="parentIter">Parent iterator.</param>
    /// <param name="valueToSet">Value that should be set.</param>
    private void SetChildIterValues(Gtk.TreeIter parentIter, bool valueToSet)
    {
      // Go through all childs and set the same value.
      if (this.rulesStore.IterHasChild(parentIter))
      {
        Gtk.TreeIter childIter;

        // Get the first child iter and after that go through each until IterNext is false..
        if (this.rulesStore.IterChildren(out childIter, parentIter))
        {
          do
          {
            this.rulesStore.SetValue(childIter, (int)TreeStoreColumns.Toggle, valueToSet);
            this.SetChildIterValues(childIter, valueToSet);
          }
          while (this.rulesStore.IterNext(ref childIter));
        }
      }
    }

    /// <summary>
    /// Goes through all parents of the child iterator and tries to set the give value.
    /// </summary>
    /// <param name="childIter">Child iterator.</param>
    /// <param name="valueToSet">Value that should be set.</param>
    private void SetParentIterValues(Gtk.TreeIter childIter, bool valueToSet)
    {
      Gtk.TreeIter parentIter;

      // Go through all parents and set the value if possible
      if (this.rulesStore.IterParent(out parentIter, childIter))
      {
        bool parentIterValue = (bool)this.rulesStore.GetValue(parentIter, (int)TreeStoreColumns.Toggle);

        if (parentIterValue != valueToSet)
        {
          // If the value is true/check the checkbox we'll just do that without any other validation.
          if (valueToSet)
          {
            this.rulesStore.SetValue(parentIter, (int)TreeStoreColumns.Toggle, valueToSet);
            this.SetParentIterValues(parentIter, valueToSet);
          }
          else
          {
            // In case we should uncheck the checkbox we'll go through each child of the parent
            // and only modify the value if no other child is checked.
            bool modifyParentIterValue = true;

            // Go through each child and check if one has a different value
            if (this.rulesStore.IterHasChild(parentIter))
            {
              if (this.rulesStore.IterChildren(out childIter, parentIter))
              {
                do
                {
                  bool childIterValue = (bool)this.rulesStore.GetValue(childIter, (int)TreeStoreColumns.Toggle);
                  if (childIterValue != valueToSet)
                  {
                    // We can stop here as we've found a child that is still checked so we don't modify the parent iterator value.
                    modifyParentIterValue = false;
                    break;
                  }
                }
                while (this.rulesStore.IterNext(ref childIter));

                if (modifyParentIterValue)
                {
                  this.rulesStore.SetValue(parentIter, (int)TreeStoreColumns.Toggle, valueToSet);
                  this.SetParentIterValues(parentIter, valueToSet);
                }
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Stores the properties for the given add-in.
    /// </summary>
    /// <param name="addIn">
    /// The add-in.
    /// </param>
    private void StoreAddinProperties(StyleCopAddIn addIn)
    {
      Param.AssertNotNull(addIn, "addIn");

      ICollection<PropertyDescriptor> addInPropertyDescriptors = addIn.PropertyDescriptors;
      if (addInPropertyDescriptors != null && addInPropertyDescriptors.Count > 0)
      {
        List<BooleanProperty> storedProperties = new List<BooleanProperty>(addInPropertyDescriptors.Count);

        foreach (PropertyDescriptor propertyDescriptor in addInPropertyDescriptors)
        {
          if (propertyDescriptor.PropertyType == PropertyType.Boolean && propertyDescriptor.DisplaySettings)
          {
            PropertyDescriptor<bool> booleanPropertyDescriptor = (PropertyDescriptor<bool>)propertyDescriptor;

            // Ensure that the property has a friendly name and a description.
            if (string.IsNullOrEmpty(propertyDescriptor.FriendlyName))
            {
              throw new ArgumentException("The friendly name of the property has not been set.");
            }

            if (string.IsNullOrEmpty(propertyDescriptor.Description))
            {
              throw new ArgumentException("The property description has not been set.");
            }

            BooleanProperty storedProperty = new BooleanProperty(booleanPropertyDescriptor, booleanPropertyDescriptor.DefaultValue);

            this.InitializePropertyState(addIn, storedProperty);

            storedProperties.Add(storedProperty);
          }
        }

        this.properties.Add(addIn, storedProperties.ToArray());
      }
    }

    /// <summary>
    /// Sets the check state for the given property.
    /// </summary>
    /// <param name="addIn">
    /// The addin that owns the property.
    /// </param>
    /// <param name="property">
    /// The property.
    /// </param>
    private void InitializePropertyState(StyleCopAddIn addIn, BooleanProperty property)
    {
      Param.AssertNotNull(addIn, "addIn");
      Param.AssertNotNull(property, "property");

      BooleanProperty mergedProperty = addIn.GetSetting(this.SettingsHandler.MergedSettings, property.PropertyName) as BooleanProperty;
      if (mergedProperty == null)
      {
        property.Value = property.DefaultValue;
      }
      else
      {
        property.Value = mergedProperty.Value;
      }
    }

    #endregion Private methods

    #region Private Structs

    /// <summary>
    /// A property addin pair.
    /// </summary>
    private struct PropertyAddInPair
    {
      #region Public Fields

      /// <summary>
      /// Gets or sets the add-in property.
      /// </summary>
      public StyleCopAddIn AddIn;

      /// <summary>
      /// Gets or sets the boolean property.
      /// </summary>
      public BooleanProperty Property;

      #endregion Public Fields
    }

    #endregion Private Structs
  }
}