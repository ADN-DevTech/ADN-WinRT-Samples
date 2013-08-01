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

namespace AdnCloudViewer
{
    public sealed partial class CloudModelSelectPage
    {
        private List<AdnMeshData> _data;

        public CloudModelSelectPage(List<AdnMeshData> data)
        {
            this.InitializeComponent();

            KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Escape)
                    GoBack(this, null);
            };

            _data = data;

            GetModelsFromWeb();
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            var mainPage = new ViewerPage(_data);
            Window.Current.Content = mainPage;
        }

        async void GetModelsFromWeb()
        {
            try
            {
                string _hostAddress = "23.23.212.64:80";

                HttpWebRequest request = WebRequest.Create(
                        "http://" + _hostAddress + "/AdnViewerSrv/rest/GetDbModelData")
                            as HttpWebRequest;

                using (HttpWebResponse response = await request.GetResponseAsync()
                            as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(
                        response.GetResponseStream());

                    string jsonMsg = reader.ReadToEnd();

                    var modelData = JsonConvert.DeserializeObject
                        <List<AdnDbModelData>>(jsonMsg);

                    ItemListView.DataContext = 
                        new ObservableCollection<object>(modelData);
                }
            }
            catch
            {
                // Goes back to previous page
                GoBack(this, null);
            }
        }

        private void ItemListView_SelectionChanged(
            object sender, 
            SelectionChangedEventArgs e)
        {
            AdnDbModelData item = ItemListView.SelectedItem 
                as AdnDbModelData;

            var page = new ViewerPage(item);

            Window.Current.Content = page;
        }
    }
}
