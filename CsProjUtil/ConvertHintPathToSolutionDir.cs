//------------------------------------------------------------------------------
// <copyright file="ConvertHintPathToSolutionDir.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace CsProjUtil
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ConvertHintPathToSolutionDir
    {
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertHintPathToSolutionDir"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private ConvertHintPathToSolutionDir(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(PackageGuids.guidCsProjUtilCmdSet, (int)PackageCommanddIDs.ConvertHintPathToSolutionDir);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ConvertHintPathToSolutionDir Instance
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
            Instance = new ConvertHintPathToSolutionDir(package);
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
            //HintPath の置換
            var itemGroups = root.Elements(nspace + "ItemGroup");
            var hintPaths = itemGroups.Elements(nspace + "Reference").Elements(nspace + "HintPath");
            foreach (var hintPath in hintPaths)
            {
                hintPath.Value = Regex.Replace(hintPath.Value, @"(\.\.\\)+", "$(SolutionDir)\\");
            }
            //ProjectReferenceの置換
            var projectReferences = root.Elements(nspace + "ProjectReference");
            foreach (var projectReference in projectReferences)
            {
                var attribute = projectReference.Attribute("Include");
                if (attribute != null)
                    attribute.Value = Regex.Replace(attribute.Value, @"(\.\.\\)+", "$(SolutionDir)\\");
            }
            //Target/Error の置換
            var errors = root.Elements(nspace + "Target").Elements(nspace + "Error");
            foreach (var error in errors)
            {
                var conditionAttribute = error.Attribute("Condition");
                if (conditionAttribute != null)
                    conditionAttribute.Value = Regex.Replace(conditionAttribute.Value, @"(\.\.\\)+", "$(SolutionDir)\\");

                var textAttribute = error.Attribute("Text");
                if (textAttribute != null)
                    textAttribute.Value = Regex.Replace(textAttribute.Value, @"(\.\.\\)+", "$(SolutionDir)\\");
            }
            //Importの置換
            var imports = root.Elements(nspace + "Import");
            foreach (var import in imports)
            {
                var attribute = import.Attribute("Project");
                if (attribute != null)
                    attribute.Value = Regex.Replace(attribute.Value, @"(\.\.\\)+", "$(SolutionDir)\\");

                var conditionAttribute = import.Attribute("Condition");
                if (conditionAttribute != null)
                    conditionAttribute.Value = Regex.Replace(conditionAttribute.Value, @"(\.\.\\)+", "$(SolutionDir)\\");
            }
            root.Save(file);
        }
    }
}
