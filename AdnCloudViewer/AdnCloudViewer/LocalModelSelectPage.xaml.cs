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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;

namespace AdnCloudViewer
{
    public sealed partial class LocalModelSelectPage
    {
        private List<AdnMeshData> _data;

        public LocalModelSelectPage(List<AdnMeshData> data)
        {
            this.InitializeComponent();

            KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Escape)
                    GoBack(this, null);
            };

            _data = data;

            GetModelsFromLocalFolder();

            ItemListView.IsItemClickEnabled = true;

            ItemListView.ItemClick += ItemListView_ItemClick;  
        }

        async void ItemListView_ItemClick(
            object sender, 
            ItemClickEventArgs e)
        {
            ModelInfo item = e.ClickedItem
               as ModelInfo;

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            StorageFolder folder = await localFolder.CreateFolderAsync(
                "LocalModels",
                CreationCollisionOption.OpenIfExists);

            StorageFile file = await folder.GetFileAsync(
                item.ModelName);

            string jsonMsgZipped = await FileIO.ReadTextAsync(file);

            string jsonMsg = AdnDataUtils.Decompress(
                jsonMsgZipped);

            var meshData =
                JsonConvert.DeserializeObject
                    <List<AdnMeshData>>(jsonMsg);

            var mainPage = new ViewerPage(meshData);

            Window.Current.Content = mainPage;
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            var mainPage = new ViewerPage(_data);

            Window.Current.Content = mainPage;
        }

        async void GetModelsFromLocalFolder()
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;

                StorageFolder folder = await localFolder.CreateFolderAsync(
                    "LocalModels",
                    CreationCollisionOption.OpenIfExists);

                var files = await folder.GetFilesAsync();

                List<ModelInfo> modelInfo = new List<ModelInfo>();

                foreach (var file in files)
                {
                    string name = file.Name;

                    modelInfo.Add(new ModelInfo(name));
                }

                ItemListView.DataContext =
                     new ObservableCollection<object>(modelInfo);                
            }
            catch
            {

            }
        }

        private void ItemListView_SelectionChanged(
            object sender, 
            SelectionChangedEventArgs e)
        {
            ModelInfo item = ItemListView.SelectedItem 
                as ModelInfo;

            //var page = new ViewerPage(item.ModelId);
            //Window.Current.Content = page;
        }

        class ModelInfo
        {
            public ModelInfo(string name)
            {
                ModelName = name;
            }

            public string ModelName
            {
                get;
                private set;
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ItemListView.SelectedItem != null)
            {
                ModelInfo item = ItemListView.SelectedItem
                   as ModelInfo;

                StorageFolder localFolder = ApplicationData.Current.LocalFolder;

                StorageFolder folder = await localFolder.GetFolderAsync(
                    "LocalModels");

                StorageFile file = await folder.GetFileAsync(
                    item.ModelName);

               await file.DeleteAsync(StorageDeleteOption.PermanentDelete);

               // Reloads models
               GetModelsFromLocalFolder();
            }
        }
    }
}
