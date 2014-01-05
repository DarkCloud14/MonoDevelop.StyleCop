//-----------------------------------------------------------------------
// <copyright file="GeneralOptionsPanel.cs">
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
  using System.Collections.Generic;
  using System.Globalization;
  using System.IO;
  using System.Reflection;

  /// <summary>
  /// StyleCop general options panel.
  /// </summary>
  internal partial class GeneralOptionsPanel : ProjectOptionsPanelBase
  {
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="MonoDevelop.StyleCop.GeneralOptionsPanel"/> class.
    /// </summary>
    public GeneralOptionsPanel()
    {
      this.Build();
      this.cultureComboBox.AppendText("en-US");

      List<CultureInfo> cultures = new List<CultureInfo>(EnumStyleCopCultureInfos());
      foreach (CultureInfo cultureInfo in cultures)
      {
        this.cultureComboBox.AppendText(cultureInfo.IetfLanguageTag);
      }
    }

    #endregion Constructor

    #region Private Static Methods

    /// <summary>
    /// Enumerates StyleCop culture information available in sub directories of the executing assembly.
    /// </summary>
    /// <returns>All available and valid StyleCop culture information.</returns>
    private static IEnumerable<CultureInfo> EnumStyleCopCultureInfos()
    {
      string directoryName = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

      foreach (string directory in Directory.GetDirectories(directoryName))
      {
        string name = System.IO.Path.GetFileNameWithoutExtension(directory);

        // Culture format is XX-YY, we discard directories that can't match.
        if (name.Length > 5)
        {
          continue;
        }

        CultureInfo culture = null;
        try
        {
          culture = CultureInfo.GetCultureInfo(name);
        }
        catch (ArgumentNullException)
        {
          continue;
        }
        catch (ArgumentException)
        {
          continue;
        }

        string resName = "StyleCop.CSharp.Rules.resources.dll";
        if (File.Exists(System.IO.Path.Combine(System.IO.Path.Combine(directoryName, name), resName)))
        {
          yield return culture;
        }
      }
    }

    #endregion Private Static Methods
  }
}