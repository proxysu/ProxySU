using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;

namespace ProxySuper.WPF.Views
{
    [MvxWindowPresentation]
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
