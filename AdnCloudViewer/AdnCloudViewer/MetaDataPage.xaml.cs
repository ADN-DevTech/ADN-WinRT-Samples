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
    public sealed partial class MetaDataPage : Page
    {
        private List<AdnMeshData> _data;

        private AdnMetaData _metaData;

        public MetaDataPage(
            List<AdnMeshData> data, 
            AdnMetaData metaData)
        {
            this.InitializeComponent();

            KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Escape)
                    GoBack(this, null);
            };

            _data = data;

            _metaData = metaData;

            ItemListView.DataContext =
                new ObservableCollection<object>(_metaData.Elements);
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            var page = new ViewerPage(_data);

            Window.Current.Content = page;
        }

        private async void Commit_Click(object sender, RoutedEventArgs e)
        {
            string _hostAddress = "23.23.212.64:80";

            string url =
               "http://" + _hostAddress + "/AdnViewerSrv/rest/UpdateMetaData";

            string jsonMsg = JsonConvert.SerializeObject(_metaData,
              Formatting.None,
              new JsonSerializerSettings { });

            byte[] uploadData = GetBytes(jsonMsg);

            HttpWebRequest request = WebRequest.Create(url)
                as HttpWebRequest;

            request.Method = "POST";
            request.ContentType = "application/json";

            using (Stream stream = await request.GetRequestStreamAsync())
            {
                stream.Write(uploadData, 0, uploadData.Length);

                var response = request.GetResponseAsync();
            }

            var page = new ViewerPage(_data);

            Window.Current.Content = page;
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
