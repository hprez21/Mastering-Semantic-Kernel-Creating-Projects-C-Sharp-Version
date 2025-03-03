using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SKVectorStoreTextSearchDemo
{
    internal class FunctionInvocationFilter : IFunctionInvocationFilter
    {
        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            if(context.Function.PluginName == "SearchPlugin")
            {
                Debug.WriteLine($"{context.Function.Name}:{JsonSerializer.Serialize(context.Arguments)}\n");
            }
            await next(context);
        }
    }
}
