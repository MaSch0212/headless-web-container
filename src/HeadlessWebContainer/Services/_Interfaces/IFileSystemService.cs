using MaSch.Presentation.Wpf;
using System;
using System.Windows.Media.Imaging;

namespace HeadlessWebContainer.Services
{
    public interface IFileSystemService
    {
        BitmapImage LoadImage(string imageFilePath);
        BitmapImage LoadDefaultImage();
        void CopyDefaultImage(string profileDir);
        ITheme LoadTheme(string themeFilePath);
        T LoadJsonFromFile<T>(string filePath, Func<T> fallbackValueFactory);
        void SaveJsonToFile<T>(string filePath, T value);
        string GetFileHash(string filePath);
    }
}