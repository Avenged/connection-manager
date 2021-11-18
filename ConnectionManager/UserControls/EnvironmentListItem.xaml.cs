using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ConnectionManager.UserControls
{
    public sealed partial class EnvironmentListItem : UserControl
    {
        public event EventHandler<Guid> OnActivateClick;
        public event EventHandler<Guid> OnEditClick;
        public event EventHandler<Guid> OnDeleteClick;

        public EnvironmentListItem()
        {
            InitializeComponent();
            Name = Guid.ToString();
        }

        public bool CanBeActivated
        {
            get { return (bool)GetValue(CanBeActivatedProperty); }
            set { SetValue(CanBeActivatedProperty, value); }
        }

        public Guid Guid
        {
            get { return (Guid)GetValue(GuidProperty); }
            set { SetValue(GuidProperty, value); }
        }

        public string EnvName
        {
            get { return (string)GetValue(EnvNameProperty); }
            set { SetValue(EnvNameProperty, value); }
        }

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        private void OnGuidChanged(DependencyPropertyChangedEventArgs e)
        {
            //Guid.Text = e.NewValue.ToString();
        }

        private void OnCanBeActivatedChanged(DependencyPropertyChangedEventArgs e)
        {
            ActivateButton.IsEnabled = (bool)e.NewValue;
        }

        private void OnNameChanged(DependencyPropertyChangedEventArgs e)
            => NameTextBlock.Text = e.NewValue.ToString();

        private void OnDescriptionChanged(DependencyPropertyChangedEventArgs e)
            => DescriptionTextBlock.Text = e.NewValue.ToString();

        private static void OnGuidChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var environmentListItem = d as EnvironmentListItem;
            environmentListItem.OnGuidChanged(e);
        }

        private static void OnCanBeActivatedChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var environmentListItem = d as EnvironmentListItem;
            environmentListItem.OnCanBeActivatedChanged(e);
        }

        private static void OnNameChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var environmentListItem = d as EnvironmentListItem;
            environmentListItem.OnNameChanged(e);
        }

        private static void OnDescriptionChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var environmentListItem = d as EnvironmentListItem;
            environmentListItem.OnDescriptionChanged(e);
        }

        public static readonly DependencyProperty CanBeActivatedProperty =
            DependencyProperty.Register("Activated", typeof(string), typeof(EnvironmentListItem), new PropertyMetadata(true, new PropertyChangedCallback(OnCanBeActivatedChanged)));

        public static readonly DependencyProperty EnvNameProperty =
            DependencyProperty.Register("EnvName", typeof(string), typeof(EnvironmentListItem), new PropertyMetadata(null, new PropertyChangedCallback(OnNameChanged)));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(EnvironmentListItem), new PropertyMetadata(null, new PropertyChangedCallback(OnDescriptionChanged)));

        public static readonly DependencyProperty GuidProperty =
            DependencyProperty.Register("Guid", typeof(Guid), typeof(EnvironmentListItem), new PropertyMetadata(Guid.NewGuid(), new PropertyChangedCallback(OnGuidChanged)));

        private void Activate_Click(object sender, RoutedEventArgs e)
        {
            OnActivateClick?.Invoke(this, Guid);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            OnEditClick?.Invoke(sender, Guid);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnDeleteClick?.Invoke(this, Guid);
        }
    }
}
