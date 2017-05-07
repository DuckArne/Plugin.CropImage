namespace Plugin.CropImage.Abstractions {
    /// <summary>
    /// Class that holds values for the  box to crop.
    /// </summary>
    public class BoundingBox
    {
        /// <summary>
        /// Box startvalue from Left
        /// </summary>
       public int Left { get; set;}
        /// <summary>
        /// Box startValue from Top
        /// </summary>
        public int Top { get; set; }
        /// <summary>
        /// Box Width
        /// </summary>
        public int Width { get;set;}
        /// <summary>
        /// Box Height
        /// </summary>
        public int Height { get;set;}

    }
}
