using System;
using Windows.UI.Xaml;

namespace ConnectionManager.Common.Models
{
    public class ProjectDO : DependencyObject
    {
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public string ConfigurationPath
        {
            get { return (string)GetValue(ConfigurationPathProperty); }
            set { SetValue(ConfigurationPathProperty, value); }
        }

        public string ServiceName
        {
            get { return (string)GetValue(ServiceNameProperty); }
            set { SetValue(ServiceNameProperty, value); }
        }

        public Guid Guid
        {
            get { return (Guid)GetValue(GuidProperty); }
            set { SetValue(GuidProperty, value); }
        }


        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(Environment), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(Environment), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ConfigurationPathProperty =
            DependencyProperty.Register("ConfigurationPath", typeof(string), typeof(Environment), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ServiceNameProperty =
            DependencyProperty.Register("ServiceName", typeof(string), typeof(Environment), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty GuidProperty =
            DependencyProperty.Register("Guid", typeof(Guid), typeof(Environment), new PropertyMetadata(Guid.NewGuid()));
    }
}
