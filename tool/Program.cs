using Sang.UAParser;
using System.Text;
using System.Text.RegularExpressions;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

internal class Program
{
    private static void Main(string[] args)
    {
        var rootCommand = new RootCommand("UserAgent Parser");
        var logCommand = new Command("log", "Parse log file");
        logCommand.AddOption(new Option<string>(new string[] { "--logfile", "-f" }, "Log file path"){ IsRequired = true });
        logCommand.AddOption(new Option<string>(new string[] { "--uafile", "-u" }, "UserAgent file save path"){ IsRequired = true });
        logCommand.Handler = CommandHandler.Create<string, string>(ParseLog);
        rootCommand.Add(logCommand);

        var saveCommand = new Command("save", "Parse UserAgent file");
        saveCommand.AddOption(new Option<string>(new string[] { "--uafile", "-u" }, "UserAgent file path"){ IsRequired = true });
        saveCommand.AddOption(new Option<string>(new string[] { "--savefile", "-s" }, "Save file path"){ IsRequired = true });
        saveCommand.Handler = CommandHandler.Create<string, string>(ParseSave);
        rootCommand.Add(saveCommand);

        rootCommand.Invoke(args);
    }

    private static void ParseLog(string logfile, string uafile)
    {
        var lines = File.ReadAllLines(logfile);
        var uas = new List<string>();
        var uaRegex = new Regex(@"(?<remote_addr>\S+) - (?<remote_user>\S+) \[(?<time_local>.+)\] ""(?<request>.+)"" (?<status>\S+) (?<body_bytes_sent>\S+) ""(?<http_referer>.*)"" ""(?<http_user_agent>.*)"" ""(?<http_x_forwarded_for>.*)""( (?<request_time>\S+))?");
        foreach (var line in lines)
        {
            var match = uaRegex.Match(line);
            if (match.Success)
            {
                var ua = match.Groups["http_user_agent"].Value;
                if (!uas.Contains(ua))
                {
                    uas.Add(ua);
                }
            }
        }
        uas.Sort();
        File.WriteAllLines(uafile, uas);
    }

    private static void ParseSave(string uafile, string savefile)
    {
        var lines = File.ReadAllLines(uafile);
        var csv = new StringBuilder();
        csv.AppendLine("Browser,BrowserVersion,OS,OSVersion,DeviceType");
        var uaParser = new UAParser();

        foreach (var line in lines)
        {
            var ua = uaParser.Parse(line);
            csv.AppendLine($"{ua.Browser},{ua.BrowserVersion},{ua.OS},{ua.OSVersion},{ua.DeviceType}");
        }
        File.WriteAllText(savefile, csv.ToString());
    }
}