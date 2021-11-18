using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

using ConnectionManager.Services;

using Windows.ApplicationModel.Activation;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace ConnectionManager.Activation
{
    internal class DefaultActivationHandler : ActivationHandler<IActivatedEventArgs>
    {
        private readonly NavigationService _navigationService;
        private readonly Type _navElement;

        public DefaultActivationHandler(Type navElement)
        {
            _navElement = navElement;
            _navigationService = Ioc.Default.GetService<NavigationService>();
        }

        protected override async Task HandleInternalAsync(IActivatedEventArgs args)
        {
            // When the navigation stack isn't restored, navigate to the first page and configure
            // the new page by passing required information in the navigation parameter
            object arguments = null;
            if (args is LaunchActivatedEventArgs launchArgs)
            {
                arguments = launchArgs.Arguments;
            }

            _navigationService.Navigate(_navElement, arguments);
            await Task.CompletedTask;
        }

        protected override bool CanHandleInternal(IActivatedEventArgs args)
        {
            // None of the ActivationHandlers has handled the app activation
            return _navigationService.Frame.Content == null && _navElement != null;
        }
    }
}
