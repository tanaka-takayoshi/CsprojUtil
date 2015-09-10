//------------------------------------------------------------------------------
// <copyright file="AddConfigTransformCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace CsProjUtil
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class AddConfigTransformCommand
    {
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        

        /// <summary>
        /// Initializes a new instance of the <see cref="AddConfigTransformCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private AddConfigTransformCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(PackageGuids.guidCsProjUtilCmdSet, (int)PackageCommanddIDs.AddConfigTransform);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static AddConfigTransformCommand Instance
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
            Instance = new AddConfigTransformCommand(package);
        }

        static readonly string replacedText = @"<UsingTask TaskName=""TransformXml"" AssemblyFile=""$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\Web\Microsoft.Web.Publishing.Tasks.dll"" />
  <Target Name=""AfterCompile"" Condition=""Exists('App.$(Configuration).config')"">
    <!--Generate transformed app config in the intermediate directory-->
    <TransformXml Source=""App.config"" Destination=""$(IntermediateOutputPath)$(TargetFileName).config"" Transform=""App.$(Configuration).config"" />
    <!--Force build process to use the transformed configuration file from now on.-->
    <ItemGroup>
      <AppConfigWithTargetPath Remove=""App.config"" />
      <AppConfigWithTargetPath Include=""$(IntermediateOutputPath)$(TargetFileName).config"">
        <TargetPath>$(TargetFileName).config</TargetPath>
      </AppConfigWithTargetPath>
    </ItemGroup>
  </Target>
  <!--Override After Publish to support ClickOnce AfterPublish. Target replaces the untransformed config file copied to the deployment directory with the transformed one.-->
  <Target Name=""AfterPublish"">
    <PropertyGroup>
      <DeployedConfig>$(_DeploymentApplicationDir)$(TargetName)$(TargetExt).config$(_DeploymentFileMappingExtension)</DeployedConfig>
    </PropertyGroup>
    <!--Publish copies the untransformed App.config to deployment directory so overwrite it-->
    <Copy Condition=""Exists('$(DeployedConfig)')"" SourceFiles=""$(IntermediateOutputPath)$(TargetFileName).config"" DestinationFiles=""$(DeployedConfig)"" />
  </Target>";

        private static XElement insertElement = XElement.Parse(@"<dummy xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <!-- ****************************************** -->
  <!-- Begin App.config Transform Settings        -->
  <!-- VSなど、BuildTargetを渡さずビルドした時専用         -->  
  <!-- ****************************************** -->  
  <UsingTask TaskName=""TransformXml"" AssemblyFile=""$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\Web\Microsoft.Web.Publishing.Tasks.dll"" />
  <Target Name=""AfterCompile"" Condition=""Exists('app.$(Configuration).config') And '$(BuildTarget)'==''"">
    <!--Generate transformed app config in the intermediate directory-->
    <TransformXml Source=""app.config"" Destination=""$(IntermediateOutputPath)$(TargetFileName).config"" Transform=""app.$(Configuration).config"" />
    <!--Force build process to use the transformed configuration file from now on.-->
    <ItemGroup>
      <AppConfigWithTargetPath Remove=""app.config"" />
      <AppConfigWithTargetPath Include=""$(IntermediateOutputPath)$(TargetFileName).config"">
        <TargetPath>$(TargetFileName).config</TargetPath>
      </AppConfigWithTargetPath>
    </ItemGroup>
  </Target>
  <!--Override After Publish to support ClickOnce AfterPublish. Target replaces the untransformed config file copied to the deployment directory with the transformed one.-->
  <Target Name=""AfterPublish"">
    <PropertyGroup>
      <DeployedConfig>$(_DeploymentApplicationDir)$(TargetName)$(TargetExt).config$(_DeploymentFileMappingExtension)</DeployedConfig>
    </PropertyGroup>
    <!--Publish copies the untransformed App.config to deployment directory so overwrite it-->
    <Copy Condition=""Exists('$(DeployedConfig)')"" SourceFiles=""$(IntermediateOutputPath)$(TargetFileName).config"" DestinationFiles=""$(DeployedConfig)"" />
  </Target>
  <!-- ****************************************** -->
  <!-- Begin App.config Custom Transform Settings -->
  <!-- MSBuild で /p:BuildTarget=Xxx を渡した時専用 -->
  <!-- ****************************************** -->  
  <Target Name=""AfterCompile"" Condition=""Exists('app.$(BuildTarget).config') And '$(BuildTarget)'!=''"">
    <!--Generate transformed app config in the intermediate directory-->
    <TransformXml Source=""app.config"" Destination=""$(IntermediateOutputPath)$(TargetFileName).config"" Transform=""app.$(BuildTarget).config"" />
    <!--Force build process to use the transformed configuration file from now on.-->
    <ItemGroup>
      <AppConfigWithTargetPath Remove=""app.config"" />
      <AppConfigWithTargetPath Include=""$(IntermediateOutputPath)$(TargetFileName).config"">
        <TargetPath>$(TargetFileName).config</TargetPath>
      </AppConfigWithTargetPath>
    </ItemGroup>
  </Target>
  <!-- ここで↑で生成した $(IntermediateOutputPath) (つまりobj/$(Configuration)) にあるコピー元となる -sc.App.Config を差し替え-->
  <!-- -sc.App.Config が元となって、bin/$(Configuration) の exe.config が差し変わる -->
  <!-- -sc.App.Config が .exe.config より新しいかも判定して MSBuild はコピーしてるので、両方差し替えるの雑だけど安定 -->
  <Target Name=""AfterBuild"" Condition=""Exists('app.$(BuildTarget).config') And '$(BuildTarget)'!=''"">
    <Delete Files=""$(IntermediateOutputPath)$(ProjectFileName)-sc.App.config"" />
    <Copy SourceFiles=""$(IntermediateOutputPath)$(TargetFileName).config""
          DestinationFiles=""$(OutDir)$(TargetFileName).config"" />
    <Copy SourceFiles=""$(IntermediateOutputPath)$(TargetFileName).config""
          DestinationFiles=""$(IntermediateOutputPath)$(ProjectFileName)-sc.App.config"" />    
  </Target>
  <!-- ****************************************** -->
  <!-- End App.config Custom Transform Settings   -->
  <!-- ****************************************** -->
</dummy>", LoadOptions.PreserveWhitespace);

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
            
            var content = File.ReadAllText(file);
            var removed = content.Replace(replacedText, "");

            var root = XElement.Parse(removed);
            var nspace = root.Name.Namespace;

            if (root.Nodes().All(n => n.ToString() != "<!-- Begin App.config Transform Settings        -->"))
            {
                root.Add(insertElement.Nodes());
            }

            root.Save(file);
        }
    }
}
