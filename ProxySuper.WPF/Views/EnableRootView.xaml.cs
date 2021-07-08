using MvvmCross.Platforms.Wpf.Views;

namespace ProxySuper.WPF.Views
{
    /// <summary>
    /// EnableRootView.xaml 的交互逻辑
    /// </summary>
    public partial class EnableRootView : MvxWindow
    {
        public EnableRootView()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            OutputText.ScrollToEnd();
        }
    }
}
