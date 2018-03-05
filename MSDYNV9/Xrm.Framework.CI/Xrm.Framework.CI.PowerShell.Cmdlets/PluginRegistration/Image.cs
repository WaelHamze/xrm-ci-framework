using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xrm.Framework.CI.PowerShell.Cmdlets
{
    public class Image
    {
        public Guid Id { get; set; }

        public string Attributes { get; set; }

        public string EntityAlias { get; set; }

        public string MessagePropertyName { get; set; }

        public int? ImageType { get; set; }
    }
}
