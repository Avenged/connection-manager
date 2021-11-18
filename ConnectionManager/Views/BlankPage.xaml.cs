using System;

using ConnectionManager.ViewModels;

using Windows.UI.Xaml.Controls;

namespace ConnectionManager.Views
{
    public sealed partial class BlankPage : Page
    {
        public BlankViewModel ViewModel { get; } = new BlankViewModel();

        public BlankPage()
        {
            InitializeComponent();
        }
    }
}
