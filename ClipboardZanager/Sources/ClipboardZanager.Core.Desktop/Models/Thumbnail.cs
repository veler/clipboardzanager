using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Enums;

namespace ClipboardZanager.Core.Desktop.Models
{
    /// <summary>
    /// Represents a thumbnail of a data of the clipboard.
    /// </summary>
    [Serializable]
    internal sealed class Thumbnail
    {
        [NonSerialized]
        private AsyncObservableCollection<string> _fileList;

        /// <summary>
        /// Gets or sets the value of the thumbnail.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the type of the value.
        /// </summary>
        public ThumbnailDataType Type { get; set; }

        /// <summary>
        /// If the value is a file list, gets one of the 3 files.
        /// </summary>
        /// <param name="index">The index, between 0 and 2</param>
        /// <returns>The file path</returns>
        internal string GetFilePath(int index)
        {
            if (index > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var files = GetFilesPath();

            if (files?.Count > index)
            {
                return files[index];
            }

            return string.Empty;
        }

        /// <summary>
        /// If the value is a file list, gets the list of file.
        /// </summary>
        /// <returns>The list of file path</returns>
        internal AsyncObservableCollection<string> GetFilesPath()
        {
            if (Type != ThumbnailDataType.Files)
            {
                return null;
            }

            if (_fileList == null)
            {
                _fileList = new AsyncObservableCollection<string>(DataHelper.FromBase64<List<string>>(Value));
            }

            return _fileList;
        }
    }
}
