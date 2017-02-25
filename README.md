# Plugin.CropImage
Crop those Images

Right now it only works for android and iOS.

In your App.Xaml.cs or somewhere before usage:
 
 VisionApi.Key = "Your Vision Api Key";
 
 
 then use an existing picture sourcepath and feed SmartCrop. I like to use MediaPlugin by James Montemagno
 
 var fileName = "Image-fullImage";
 
 var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions()
 {
 Name=DateNow()+fileName ,
 Directory= "ImageFiles",
 PhotoSize= PhotoSize.Large
 });
 
 
 var thumbnailPath = await Plugin.CropImage.CrossCropImage.Current.SmartCrop(file.Path, 60, 60, "-thumbnail-60x60", "-fullImage");


  
