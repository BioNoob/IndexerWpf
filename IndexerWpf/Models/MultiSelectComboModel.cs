using IndexerWpf.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace IndexerWpf.Models
{
    public class MultiSelectComboModel : Proper
    {

        //WpfObservableRangeCollection<IndxElements>
        bool isPopUp;
        public bool IsPopUp { get => isPopUp; set => SetProperty(ref isPopUp, value); }
        public MultiSelectComboModel(IEnumerable<IndxElements> list)
        {
            IsPopUp = false;
            ExistedIndexes = new WpfObservableRangeCollection<IndxElements>(list);
        }
        public MultiSelectComboModel()
        {
            IsPopUp = false;
            ExistedIndexes = new WpfObservableRangeCollection<IndxElements>();
        }
        private CommandHandler _popchanged;
        public CommandHandler PopChangedCommand
        {
            get
            {
                return _popchanged ?? (_popchanged = new CommandHandler(obj =>
                {
                    IsPopUp = !IsPopUp;
                },
                (obj) => true
                ));
            }
        }
        //продумать как при добавлении или удалении приаязаться к событию изменения выбора индекса чтоб добавлять в список сосисок (или удалять)
        public WpfObservableRangeCollection<IndxElements> ExistedIndexes
        {
            get => existedIndexs; set
            {
                SetProperty(ref existedIndexs, value); foreach (var item in existedIndexs)
                {
                    item.SelectedChanged -= Item_SelectedChanged;
                    item.SelectedChanged += Item_SelectedChanged;
                } } }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bool t = false;   
        }

        private void Item_SelectedChanged(bool state,string nm)
        {
            //if(state)
            //{
            //    SelectedBeString = SelectedBeString.
            //}
            //else
            //{
            //    SelectedBeString = SelectedBeString.Replace()
            //}
        }

        private WpfObservableRangeCollection<IndxElements> existedIndexs;

        public string SelectedBeString { get => selectedbestering; set => SetProperty(ref selectedbestering, value); }
        private string selectedbestering;
    }
}
