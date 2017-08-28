using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using ClipboardZanager.Core.Desktop.ComponentModel;
using ClipboardZanager.Core.Desktop.Enums;
using ClipboardZanager.Core.Desktop.Models;
using System.Text;
using ClipboardZanager.Core.Desktop.Interop;

namespace ClipboardZanager.ComponentModel.UI.Converters
{
    /// <summary>
    /// Convert a partial file path to a displayable <see cref="BitmapImage"/> value.
    /// </summary>
    [ValueConversion(typeof(Thumbnail), typeof(BitmapImage))]
    internal class FilePathToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int intParameter;
            var thumbnail = value as Thumbnail;
            int.TryParse(parameter.ToString(), out intParameter);

            if (thumbnail?.Value == null || thumbnail.Type != ThumbnailDataType.Files)
            {
                return DependencyProperty.UnsetValue;
            }

            var filePath = thumbnail.GetFilePath(intParameter);

            if (!File.Exists(filePath))
            {
                return DependencyProperty.UnsetValue;
            }

            ushort uicon;
            var strFilePath = new StringBuilder(filePath);
            var iconHandle = NativeMethods.ExtractAssociatedIcon(CoreHelper.GetCurrentModuleHandle(), strFilePath, out uicon);

            if (iconHandle == IntPtr.Zero)
            {
                return DependencyProperty.UnsetValue;
            }

            using (var sysicon = Icon.FromHandle(iconHandle))
            {
                if (sysicon != null)
                {
                    return Imaging.CreateBitmapSourceFromHIcon(sysicon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(Consts.FileIconsSize, Consts.FileIconsSize));
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
