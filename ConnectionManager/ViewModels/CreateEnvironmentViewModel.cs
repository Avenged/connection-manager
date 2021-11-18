using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using ConnectionManager.Common.Services;

namespace ConnectionManager.ViewModels
{
    using AutoMapper;
    using ConnectionManager.Common.Models;
    using ConnectionManager.Services;
    using ConnectionManager.Views;
    using Microsoft.Toolkit.Mvvm.DependencyInjection;

    public class CreateEnvironmentViewModel : ObservableObject
    {
        private readonly NavigationService _navigationService;
        private readonly IMapper _mapper;
        private readonly ProjectService _projectService;
        private readonly EnvironmentService _environmentService;
        private Project _project;
        private EnvironmentDO _environment;
        private ICommand _createClickCommand;

        public ICommand CreateClickCommand => _createClickCommand ?? (_createClickCommand = new RelayCommand(OnCreateClick));
        public Project Project { get => _project; }
        public EnvironmentDO Environment { get => _environment; }

        public CreateEnvironmentViewModel()
        {
            _projectService = Ioc.Default.GetService<ProjectService>();
            _environmentService = Ioc.Default.GetService<EnvironmentService>();
            _navigationService = Ioc.Default.GetService<NavigationService>();
            _mapper = new Mapper(Ioc.Default.GetService<MapperConfiguration>());
        }

        public async Task InitializeAsync(Guid projectGuid)
        {
            _project = await _projectService.GetByGuidAsync(projectGuid);
            _environment = new EnvironmentDO
            {
                ProjectGuid = Project.Guid,
                Guid = Guid.NewGuid()
            };
        }

        private async void OnCreateClick()
        {
            var environment = _mapper.Map<Environment>(_environment);
            await _environmentService.CreateAsync(environment);
            _navigationService.Navigate<ProjectDetailPage>(_project.Guid);
        }
    }
}
