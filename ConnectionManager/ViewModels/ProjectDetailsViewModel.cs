using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ConnectionManager.Common.Models;
using ConnectionManager.Common.Services;
using ConnectionManager.Services;
using ConnectionManager.Views;

namespace ConnectionManager.ViewModels
{
    using AutoMapper;
    using Common.Models;
    using ConnectionManager.Common;
    using ConnectionManager.Filesystem;
    using Microsoft.Toolkit.Mvvm.DependencyInjection;
    using Microsoft.Toolkit.Uwp.Notifications;
    using Windows.Foundation.Collections;
    using Windows.UI.Notifications;
    using Windows.UI.Popups;

    public class ProjectDetailsViewModel : ObservableObject
    {
        private readonly NavigationService _navigationService;
        private readonly ProjectService _projectService;
        private readonly EnvironmentService _environmentService;
        private readonly ActivedEnvironmentService _activedEnvironmentService;
        private Project _project;
        private ActivedEnvironment _activedEnvironment;
        private readonly IMapper _mapper;
        private ICommand _itemClickCommand;
        private ICommand _restartServiceClickCommand;
        private ICommand _editClickCommand;
        private ICommand _createEnvironmentClickCommand;
        private ICommand _deleteProjectClickCommand;

        public ICommand DeleteProjectClickCommand => _deleteProjectClickCommand ??= new RelayCommand<EnvironmentDO>(OnDeleteProjectClick);
        public ICommand ItemClickCommand => _itemClickCommand ??= new RelayCommand<EnvironmentDO>(OnItemClick);
        public ICommand RestartServiceClickCommand => _restartServiceClickCommand ??= new RelayCommand<EnvironmentDO>(OnRestartServiceClick);
        public ICommand CreateEnvironmentClickCommand => _createEnvironmentClickCommand ??= new RelayCommand(OnCreateEnvironmentClick);
        public ICommand EditClickCommand => _editClickCommand ??= new RelayCommand(OnEditClick);

        public ObservableCollection<EnvironmentDO> Source { get; } = new ObservableCollection<EnvironmentDO>();

        public ActivedEnvironment ActivedEnvironment
        {
            get { return _activedEnvironment; }
            set { SetProperty(ref _activedEnvironment, value); }
        }

        public Project Project
        {
            get { return _project; }
            set { SetProperty(ref _project, value); }
        }

        private async void OnDeleteProjectClick(EnvironmentDO obj)
        {
            var dialog = new MessageDialog($"Are you sure to delete the project {Project.Name} - {Project.Description}?", "Delete project");
            dialog.Commands.Add(new UICommand("Accept", new UICommandInvokedHandler(CommandInvokedHandler), 0));
            dialog.Commands.Add(new UICommand("Cancel", new UICommandInvokedHandler(CommandInvokedHandler), 1));
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;

            await dialog.ShowAsync();

            async void CommandInvokedHandler(IUICommand command)
            {
                if ((int)command.Id == 0)
                {
                    await _activedEnvironmentService.DeleteByProjectGuidAsync(Project.Guid);
                    await _environmentService.DeleteAllByProjectGuidAsync(Project.Guid);
                    await _projectService.DeleteByGuidAsync(Project.Guid);
                    
                    _navigationService.Navigate<ProjectsPage>();
                }
            }
        }

