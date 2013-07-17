//-----------------------------------------------------------------------
// <copyright file="StartupHandler.cs">
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
  using MonoDevelop.Components.Commands;

  /// <summary>
  /// This class get's initialized on MonoDevelop startup and setup all necessary stuff that has to be loaded only once.
  /// </summary>
  internal sealed class StartupHandler : CommandHandler
  {
    #region Protected Override Methods

    /// <summary>
    /// This method get's called on MonoDevelop startup and will setup all necessary stuff that has to be loaded only once.
    /// </summary>
    protected override void Run()
    {
      base.Run();

      // Call this function of ProjectUtilities to initialize everything.
      ProjectUtilities.Instance.CacheKnownFiles(AnalysisType.ActiveDocument);

      // Get the version numbers of StyleCop to initialize StyleCopVersion class.
      string styleCopMajorMinorVersionNumber = StyleCopVersion.VersionNumberMajorMinor;
      string styleCopFullVersionNumber = StyleCopVersion.VersionNumberFull;

      // Some debugging output..
      System.Diagnostics.Debug.WriteLine("MonoDevelop.StyleCop Addin loaded..");
      System.Diagnostics.Debug.WriteLine(string.Format("StyleCop Version {0} (build {1})", styleCopMajorMinorVersionNumber, styleCopFullVersionNumber));
    }

    #endregion Protected Override Methods
  }
}