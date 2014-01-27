//-----------------------------------------------------------------------
// <copyright file="RulesOptionsPanel.cs">
//   APL 2.0
// </copyright>
// <license>
//   Copyright 2013 Alexander Jochum
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
      /// Column index number of the text cell renderer
      /// </summary>
      Text = 2
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
      this.InitializeAndFillTreeView();

      return base.CreatePanelWidget();
    }

    #endregion Public Override Methods

    #region Private Methods

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
                ruleParentIter = this.rulesStore.AppendValues(analyzerIter, false, null, rule.RuleGroup);
                tempRuleGroups.Add(rule.RuleGroup, ruleParentIter);
              }
            }

            this.rulesStore.AppendValues(ruleParentIter, false, null, string.Format("{0}: {1}", rule.CheckId, rule.Name));
          }
        }
      }
    }

    /// <summary>
    /// Initializes and fills the tree view
    /// </summary>
    private void InitializeAndFillTreeView()
    {
      Gtk.CellRendererPixbuf rulePixBufRenderer = new Gtk.CellRendererPixbuf();
      Gtk.CellRendererText ruleTextRenderer = new Gtk.CellRendererText();
      Gtk.CellRendererToggle ruleToggleRenderer = new Gtk.CellRendererToggle();
      ruleToggleRenderer.Activatable = true;
      ruleToggleRenderer.Toggled += new Gtk.ToggledHandler(this.RuleToggleRenderer_Toggled);

      Gtk.TreeViewColumn rulesColumn = new Gtk.TreeViewColumn();
      rulesColumn.Title = GettextCatalog.GetString("Enabled rules");
      rulesColumn.PackStart(ruleToggleRenderer, false);
      rulesColumn.PackStart(rulePixBufRenderer, false);
      rulesColumn.PackStart(ruleTextRenderer, false);
      rulesColumn.AddAttribute(ruleToggleRenderer, "active", (int)TreeStoreColumns.Toggle);
      rulesColumn.AddAttribute(rulePixBufRenderer, "pixbuf", (int)TreeStoreColumns.PixBuf);
      rulesColumn.AddAttribute(ruleTextRenderer, "text", (int)TreeStoreColumns.Text);

      this.treeview1.AppendColumn(rulesColumn);

      this.rulesStore = new Gtk.TreeStore(typeof(bool), typeof(Gdk.Pixbuf), typeof(string));
      this.treeview1.Model = this.rulesStore;

      if (this.rulesStore != null)
      {
        // Add each of the parsers and analyzers to the tree.
        foreach (SourceParser parser in this.SettingsHandler.Core.Parsers)
        {
          Gtk.TreeIter parserIter = this.rulesStore.AppendValues(false, null, parser.Name);

          // Iterate through each of the analyzers and add a checkbox for each.
          foreach (SourceAnalyzer analyzer in parser.Analyzers)
          {
            Gtk.TreeIter analyzerIter = this.rulesStore.AppendValues(parserIter, false, null, analyzer.Name);
            this.FillAnalyzerRules(analyzer, analyzerIter);
          }
        }
      }
    }

    /// <summary>
    /// Called when a CellRendererToggle checkbox is checked or unchecked.
    /// </summary>
    /// <param name="sender">object sender</param>
    /// <param name="toggledArgs">The toggled event arguments.</param>
    private void RuleToggleRenderer_Toggled(object sender, Gtk.ToggledArgs toggledArgs)
    {
      Gtk.TreeIter iter;

      if (this.rulesStore.GetIter(out iter, new Gtk.TreePath(toggledArgs.Path)))
      {
        bool old = (bool)this.rulesStore.GetValue(iter, (int)TreeStoreColumns.Toggle);
        this.rulesStore.SetValue(iter, (int)TreeStoreColumns.Toggle, !old);
        this.SetChildIterValues(iter, !old);
        this.SetParentIterValues(iter, !old);
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

    #endregion Private methods
  }
}