//-----------------------------------------------------------------------
// <copyright file="StyleCopVersion.cs">
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
  using System.Reflection;
  using global::StyleCop;

  /// <summary>
  /// Class which can be used to get the StyleCop version number
  /// </summary>
  internal static class StyleCopVersion
  {
    #region Private Static Fields

    /// <summary>
    /// Stores the full version number of StyleCop i.e.  4.7.36.0.
    /// </summary>
    private static readonly string FullVersionNumber;

    /// <summary>
    /// Store the Major.Minor parts of the StyleCop version number i.e. 4.7.
    /// </summary>
    private static readonly string MajorMinorVersionNumber;

    #endregion Private Static Fields

    #region Constructor

    /// <summary>
    /// Initializes static members of the <see cref="StyleCopVersion"/> class.
    /// </summary>
    static StyleCopVersion()
    {
      var assembly = Assembly.GetAssembly(typeof(StyleCopCore));
      MajorMinorVersionNumber = assembly.GetName().Version.ToString(2);

      var customAttribute = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
      if (customAttribute.Length > 0)
      {
        Version tempVersion = new Version(((AssemblyFileVersionAttribute)customAttribute[0]).Version);
        FullVersionNumber = tempVersion.ToString(4);
      }
      else
      {
        FullVersionNumber = assembly.GetName().Version.ToString(4);
      }
    }

    #endregion Constructor

    #region Internal Static Properties

    /// <summary>
    /// Gets the full version number of StyleCop (not MonoDevelop.StyleCop!) i.e. 4.7.36.0.
    /// </summary>
    internal static string VersionNumberFull
    {
      get { return FullVersionNumber; }
    }

    /// <summary>
    /// Gets the Major.Minor parts of the StyleCop (not MonoDevelop.StyleCop!) version number i.e. 4.7.
    /// </summary>
    internal static string VersionNumberMajorMinor
    {
      get { return MajorMinorVersionNumber; }
    }

    #endregion Internal Static Properties
  }
}