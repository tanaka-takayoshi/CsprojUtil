using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editors.PropertyPages;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace CsProjUtil
{
    [ComVisible(true)]
    [Guid("A730A448-3E8B-4889-A233-AB0E0463E8AA")]
    [ProvideObject(typeof(CsProjUtilPropertyPageProvider))]
    public class CsProjUtilPropertyPageProvider : PropPageBase
    {
        private CsProjUtilPropertyPageControl control;

        protected override Control CreateControl() => control ?? (control = new CsProjUtilPropertyPageControl());
        protected override Type ControlType => typeof(CsProjUtilPropertyPageControl);
        protected override string Title => "CsProjUtil Property Page";
    }
}
