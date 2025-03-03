using Microsoft.SemanticKernel;
using SKPluginsDemo.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKPluginsDemo.Plugins
{
    public class FilePlugin
    {
        private readonly IFileService _fileService;
        public FilePlugin(IFileService fileService)
        {
            _fileService = fileService;
        }

        [KernelFunction("create_file")]
        [Description("Creates a file requesting a name for the file")]
        public void CreateFile(string fileName)
        {
            _fileService.CreateFile(fileName);
        }
        [KernelFunction("delete_file")]
        public void DeleteFile(string fileName)
        {
            _fileService.DeleteFile(fileName);
        }
        [KernelFunction("read_file")]
        public string ReadFile(string filePath)
        {
            return _fileService.ReadFile(filePath);
        }

        [KernelFunction("write_file")]
        public void WriteFile(string fileName, string content)
        {
            _fileService.WriteFile(fileName, content);
        }
    }

}
