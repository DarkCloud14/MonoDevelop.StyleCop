//-----------------------------------------------------------------------
// <copyright file="StyleCopNodeExtension.cs">
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
  using MonoDevelop.Ide;
  using MonoDevelop.Ide.Gui.Components;
  using MonoDevelop.Ide.Gui.Pads.ProjectPad;
  using MonoDevelop.Projects;

  internal class StyleCopNodeExtension : NodeBuilderExtension
  {
    SolutionItemChangeEventHandler solutionItemRemovedHandler;
    ProjectFileEventHandler fileAddedHandler;
    ProjectFileEventHandler fileRemovedHandler;

    public override void Dispose()
    {
      IdeApp.Workspace.ItemRemovedFromSolution -= solutionItemRemovedHandler;
      IdeApp.Workspace.FileAddedToProject -= fileAddedHandler;
      IdeApp.Workspace.FileRemovedFromProject -= fileRemovedHandler;
      base.Dispose();
    }

    protected override void Initialize()
    {
      base.Initialize();

      solutionItemRemovedHandler = (SolutionItemChangeEventHandler)DispatchService.GuiDispatch(new SolutionItemChangeEventHandler(OnSolutionItemRemoved));
      fileAddedHandler = (ProjectFileEventHandler)DispatchService.GuiDispatch(new ProjectFileEventHandler(OnAddFile));
      fileRemovedHandler = (ProjectFileEventHandler)DispatchService.GuiDispatch(new ProjectFileEventHandler(OnRemoveFile));

      IdeApp.Workspace.ItemRemovedFromSolution += solutionItemRemovedHandler;
      IdeApp.Workspace.FileAddedToProject += fileAddedHandler;
      IdeApp.Workspace.FileRemovedFromProject += fileRemovedHandler;
    }

    public override bool CanBuildNode(Type dataType)
    {
      return typeof(ProjectFile).IsAssignableFrom(dataType) ||
        typeof(Project).IsAssignableFrom(dataType) ||
        typeof(ProjectFolder).IsAssignableFrom(dataType) ||
        typeof(Solution).IsAssignableFrom(dataType);
    }

    public override Type CommandHandlerType
    {
      get { return typeof(StyleCopNodeCommandHandler); }
    }

    void OnSolutionItemRemoved(object sender, SolutionItemChangeEventArgs args)
    {
      if (args.SolutionItem != null && args.SolutionItem is Project)
      {
        ProjectUtilities.Instance.CachedProjects.RemoveProject(args.SolutionItem as Project);
      }
    }

    void OnAddFile(object sender, ProjectFileEventArgs args)
    {
      foreach (ProjectFileEventInfo e in args)
      {
        if (e.ProjectFile != null)
        {
          ProjectUtilities.Instance.CachedProjects.AddFile(e.ProjectFile);
        }
      }
    }

    void OnRemoveFile(object sender, ProjectFileEventArgs args)
    {
      foreach (ProjectFileEventInfo e in args)
      {
        if (e.ProjectFile != null)
        {
          ProjectUtilities.Instance.CachedProjects.RemoveFile(e.ProjectFile, e.Project);
        }
      }
    }

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
  }
}