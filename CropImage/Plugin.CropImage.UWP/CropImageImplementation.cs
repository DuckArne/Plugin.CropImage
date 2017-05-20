using Plugin.CropImage.Abstractions;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
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
            var downSampledPath = await FixMaxImageSize(originalSourcePath, 4000000);
            var newFile = await MakeCopyOfFile(downSampledPath, addToFilename, removeFromOriginalSourceFilename);
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
            byte[] thumbNailByteArray=null;
            if (originalSourcePath.IsUrl()) {
                thumbNailByteArray = await VisionApi.GetThumbNail(originalSourcePath, width, height);
            }else {
                var downSampledPath = await FixMaxImageSize(originalSourcePath, 4000000);
                var originalBytes = File.ReadAllBytes(downSampledPath);
                 thumbNailByteArray = await VisionApi.GetThumbNail(originalBytes, width, height);
            }
            newPath = SetupNewSourcePath(originalSourcePath,removeFromOriginalSourceFilename,addToFilename);

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
        async public Task<string> FixMaxImageSize(string filePath, long maxBytes) {
            var info = new FileInfo(filePath);
            var length = info.Length;
            if (length > maxBytes) {
                var percentageToFit =(float) length / maxBytes;

                var originalFile = await StorageFile.GetFileFromPathAsync(filePath);
                var folder = await  GetFolderFromStorageFile(originalFile);
                var newFile =await folder.CreateFileAsync( SetupNewSourcePath(originalFile.Name,"","-smallerCopy"));
                var softBitmap =await  GetSoftwareBitmap(originalFile);
                await ScaleBitmap(newFile, softBitmap, softBitmap.PixelWidth/percentageToFit,softBitmap.PixelHeight/percentageToFit);
                return newFile.Path;             
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
                var percentageToFit = (float)length / maxBytes;
                var scaledFile = await CreateTempFileScaled(stream,percentageToFit);

                IBuffer buffer = await FileIO.ReadBufferAsync(scaledFile);
                 return buffer.ToArray();                        
            }
            return stream.ToByteArray();
        }

        async private Task<StorageFile> CreateTempFileScaled(Stream stream, float percentageToFit) {
            var appFolder = ApplicationData.Current.TemporaryFolder;
            var tempFile = await appFolder.CreateFileAsync(Guid.NewGuid() + ".jpg");

            using (IRandomAccessStream str = await tempFile.OpenAsync(FileAccessMode.ReadWrite) ) {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
                var softBitmap = await decoder.GetSoftwareBitmapAsync();
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, str);
                encoder.SetSoftwareBitmap(softBitmap);
                encoder.BitmapTransform.ScaledWidth = (uint)(softBitmap.PixelWidth/percentageToFit);
                encoder.BitmapTransform.ScaledHeight = (uint)(softBitmap.PixelHeight/percentageToFit);
                try {
                    await encoder.FlushAsync();
                    return tempFile;
                }
                catch (Exception err) {
                    throw new Exception("[CropImageImplementation] ScaleBitmap Could not scale Bitmap Message= " + err.Message);
                }
                           
            }
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

        async private Task ScaleBitmap(StorageFile croppedFile, SoftwareBitmap croppedBitmap,float width,float height) {

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