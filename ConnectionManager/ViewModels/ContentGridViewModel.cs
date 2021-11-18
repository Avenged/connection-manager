using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Uwp.UI.Animations;

using ConnectionManager.Common.Models;
using ConnectionManager.Common.Services;
using ConnectionManager.Services;
using ConnectionManager.Views;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace ConnectionManager.ViewModels
{
    public class ContentGridViewModel : ObservableObject
    {
        private readonly NavigationService _navigationService;
        private ICommand _itemClickCommand;

        public ICommand ItemClickCommand => _itemClickCommand ?? (_itemClickCommand = new RelayCommand<SampleOrder>(OnItemClick));

        public ObservableCollection<SampleOrder> Source { get; } = new ObservableCollection<SampleOrder>();

        public ContentGridViewModel()
        {
            _navigationService = Ioc.Default.GetService<NavigationService>();
        }

        public async Task LoadDataAsync()
        {
            Source.Clear();

            // Replace this with your actual data
            var data = await SampleDataService.GetContentGridDataAsync();
            foreach (var item in data)
            {
                Source.Add(item);
            }
        }

        private void OnItemClick(SampleOrder clickedItem)
        {
            if (clickedItem != null)
            {
                _navigationService.Frame.SetListDataItemForNextConnectedAnimation(clickedItem);
                _navigationService.Navigate<ContentGridDetailPage>(clickedItem.OrderID);
            }
        }
    }
}
