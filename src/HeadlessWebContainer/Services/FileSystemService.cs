using MaSch.Core.Extensions;
using MaSch.Presentation.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.IO.Packaging;
using System.Net;
using System.Security.Cryptography;
using System.Windows.Media.Imaging;

namespace HeadlessWebContainer.Services
{
    public class FileSystemService : IFileSystemService
    {
        private static readonly Uri DefaultIconPath = new("pack://application:,,,/HeadlessWebContainer;component/Resources/Icon.ico");
        private static readonly IWebRequestCreate PackRequestFactory = new PackWebRequestFactory();
        private readonly JsonSerializerSettings _jsonSettings;

        public FileSystemService()
        {
            _jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };
            _jsonSettings.Converters.Add(new StringEnumConverter());
        }

        public BitmapImage LoadImage(string imageFilePath)
        {
            var img = new BitmapImage();

            using (var fs = new FileStream(imageFilePath, FileMode.Open))
            {
                img.BeginInit();
                img.StreamSource = fs;
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
            }

            img.Freeze();

            return img;
        }

        public BitmapImage LoadDefaultImage()
        {
            var img = new BitmapImage();

            var request = PackRequestFactory.Create(DefaultIconPath);
            using (var response = request.GetResponse())
            {
                img.BeginInit();
                img.StreamSource = response.GetResponseStream();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
            }

            img.Freeze();

            return img;
        }

        public void CopyDefaultImage(string profileDir)
        {
            var request = PackRequestFactory.Create(DefaultIconPath);
            using var response = request.GetResponse();
            using var fileStream = new FileStream(Path.Combine(profileDir, "icon.ico"), FileMode.Create);
            response.GetResponseStream().CopyTo(fileStream);
        }

        public ITheme LoadTheme(string themeFilePath)
            => Theme.FromFile(themeFilePath);

        public T LoadJsonFromFile<T>(string filePath, Func<T> fallbackValueFactory)
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                try
                {
                    return JsonConvert.DeserializeObject<T>(json, _jsonSettings) ?? fallbackValueFactory();
                }
                catch
                {
                }
            }

            return fallbackValueFactory();
        }

        public void SaveJsonToFile<T>(string filePath, T value)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (dir != null)
                Directory.CreateDirectory(dir);
            File.WriteAllText(filePath, JsonConvert.SerializeObject(value, _jsonSettings));
        }

        public string GetFileHash(string filePath)
        {
            using var bs = new BufferedStream(File.OpenRead(filePath), 1200000);
            var md5 = new MD5CryptoServiceProvider();
            var checksum = md5.ComputeHash(bs);
            return checksum.ToHexString();
        }
    }
}
