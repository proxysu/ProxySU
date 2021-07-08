using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;
using System;

namespace ProxySuper.WPF.Views
{
    /// <summary>
    /// XrayEditorView.xaml 的交互逻辑
    /// </summary>
    [MvxWindowPresentation(Identifier = nameof(XrayEditorView), Modal = false)]
    public partial class XrayEditorView : MvxWindow
    {
        public XrayEditorView()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }
    }
}
