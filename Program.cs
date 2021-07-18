using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace PhotoLab
{
    internal class Program
    {
        public static long MAX_PHOTO_SIZE_KB = 1024;
        public static string WorkingDirectory;
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                WorkingDirectory = args[0];
                if (args.Length == 2)
                {
                    MAX_PHOTO_SIZE_KB = Convert.ToInt64(args[1]);
                }
            }
            else
            {
                throw new ArgumentException(@"WorkingDirectory required. (ex 'C:\Users\pm\Desktop\')  Optional Argument: MAX_PHOTO_SIZE_KB (ex. 1024)");
            }
            
            var outputDirectory = WorkingDirectory + @"\Resized\";
            Directory.CreateDirectory(outputDirectory);
            
            var photos = Directory.EnumerateFiles(WorkingDirectory, "*.jpg");

            foreach (var photo in photos)
            {
                var photoName = Path.GetFileNameWithoutExtension(photo);
                var fi = new FileInfo(photo);
                Console.WriteLine($"Photo: {photo} {fi.Length / 1000}kB");

                if (fi.Length / 1000 > MAX_PHOTO_SIZE_KB)
                {
                    using (var image = Image.FromFile(photo)) 
                    {
                        using (var stream = DownscaleImage(image))
                        {
                            using (var file = File.Create(outputDirectory + photoName + "-resized.jpg"))
                            {
                                stream.CopyTo(file);
                                Console.WriteLine($"Resized: {file.Name} {file.Length / 1000}kB \n");
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Skipping Resize, copying file {photoName}\n");
                    File.Copy(WorkingDirectory + @"\"+ photoName + ".jpg", outputDirectory + photoName + "-resized.jpg");
                }
            }
            
        }
        
        
        private static MemoryStream DownscaleImage(Image photo)
        {
            MemoryStream resizedPhotoStream = new MemoryStream();

            long resizedSize = 0;
            var quality = 93;
            //long lastSizeDifference = 0;
            do
            {
                resizedPhotoStream.SetLength(0);

                EncoderParameters eps = new EncoderParameters(1);
                eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);
                ImageCodecInfo ici = GetEncoderInfo("image/jpeg");

                photo.Save(resizedPhotoStream, ici, eps);
                resizedSize = resizedPhotoStream.Length / 1000;

                //long sizeDifference = resizedSize - MAX_PHOTO_SIZE_KB;
                //Console.WriteLine(resizedSize + "(" + sizeDifference + " " + (lastSizeDifference - sizeDifference) + ")");
                //lastSizeDifference = sizeDifference;
                quality--;

            } while (resizedSize > MAX_PHOTO_SIZE_KB);

            resizedPhotoStream.Seek(0, SeekOrigin.Begin);

            return resizedPhotoStream;
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
    }
}