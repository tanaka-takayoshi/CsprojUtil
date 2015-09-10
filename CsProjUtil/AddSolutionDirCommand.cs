//------------------------------------------------------------------------------
// <copyright file="AddSolutionDirCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace CsProjUtil
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class AddSolutionDirCommand
    {
        private DTE2 dte;

        internal DTE2 DTE => dte ?? (dte = ServiceProvider.GetService(typeof (DTE)) as DTE2);

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddSolutionDirCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private AddSolutionDirCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(PackageGuids.guidCsProjUtilCmdSet, (int)PackageCommanddIDs.AddSolutionDir);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static AddSolutionDirCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new AddSolutionDirCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var file = CsProjUtil.GetPathOfSelectedItem();
            var root = XElement.Load(file);
            var nspace = root.Name.Namespace;

            var hasSolutionDir = root
                .Elements(nspace + "PropertyGroup")
                .Any(p => p.Elements(nspace + "SolutionDir").Any());
            if (hasSolutionDir)
            {
                VsShellUtilities.ShowMessageBox(
                    this.ServiceProvider,
                    "SolutionDir is already defined in csproj.",
                    "Nothing to be changed.",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
            else
            {
                var target = root.Elements(nspace + "PropertyGroup")
                                .First(p => !p.Attributes(nspace + "Condition").Any());
                var solutionDir = new XElement(nspace + "SolutionDir");
                solutionDir.SetAttributeValue("Condition", "$(SolutionDir) == '' Or $(SolutionDir) == '*Undefined*'");
                solutionDir.SetValue(@"..\");
                target.Add(solutionDir);
                root.Save(file);
            }
        }
    }
}
