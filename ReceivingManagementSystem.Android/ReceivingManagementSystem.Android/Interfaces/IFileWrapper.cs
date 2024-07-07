using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingManagementSystem.Android.Interfaces
{
    public interface IFileWrapper
    {
        Task<Models.FileModel> ChooseFile(string fileType);
        Task<string> ChooseFolder();
        Task ExportFile(MemoryStream data);
        Task<string> SaveFile(Models.FileModel file);
    }
}
