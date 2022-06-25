﻿using IndexerWpf.Classes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace IndexerWpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += delegate { this.DragMove(); };
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
    }
}
