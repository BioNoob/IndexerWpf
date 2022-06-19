using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using IndexerWpf.Classes;
using IndexerWpf.Models;

namespace IndexerWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            StaticModel.LoadEndEvent += StaticModel_LoadEndEvent;
        }

        private void StaticModel_LoadEndEvent()
        {
            //foreach (var item in treeView1.Items)
            //{
            //    TreeViewItem treeItem = this.treeView1.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
            //    var t = treeItem.Header as IndxElement;
            //    if (t.Prnt == null)
            //    {
            //        //treeItem.IsExpanded = true;
            //        return;
            //    }

            //}


        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var a = sender as ListView;
            IndxElement b = (IndxElement)a.SelectedItem;
            b.Founded = true;
            var mod = (MainViewModel)this.DataContext;
            //тут надо у всех открытых проставить закрытие
            //а потом по всем элементам, родительским пробежаться открыть
           mod.Indexes.AllFiles.Where(t=>!t.Founded).
            //folder_tree.ite
        }
    }
}
