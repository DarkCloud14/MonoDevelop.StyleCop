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

    #region Public Override Methods

    /// <summary>
    /// Initializes the options panel values just before the panel is shown to user for the first time.
    /// </summary>
    /// <returns>The options panel widget.</returns>
    /// <remarks>Will only be called if the user really gets to see the options panel.</remarks>
    public override Gtk.Widget CreatePanelWidget()
    {
      Gtk.CellRendererPixbuf pixBufRenderer = new Gtk.CellRendererPixbuf();
      Gtk.CellRendererText textRenderer = new Gtk.CellRendererText();
      Gtk.CellRendererToggle toggleRenderer = new Gtk.CellRendererToggle();
      toggleRenderer.Activatable = true;

      Gtk.TreeViewColumn rulesColumn = new Gtk.TreeViewColumn();
      rulesColumn.Title = GettextCatalog.GetString("Enabled rules");
      rulesColumn.PackStart(toggleRenderer, false);
      rulesColumn.PackStart(pixBufRenderer, false);
      rulesColumn.PackStart(textRenderer, true);
      rulesColumn.AddAttribute(toggleRenderer, "active", 0);
      rulesColumn.AddAttribute(pixBufRenderer, "pixbuf", 1);
      rulesColumn.AddAttribute(textRenderer, "text", 2);

      this.treeview1.AppendColumn(rulesColumn);

      Gtk.TreeStore rulesStore = new Gtk.TreeStore(typeof(bool), typeof(Gdk.Pixbuf), typeof(string));
      this.treeview1.Model = rulesStore;

      if (rulesStore != null)
      {
        // Add each of the parsers and analyzers to the tree.
        foreach (SourceParser parser in this.SettingsHandler.Core.Parsers)
        {
          Gtk.TreeIter parserIter = rulesStore.AppendValues(true, null, parser.Name);

          // Iterate through each of the analyzers and add a checkbox for each.
          foreach (SourceAnalyzer analyzer in parser.Analyzers)
          {
            Gtk.TreeIter analyzerIter = rulesStore.AppendValues(parserIter, false, null, analyzer.Name);
          }
        }
      }

      return base.CreatePanelWidget();
    }


    #endregion Public Override Methods

  }
}