using System;
using Windows.UI.Xaml;

namespace ConnectionManager.Common.Models
{
    public class EnvironmentDO : DependencyObject
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

        //public string Path
        //{
        //    get { return (string)GetValue(PathProperty); }
        //    set { SetValue(PathProperty, value); }
        //}

        public string ConnectionStrings
        {
            get { return (string)GetValue(ConnectionStringsProperty); }
            set { SetValue(ConnectionStringsProperty, value); }
        }

        public Guid ProjectGuid
        {
            get { return (Guid)GetValue(ProjectGuidProperty); }
            set { SetValue(ProjectGuidProperty, value); }
        }

        public Guid Guid
        {
            get { return (Guid)GetValue(GuidProperty); }
            set { SetValue(GuidProperty, value); }
        }

        public bool CanBeActivated
        {
            get { return (bool)GetValue(CanBeActivatedProperty); }
            set { SetValue(CanBeActivatedProperty, value); }
        }

        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(Environment), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(Environment), new PropertyMetadata(string.Empty));

        //public static readonly DependencyProperty PathProperty =
        //    DependencyProperty.Register("Path", typeof(string), typeof(Environment), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ConnectionStringsProperty =
            DependencyProperty.Register("ConnectionStrings", typeof(string), typeof(Environment), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty GuidProperty =
            DependencyProperty.Register("Guid", typeof(Guid), typeof(Environment), new PropertyMetadata(Guid.NewGuid()));

        public static readonly DependencyProperty CanBeActivatedProperty =
            DependencyProperty.Register("CanBeActivated", typeof(bool), typeof(Environment), new PropertyMetadata(true));

        public static readonly DependencyProperty ProjectGuidProperty =
            DependencyProperty.Register("ProjectGuid", typeof(Guid), typeof(Environment), new PropertyMetadata(Guid.NewGuid()));
    }
}
