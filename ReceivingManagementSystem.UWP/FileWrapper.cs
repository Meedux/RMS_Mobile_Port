using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Xamarin.Essentials;
using ReceivingManagementSystem.Wrapper;
using ReceivingManagementSystem.Wrapper.Models;

[assembly: Xamarin.Forms.Dependency(typeof(ReceivingManagementSystem.UWP.FileWrapper))]
namespace ReceivingManagementSystem.UWP
{
    public class FileWrapper : IFileWrapper
    {
        public async Task<FileModel> ChooseFile(string fileType)
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>> {
                { DevicePlatform.UWP, fileType.Split(";") }
            });

            var options = new PickOptions
            {
                PickerTitle = $"Please select a {fileType} file",
                FileTypes = customFileType,
            };

            var folderPicker = new Windows.Storage.Pickers.FileOpenPicker();
            folderPicker.FileTypeFilter.Add(fileType);

            Windows.Storage.StorageFile file = await folderPicker.PickSingleFileAsync();

            //var result = await FilePicker.PickAsync(options);

            if (file != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                    FutureAccessList.AddOrReplace("PickedFileToken", file);

                FileModel fileModel = new FileModel();
                fileModel.FileStream = await file.OpenStreamForReadAsync();
                fileModel.FileName = file.Name;

                return fileModel;
            }

            return null;
        }

        public async Task<string> ChooseFolder()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                return folder.Path;
            }

            return string.Empty;
        }

        public async Task ExportFile(MemoryStream data)
        {
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("CSV", new List<string>() { ".csv" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "New Document";

            StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                await Windows.Storage.FileIO.WriteBytesAsync(file, data.ToArray());
            }
        }

        public async Task<string> SaveFile(FileModel file)
        {
            string pathFolder = Path.Combine(Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path, "Media");
            if (!Directory.Exists(pathFolder))
            {
                Directory.CreateDirectory(pathFolder);
            }

            string path = Path.Combine(pathFolder, file.FileName);

            using (var fileStream = File.Create(path))
            {
                file.FileStream.Seek(0, SeekOrigin.Begin);
                await file.FileStream.CopyToAsync(fileStream);
            }

            return path;
        }
    }
}
