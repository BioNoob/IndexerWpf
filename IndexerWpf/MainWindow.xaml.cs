using IndexerWpf.Classes;
using IndexerWpf.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace IndexerWpf
{
    public partial class MainWindow : Window
    {
        public bool FocusOnList = false;
        //public WpfObservableRangeCollection<IndxElement> SelectedElements { get => (this.DataContext as MainViewModel).SelectedElements; }
        public MainWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += delegate { this.DragMove(); };
            (DataContext as MainViewModel).PropertyChanged += MainWindow_PropertyChanged;
            StaticModel.ItemIsExpandChange += StaticModel_ItemIsExpandChange;
            //Toggle.IsChecked = false;
        }
        IndxElementNew needfocus = null;
        private void StaticModel_ItemIsExpandChange(IndxElementNew item)
        {
            //if(item.ChildElements == null)
            //folder_tree.FocusItem(item, true);
            needfocus = item;
        }

        //bool fadeout = false;
        private void MainWindow_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Is_scanned")
            {
                DoubleAnimation opacityAnimation = new DoubleAnimation();
                var storyboard = new Storyboard();
                if ((DataContext as MainViewModel).Is_scanned)
                {
                    opacityAnimation.From = 0.89;
                    opacityAnimation.To = 0;
                    opacityAnimation.Duration = TimeSpan.FromSeconds(1);
                    storyboard.Completed += Storyboard_Completed1;

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
                storyboard.Children.Add(opacityAnimation);
                storyboard.Begin();
            }
            else if (e.PropertyName == "Is_Search")
            {
                DoubleAnimation opacityAnimation = new DoubleAnimation();
                var storyboard = new Storyboard();
                if ((DataContext as MainViewModel).Is_Search)
                {
                    opacityAnimation.From = 0;
                    opacityAnimation.To = 0.89;
                    opacityAnimation.Duration = TimeSpan.FromSeconds(0.5);

                }
                else if (!(DataContext as MainViewModel).Is_Search)
                {
                    opacityAnimation.From = 0.89;
                    opacityAnimation.To = 0;
                    opacityAnimation.Duration = TimeSpan.FromSeconds(0.5);
                }


                Storyboard.SetTarget(opacityAnimation, minispinner);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(Border.Opacity)"));
                storyboard.Children.Add(opacityAnimation);
                storyboard.Begin();
            }
        }

        private void Storyboard_Completed1(object sender, EventArgs e)
        {
            //if (fadeout)
            //{
                //fadeout = false;
                borderspinner.Visibility = Visibility.Collapsed;
            //}
        }


        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            RegexHelpWindow rgx = new RegexHelpWindow
            {
                Owner = this
            };
            rgx.Show();
            rgx.Left += this.Width / 2;
            e.Handled = true;
        }

        private void Folder_tree_GotFocus(object sender, RoutedEventArgs e)
        {
            FocusOnList = false;
        }

        private void SearchResult_GotFocus(object sender, RoutedEventArgs e)
        {
            FocusOnList = true;
        }
        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).ShowPopUp = false;
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            MainViewModel dtx = DataContext as MainViewModel;
            dtx.Is_scanned = true;
            dtx.LoadListIndexes();
            var lds = dtx.GetSelectedIndexes(dtx.Sets.LastIndex);
            foreach (var item in lds)
            {
                //var a = dtx.ListOfIndexes.SingleOrDefault(t => Path.GetFileNameWithoutExtension(t.JsonFileName) == item);
                var a = dtx.ListOfIndexes.SingleOrDefault(t => t.GetName == item);//.Distinct().SingleOrDefault();
                if (a != null)
                {
                    a.IsSelectedChangedEvent -= dtx.Q_IsSelectedChangedEvent;
                    a.IsSelected = true;
                    if (string.IsNullOrEmpty(dtx.SelectedIndexsString))
                        dtx.SelectedIndexsString += a.GetName;
                    else
                        dtx.SelectedIndexsString += ", " + a.GetName;
                    a.IsSelectedChangedEvent += dtx.Q_IsSelectedChangedEvent;
                }
                else
                    dtx.Is_scanned = false;
            }
            dtx.DoLoad();
        }

        private void folder_tree_LayoutUpdated(object sender, EventArgs e)
        {
            if(needfocus != null)
            {
                folder_tree.FocusItem(needfocus, true);
                needfocus = null;  
            }
                
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SetsPopUp.IsOpen = !SetsPopUp.IsOpen;
            setsbtn.IsEnabled = false;
        }

        private void SetsPopUp_Closed(object sender, EventArgs e)
        {
            setsbtn.IsEnabled = true;
        }

        private void ContentControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MainViewModel dtx = DataContext as MainViewModel;
            var element = (IndxElementNew)(sender as ContentControl).Tag;
            if(element != null)
            {
                switch (dtx.SelectedDoubleClickOptionTag)
                {
                    case DoubleClickAction.Folder:
                        var t1 = element.OpenFolderCommand;
                        if (t1.CanExecute(null))
                        {
                            t1.Execute(null);
                            e.Handled = false;
                        }
                        return;
                    case DoubleClickAction.File:
                        var t = element.OpenFileCommand;
                        if (t.CanExecute(null))
                        {
                            t.Execute(null);
                            e.Handled = false;
                        }
                        return;
                    case DoubleClickAction.Tree:
                        var t2 = dtx.ShowOnTreeCommand;
                        if (t2.CanExecute(null))
                        {
                            t2.Execute(null);
                            e.Handled = false;
                        }
                        return;
                }
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainViewModel dtx = DataContext as MainViewModel;
            dtx.CloseWindowCommand.Execute(null);
        }
    }
}
