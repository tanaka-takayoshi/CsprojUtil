using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Editors.PropertyPages;

namespace CsProjUtil
{
    public class CsProjUtilPropertyPageControl : PropPageUserControlBase
    {
        private System.Windows.Forms.CheckBox checkBox1;

        private void InitializeComponent()
        {
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(21, 21);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(80, 16);
            this.checkBox1.TabIndex = 0;
            this.checkBox1.Text = "checkBox1";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // CsProjUtilPropertyPageControl
            // 
            this.Controls.Add(this.checkBox1);
            this.Name = "CsProjUtilPropertyPageControl";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
