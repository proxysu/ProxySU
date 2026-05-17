using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;

namespace ProxySuper.WPF.Views
{
    /// <summary>
    /// SingboxEditorView.xaml 的交互逻辑
    /// </summary>
    [MvxWindowPresentation(Identifier = nameof(SingboxEditorView), Modal = false)]
    public partial class SingboxEditorView : MvxWindow
    {
        public SingboxEditorView()
        {
            InitializeComponent();
        }
    }
}
