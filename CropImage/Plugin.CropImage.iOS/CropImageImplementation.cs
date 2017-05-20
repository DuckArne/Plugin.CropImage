using CoreGraphics;
using Foundation;
using Plugin.CropImage.Abstractions;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using UIKit;

namespace Plugin.CropImage {
    /// <summary>
    /// Implementation for CropImage
    /// </summary>
    public class CropImageImplementation : ICropImage
    {
        /// <summary>
        /// Crops an image by the BoundingBox and returns in the size you specify.
        /// The new image will be saved on the device.
        /// </summary>
        /// <param name="originalSourcePath">The original SourcePath to a file on Device</param>
        /// <param name="boundingBox">The image box that will be used to crop</param>
        /// <param name="width">Width of the cropped image</param>
        /// <param name="height">Height of the cropped image</param>
        /// <param name="addToFilename">What string should be after the originalSourcePath. if original is img20161203.jpg and addToFileName is -thumbnail then the outcome will be img20161203-thumbnail.jpg</param>
        ///  /// <param name="removeFromOriginalSourceFilename">a string that should be removed from original source ex. originalSourcepath = "Image-fullImage.jpg"  removeFromOriginalSourceFilename = "-fullImage" the resulting path string will be "Image"+"addToFilename+".jpg"</param> 
        /// <returns>The path to the cropped image</returns>
      public Task<string> CropImage(string originalSourcePath, BoundingBox boundingBox, int width, int height, string addToFilename, string removeFromOriginalSourceFilename = null)
        {       
           UIImage image =  GetImageFromPath(originalSourcePath);

            CGRect clipRect = CalculateClipRect(image,boundingBox,width, height);
                  
            UIImage newImage = CropImage(image, clipRect, width, height);
           
            byte[] bytes = newImage.AsJPEG().ToArray();
            string newPath = SetupNewSourcePath(originalSourcePath, removeFromOriginalSourceFilename, addToFilename);
            File.WriteAllBytes(newPath, bytes);            
            return Task.FromResult(newPath);
        }

        /// <summary>
        /// Uses the Microsoft Vision API to generate a picture that crops automatically to whatever size you choose.
        /// </summary>
        /// <param name="originalSourcePath">The original SourcePath to a file on Device OR An url to a picture</param>
        /// <param name="width">Width of the cropped image</param>
        /// <param name="height">Height of the cropped image</param>
        /// <param name="addToFilename">What string should be after the originalSourcePath. if original is img20161203.jpg and addToFileName is -thumbnail then the outcome will be img20161203-thumbnail.jpg</param>
        /// <param name="removeFromOriginalSourceFilename">a string that should be removed from original source ex. originalSourcepath = "Image-fullImage.jpg"  removeFromOriginalSourceFilename = "-fullImage" the resulting path string will be "Image"+"addToFilename+".jpg"</param>
        /// <returns></returns>
        async public Task<string> SmartCrop(string originalSourcePath, int width, int height, string addToFilename, string removeFromOriginalSourceFilename = null)
        {
            string newPath = null;
            byte[] thumbNailByteArray = null;
            if (originalSourcePath.IsUrl()) {
                thumbNailByteArray = await VisionApi.GetThumbNail(originalSourcePath, width, height);
            }
            else {
                var downSampledPath = await FixMaxImageSize(originalSourcePath, 4000000);
                var originalBytes = File.ReadAllBytes(downSampledPath);
                thumbNailByteArray = await VisionApi.GetThumbNail(originalBytes, width, height);
            }
            newPath = SetupNewSourcePath(originalSourcePath, removeFromOriginalSourceFilename, addToFilename);

            File.WriteAllBytes(newPath, thumbNailByteArray);

            return newPath;
        }

        /// <summary>
        /// Uses the Microsoft Vision API to generate a picture that crops automatically to whatever size you choose.
        /// </summary>
        /// <param name="originalSourcePath">The original SourcePath to a file on Device OR An url to a picture</param>
        /// <param name="width">Width of the cropped image</param>
        /// <param name="height">Height of the cropped image</param>
        /// <returns>Byte array of new image</returns>
        async public Task<byte[]> SmartCrop(string originalSourcePath, int width, int height) {
            if (originalSourcePath.IsUrl()) {
                return await VisionApi.GetThumbNail(originalSourcePath, width, height);

            }
            else {
                var downSampledPath = await FixMaxImageSize(originalSourcePath, 4000000);
                var originalBytes = File.ReadAllBytes(downSampledPath);
                return await VisionApi.GetThumbNail(originalBytes, width, height);
            }
        }

        /// <summary>
        /// Uses the Microsoft Vision API to generate a picture that crops automatically to whatever size you choose.
        /// </summary>
        /// <param name="stream">Stream of an image that is used to send to Vision api</param>
        /// <param name="width">Width of the cropped image</param>
        /// <param name="height">Height of the cropped image</param>
        /// <returns>Byte array of new image</returns>
        async public Task<byte[]> SmartCrop(Stream stream, int width, int height) {
            if (stream.Length > 4000000) {
                throw new NotSupportedException("You are trying to SmartCrop a Stream that is bigger than 4Mb");
            }
            return await VisionApi.GetThumbNail(stream.ToByteArray(), width, height);
        }

