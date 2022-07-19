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
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace IndexerWpf
{
    public partial class MainWindow : Window
    {
        public bool FocusOnList = false;
        public WpfObservableRangeCollection<IndxElement> SelectedElements { get => (this.DataContext as MainViewModel).SelectedElements; }
        public MainWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += delegate { this.DragMove(); };
            (DataContext as MainViewModel).PropertyChanged += MainWindow_PropertyChanged;
            StaticModel.LoadEndEvent += StaticModel_LoadEndEvent;
            Toggle.IsChecked = false;
        }

        private void StaticModel_LoadEndEvent()
        {
            MainViewModel dtx = DataContext as MainViewModel;
            count_first_load--;
            if (count_first_load == 0)
            {
                dtx.ignore_scanned = false;
                dtx.Is_scanned = false;
                StaticModel.LoadEndEvent -= StaticModel_LoadEndEvent;
            }

        }

        bool fadeout = false;
        private void MainWindow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Is_scanned")
            {
                DoubleAnimation opacityAnimation = new DoubleAnimation();
                if ((DataContext as MainViewModel).Is_scanned)
                {
                    opacityAnimation.From = 0.89;
                    opacityAnimation.To = 0;
                    opacityAnimation.Duration = TimeSpan.FromSeconds(1);
                    fadeout = true;

                }
                else if (!(DataContext as MainViewModel).Is_scanned)
                {
                    borderspinner.Visibility = Visibility.Visible;
                    opacityAnimation.From = 0;
                    opacityAnimation.To = 0.89;
                    opacityAnimation.Duration = TimeSpan.FromSeconds(1);
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
            if (fadeout)
            {
                fadeout = false;
                borderspinner.Visibility = Visibility.Collapsed;
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var a = sender as ListView;
            //var added = e.AddedItems.OfType<IndxElement>().ToList();
            //added.FirstOrDefault().IsExpanded = true;
            //var deleted = e.RemovedItems.OfType<IndxElement>().ToList();
            //if (added != null)
            //{
            //    foreach (var item in added)
            //    {
            //        if (!SelectedElements.Contains(item))
            //        {
            //            SelectedElements.Add(item);
            //            item.IsSelected = true;
            //            item.IsExpanded = true;
            //        }
            //    }

            //}
            //if (deleted != null)
            //{
            //    foreach (var item in deleted)
            //    {
            //        if (SelectedElements.Contains(item))
            //        {
            //            SelectedElements.Remove(item);
            //            item.IsSelected = false;
            //            item.IsExpanded = false;
            //        }
            //    }
            //}
        }


        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            RegexHelpWindow rgx = new RegexHelpWindow();
            rgx.Owner = this;
            rgx.Show();
            rgx.Left += this.Width / 2;
            e.Handled = true;
        }

        private void folder_tree_SelectionChanged(object sender, EventArgs e)
        {

            //MultiSelectTreeView a = sender as MultiSelectTreeView;
            //if (a.LastSelectedItem == null) return;
            //a.FocusItem(a.LastSelectedItem, true);
            //if(FocusOnList)
            //SearchResult.Focus();
        }

        private void folder_tree_GotFocus(object sender, RoutedEventArgs e)
        {
            FocusOnList = false;
        }

        private void SearchResult_GotFocus(object sender, RoutedEventArgs e)
        {
            FocusOnList = true;
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            //Toggle.IsChecked = false;
            if (Toggle.IsChecked == true)
                Toggle.IsChecked = false;
            Debug.WriteLine("CLOSED");
            
        }

        private void TextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                if (Toggle.IsChecked != true)
                    Toggle.IsChecked = true;
            //Toggle.IsChecked = !Toggle.IsChecked;
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).ShowPopUp = false;
        }
        int count_first_load = 0;
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            MainViewModel dtx = DataContext as MainViewModel;

            dtx.Is_scanned = true;
            dtx.ignore_scanned = true;
            dtx.LoadListIndexes();

            var lds = dtx.GetSelectedIndexes(dtx.Sets.LastIndex);
            count_first_load = lds.Count;
            foreach (var item in lds)
            {
                var a = dtx.ListOfIndexes.SingleOrDefault(t => t.GetName == item);
                if (a != null)
                    a.IsSelected = true;
            }
        }
    }
}
