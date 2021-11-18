using Microsoft.Extensions.DependencyInjection;
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

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=234238

namespace ConnectionManager.Views
{
    using Common.Models;
    using ConnectionManager.Services;
    using Microsoft.Toolkit.Mvvm.DependencyInjection;
    using Windows.UI.Text;

    /// <summary>
    /// Una página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class CreateEnvironmentPage : Page
    {
        private readonly NavigationService _navigationService;

        public CreateEnvironmentViewModel ViewModel { get; } = new();

        public CreateEnvironmentPage()
        {
            InitializeComponent();
            Loaded += CreateEnvironmentPage_Loaded;
            _navigationService = Ioc.Default.GetService<NavigationService>();
        }

        private void CreateEnvironmentPage_Loaded(object sender, RoutedEventArgs e)
        {
            NameTextBox.Focus(FocusState.Programmatic);
        }

        public void OnCreateClick()
        {
            CreateButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
            NameTextBox.IsEnabled = false;
            DescriptionTextBox.IsEnabled = false;
            ConnStrsRichEditBox.IsEnabled = false;

            ConnStrsRichEditBox.Document.GetText(TextGetOptions.None,
                out string connectionStrings);

            ViewModel.Environment.ConnectionStrings = connectionStrings.Trim();

            ViewModel.CreateClickCommand.Execute(null);
        }

        public void OnCancelClick()
        {
            _navigationService.GoBack();
        }

        public string ProjectName
        {
            get { return (string)GetValue(ProjectNameProperty); }
            set { SetValue(ProjectNameProperty, value); }
        }

        private void OnProjectNameChanged(DependencyPropertyChangedEventArgs e)
            => ProjectNameTextBox.Text = e.NewValue.ToString();

        private static void OnProjectNameChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            var environmentListItem = d as CreateEnvironmentPage;
            environmentListItem.OnProjectNameChanged(e);
        }

        public static readonly DependencyProperty ProjectNameProperty =
            DependencyProperty.Register("ProjectName", typeof(string), typeof(CreateEnvironmentPage), new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnProjectNameChanged)));

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Guid projectGuid)
            {
                await ViewModel.InitializeAsync(projectGuid);
            }

            ProjectName = ViewModel.Project.Name;
        }
    }
}
