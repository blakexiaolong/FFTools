using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;

namespace ModManager.Models
{
    public class ModData
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string[] ImageUrls { get; set; }
        public Dictionary<string, string> Files { get; set; }
        public Dictionary<string, string> OtherFiles { get; set; }

        public ModData(string url)
        {
            Url = url;
            ImageUrls = new string[] { };
            Files = new Dictionary<string, string>();
            OtherFiles = new Dictionary<string, string>();
            Scrape();
        }
        private void Scrape()
        {
            HtmlDocument doc = new HtmlDocument();
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.Headers.Add(HttpRequestHeader.Cookie, Properties.Settings.Default.SessionCookie);
                try
                {
                    doc.LoadHtml(client.DownloadString(Url));

                    Name = doc.DocumentNode.SelectSingleNode("//*//h1").InnerText; // name
                    Description = StringifyHtml(doc.DocumentNode.SelectSingleNode("//*[@id='info']")); // description html
                    Author = doc.DocumentNode
                        .SelectNodes("//*[@class='user-card-link']")
                        .First(x => !string.IsNullOrEmpty(x.InnerText.Trim('\n'))).InnerText; // author
                    Version = doc.DocumentNode.SelectNodes("//*/code").First().InnerText;
                    ImageUrls = doc.DocumentNode
                        .SelectNodes("//*[@id='mod-images']//div//div//a//img")
                        .SelectMany(x => x.Attributes.Where(y => y.Name.Contains("src")).Select(y => y.Value)).ToArray(); // images url link
                    Files = doc.DocumentNode
                        .SelectNodes("//*[@class='primary-download-listing']")
                        .ToDictionary(x =>
                            x.ChildNodes[0].InnerHtml.Split(':')[0],
                            x => x.ChildNodes[1].Attributes.First(y => y.Name == "href").Value.StartsWith("/")
                                ? $"{Properties.Settings.Default.XMAUrl}{x.ChildNodes[1].Attributes.First(y => y.Name == "href").Value}"
                                : x.ChildNodes[1].Attributes.First(y => y.Name == "href").Value);
                    try
                    {
                        OtherFiles = doc.DocumentNode
                            .SelectNodes("//*[@id='files']//div//ul//li")
                            .ToDictionary(x =>
                                x.ChildNodes[0].InnerHtml.Split(':')[0],
                                x => x.ChildNodes[1].Attributes.First(y => y.Name == "href").Value.StartsWith("/")
                                    ? $"{Properties.Settings.Default.XMAUrl}{x.ChildNodes[1].Attributes.First(y => y.Name == "href").Value}"
                                    : x.ChildNodes[1].Attributes.First(y => y.Name == "href").Value);
                    }
                    catch
                    {
                        OtherFiles = new Dictionary<string, string>();
                    }
                }
                catch (WebException e)
                {
                    if (e.Message.Contains("(403)"))
                    {
                        MessageBox.Show(
                            "We're getting an unhappy error from the Mod Archive. Please go into Path Settings and set a non-expired Session ID, as thats likely what's causing the issue.",
                            "403 Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                    }
                }
            }
        }
        public List<Mod> Import(string dirPath)
        {
            bool openedExplorer = false;
            NukeInvalidDirChars();
            string filesPath = Path.Combine(dirPath, Name);
            Directory.CreateDirectory(filesPath);

            File.WriteAllText(Path.Combine(filesPath, "Description.txt"), $@"
                Source: {Url}
                Author: {Author}
                {Description.Replace("<br>", "\n")}"
            );
            File.WriteAllText(Path.Combine(filesPath, Version.Replace(":", "")), Version);
            using (WebClient client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.Cookie, Properties.Settings.Default.SessionCookie);
                for (int i = 0; i < ImageUrls.Length; i++)
                {
                    client.DownloadFile(ImageUrls[i], Path.Combine(filesPath, $"{i}.{Path.GetExtension(ImageUrls[i])}"));
                }
                DownloadFiles(client, filesPath, Files, false, ref openedExplorer);
                DownloadFiles(client, filesPath, OtherFiles, true, ref openedExplorer);
            }

            string[] ttmpFiles = Directory.GetFiles(filesPath, "*.ttmp", SearchOption.AllDirectories);
            string[] ttmp2Files = Directory.GetFiles(filesPath, "*.ttmp2", SearchOption.AllDirectories);

            List<Mod> importedMods = new List<Mod>();
            List<string> files = ttmpFiles.Concat(ttmp2Files).ToList();
            foreach(string file in files)
            {
                Mod mod = new Mod();
                mod.ImportFromFile(file);
                importedMods.Add(mod);
            }

            return importedMods;
        }
        public void NukeInvalidDirChars()
        {
            char[] invalidChars = Path.GetInvalidPathChars();
            foreach (char c in invalidChars) Name = Name.Replace(c, '_');
        }

        private void DownloadFiles(WebClient client, string filesPath, Dictionary<string, string> files, bool showBox, ref bool openedExplorer)
        {
            int counter = 0;
            foreach (KeyValuePair<string, string> file in files)
            {
                if (file.Value.StartsWith(Properties.Settings.Default.XMAUrl))
                {
                    client.DownloadFile(file.Value, Path.Combine(filesPath, $"{counter}{Path.GetExtension(file.Value)}"));
                }
                else
                {
                    MessageBoxResult result = MessageBoxResult.None;
                    if (showBox)
                    {
                        result = MessageBox.Show($"{file.Key} -> {file.Value}", "Open File?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                    }
                    if (!showBox || result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(file.Value);
                        if (!openedExplorer)
                        {
                            System.Diagnostics.Process.Start(filesPath);
                            openedExplorer = true;
                        }
                    }
                }
            }
        }

        private string StringifyHtml(HtmlNode node, bool top = true)
        {
            string ret = "";

            if (node.ChildNodes.Any())
            {
                foreach (var child in node.ChildNodes)
                {
                    if (child.Name == "br") ret += "\n";
                    else ret += StringifyHtml(child, false);
                }
            }
            else
            {
                ret = node.InnerHtml;
            }

            return top ? ret.Trim('\n') : ret;
        }
    }
}
