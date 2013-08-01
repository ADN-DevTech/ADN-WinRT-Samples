using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;

namespace AdnCloudViewer
{
    public class AdnMeshData
    {
        /////////////////////////////////////////////////////////////////////////////
        // 
        //
        /////////////////////////////////////////////////////////////////////////////
        public AdnMeshData()
        {

        }

        /////////////////////////////////////////////////////////////////////////////
        // 
        //
        /////////////////////////////////////////////////////////////////////////////
        [JsonConstructor]
        public AdnMeshData(
            int facetCount,
            int vertexCount,
            float[] vertexCoords,
            int[] vertexIndices,
            float[] normals,
            int[] normalIndices,
            float[] center,
            int[] color,
            string id)
        {
            FacetCount = facetCount;
            VertexCount = vertexCount;

            VertexCoords = vertexCoords;
            VertexIndices = vertexIndices;

            Normals = normals;
            NormalIndices = normalIndices;

            Center = center;
            Color = color;
            
            Id = id;
        }

        public int FacetCount
        {
            get;
            protected set;
        }

        public int VertexCount
        {
            get;
            protected set;
        }

        public float[] VertexCoords
        {
            get;
            protected set;
        }

        public int[] VertexIndices
        {
            get;
            protected set;
        }

        public float[] Normals
        {
            get;
            protected set;
        }

        public int[] NormalIndices
        {
            get;
            protected set;
        }

        public float[] Center
        {
            get;
            protected set;
        }

        public int[] Color
        {
            get;
            protected set;
        }

        public string Id
        {
            get;
            protected set;
        }
    }
}

