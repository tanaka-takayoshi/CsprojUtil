using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace CsProjUtil
{
    public static class CsProjUtil
    {
        public static string GetPathOfSelectedItem()
        {
            var monitorSelection = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;

            IVsMultiItemSelect multiItemSelect = null;
            var hierarchyPtr = IntPtr.Zero;
            var selectionContainerPtr = IntPtr.Zero;
            var hr = VSConstants.S_OK;
            var itemid = VSConstants.VSITEMID_NIL;

            hr = monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemid, out multiItemSelect, out selectionContainerPtr);

            var hierarchy = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy;
            var uiHierarchy = hierarchy as IVsUIHierarchy;

            // Get the file path
            string projFilePath = null;
            ((IVsProject)hierarchy).GetMkDocument(itemid, out projFilePath);
            return projFilePath;
        }

        public static T GetProperty<T>(Project project, string index) where T : class
        {
            try
            {
                var assemblyName = project.Properties.Item(index).Value as T;
                return assemblyName;
            }
            catch (Exception)
            {
                try
                {
                    var properties = project.ConfigurationManager?.ActiveConfiguration?.Properties;
                    return properties?.Item(index).Value as T;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}
