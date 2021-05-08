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
    public class SecondViewModel : MvxViewModel
    {
        private readonly IMvxNavigationService _navigationService;

        public SecondViewModel(IMvxNavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public string Message { get; set; } = "Hello world!";

        public IMvxCommand BackCommand => new MvxCommand(Back);

        public void Back()
        {
            _navigationService.Close(this);
        }
    }
}
