using MaSch.Presentation.Wpf;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Media.Imaging;

namespace HeadlessWebContainer.Services
{
    public static class FileSystemServiceExtensions
    {
        public static bool TryLoadImage(this IFileSystemService service, [NotNullWhen(true)] string? imageFilePath, [NotNullWhen(true)] out BitmapImage? image)
            => TryLoad(imageFilePath, x => service.LoadImage(x), out image);

        public static bool TryLoadTheme(this IFileSystemService service, [NotNullWhen(true)] string? themeFilePath, [NotNullWhen(true)] out ITheme? theme)
            => TryLoad(themeFilePath, x => service.LoadTheme(x), out theme);

        private static bool TryLoad<T>([NotNullWhen(true)] string? filePath, Func<string, T> objFactory, [MaybeNullWhen(false)] out T obj)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                try
                {
                    obj = objFactory(filePath);
                    return true;
                }
                catch
                {
                }
            }

            obj = default;
            return false;
        }
    }
}
