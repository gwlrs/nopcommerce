using Nop.Services.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Nop.Plugin.CustomPayments.Turkey
{
    public class PaymentForTurkey : IPlugin
    {
        public PluginDescriptor PluginDescriptor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string GetConfigurationPageUrl()
        {
            throw new NotImplementedException();
        }

        public void Install()
        {
            Debug.WriteLine("Install Completed !");
        }

        public void PreparePluginToUninstall()
        {
            
        }

        public void Uninstall()
        {
            Debug.WriteLine("UnInstall Completed !");

        }

        public void Update(string currentVersion, string targetVersion)
        {
            Debug.WriteLine("Update Completed !");

        }
    }
}
