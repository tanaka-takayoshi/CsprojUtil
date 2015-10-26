using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace CsProjUtil
{
    internal abstract class RunningDocumentTableEvents : IDisposable, IVsRunningDocTableEvents
    {
        private readonly IVsRunningDocumentTable rdt;

        private readonly uint sinkCookie;

        internal RunningDocumentTableEvents()
        {
            rdt = (IVsRunningDocumentTable)Package.GetGlobalService(typeof(SVsRunningDocumentTable));

            uint cookie;
            rdt.AdviseRunningDocTableEvents(this, out cookie);
            sinkCookie = cookie;
        }

        protected abstract void OnAfterSave(AfterSaveEventArgs e);

        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        int IVsRunningDocTableEvents.OnAfterSave(uint docCookie)
        {
            uint flags, readLocks, editLocks, itemId;
            string moniker;
            IVsHierarchy hierarchy;
            IntPtr docData;

            int hr = rdt.GetDocumentInfo(
                docCookie, out flags, out readLocks, out editLocks, out moniker,
                out hierarchy, out itemId, out docData);

            if (hr == VSConstants.S_OK)
            {
                var e = new AfterSaveEventArgs { FileName = moniker};
                this.OnAfterSave(e);
            }

            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }


        public void Dispose()
        {
            rdt.UnadviseRunningDocTableEvents(this.sinkCookie);
        }
    }
    
    public class AfterSaveEventArgs : EventArgs
    {
        public string FileName { get; set; }
    }
}