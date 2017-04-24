using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using Microsoft.Xrm.Client.Windows.Controls.ConnectionDialog;
using System.Windows.Forms.Design;
using System.Windows.Forms;

namespace Xrm.Framework.CI.TeamFoundation.Activities
{
    public class CrmConnectionEditor : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(
                typeof(IWindowsFormsEditorService)
            );

            if (editorService != null)
            {

                ConnectionDialog dialog = new ConnectionDialog();

                if (value != null)
                {
                    dialog.ConnectionString = value.ToString();
                }

                bool? connected = dialog.ShowDialog();

                if (connected.HasValue && connected.Value)
                {
                    value = dialog.ConnectionString;
                }
            }

            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}
