﻿//-----------------------------------------------------------------------
// <copyright file="AnalysisType.cs">
//   APL 2.0
// </copyright>
// <license>
//   Copyright 2012, 2013, 2018 Alexander Jochum
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
  /// <summary>
  /// Available analysis possibilities/types.
  /// </summary>
  internal enum AnalysisType
  {
    /// <summary>
    /// Analyze the active document.
    /// </summary>
    ActiveDocument,
    
    /// <summary>
    /// Analyze all supported files in the solution.
    /// </summary>
    Solution,
    
    /// <summary>
    /// Analyze all supported files in the solution.
    /// </summary>
    /// <remarks>StyleCop will be forced to do a full analysis again instead of using is previously build cache.</remarks>
    SolutionFullAnalysis,
    
    /// <summary>
    /// Analyze all supported files of the selected nodes.
    /// </summary>
    NodeAnalysis,
    
    /// <summary>
    /// Analyze all supported files in all supported and selected project nodes.
    /// </summary>
    /// <remarks>StyleCop will be forced to do a full analysis again instead of using is previously build cache.</remarks>
    FullNodeAnalysis,

    /// <summary>
    /// Exclude the active document from StyleCop analysis.
    /// </summary>
    ExcludeActiveDocument,

    /// <summary>
    /// Exclude all selected project files from StyleCop analysis.
    /// </summary>
    ExcludeProjectFile,
  }
}