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

namespace ProxySU_Core.Views
{
    /// <summary>
    /// TextBoxWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TextBoxWindow
    {
        public string Message { get; set; }

        public TextBoxWindow(string title, string message)
        {
            InitializeComponent();
            this.Title = title;
            this.Message = message;

            this.DataContext = this;
        }
    }
}
