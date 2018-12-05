using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SpotlightToBackgrounds
{
    class Program
    {
        static void Main(string[] args)
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var assets = @"Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets";
            var folder = Path.Combine(appdata, assets);

            var backgroundFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                @"Backgrounds"
            );

            var alg = (HashAlgorithm)SHA256.Create();

            var backgroundFiles = Directory.EnumerateFiles(backgroundFolder, "*.png");
            var backgroundHashes = new Dictionary<string, string>();
            foreach (var file in backgroundFiles)
            {
                using (var stream = File.OpenRead(file))
                {
                    var hash = ArrayToHex(alg.ComputeHash(stream));
                    backgroundHashes.Add(hash, file);
                }
            }

            var files = Directory.EnumerateFiles(folder);
            foreach (var file in files)
            {
                using (var in_stream = File.OpenRead(file))
                {
                    var hash = ArrayToHex(alg.ComputeHash(in_stream));
                    if (backgroundHashes.ContainsKey(hash))
                        continue;

                    using (var img = Image.FromFile(file))
                    {
                        // Only HD images
                        if (img.Width < 1920 || img.Height < 1080)
                            continue;

                        // Only Horizontal Aspect
                        var aspect = (double)img.Width / img.Height;
                        if (aspect != (double)16 / 9)
                            continue;

                        var backgroundFilepath = Path.Combine(backgroundFolder, Path.GetFileName(file) + ".png");
                        File.Copy(file, backgroundFilepath);
                    }
                }
            }
        }

        static string ArrayToHex(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", "");
        }
    }
}
