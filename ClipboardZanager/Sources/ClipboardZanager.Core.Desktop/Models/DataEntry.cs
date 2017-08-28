using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ClipboardZanager.Core.Desktop.ComponentModel;

namespace ClipboardZanager.Core.Desktop.Models
{
    /// <summary>
    /// Represents the information about a data from the clipboard.
    /// </summary>
    [Serializable]
    internal sealed class DataEntry : DataEntryBase, INotifyPropertyChanged
    {
        #region Fields

        internal string _icon;

        [NonSerialized]
        private BitmapImage _iconBitmap = null;

        [NonSerialized]
        private DispatcherTimer _timer;

        [NonSerialized]
        private bool _isMoreInfoDisplayed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the linked <see cref="Thumbnail"/>
        /// </summary>
        public Thumbnail Thumbnail { get; set; }

        /// <summary>
        /// Gets or sets the icon of the application from where the data is coming from.
        /// </summary>
        public BitmapImage Icon
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_icon))
                {
                    return null;
                }

                return (BitmapImage)DataHelper.ByteArrayToBitmapSource(DataHelper.ByteArrayFromBase64(_icon));
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _icon = DataHelper.ToBase64(DataHelper.BitmapSourceToByteArray(value));
                _iconBitmap = value;
            }
        }

        /// <summary>
        /// Gets the icon to display in the software.
        /// </summary>
        public BitmapImage DisplayedIcon
        {
            get
            {
                // By binding this property to the UI, we are sure that the image is loaded one time for all the screens (and so all the windows that will display this picture).
                if (_iconBitmap == null)
                {
                    _iconBitmap = Icon;
                }

                return _iconBitmap;
            }
        }

        /// <summary>
        /// Gets or sets a value that defines whether this data from the clipboard has been cutted or just copied.
        /// </summary>
        public bool IsCut { get; set; }

        /// <summary>
        /// Gets or sets a value that defines whether this data can be synchronized in the cloud or not.
        /// </summary>
        public bool CanSynchronize { get; set; }

        /// <summary>
        /// Gets or sets a value that defines whether the icon comes from a Windows Store app or not.
        /// </summary>
        public bool IconIsFromWindowStore { get; set; }

        /// <summary>
        /// Gets or sets a value that defines whether the More Information panel is displayed or not.
        /// </summary>
        public bool IsMoreInfoDisplayed
        {
            get { return _isMoreInfoDisplayed; }
            set { _isMoreInfoDisplayed = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new instance of the <see cref="DataEntry"/> class.
        /// </summary>
        public DataEntry()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMinutes(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        #endregion

        #region Handled Methods

        private void Timer_Tick(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(Date));
        }

        #endregion

        #region Methods

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
