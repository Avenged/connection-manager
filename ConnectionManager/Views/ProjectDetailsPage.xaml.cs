using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ConnectionManager.Services;
using ConnectionManager.UserControls;
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
    using Common.Models;
    using Microsoft.Toolkit.Mvvm.DependencyInjection;
    using System.Threading.Tasks;
    using Windows.UI.Popups;

    /// <summary>
    /// Una página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class ProjectDetailPage : Page
    {
        private readonly NavigationService _navigationService;

        public ProjectDetailsViewModel ViewModel { get; } = new();

        public ProjectDetailPage()
        {
            InitializeComponent();
            _navigationService = Ioc.Default.GetService<NavigationService>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //this.RegisterElementForConnectedAnimation("animationKeyContentGrid", itemHero);
            if (e.Parameter is Guid projectGuid)
            {
                await ViewModel.InitializeAsync(projectGuid);
            }
            else if (e.Parameter is Project project)
            {
                await ViewModel.InitializeAsync(project.Guid);
            }
        }

        public void OnEditClick(object sender, Guid environmentGuid)
        {
            _navigationService.Navigate<EditEnvironmentPage>(environmentGuid);
        }

        public async void OnActivateClick(object sender, Guid environmentGuid)
        {
            await ViewModel.OnActivateClickAsync(environmentGuid);
        }

        public async void OnDeleteClick(object sender, Guid environmentGuid)
        {
            var environmentListItem = sender as EnvironmentListItem;
            var dialog = new MessageDialog($"Are you sure to delete the environment {environmentListItem.EnvName}?", "Delete environment");
            dialog.Commands.Add(new UICommand("Accept", new UICommandInvokedHandler(CommandInvokedHandler), 0));
            dialog.Commands.Add(new UICommand("Cancel", new UICommandInvokedHandler(CommandInvokedHandler), 1));
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;

            await dialog.ShowAsync();

            async void CommandInvokedHandler(IUICommand command)
            {
                if ((int)command.Id == 0)
                {
                    environmentListItem.IsEnabled = false;
                    await ViewModel.OnDeleteEnvironment(environmentGuid);
                }
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                //NavigationService.Frame.SetListDataItemForNextConnectedAnimation(ViewModel.Item);
            }
        }
    }
}
