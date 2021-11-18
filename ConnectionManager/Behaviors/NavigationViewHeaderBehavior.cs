using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xaml.Interactivity;

using ConnectionManager.Services;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using WinUI = Microsoft.UI.Xaml.Controls;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace ConnectionManager.Behaviors
{
    public class NavigationViewHeaderBehavior : Behavior<WinUI.NavigationView>
    {
        private readonly NavigationService _navigationService;
        private static NavigationViewHeaderBehavior _current;
        private Page _currentPage;

        public DataTemplate DefaultHeaderTemplate { get; set; }

        public NavigationViewHeaderBehavior()
        {
            _navigationService = Ioc.Default.GetService<NavigationService>();
        }

        public object DefaultHeader
        {
            get { return GetValue(DefaultHeaderProperty); }
            set { SetValue(DefaultHeaderProperty, value); }
        }

        public static readonly DependencyProperty DefaultHeaderProperty = DependencyProperty.Register("DefaultHeader", typeof(object), typeof(NavigationViewHeaderBehavior), new PropertyMetadata(null, (d, e) => _current.UpdateHeader()));

        public static NavigationViewHeaderMode GetHeaderMode(Page item)
        {
            return (NavigationViewHeaderMode)item.GetValue(HeaderModeProperty);
        }

        public static void SetHeaderMode(Page item, NavigationViewHeaderMode value)
        {
            item.SetValue(HeaderModeProperty, value);
        }

        public static readonly DependencyProperty HeaderModeProperty =
            DependencyProperty.RegisterAttached("HeaderMode", typeof(bool), typeof(NavigationViewHeaderBehavior), new PropertyMetadata(NavigationViewHeaderMode.Always, (d, e) => _current.UpdateHeader()));

        public static object GetHeaderContext(Page item)
        {
            return item.GetValue(HeaderContextProperty);
        }

        public static void SetHeaderContext(Page item, object value)
        {
            item.SetValue(HeaderContextProperty, value);
        }

        public static readonly DependencyProperty HeaderContextProperty =
            DependencyProperty.RegisterAttached("HeaderContext", typeof(object), typeof(NavigationViewHeaderBehavior), new PropertyMetadata(null, (d, e) => _current.UpdateHeader()));

        public static DataTemplate GetHeaderConnectionManager(Page item)
        {
            return (DataTemplate)item.GetValue(HeaderConnectionManagerProperty);
        }

        public static void SetHeaderConnectionManager(Page item, DataTemplate value)
        {
            item.SetValue(HeaderConnectionManagerProperty, value);
        }

        public static readonly DependencyProperty HeaderConnectionManagerProperty =
            DependencyProperty.RegisterAttached("HeaderConnectionManager", typeof(DataTemplate), typeof(NavigationViewHeaderBehavior), new PropertyMetadata(null, (d, e) => _current.UpdateHeaderConnectionManager()));

        protected override void OnAttached()
        {
            base.OnAttached();
            _current = this;
            _navigationService.Navigated += OnNavigated;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            _navigationService.Navigated -= OnNavigated;
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            var frame = sender as Frame;
            if (frame.Content is Page page)
            {
                _currentPage = page;

                UpdateHeader();
                UpdateHeaderConnectionManager();
            }
        }

        private void UpdateHeader()
        {
            if (_currentPage != null)
            {
                var headerMode = GetHeaderMode(_currentPage);
                if (headerMode == NavigationViewHeaderMode.Never)
                {
                    AssociatedObject.Header = null;
                    AssociatedObject.AlwaysShowHeader = false;
                }
                else
                {
                    var headerFromPage = GetHeaderContext(_currentPage);
                    if (headerFromPage != null)
                    {
                        AssociatedObject.Header = headerFromPage;
                    }
                    else
                    {
                        AssociatedObject.Header = DefaultHeader;
                    }

                    if (headerMode == NavigationViewHeaderMode.Always)
                    {
                        AssociatedObject.AlwaysShowHeader = true;
                    }
                    else
                    {
                        AssociatedObject.AlwaysShowHeader = false;
                    }
                }
            }
        }

        private void UpdateHeaderConnectionManager()
        {
            if (_currentPage != null)
            {
                var headerConnectionManager = GetHeaderConnectionManager(_currentPage);
                AssociatedObject.HeaderTemplate = headerConnectionManager ?? DefaultHeaderTemplate;
            }
        }
    }
}
