using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SKPluginsDemo.Services
{
    public class FileService : IFileService
    {
        public void CreateFile(string fileName)
        {
            using (File.CreateText(fileName))
            {
            }
        }

        public void DeleteFile(string fileName)
        {
            File.Delete(fileName);
        }

        public string ReadFile(string filePath)
        {
            var content = File.ReadAllText(filePath);
            return content;
        }

        public void WriteFile(string fileName, string content)
        {
            if (!File.Exists(fileName))
            {
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    sw.WriteLine(content);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(fileName))
                {
                    sw.WriteLine(content);
                }
            }
        }
    }

}
