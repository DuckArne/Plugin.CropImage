using Plugin.CropImage.Abstractions;
using System;
using System.Threading.Tasks;

namespace Plugin.CropImage
{
    /// <summary>
    /// Implementation for CropImage
    /// </summary>
    public class CropImageImplementation : ICropImage
    {
        public Task<string[]> CreateMediumAndLowResolutionPicturesOf(string sourceImagePath)
        {
            throw new NotImplementedException();
        }

        public Task<string> CropImage(string originalSourcePath, BoundingBox boundingBoxToCrop, int newWidth, int newHeight, string addToFilename)
        {
            throw new NotImplementedException();
        }

        public Task<string> CropImage(string originalSourcePath, BoundingBox boundingBox, int width, int height, string addToFilename, string removeFromOriginalSourceFilename = null)
        {
            throw new NotImplementedException();
        }

        public Task<string> CropImageFace(string originalSourcePath)
        {
            throw new NotImplementedException();
        }

        public Task<string> CropImageFace(string originalSourcePath, int width, int height, string addToFilename, int extraAroundFaceRectangle = 30)
        {
            throw new NotImplementedException();
        }

        public Task<string> CropImageFace(string originalSourcePath, int width, int height, string addToFilename, string removeFromOriginalSourceFilename, int extraAroundFaceRectangle = 30)
        {
            throw new NotImplementedException();
        }

        public Task<string> SmartCrop(string originalSourcePath, int width, int height, string addToFilename, string removeFromOriginalSourceFilename = null)
        {
            throw new NotImplementedException();
        }
    }
}