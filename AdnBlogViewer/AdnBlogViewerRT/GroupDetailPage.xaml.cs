using AdnBlogViewerRT.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using Newtonsoft.Json;
using AdnBlogViewerRT.Common;

// The Group Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234229

namespace AdnBlogViewerRT
{
    /// <summary>
    /// A page that displays an overview of a single group, including a preview of the items
    /// within the group.
    /// </summary>
    public sealed partial class GroupDetailPage : AdnBlogViewerRT.Common.LayoutAwarePage
    {
        AdnBlogPostCollection _postCollection;

        public GroupDetailPage()
        {
            this.InitializeComponent();

            KeyDown += (s, e) =>
            {
                if (e.Key == Windows.System.VirtualKey.Escape)
                    App.Quit();
            };
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected async override void LoadState(
            Object navigationParameter, 
            Dictionary<String, Object> pageState)
        {
            // Allow saved page state to override the initial item to display
            if (pageState != null && pageState.ContainsKey("Content"))
            {
                _postCollection =
                    JsonConvert.DeserializeObject<AdnBlogPostCollection>(
                        pageState["Content"] as string);
            }
            else
            { 
                ShowWaitDialog();

                string blogId = (string)navigationParameter;

                _postCollection = await AdnBlogManager.GetBlogPosts(
                    "Cloud & Mobile DevBlog", 
                    blogId);

                HideWaitDialog();
            }

            listView.DataContext =
                new ObservableCollection<object>(
                    _postCollection.Items);

            listView.SelectionChanged += SelectionChanged;

            pageTitle.Text = _postCollection.BlogDisplayName;          
        }

        void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AdnBlogPost item = listView.SelectedItem as AdnBlogPost;
            this.Frame.Navigate(typeof(ItemDetailPage), item);
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["Content"] = JsonConvert.SerializeObject(_postCollection);
        }

        private void HideWaitDialog()
        {
            rectBackgroundHide.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            waitPopup.IsOpen = false;
        }

        private void ShowWaitDialog()
        {
            Message.Text = "Loading Posts, Please Wait...";

            CoreWindow currentWindow = Window.Current.CoreWindow;

            //Set our background rectangle to fill the entire window
            rectBackgroundHide.Height = currentWindow.Bounds.Height;
            rectBackgroundHide.Width = currentWindow.Bounds.Width;
            rectBackgroundHide.Margin = new Thickness(0, 0, 0, 0);

            //Make sure the background is visible
            rectBackgroundHide.Visibility = Windows.UI.Xaml.Visibility.Visible;

            //Now we figure out where the center of the screen is, and we
            //move the popup to that location.
            waitPopup.HorizontalOffset = (currentWindow.Bounds.Width / 2) - (400 / 2);
            waitPopup.VerticalOffset = (currentWindow.Bounds.Height / 2) - (150 / 2);
            waitPopup.IsOpen = true;
        }
    }
}
