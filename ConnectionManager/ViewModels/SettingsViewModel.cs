using System;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

using ConnectionManager.Helpers;
using ConnectionManager.Services;

using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.Storage;
using System.IO;
using Windows.Storage.Pickers;
using System.Collections.Generic;
using Windows.Storage.Provider;
using Windows.UI.Popups;

namespace ConnectionManager.ViewModels
{
    // TODO WTS: Add other settings as necessary. For help see https://github.com/Microsoft/WindowsConnectionManagerStudio/blob/release/docs/UWP/pages/settings.md
    public class SettingsViewModel : ObservableObject
    {
        private string _versionDescription;
        private ICommand _switchThemeCommand;
        private ICommand _exportConfigurationClickCommand;
        private ICommand _importConfigurationClickCommand;
        private ElementTheme _elementTheme = ThemeSelectorService.Theme;

        public ICommand ExportConfigurationClickCommand => _exportConfigurationClickCommand ??= new RelayCommand(OnExportConfiguration);
        public ICommand ImportConfigurationClickCommand => _importConfigurationClickCommand ??= new RelayCommand(OnImportConfiguration);

        private async void OnImportConfiguration()
        {
            var localFolder = ApplicationData.Current.LocalFolder;

            FileOpenPicker openPicker = new();
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".json");

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file == null)
            {
                return;
            }

            CachedFileManager.DeferUpdates(file);
            await localFolder.SaveFileAsync(await file.ReadBytesAsync(), "data.json");
            FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

            MessageDialog dialog;

            if (status == FileUpdateStatus.Complete)
            {
                dialog = new("The configuration was imported successfully.", "Connection Manager");
                await dialog.ShowAsync();
                return;
            }

            dialog = new("The configuration was not imported due to a problem.", "Connection Manager");
            await dialog.ShowAsync();
            return;
        }

        private async void OnExportConfiguration()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var configFile = await localFolder.GetFileAsync("data.json");
            var configFileBytes = await configFile.ReadBytesAsync();

            FileSavePicker savePicker = new();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("JSON", new List<string>() { ".json" });
            savePicker.SuggestedFileName = "data";

            StorageFile file = await savePicker.PickSaveFileAsync();

            if (file == null)
            {
                return;
            }

            CachedFileManager.DeferUpdates(file);
            await FileIO.WriteBytesAsync(file, configFileBytes);
            FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

            MessageDialog dialog;

            if (status == FileUpdateStatus.Complete)
            {
                dialog = new("The configuration was exported successfully.", "Connection Manager");
                await dialog.ShowAsync();
                return;
            }

            dialog = new("The configuration was not exported due to a problem.", "Connection Manager");
            await dialog.ShowAsync();
            return;
        }

        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }
            set { SetProperty(ref _elementTheme, value); }
        }

        public string VersionDescription
        {
            get { return _versionDescription; }

            set { SetProperty(ref _versionDescription, value); }
        }


        public ICommand SwitchThemeCommand
        {
            get
            {
                if (_switchThemeCommand == null)
                {
                    _switchThemeCommand = new RelayCommand<ElementTheme>(
                        async (param) =>
                        {
                            ElementTheme = param;
                            await ThemeSelectorService.SetThemeAsync(param);
                        });
                }

                return _switchThemeCommand;
            }
        }

        public SettingsViewModel()
        {
        }



        public async Task InitializeAsync()
        {
            VersionDescription = GetVersionDescription();
            await Task.CompletedTask;
        }

        private string GetVersionDescription()
        {
            var appName = "AppDisplayName".GetLocalized();
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{appName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }
}
