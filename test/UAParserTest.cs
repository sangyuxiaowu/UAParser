namespace Sang.UAParser.Test
{
    public class UAParserTest
    {
        [Theory]
        [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36 Edg/125.0.0.0",
        "Edge","125.0.0.0","Windows","10","Desktop")]
        [InlineData("Mozilla/5.0 (Linux; Android 11; Pixel 5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.4467.141 Mobile Safari/537.36",
        "Safari","537.36","Android","11","Mobile")]
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
    }
}