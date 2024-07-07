using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceivingManagementSystem.Wrapper.Models
{
    public class FileModel
    {
        public Stream FileStream { get; set; }
        public string FileName { get; set; }
    }
}
