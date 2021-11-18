using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Text;
// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=234238

namespace ConnectionManager.Views
{
    using Common.Models;
    using ConnectionManager.Services;
    using ConnectionManager.ViewModels;
    using Microsoft.Toolkit.Mvvm.DependencyInjection;

    /// <summary>
    /// Una página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class EditEnvironmentPage : Page
    {
        private readonly NavigationService _navigationService;

        public EditEnvironmentViewModel ViewModel { get; } = new();

        public EditEnvironmentPage()
        {
            InitializeComponent();
            Loaded += EditEnvironmentPage_Loaded;
            _navigationService = Ioc.Default.GetService<NavigationService>();
        }

        private void EditEnvironmentPage_Loaded(object sender, RoutedEventArgs e)
        {
            ProjectName = ViewModel.Project.Name;
            var text = ViewModel.Environment.ConnectionStrings ?? string.Empty;
            ConnStrsRichEditBox.Document.SetText(TextSetOptions.None, text);
            NameTextBox.Focus(FocusState.Programmatic);
        }

        public void OnSaveClick()
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
            var environmentListItem = d as EditEnvironmentPage;
            environmentListItem.OnProjectNameChanged(e);
        }

        public static readonly DependencyProperty ProjectNameProperty =
            DependencyProperty.Register("ProjectName", typeof(string), typeof(EditEnvironmentPage), new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnProjectNameChanged)));

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Guid environmentGuid)
            {
                await ViewModel.InitializeAsync(environmentGuid);
            }
        }
    }
}
