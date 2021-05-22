using Microsoft.Win32;
using MvvmCross.ViewModels;
using ProxySuper.Core.Models.Projects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using QRCoder;

namespace ProxySuper.Core.ViewModels
{
    public class XrayConfigViewModel : MvxViewModel<XraySettings>
    {

        public XraySettings Settings { get; set; }

        public override void Prepare(XraySettings parameter)
        {
            Settings = parameter;
        }


    }
}
