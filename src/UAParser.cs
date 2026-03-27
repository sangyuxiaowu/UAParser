using System.Text.RegularExpressions;

namespace Sang.UAParser
{

    /// <summary>
    /// HTTP User Agent 解析类
    /// </summary>
    public class UAParser
    {
        /// <summary>
        /// 浏览器标识符
        /// </summary>
        /// <remarks>
        /// 顺序很重要，因为有些浏览器标识符可能包含在其他标识符中
        /// UC 浏览器，Vivo 浏览器，小米浏览器，夸克浏览器，联想浏览器，遨游浏览器，115 浏览器，极速浏览器，华为浏览，世界之窗浏览器
        /// 360浏览器[x3]，用友浏览器，奇安信可信浏览器，UOS 专业版，UOS
        /// 微信内置，QQ 浏览器，QQ，搜狗浏览器，Opera浏览器，Edge 浏览器，火狐浏览器，谷歌浏览器，Safari 浏览器，IE 浏览器
        /// </remarks>
        private static readonly string[] DefaultBrowserIdentifiers =
        {
            "UCBrowser", "VivoBrowser", "MiuiBrowser", "QuarkPC", "SLBrowser", "Maxthon", "115Browser", "JiSu", "HBPC", "TheWorld",
            "QIHU 360ENT", "QIHU 360SE", "QIHU 360EE", "UBrowser", "Qaxbrowser", "HuaweiBrowser", "UOS Professional", "UOS",
            "MicroMessenger", "QQBrowser", "QQ", "MetaSr", "OPR", "Opera", "Edg", "Firefox", "Chrome", "Safari", "MSIE"
        };

        /// <summary>
        /// 操作系统标识符
        /// </summary>
        /// <remarks>
        /// Windows NT, iPhone OS, Mac OS X, Android, OpenHarmony
        /// Linux 为一大类，一般不包含版本号，单独处理
        /// </remarks>
        private static readonly string[] DefaultOsIdentifiers =
        {
            "Windows NT", "iPhone OS", "Mac OS X", "Android", "OpenHarmony"
        };

        /// <summary>
        /// 默认浏览器正则表达式模式
        /// </summary>
        /// <remarks>
        /// 从右向左匹配，匹配到第一个即可，部分浏览器无版本号
        /// </remarks>
        private static readonly Regex DefaultBrowserRegex = CreateBrowserRegex(DefaultBrowserIdentifiers);

        /// <summary>
        /// 默认操作系统正则表达式模式
        /// </summary>
        /// <remarks>
        /// Windows NT, iPhone OS, Mac OS X, Android, OpenHarmony
        /// Linux 为一大类，一般不包含版本号，单独处理
        /// </remarks>
        private static readonly Regex DefaultOsRegex = CreateOsRegex(DefaultOsIdentifiers);

