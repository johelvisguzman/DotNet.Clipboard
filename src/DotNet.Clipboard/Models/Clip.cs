namespace DotNet.Clipboard.Models
{
    using System;

    public class Clip
    {
        public int Id { get; set; }
        public byte[] Data { get; set; }
        public string Format { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime LastUsedDate { get; set; }
    }
}
