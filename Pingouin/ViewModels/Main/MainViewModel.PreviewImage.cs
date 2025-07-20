using System;
using System.IO;
using System.Drawing;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using StudioElevenLib.Level5.Image;
using StudioElevenLib.Tools;
using Pingouin.Models;

namespace Pingouin.ViewModels
{
    public partial class MainViewModel
    {
        private ImageSource _previewImage;
        /// <summary>
        /// Gets the image source for the preview panel.
        /// This property is updated asynchronously when an image file is selected.
        /// </summary>
        public ImageSource PreviewImage
        {
            get => _previewImage;
            private set => SetProperty(ref _previewImage, value);
        }

        /// <summary>
        /// Asynchronously updates the image preview based on the currently selected item.
        /// It runs on a background thread to prevent locking up the UI during file decoding.
        /// </summary>
        private async Task UpdatePreviewImageAsync()
        {
            // Reset the preview image first to clear the old one.
            PreviewImage = null;

            var fileItem = SelectedItem as FileItemViewModel;

            // Do nothing if the selected item is not a single, valid image file.
            if (fileItem == null || fileItem.Type != "Image File")
            {
                return;
            }

            try
            {
                // The Tag property of the ViewModel item holds the raw file data reference.
                var fileDataPair = (System.Collections.Generic.KeyValuePair<string, SubMemoryStream>)fileItem.Tag;
                var subStream = fileDataPair.Value;

                // File reading and image decoding can be slow, so we offload this work to a background thread.
                var imageSource = await Task.Run(() =>
                {
                    // Ensure the file's byte data is loaded into memory from the archive if it hasn't been already.
                    if (!subStream.IsContentLoaded)
                    {
                        subStream.Read();
                    }

                    // Use the library's method to decode the byte array into a Bitmap object.
                    Bitmap bitmap = IMGC.ToBitmap(subStream.ByteContent);

                    // Convert the System.Drawing.Bitmap to a WPF-compatible ImageSource.
                    return BitmapToImageSource(bitmap);
                });

                // After the task completes, update the property on the UI thread.
                PreviewImage = imageSource;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating image preview: {ex.Message}");
                PreviewImage = null;
            }
        }

        /// <summary>
        /// Converts a System.Drawing.Bitmap into a WPF-compatible BitmapSource.
        /// </summary>
        /// <param name="bitmap">The System.Drawing.Bitmap to convert.</param>
        /// <returns>A frozen, UI-thread-safe BitmapSource, or null if the input is null.</returns>
        private BitmapSource BitmapToImageSource(Bitmap bitmap)
        {
            if (bitmap == null) return null;

            using (var memory = new MemoryStream())
            {
                // Save the bitmap to a memory stream. PNG is used as it supports transparency.
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Load the image fully at this point.
                bitmapImage.EndInit();

                // Freeze the image to make it cross-thread accessible and improve performance.
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}