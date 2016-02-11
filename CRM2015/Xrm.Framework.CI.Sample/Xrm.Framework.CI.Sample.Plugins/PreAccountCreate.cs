using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xrm.Framework.CI.Sample.Plugins
{
    public class PreAccountCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            throw new InvalidPluginExecutionException("Sample Account Plug-in");
        }
    }
}
