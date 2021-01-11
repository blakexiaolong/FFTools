using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;

namespace ModManager.Models
{
    public class ModData
    {
        public static string ModArchive = "https://www.xivmodarchive.com";

        public string Name { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string[] ImageUrls { get; set; }
        public Dictionary<string, string> Files { get; set; }
        public Dictionary<string, string> OtherFiles { get; set; }

        public ModData(string url)
        {
            Url = url;
            Scrape();
        }
        private void Scrape()
        {
            HtmlDocument doc = new HtmlDocument();
            using (WebClient client = new WebClient())
            {
                doc.LoadHtml(client.DownloadString(Url));

                Name = doc.DocumentNode.SelectSingleNode("//*//h1").InnerText; // name
                Description = doc.DocumentNode.SelectSingleNode("//*[@id='info']").InnerText; // description html
                Author = doc.DocumentNode
                    .SelectNodes("//*[@class='user-card-link']")
                    .First(x => !string.IsNullOrEmpty(x.InnerText.Trim('\n'))).InnerText; // author
                ImageUrls = doc.DocumentNode
                    .SelectNodes("//*[@id='mod-images']//div//div//a//img")
                    .SelectMany(x => x.Attributes.Where(y => y.Name.Contains("src")).Select(y => y.Value)).ToArray(); // images url link
                Files = doc.DocumentNode
                    .SelectNodes("//*[@class='primary-download-listing']")
                    .ToDictionary(x =>
                        x.ChildNodes[0].InnerHtml.Split(':')[0],
                        x => x.ChildNodes[1].Attributes.First(y => y.Name == "href").Value.StartsWith("/")
                            ? $"{ModArchive}{x.ChildNodes[1].Attributes.First(y => y.Name == "href").Value}"
                            : x.ChildNodes[1].Attributes.First(y => y.Name == "href").Value);
                OtherFiles = doc.DocumentNode
                    .SelectNodes("//*[@id='files']//div//ul//li")
                    .ToDictionary(x =>
                        x.ChildNodes[0].InnerHtml.Split(':')[0],
                        x => x.ChildNodes[1].Attributes.First(y => y.Name == "href").Value.StartsWith("/")
                            ? $"{ModArchive}{x.ChildNodes[1].Attributes.First(y => y.Name == "href").Value}"
                            : x.ChildNodes[1].Attributes.First(y => y.Name == "href").Value);
            }
        }
        public List<Mod> Import(string dirPath)
        {
            Directory.CreateDirectory(dirPath);
            string filesPath = Path.Combine(dirPath, Name);
            File.WriteAllText(Path.Combine(filesPath, "Description"), $@"
                Source: {Url}
                Author: {Author}
                {Description.Replace("<br>", "\n")}"
            );
            using (WebClient client = new WebClient())
            {
                for (int i = 0; i < ImageUrls.Length; i++)
                {
                    client.DownloadFile(ImageUrls[i], Path.Combine(filesPath, $"{i}.{Path.GetExtension(ImageUrls[i])}"));
                }
                DownloadFiles(client, filesPath, Files, false);
                DownloadFiles(client, filesPath, OtherFiles, true);
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

        private void DownloadFiles(WebClient client, string filesPath, Dictionary<string, string> files, bool showBox)
        {
            int counter = 0;
            foreach (KeyValuePair<string, string> file in files)
            {
                if (file.Value.StartsWith(ModArchive))
                {
                    client.DownloadFile(file.Value, Path.Combine(filesPath, $"{counter}.{Path.GetExtension(file.Value)}"));
                }
                else
                {
                    MessageBoxResult result = MessageBoxResult.Yes;
                    if (showBox)
                    {
                        result = MessageBox.Show("Open File?", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                    }
                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(Url);
                    }
                }
            }
        }
    }
}
