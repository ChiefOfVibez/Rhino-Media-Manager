namespace BoschMediaBrowser.Core.Services
{
    /// <summary>
    /// Thumbnail size options for multi-resolution caching
    /// </summary>
    public enum ThumbnailSize
    {
        /// <summary>
        /// Small thumbnail (256x256px) for grid view
        /// </summary>
        Small = 256,
        
        /// <summary>
        /// Medium thumbnail (512x512px) for detail pane
        /// </summary>
        Medium = 512,
        
        /// <summary>
        /// Large thumbnail (1024x1024px) for fullscreen preview
        /// </summary>
        Large = 1024
    }
}
