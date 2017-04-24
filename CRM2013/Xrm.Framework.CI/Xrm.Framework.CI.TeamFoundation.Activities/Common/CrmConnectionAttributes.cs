using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xrm.Framework.CI.Common
{
    public class CrmConnectionAttributes
    {
        public string Port { get; set; }

        public string Protocol { get; set; }

        public string Domain { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
