//Review: 17/02/15
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PatternsPluginsCommon
{
    public interface IPluginTask
    {
        bool Init(string pluginsServiceURL);
        bool Start();
        bool Stop();
        bool Apply(XElement request, string requestName, XElement response);
        void UpdateDowntime(bool isInDowntime, bool isAfterDowntime);
        string Name();
        bool IsEnabled();
        
    }
}
