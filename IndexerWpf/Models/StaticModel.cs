using IndexerWpf.Classes;
using System;
using System.Text.RegularExpressions;
using System.Threading;

namespace IndexerWpf.Models
{
    public static class StaticModel
    {
        public delegate void ItemExpand(IndxElementNew item);
        public static event ItemExpand ItemIsExpandChange;
        public static void InvokeItemIsExpandChange(IndxElementNew item)
        {
            ItemIsExpandChange?.Invoke(item);
        }
        public delegate void LoadEnd();
        public static event LoadEnd LoadEndEvent;
        //public delegate void Idincreased(int val, string path);
        //public static event Idincreased IdincreasedEvent;
        public static void InvokeLoadEndEvent()
        {
            LoadEndEvent?.Invoke();
        }
        //public static void InvokeIdincreasedEvent(int val, string path)
        //{
        //    IdincreasedEvent?.Invoke(val, path);
        //}
        public static event LoadEnd WindowClosing;
        public static void InvokeWindowClosing()
        {
            WindowClosing?.Invoke();
        }
        public static bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return false;

            try
            {
                Regex.Match("", pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }
        public static CancellationTokenSource CancelToken { get; set; } = new CancellationTokenSource();
    }

}
