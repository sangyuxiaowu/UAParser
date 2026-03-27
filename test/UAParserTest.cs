namespace Sang.UAParser.Test
{
    public class UAParserTest
    {
        [Theory]
        [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36 Edg/125.0.0.0",
        "Edge","125.0.0.0","Windows","10","Desktop")]
        [InlineData("Mozilla/5.0 (Linux; Android 11; Pixel 5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.4467.141 Mobile Safari/537.36",
        "Chrome","80.0.4467.141","Android","11","Mobile")]
        [InlineData("Mozilla/5.0 (Phone; OpenHarmony 5.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36 ArkWeb/4.1.6.1 Mobile",
        "Chrome","114.0.0.0","HarmonyOS","5.0","Mobile")]
        [InlineData("Mozilla/5.0 (Linux; Android 12; HarmonyOS; NAM-AL00; HMSCore 6.14.0.302) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.5735.196 HuaweiBrowser/15.0.7.301 Mobile Safari/537.36",
        "HuaweiBrowser","15.0.7.301","Android","12","Mobile")]
        [InlineData("Mozilla/3.0 (compatible; NetPositive/2.1.1; BeOS)",
        "NetPositive","2.1.1","Other","","Bot")]
        // 添加更多的测试数据
        public void TestMyMethod(string useragent, string browser, string browserVersion, string os, string osVersion, string deviceType)
        {
            var ua = new UAParser().Parse(useragent);
            Assert.Equal(browser, ua.Browser);
            Assert.Equal(browserVersion, ua.BrowserVersion);
            Assert.Equal(os, ua.OS);
            Assert.Equal(osVersion, ua.OSVersion);
            Assert.Equal(deviceType, ua.DeviceType);
        }

        [Fact]
        public void SetParseSpider_ShouldOnlyAffectCurrentInstance()
        {
            const string userAgent = "Googlebot/2.1";

            var parserWithoutSpider = new UAParser().SetParseSpider(false);
            var defaultParser = new UAParser();

            var withoutSpider = parserWithoutSpider.Parse(userAgent);
            var withSpider = defaultParser.Parse(userAgent);

            Assert.Equal("Other", withoutSpider.Browser);
            Assert.Equal("Other", withoutSpider.DeviceType);
            Assert.Equal("Googlebot", withSpider.Browser);
            Assert.Equal("Spider", withSpider.DeviceType);
        }

        [Fact]
        public void SetBrowserIdentifiers_ShouldEscapeRegexMetaCharacters()
        {
            var parser = new UAParser().SetBrowserIdentifiers(["My.Browser"]);

            var result = parser.Parse("Mozilla/5.0 My.Browser/1.2.3");

            Assert.Equal("My.Browser", result.Browser);
            Assert.Equal("1.2.3", result.BrowserVersion);
        }

        [Fact]
        public void ShouldPreferHuaweiBrowserOverChrome()
        {
            const string userAgent = "Mozilla/5.0 (Linux; Android 12; HarmonyOS; NAM-AL00; HMSCore 6.14.0.302) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.5735.196 HuaweiBrowser/15.0.7.301 Mobile Safari/537.36";

            var result = new UAParser().Parse(userAgent);

            Assert.Equal("HuaweiBrowser", result.Browser);
            Assert.Equal("15.0.7.301", result.BrowserVersion);
        }
    }
}