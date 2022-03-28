using System;

namespace ImagesAndPasswords.Data
{
    public class Image
    {
        public int ID { get; set; } 
        public string FileName { get; set; }
        public string Password { get; set; }
        public int Views { get; set; }
    }
}
