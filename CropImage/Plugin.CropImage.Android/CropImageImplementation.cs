using Android.Graphics;
using Plugin.CropImage.Abstractions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Plugin.CropImage {
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    public class CropImageImplementation : ICropImage {
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
        async public Task<string> CropImage(string originalSourcePath, BoundingBox boundingBox, int width, int height, string addToFilename, string removeFromOriginalSourceFilename = null) {
            string newPath = null;
           var downSampledPath =await FixMaxImageSize(originalSourcePath, 4000000);
            Bitmap originalImage = await LoadOriginalBitmap(downSampledPath);

            var croppedBitmap = Bitmap.CreateBitmap(originalImage, boundingBox.Left, boundingBox.Top, boundingBox.Width, boundingBox.Height);

            Bitmap resizedImage = Bitmap.CreateScaledBitmap(croppedBitmap, width, height, false);

            byte[] compressed = CompressBitmap(resizedImage);

            croppedBitmap.Recycle();
            resizedImage.Recycle();
            originalImage.Recycle();

            newPath = SetupNewSourcePath(originalSourcePath, removeFromOriginalSourceFilename,addToFilename);
           
            File.WriteAllBytes(newPath, compressed);

            return newPath;

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
        async public Task<string> SmartCrop(string originalSourcePath, int width, int height, string addToFilename, string removeFromOriginalSourceFilename = null) {
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
            else          
            {
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
        async public Task<string> FixMaxImageSize(string filePath, long maxBytes) {
            FileInfo info = new FileInfo(filePath);

            var length = info.Length;
            if (length > maxBytes) {
                var originalBitmap = await DecodeSampledBitmapFromFile(filePath, 500, 500);              
                byte[] compressed = CompressBitmap(originalBitmap);
                var newPath = SetupNewSourcePath(filePath, "", "-smallerCopy");
                File.WriteAllBytes(newPath, compressed);
                originalBitmap.Recycle();
                return newPath;
            }
            return filePath;
        }


        /// <summary>
        /// Checks if size of file is bigger than given maxBytes. If so it downsamples the image to the size of maxBytes.
        /// </summary>
        /// <param name="stream">Stream </param>
        /// <param name="maxBytes">Max aloud bytes</param>
        /// <returns>Byte array of stream under given maxBytes, or the byte array of original stream if stream length was smaller than given maxBytes</returns>
        async public Task<byte[]> FixMaxImageSize(Stream stream, long maxBytes) {   
            var length = stream.Length;
            if (length > maxBytes) {
                var originalBitmap = await DecodeSampledBitmapFromBytes(stream.ToByteArray(), 500, 500);                
                originalBitmap.Recycle();             
                return CompressBitmap(originalBitmap);
            }
            return stream.ToByteArray();
        }

        #region Private Methods
        private Bitmap ResizeBitmap(Bitmap originalBitmap, long maxBytes) {
            var percentageToFit = (float)originalBitmap.ByteCount / maxBytes;
            return Bitmap.CreateScaledBitmap(originalBitmap, (int)(originalBitmap.Width / percentageToFit), (int)(originalBitmap.Height / percentageToFit), false);
        }

        private string SetupNewSourcePath(string originalSourcePath, string removeFromOriginalSourceFilename, string addToFilename) {
            var orSourcePath = originalSourcePath;
            if (!string.IsNullOrEmpty(removeFromOriginalSourceFilename)) {
                orSourcePath = orSourcePath.Replace(removeFromOriginalSourceFilename, "");
            }

            var extension = orSourcePath.Substring(orSourcePath.LastIndexOf("."));
            return orSourcePath.Replace(extension, addToFilename + extension);
        }

        async private Task<Bitmap> LoadOriginalBitmap(string originalSourcePath) {
           await  FixMaxImageSize(originalSourcePath, 4000000);
            var original = File.ReadAllBytes(originalSourcePath);
            return await BitmapFactory.DecodeByteArrayAsync(original, 0, original.Length);
        }

        private byte[] CompressBitmap(Bitmap bitmap) {
            byte[] compressed = null;
            using (MemoryStream ms = new MemoryStream()) {
                bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
                compressed = ms.ToArray();
            }
            return compressed;
        }

        private  int CalculateInSampleSize(
             BitmapFactory.Options options, int reqWidth, int reqHeight) {
            // Raw height and width of image
             int height = options.OutHeight;
           int width = options.OutWidth;
            int inSampleSize = 1;

            if (height > reqHeight || width > reqWidth) {

                 int halfHeight = height / 2;
                 int halfWidth = width / 2;

                // Calculate the largest inSampleSize value that is a power of 2 and keeps both
                // height and width larger than the requested height and width.
                while ((halfHeight / inSampleSize) >= reqHeight
                        && (halfWidth / inSampleSize) >= reqWidth) {
                    inSampleSize *= 2;
                }
            }

            return inSampleSize;
        }

       async private Task<Bitmap> DecodeSampledBitmapFromFile(string path,int reqWidth, int reqHeight) {

            // First decode with inJustDecodeBounds=true to check dimensions
             BitmapFactory.Options options = new BitmapFactory.Options();
            options.InJustDecodeBounds = true;
            await BitmapFactory.DecodeFileAsync(path,options);
           
            // Calculate inSampleSize
            options.InSampleSize = CalculateInSampleSize(options, reqWidth, reqHeight);

            // Decode bitmap with inSampleSize set
            options.InJustDecodeBounds = false;
            return await BitmapFactory.DecodeFileAsync(path,options);
        }

        async private Task<Bitmap> DecodeSampledBitmapFromBytes(byte[] bytes, int reqWidth, int reqHeight) {

            // First decode with inJustDecodeBounds=true to check dimensions
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InJustDecodeBounds = true;
            await BitmapFactory.DecodeByteArrayAsync(bytes,0,bytes.Length,options);

            // Calculate inSampleSize
            options.InSampleSize = CalculateInSampleSize(options, reqWidth, reqHeight);

            // Decode bitmap with inSampleSize set
            options.InJustDecodeBounds = false;
            return await BitmapFactory.DecodeByteArrayAsync(bytes,0,bytes.Length, options);
        }
        #endregion


    }
}