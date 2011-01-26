namespace MonoDevelop.StyleCop
{
    using System;
    using System.Collections.Generic;

    using Microsoft.StyleCop;

    using MonoDevelop.Components.Commands;
    using MonoDevelop.Ide;
    using MonoDevelop.Ide.Gui;
    using MonoDevelop.Ide.Gui.Pads;
    using MonoDevelop.Ide.Tasks;

    /// <summary>
    /// Class used to handle Stylecop functions
    /// </summary>
    public abstract class RunStyleCopHandler : CommandHandler
    {
        #region Fields

        /// <summary>
        /// Static object defining ownership of Task objects
        /// </summary>
        private static object styleCopAddinOwner = new object();

        /// <summary>
        /// Pad containing errors
        /// </summary>
        private ErrorListPad errorPad = null;

        /// <summary>
        /// Output logger of Stylecop messages
        /// </summary>
        private System.IO.TextWriter logger = null;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the RunStyleCopHandler class.
        /// </summary>
        public RunStyleCopHandler()
        {
            Pad tmpPad = IdeApp.Workbench.Pads.Find(
                  delegate(Pad toCheck)
                  {
                    return toCheck.Id.Equals("MonoDevelop.Ide.Gui.Pads.ErrorListPad");
                  });
            if (tmpPad != null)
            {
                this.errorPad = tmpPad.Content as ErrorListPad;
            }

            this.logger = this.errorPad.GetBuildProgressMonitor().Log;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Run the stylecop analyse on the file
        /// </summary>
        protected override void Run()
        {
            base.Run();
            IdeApp.Workbench.ActiveDocument.Save();

            TaskService.Errors.ClearByOwner(RunStyleCopHandler.styleCopAddinOwner);
            this.errorPad.ShowResults(this, new EventArgs());

            StyleCopConsole console = new StyleCopConsole(null, true, null, null, true);
            
            List<CodeProject> projects = this.GetCodeProjectList(console);

            console.OutputGenerated += this.OnOutputGenerated;
            console.ViolationEncountered += this.OnViolationEncountered;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            console.Start(projects, false);
            console.OutputGenerated -= this.OnOutputGenerated;
            console.ViolationEncountered -= this.OnViolationEncountered;
            console.Dispose();
        }
        
        /// <summary>
        /// Give list of files and projects to be treated by Stylecop
        /// </summary>
        /// <param name="console">
        /// A <see cref="StyleCopConsole"/> which defines the console which will treat the files
        /// </param>
        /// <returns>
        /// A <see cref="List<CodeProject>"/> which contains all the project to be treated by stylecop
        /// </returns>
        protected abstract List<CodeProject> GetCodeProjectList(StyleCopConsole console);

        /// <summary>
        /// Generate an output message for Stylecop informations messages
        /// </summary>
        /// <param name="sender">
        /// A <see cref="System.Object"/> determining which object sends this output message
        /// </param>
        /// <param name="args">
        /// A <see cref="OutputEventArgs"/> determining elements composing the message
        /// </param>
        private void OnOutputGenerated(object sender, OutputEventArgs args)
        {
            // For the moment, Nothing to do
            if (this.logger != null)
            {
                this.logger.WriteLine(args.Output);
            }
        }

        /// <summary>
        /// Reports violations encountered by stylecop to the error pad
        /// </summary>
        /// <param name="sender">
        /// A <see cref="System.Object"/> determining which object sends this violation
        /// </param>
        /// <param name="args">
        /// A <see cref="ViolationEventArgs"/> determining elements composing the violation
        /// </param>
        private void OnViolationEncountered(object sender, ViolationEventArgs args)
        {
            try
            {
            MonoDevelop.Projects.BuildError b = new MonoDevelop.Projects.BuildError(args.Element.Document.SourceCode.Path, args.LineNumber, 0, args.Violation.Rule.CheckId, args.Message);
            b.IsWarning = args.Warning;
            Task errorTask = new Task(b, RunStyleCopHandler.styleCopAddinOwner);

            this.errorPad.AddTask(errorTask);
            }
            catch (Exception e)
            {
                this.logger.WriteLine("Error: {0}\nStackTrace:{1}", e.Message, e.StackTrace);
            }
        }

        #endregion Methods
    }
}