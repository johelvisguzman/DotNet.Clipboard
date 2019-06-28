namespace DotNet.Clipboard.Infrastructure
{
    using Native;
    using Properties;
    using System;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Cryptography;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Contains various utility helpers.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Converts the specified <paramref name="bitmap" /> to a <see cref="BitmapSource"/>.
        /// </summary>
        /// <param name="bitmap">The bitmap to convert.</param>
        /// <returns>The converted <see cref="BitmapSource"/>.</returns>
        public static BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        /// <summary>
        /// Converts the specified <paramref name="imageSource" /> to a byte array.
        /// </summary>
        /// <param name="imageSource">The image source.</param>
        /// <param name="encoder">The encoder. If <c>null</c>, a default <see cref="PngBitmapEncoder"/> encoder will be used instead.</param>
        /// <returns>The converted image in bytes.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="imageSource"/> is <c>null</c>.</exception>
        public static byte[] ImageSourceToByteArray(ImageSource imageSource, BitmapEncoder encoder = null)
        {
            if (imageSource == null)
                throw new ArgumentNullException(nameof(imageSource));

            if (encoder == null)
                encoder = new PngBitmapEncoder();

            byte[] bytes = null;
            var bitmapSource = imageSource as BitmapSource;

            if (bitmapSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }

            return bytes;
        }

        /// <summary>
        /// Converts the specified <paramref name="array" /> to image source object.
        /// </summary>
        /// <param name="array">The byte array.</param>
        /// <returns>The converted image source.</returns>
        public static ImageSource ByteArrayToImageSource(byte[] array)
        {
            using (var stream = new MemoryStream(array))
            {
                stream.Seek(0, SeekOrigin.Begin);

                var image = new BitmapImage();

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();

                return image;
            }
        }

        /// <summary>
        /// Converts the specified <paramref name="obj" /> to a byte array.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="obj"/> is <c>null</c>.</exception>
        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var binaryFormatter = new BinaryFormatter();

            using (var stream = new MemoryStream())
            {
                stream.Seek(0, SeekOrigin.Begin);
                binaryFormatter.Serialize(stream, obj);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Converts the specified <paramref name="data" /> to an object.
        /// </summary>
        /// <param name="data">The data in bytes.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        public static object ByteArrayToObject(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var bf = new BinaryFormatter();

            using (var stream = new MemoryStream(data))
            {
                stream.Seek(0, SeekOrigin.Begin);
                return bf.Deserialize(stream);
            }
        }

        /// <summary>
        /// Verifies that specified images are equal.
        /// </summary>
        /// <param name="imageSourceA">The first image to compare.</param>
        /// <param name="imageSourceB">The second image to compare.</param>
        /// <returns><c>true</c> if both images are identical; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// <para><paramref name="imageSourceA"/> is <c>null</c>.</para>
        /// <para>-- or -- </para>
        /// <para><paramref name="imageSourceB"/> is <c>null</c>.</para>
        /// </exception>
        public static bool AreEqual(ImageSource imageSourceA, ImageSource imageSourceB)
        {
            if (imageSourceA == null)
                throw new ArgumentNullException(nameof(imageSourceA));

            if (imageSourceB == null)
                throw new ArgumentNullException(nameof(imageSourceB));

            if (!AreAlmostEqual(imageSourceA.Width, imageSourceB.Width))
                return false;

            if (!AreAlmostEqual(imageSourceA.Height, imageSourceB.Height))
                return false;

            var imageADataInBytes = ImageSourceToByteArray(imageSourceA);
            var imageBDataInBytes = ImageSourceToByteArray(imageSourceB);
            var shaM = new SHA256Managed();
            var hash1 = shaM.ComputeHash(imageADataInBytes);
            var hash2 = shaM.ComputeHash(imageBDataInBytes);

            var result = !hash1.Where((nextByte, index) => nextByte != hash2[index]).Any();

            return result;
        }

        /// <summary>
        /// Verifies that specified values are almost equal.
        /// </summary>
        /// <param name="x">The value1 to compare.</param>
        /// <param name="y">The value2 to compare.</param>
        /// <returns><c>true</c> if both values are almost equal; otherwise, <c>false</c>.</returns>
        public static bool AreAlmostEqual(double x, double y)
        {
            var epsilon = Math.Max(Math.Abs(x), Math.Abs(y)) * 1E-15;

            return Math.Abs(x - y) <= epsilon;
        }

        /// <summary>
        /// Gets the mouse position.
        /// </summary>
        /// <returns>The mouse position.</returns>
        public static Point GetMousePosition()
        {
            POINT p;
            NativeMethods.GetCursorPos(out p);

            return new Point(p.X, p.Y);
        }

        /// <summary>
        /// Gets the screen rectangle from the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>The screen rectangle at the specified point.</returns>
        public static Tuple<Rect, Rect> GetScreenRectFromPoint(Point point)
        {
            var pt = new POINT((int)point.X, (int)point.Y);
            var mh = NativeMethods.MonitorFromPoint(pt, (int)NativeMethods.MFW.MONITOR_DEFAULTTONEAREST);
            var mi = new MONITORINFO();

            NativeMethods.GetMonitorInfo(mh, mi);

            var monitorRect = new Rect(mi.rcMonitor.Left, mi.rcMonitor.Top, mi.rcMonitor.Width, mi.rcMonitor.Height);
            var workingAreaRect = new Rect(mi.rcWork.Left, mi.rcWork.Top, mi.rcWork.Width, mi.rcWork.Height);

            return new Tuple<Rect, Rect>(monitorRect, workingAreaRect);
        }

        /// <summary>
        /// Gets the application task bar rectangle.
        /// </summary>
        /// <returns>The application task bar rectangle</returns>
        public static Rect GetAppTaskBarRect()
        {
            var d = new APPBARDATA();

            NativeMethods.SHAppBarMessage((int)NativeMethods.ABM.GETTASKBARPOS, ref d);

            return new Rect(d.rc.Left, d.rc.Top, d.rc.Width, d.rc.Height);
        }

        /// <summary>
        /// Gets the application data directory.
        /// </summary>
        /// <returns>The application data directory.</returns>
        public static string GetAppDataDirectory()
        {
            var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" + assemblyName;
        }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <returns></returns>
        public static string GetConnectionString()
        {
            var connection = ConfigurationManager.ConnectionStrings["DefaultConnection"];
            if (connection == null)
                return string.Empty;

            return connection.ConnectionString.Replace("|DataDirectory|", GetAppDataDirectory());
        }

        /// <summary>
        /// Creates the file.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>The name of the file.</returns>
        public static string CreateBitmapImageFile(BitmapSource image)
        {
            var fileRootDirectory = GetAppDataDirectory() + "\\" + Resources.DragFilesFolderName;

            if (!Directory.Exists(fileRootDirectory))
                Directory.CreateDirectory(fileRootDirectory);

            var fileRootDirectoryInfo = new FileInfo(fileRootDirectory);
            var fileCount = fileRootDirectoryInfo.Directory?.GetFiles("*", SearchOption.AllDirectories).Length;
            var fileName = "image" + "_" + fileCount + ".png";
            var filePath = fileRootDirectory + "\\" + fileName;

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                var encoder = new PngBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(stream);
            }

            return filePath;
        }

        /// <summary>
        /// Creates the file.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The name of the file.</returns>
        public static string CreateTextFile(string text)
        {
            var fileRootDirectory = GetAppDataDirectory() + "\\" + Resources.DragFilesFolderName;

            if (!Directory.Exists(fileRootDirectory))
                Directory.CreateDirectory(fileRootDirectory);

            var fileRootDirectoryInfo = new FileInfo(fileRootDirectory);
            var fileCount = fileRootDirectoryInfo.Directory?.GetFiles("*", SearchOption.AllDirectories).Length;
            var fileName = "text" + "_" + fileCount + ".txt";
            var filePath = fileRootDirectory + "\\" + fileName;

            using (var stream = new StreamWriter(File.Open(filePath, FileMode.Create)))
            {
                stream.WriteLine(text);
            }

            return filePath;
        }

        /// <summary>
        /// Creates a file from the specified <paramref name="viewModel"/>.
        /// </summary>
        /// <returns>The created file.</returns>
        public static string CreateFile(ViewModels.ClipViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            if (viewModel.Format == DataFormats.Bitmap)
                return Utils.CreateBitmapImageFile((BitmapSource)viewModel.Data);

            if (viewModel.Format == DataFormats.Text)
                return Utils.CreateTextFile((string)viewModel.Data);

            return string.Empty;
        }

        /// <summary>
        /// Converts the specified <paramref name="hotkey" /> to hotkey.
        /// </summary>
        /// <param name="hotkey">The hotkey string.</param>
        /// <returns>The converted hotkey.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="hotkey"/> is <c>null</c> or empty string.</exception>
        public static Tuple<Key, ModifierKeys> ConvertStringToHotKey(string hotkey)
        {
            if (string.IsNullOrEmpty(hotkey))
                return null;

            try
            {
                var modKeys = ModifierKeys.None;
                var allKeys = hotkey.Split('+');
                var key = (Key)Enum.Parse(typeof(Key), allKeys.Last());

                foreach (var modifierKey in allKeys.Take(allKeys.Count() - 1))
                {
                    modKeys |= (ModifierKeys)Enum.Parse(typeof(ModifierKeys), modifierKey);
                }

                return new Tuple<Key, ModifierKeys>(key, modKeys);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Converts the specified key and modifier to a hotkey string.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="modifierKeys">The modifier keys.</param>
        /// <returns>The converted hotkey string.</returns>
        public static string ConvertHotKeyToString(Key key, ModifierKeys modifierKeys)
        {
            return modifierKeys + "+" + key;
        }

        /// <summary>
        /// Get the size of the specified file.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The size of the specified file.</returns>
        // https://stackoverflow.com/questions/281640/how-do-i-get-a-human-readable-file-size-in-bytes-abbreviation-using-net
        public static string GetFileSize(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
                return string.Empty;

            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = new FileInfo(fileName).Length;
            var order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
