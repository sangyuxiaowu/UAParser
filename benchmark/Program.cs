using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Sang.UAParser;

BenchmarkRunner.Run<UAParserBenchmarks>();

[MemoryDiagnoser]
public class UAParserBenchmarks
{
    private static readonly string ChromeUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36 Edg/125.0.0.0";
    private static readonly string OpenHarmonyUserAgent = "Mozilla/5.0 (Phone; OpenHarmony 5.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36 ArkWeb/4.1.6.1 Mobile";
    private static readonly string SpiderUserAgent = "Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; GPTBot/1.0; +https://openai.com/gptbot)";

    private readonly UAParser parser = new();
    private string[] userAgents = [];

    [GlobalSetup]
    public void Setup()
    {
        var dataPath = Path.Combine(AppContext.BaseDirectory, "data", "uatest.txt");
        userAgents = File.ReadAllLines(dataPath)
            .Where(static line => !string.IsNullOrWhiteSpace(line))
            .ToArray();
    }

    [Benchmark]
    public ClientInfo ParseChrome()
    {
        return parser.Parse(ChromeUserAgent);
    }

    [Benchmark]
    public ClientInfo ParseOpenHarmony()
    {
        return parser.Parse(OpenHarmonyUserAgent);
    }

    [Benchmark]
    public ClientInfo ParseSpider()
    {
        return parser.Parse(SpiderUserAgent);
    }

    [Benchmark]
    public int ParseWholeDataset()
    {
        var count = 0;
        foreach (var userAgent in userAgents)
        {
            _ = parser.Parse(userAgent);
            count++;
        }

        return count;
    }
}
