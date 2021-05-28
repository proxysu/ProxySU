using MvvmCross.Platforms.Wpf.Views;
using ProxySuper.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProxySuper.WPF.Views
{
    /// <summary>
    /// TrojanGoConfigView.xaml 的交互逻辑
    /// </summary>
    public partial class TrojanGoConfigView : MvxWindow
    {
        public TrojanGoConfigView()
        {
            InitializeComponent();
        }

        public new TrojanGoConfigViewModel ViewModel
        {
            get
            {
                return (TrojanGoConfigViewModel)base.ViewModel;
            }
        }
    }
}
