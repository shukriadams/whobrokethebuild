namespace Wbtb.Core.Common
{
    public class RevisionFile
    {
        /// <summary>
        /// Always set. Path of file in source control system
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Local path when changed. Set on source control systems that use local mapping
        /// </summary>
        public string LocalPath { get; set; }

        public RevisionFile() 
        {
            this.Path = string.Empty;
        }
    }
}