        private async void OnRestartServiceClick(EnvironmentDO obj)
        {
            GenericOperationResult genericOpReuslt;
            FilesystemOperations operations = new();
            FilesystemResult fsResult;
            MessageDialog dialog;

            string serviceName = _project.ServiceName;

            if (string.IsNullOrEmpty(serviceName))
            {
                dialog = new("First you need to set the service name", "Configuration Required");
                await dialog.ShowAsync();
                return;
            }

            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = $"The service {serviceName} will be restarted."
                            },
                        },
                    }
                }
            };

            var toastNotif = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toastNotif);

            (fsResult, genericOpReuslt) = await operations.RestartServiceAsync(_project.ServiceName);

            if (fsResult.ErrorCode != Enums.FileSystemStatusCode.Success)
            {
                dialog = new(genericOpReuslt.ErrorMessage, "An error has occurred");
                await dialog.ShowAsync();
                return;
            }

            toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = $"The service {serviceName} was restarted successfully."
                            },
                        },
                    }
                }
            };

            toastNotif = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toastNotif);

            dialog = new($"The service {_project.ServiceName} was restarted successfully.", "Successful");
            await dialog.ShowAsync();
        }

        public async Task<bool> OnDeleteEnvironment(Guid environmentGuid)
        {
            var result = await _environmentService.DeleteByGuidAsync(environmentGuid);

            if (result)
            {
                var env = Source.FirstOrDefault(x => x.Guid == environmentGuid);
                Source.Remove(env);
            }

            return result;
        }

        public ProjectDetailsViewModel()
        {
            _projectService = Ioc.Default.GetService<ProjectService>();
            _environmentService = Ioc.Default.GetService<EnvironmentService>();
            _activedEnvironmentService = Ioc.Default.GetService<ActivedEnvironmentService>();
            _mapper = new Mapper(Ioc.Default.GetService<MapperConfiguration>());
            _navigationService = Ioc.Default.GetService<NavigationService>();
        }

        public async Task<bool> OnActivateClickAsync(Guid environmentGuid)
        {
            try
            {
                GenericOperationResult genericOpReuslt;
                FilesystemResult fsResult;

                var operations = new FilesystemOperations();

                var environment = await _environmentService.GetByGuidAsync(environmentGuid);              

                (fsResult, genericOpReuslt) = await operations.ReplaceConnectionStringAsync(_project.ConfigurationPath,
                    environment.ConnectionStrings);

                if (fsResult.ErrorCode != Enums.FileSystemStatusCode.Success)
                {
                    throw new Exception(genericOpReuslt?.ErrorMessage);
                }

                var activedEnvironment = new ActivedEnvironment
                {
                    ProjectGuid = Project.Guid,
                    EnvironmentGuid = environmentGuid,
                };

                bool result = await _activedEnvironmentService.Update(activedEnvironment);

                ActivedEnvironment = await _activedEnvironmentService.GetByProjectGuidAsync(Project.Guid);

                foreach (EnvironmentDO environmentDO in Source)
                {
                    environmentDO.CanBeActivated = !(ActivedEnvironment.EnvironmentGuid == environmentDO.Guid);
                }

                MessageDialog dialog = new("The environment was successfully activated, don't forget restart the service.", "Successful");
                await dialog.ShowAsync();

                return true;
            }
            catch (Exception ex)
            {
                MessageDialog dialog = new(ex.Message, "An error has occurred");
                await dialog.ShowAsync();
            }

            return false;
        }

        public async Task InitializeAsync(Guid projectGuid)
        {
            Source.Clear();
            Project = await _projectService.GetByGuidAsync(projectGuid);
            ActivedEnvironment = await _activedEnvironmentService.GetByProjectGuidAsync(projectGuid);

            var environments = await _environmentService.GetByProjectGuidAsync(projectGuid);

            foreach (Environment environment in environments)
            {
                var environmentDO = _mapper.Map<EnvironmentDO>(environment);
                environmentDO.CanBeActivated = !(ActivedEnvironment.EnvironmentGuid == environmentDO.Guid);
                Source.Add(environmentDO);
            }
        }

        private void OnCreateEnvironmentClick()
        {
            _navigationService.Navigate<CreateEnvironmentPage>(Project.Guid);
        }

        private void OnEditClick()
        {
            _navigationService.Navigate<EditProjectPage>(_project);
        }

        private void OnItemClick(EnvironmentDO clickedItem)
        {
            if (clickedItem != null)
            {
                //NavigationService.Frame.SetListDataItemForNextConnectedAnimation(clickedItem);
                _navigationService.Navigate<EnvironmentDetailsPage>(clickedItem.Guid);
            }
        }
    }
}
