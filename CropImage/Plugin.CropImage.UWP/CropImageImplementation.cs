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

        async public Task<string> CropImage(string originalSourcePath, BoundingBox boundingBox, int width, int height, string addToFilename, string removeFromOriginalSourceFilename = null) {

            var newPathToGeneratedFile = SetupNewSourcePath(originalSourcePath, removeFromOriginalSourceFilename, addToFilename);

            var originalFile = await StorageFile.GetFileFromPathAsync(originalSourcePath);

            var newFile = await originalFile.CopyAsync(await StorageFolder.GetFolderFromPathAsync(newPathToGeneratedFile));
            var softwareBitmap = await GetSoftwareBitmap(newFile);
            await CropBitmap(newFile, softwareBitmap, boundingBox);

            var croppedFile = await StorageFile.GetFileFromPathAsync(newPathToGeneratedFile);
            var croppedBitmap = await GetSoftwareBitmap(croppedFile);
            await ScaleBitmap(croppedFile, croppedBitmap, width, height);
            return newPathToGeneratedFile;
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

        [Obsolete("Is going to be deleted in release version")]
        public Task<string> CropImageFace(string originalSourcePath, int width, int height, string addToFilename, string removeFromOriginalSourceFilename, int extraAroundFaceRectangle = 30) {
            throw new NotImplementedException();
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
           
            if (string.IsNullOrEmpty(VisionApi.Key)) {
                throw new Exception("You must set VisionApi.Key");
            }

            var originalBytes = File.ReadAllBytes(originalSourcePath);

            var thumbNailByteArray = await VisionApi.GetThumbNail(originalBytes, VisionApi.Key, width, height);

            var orSourcePath = originalSourcePath;
            if (!string.IsNullOrEmpty(removeFromOriginalSourceFilename)) {
                orSourcePath = orSourcePath.Replace(removeFromOriginalSourceFilename, "");
            }

            var extension = orSourcePath.Substring(orSourcePath.LastIndexOf("."));

            newPath = orSourcePath.Replace(extension, addToFilename + extension);

            File.WriteAllBytes(newPath, thumbNailByteArray);

            return newPath;
        }
    }
}