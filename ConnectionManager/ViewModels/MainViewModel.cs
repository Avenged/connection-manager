using System;

using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ConnectionManager.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
        }

        private bool isFullTrustElevated = false;

        public bool IsFullTrustElevated
        {
            get => isFullTrustElevated;
            set => SetProperty(ref isFullTrustElevated, value);
        }
    }
}
