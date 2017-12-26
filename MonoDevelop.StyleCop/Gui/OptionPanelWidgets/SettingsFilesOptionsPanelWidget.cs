﻿//-----------------------------------------------------------------------
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
  /// <summary>
  /// StyleCop settings files options panel widget.
  /// </summary>
  public partial class SettingsFilesOptionsPanelWidget : Gtk.Bin
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="MonoDevelop.StyleCop.Gui.OptionPanelWidgets.SettingsFilesOptionsPanelWidget"/> class.
    /// </summary>
    public SettingsFilesOptionsPanelWidget()
    {
      this.Build();
    }
  }
}