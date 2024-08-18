using MainApp.Models.Music;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace MainApp.Services.Music
{
    /// <summary>
    /// The service for compression of image file.
    /// </summary>
    public sealed class CompressService
    {
        /// <summary>
        /// Compresses image file.
        /// </summary>
        /// <param name="imageFile">The image file model.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, 
        /// containing a <see cref="TrackImageModel"/> indicating whether the compression was successful.
        /// </returns>
        public static async Task<TrackImageModel> CompressImageFileAsync(IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                using var inputStream = imageFile.OpenReadStream();
                using var image = await Image.LoadAsync(inputStream);

                var encoder = new JpegEncoder
                {
                    Quality = 75
                };

                using var outputStream = new MemoryStream();
                await image.SaveAsJpegAsync(outputStream, encoder);

                return new TrackImageModel
                {
                    ContentType = "image/jpeg",
                    ImageData = outputStream.ToArray()
                };
            }

            return new TrackImageModel();
        }
    }
}
