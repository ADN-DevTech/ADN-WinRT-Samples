using System;

namespace AdnCloudViewer
{
    public class AdnDbModelData
    {
        public AdnDbModelData(
            string modelId,
            string modelName,
            int docType,
            int facetCount,
            int vertexCount)
        {
            ModelId = modelId.Trim(new char[] { '{', '}' });

            ModelName = modelName;

            DocType = docType;

            FacetCount = facetCount;

            VertexCount = vertexCount;
        }

        public int DocType
        {
            get;
            private set;
        }

        public string ModelId
        {
            get;
            private set;
        }

        public string ModelName
        {
            get;
            private set;
        }

        public int FacetCount
        {
            get;
            private set;
        }

        public int VertexCount
        {
            get;
            private set;
        }
    }
}