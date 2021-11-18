using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ConnectionManager.Common.Models;
using ConnectionManager.Common.Services;
using ConnectionManager.Services;
using ConnectionManager.Views;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Windows.Storage.Pickers;
using Windows.Storage;

namespace ConnectionManager.ViewModels
{
    public class EditProjectViewModel : ObservableObject
    {
        private readonly NavigationService _navigationService;
        private readonly ProjectService _projectService;
        private readonly IMapper _mapper;
        private ProjectDO _project;
        private ICommand _createClickCommand;
        private ICommand _cancelClickCommand;
        private ICommand _selectFileClickCommand;

        public ICommand SelectFileClickCommand => _selectFileClickCommand ??= new RelayCommand(OnSelectFileClick);
        public ICommand CreateClickCommand => _createClickCommand ??= new RelayCommand(OnSaveClick);
        public ICommand CancelClickCommand => _cancelClickCommand ??= new RelayCommand(OnCancelClick);

        public ProjectDO Project => _project;

        public EditProjectViewModel()
        {
            _projectService = Ioc.Default.GetService<ProjectService>();
            _mapper = new Mapper(Ioc.Default.GetService<MapperConfiguration>());
            _navigationService = Ioc.Default.GetService<NavigationService>();
        }

        public async Task InitializeAsync(Guid projectGuid)
        {
            var project = await _projectService.GetByGuidAsync(projectGuid);
            _project = _mapper.Map<ProjectDO>(project);
        }

        private async void OnSelectFileClick()
        {
            FileOpenPicker openPicker = new();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            openPicker.FileTypeFilter.Add(".config");

            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                Project.ConfigurationPath = file.Path;
            }
        }

        public async void OnSaveClick()
        {
            var project = _mapper.Map<Project>(_project);
            await _projectService.UpdateAsync(project);
            _navigationService.Navigate<ProjectDetailPage>(project);
        }

        public void OnCancelClick()
        {
            _navigationService.GoBack();
        }
    }
}
