using Microsoft.SemanticKernel;
using PodcastGenerator.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodcastGenerator
{
    public class YamlExecutor
    {
        public async Task<string> ExecuteYaml(string yamlFileName, 
            Kernel kernel, KernelArguments arguments)
        {
            var scriptYaml =
                EmbeddedResource.Read(yamlFileName);

            var function =
                kernel.CreateFunctionFromPromptYaml(scriptYaml);

            var result =
                await kernel.InvokeAsync(function, arguments);

            return result.ToString();

        }
    }
}
