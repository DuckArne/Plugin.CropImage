using Android.Graphics;
using System.IO;

namespace Plugin.CropImage
{
    public static class ImageResizer
    {
        public static byte[] ResizeImageToMedium(byte[] imageData)
        {
            // Load the bitmap
            Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);


            Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, originalImage.Width / 2, originalImage.Height / 2, false);

            using (MemoryStream ms = new MemoryStream())
            {
                resizedImage.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
                return ms.ToArray();
            }
        }
        public static byte[] ResizeImageToSmall(byte[] imageData)
        {
            // Load the bitmap
            Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);


            Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, originalImage.Width / 4, originalImage.Height / 4, false);

            using (MemoryStream ms = new MemoryStream())
            {
                resizedImage.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
                return ms.ToArray();
            }
        }
    }
}