using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Enums;
using ClipboardZanager.Core.Desktop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace ClipboardZanager.ComponentModel.UI.Converters
{
    /// <summary>
    /// Convert a <see cref="Thumbnail"/> to a displayable <see cref="object"/> value.
    /// </summary>
    [ValueConversion(typeof(Thumbnail), typeof(object))]
    internal class ThumbnailToValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var thumbnail = value as Thumbnail;
            var parameterString = parameter as string;
            if (parameterString == null)
            {
                return DependencyProperty.UnsetValue;
            }

            var parameterStringLower = parameterString.ToLower();

            if (thumbnail?.Value == null)
            {
                return DependencyProperty.UnsetValue;
            }

            ThumbnailDataType parameterValue;
            if (!Enum.TryParse(parameterString, true, out parameterValue))
            {
                if (parameterStringLower == "linktitle" || parameterStringLower == "linkuri")
                {
                    parameterValue = ThumbnailDataType.Link;
                }
            }

            if (parameterValue != thumbnail.Type)
            {
                return null;
            }

            switch (parameterValue)
            {
                case ThumbnailDataType.Unknown:
                    return null;

                case ThumbnailDataType.Link:
                    var link = DataHelper.FromBase64<Link>(thumbnail.Value);
                    if (parameterStringLower == "linktitle")
                    {
                        return link.Title;
                    }
                    return link.Uri;

                case ThumbnailDataType.String:
                    return DataHelper.FromBase64<string>(thumbnail.Value);

                case ThumbnailDataType.Files:
                    var filesSource = thumbnail.GetFilesPath();
                    var fileNames = new List<string>();

                    foreach (var filePath in filesSource.Take(3))
                    {
                        if (filePath.Contains("\\"))
                        {
                            fileNames.Add(Path.GetFileName(filePath));
                        }
                        else
                        {
                            fileNames.Add(filePath);
                        }
                    }

                    if (filesSource.Count > 3)
                    {
                        fileNames.Add("...");
                    }

                    return fileNames;

                case ThumbnailDataType.Bitmap:
                    return DataHelper.ByteArrayToBitmapSource(DataHelper.ByteArrayFromBase64(thumbnail.Value));

                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
