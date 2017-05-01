using System;
using System.Threading.Tasks;
using Plugin.CropImage.Abstractions;
using System.IO;

namespace Plugin.CropImage
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    public class CropImageImplementation : ICropImage {
        public Task<string> CropImage(string originalSourcePath, BoundingBox boundingBox, int width, int height, string addToFilename, string removeFromOriginalSourceFilename = null) {
            throw new NotImplementedException();
        }

        public Task<string> CropImageFace(string originalSourcePath, int width, int height, string addToFilename, string removeFromOriginalSourceFilename, int extraAroundFaceRectangle = 30) {
            throw new NotImplementedException();
        }

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