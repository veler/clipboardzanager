using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Models;
using ClipboardZanager.Strings;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace ClipboardZanager.ComponentModel.UI.Converters
{
    /// <summary>
    /// Convert a <see cref="DataEntry"/> to a <see cref="string"/> descriptive value.
    /// </summary>
    [ValueConversion(typeof(DataEntry), typeof(string))]
    internal class DataEntryToDescriptiveTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataEntry)
            {
                var dataEntry = (DataEntry)value;
                var language = LanguageManager.GetInstance();

                switch (dataEntry.Thumbnail.Type)
                {
                    case Core.Desktop.Enums.ThumbnailDataType.Bitmap:
                        return language.PasteBarWindow.DataImage;

                    case Core.Desktop.Enums.ThumbnailDataType.Files:
                        var filesSource = dataEntry.Thumbnail.GetFilesPath();
                        var fileList = string.Empty;

                        foreach (var filePath in filesSource)
                        {
                            if (filePath.Contains("\\"))
                            {
                                fileList += Path.GetFileName(filePath) + "; ";
                            }
                            else
                            {
                                fileList += filePath + "; ";
                            }
                        }

                        return string.Format(language.PasteBarWindow.DataFile, fileList);

                    case Core.Desktop.Enums.ThumbnailDataType.Link:
                        var link = DataHelper.FromBase64<Link>(dataEntry.Thumbnail.Value);
                        return string.Format(language.PasteBarWindow.DataLink, link.Title, link.Uri);

                    case Core.Desktop.Enums.ThumbnailDataType.String:
                        return string.Format(language.PasteBarWindow.DataText, DataHelper.FromBase64<string>(dataEntry.Thumbnail.Value));

                    case Core.Desktop.Enums.ThumbnailDataType.Unknow:
                        return language.PasteBarWindow.DataUnknow;
                }
            }
            throw new ArgumentException("DataEntry value needed", nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
