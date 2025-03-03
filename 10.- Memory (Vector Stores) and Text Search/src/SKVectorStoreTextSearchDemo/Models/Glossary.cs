using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable SKEXP0001

namespace SKVectorStoreTextSearchDemo.Models
{
    public class Glossary
    {
        [VectorStoreRecordKey] 
        [TextSearchResultName]
        public string Key { get; set; }
        [VectorStoreRecordData(IsFilterable = true)]
        public string Category { get; set; }
        [VectorStoreRecordData]
        public string Term { get; set; }
        [VectorStoreRecordData]
        [TextSearchResultValue]
        public string Definition { get; set; }
        [VectorStoreRecordVector(1536)]
        public ReadOnlyMemory<float> DefinitionEmbedding { get; set; }
        [VectorStoreRecordData]
        public string Definition2 { get; set; }
        [VectorStoreRecordVector(1536)]
        public ReadOnlyMemory<float> Definition2Embedding { get; set; }

    }
}
