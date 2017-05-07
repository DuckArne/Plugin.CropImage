namespace Plugin.CropImage.Abstractions {
    /// <summary>
    /// Which Endpoint to run Vision Api against
    /// </summary>   
    public enum Endpoint {
        /// <summary>
        /// westus.api.cognitive.microsoft.com
        /// </summary>
        WestUS,
        /// <summary>
        /// eastus2.api.cognitive.microsoft.com
        /// </summary>
        EastUS2,
        /// <summary>
        /// westcentralus.api.cognitive.microsoft.com
        /// </summary>
        WestCentralUs,
        /// <summary>
        /// westeurope.api.cognitive.microsoft.com
        /// </summary>
        WestEurope,
        /// <summary>
        /// southeastasia.api.cognitive.microsoft.com
        /// </summary>
        SouthEastAsia
    }
}