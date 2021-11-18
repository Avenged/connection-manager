using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI.Animations;

using ConnectionManager.Services;
using ConnectionManager.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace ConnectionManager.Views
{
    public sealed partial class ContentGridDetailPage : Page
    {
        private readonly NavigationService _navigationService;

        public ContentGridDetailViewModel ViewModel { get; } = new ContentGridDetailViewModel();

        public ContentGridDetailPage()
        {
            InitializeComponent();
            _navigationService = Ioc.Default.GetService<NavigationService>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.RegisterElementForConnectedAnimation("animationKeyContentGrid", itemHero);
            if (e.Parameter is long orderID)
            {
                await ViewModel.InitializeAsync(orderID);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                _navigationService.Frame.SetListDataItemForNextConnectedAnimation(ViewModel.Item);
            }
        }
    }
}
