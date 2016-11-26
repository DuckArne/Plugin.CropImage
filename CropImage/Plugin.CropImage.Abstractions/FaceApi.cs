using Microsoft.ProjectOxford.Face;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.CropImage.Abstractions
{
    /// <summary>
    /// Class that holds a static string with The ApiKey
    /// </summary>
    public class FaceApi
    {
        /// <summary>
        /// Set if you are going to use CropImageFace
        /// </summary>
        public static string Key { get; set;}

       static IFaceServiceClient faceService;
        /// <summary>
        /// Gets the Microsoft Cognitive Face Api methods.
        /// </summary>
        public static IFaceServiceClient FaceService
        {
            get
            {
                if (faceService == null)
                {
                    if (FaceApi.Key == null) throw new Exception("You must set FaceApi.Key");

                    faceService = new FaceServiceClient(FaceApi.Key);
                }
                return faceService;
            }
        }
    }
}
