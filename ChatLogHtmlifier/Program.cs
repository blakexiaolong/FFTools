using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ChatLogHtmlifier
{
    public class ChatLogHtmlifier
    {
        public const string TEXT_IN = @"C:\Users\Andrew\Documents\FFXIV_ChatExtender\Logs";

        public static void Main()
        {
            string outFolder = Path.Combine(TEXT_IN, "html");
            Directory.CreateDirectory(outFolder);

            IEnumerable<string> fileNames = Directory.EnumerateFiles(TEXT_IN);
            Console.WriteLine($"{fileNames.Count()} files found");

            foreach (string fileName in fileNames)
            {
                string outFile = Path.ChangeExtension(Path.Combine(outFolder, Path.GetFileName(fileName)), ".html");
                if (File.Exists(outFile)) continue;

                List<string> outLines = new List<string>();
                List<string> lines = File.ReadAllLines(fileName).ToList();
                for (int i = 0; i < lines.Count(); i++)
                {
                    if (!lines[i].StartsWith(' ')) continue;

                    lines[i-1] += lines[i];
                    lines.RemoveAt(i);
                    i--;
                }

                foreach (string line in lines)
                {
                    string outLine = "";
                    string[] splits = line.Split(']');
                    if (Regex.Match(line, @"\[[0-9]{2}\:[0-9]{2}\]\[\w*\]<?.*>?:.*").Success)
                    {
                        try
                        {
                            string color = GetColor(splits[1]);
                            string endText = string.Join(']', splits.Skip(2));
                            outLine =
                                $"<span style=\"color: grey; text-shadow: 2px 2px 4px #666666;\">{splits[0]}]</span>" +
                                $"<span style=\"color: {color}; text-shadow: 2px 2px 4px #666666;\">{splits[1]}]</span>" +
                                $"<span style=\"color: {color}; text-shadow: 2px 2px 4px #666666;\">{MakeHtmlSafe(endText)}</span>";
                        }
                        catch (FormatException e)
                        {
                            throw new FormatException($"{e.Message}: {line} in {fileName}");
                        }
                    }
                    else if (!line.Contains('['))
                    {
                        outLine = $"<span style=\"color: black; text-shadow: 1px 1px 2px #666666;\">{splits[0]}</span>";
                    }
                    else
                    {
                        throw new FormatException($"Line was not in expected format -> {line}");
                    }

                    outLines.Add(outLine + "<br />");
                }

                outLines.Insert(0, "<html style=\"background-color:#40444b\">");
                outLines.Add("</html>");

                File.WriteAllLines(outFile, outLines);
            }
        }

        public static string GetColor(string identifier)
        {
            string it = identifier.Trim('[');
            return it switch
            {
                // system things
                "SystemError" => "red",
                "Notice" => "#B6B6B4",
                "FCLogin" => "white",
                "FCLogout" => "white",
                "SystemMessage" => "#B041FF",
                "GatheringSystemMessage" => "#B6B6B4",
                "RetainerSale" => "#B6B6B4",
                "FCAnnouncement" => "#B6B6B4",
                "StandardEmote" => "aliceblue",
                "CustomEmote" => "aliceblue",

                // chat channels
                "Echo" => "#B6B6B4",
                "FreeCompany" => "orange",
                "Party" => "dodgerblue",
                "Alliance" => "indigo",
                "Say" => "gainsboro",
                "Yell" => "yellow",
                "Shout" => "darkorange",
                "TellIncoming" => "magenta",
                "TellOutgoing" => "magenta",

                // linkshells
                "Ls1" => "skyblue",
                "Ls2" => "yellowgreen",
                "Ls3" => "beeyellow",
                "Ls4" => "chilipepper",
                "Ls5" => "redwine",
                "CrossLinkShell3" => "greenonion",
                "CrossLinkShell4" => "greenonion",

                // no idea what color to make these
                "None" => "black",
                "Loot" => "black",
                "Ls6" => "black",
                "Ls7" => "black",
                "CrossLinkShell1" => "black",
                "CrossLinkShell2" => "black",
                _ => throw new FormatException($"Chat code was not in expected format or recognized -> {it}"),
            };
        }

        public static string MakeHtmlSafe(string text)
        {
            string workingText = text;
            if (workingText.StartsWith("<>")) workingText = string.Join("", workingText.Skip(2));
            return workingText.Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }
}
