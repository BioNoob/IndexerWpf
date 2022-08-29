using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace IndexerWpf.Classes
{
    public static class Voyadger1
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHELLEXECUTEINFO
        {
            public int cbSize;
            public uint fMask;
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpVerb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpFile;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpParameters;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDirectory;
            public int nShow;
            public IntPtr hInstApp;
            public IntPtr lpIDList;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpClass;
            public IntPtr hkeyClass;
            public uint dwHotKey;
            public IntPtr hIcon;
            public IntPtr hProcess;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);

        private const int SW_SHOW = 5;

        public static bool OpenFolderInExplorer(string path)
        {
            var info = new SHELLEXECUTEINFO();
            info.cbSize = Marshal.SizeOf<SHELLEXECUTEINFO>();
            info.lpVerb = "explore";
            info.lpParameters = "/n,/select";
            info.nShow = SW_SHOW;
            info.lpFile = path;
            return ShellExecuteEx(ref info);
        }
    }

    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {
        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        #region Private Fields    
        [NonSerialized]
        private DeferredEventsCollection _deferredEvents;
        #endregion Private Fields


        //------------------------------------------------------
        //
        //  Constructors
        //
        //------------------------------------------------------

        #region Constructors
        /// <summary>
        /// Initializes a new instance of ObservableCollection that is empty and has default initial capacity.
        /// </summary>
        public ObservableRangeCollection() { }

        /// <summary>
        /// Initializes a new instance of the ObservableCollection class that contains
        /// elements copied from the specified collection and has sufficient capacity
        /// to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        /// <remarks>
        /// The elements are copied onto the ObservableCollection in the
        /// same order they are read by the enumerator of the collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException"> collection is a null reference </exception>
        public ObservableRangeCollection(IEnumerable<T> collection) : base(collection) { }

        /// <summary>
        /// Initializes a new instance of the ObservableCollection class
        /// that contains elements copied from the specified list
        /// </summary>
        /// <param name="list">The list whose elements are copied to the new list.</param>
        /// <remarks>
        /// The elements are copied onto the ObservableCollection in the
        /// same order they are read by the enumerator of the list.
        /// </remarks>
        /// <exception cref="ArgumentNullException"> list is a null reference </exception>
        public ObservableRangeCollection(List<T> list) : base(list) { }

        #endregion Constructors

        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------

        #region Public Methods

        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="collection">
        /// The collection whose elements should be added to the end of the <see cref="ObservableCollection{T}"/>.
        /// The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        public void AddRange(IEnumerable<T> collection)
        {
            InsertRange(Count, collection);
        }

        /// <summary>
        /// Inserts the elements of a collection into the <see cref="ObservableCollection{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="collection">The collection whose elements should be inserted into the List<T>.
        /// The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>                
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not in the collection range.</exception>
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (collection is ICollection<T> countable)
            {
                if (countable.Count == 0)
                {
                    return;
                }
            }
            else if (!ContainsAny(collection))
            {
                return;
            }

            CheckReentrancy();

            //expand the following couple of lines when adding more constructors.
            var target = (List<T>)Items;
            target.InsertRange(index, collection);

            OnEssentialPropertiesChanged();

            if (!(collection is IList list))
                list = new List<T>(collection);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list, index));
        }


        /// <summary> 
        /// Removes the first occurence of each item in the specified collection from the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="collection">The items to remove.</param>        
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        public void RemoveRange(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (Count == 0)
            {
                return;
            }
            else if (collection is ICollection<T> countable)
            {
                if (countable.Count == 0)
                    return;
                else if (countable.Count == 1)
                    using (IEnumerator<T> enumerator = countable.GetEnumerator())
                    {
                        enumerator.MoveNext();
                        Remove(enumerator.Current);
                        return;
                    }
            }
            else if (!(ContainsAny(collection)))
            {
                return;
            }

            CheckReentrancy();

            var clusters = new Dictionary<int, List<T>>();
            var lastIndex = -1;
            List<T> lastCluster = null;

            var buf = new List<T>(collection);
            foreach (T item in buf)
            {
                var index = IndexOf(item);
                if (index < 0)
                {
                    continue;
                }

                Items.RemoveAt(index);

                if (lastIndex == index && lastCluster != null)
                {
                    lastCluster.Add(item);
                }
                else
                {
                    clusters[lastIndex = index] = lastCluster = new List<T> { item };
                }
            }

            OnEssentialPropertiesChanged();

            if (Count == 0)
                OnCollectionReset();
            else
                foreach (KeyValuePair<int, List<T>> cluster in clusters)
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, cluster.Value, cluster.Key));

        }

        /// <summary>
        /// Iterates over the collection and removes all items that satisfy the specified match.
        /// </summary>
        /// <remarks>The complexity is O(n).</remarks>
        /// <param name="match"></param>
        /// <returns>Returns the number of elements that where </returns>
        /// <exception cref="ArgumentNullException"><paramref name="match"/> is null.</exception>
        public int RemoveAll(Predicate<T> match)
        {
            return RemoveAll(0, Count, match);
        }

        /// <summary>
        /// Iterates over the specified range within the collection and removes all items that satisfy the specified match.
        /// </summary>
        /// <remarks>The complexity is O(n).</remarks>
        /// <param name="index">The index of where to start performing the search.</param>
        /// <param name="count">The number of items to iterate on.</param>
        /// <param name="match"></param>
        /// <returns>Returns the number of elements that where </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is out of range.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match"/> is null.</exception>
        public int RemoveAll(int index, int count, Predicate<T> match)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (index + count > Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            if (Count == 0)
                return 0;

            List<T> cluster = null;
            var clusterIndex = -1;
            var removedCount = 0;

            using (BlockReentrancy())
            using (DeferEvents())
            {
                for (var i = 0; i < count; i++, index++)
                {
                    T item = Items[index];
                    if (match(item))
                    {
                        Items.RemoveAt(index);
                        removedCount++;

                        if (clusterIndex == index)
                        {
                            Debug.Assert(cluster != null);
                            cluster.Add(item);
                        }
                        else
                        {
                            cluster = new List<T> { item };
                            clusterIndex = index;
                        }

                        index--;
                    }
                    else if (clusterIndex > -1)
                    {
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, cluster, clusterIndex));
                        clusterIndex = -1;
                        cluster = null;
                    }
                }

                if (clusterIndex > -1)
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, cluster, clusterIndex));
            }

            if (removedCount > 0)
                OnEssentialPropertiesChanged();

            return removedCount;
        }

        /// <summary>
        /// Removes a range of elements from the <see cref="ObservableCollection{T}"/>>.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">The specified range is exceeding the collection.</exception>
        public void RemoveRange(int index, int count)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (index + count > Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (count == 0)
                return;

            if (count == 1)
            {
                RemoveItem(index);
                return;
            }

            //Items will always be List<T>, see constructors
            var items = (List<T>)Items;
            List<T> removedItems = items.GetRange(index, count);

            CheckReentrancy();

            items.RemoveRange(index, count);

            OnEssentialPropertiesChanged();

            if (Count == 0)
                OnCollectionReset();
            else
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems, index));
        }

        /// <summary> 
        /// Clears the current collection and replaces it with the specified collection,
        /// using the default <see cref="EqualityComparer{T}"/>.
        /// </summary>             
        /// <param name="collection">The items to fill the collection with, after clearing it.</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        public void ReplaceRange(IEnumerable<T> collection)
        {
            ReplaceRange(0, Count, collection, EqualityComparer<T>.Default);
        }

        /// <summary>
        /// Clears the current collection and replaces it with the specified collection,
        /// using the specified comparer to skip equal items.
        /// </summary>
        /// <param name="collection">The items to fill the collection with, after clearing it.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{T}"/> to be used
        /// to check whether an item in the same location already existed before,
        /// which in case it would not be added to the collection, and no event will be raised for it.</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is null.</exception>
        public void ReplaceRange(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            ReplaceRange(0, Count, collection, comparer);
        }

        /// <summary>
        /// Removes the specified range and inserts the specified collection,
        /// ignoring equal items (using <see cref="EqualityComparer{T}.Default"/>).
        /// </summary>
        /// <param name="index">The index of where to start the replacement.</param>
        /// <param name="count">The number of items to be replaced.</param>
        /// <param name="collection">The collection to insert in that location.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is out of range.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        public void ReplaceRange(int index, int count, IEnumerable<T> collection)
        {
            ReplaceRange(index, count, collection, EqualityComparer<T>.Default);
        }

        /// <summary>
        /// Removes the specified range and inserts the specified collection in its position, leaving equal items in equal positions intact.
        /// </summary>
        /// <param name="index">The index of where to start the replacement.</param>
        /// <param name="count">The number of items to be replaced.</param>
        /// <param name="collection">The collection to insert in that location.</param>
        /// <param name="comparer">The comparer to use when checking for equal items.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is out of range.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is null.</exception>
        public void ReplaceRange(int index, int count, IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (index + count > Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            if (collection is ICollection<T> countable)
            {
                if (countable.Count == 0)
                {
                    RemoveRange(index, count);
                    return;
                }
            }
            else if (!ContainsAny(collection))
            {
                RemoveRange(index, count);
                return;
            }

            if (index + count == 0)
            {
                InsertRange(0, collection);
                return;
            }

            if (!(collection is IList<T> list))
                list = new List<T>(collection);

            using (BlockReentrancy())
            using (DeferEvents())
            {
                var rangeCount = index + count;
                var addedCount = list.Count;

                var changesMade = false;
                List<T>
                    newCluster = null,
                    oldCluster = null;


                int i = index;
                for (; i < rangeCount && i - index < addedCount; i++)
                {
                    //parallel position
                    T old = this[i], @new = list[i - index];
                    if (comparer.Equals(old, @new))
                    {
                        OnRangeReplaced(i, newCluster, oldCluster);
                        continue;
                    }
                    else
                    {
                        Items[i] = @new;

                        if (newCluster == null)
                        {
                            Debug.Assert(oldCluster == null);
                            newCluster = new List<T> { @new };
                            oldCluster = new List<T> { old };
                        }
                        else
                        {
                            newCluster.Add(@new);
                            oldCluster.Add(old);
                        }

                        changesMade = true;
                    }
                }

                OnRangeReplaced(i, newCluster, oldCluster);

                //exceeding position
                if (count != addedCount)
                {
                    var items = (List<T>)Items;
                    if (count > addedCount)
                    {
                        var removedCount = rangeCount - addedCount;
                        T[] removed = new T[removedCount];
                        items.CopyTo(i, removed, 0, removed.Length);
                        items.RemoveRange(i, removedCount);
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed, i));
                    }
                    else
                    {
                        var k = i - index;
                        T[] added = new T[addedCount - k];
                        for (int j = k; j < addedCount; j++)
                        {
                            T @new = list[j];
                            added[j - k] = @new;
                        }
                        items.InsertRange(i, added);
                        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added, i));
                    }

                    OnEssentialPropertiesChanged();
                }
                else if (changesMade)
                {
                    OnIndexerPropertyChanged();
                }
            }
        }

        #endregion Public Methods


        //------------------------------------------------------
        //
        //  Protected Methods
        //
        //------------------------------------------------------

        #region Protected Methods

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when the list is being cleared;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void ClearItems()
        {
            if (Count == 0)
                return;

            CheckReentrancy();
            base.ClearItems();
            OnEssentialPropertiesChanged();
            OnCollectionReset();
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is set in list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void SetItem(int index, T item)
        {
            if (Equals(this[index], item))
                return;

            CheckReentrancy();
            T originalItem = this[index];
            base.SetItem(index, item);

            OnIndexerPropertyChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, originalItem, item, index);
        }

        /// <summary>
        /// Raise CollectionChanged event to any listeners.
        /// Properties/methods modifying this ObservableCollection will raise
        /// a collection changed event through this virtual method.
        /// </summary>
        /// <remarks>
        /// When overriding this method, either call its base implementation
        /// or call <see cref="BlockReentrancy"/> to guard against reentrant collection changes.
        /// </remarks>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_deferredEvents != null)
            {
                _deferredEvents.Add(e);
                return;
            }
            base.OnCollectionChanged(e);
        }

        protected virtual IDisposable DeferEvents() => new DeferredEventsCollection(this);

        #endregion Protected Methods


        //------------------------------------------------------
        //
        //  Private Methods
        //
        //------------------------------------------------------

        #region Private Methods

        /// <summary>
        /// Helper function to determine if a collection contains any elements.
        /// </summary>
        /// <param name="collection">The collection to evaluate.</param>
        /// <returns></returns>
        private static bool ContainsAny(IEnumerable<T> collection)
        {
            using IEnumerator<T> enumerator = collection.GetEnumerator();
            return enumerator.MoveNext();
        }

        /// <summary>
        /// Helper to raise Count property and the Indexer property.
        /// </summary>
        private void OnEssentialPropertiesChanged()
        {
            OnPropertyChanged(EventArgsCache.CountPropertyChanged);
            OnIndexerPropertyChanged();
        }

        /// <summary>
        /// /// Helper to raise a PropertyChanged event for the Indexer property
        /// /// </summary>
        private void OnIndexerPropertyChanged() =>
          OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index) =>
          OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));

        /// <summary>
        /// Helper to raise CollectionChanged event with action == Reset to any listeners
        /// </summary>
        private void OnCollectionReset() =>
          OnCollectionChanged(EventArgsCache.ResetCollectionChanged);

        /// <summary>
        /// Helper to raise event for clustered action and clear cluster.
        /// </summary>
        /// <param name="followingItemIndex">The index of the item following the replacement block.</param>
        /// <param name="newCluster"></param>
        /// <param name="oldCluster"></param>
        //TODO should have really been a local method inside ReplaceRange(int index, int count, IEnumerable<T> collection, IEqualityComparer<T> comparer),
        //move when supported language version updated.
        private void OnRangeReplaced(int followingItemIndex, ICollection<T> newCluster, ICollection<T> oldCluster)
        {
            if (oldCluster == null || oldCluster.Count == 0)
            {
                Debug.Assert(newCluster == null || newCluster.Count == 0);
                return;
            }

            OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Replace,
                    new List<T>(newCluster),
                    new List<T>(oldCluster),
                    followingItemIndex - oldCluster.Count));

            oldCluster.Clear();
            newCluster.Clear();
        }

        #endregion Private Methods

        //------------------------------------------------------
        //
        //  Private Types
        //
        //------------------------------------------------------

        #region Private Types
        private sealed class DeferredEventsCollection : List<NotifyCollectionChangedEventArgs>, IDisposable
        {
            private readonly ObservableRangeCollection<T> _collection;
            public DeferredEventsCollection(ObservableRangeCollection<T> collection)
            {
                Debug.Assert(collection != null);
                Debug.Assert(collection._deferredEvents == null);
                _collection = collection;
                _collection._deferredEvents = this;
            }

            public void Dispose()
            {
                _collection._deferredEvents = null;
                foreach (var args in this)
                    _collection.OnCollectionChanged(args);
            }
        }

        #endregion Private Types

    }

    /// <remarks>
    /// To be kept outside <see cref="ObservableCollection{T}"/>, since otherwise, a new instance will be created for each generic type used.
    /// </remarks>
    internal static class EventArgsCache
    {
        internal static readonly PropertyChangedEventArgs CountPropertyChanged = new PropertyChangedEventArgs("Count");
        internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new PropertyChangedEventArgs("Item[]");
        internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
    }
    public class WpfObservableRangeCollection<T> : ObservableRangeCollection<T>
    {
        DeferredEventsCollection _deferredEvents;

        public WpfObservableRangeCollection()
        {
        }

        public WpfObservableRangeCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public WpfObservableRangeCollection(List<T> list) : base(list)
        {
        }


        /// <summary>
        /// Raise CollectionChanged event to any listeners.
        /// Properties/methods modifying this ObservableCollection will raise
        /// a collection changed event through this virtual method.
        /// </summary>
        /// <remarks>
        /// When overriding this method, either call its base implementation
        /// or call <see cref="BlockReentrancy"/> to guard against reentrant collection changes.
        /// </remarks>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var _deferredEvents = (ICollection<NotifyCollectionChangedEventArgs>)typeof(ObservableRangeCollection<T>).GetField("_deferredEvents", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
            if (_deferredEvents != null)
            {
                _deferredEvents.Add(e);
                return;
            }

            foreach (var handler in GetHandlers())
                if (IsRange(e) && handler.Target is CollectionView cv)
                    cv.Refresh();
                else
                    handler(this, e);
        }

        protected override IDisposable DeferEvents() => new DeferredEventsCollection(this);

        bool IsRange(NotifyCollectionChangedEventArgs e) => e.NewItems?.Count > 1 || e.OldItems?.Count > 1;
        IEnumerable<NotifyCollectionChangedEventHandler> GetHandlers()
        {
            var info = typeof(ObservableCollection<T>).GetField(nameof(CollectionChanged), BindingFlags.Instance | BindingFlags.NonPublic);
            var @event = (MulticastDelegate)info.GetValue(this);
            return @event?.GetInvocationList()
              .Cast<NotifyCollectionChangedEventHandler>()
              .Distinct()
              ?? Enumerable.Empty<NotifyCollectionChangedEventHandler>();
        }

        class DeferredEventsCollection : List<NotifyCollectionChangedEventArgs>, IDisposable
        {
            private readonly WpfObservableRangeCollection<T> _collection;
            public DeferredEventsCollection(WpfObservableRangeCollection<T> collection)
            {
                Debug.Assert(collection != null);
                Debug.Assert(collection._deferredEvents == null);
                _collection = collection;
                _collection._deferredEvents = this;
            }

            public void Dispose()
            {
                _collection._deferredEvents = null;

                var handlers = _collection
                  .GetHandlers()
                  .ToLookup(h => h.Target is CollectionView);

                foreach (var handler in handlers[false])
                    foreach (var e in this)
                        handler(_collection, e);

                foreach (var cv in handlers[true]
                  .Select(h => h.Target)
                  .Cast<CollectionView>()
                  .Distinct())
                    cv.Refresh();
            }
        }
    }

    public class CommandHandler : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public CommandHandler(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;
            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute?.Invoke(parameter);
        }
    }
    public enum TypeOfError
    {
        Deleted,
        Invalid,
        CancelTask,
        SomeThing
    }
    public class ProcessingFileException : Exception
    {

        public ProcessingFileException(/*Exception innerException,*/ TypeOfError type, string path_to_Json, IndxElements ien)
            //: base(GetMessage(type), innerException)
        {
            Path_to_Json = path_to_Json;
            ErrorType = type;
            Source = ien;
        }
        public new IndxElements Source;
        public string Path_to_Json = string.Empty;
        public TypeOfError ErrorType = TypeOfError.SomeThing;

        public string GetMessage()
        {
            switch (ErrorType)
            {
                case TypeOfError.Deleted:
                    return $"File {Path_to_Json} is deleted!";

                case TypeOfError.Invalid:
                    return $"File {Path_to_Json} is invalid!";
                case TypeOfError.CancelTask:
                    return $"Task was canceled!";
            }

            return $"Unknown TypeOfError: {ErrorType}";
        }

    }
    public class ExtendedTreeView : TreeView
    {
        public ExtendedTreeView()
            : base()
        {
            this.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(ICH);
        }

        void ICH(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SelectedItem != null)
            {
                if (SelectedItem.GetType() == Items.CurrentItem.GetType())
                    SetValue(SelectedItem_Property, SelectedItem);
            }
        }

        public object SelectedItem_
        {
            get { return (object)GetValue(SelectedItem_Property); }
            set { SetValue(SelectedItem_Property, value); }
        }
        public static readonly DependencyProperty SelectedItem_Property = DependencyProperty.Register("SelectedItem_", typeof(object), typeof(ExtendedTreeView), new UIPropertyMetadata(null));
    }
    public class MathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object result = value;

            if (value != null && targetType == typeof(double) &&
                double.TryParse((string)parameter,
                NumberStyles.Float, culture, out double parameterValue))
            {
                result = (double)value + parameterValue;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class JsonEncryptAttribute : Attribute
    {
    }
    public class EnumItemsSource : Collection<String>, IValueConverter
    {

        Type type;

        IDictionary<Object, Object> valueToNameMap;

        IDictionary<Object, Object> nameToValueMap;

        public Type Type
        {
            get { return this.type; }
            set
            {
                if (!value.IsEnum)
                    throw new ArgumentException("Type is not an enum.", "value");
                this.type = value;
                Initialize();
            }
        }

        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return this.valueToNameMap[value];
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return this.nameToValueMap[value];
        }

        void Initialize()
        {
            this.valueToNameMap = this.type
              .GetFields(BindingFlags.Static | BindingFlags.Public)
              .ToDictionary(fi => fi.GetValue(null), GetDescription);
            this.nameToValueMap = this.valueToNameMap
              .ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
            Clear();
            foreach (String name in this.nameToValueMap.Keys)
                Add(name);
        }

        static Object GetDescription(FieldInfo fieldInfo)
        {
            var descriptionAttribute =
              (DescriptionAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute));
            return descriptionAttribute != null ? descriptionAttribute.Description : fieldInfo.Name;
        }

    }
    //public class EncryptedStringPropertyResolver : DefaultContractResolver
    //{
    //    private readonly byte[] encryptionKeyBytes;

    //    public EncryptedStringPropertyResolver(string encryptionKey)
    //    {
    //        if (encryptionKey == null)
    //            throw new ArgumentNullException("encryptionKey");

    //        // Hash the key to ensure it is exactly 256 bits long, as required by AES-256
    //        using SHA256Managed sha = new SHA256Managed();
    //        encryptionKeyBytes =
    //            sha.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
    //    }

    //    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    //    {
    //        IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);

    //        // Find all string properties that have a [JsonEncrypt] attribute applied
    //        // and attach an EncryptedStringValueProvider instance to them
    //        foreach (JsonProperty prop in props.Where(p => p.PropertyType == typeof(string)))
    //        {
    //            PropertyInfo pi = type.GetProperty(prop.UnderlyingName);
    //            if (pi != null && pi.GetCustomAttribute(typeof(JsonEncryptAttribute), true) != null)
    //            {
    //                prop.ValueProvider =
    //                    new EncryptedStringValueProvider(pi, encryptionKeyBytes);
    //            }
    //        }

    //        return props;
    //    }

    //    class EncryptedStringValueProvider : IValueProvider
    //    {
    //        readonly PropertyInfo targetProperty;
    //        private readonly byte[] encryptionKey;

    //        public EncryptedStringValueProvider(PropertyInfo targetProperty, byte[] encryptionKey)
    //        {
    //            this.targetProperty = targetProperty;
    //            this.encryptionKey = encryptionKey;
    //        }

    //        // GetValue is called by Json.Net during serialization.
    //        // The target parameter has the object from which to read the unencrypted string;
    //        // the return value is an encrypted string that gets written to the JSON
    //        public object GetValue(object target)
    //        {
    //            string value = (string)targetProperty.GetValue(target);
    //            byte[] buffer = Encoding.UTF8.GetBytes(value);

    //            using MemoryStream inputStream = new MemoryStream(buffer, false);
    //            using MemoryStream outputStream = new MemoryStream();
    //            using AesManaged aes = new AesManaged { Key = encryptionKey };
    //            byte[] iv = aes.IV;  // first access generates a new IV
    //            outputStream.Write(iv, 0, iv.Length);
    //            outputStream.Flush();

    //            ICryptoTransform encryptor = aes.CreateEncryptor(encryptionKey, iv);
    //            using (CryptoStream cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write))
    //            {
    //                inputStream.CopyTo(cryptoStream);
    //            }

    //            return Convert.ToBase64String(outputStream.ToArray());
    //        }

    //        // SetValue gets called by Json.Net during deserialization.
    //        // The value parameter has the encrypted value read from the JSON;
    //        // target is the object on which to set the decrypted value.
    //        public void SetValue(object target, object value)
    //        {
    //            byte[] buffer = Convert.FromBase64String((string)value);

    //            using MemoryStream inputStream = new MemoryStream(buffer, false);
    //            using MemoryStream outputStream = new MemoryStream();
    //            using AesManaged aes = new AesManaged { Key = encryptionKey };
    //            byte[] iv = new byte[16];
    //            int bytesRead = inputStream.Read(iv, 0, 16);
    //            if (bytesRead < 16)
    //            {
    //                throw new CryptographicException("IV is missing or invalid.");
    //            }

    //            ICryptoTransform decryptor = aes.CreateDecryptor(encryptionKey, iv);
    //            using (CryptoStream cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read))
    //            {
    //                cryptoStream.CopyTo(outputStream);
    //            }

    //            string decryptedValue = Encoding.UTF8.GetString(outputStream.ToArray());
    //            targetProperty.SetValue(target, decryptedValue);
    //        }

    //    }
    //}

}