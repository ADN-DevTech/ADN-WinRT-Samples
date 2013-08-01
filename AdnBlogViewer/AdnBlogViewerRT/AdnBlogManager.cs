using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace AdnBlogViewerRT
{
    class AdnBlogManager
    {
        // performs REST request to retrieve the 50 most recent posts
        // http://api.typepad.com/blogs/blogId/post-assets/@published/@recent.js

        public async static Task<AdnBlogPostCollection> GetBlogPosts(
            string blogDisplayName, 
            string blogId)
        {
            string baseUrl = 
                "http://api.typepad.com/blogs/" 
                + blogId + 
                "/post-assets/@published/";

            RestClient client = new RestClient(baseUrl);

            RestRequest request = new RestRequest
            {
                Resource = "@recent.js",
                Method = Method.GET,
            };

            IRestResponse response =
                await client.ExecuteAsync(request);

            // removes "callback{ ... }" component around reply
            string content = response.Content.Substring(
                9, response.Content.Length-17);

            // uses Linq to JSON
            JObject result = JObject.Parse(content);

            // 50 posts entries
            IList<JToken> entries = 
                result["entries"].Children().ToList();

            return new AdnBlogPostCollection(blogDisplayName, entries);
        }
    }

    class AdnBlogPostCollection
    {
        public AdnBlogPostCollection(
            string blogDisplayName,
            IList<JToken> entries)
        {
            BlogDisplayName = blogDisplayName;

            Items = new List<AdnBlogPost>();

            foreach (var entry in entries)
            {
                string str = entry.ToString();

                AdnBlogPost post = JsonConvert.DeserializeObject(
                    str,
                    typeof(AdnBlogPost)) as AdnBlogPost;

                Items.Add(post);
            }
        }

        [JsonConstructor]
        public AdnBlogPostCollection(
            string blogDisplayName,
            List<AdnBlogPost> items)
        {
            BlogDisplayName = blogDisplayName;

            Items = items;
        }

        public List<AdnBlogPost> Items
        {
            get;
            private set;
        }

        public string BlogDisplayName
        {
            get;
            private set;
        }
    }

    class AdnBlogPost
    {
        public string urlId
        {
            get;
            set;
        }

        public string content
        {
            get;
            set;
        }

        public Author author
        {
            get;
            set;
        }

        public Container container
        {
            get;
            set;
        }
         
        public string title;

        public string DecodedTitle
        {
            get
            {
                return System.Net.WebUtility.HtmlDecode(title);
            }
        }
    }

	class Author
	{
        public string displayName
        {
            get;
            set;
        }

        public Avatar avatarLink
        {
            get;
            set;
        }
	}
	
	class Container
	{
        public string urlId
        {
            get;
            set;
        }
	}

    class Avatar
    {
        public int height
        {
            get;
            set;
        }

        public int width
        {
            get;
            set;
        }

        public string url
        {
            get;
            set;
        }
    }
}
