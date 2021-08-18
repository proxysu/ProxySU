using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;

namespace ProxySuper.WPF.Views
{
    /// <summary>
    /// BrookEditorView.xaml 的交互逻辑
    /// </summary>
    [MvxWindowPresentation(Identifier = nameof(XrayEditorView), Modal = false)]
    public partial class BrookEditorView : MvxWindow
    {
        public BrookEditorView()
        {
            InitializeComponent();
        }
    }
}
