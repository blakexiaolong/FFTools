//using LinqToTwitter;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Text;

//namespace DiscordImageDownloader
//{
//    public class TwitterDownloader
//    {
//        private TwitterContext context;
//        private string _tag;
//        private string _root;

//        private readonly WebClient client;
//        private readonly Dictionary<string, string> sources;
//        protected readonly int _tagIndex;

//        public TwitterDownloader(Dictionary<string, string> sources, string tag, int tagIndex, string root)
//        {
//            client = new WebClient();
//            this.sources = sources;
//            _tagIndex = tagIndex;

//            _tag = tag;
//            _root = root;

//            // Setup credentials and things
//            SingleUserAuthorizer authorizer = new SingleUserAuthorizer
//            {
//                CredentialStore = new SingleUserInMemoryCredentialStore
//                {
//                    ConsumerKey = Properties.Settings.Default.TwitterConsumerKey,
//                    ConsumerSecret = Properties.Settings.Default.TwitterConsumerSecret,
//                    AccessToken = Properties.Settings.Default.TwitterAccessToken,
//                    AccessTokenSecret = Properties.Settings.Default.TwitterAccessTokenSecret,
//                }
//            };
//            context = new TwitterContext(authorizer);
//        }
//        public void Run(string suffix)
//        {
//            // Use the tag index to determine our saved starting index
//            ulong[] startIds = Properties.Settings.Default.TwitterStartIds.Split('|').Select(x => Convert.ToUInt64(x)).ToArray();
//            ulong? startIndex = startIds[_tagIndex];
//            startIndex = startIndex == 0 ? null : startIndex;

//            // Run the main downloader function and save the index of the last tweet retrieved
//            startIds[_tagIndex] = Download(_root + suffix, startIndex) ?? 0;
//            Properties.Settings.Default.TwitterStartIds = string.Join("|", startIds);
//            Properties.Settings.Default.Save();
//        }
//        public ulong? Download(string directory, ulong? start)
//        {
//            // Get list of tweets and iterate over them
//            List<Status> tweets = GetTweets(start);
//            foreach (var tweet in tweets)
//            {
//                foreach (var media in tweet.ExtendedEntities.MediaEntities)
//                {
//                    try
//                    {
//                        // Iterate over attached videos and download each
//                        // TODO: need to find a way to remove similar videos of varying quality
//                        if (media.VideoInfo.Variants != null && media.VideoInfo.Variants.Any(x => x.Url != null))
//                        {
//                            int variantCount = 0;
//                            foreach (var variant in media.VideoInfo.Variants.Where(x => x.Url != null))
//                            {
//                                try
//                                {
//                                    client.DownloadFile(variant.Url, Path.Combine(directory, $"{media.ID}_{variantCount++}.{variant.Url.Split('.').Last().Split('?').First()}"));
//                                }
//                                catch (Exception e)
//                                {
//                                    Console.WriteLine(e);
//                                }
//                            }
//                        }
//                        // Download attached image
//                        // TODO: need to find way to download multiple images per post, is this possible?
//                        else client.DownloadFile(media.MediaUrl, Path.Combine(directory, $"{media.ID}.{media.MediaUrl.Split('.').Last()}"));
//                    }
//                    catch (Exception e)
//                    {
//                        Console.WriteLine(e);
//                    }
//                }
//            }
//            // Return the index of the most recent post retrieved, or the input index if no tweets were retrieved
//            return tweets.Any() ? tweets.First().StatusID : start;
//        }
//        public List<Status> GetTweets(ulong? start)
//        {

//            // Get tweets with the specified tag, starting at the given index if available
//            if (start == 0) start = null;
//            return start.HasValue
//                ? Enumerable.SingleOrDefault(
//                    from search in context.Search
//                    where
//                        search.Type == SearchType.Search
//                        && search.Query == _tag
//                        && search.Count == 100
//                        && search.SinceID == start
//                    select search)?.Statuses
//                : Enumerable.SingleOrDefault(
//                    from search in context.Search
//                    where
//                        search.Type == SearchType.Search
//                        && search.Query == _tag
//                        && search.Count == 100
//                    select search)?.Statuses;
//        }
//    }
//}
