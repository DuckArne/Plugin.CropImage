using Plugin.CropImage.Abstractions;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

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

            var newFile = await MakeCopyOfFile(originalSourcePath, addToFilename, removeFromOriginalSourceFilename);
            var softwareBitmap = await GetSoftwareBitmap(newFile);
            await CropBitmap(newFile, softwareBitmap, boundingBox);

            var croppedFile = await StorageFile.GetFileFromPathAsync(newFile.Path);
            var croppedBitmap = await GetSoftwareBitmap(croppedFile);
            await ScaleBitmap(croppedFile, croppedBitmap, width, height);
            return newFile.Path;
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
        public async Task<string> SmartCrop(string originalSourcePath, int width, int height, string addToFilename, string removeFromOriginalSourceFilename = null) {
            string newPath = null;
      
            var originalBytes = File.ReadAllBytes(originalSourcePath);

            var thumbNailByteArray = await VisionApi.GetThumbNail(originalBytes, width, height);

            var orSourcePath = originalSourcePath;
            if (!string.IsNullOrEmpty(removeFromOriginalSourceFilename)) {
                orSourcePath = orSourcePath.Replace(removeFromOriginalSourceFilename, "");
            }

            var extension = orSourcePath.Substring(orSourcePath.LastIndexOf("."));

            newPath = orSourcePath.Replace(extension, addToFilename + extension);

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
            var originalBytes = File.ReadAllBytes(originalSourcePath);
            return await VisionApi.GetThumbNail(originalBytes, width, height);
        }

        /// <summary>
        /// Uses the Microsoft Vision API to generate a picture that crops automatically to whatever size you choose.
        /// </summary>
        /// <param name="stream">Stream of an image that is used to send to Vision api</param>
        /// <param name="width">Width of the cropped image</param>
        /// <param name="height">Height of the cropped image</param>
        /// <returns>Byte array of new image</returns>
        async public Task<byte[]> SmartCrop(Stream stream, int width, int height) {
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
            var thumbNailByteArray = await VisionApi.GetThumbNail(stream.ToByteArray(), width, height);
            File.WriteAllBytes(newFilePath, thumbNailByteArray);
            return newFilePath;
        }

        #region Private 
        async private Task<StorageFile> MakeCopyOfFile(string originalSourcePath, string addToFilename, string removeFromOriginalSourceFilename) {
            var originalFile = await StorageFile.GetFileFromPathAsync(originalSourcePath);
            var newFileName = SetupNewSourcePath(originalFile.Name, removeFromOriginalSourceFilename, addToFilename);
            var newPathToGeneratedFile = SetupNewSourcePath(originalSourcePath, removeFromOriginalSourceFilename, addToFilename);

            return await originalFile.CopyAsync(await GetFolderFromStorageFile(originalFile), newFileName, NameCollisionOption.ReplaceExisting);
        }

        async private Task<IStorageFolder> GetFolderFromStorageFile(StorageFile originalFile) {
            return await StorageFolder.GetFolderFromPathAsync(originalFile.Path.Replace(originalFile.Name, ""));
        }

        async private Task ScaleBitmap(StorageFile croppedFile, SoftwareBitmap croppedBitmap, int width, int height) {

            using (IRandomAccessStream newStream = await croppedFile.OpenAsync(FileAccessMode.ReadWrite)) {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, newStream);
                encoder.SetSoftwareBitmap(croppedBitmap);
                encoder.BitmapTransform.ScaledWidth = (uint)width;
                encoder.BitmapTransform.ScaledHeight = (uint)height;
                try {
                    await encoder.FlushAsync();
                }
                catch (Exception err) {
                    throw new Exception("[CropImageImplementation] ScaleBitmap Could not scale Bitmap Message= " + err.Message);
                }
            }
        }

        async private Task CropBitmap(StorageFile newFile, SoftwareBitmap softwareBitmap, BoundingBox boundingBox) {

            using (IRandomAccessStream newStream = await newFile.OpenAsync(FileAccessMode.ReadWrite)) {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, newStream);
                encoder.SetSoftwareBitmap(softwareBitmap);
                encoder.BitmapTransform.Bounds = new BitmapBounds() { X = (uint)boundingBox.Left, Width = (uint)boundingBox.Width, Y = (uint)boundingBox.Top, Height = (uint)boundingBox.Height };

                try {
                    await encoder.FlushAsync();
                }
                catch (Exception err) {
                    throw new Exception("[CropImageImplementation] CropBitmap Could not Crop Image Message= " + err.Message);
                }
            }
        }

        async private Task<SoftwareBitmap> GetSoftwareBitmap(StorageFile newFile) {
            SoftwareBitmap softwareBitmap;
            using (IRandomAccessStream stream = await newFile.OpenAsync(FileAccessMode.Read)) {
                // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // Get the SoftwareBitmap representation of the file
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            }
            return softwareBitmap;
        }

        private string SetupNewSourcePath(string originalSourcePath, string removeFromOriginalSourceFilename, string addToFilename) {
            var orSourcePath = originalSourcePath;
            if (!string.IsNullOrEmpty(removeFromOriginalSourceFilename)) {
                orSourcePath = orSourcePath.Replace(removeFromOriginalSourceFilename, "");
            }

            var extension = orSourcePath.Substring(orSourcePath.LastIndexOf("."));
            return orSourcePath.Replace(extension, addToFilename + extension);
        }

      
        #endregion
    }
}