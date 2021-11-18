using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ConnectionManager.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ConnectionManager.Views
{
    /// <summary>
    /// Una página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class EnvironmentDetailsPage : Page
    {
        public EnvironmentDetailsViewModel ViewModel { get; } = new EnvironmentDetailsViewModel();

        public EnvironmentDetailsPage()
        {
            this.InitializeComponent();
            Loaded += EnvironmentDetailsPage_Loaded;
        }

        private void EnvironmentDetailsPage_Loaded(object sender, RoutedEventArgs e)
        {
            var connStrs = ViewModel.Environment.ConnectionStrings ?? string.Empty;
            ConnStrsRichEditBox.Document.SetText(Windows.UI.Text.TextSetOptions.None,
                connStrs);
            ConnStrsRichEditBox.IsReadOnly = true;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Guid environmentGuid)
            {
                await ViewModel.InitializeAsync(environmentGuid);
            }
        }
    }
}
