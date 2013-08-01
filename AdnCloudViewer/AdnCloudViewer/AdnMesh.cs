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
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdnCloudViewer
{
    public class DxMeshEntity: IDisposable
    {
        ////////////////////////////////////////////////////////////////////////
        // 
        //
        ////////////////////////////////////////////////////////////////////////
        private Buffer<VertexPositionNormalTexture> _vertexBuffer;
        private List<AdnPoint> _vertices;

        private VertexInputLayout _inputLayout;
        private AdnBoundingBox _boundingBox;
        private List<float[]> _colors;
        private Game _renderer;

        ////////////////////////////////////////////////////////////////////////
        // 
        //
        ////////////////////////////////////////////////////////////////////////
        public string Id
        {
            get;
            private set;
        }

        ////////////////////////////////////////////////////////////////////////
        // 
        //
        ////////////////////////////////////////////////////////////////////////
        public bool IsPreselected
        {
            get;
            set;
        }

        ////////////////////////////////////////////////////////////////////////
        // 
        //
        ////////////////////////////////////////////////////////////////////////
        public bool IsSelected
        {
            get;
            set;
        }

        ////////////////////////////////////////////////////////////////////////
        // 
        //
        ////////////////////////////////////////////////////////////////////////
        public DxMeshEntity(Game renderer, AdnMeshData meshData)
        {
            Id = meshData.Id;
            IsSelected = false;
            IsPreselected = false;

            _renderer = renderer;

            _colors = new List<float[]>();

            foreach (var color in meshData.Color)
            {
                _colors.Add(ConvertClr(color));
            }

            _vertices = new List<AdnPoint>();

            List<VertexPositionNormalTexture> vertList =
                new List<VertexPositionNormalTexture>();

            for (int i = 0; i < meshData.VertexIndices.Length; ++i)
            {
                int vIdx = meshData.VertexIndices[i];
                int nIdx = meshData.NormalIndices[i];

                _vertices.Add(
                    new AdnPoint(
                        meshData.VertexCoords[3 * vIdx],
                        meshData.VertexCoords[3 * vIdx + 1],
                        meshData.VertexCoords[3 * vIdx + 2]));

                vertList.Add(
                    new VertexPositionNormalTexture(
                        new Vector3(
                            meshData.VertexCoords[3 * vIdx],
                            meshData.VertexCoords[3 * vIdx + 1],
                            meshData.VertexCoords[3 * vIdx + 2]),
                        new Vector3(
                            meshData.Normals[3 * nIdx],
                            meshData.Normals[3 * nIdx + 1],
                            meshData.Normals[3 * nIdx + 2]),
                        new Vector2()));
            }

            _boundingBox = new AdnBoundingBox(_vertices);

            _vertexBuffer = SharpDX.Toolkit.Graphics.Buffer.Vertex.New(
                _renderer.GraphicsDevice,
                vertList.ToArray());

            _inputLayout = VertexInputLayout.FromBuffer(0, _vertexBuffer);
        }

        ////////////////////////////////////////////////////////////////////////
        // 
        //
        ////////////////////////////////////////////////////////////////////////
        public void Draw(BasicEffect effect)
        {
            _renderer.GraphicsDevice.SetVertexBuffer(_vertexBuffer);
            _renderer.GraphicsDevice.SetVertexInputLayout(_inputLayout);

            if (IsSelected)
            {
                 effect.DiffuseColor = new Vector4(
                     0.1f, 1.0f, 1.0f, 0.5f); 
            }
            else if (IsPreselected)
            {
                effect.DiffuseColor = new Vector4(
                    0.4f, 1.0f, 1.0f, 0.5f);
            }
            else if (_colors.Count == 4)
            {
                effect.AmbientLightColor = new Vector3(
                    _colors[0][0],
                    _colors[0][1],
                    _colors[0][2]);

                effect.DiffuseColor = new Vector4(
                    _colors[1][0],
                    _colors[1][1],
                    _colors[1][2],
                    _colors[1][3]);

                effect.EmissiveColor = new Vector3(
                    _colors[2][0],
                    _colors[2][1],
                    _colors[2][2]);

                effect.SpecularColor = new Vector3(
                    _colors[3][0],
                    _colors[3][1],
                    _colors[3][2]);

                //effect.SpecularPower = _sourceMesh.Material.Shininess;
                //effect.Alpha = _sourceMesh.Material.Transparency;
            }
            else //supports previous version of the data model
            {
                effect.DiffuseColor = new Vector4(
                    1.0f, 1.0f, 1.0f, 1.0f);

                effect.SpecularColor = new Vector3(
                   _colors[0][0],
                   _colors[0][1],
                   _colors[0][2]);
            }

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                _renderer.GraphicsDevice.Draw(
                    PrimitiveType.TriangleList,
                    _vertexBuffer.ElementCount);
            }

            _renderer.GraphicsDevice.SetIndexBuffer(
                null,
                false);

            _renderer.GraphicsDevice.SetVertexBuffer(
                (Buffer<VertexPositionNormalTexture>)null);
        }

        ////////////////////////////////////////////////////////////////////////
        // 
        //
        ////////////////////////////////////////////////////////////////////////
        float[] ConvertClr(int clr)
        {
            return new float[]
                {
                    (byte)(clr >> 24) / 255.0f,
                    (byte)(clr >> 16) / 255.0f,
                    (byte)(clr >> 8)  / 255.0f,
                    (byte)clr
                };
        }

        public bool Intersects(
            AdnRay ray,
            ref AdnPoint point)
        {
            point = null;

            // Perform intersection test on bounding box
            // first for performances
            if (!_boundingBox.Intersects(ray))
                return false;

            double mindist = double.PositiveInfinity;

            for (int idx = 0; idx < _vertexBuffer.ElementCount; idx += 3)
            {
                AdnPoint p = null;

                if (ray.Intersect(
                    _vertices[idx],
                    _vertices[idx + 1],
                    _vertices[idx + 2],
                    out p))
                {
                    double dist = ray.Origin.SquaredDistanceTo(p);

                    if (dist < mindist)
                    {
                        mindist = dist;
                        point = p;
                    }
                }
            }

            return (point != null);
        }

        public void Dispose()
        {
            if (_vertexBuffer != null)
                _vertexBuffer.Dispose();
        }
    }

    ////////////////////////////////////////////////////////////////////////
    // 
    //
    ////////////////////////////////////////////////////////////////////////
    public class DxMesh: IDisposable
    {
        private Vector3 _center;

        private Dictionary<string, DxMeshEntity> _meshEntities;

        public DxMesh(
            Game renderer, 
            List<AdnMeshData> meshDataList)
        {
            _center = new Vector3(0, 0, 0);

            _meshEntities = new Dictionary<string, DxMeshEntity>();

            foreach (AdnMeshData meshData in meshDataList)
            {
                _center += new Vector3(
                    meshData.Center[0],
                    meshData.Center[1],
                    meshData.Center[2]);

                _meshEntities.Add(
                    meshData.Id,
                    new DxMeshEntity(
                        renderer, 
                        meshData));
            }

            _center.X = -_center.X / meshDataList.Count;
            _center.Y = -_center.Y / meshDataList.Count;
            _center.Z = -_center.Z / meshDataList.Count;
        }

        public void Draw(BasicEffect effect)
        {
            Matrix world = effect.World;

            effect.World = Matrix.Translation(_center) * world;

            foreach (DxMeshEntity meshEntity in _meshEntities.Values)
            {
                meshEntity.Draw(effect);
            }

            effect.World = world;
        }

        public DxMeshEntity Intersect(
            Vector3 rayPos,
            Vector3 rayDir)
        {
            double mindist = double.MaxValue;

            AdnPoint point = null;

            DxMeshEntity closestEntity = null;

            AdnRay ray = new AdnRay(
                new AdnPoint(
                    rayPos.X - _center.X,
                    rayPos.Y - _center.Y,
                    rayPos.Z - _center.Z),
                new AdnVector(
                    rayDir.X,
                    rayDir.Y,
                    rayDir.Z));

            foreach (var entity in _meshEntities.Values)
            {
                if (entity.Intersects(ray, ref point))
                {
                    double dist = ray.Origin.SquaredDistanceTo(point);

                    if (dist < mindist)
                    {
                        mindist = dist;
                        closestEntity = entity;
                    }
                }
            }

            return closestEntity;
        }

        ////////////////////////////////////////////////////////////////////////
        // 
        //
        ////////////////////////////////////////////////////////////////////////
        public void Preselect(DxMeshEntity entity)
        {
            foreach (var ent in _meshEntities.Values)
            {
                ent.IsPreselected = false;
            }

            entity.IsPreselected = true;
        }

        ////////////////////////////////////////////////////////////////////////
        // 
        //
        ////////////////////////////////////////////////////////////////////////
        public void UnPreselectAll()
        {
            foreach (var entity in _meshEntities.Values)
            {
                entity.IsPreselected = false;
            }
        }

        ////////////////////////////////////////////////////////////////////////
        // 
        //
        ////////////////////////////////////////////////////////////////////////
        public void Select(DxMeshEntity entity)
        {
            foreach (var ent in _meshEntities.Values)
            {
                ent.IsSelected = false;
            }

            entity.IsSelected = true;
        }

        ////////////////////////////////////////////////////////////////////////
        // 
        //
        ////////////////////////////////////////////////////////////////////////
        public void UnSelectAll()
        {
            foreach (var ent in _meshEntities.Values)
            {
                ent.IsSelected = false;
            }
        }

        ////////////////////////////////////////////////////////////////////////
        // 
        //
        ////////////////////////////////////////////////////////////////////////
        public void Dispose()
        {
            if (_meshEntities != null)
                foreach (var ent in _meshEntities.Values)
                    ent.Dispose();
        }
    }
}
