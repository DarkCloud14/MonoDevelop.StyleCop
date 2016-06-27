//-----------------------------------------------------------------------
// <copyright file="StyleCopNodeExtension.cs">
//   APL 2.0
// </copyright>
// <license>
//   Copyright 2013-2016 Alexander Jochum
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
  using MonoDevelop.Ide;
  using MonoDevelop.Ide.Gui.Components;
  using MonoDevelop.Ide.Gui.Pads.ProjectPad;
  using MonoDevelop.Projects;

  /// <summary>
  /// StyleCop node builder extension.
  /// </summary>
  internal class StyleCopNodeExtension : NodeBuilderExtension
  {
    #region Public Override Properties

    /// <summary>
    /// Gets the type of the command handler.
    /// </summary>
    /// <value>The type of the command handler.</value>
    public override Type CommandHandlerType
    {
      get { return typeof(StyleCopNodeCommandHandler); }
    }

    #endregion Public Override Properties

    #region Public Override Methods

    /// <summary>
    /// Determines whether this instance can build node the specified dataType.
    /// </summary>
    /// <returns><c>true</c> if this instance can build node the specified dataType; otherwise, <c>false</c>.</returns>
    /// <param name="dataType">Data type.</param>
    public override bool CanBuildNode(Type dataType)
    {
      return typeof(ProjectFile).IsAssignableFrom(dataType) ||
        typeof(Project).IsAssignableFrom(dataType) ||
        typeof(ProjectFolder).IsAssignableFrom(dataType) ||
        typeof(Solution).IsAssignableFrom(dataType);
    }

    /// <summary>
    /// Releases all resource used by the <see cref="MonoDevelop.StyleCop.StyleCopNodeExtension"/> object.
    /// </summary>
    /// <remarks>Call <see cref="Dispose"/> when you are finished using the
    /// <see cref="MonoDevelop.StyleCop.StyleCopNodeExtension"/>. The <see cref="Dispose"/> method leaves the
    /// <see cref="MonoDevelop.StyleCop.StyleCopNodeExtension"/> in an unusable state. After calling
    /// <see cref="Dispose"/>, you must release all references to the
    /// <see cref="MonoDevelop.StyleCop.StyleCopNodeExtension"/> so the garbage collector can reclaim the memory that
    /// the <see cref="MonoDevelop.StyleCop.StyleCopNodeExtension"/> was occupying.</remarks>
    public override void Dispose()
    {
      IdeApp.Workspace.ItemRemovedFromSolution -= this.OnSolutionItemRemoved;
      IdeApp.Workspace.FileAddedToProject -= this.OnAddFile;
      IdeApp.Workspace.FileRemovedFromProject -= this.OnRemoveFile;
      base.Dispose();
    }

    /// <summary>
    /// Raises the node added event.
    /// </summary>
    /// <param name="dataObject">Data object.</param>
    public override void OnNodeAdded(object dataObject)
    {
      if (dataObject is Project)
      {
        ProjectUtilities.Instance.CachedProjects.AddProject(dataObject as Project);
      }
      else if (dataObject is ProjectFile)
      {
        ProjectUtilities.Instance.CachedProjects.AddFile(dataObject as ProjectFile);
      }
    }

    #endregion Public Override Methods

    #region Protected Override Methods

    /// <summary>
    /// Initialize this instance.
    /// </summary>
    protected override void Initialize()
    {
      base.Initialize();

      IdeApp.Workspace.ItemRemovedFromSolution += this.OnSolutionItemRemoved;
      IdeApp.Workspace.FileAddedToProject += this.OnAddFile;
      IdeApp.Workspace.FileRemovedFromProject += this.OnRemoveFile;
    }

    #endregion Protected Override Methods

    #region Private Methods

    /// <summary>
    /// Raises the solution item removed event.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="args">Arguments object.</param>
    private void OnSolutionItemRemoved(object sender, SolutionItemChangeEventArgs args)
    {
      if (args.SolutionItem != null && args.SolutionItem is Project)
      {
        ProjectUtilities.Instance.CachedProjects.RemoveProject(args.SolutionItem as Project);
      }
    }

    /// <summary>
    /// Raises the add file event.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="args">Arguments object.</param>
    private void OnAddFile(object sender, ProjectFileEventArgs args)
    {
      foreach (ProjectFileEventInfo e in args)
      {
        if (e.ProjectFile != null)
        {
          ProjectUtilities.Instance.CachedProjects.AddFile(e.ProjectFile);
        }
      }
    }

    /// <summary>
    /// Raises the remove file event.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="args">Arguments object.</param>
    private void OnRemoveFile(object sender, ProjectFileEventArgs args)
    {
      foreach (ProjectFileEventInfo e in args)
      {
        if (e.ProjectFile != null)
        {
          ProjectUtilities.Instance.CachedProjects.RemoveFile(e.ProjectFile, e.Project);
        }
      }
    }

    #endregion Private Methods
  }
}