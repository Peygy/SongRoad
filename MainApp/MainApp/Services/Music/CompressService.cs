using MainApp.Models.Music;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace MainApp.Services.Music
{
    public sealed class CompressService
    {
        /// <summary>
        /// Method for compress image file
        /// </summary>
        /// <param name="imageFile">Image file</param>
        /// <returns>Compressed music track image object</returns>
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
