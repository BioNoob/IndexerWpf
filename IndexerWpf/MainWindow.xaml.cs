using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    //public class BindingErrorListener : TraceListener
    //{
    //    private Action<string> logAction;
    //    public static void Listen(Action<string> logAction)
    //    {
    //        PresentationTraceSources.DataBindingSource.Listeners
    //            .Add(new BindingErrorListener() { logAction = logAction });
    //    }
    //    public override void Write(string message) { }
    //    public override void WriteLine(string message)
    //    {
    //        logAction(message);
    //    }
    //}
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
           // BindingErrorListener.Listen(m => MessageBox.Show(m));
            InitializeComponent();
            //StaticModel.LoadEndEvent += StaticModel_LoadEndEvent;
            this.MouseLeftButtonDown += delegate { this.DragMove(); };

        }

        //private void StaticModel_LoadEndEvent()
        //{
        //    //foreach (var item in treeView1.Items)
        //    //{
        //    //    TreeViewItem treeItem = this.treeView1.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
        //    //    var t = treeItem.Header as IndxElement;
        //    //    if (t.Prnt == null)
        //    //    {
        //    //        //treeItem.IsExpanded = true;
        //    //        return;
        //    //    }

        //    //}


        //}
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

            //foreach (var item in StaticModel.ElIndx.AllFiles.Where(t=>t.Founded))
            //{
            //    item.Founded = false;
            //}
            //IndxElement ty = b;
            //ty.Founded = true;
            ////List<IndxElement> ty = new List<IndxElement>();
            ////ty.Add(b);
            ////StaticModel.ElIndx.AllFiles.Where(t => t.Id == Prnt).FirstOrDefault().Founded = value;
            //while (ty.Prnt != null)
            //{
            //    ty = StaticModel.ElIndx.AllFiles.FirstOrDefault(t => t.Id == ty.Prnt);
            //    ty.Founded = true;
            //    //ty.AddRange(StaticModel.ElIndx.AllFiles.Where(t => t.Items != null && t.Items.Contains(b)).ToList());
            //}
            //b.Founded = true;
            // lastSelected = b;
            //}

            // var mod = (MainViewModel)this.DataContext;
            // //тут надо у всех открытых проставить закрытие
            // //а потом по всем элементам, родительским пробежаться открыть
            //mod.Indexes.AllFiles.Where(t=>!t.Founded).
            //folder_tree.ite
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

        //private void Window_Loaded(object sender, RoutedEventArgs e)
        //{

        //}
    }
}
