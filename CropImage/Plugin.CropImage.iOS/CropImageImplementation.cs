using CoreGraphics;
using Microsoft.ProjectOxford.Face.Contract;
using Plugin.CropImage.Abstractions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace Plugin.CropImage
{
    /// <summary>
    /// Implementation for CropImage
    /// </summary>
    public class CropImageImplementation : ICropImage
    {


        async public Task<string> CropImage(string originalSourcePath, BoundingBox boundingBoxToCrop, int newWidth, int newHeight, string addToFilename)
        {
            string newPath = null;


            byte[] originalImage = File.ReadAllBytes(originalSourcePath);
            var image = ImageResizer.ImageFromByteArray(originalImage);

            double ratio;
            double delta;
            CGPoint offset;

            //make a new square size, that is the resized imaged width
            CGSize sz = new CGSize(newWidth, newHeight);



            ratio = newWidth / image.Size.Width;
            delta = (ratio * image.Size.Width - ratio * image.Size.Height);
            offset = new CGPoint(boundingBoxToCrop.Left, boundingBoxToCrop.Top);



            //make the final clipping rect based on the calculated values
            CGRect clipRect = new CGRect(-offset.X, -offset.Y,
                                         (ratio * image.Size.Width) + delta,
                                         (ratio * image.Size.Height) + delta);

            //start a new context, with scale factor 0.0 so retina displays get
            //high quality image
            if (UIScreen.MainScreen.Scale == 2.0)
            {
                UIGraphics.BeginImageContextWithOptions(sz, true, 0);
            }
            else
            {
                UIGraphics.BeginImageContext(sz);
            }

            UIGraphics.RectClip(clipRect);
            image.DrawAsPatternInRect(clipRect);

            UIImage newImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            var bytes = newImage.AsJPEG().ToArray();

            var extension = originalSourcePath.Substring(originalSourcePath.LastIndexOf("."));

            newPath = originalSourcePath.Replace(extension, addToFilename + extension);
            File.WriteAllBytes(newPath, bytes);



            return newPath;

        }



        async public Task<string> CropImageFace(string originalSourcePath, int width, int height, string addToFilename, int extraAroundFaceRectangle = 30)
        {
            string newPath = null;


            await Task.Run(async () =>
           {
               bool hasFaceValue;
               FaceRectangle faceRectangle = await GetFaceRectangle(originalSourcePath);
               hasFaceValue = faceRectangle != null ? true : false;

               var original = File.ReadAllBytes(originalSourcePath);
               var originalImage = ImageResizer.ImageFromByteArray(original);

               var x = 0;
               var y = 0;
               var faceWidth = originalImage.Size.Width;
               var faceHeight = originalImage.Size.Height;

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
               var box = new BoundingBox { Left = x, Top = y, Width = (int)faceWidth, Height = (int)faceHeight };
              newPath= await CropImage(originalSourcePath, box, width, height, addToFilename);
           });
            return newPath;
        }

        private bool IsOkExtraAroundCropping(UIImage originalImage, int extraAroundFaceRectangle, FaceRectangle face)
        {
            var maxWidth = originalImage.Size.Width;
            var maxHeight = originalImage.Size.Height;
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

        public Task<string> CropImage(string originalSourcePath, BoundingBox boundingBox, int width, int height, string addToFilename, string removeFromOriginalSourceFilename = null)
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




        //async  public Task<string[]> CreateMediumAndLowResolutionPicturesOf(string sourceImagePath)
        //  {
        //      string mediumPath = null, smallPath = null;

        //      await Task.Run(() =>
        //      {
        //          var extension = sourceImagePath.Substring(sourceImagePath.LastIndexOf("."));

        //          byte[] originalImage = File.ReadAllBytes(sourceImagePath);

        //          byte[] medium = ImageResizer.ResizeImageToMedium(originalImage);
        //          mediumPath = sourceImagePath.Replace(extension, "-medium" + extension);
        //          File.WriteAllBytes(mediumPath, medium);

        //          byte[] small = ImageResizer.ResizeImageToSmall(originalImage);
        //          smallPath = sourceImagePath.Replace(extension, "-small" + extension);
        //          File.WriteAllBytes(smallPath, small);
        //      });
        //      return new string[] { mediumPath, smallPath };
        //  }
    }
}