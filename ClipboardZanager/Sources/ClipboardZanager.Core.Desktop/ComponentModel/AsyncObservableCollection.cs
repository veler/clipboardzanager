using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using ClipboardZanager.Shared.Core;

namespace ClipboardZanager.Core.Desktop.ComponentModel
{
    /// <summary>
    /// Provides an <see cref="ObservableCollection{T}"/> that raise the CollectionChanged event from any thread.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class AsyncObservableCollection<T> : ObservableCollection<T>
    {
        #region Fields

        [NonSerialized]
        private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current;

        [NonSerialized]
        private bool _suppressNotification;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="AsyncObservableCollection{T}"/> class.
        /// </summary>
        public AsyncObservableCollection()
        {
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="AsyncObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="list">The item source.</param>
        public AsyncObservableCollection(IEnumerable<T> list)
            : base(list)
        {
        }

        #endregion

        #region Methods

        public void AddRange(IEnumerable<T> list)
        {
            Requires.NotNull(list, nameof(list));

            _suppressNotification = true;
            foreach (var item in list)
            {
                Add(item);
            }
            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list.ToList()));
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
            {
                if (SynchronizationContext.Current == _synchronizationContext || CoreHelper.IsUnitTesting())
                {
                    // Execute the CollectionChanged event on the current thread
                    RaiseCollectionChanged(e);
                }
                else
                {
                    // Raises the CollectionChanged event on the creator thread
                    _synchronizationContext.Send(RaiseCollectionChanged, e);
                }
            }
        }

        private void RaiseCollectionChanged(object param)
        {
            // We are in the creator thread, call the base implementation directly
            base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (!_suppressNotification)
            {
                if (SynchronizationContext.Current == _synchronizationContext || CoreHelper.IsUnitTesting())
                {
                    // Execute the PropertyChanged event on the current thread
                    RaisePropertyChanged(e);
                }
                else
                {
                    // Raises the PropertyChanged event on the creator thread
                    _synchronizationContext.Send(RaisePropertyChanged, e);
                }
            }
        }

        private void RaisePropertyChanged(object param)
        {
            // We are in the creator thread, call the base implementation directly
            base.OnPropertyChanged((PropertyChangedEventArgs)param);
        }

        #endregion
    }
}
