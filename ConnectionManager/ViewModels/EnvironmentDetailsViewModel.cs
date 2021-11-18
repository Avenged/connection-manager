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
using ConnectionManager.Services;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace ConnectionManager.ViewModels
{
    public class EnvironmentDetailsViewModel : ObservableObject
    {
        private ICommand _itemClickCommand;
        private Common.Models.Environment _environment;
        private readonly ProjectService _projectService;
        private readonly EnvironmentService _environmentService;

        public ICommand ItemClickCommand => _itemClickCommand ?? (_itemClickCommand = new RelayCommand<Guid>(OnItemClick));

        public Common.Models.Environment Environment
        {
            get { return _environment; }
            set { SetProperty(ref _environment, value); }
        }

        public EnvironmentDetailsViewModel()
        {
            _projectService = Ioc.Default.GetService<ProjectService>();
            _environmentService = Ioc.Default.GetService<EnvironmentService>();
        }

        public async Task InitializeAsync(Guid environmentGuid)
        {
            var environment = await _environmentService.GetByGuidAsync(environmentGuid);
            Environment = environment;
        }

        private void OnItemClick(Guid clickedItem)
        {
            if (clickedItem != null)
            {
                //NavigationService.Navigate<ProjectDetailPage>(clickedItem.Guid);
            }
        }
    }
}
