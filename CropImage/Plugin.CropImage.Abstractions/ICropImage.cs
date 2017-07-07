using System.IO;
using System.Threading.Tasks;

namespace Plugin.CropImage.Abstractions {
    /// <summary>
    /// Interface for CropImage
    /// </summary>
    public interface ICropImage
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
        ///  <param name="removeFromOriginalSourceFilename">a string that should be removed from original source ex. originalSourcepath = "Image-fullImage.jpg"  removeFromOriginalSourceFilename = "-fullImage" the resulting path string will be "Image"+"addToFilename+".jpg"</param>
        /// <returns>The path to the cropped image</returns>
        Task<string> CropImage(string originalSourcePath, BoundingBox boundingBox,int width,int height,string addToFilename, string removeFromOriginalSourceFilename = null);


        /// <summary>
        /// Uses the Microsoft Vision API to generate a picture that crops automatically to whatever size you choose.
        /// </summary>
        /// <param name="originalSourcePath">The original SourcePath to a file on Device OR An url to a picture</param>
        /// <param name="width">Width of the cropped image</param>
        /// <param name="height">Height of the cropped image</param>
        /// <param name="addToFilename">What string should be after the originalSourcePath. if original is img20161203.jpg and addToFileName is -thumbnail then the outcome will be img20161203-thumbnail.jpg</param>
        /// <param name="removeFromOriginalSourceFilename">a string that should be removed from original source ex. originalSourcepath = "Image-fullImage.jpg"  removeFromOriginalSourceFilename = "-fullImage" the resulting path string will be "Image"+"addToFilename+".jpg"</param>
        /// <returns>The path to the cropped image</returns>
        Task<string> SmartCrop(string originalSourcePath, int width, int height, string addToFilename, string removeFromOriginalSourceFilename=null);

        /// <summary>
        /// Uses the Microsoft Vision API to generate a picture that crops automatically to whatever size you choose.
        /// </summary>
        /// <param name="originalSourcePath">The original SourcePath to a file on Device OR An url to a picture</param>
        /// <param name="width">Width of the cropped image</param>
        /// <param name="height">Height of the cropped image</param>
        /// <returns>Byte array of new image</returns>
        Task<byte[]> SmartCrop(string originalSourcePath, int width, int height);

        /// <summary>
        /// Uses the Microsoft Vision API to generate a picture that crops automatically to whatever size you choose.
        /// </summary>
        /// <param name="stream">Stream of an image that is used to send to Vision api</param>
        /// <param name="width">Width of the cropped image</param>
        /// <param name="height">Height of the cropped image</param>
        /// <returns>Byte array of new image</returns>
        Task<byte[]> SmartCrop(Stream stream, int width, int height);

        /// <summary>
        /// Uses the Microsoft Vision API to generate a picture that crops automatically to whatever size you choose.
        /// </summary>
        /// <param name="stream">Stream of an image that is used to send to Vision api</param>
        /// <param name="width">Width of the cropped image</param>
        /// <param name="height">Height of the cropped image</param>
        /// <param name="newFilePath">path to file that is going to be created</param>
        /// <returns>The path to the cropped image</returns>
        Task<string> SmartCrop(Stream stream, int width, int height, string newFilePath);

        /// <summary>
        /// Microsoft Vision Api has a max size of 4MB use this to maximize quality.
        /// </summary>
        /// <param name="filePath">path to Image to check</param>
        /// <param name="maxBytes">the max length in bytes</param>
        /// <returns>path to file</returns>
        Task<string> FixMaxImageSize(string filePath, long maxBytes);

        /// <summary>
        /// Microsoft Vision Api has a max size of 4MB use this to maximize quality.
        /// </summary>
        /// <param name="file">Image to check</param>
        /// <param name="maxBytes">the max length in bytes</param>
        /// <returns>path to file</returns>
        Task<byte[]> FixMaxImageSize(Stream file, long maxBytes);
    }
}
