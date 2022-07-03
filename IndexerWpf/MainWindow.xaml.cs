using IndexerWpf.Classes;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using IndexerWpf.Models;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Diagnostics;

namespace IndexerWpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += delegate { this.DragMove(); };
            (DataContext as MainViewModel).PropertyChanged += MainWindow_PropertyChanged;
        }
        bool fadeout = false;
        private void MainWindow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Is_scanned")
            {
                DoubleAnimation opacityAnimation = new DoubleAnimation();
                if ((DataContext as MainViewModel).Is_scanned)
                {
                    opacityAnimation.From = 0.89;
                    opacityAnimation.To = 0;
                    opacityAnimation.Duration = TimeSpan.FromSeconds(2);

                }
                else if (!(DataContext as MainViewModel).Is_scanned)
                {
                    borderspinner.Visibility = Visibility.Visible;
                    opacityAnimation.From = 0;
                    opacityAnimation.To = 0.89;
                    opacityAnimation.Duration = TimeSpan.FromSeconds(2);
                    fadeout = true;
                }


                Storyboard.SetTarget(opacityAnimation, borderspinner);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(Border.Opacity)"));

                var storyboard = new Storyboard();
                storyboard.Completed += Storyboard_Completed1;
                storyboard.Children.Add(opacityAnimation);
                storyboard.Begin();


            }
        }

        private void Storyboard_Completed1(object sender, EventArgs e)
        {
            if(fadeout)
            {
                fadeout = false;
                borderspinner.Visibility = Visibility.Collapsed;
            }
        }

        IndxElement lastSelected = null;
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var a = sender as ListView;
            IndxElement b = (IndxElement)a.SelectedItem;
            if (b == null) return;
            if (b != lastSelected)
            {
                if (lastSelected != null)
                {
                    lastSelected.IsExpanded = false;
                    lastSelected.IsSelected = false;
                }
            }
            b.IsExpanded = true;
            b.IsSelected = true;
            lastSelected = b;
        }

        private void TreeViewSelectedItemChanged(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                item.BringIntoView();
                e.Handled = true;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            RegexHelpWindow rgx = new RegexHelpWindow();
            rgx.Owner = this;
            rgx.Show();
            rgx.Left += this.Width / 2;
            e.Handled = true;
        }
        private void OnListViewItemPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Trace.WriteLine("Preview MouseRightButtonDown");

            e.Handled = true;
        }
    }
}
