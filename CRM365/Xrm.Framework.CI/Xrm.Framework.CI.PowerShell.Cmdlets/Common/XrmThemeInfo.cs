using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xrm.Framework.CI.PowerShell.Cmdlets.Common
{
    public class XrmThemeInfo
    {
        public Guid Id;
        public string Name;
        public Guid LogoId;
        public string HeaderColor { get; set; }
        public string HoverLinkEffect { get; set; }
        public string LogoToolTip { get; set; }
        public string MainColor { get; set; }
        public string NavBarBackgroundColor { get; set; }
        public string NavBarShelfColor { get; set; }
        public string PageHeaderBackgroundColor { get; set; }
        public string PanelHeaderBackgroundColor { get; set; }
        public string ProcessControlColor { get; set; }
        public string SelectedLinkEffect { get; set; }
        public string AccentColor { get; set; }
        public string BackgroundColor { get; set; }
        public string ControlBorder { get; set; }
        public string ControlShade { get; set; }
        public string DefaultCustomEntityColor { get; set; }
        public string DefaultEntityColor { get; set; }
        public string GlobalLinkColor { get; set; }
    }
}
