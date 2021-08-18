using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;

namespace ProxySuper.WPF.Views
{
    /// <summary>
    /// TrojanEditorView.xaml 的交互逻辑
    /// </summary>
    [MvxWindowPresentation(Identifier = nameof(TrojanGoEditorView), Modal = false)]
    public partial class TrojanGoEditorView : MvxWindow
    {
        public TrojanGoEditorView()
        {
            InitializeComponent();
        }
    }
}
