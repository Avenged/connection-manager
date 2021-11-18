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
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace ConnectionManager.ViewModels
{
    public class ProjectsViewModel : ObservableObject
    {
        private readonly NavigationService _navigationService;
        private readonly ProjectService _projectService;
        private ICommand _projectClickCommand;
        private ICommand _createClickCommand;

        public ICommand ProjectClickCommand => _projectClickCommand ?? (_projectClickCommand = new RelayCommand<Project>(OnItemClick));
        public ICommand CreateClickCommand => _createClickCommand ?? (_createClickCommand = new RelayCommand(OnCreateClick));

        public ObservableCollection<Project> Source { get; } = new ObservableCollection<Project>();

        public ProjectsViewModel()
        {
            _projectService = Ioc.Default.GetService<ProjectService>();
            _navigationService = Ioc.Default.GetService<NavigationService>();
        }

        public async Task LoadDataAsync()
        {
            Source.Clear();

            // Replace this with your actual data
            var data = await _projectService.GetAllAsync();
            foreach (var item in data)
            {
                Source.Add(item);
            }
        }

        private void OnCreateClick()
        {
            _navigationService.Navigate<CreateProjectPage>(null);
        }

        private void OnItemClick(Project clickedItem)
        {
            if (clickedItem != null)
            {
                //NavigationService.Frame.SetListDataItemForNextConnectedAnimation(clickedItem);
                _navigationService.Navigate<ProjectDetailPage>(clickedItem.Guid);
            }
        }
    }
}
