/////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved 
// Written by Philippe Leefsma 2013 - ADN/Developer Technical Services
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted, 
// provided that the above copyright notice appears in all copies and 
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting 
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
/////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;

namespace AdnCloudViewer
{
    /////////////////////////////////////////////////////////////////////////////
    // 
    //
    /////////////////////////////////////////////////////////////////////////////
    public class AdnDataUtils
    {
        public static string Decompress(string compressedText)
        {
            byte[] gzBuffer = Convert.FromBase64String(compressedText);

            using (MemoryStream ms = new MemoryStream())
            {
                int msgLength = BitConverter.ToInt32(gzBuffer, 0);

                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                byte[] buffer = new byte[msgLength];

                ms.Position = 0;

                using (GZipStream zip =
                    new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            }
        }
    }

    /////////////////////////////////////////////////////////////////////////////
    // 
    //
    /////////////////////////////////////////////////////////////////////////////
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

    /////////////////////////////////////////////////////////////////////////////
    // 
    //
    /////////////////////////////////////////////////////////////////////////////
    public class AdnMeshData
    {
        public AdnMeshData()
        {

        }

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

    /////////////////////////////////////////////////////////////////////////////
    // 
    //
    /////////////////////////////////////////////////////////////////////////////
    public class AdnMetaDataElement
    {
        [JsonConstructor]
        public AdnMetaDataElement(
            string name,
            string category,
            object value)
        {
            Name = name;
            Category = category;
            Value = value;
        }

        public string Name
        {
            get;
            set;
        }

        public string Category
        {
            get;
            set;
        }

        public object Value
        {
            get;
            set;
        }
    }

    /////////////////////////////////////////////////////////////////////////////
    // 
    //
    /////////////////////////////////////////////////////////////////////////////
    public class AdnMetaData
    {
        [JsonConstructor]
        public AdnMetaData(
            string id,
            AdnMetaDataElement[] elements)
        {
            Id = id;

            Elements = elements;
        }

        public string Id
        {
            get;
            protected set;
        }

        public AdnMetaDataElement[] Elements
        {
            get;
            protected set;
        }
    }
}

