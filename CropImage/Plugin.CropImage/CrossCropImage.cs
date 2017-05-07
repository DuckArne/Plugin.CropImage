using Plugin.CropImage.Abstractions;

using System;

namespace Plugin.CropImage
{
  /// <summary>
  /// Cross platform CropImage implemenations
  /// </summary>
  public class CrossCropImage
  {
    static Lazy<ICropImage> Implementation = new Lazy<ICropImage>(() => CreateCropImage(), System.Threading.LazyThreadSafetyMode.PublicationOnly);

    /// <summary>
    /// Current settings to use
    /// </summary>
    public static ICropImage Current
    {
      get
      {
        var ret = Implementation.Value;
        if (ret == null)
        {
          throw NotImplementedInReferenceAssembly();
        }
        return ret;
      }
    }

    static ICropImage CreateCropImage()
    {
 
         

#if (PORTABLE && !FORMS)

        return null;
#else
            return new CropImageImplementation();
#endif
        }

        internal static Exception NotImplementedInReferenceAssembly()
    {
      return new NotImplementedException("This functionality is not implemented in the portable version of this assembly.  You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
    }
  }
}
