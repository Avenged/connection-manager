using System;
using System.Security.Principal;
using ConnectionManager.ViewModels;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml.Controls;

namespace ConnectionManager.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainPage()
        {
            InitializeComponent();
        }

        private async void ElevateButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("Admin");
            }
        }
    }
}
