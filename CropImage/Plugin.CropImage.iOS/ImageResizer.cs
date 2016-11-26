using CoreGraphics;
using System;
using System.Drawing;
using UIKit;

namespace Plugin.CropImage
{
    public static class ImageResizer
    {
        public static byte[] ResizeImage(UIImage image,nfloat width,nfloat height)
        {
            
            UIImageOrientation orientation = image.Orientation;

            //create a 24bit RGB image
            using (CGBitmapContext context = new CGBitmapContext(IntPtr.Zero,
                                                 (nint)width, (nint)height, 8,
                                                (int) (4 * width), CGColorSpace.CreateDeviceRGB(),
                                                 CGImageAlphaInfo.PremultipliedFirst))
            {

                RectangleF imageRect = new RectangleF(0, 0, (float)width, (float)height);

                // draw the image
                context.DrawImage(imageRect, image.CGImage);

                UIImage resizedImage = UIImage.FromImage(context.ToImage(), 0, orientation);

                // save the image as a jpeg
                return resizedImage.AsJPEG().ToArray();
            }
        }

        public static byte[] ResizeImageToMedium(byte[] imageData)
        {
            UIImage originalImage = ImageFromByteArray(imageData);
            return ResizeImage(originalImage, originalImage.Size.Width / 2, originalImage.Size.Height / 2);
        }

        public static byte[] ResizeImageToSmall(byte[] imageData)
        {
            UIImage originalImage = ImageFromByteArray(imageData);
            return ResizeImage(originalImage, originalImage.Size.Width / 4, originalImage.Size.Height / 4);
        }

        public static UIImage ImageFromByteArray(byte[] data)
        {
            if (data == null)
            {
                return null;
            }

           UIImage image;
            try
            {
                image = new UIImage(Foundation.NSData.FromArray(data));
            }
            catch (Exception e)
            {
                Console.WriteLine("Image load failed: " + e.Message);
                return null;
            }
            return image;
        }
    }
}
