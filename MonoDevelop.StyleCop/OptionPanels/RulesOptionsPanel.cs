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
  using MonoDevelop.Components;
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
      /// Column index number of the column which can contain any object
      /// </summary>
      Object = 2,

      /// <summary>
      /// Column index number of the column which can contain a SourceAnalyzer
      /// </summary>
      SourceAnalyzer = 3,

      /// <summary>
      /// Column index number of the column which contains the information if the rule is overridden or not
      /// </summary>
      Overridden = 4
    }

    #endregion Private Enumerators

    #region Public Override Methods

    /// <summary>
    /// Initializes the options panel values just before the panel is shown to user for the first time.
    /// </summary>
    /// <returns>The options panel widget.</returns>
    /// <remarks>Will only be called if the user really gets to see the options panel.</remarks>
    public override Control CreatePanelWidget()
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
    /// Checks if the given analyzer rule setting is overridden.
    /// </summary>
    /// <param name="analyzer">Source analyzer of the rule to check.</param>
    /// <param name="ruleToCheck">The rule that should be checked.</param>
    /// <param name="isChecked">Is the check box checked or unchecked?</param>
    /// <param name="hasOverriddenChilds">Value that is used for cells that don't have a analyzer.</param>
    /// <returns>True if the rule is overridden, false otherwise.</returns>
    private bool DetectRuleOverride(SourceAnalyzer analyzer, Rule ruleToCheck, bool isChecked, bool hasOverriddenChilds)
    {
      bool overridden = false;

      if (analyzer != null)
      {
        // Create a property representing the current value of the selection.
        string propertyName = ruleToCheck.Name + "#Enabled";
        BooleanProperty localValue = new BooleanProperty(analyzer, propertyName, isChecked);

        // Compare this with the parent value.
        overridden = this.SettingsHandler.SettingsComparer.IsAddInSettingOverwritten(analyzer, propertyName, localValue);
      }
      else
      {
        overridden = hasOverriddenChilds;
      }

      return overridden;
    }

    /// <summary>
    /// Fills the given analyzer node with it's rules.
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
                ruleParentIter = this.rulesStore.AppendValues(analyzerIter, false, null, rule, null, false);
                tempRuleGroups.Add(rule.RuleGroup, ruleParentIter);
              }
            }

            bool isChecked = rule.EnabledByDefault;
            BooleanProperty enabledDisabledSetting = analyzer.GetRuleSetting(this.SettingsHandler.MergedSettings, rule.Name, "Enabled") as BooleanProperty;
            if (enabledDisabledSetting != null)
            {
              isChecked = enabledDisabledSetting.Value;
            }

            bool isOverridden = this.DetectRuleOverride(analyzer, rule, isChecked, false);
            Gtk.TreeIter ruleIter = this.rulesStore.AppendValues(ruleParentIter, isChecked, null, rule, analyzer, isOverridden);
            this.SetParentIterValues(ruleIter, isChecked, null);

            if (isOverridden)
            {
              this.rulesStore.SetValue(ruleParentIter, (int)TreeStoreColumns.Overridden, true);
              this.rulesStore.SetValue(analyzerIter, (int)TreeStoreColumns.Overridden, true);
            }
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

      if (this.rulesTreeView.Selection.GetSelected(out selectedNodeIter))
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
    /// Initializes and fills the tree view.
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
      rulesColumn.SetCellDataFunc(ruleTextRenderer, new Gtk.TreeCellDataFunc(this.RenderRule));
      this.rulesTreeView.AppendColumn(rulesColumn);

      this.rulesStore = new Gtk.TreeStore(typeof(bool), typeof(Gdk.Pixbuf), typeof(object), typeof(SourceAnalyzer), typeof(bool));
      this.rulesTreeView.Model = this.rulesStore;
      this.rulesTreeView.Selection.Changed += new EventHandler(this.OnTreeViewSelectionChanged);

      if (this.rulesStore != null)
      {
        // Add each of the parsers and analyzers to the tree.
        foreach (SourceParser parser in this.SettingsHandler.Core.Parsers)
        {
          Gtk.TreeIter parserIter = this.rulesStore.AppendValues(false, null, parser, null, false);

          // Add each of the boolean properties exposed by the parser.
          this.StoreAddinProperties(parser);

          // Iterate through each of the analyzers and add a checkbox for each.
          foreach (SourceAnalyzer analyzer in parser.Analyzers)
          {
            Gtk.TreeIter analyzerIter = this.rulesStore.AppendValues(parserIter, false, null, analyzer, null, false);

            // Add each of the boolean properties exposed by the analyzer.
            this.StoreAddinProperties(analyzer);

            this.FillAnalyzerRules(analyzer, analyzerIter);

            if ((bool)this.rulesStore.GetValue(analyzerIter, (int)TreeStoreColumns.Overridden))
            {
              this.rulesStore.SetValue(parserIter, (int)TreeStoreColumns.Overridden, true);
            }
          }

          this.rulesTreeView.ExpandRow(this.rulesStore.GetPath(parserIter), false);
        }
      }
    }

    /// <summary>
    /// Initializes the node view.
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
      this.detailedSettingsNodeView.AppendColumn(detailedSettingsColumn);

      this.detailedSettingsStore = new Gtk.ListStore(typeof(bool), typeof(string), typeof(PropertyAddInPair));
      this.detailedSettingsNodeView.Model = this.detailedSettingsStore;
      this.detailedSettingsNodeView.Selection.Changed += new EventHandler(this.OnNodeViewSelectionChanged);
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

      this.descriptionTextView.Buffer.Clear();
      if (this.detailedSettingsNodeView.Selection.GetSelected(out selectedNodeIter))
      {
        PropertyAddInPair propertyAddInPair = (PropertyAddInPair)this.detailedSettingsStore.GetValue(selectedNodeIter, (int)ListStoreColumns.Object);
        this.descriptionTextView.Buffer.Text = propertyAddInPair.Property.Description;
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
        bool hasOverriddenChilds = this.SetChildIterValues(iter, newValue);
        this.SetParentIterValues(iter, newValue, hasOverriddenChilds);
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

      this.descriptionTextView.Buffer.Clear();
      this.FillDetailsNodeView();
      if (this.rulesTreeView.Selection.GetSelected(out selectedNodeIter))
      {
        object objectOfSelectedNode = this.rulesStore.GetValue(selectedNodeIter, (int)TreeStoreColumns.Object);
        StyleCopAddIn addIn = objectOfSelectedNode as StyleCopAddIn;

        if (addIn != null)
        {
          this.descriptionTextView.Buffer.Text = addIn.Description;
        }
        else
        {
          Rule rule = objectOfSelectedNode as Rule;
          if (rule != null)
          {
            this.descriptionTextView.Buffer.Text = rule.Description;
          }
          else
          {
            this.descriptionTextView.Buffer.Clear();
          }
        }
      }
    }

    /// <summary>
    /// Renders the rule text.
    /// </summary>
    /// <param name="column">A Gtk.TreeViewColumn.</param>
    /// <param name="cell">The Gtk.CellRenderer that is being rendered by <paramref name="column"/>.</param>
    /// <param name="model">The Gtk.TreeModel being rendered.</param>
    /// <param name="iter">A Gtk.TreeIter of the current row rendered.</param>
    private void RenderRule(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
    {
      string textToDisplay = "Unkown";

      Rule currentRule = model.GetValue(iter, (int)TreeStoreColumns.Object) as Rule;
      SourceAnalyzer analyzer = model.GetValue(iter, (int)TreeStoreColumns.SourceAnalyzer) as SourceAnalyzer;
      bool isOverridden = (bool)model.GetValue(iter, (int)TreeStoreColumns.Overridden);

      if (currentRule != null && analyzer != null)
      {
        textToDisplay = string.Format("{0}: {1}", currentRule.CheckId, currentRule.Name);
      }
      else
      {
        if (currentRule != null && !string.IsNullOrEmpty(currentRule.RuleGroup))
        {
          textToDisplay = currentRule.Name;
        }
        else
        {
          SourceParser parser = model.GetValue(iter, (int)TreeStoreColumns.Object) as SourceParser;
          if (parser != null)
          {
            textToDisplay = parser.Name;
          }
          else
          {
            analyzer = model.GetValue(iter, (int)TreeStoreColumns.Object) as SourceAnalyzer;
            if (analyzer != null)
            {
              textToDisplay = analyzer.Name;
            }
          }
        }
      }

      (cell as Gtk.CellRendererText).Markup = GetBoldMarkup(textToDisplay, isOverridden);
    }

    /// <summary>
    /// Goes through all children of the parent iterator and sets the given value.
    /// </summary>
    /// <param name="parentIter">Parent iterator.</param>
    /// <param name="valueToSet">Value that should be set.</param>
    /// <returns>True if overridden childs are available, false otherwise.</returns>
    private bool SetChildIterValues(Gtk.TreeIter parentIter, bool valueToSet)
    {
      bool hasOverriddenChilds = false;

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
            hasOverriddenChilds |= this.SetChildIterValues(childIter, valueToSet);
          }
          while (this.rulesStore.IterNext(ref childIter));
        }
      }

      Rule currentRule = this.rulesStore.GetValue(parentIter, (int)TreeStoreColumns.Object) as Rule;
      SourceAnalyzer analyzer = this.rulesStore.GetValue(parentIter, (int)TreeStoreColumns.SourceAnalyzer) as SourceAnalyzer;
      hasOverriddenChilds |= this.DetectRuleOverride(analyzer, currentRule, valueToSet, hasOverriddenChilds);
      this.rulesStore.SetValue(parentIter, (int)TreeStoreColumns.Overridden, hasOverriddenChilds);

      return hasOverriddenChilds;
    }

    /// <summary>
    /// Goes through all parents of the child iterator, tries to set the given property value and  the correct override value.
    /// </summary>
    /// <param name="childIter">Child iterator.</param>
    /// <param name="valueToSet">Value that should be set.</param>
    /// <param name="hasOverriddenChilds">If not null this will lead to an additional check of the parents override value and make sure it's set correctly.</param>
    private void SetParentIterValues(Gtk.TreeIter childIter, bool valueToSet, bool? hasOverriddenChilds)
    {
      Gtk.TreeIter parentIter;

      // Go through all parents and set the value if possible
      if (this.rulesStore.IterParent(out parentIter, childIter))
      {
        bool parentIterValue = (bool)this.rulesStore.GetValue(parentIter, (int)TreeStoreColumns.Toggle);
        bool parentIterOverrideValue = (bool)this.rulesStore.GetValue(parentIter, (int)TreeStoreColumns.Overridden);

        if (parentIterValue != valueToSet)
        {
          // If the value is true/check the checkbox we'll just do that without any other validation.
          if (valueToSet)
          {
            this.rulesStore.SetValue(parentIter, (int)TreeStoreColumns.Toggle, valueToSet);
            this.SetParentIterValues(parentIter, valueToSet, hasOverriddenChilds);
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
                  this.SetParentIterValues(parentIter, valueToSet, hasOverriddenChilds);
                }
              }
            }
          }
        }

        if (hasOverriddenChilds != null && parentIterOverrideValue != hasOverriddenChilds)
        {
          bool newParentIterOverrideValue = false;

          // Go through each child and get the correct override value..
          if (this.rulesStore.IterHasChild(parentIter))
          {
            if (this.rulesStore.IterChildren(out childIter, parentIter))
            {
              do
              {
                bool childIterOverrideValue = (bool)this.rulesStore.GetValue(childIter, (int)TreeStoreColumns.Overridden);
                newParentIterOverrideValue |= childIterOverrideValue;
              }
              while (this.rulesStore.IterNext(ref childIter));

              this.rulesStore.SetValue(parentIter, (int)TreeStoreColumns.Overridden, newParentIterOverrideValue);
              this.SetParentIterValues(parentIter, valueToSet, hasOverriddenChilds);
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