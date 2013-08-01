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
using SharpDX.Toolkit.Graphics;
using System.Net;
using System.IO;
using System.IO.Compression;
using SharpDX.Text;
using SharpDX.Toolkit;
using SharpDX;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AdnCloudViewer
{    
    public class AdnRenderer : Game
    {
        private GraphicsDeviceManager _graphicsDeviceManager;
        private List<DxMesh> _models;
        private BasicEffect _effect;

        private float _yOffsetRad;
        private float _xOffsetRad;
        private Vector3 _eye;

        public AdnRenderer()
        {
            // Mandatory: Creates a graphics manager.
            _graphicsDeviceManager = new GraphicsDeviceManager(this);

            // Setup the relative directory to the executable directory
            // for loading contents with the ContentManager
            Content.RootDirectory = "Content";

            _models = new List<DxMesh>();
        }

        protected override void Initialize()
        {
            Window.Title =
                "ADN Cloud Viewer - WinRT, by Philippe Leefsma";

            base.Initialize();
        }

        public void LoadModel(List<AdnMeshData> meshDataList)
        {
            var model = new DxMesh(this, meshDataList);

            _models.Add(model);
        }

        public void CloseModel()
        {
            foreach (var model in _models)
            {
                model.Dispose();
            }

            _models.Clear();
        }      

        protected override void LoadContent()
        {
            _eye = new Vector3(0, 0, -30);

            _yOffsetRad = 0.0f;
            _xOffsetRad = 0.0f;

            _effect = 
                ToDisposeContent(
                    new BasicEffect(GraphicsDevice));

            _effect.EnableDefaultLighting();

            _effect.AmbientLightColor = 
                new Vector3(0.5f, 0.5f, 0.5f);

            _effect.PreferPerPixelLighting = true;
            
            _effect.View = Matrix.LookAtLH(
                _eye, 
                new Vector3(0, 0, 0), 
                Vector3.UnitY);

            _effect.Projection = Matrix.PerspectiveFovLH(
                (float)Math.PI / 4.0f, 
                (float)GraphicsDevice.BackBuffer.Width / 
                GraphicsDevice.BackBuffer.Height, 
                0.01f, 
                100.0f);

            _effect.TextureEnabled = false;
            _effect.Texture = null;

            _effect.World = Matrix.Identity;

            base.LoadContent();
        }

        public void Rotate(float xOffset, float yOffset)
        {
            _xOffsetRad += xOffset * (float)Math.PI / 180.0f;
            _yOffsetRad += yOffset * (float)Math.PI / 180.0f;
            
            _effect.World = 
                Matrix.RotationY(_xOffsetRad) * 
                Matrix.RotationX(_yOffsetRad);
        }

        public void AddZoom(float dZ)
        {
            _eye.Z += dZ;

            if (_eye.Z < -100)
                _eye.Z = -100;

            if (_eye.Z > -5)
                _eye.Z = -5;

            _effect.View = Matrix.LookAtLH(
               _eye,
               new Vector3(0, 0, 0),
               Vector3.UnitY);
        }

        public void SetZoom(float zoom)
        {
            _eye.Z = zoom;

            _effect.View = Matrix.LookAtLH(
               _eye,
               new Vector3(0, 0, 0),
               Vector3.UnitY);
        }

        // A debug test function to visualize ray tracing
        void DrawRay(Vector3 origin, Vector3 dir)
        {
            List<AdnMeshData> dataList = 
                new List<AdnMeshData>();

            float of = 0.0f;
            float len = -100.0f;

            AdnMeshData data = new AdnMeshData(
                2,
                3,
                new float[]
                {
                    origin.X + of * dir.X,
                    origin.Y + of * dir.Y,
                    origin.Z + of * dir.Z + 1.0f,

                    origin.X + of * dir.X,
                    origin.Y + of * dir.Y,
                    origin.Z + of * dir.Z - 1.0f,

                    origin.X + len * dir.X,
                    origin.Y + len * dir.Y,
                    origin.Z + len * dir.Z,
                },
                new int[]
                {
                    0, 1, 2,
                    2, 1, 0
                },
                new float[]
                {
                    0,0,1,
                    0,0,1,
                    0,0,1
                },
                new int[]
                {
                    0, 1, 2,
                    2, 1, 0
                },
                new float[]
                {
                    0,0,0
                },
                new int[]
                {
                    8235263,
                    8235263,
                    8235263,
                    8235263
                },
                string.Empty);

            dataList.Add(data);

            //_ray = new DxMesh(this, dataList);
        }

        public DxMeshEntity EntityUnderCursor(
            float mouseX,
            float mouseY)
        {
            ViewportF viewport = GraphicsDevice.Viewport;

            Vector3 mouseNearVector = new Vector3(
                mouseX, 
                mouseY, 
                0.1f);

            Vector3 pointNear = viewport.Unproject(
                mouseNearVector, 
                _effect.Projection, 
                _effect.View, 
                _effect.World);

            Vector3 mouseFarVector = new Vector3(
                mouseX, 
                mouseY, 
                100.0f);

            Vector3 pointFar = viewport.Unproject(
                mouseFarVector, 
                _effect.Projection, 
                _effect.View, 
                _effect.World);

            Vector3 rayDir = 
                Vector3.Normalize(
                    pointFar - pointNear);

            foreach (var model in _models)
            {
                DxMeshEntity entity =
                    model.Intersect(
                        pointNear, 
                        rayDir);

                return entity;
            }

            return null;
        }

        public void CheckPreSelection(
           float mouseX,
           float mouseY)
        {          
            DxMeshEntity entity = EntityUnderCursor(
                mouseX, 
                mouseY);

            foreach (var model in _models)
            {
                if (entity != null)
                {
                    if (entity.IsPreselected)
                        return;

                    model.Preselect(entity);
                }
                else
                {
                    model.UnPreselectAll();
                }
            }
        }

        public void CheckSelection(
           float mouseX,
           float mouseY)
        {
            DxMeshEntity entity = EntityUnderCursor(
                mouseX, 
                mouseY);

            foreach (var model in _models)
            {
                if (entity != null)
                {
                    if (entity.IsSelected)
                        return;

                    model.Select(entity);
                }
                else
                {
                    model.UnSelectAll();
                }
            }
        }

        public void CheckDisplaySelection(
           float mouseX,
           float mouseY)
        {
            DxMeshEntity entity = EntityUnderCursor(
                mouseX, 
                mouseY);

            if (entity != null)
            {
                OnMetaDataDisplay(entity.Id);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            OnDraw();

            // Clears the screen with color
            GraphicsDevice.Clear(Color.CornflowerBlue);

            foreach(var model in _models)
                model.Draw(_effect);

            base.Draw(gameTime);
        }

        private void OnDraw()
        {
            if (OnDrawEvent != null)
                OnDrawEvent();
        }

        public event OnDrawHandler 
            OnDrawEvent = null;

        private void OnMetaDataDisplay(string id)
        {
            if (OnMetaDataDisplayEvent != null)
                OnMetaDataDisplayEvent(id);
        }

        public event OnMetaDataDisplayHandler
            OnMetaDataDisplayEvent = null;
    }

    public delegate void OnDrawHandler();
    public delegate void OnMetaDataDisplayHandler(string id);
}
