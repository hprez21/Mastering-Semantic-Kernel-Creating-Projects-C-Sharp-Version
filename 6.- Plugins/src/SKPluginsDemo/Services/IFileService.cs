using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKPluginsDemo.Services
{
    public interface IFileService
    {
        void CreateFile(string fileName);
        void DeleteFile(string fileName);
        string ReadFile(string filePath);
        void WriteFile(string fileName, string content);
    }

}