        /// <summary>
        /// Uses the Microsoft Vision API to generate a picture that crops automatically to whatever size you choose.
        /// </summary>
        /// <param name="stream">Stream of an image that is used to send to Vision api</param>
        /// <param name="width">Width of the cropped image</param>
        /// <param name="height">Height of the cropped image</param>
        /// <param name="newFilePath">path to file that is going to be created</param>
        /// <returns>The path to the cropped image</returns>
        async public Task<string> SmartCrop(Stream stream, int width, int height, string newFilePath) {
            if (stream.Length > 4000000) {
                throw new NotSupportedException("You are trying to SmartCrop a Stream that is bigger than 4Mb");
            }
            var thumbNailByteArray = await VisionApi.GetThumbNail(stream.ToByteArray(), width, height);
            File.WriteAllBytes(newFilePath, thumbNailByteArray);
            return newFilePath;
        }

        /// <summary>
        /// Checks if size of file is bigger than given maxBytes. If so it downsamples the image to the size of maxBytes.
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="maxBytes">Max aloud bytes</param>
        /// <returns>Path to file under given maxBytes or the filePath if filesize was smaller than given maxBytes</returns>
         public Task<string> FixMaxImageSize(string filePath, long maxBytes) {
            var info = new FileInfo(filePath);

            var length = info.Length;
            Stream newImageStream;

            if (length > maxBytes) {
                newImageStream =  ResizeImageToFit(filePath, length, maxBytes);
                var newFilePath = SetupNewSourcePath(filePath, "", "-smallerCopy");
                File.WriteAllBytes(newFilePath, newImageStream.ToByteArray());
                return Task.FromResult(newFilePath);
            }
            return Task.FromResult(filePath);
        }


        /// <summary>
        /// Checks if size of file is bigger than given maxBytes. If so it downsamples the image to the size of maxBytes.
        /// </summary>
        /// <param name="stream">Stream </param>
        /// <param name="maxBytes">Max aloud bytes</param>
        /// <returns>Byte array of stream under given maxBytes, or the byte array of original stream if stream length was smaller than given maxBytes</returns>
        public Task<byte[]> FixMaxImageSize(Stream stream, long maxBytes) {
            Stream newImageStream;
            var length = stream.Length; 
            if (length > maxBytes) {
                newImageStream = ResizeImageToFit(null, length, maxBytes,stream);
                return Task.FromResult(newImageStream.ToByteArray());                          
            }
            return Task.FromResult(stream.ToByteArray());         
        }

        #region Private Methods
        private Stream ResizeImageToFit(string filePath, long length, long maxBytes, Stream stream = null) {
            var percentageToFit = (float)length/maxBytes;
            UIImage oldImage = null;
            if (stream == null) {
                oldImage = UIImage.FromFile(filePath);
            }
            else {
                oldImage = UIImage.LoadFromData(NSData.FromStream(stream));
            }
            var newSize = new SizeF((float)oldImage.Size.Width / percentageToFit, (float)oldImage.Size.Height / percentageToFit);
            var newImage = oldImage.Scale(newSize);
            return newImage.AsJPEG().AsStream();
        }

        private string SetupNewSourcePath(string originalSourcePath, string removeFromOriginalSourceFilename, string addToFilename) {
            var orSourcePath = originalSourcePath;
            if (!string.IsNullOrEmpty(removeFromOriginalSourceFilename)) {
                orSourcePath = orSourcePath.Replace(removeFromOriginalSourceFilename, "");
            }

            var extension = orSourcePath.Substring(orSourcePath.LastIndexOf("."));
            return orSourcePath.Replace(extension, addToFilename + extension);
        }

        private UIImage CropImage(UIImage image, CGRect clipRect, int width, int height) {
            CGSize sz = new CGSize(width, height);
            if (UIScreen.MainScreen.Scale == 2.0) {
                UIGraphics.BeginImageContextWithOptions(sz, true, 0);
            }
            else {
                UIGraphics.BeginImageContext(sz);
            }

            UIGraphics.RectClip(clipRect);
            image.DrawAsPatternInRect(clipRect);
            UIImage newImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return newImage;
        }

        private CGRect CalculateClipRect(UIImage image, BoundingBox boundingBox, int width, int height) {
            double ratio;
            double delta;
            CGPoint offset;

            ratio = width / image.Size.Width;
            delta = (ratio * image.Size.Width - ratio * image.Size.Height);
            offset = new CGPoint(boundingBox.Left, boundingBox.Top);

            return new CGRect(-offset.X, -offset.Y,
                                         (ratio * image.Size.Width) + delta,
                                         (ratio * image.Size.Height) + delta);
        }

        private UIImage GetImageFromPath(string originalSourcePath) {
            return ImageFromByteArray(File.ReadAllBytes(originalSourcePath));
        }

        private static UIImage ImageFromByteArray(byte[] data) {
            if (data == null) {
                return null;
            }

            UIImage image;
            try {
                image = new UIImage(Foundation.NSData.FromArray(data));
            }
            catch (Exception e) {
                Console.WriteLine("Image load failed: " + e.Message);
                return null;
            }
            return image;
        }

      


        #endregion
    }
}