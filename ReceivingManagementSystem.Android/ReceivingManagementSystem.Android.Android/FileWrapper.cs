using Android.Content;
using Android.OS;
using Java.IO;
using ReceivingManagementSystem.Android.Droid;
using ReceivingManagementSystem.Android.Interfaces;
using ReceivingManagementSystem.Android.Interfaces.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using static Android.Icu.Text.CaseMap;

[assembly: Dependency(typeof(ReceivingManagementSystem.Android.FileWrapper))]
namespace ReceivingManagementSystem.Android
{
    public class FileWrapper : IFileWrapper
    {
        public async Task<FileModel> ChooseFile(string fileType)
        {
            var options = new PickOptions
            {
                PickerTitle = "Please select a file",
                FileTypes = FilePickerFileType.Images,
            };

            var result = await FilePicker.PickAsync(options);
            if (result != null)
            {
                var stream = await result.OpenReadAsync();
                return new FileModel { FileName = result.FileName, FileStream = stream };
            }

            return null;
        }

        public Task<string> ChooseFolder()
        {
            // Xamarin.Essentials does not currently support folder picking
            throw new NotImplementedException();
        }

        public async Task ExportFile(MemoryStream data)
        {
            var fn = "ExportedFile.txt";
            var file = Path.Combine(FileSystem.CacheDirectory, fn);
            System.IO.File.WriteAllBytes(file, data.ToArray());

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "My Share Request",
                File = new ShareFile(file)
            });
        }


        public async Task<string> SaveFile(FileModel file)
        {
            string pathFolder = Path.Combine(MainActivity.AppContext.CacheDir.Path, "Media");
            if (!Directory.Exists(pathFolder))
            {
                Directory.CreateDirectory(pathFolder);
            }

            string path = Path.Combine(pathFolder, file.FileName);

            using (var fileStream = System.IO.File.Create(path))
            {
                file.FileStream.Seek(0, SeekOrigin.Begin);
                await file.FileStream.CopyToAsync(fileStream);
            }

            return path;
        }
    }
}
