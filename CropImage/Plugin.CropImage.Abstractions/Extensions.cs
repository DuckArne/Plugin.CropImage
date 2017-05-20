using System;
using System.IO;

namespace Plugin.CropImage.Abstractions {
    public static class Extensions {
        public static  byte[] ToByteArray(this Stream input) {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream()) {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0) {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }

        }

        public static bool IsUrl(this string str) {
            Uri uriResult;
           return Uri.TryCreate(str, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == "http" || uriResult.Scheme == "https");
        }
    }
}
