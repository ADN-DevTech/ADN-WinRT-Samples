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
using Windows.UI.Xaml.Media.Imaging;
using NotificationsExtensions.TileContent;
using Windows.UI.Notifications;
using Newtonsoft.Json;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace AdnBlogViewerRT
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class ItemDetailPage : AdnBlogViewerRT.Common.LayoutAwarePage
    {
        AdnBlogPost _currentPost;

        public ItemDetailPage()
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
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // Allow saved page state to override the initial item to display
            if (pageState != null && pageState.ContainsKey("Content"))
            {
                navigationParameter = JsonConvert.DeserializeObject<AdnBlogPost>
                    (pageState["Content"] as string);
            }

            _currentPost = navigationParameter as AdnBlogPost;

            pageTitle.Text = _currentPost.DecodedTitle;

            Image.Source = new BitmapImage(
                new Uri(_currentPost.author.avatarLink.url, UriKind.Absolute));

            string html = _currentPost.content;

            webview.NavigateToString(html);

            UpdateTileContent();
        }

        private void UpdateTileContent()
        {
            string stringContent = 
                _currentPost.DecodedTitle +
                " by " + 
                _currentPost.author.displayName;

            // create the wide template
            ITileWideText03 tileContent = 
                TileContentFactory.CreateTileWideText03();

            tileContent.TextHeadingWrap.Text = stringContent;

            // Users can resize tiles to square or wide.
            // Apps can choose to include only square assets 
            // (meaning the app's tile can never be wide), or
            // include both wide and square assets 
            // (the user can resize the tile to square or wide).
            // Apps cannot include only wide assets.

            // Apps that support being wide should include 
            // square tile notifications since users
            // determine the size of the tile.

            // create the square template and attach it to the wide template
            ITileSquareText04 squareContent = 
                TileContentFactory.CreateTileSquareText04();

            squareContent.TextBodyWrap.Text = stringContent;

            tileContent.SquareContent = squareContent;

            // send the notification
            TileUpdateManager.CreateTileUpdaterForApplication().Update(
                tileContent.CreateNotification());
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["Content"] = JsonConvert.SerializeObject(_currentPost);
        }
    }
}
