using Stannieman.HttpQueries;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
namespace Plugin.CropImage.Abstractions {
    /// <summary>
    /// Class that holds static property with key for VisionApi.
    /// </summary>
    public class VisionApi
    {
        /// <summary>
        /// Vision Subscription Key
        /// </summary>
        public static string Key{get;set; } 

        /// <summary>
        /// Which Endpoint to run against.
        /// </summary>
        public static Endpoint Server { get; set; } = Endpoint.WestEurope;

        /// <summary>
        /// Returns an image byte[] smartcropped ...
        /// </summary>
        /// <param name="originalSource"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="smartCrop">default true</param>
        /// <returns></returns>
      async  public static Task<byte []> GetThumbNail(byte[] originalSource ,int width,int height, bool smartCrop=true)
        {
            if (string.IsNullOrEmpty(Key)) {
                throw new Exception("You must set VisionApi.Key");
            }

            var client = new HttpClient();          
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Key );

            var uri = BuildUriString(width, height, smartCrop);

            HttpResponseMessage response;
            using (var content = new ByteArrayContent(originalSource))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(uri, content);
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }

        private static string BuildUriString(int width, int height, bool smartCrop) {
            var queryString = new Query();
            queryString.AddParameter("width", width);
            queryString.AddParameter("height", height);
            queryString.AddParameter("smartCropping", smartCrop.ToString());

            return "https://" + Server.ToString().ToLower() + ".api.cognitive.microsoft.com/vision/v1.0/generateThumbnail?" + queryString.QueryString;
        }
    }
}
