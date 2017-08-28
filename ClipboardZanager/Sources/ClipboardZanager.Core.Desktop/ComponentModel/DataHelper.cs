using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media.Imaging;
using ClipboardZanager.Core.Desktop.Interop.Structs;
using ClipboardZanager.Shared.Core;

namespace ClipboardZanager.Core.Desktop.ComponentModel
{
    /// <summary>
    /// Provides a set of functions designed to convert data
    /// </summary>
    internal static class DataHelper
    {
        /// <summary>
        /// Convert any serializable object to a <see cref="byte"/> array
        /// </summary>
        /// <typeparam name="T">Represents a <see cref="Type"/> that corresponds to a class</typeparam>
        /// <param name="value">The value to convert</param>
        /// <returns>Returns a <see cref="byte"/> array. If the value is null, returns null.</returns>
        internal static byte[] ToByteArray<T>(T value) where T : class
        {
            if (value == null || !value.GetType().IsSerializable)
            {
                return null;
            }

            using (var memoryStream = new MemoryStream())
            {
                var serializer = new BinaryFormatter();
                serializer.Serialize(memoryStream, value);
                memoryStream.Flush();
                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Convert a <see cref="byte"/> array to the specified type
        /// </summary>
        /// <typeparam name="T">The expected data type</typeparam>
        /// <param name="array">the <see cref="byte"/> array</param>
        /// <returns>Returns the converted value. If the array is null, or if the data cannot be converted, returns the default value or null.</returns>
        internal static T FromByteArray<T>(byte[] array) where T : class
        {
            if (array == null)
            {
                return default(T);
            }

            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(array, 0, array.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var deserializer = new BinaryFormatter();
                var obj = deserializer.Deserialize(memoryStream);
                return obj as T;
            }
        }

        /// <summary>
        /// Convert a value to a <see cref="byte"/> array and then to a base64 <see cref="string"/>.
        /// </summary>
        /// <typeparam name="T">Represents a <see cref="Type"/> that corresponds to a class</typeparam>
        /// <param name="value">The value to convert</param>
        /// <returns>Returns a <see cref="string"/>. If the value is null, throw an exception.</returns>
        internal static string ToBase64<T>(T value) where T : class
        {
            return Convert.ToBase64String(ToByteArray(value));
        }

        /// <summary>
        /// Convert a <see cref="byte"/> array and then to a base64 <see cref="string"/>.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>Returns a <see cref="string"/>. If the value is null, throw an exception.</returns>
        internal static string ToBase64(byte[] value)
        {
            return Convert.ToBase64String(value);
        }

        /// <summary>
        /// Convert a base64 <see cref="string"/> to the specified <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">Represents a <see cref="Type"/> that corresponds to a class</typeparam>
        /// <param name="base64String">The base64 string to convert</param>
        /// <returns>Returns the converted value. If the value is null, throw an exception.</returns>
        internal static T FromBase64<T>(string base64String) where T : class
        {
            return FromByteArray<T>(Convert.FromBase64String(base64String));
        }

        /// <summary>
        /// Convert a base64 <see cref="string"/> to <see cref="byte"/> array.
        /// </summary>
        /// <param name="base64String">The base64 string to convert</param>
        /// <returns>Returns the converted value. If the value is null, throw an exception.</returns>
        internal static byte[] ByteArrayFromBase64(string base64String)
        {
            return Convert.FromBase64String(base64String);
        }

        /// <summary>
        /// Convert an <see cref="IntPtr"/> to a <see cref="int"/>
        /// </summary>
        /// <param name="intPtr">The <see cref="IntPtr"/> to convert</param>
        /// <returns>The <see cref="int"/></returns>
        internal static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        /// <summary>
        /// Comvert a <see cref="Bitmap"/> to a <see cref="BitmapImage"/>
        /// </summary>
        /// <param name="bitmap">The <see cref="Image"/></param>
        /// <param name="maxHeight">The maximum height of the picture. Use null to don't specify a maximum height</param>
        /// <returns>The <see cref="BitmapImage"/></returns>
        internal static BitmapImage BitmapToBitmapImage(Image bitmap, int? maxHeight)
        {
            Requires.NotNull(bitmap, nameof(bitmap));

            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);

                stream.Position = 0;
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;

                if (maxHeight.HasValue)
                {
                    image.DecodePixelHeight = maxHeight.Value;
                }

                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }
        /// <summary>
        /// Convert a <see cref="BitmapSource"/> to a <see cref="byte"/> array
        /// </summary>
        /// <param name="value">The <see cref="BitmapSource"/> to convert</param>
        /// <returns>Returns a <see cref="byte"/> array. If the value is null, returns null.</returns>
        internal static byte[] BitmapSourceToByteArray(BitmapSource value)
        {
            if (value == null)
            {
                return null;
            }

            using (var memoryStream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(value));
                encoder.Save(memoryStream);
                memoryStream.Position = 0;

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Convert a <see cref="byte"/> array to a <see cref="BitmapSource"/>
        /// </summary>
        /// <param name="array">The <see cref="byte"/> array to convert</param>
        /// <returns>Returns a <see cref="BitmapSource"/>. If the array is null, or if the data cannot be converted, returns the default value or null.</returns>
        internal static BitmapSource ByteArrayToBitmapSource(byte[] array)
        {
            if (array == null)
            {
                return default(BitmapSource);
            }

            using (var memoryStream = new MemoryStream(array))
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        /// <summary>
        /// Convert a DeviceIndependentBitmap to a <see cref="BitmapSource"/>
        /// </summary>
        /// <param name="memoryStream">The <see cref="MemoryStream"/> to convert</param>
        /// <param name="maxHeight">The maximum height of the picture. Use null to don't specify a maximum height</param>
        /// <returns>Returns a <see cref="BitmapSource"/>. If the memoryStream is null, returns null.</returns>
        internal static BitmapSource DeviceIndependentBitmapToBitmapSource(MemoryStream memoryStream, int? maxHeight)
        {
            if (memoryStream == null)
            {
                return null;
            }

            var bytes = memoryStream.ToArray();
            var resultMemoryStream = new MemoryStream();
            var width = BitConverter.ToInt32(bytes, 4);
            var height = BitConverter.ToInt32(bytes, 8);
            var bpp = BitConverter.ToInt16(bytes, 14);

            if (bpp == 32)
            {
                // Potentially, this way of convert, by using System.Drawing.Bitmap (non-WPF) can keep the transparency of the picture.
                var gch = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                Bitmap bmp = null;
                try
                {
                    var ptr = new IntPtr((long)gch.AddrOfPinnedObject() + 40);
                    bmp = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, ptr);
                    bmp.MakeTransparent(Color.Transparent);
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipX);

                    bmp.Save(resultMemoryStream, ImageFormat.Png);
                }
                finally
                {
                    gch.Free();
                    if (bmp != null)
                    {
                        bmp.Dispose();
                    }
                }
            }
            else
            {
                // This way does not keep transparency
                var infoHeader = FromByteArrayToStruct<BitmapInfoHeader>(bytes);

                var fileHeaderSize = Marshal.SizeOf(typeof(BitmapFileHeader));
                var infoHeaderSize = infoHeader.biSize;
                var fileSize = fileHeaderSize + infoHeader.biSize + infoHeader.biSizeImage;

                var fileHeader = new BitmapFileHeader
                {
                    bfType = BitmapFileHeader.BM,
                    bfSize = fileSize,
                    bfReserved1 = 0,
                    bfReserved2 = 0,
                    bfOffBits = fileHeaderSize + infoHeaderSize + infoHeader.biClrUsed * 4
                };

                var fileHeaderBytes = StructToByteArray(fileHeader);

                resultMemoryStream.Write(fileHeaderBytes, 0, fileHeaderSize);
                resultMemoryStream.Write(bytes, 0, bytes.Length);
                resultMemoryStream.Seek(0, SeekOrigin.Begin);

                resultMemoryStream.Flush();
            }

            var result = new BitmapImage();
            result.BeginInit();
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.CreateOptions = BitmapCreateOptions.PreservePixelFormat;

            if (maxHeight.HasValue)
            {
                result.DecodePixelHeight = maxHeight.Value;
            }

            result.StreamSource = resultMemoryStream;
            result.EndInit();
            result.Freeze();

            return result;
        }

        /// <summary>
        /// Convert a <see cref="byte"/> array to the specified structure
        /// </summary>
        /// <typeparam name="T">Represents a <see cref="Type"/> that corresponds to a structure</typeparam>
        /// <param name="array">The <see cref="byte"/> array</param>
        /// <returns>Returns the converted value.</returns>
        private static T FromByteArrayToStruct<T>(byte[] array) where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(array, 0, ptr, size);
            var obj = Marshal.PtrToStructure(ptr, typeof(T));
            if (ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(ptr);
            }

            return (T)obj;
        }

        /// <summary>
        /// Convert a structure to a <see cref="byte"/> array
        /// </summary>
        /// <typeparam name="T">Represents a <see cref="Type"/> that corresponds to a structure</typeparam>
        /// <param name="value">The value to convert</param>
        /// <returns>Returns a <see cref="byte"/> array.</returns>
        private static byte[] StructToByteArray<T>(T value) where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, ptr, true);
            var bytes = new byte[size];
            Marshal.Copy(ptr, bytes, 0, size);
            if (ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(ptr);
            }

            return bytes;
        }
    }
}
