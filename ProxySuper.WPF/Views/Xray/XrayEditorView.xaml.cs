using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;
using System;

namespace ProxySuper.WPF.Views
{
    [MvxWindowPresentation]
    public partial class XrayEditorView : MvxWindow
    {
        public XrayEditorView()
        {
            InitializeComponent();
        }
    }
}