        /// <summary>
        /// 爬虫正则表达式模式
        /// </summary>
        /// <remarks>
        /// 将 bot 或 spider、版本号分别提取出来
        /// </remarks>
        private static readonly Regex SpiderRegex = new Regex(@"\b([a-zA-Z-\s]*?(?:bot|spider)[a-zA-Z0-9-]*)(?:\/(\d+(\.\d+)*))?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        /// <summary>
        /// 程序爬虫正则表达式模式
        /// </summary>
        /// <remarks>
        /// 按照 名称(可含数字，但不能纯数字)/版本号(可带 v 前缀) 的格式匹配一次
        /// </remarks>
        private static readonly Regex NormalBotRegex = new Regex(@"(?:([a-zA-Z0-9-]*[a-zA-Z][a-zA-Z0-9-]*)[/\s](v?\d+(\.\d+)*))", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// 程序爬虫正则表达式模式2
        /// </summary>
        /// <remarks>
        /// 按照 名称 的格式匹配一次，要求完整匹配
        /// </remarks>
        private static readonly Regex BotRegex = new Regex(@"^[a-zA-Z][a-zA-Z\s-]{0,28}[a-zA-Z]$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// Chrome 浏览器正则表达式模式
        /// 用于修正 Safari
        /// </summary>
        private static readonly Regex ChromeRegex = new Regex(@"(?:(Chrome)[/\s]?(\d+(\.\d+)*)?)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// HuaweiBrowser 浏览器正则表达式模式
        /// 用于修正 Safari
        /// </summary>
        private static readonly Regex HuaweiBrowserRegex = new Regex(@"(?:(HuaweiBrowser)[/\s]?(\d+(\.\d+)*)?)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// 是否解析爬虫
        /// </summary>
        private bool parseSpider = true;

        /// <summary>
        /// 当前实例使用的浏览器正则表达式模式
        /// </summary>
        private Regex browserRegex = DefaultBrowserRegex;

        /// <summary>
        /// 当前实例使用的操作系统正则表达式模式
        /// </summary>
        private Regex osRegex = DefaultOsRegex;

        /// <summary>
        /// 创建一个 UAParser 实例
        /// Sang.UAParser
        /// </summary>
        public UAParser()
        {

        }

        /// <summary>
        /// 解析 UserAgent
        /// </summary>
        /// <param name="httpUserAgent">UserAgent 字符串</param>
        public ClientInfo Parse(string httpUserAgent = "")
        {
            var clientInfo = new ClientInfo(){
                UserAgent = httpUserAgent
            };

            if (string.IsNullOrWhiteSpace(httpUserAgent))
            {
                return clientInfo;
            } 

            // 优先判断是否是爬虫，爬虫也会加入普通浏览器标识符，所以要先判断
            if(parseSpider)
            {
                var spiderMatch = SpiderRegex.Match(clientInfo.UserAgent);
                if (spiderMatch.Success)
                {
                    clientInfo.Browser = spiderMatch.Groups[1].Value.Trim();
                    clientInfo.BrowserVersion = spiderMatch.Groups[2].Value;
                    clientInfo.DeviceType = "Spider";
                }
            }
            

            // 不是爬虫，解析浏览器
            if(clientInfo.Browser == "Other"){
                var browserMatch = browserRegex.Match(clientInfo.UserAgent);
                if (browserMatch.Success)
                {
                    clientInfo.Browser = browserMatch.Groups[1].Value;
                    clientInfo.BrowserVersion = browserMatch.Groups[2].Value;
                }
            }

            // 对于未知浏览器， UserAgent 字符串在 61 以内 或者带着爬虫协议的 ，按照爬虫匹配一次
            if(parseSpider && clientInfo.Browser == "Other" && (clientInfo.UserAgent.Length < 61 || clientInfo.UserAgent.Contains("http") ) )
            {
                // 修正 UserAgent 匹配，排除 Mozilla 干扰
                var tmpAgent = clientInfo.UserAgent.StartsWith("Mozilla", StringComparison.Ordinal) ? clientInfo.UserAgent[8..] : clientInfo.UserAgent;
                var normalBotMatch = NormalBotRegex.Match(tmpAgent);
                if (normalBotMatch.Success)
                {
                    clientInfo.Browser = normalBotMatch.Groups[1].Value;
                    clientInfo.BrowserVersion = normalBotMatch.Groups[2].Value;
                    clientInfo.DeviceType = "Bot";
                }else{
                    // 有些爬虫没有版本号，再匹配一次，完整匹配
                    var botMatch = BotRegex.Match(clientInfo.UserAgent);
                    if(botMatch.Success)
                    {
                        clientInfo.Browser = clientInfo.UserAgent;
                        clientInfo.DeviceType = "Bot";
                    }
                }
            }
            
            // 解析操作系统
            var osMatch = osRegex.Match(clientInfo.UserAgent);
            if (osMatch.Success)
            {
                clientInfo.OS = osMatch.Groups[1].Value;
                clientInfo.OSVersion = osMatch.Groups[2].Value;
            }

            if (clientInfo.OS == "Other" && (clientInfo.UserAgent.Contains("Linux", StringComparison.Ordinal) || clientInfo.UserAgent.Contains("X11;", StringComparison.Ordinal)) )
            {
                clientInfo.OS = "Linux";
            }


            // 判断设备类型
            if(clientInfo.DeviceType == "Other")
            {
                if(clientInfo.OS == "Android" || clientInfo.OS == "iPhone OS" || clientInfo.UserAgent.Contains("Mobile"))
                {
                    clientInfo.DeviceType = "Mobile";
                }else if(clientInfo.OS == "Windows NT" || clientInfo.OS == "Mac OS X" || clientInfo.OS == "Linux")
                {
                    clientInfo.DeviceType = "Desktop";
                }
            }

            // 特殊处理
            if(clientInfo.OS=="Windows NT")
            {
                clientInfo.OS = "Windows";
                // 特殊处理 Windows NT 版本号
                if(clientInfo.OSVersion.StartsWith("10.0"))
                {
                    clientInfo.OSVersion = "10";
                }
                else if(clientInfo.OSVersion.StartsWith("6.3"))
                {
                    clientInfo.OSVersion = "8.1";
                }
                else if(clientInfo.OSVersion.StartsWith("6.2"))
                {
                    clientInfo.OSVersion = "8";
                }
                else if(clientInfo.OSVersion.StartsWith("6.1"))
                {
                    clientInfo.OSVersion = "7";
                }
                else if(clientInfo.OSVersion.StartsWith("6.0"))
                {
                    clientInfo.OSVersion = "Vista";
                }
                else if(clientInfo.OSVersion.StartsWith("5.2") || clientInfo.OSVersion.StartsWith("5.1"))
                {
                    clientInfo.OSVersion = "XP";
                }else if(clientInfo.OSVersion.StartsWith("5.0")){
                    clientInfo.OSVersion = "2000";
                }else{
                    clientInfo.OSVersion = "Unknown";
                }
            }else if(clientInfo.OS == "Mac OS X"){
                clientInfo.OS = "macOS";
                clientInfo.OSVersion = clientInfo.OSVersion.Replace("_",".");
            }else if(clientInfo.OS == "iPhone OS"){
                clientInfo.OS = "iOS";
                clientInfo.OSVersion = clientInfo.OSVersion.Replace("_",".");
            }else if(clientInfo.OS == "OpenHarmony"){
                clientInfo.OS = "HarmonyOS";
                clientInfo.OSVersion = clientInfo.OSVersion.Replace("_", ".");
            }

            // 特殊处理浏览器
            if (clientInfo.Browser == "OPR")
            {
                clientInfo.Browser = "Opera";
            }
            else if (clientInfo.Browser == "Edg")
            {
                clientInfo.Browser = "Edge";
            }
            else if (clientInfo.Browser == "MetaSr")
            {
                clientInfo.Browser = "Sogou";
            }
            else if (clientInfo.Browser == "MSIE")
            {
                clientInfo.Browser = "IE";
            }else if(clientInfo.Browser == "Safari")
            {
                var huaweiBrowserMatch = HuaweiBrowserRegex.Match(clientInfo.UserAgent);
                if(huaweiBrowserMatch.Success)
                {
                    clientInfo.Browser = "HuaweiBrowser";
                    clientInfo.BrowserVersion = huaweiBrowserMatch.Groups[2].Value;
                }
                else
                {
                    // Chrome 会被 Safari 识别，修正
                    var chromeMatch = ChromeRegex.Match(clientInfo.UserAgent);
                    if(chromeMatch.Success)
                    {
                        clientInfo.Browser = "Chrome";
                        clientInfo.BrowserVersion = chromeMatch.Groups[2].Value;
                    }
                }
            }else if(clientInfo.Browser.StartsWith("QIHU 360"))
            {
                clientInfo.BrowserVersion = clientInfo.Browser.Replace("QIHU 360","");
                clientInfo.Browser = "360";
            }else if(clientInfo.Browser == "UOS Professional")
            {
                clientInfo.Browser = "UOS";
                clientInfo.BrowserVersion = "Professional";
            }
            return clientInfo;
        }


        /// <summary>
        /// 设置浏览器 Identifiers
        /// </summary>
        /// <param name="identifiers">Identifiers</param>
        /// <returns>UAParser</returns>
        public UAParser SetBrowserIdentifiers(List<string> identifiers)
        {
            ArgumentNullException.ThrowIfNull(identifiers);
            browserRegex = CreateBrowserRegex(identifiers);
            return this;
        }

        /// <summary>
        /// 设置操作系统 Identifiers
        /// </summary>
        /// <param name="identifiers">Identifiers</param>
        /// <returns>UAParser</returns>
        public UAParser SetOSIdentifiers(List<string> identifiers)
        {
            ArgumentNullException.ThrowIfNull(identifiers);
            osRegex = CreateOsRegex(identifiers);
            return this;
        }

        /// <summary>
        /// 设置是否解析爬虫
        /// </summary>
        /// <param name="parse">是否解析爬虫</param>
        /// <returns>UAParser</returns>
        public UAParser SetParseSpider(bool parse)
        {
            parseSpider = parse;
            return this;
        }

        /// <summary>
        /// 根据浏览器标识符创建正则表达式模式
        /// </summary>
        /// <remarks>
        /// 从右向左匹配，匹配到第一个即可，部分浏览器无版本号
        /// </remarks>
        private static Regex CreateBrowserRegex(IEnumerable<string> identifiers)
        {
            var pattern = string.Join("|", identifiers.Select(Regex.Escape));
            return new Regex($@"(?:({pattern})\b[/\s]?(\d+(\.\d+)*)?)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.RightToLeft);
        }

        /// <summary>
        /// 根据操作系统标识符创建正则表达式模式
        /// </summary>
        private static Regex CreateOsRegex(IEnumerable<string> identifiers)
        {
            var pattern = string.Join("|", identifiers.Select(Regex.Escape));
            return new Regex($@"(?:({pattern})[/\s]?(\d+([._]\d+)*))", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

    }
}