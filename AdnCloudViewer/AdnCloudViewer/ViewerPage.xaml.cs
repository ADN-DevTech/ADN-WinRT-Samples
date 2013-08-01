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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Threading.Tasks;

namespace AdnCloudViewer
{
    public sealed partial class ViewerPage
    {
        private AdnGestureManager _gestureManager;
        private List<AdnMeshData> _currentMeshData;
        private AdnDbModelData _currentModelData;
        private AdnRenderer _renderer;

        public ViewerPage()
        {
            InitializeComponent();

            KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Escape)
                    OnQuit();
            };

            _renderer = new AdnRenderer();

            _renderer.OnMetaDataDisplayEvent += 
                OnMetaDataDisplayEvent;           

            _gestureManager =
                new AdnGestureManager(
                    this,
                    _renderer);

            _renderer.Run(this.DXSwapChainPanel);
        }

        async void OnMetaDataDisplayEvent(string metaDataId)
        {
            var metaData = await LoadMetaDataFromWeb(metaDataId);

            if (metaData != null)
            {
                _renderer.Exit();

                var page = new MetaDataPage(
                    _currentMeshData,
                    metaData);

                Window.Current.Content = page;
            }
        }

        public ViewerPage(AdnDbModelData data)
            : this()
        {
            _currentModelData = data;

            _renderer.OnDrawEvent +=
                OnContentLoaded;
        }

        public ViewerPage(List<AdnMeshData> data)
            : this()
        {
            _currentMeshData = data;

            _renderer.OnDrawEvent += 
                OnContentLoaded;
        }

        void _renderer_Activated(object sender, EventArgs e)
        {
            OnContentLoaded();
        }

        void OnContentLoaded()
        {
            if (_currentModelData != null)
                LoadModelFromWeb(_currentModelData);

            if (_currentMeshData != null)
                _renderer.LoadModel(_currentMeshData);

            _renderer.OnDrawEvent -=
                OnContentLoaded;
        }

        private void OnAppBarOpened(object sender, object e)
        {
            //Not used...
        }

        private void OnCloseButtonClicked(
            object sender,
            RoutedEventArgs e)
        {
            _currentMeshData = null;

            _renderer.CloseModel();
        }

        private void OnOpenButtonClicked(
            object sender,
            RoutedEventArgs e)
        {
            _renderer.Exit();

            var page = new CloudModelSelectPage(
                _currentMeshData);

            Window.Current.Content = page;
        }

        private async void SaveToLocal(
            AdnDbModelData data,
            string json)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            StorageFolder folder = await localFolder.CreateFolderAsync(
                "LocalModels",
                CreationCollisionOption.OpenIfExists);

            StorageFile file = await folder.CreateFileAsync(
                data.ModelName,
                CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteTextAsync(file, json);
        }

        private void OnLoadButtonClicked(
            object sender, 
            RoutedEventArgs e)
        {
            _renderer.Exit();

            var page = new LocalModelSelectPage(
                _currentMeshData);

            Window.Current.Content = page;
        }

        async void LoadModelFromWeb(
            AdnDbModelData data)
        {
            try
            {
                string _hostAddress = "23.23.212.64:80";

                HttpWebRequest request = WebRequest.Create(
                       "http://" + _hostAddress + "/AdnViewerSrv/rest/GetMeshData/" + data.ModelId)
                           as HttpWebRequest;

                using (HttpWebResponse response = await request.GetResponseAsync()
                            as HttpWebResponse)
                {
                    StreamReader reader =
                        new StreamReader(response.GetResponseStream());

                    string jsonMsgZipped = reader.ReadToEnd().
                        Replace("\\", "").
                        Replace("\"", "");

                    string jsonMsg = AdnDataUtils.Decompress(
                        jsonMsgZipped);

                    _currentMeshData =
                        JsonConvert.DeserializeObject
                            <List<AdnMeshData>>(jsonMsg);

                    _renderer.LoadModel(_currentMeshData);

                    SaveToLocal(
                        data,                       
                        jsonMsgZipped);
                }
            }
            catch
            {

            }
        }

        async Task<AdnMetaData> LoadMetaDataFromWeb(
            string metaDataId)
        {
            try
            {
                string _hostAddress = "23.23.212.64:80";

                HttpWebRequest request = WebRequest.Create(
                       "http://" + _hostAddress + "/AdnViewerSrv/rest/GetMetaData/" + metaDataId)
                           as HttpWebRequest;

                using (HttpWebResponse response = await request.GetResponseAsync()
                            as HttpWebResponse)
                {
                    StreamReader reader =
                        new StreamReader(response.GetResponseStream());

                    string jsonMsgZipped = reader.ReadToEnd().
                       Replace("\\", "").
                       Replace("\"", "");

                    string jsonMsg = AdnDataUtils.Decompress(jsonMsgZipped);

                    AdnMetaData metaData = JsonConvert.DeserializeObject
                         <AdnMetaData>(jsonMsg);

                    return metaData;
                }
            }
            catch
            {
                return null;
            }
        }

        private void OnQuitButtonClicked(object sender, RoutedEventArgs e)
        {
            OnQuit();
        }

        private async void OnQuit()
        {
            MessageDialog dialog = new MessageDialog("Comfirm Quit...?");

            UICommand cmdYes = new UICommand("Yes", (cmd) =>
            {
                App.Current.Exit();
            }, 1);

            UICommand cmdNo = new UICommand("No", null, 2);

            dialog.Commands.Add(cmdNo);
            dialog.Commands.Add(cmdYes);

            dialog.DefaultCommandIndex = 2;

            await dialog.ShowAsync();
        }
    }
}
