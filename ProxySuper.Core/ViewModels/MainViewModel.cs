using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxySuper.Core.ViewModels
{
    public class MainViewModel : MvxViewModel
    {
        private readonly IMvxNavigationService _navigationService;

        public MainViewModel(IMvxNavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        private int _count = 1;

        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                SetProperty(ref _count, value);
            }
        }

        public IMvxCommand PlusCommand => new MvxCommand(Plus);

        public void Plus()
        {
            this.Count++;

            if (this.Count >= 2)
            {
                _navigationService.Navigate<SecondViewModel>();
            }
        }
    }
}
