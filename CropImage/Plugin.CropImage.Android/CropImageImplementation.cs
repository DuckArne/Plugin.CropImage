using Android.Graphics;
using Microsoft.ProjectOxford.Face.Contract;
using Plugin.CropImage.Abstractions;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Plugin.CropImage
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    public class CropImageImplementation : ICropImage
    {

        //public bool IsBusy {get;set;}

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
        async public Task<string> CropImage(string originalSourcePath, BoundingBox boundingBox, int width, int height, string addToFilename,string removeFromOriginalSourceFilename = null)
        {
           
            
            string newPath = null;
           

            await Task.Run(async () =>
            {
                
                var original = File.ReadAllBytes(originalSourcePath);
                var originalImage = await BitmapFactory.DecodeByteArrayAsync(original, 0, original.Length);

                var startX = boundingBox.Left;
                var startY = boundingBox.Top;

                var croppedBitmap = Bitmap.CreateBitmap(originalImage, startX, startY, boundingBox.Width, boundingBox.Height);

                Bitmap resizedImage = Bitmap.CreateScaledBitmap(croppedBitmap, width, height, false);

                byte[] resized = null;

                using (MemoryStream ms = new MemoryStream())
                {
                    resizedImage.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
                    resized = ms.ToArray();
                }

                croppedBitmap.Recycle();
                resizedImage.Recycle();
                originalImage.Recycle();

                var orSourcePath = originalSourcePath;
                if (!string.IsNullOrEmpty(removeFromOriginalSourceFilename))
                {
                  orSourcePath = orSourcePath.Replace(removeFromOriginalSourceFilename, "");
                }

                var extension = orSourcePath.Substring(orSourcePath.LastIndexOf("."));

                newPath = orSourcePath.Replace(extension, addToFilename + extension);
                File.WriteAllBytes(newPath, resized);
            });


          
            return newPath;

        }






        /// <summary>
        /// Crops an Image around faces, if there are any.
        /// Remember to check internet connectivity otherwise will throw exception. 
        /// </summary>
        /// <param name="originalSourcePath">The original SourcePath to a file on Device</param>
        /// <param name="width">Width of the cropped image</param>

        /// <param name="addToFilename">What string should be after the originalSourcePath. if original is img20161203.jpg and addToFileName is -thumbnail then the outcome will be img20161203-thumbnail.jpg</param>
        /// <param name="removeFromOriginalSourceFilename">a string that should be removed from original source ex. originalSourcepath = "Image-fullImage.jpg"  removeFromOriginalSourceFilename = "-fullImage" the resulting path string will be "Image"+"addToFilename+".jpg"</param> 
        /// <param name="extraAroundFaceRectangle">Face api returns a rectangle of the face this adds extra space around that</param>
        /// <returns>Path of the new Image File</returns>
        async public Task<string> CropImageFace(string originalSourcePath, int width, int height, string addToFilename,string removeFromOriginalSourceFilename, int extraAroundFaceRectangle = 30)
        {
            string newPath = null;


            await Task.Run(async () =>
            {
                bool hasFaceValue;
                FaceRectangle faceRectangle = await GetFaceRectangle(originalSourcePath);
                hasFaceValue = faceRectangle != null ? true : false;

                var original = File.ReadAllBytes(originalSourcePath);
                var originalImage = await BitmapFactory.DecodeByteArrayAsync(original, 0, original.Length);

                var x = 0;
                var y = 0;
                var faceWidth = originalImage.Width;
                var faceHeight = originalImage.Height;

                if (hasFaceValue)
                {
                    x = faceRectangle.Left;
                    y = faceRectangle.Top;
                    faceWidth = faceRectangle.Width;
                    faceHeight = faceRectangle.Height;

                    if (IsOkExtraAroundCropping(originalImage, extraAroundFaceRectangle, faceRectangle))
                    {
                        x -= extraAroundFaceRectangle;
                        y += extraAroundFaceRectangle;
                        faceWidth += extraAroundFaceRectangle;
                        faceHeight -= extraAroundFaceRectangle;
                    }
                }
                var box = new BoundingBox { Left = x, Top = y, Width = faceWidth, Height = faceHeight };
                newPath = await CropImage(originalSourcePath, box, width, height, addToFilename,removeFromOriginalSourceFilename);
            });



            return newPath;

        }

        private bool IsOkExtraAroundCropping(Bitmap originalImage, int extraAroundFaceRectangle, FaceRectangle face)
        {
            var maxWidth = originalImage.Width;
            var maxHeight = originalImage.Height;
            if (face.Left - extraAroundFaceRectangle < 0
                || (face.Left + face.Width + extraAroundFaceRectangle) > maxWidth
                || face.Top - extraAroundFaceRectangle > maxHeight
                || (face.Top - extraAroundFaceRectangle) < 0
                )
            {
                return false;
            }
            return true;
        }

        async private Task<FaceRectangle> GetFaceRectangle(string sourcePath)
        {
            using (Stream imageFileStream = File.OpenRead(sourcePath))
            {
                var faces = await FaceApi.FaceService.DetectAsync(imageFileStream);
                var faceRects = faces.Select(face => face.FaceRectangle);
                var faceArray = faceRects.ToArray();

                switch (faceArray.Length)
                {
                    case 0:
                        return null;
                    case 1:
                        return faceArray[0];
                    default:
                        return null;
                }
            }
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
           if (string.IsNullOrEmpty(VisionApi.Key))
            {
                throw new Exception("You must set VisionApi.Key");
            }

            var originalBytes = File.ReadAllBytes(originalSourcePath);

            var thumbNailByteArray = await VisionApi.GetThumbNail(originalBytes, VisionApi.Key, width, height);

            var orSourcePath = originalSourcePath;
            if (!string.IsNullOrEmpty(removeFromOriginalSourceFilename))
            {
                orSourcePath = orSourcePath.Replace(removeFromOriginalSourceFilename, "");
            }

            var extension = orSourcePath.Substring(orSourcePath.LastIndexOf("."));

            newPath = orSourcePath.Replace(extension, addToFilename + extension);

            File.WriteAllBytes(newPath, thumbNailByteArray);

            return newPath;
        }

        //private FaceRectangle CalculateFacesRect(FaceRectangle[] faceArray)
        //{

        //var faceRectangle = new FaceRectangle();
        //var left = 10000;
        //var top = 0;
        //var rightMax = 0;
        //var minBottom = 10000;

        //foreach (var face in faceArray)
        //{

        //    if (face.Left < left)
        //    {
        //        left = face.Left;
        //    }

        //    if (rightMax < (face.Left + face.Width))
        //    {
        //        rightMax = face.Left + face.Width;
        //    }


        //    if (face.Top > top)
        //    {
        //        top = face.Top;
        //    }

        //    if (face.Top - face.Height < minBottom)
        //    {
        //        minBottom = face.Top - face.Height;
        //    }

        //}
        //faceRectangle.Left = left;
        //faceRectangle.Top = top;


        //faceRectangle.Width = rightMax;
        //faceRectangle.Height = rightMax;

        //return faceRectangle;
        //}

        //async public Task<string[]> CreateMediumAndLowResolutionPicturesOf(string sourceImagePath)
        //{
        //    string mediumPath = null, smallPath = null;

        //    await Task.Run(() =>
        //    {
        //        var extension = sourceImagePath.Substring(sourceImagePath.LastIndexOf("."));

        //        byte[] originalImage = File.ReadAllBytes(sourceImagePath);

        //        byte[] medium = ImageResizer.ResizeImageToMedium(originalImage);
        //        mediumPath = sourceImagePath.Replace(extension, "-medium" + extension);
        //        File.WriteAllBytes(mediumPath, medium);

        //        byte[] small = ImageResizer.ResizeImageToSmall(originalImage);
        //        smallPath = sourceImagePath.Replace(extension, "-small" + extension);
        //        File.WriteAllBytes(smallPath, small);
        //    });
        //    return new string[] { mediumPath, smallPath };

        //}
    }
}