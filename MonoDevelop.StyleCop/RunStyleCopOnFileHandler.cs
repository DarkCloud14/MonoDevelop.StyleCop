namespace MonoDevelop.StyleCop
{
    using System;
    using System.Collections.Generic;

    using Microsoft.StyleCop;

    using MonoDevelop.Components.Commands;
    using MonoDevelop.Ide;

    /// <summary>
    /// Class used to handle a single file from Stylecop
    /// </summary>
    public class RunStyleCopOnFileHandler : RunStyleCopHandler
    {
        #region Methods

        /// <summary>
        /// Give list of files and projects to be treated by Stylecop
        /// </summary>
        /// <param name="console">
        /// A <see cref="StyleCopConsole"/> which define the console which will treat the files
        /// </param>
        /// <returns>
        /// A <see cref="List<CodeProject>"/> which contains all the project to be treated by stylecop
        /// </returns>
        protected override List<CodeProject> GetCodeProjectList(StyleCopConsole console)
        {
            Configuration configuration = new Configuration(new string[] { "DEBUG" });
            List<CodeProject> projects = new List<CodeProject>();
            CodeProject project = new CodeProject(IdeApp.ProjectOperations.CurrentSelectedProject.BaseDirectory.GetHashCode(), IdeApp.ProjectOperations.CurrentSelectedProject.BaseDirectory, configuration);

            // Add each source file to this project.
            console.Core.Environment.AddSourceCode(project, IdeApp.Workbench.ActiveDocument.FileName, null);
            projects.Add(project);

            return projects;
        }

        /// <summary>
        /// Update availability of command
        /// </summary>
        /// <param name="info">
        /// A <see cref="CommandInfo"/>
        /// </param>
        protected override void Update(CommandInfo info)
        {
            base.Update(info);
            MonoDevelop.Ide.Gui.Document document = IdeApp.Workbench.ActiveDocument;
            info.Enabled = document != null && System.IO.Path.GetExtension(document.FileName) == ".cs";
        }

        #endregion Methods
    }
}