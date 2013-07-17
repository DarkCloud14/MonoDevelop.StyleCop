//-----------------------------------------------------------------------
// <copyright file="XmlDocumentExtension.cs">
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
  using System.Xml;
  using System.Xml.Linq;

  /// <summary>
  /// Extension class to extend the XmlDocument class
  /// </summary>
  public static class XmlDocumentExtension
  {
    #region Public Static Extension Methods

    /// <summary>
    /// Converts XmlDocument class to XDocument class with LoadOptions.None.
    /// </summary>
    /// <param name="xmlDocument">The XML document to convert.</param>
    /// <returns>Returns the converted XDocument.</returns>
    public static XDocument ToXDocument(this XmlDocument xmlDocument)
    {
      return ToXDocument(xmlDocument, LoadOptions.None);
    }

    /// <summary>
    /// Converts XmlDocument class to XDocument class.
    /// </summary>
    /// <param name="xmlDocument">The XML document to convert.</param>
    /// <param name="loadOptions">A LoadOptions that specifies whether to load base URI and line information.</param>
    /// <returns>Returns the converted XDocument.</returns>
    public static XDocument ToXDocument(this XmlDocument xmlDocument, LoadOptions loadOptions)
    {
      return XDocument.Load(new XmlNodeReader(xmlDocument), loadOptions);
    }

    #endregion Public Static Extension Methods
  }
}