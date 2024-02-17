using EasySaveV2.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using Newtonsoft.Json;
using System.Windows.Controls;
using System.Xml;

namespace EasySaveV2.MVVM.ViewModels
{
    public class DashboardViewModels
    {
        public class Backup
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Source { get; set; }
            public string Target { get; set; }
            public int Type { get; set; }
        }
    }
}