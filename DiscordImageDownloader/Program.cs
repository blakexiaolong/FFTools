using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace DiscordImageDownloader
{
    class Program
    {
        public const int PAUSE_COUNT = 10;
        public const int PAUSE_MS = 200;

        public const string IMAGE_IN_PATH = @"D:\Pictures\discord-image-downloader\image-input";
        public const string IMAGE_OUT_PATH = @"D:\Pictures\discord-image-downloader\image-output";

        public const string POSE_IN_PATH = @"D:\Pictures\discord-image-downloader\pose-input";
        public const string POSE_OUT_PATH = @"D:\Pictures\discord-image-downloader\pose-output";

        static void Main()
        {
            SaveNodes(IMAGE_IN_PATH, IMAGE_OUT_PATH, "//*[@class=\"chatlog__attachment\"]//a//img", (counter, countersUsed, majorInterval, mult, client, folder, nodes) =>
            {
                int ct = 0;
                foreach (var tag in nodes)
                {
                    HandleCounter(ref counter, ref countersUsed, ref majorInterval, mult);
                    DownloadFiles(client, folder, tag, "src", ref ct);
                }
                return counter;
            });

            SaveNodes(POSE_IN_PATH, POSE_OUT_PATH, "//*[@class=\"chatlog__message \"]", (counter, countersUsed, majorInterval, mult, client, folder, nodes) =>
            {
                int ct = 0;
                string poseName, poseFolder = folder;
                foreach (var tag in nodes)
                {
                    HandleCounter(ref counter, ref countersUsed, ref majorInterval, mult);

                    HtmlNode markdown = tag.SelectSingleNode("div[@class=\"chatlog__content\"]//div[@class=\"markdown\"]");
                    if (markdown.InnerText.Contains(@"Name:"))
                    {
                        string startString = "Name:";
                        int start = markdown.InnerText.IndexOf(startString);
                        string s = markdown.InnerText.Substring(start + startString.Length);

                        if (s.StartsWith('\n')) s = s.Substring(1);
                        foreach (char c in new[] { '/', '\\', ':', '*', '?', '"', '<', '>', '|' }) s = s.Replace(c, '&');
                        foreach (string c in new[] { "\n", "▫️", "⬝" })
                        {
                            int stopIndex = s.IndexOf(c);
                            if (stopIndex > 0 && stopIndex < s.Length) s = s.Substring(0, stopIndex);
                        }
                        poseName = StripName(s);

                        poseFolder = Path.Combine(folder, poseName);
                        Directory.CreateDirectory(poseFolder);

                        // dump the pose description data to a folder here
                        File.WriteAllText(Path.Combine(poseFolder, "description.html"), tag.InnerHtml);
                    }

                    HtmlNodeCollection attachments = tag.SelectNodes("*//a");
                    if (attachments != null)
                    {
                        foreach (var link in attachments)
                        {
                            DownloadFiles(client, poseFolder, link, "href", ref ct);
                        }
                    }
                }

                return counter;
            });
        }

        static void HandleCounter(ref long counter, ref long countersUsed, ref bool majorInterval, double mult)
        {
            while (counter / mult > countersUsed)
            {
                countersUsed++;
                if (majorInterval) Console.Write("=");
                else Console.Write("-");
                majorInterval = !majorInterval;
            }
            if (counter % PAUSE_COUNT == 0) Thread.Sleep(PAUSE_MS);
            counter++;
        }

        static void DownloadFiles(WebClient client, string folder, HtmlNode link, string attributeName, ref int ct)
        {
            foreach (var attribute in link.Attributes.AttributesWithName(attributeName))
            {
                string[] splits = attribute.Value.Split('/');
                string filePath = Path.Combine(folder, $"{splits[^2]}-{splits[^1]}");
                if (File.Exists(filePath)) continue;
                string[] linkSplits = attribute.Value.Split(".");
                if (linkSplits.Length <= 3 || !new[] { "cmp", "png", "jpg", "jpeg" }.Contains(linkSplits.LastOrDefault())) continue;

                ct++;

                try { client.DownloadFile(attribute.Value, filePath); }
                catch { }
            }
        }

        static string StripName(string input) => Regex.Replace(input, "<.*?>", string.Empty).Replace("▫️", "").Replace("?", "").Replace(":", "").Replace("&quot;", "").Replace("\n", " ").Replace("\r", " ").Replace("&#39;", "'").Trim(new[] { ' ', '.' });

        static void SaveNodes(string inPath, string outPath, string xPath, Func<long, long, bool, double, WebClient, string, HtmlNodeCollection, long> tagHandler)
        {
            Directory.CreateDirectory(inPath);
            Directory.CreateDirectory(outPath);
            using WebClient client = new WebClient();
            IEnumerable<string> fileNames = Directory.EnumerateFiles(inPath);
            Console.WriteLine($"{fileNames.Count()} files found");
            foreach (string fileName in fileNames)
            {
                string folderName = GetFolderName(fileName);
                Console.WriteLine(Path.GetFileNameWithoutExtension(folderName));
                string fileFolder = Path.Combine(outPath, Path.GetFileNameWithoutExtension(folderName));
                Directory.CreateDirectory(fileFolder);

                HtmlDocument html = new HtmlDocument();
                html.LoadHtml(File.ReadAllText(fileName));
                HtmlNodeCollection nodes = html.DocumentNode.SelectNodes(xPath);
                Console.WriteLine($"{nodes.Count} nodes found");

                long div = 21, countersUsed = 1, counter = 0;
                double mult = nodes.Count / (double)div;
                bool majorInterval = false;

                tagHandler.Invoke(counter, countersUsed, majorInterval, mult, client, fileFolder, nodes);
                Console.WriteLine("");
            }
        }

        static string GetFolderName(string fileName)
        {
            string[] splits = fileName.Split(']');
            return string.Join(']', splits.Take(splits.Length - 1)) + ']';
        }
    }
}