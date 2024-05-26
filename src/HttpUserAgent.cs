using System.Text.RegularExpressions;

namespace Sang.UAParser
{

    /// <summary>
    /// HTTP User Agent 解析类
    /// </summary>
    public record class HttpUserAgent
    {

        /// <summary>
        /// 爬虫正则表达式模式
        /// </summary>
        /// <remarks>
        /// 将 "bot" 或 "spider" ， "-" 和其后面的任何字符，版本号，三个部分分别提取出来
        /// </remarks>
        //private static Regex spiderRegex = new Regex(@"((?:bot|spider))(?:-(.*))?[/\s]?(\d+(\.\d+)*)?");
        private static Regex spiderRegex = new Regex(@"([a-zA-Z-]*?(?:bot|spider)[a-zA-Z0-9-]*)(?:\/(\d+(\.\d+)*))?", RegexOptions.IgnoreCase);


        /// <summary>
        /// 程序爬虫正则表达式模式
        /// </summary>
        /// <remarks>
        /// 按照 名称/版本号 的格式匹配一次
        private static Regex normalSpiderRegex = new Regex(@"(?:([a-zA-Z-]+)[/\s]?(\d+(\.\d+)*))", RegexOptions.Compiled);

        /// <summary>
        /// 浏览器标识符
        /// </summary>
        /// <remarks>
        /// 顺序很重要，因为有些浏览器标识符可能包含在其他标识符中
        /// UC 浏览器，Vivo 浏览器，小米浏览器，夸克浏览器，联想浏览器，遨游浏览器，115 浏览器，极速浏览器，华为浏览，世界之窗浏览器
        /// 360企业安全浏览器，奇安信可信浏览器，UOS 专业版，UOS
        /// 微信内置，QQ 浏览器，搜狗浏览器，Opera浏览器，Edge 浏览器，火狐浏览器，谷歌浏览器，Safari 浏览器，IE 浏览器
        /// </remarks>
        private static List<string> browserIdentifiers = new List<string>
        {
           "UCBrowser","VivoBrowser","MiuiBrowser","QuarkPC","SLBrowser","Maxthon","115Browser","JiSu","HBPC","TheWorld",
           "QIHU 360ENT","Qaxbrowser","UOS Professional","UOS",
           "MicroMessenger", "QQBrowser", "MetaSr", "OPR","Opera", "Edg", "Firefox", "Chrome" ,"Safari", "MSIE"
        };

        /// <summary>
        /// 浏览器正则表达式模式
        /// </summary>
        /// <remarks>
        /// 从右向左匹配，匹配到第一个即可，部分浏览器无版本号
        /// </remarks>
        private static Regex browserRegex = new Regex($@"(?:({string.Join("|", browserIdentifiers)})[/\s]?(\d+(\.\d+)*)?)",RegexOptions.RightToLeft);


        /// <summary>
        /// 操作系统标识符
        /// </summary>
        /// <remarks>
        /// Windows NT, iPhone OS, Mac OS X, Android
        /// Linux 为一大类，一般不包含版本号，单独处理
        /// </remarks>
        private static List<string> osIdentifiers = new List<string>
        {
            "Windows NT","iPhone OS", "Mac OS X", "Android"
        };

        private static Regex osRegex = new Regex($@"(?:({string.Join("|", osIdentifiers)})[/\s]?(\d+([._]\d+)*))",RegexOptions.Compiled);

        /// <summary>
        /// 浏览器
        /// </summary>
        public string Browser { get; set; } = "Other";

        /// <summary>
        /// 浏览器版本
        /// </summary>
        public string BrowserVersion { get; set; } = string.Empty;

        /// <summary>
        /// 操作系统
        /// </summary>
        public string OS { get; set; } = "Other";
        /// <summary>
        /// 操作系统版本
        /// </summary>
        public string OSVersion { get; set; } = string.Empty;

        /// <summary>
        /// 设备类型
        /// Desktop, Mobile, Spider, Bot, Other
        /// </summary>
        public string DeviceType { get; set; } = "Other";

        /// <summary>
        /// 原始 User Agent
        /// </summary>
        public string UserAgent { get; set; } = string.Empty;

        /// <summary>
        /// 创建一个 HttpUserAgent 实例
        /// Sang.UAParser
        /// </summary>
        /// <param name="httpUserAgent">UserAgent 字符串</param>
        public HttpUserAgent(string httpUserAgent = "")
        {
            if (!string.IsNullOrWhiteSpace(httpUserAgent))
            {
                Parse(httpUserAgent);
            }
        }

        /// <summary>
        /// 解析 UserAgent
        /// </summary>
        /// <param name="httpUserAgent">UserAgent 字符串</param>
        public void Parse(string httpUserAgent = "")
        {
            Browser = "Other";
            BrowserVersion = string.Empty;
            OS = "Other";
            OSVersion = string.Empty;
            DeviceType = "Other";
            UserAgent = httpUserAgent;

            if (string.IsNullOrWhiteSpace(httpUserAgent))
            {
                return;
            } 

            // 优先判断是否是爬虫，爬虫也会加入普通浏览器标识符，所以要先判断
            var spiderMatch = spiderRegex.Match(UserAgent);
            if (spiderMatch.Success)
            {
                Browser = spiderMatch.Groups[1].Value;
                BrowserVersion = spiderMatch.Groups[2].Value;
                DeviceType = "Spider";
            }

            // 不是爬虫，解析浏览器
            if(Browser == "Other"){
                var browserMatch = browserRegex.Match(UserAgent);
                if (browserMatch.Success)
                {
                    Browser = browserMatch.Groups[1].Value;
                    BrowserVersion = browserMatch.Groups[2].Value;
                }
            }

            // 对于未知浏览器， UserAgent 字符串在 50 以内 或者带着爬虫协议的 ，按照爬虫匹配一次
            if(Browser == "Other" && (UserAgent.Length < 51 || UserAgent.Contains("+http") ) )
            {
                var normalSpiderMatch = normalSpiderRegex.Match(UserAgent);
                if (normalSpiderMatch.Success)
                {
                    Browser = normalSpiderMatch.Groups[1].Value;
                    BrowserVersion = normalSpiderMatch.Groups[2].Value;
                    DeviceType = "Bot";
                }
            }
            
            // 解析操作系统
            var osMatch = osRegex.Match(UserAgent);
            if (osMatch.Success)
            {
                OS = osMatch.Groups[1].Value;
                OSVersion = osMatch.Groups[2].Value;
            }

            if (OS == "Other" && (UserAgent.Contains("Linux") || UserAgent.Contains("X11;")) )
            {
                OS = "Linux";
            }


            // 判断设备类型
            if(DeviceType == "Other")
            {
                if(OS == "Android" || OS == "iPhone OS" || UserAgent.Contains("Mobile"))
                {
                    DeviceType = "Mobile";
                }else if(OS == "Windows NT" || OS == "Mac OS X" || OS == "Linux")
                {
                    DeviceType = "Desktop";
                }
            }

            // 特殊处理
            if(OS=="Windows NT")
            {
                OS = "Windows";
                // 特殊处理 Windows NT 版本号
                if(OSVersion.StartsWith("10.0"))
                {
                    OSVersion = "10";
                }
                else if(OSVersion.StartsWith("6.3"))
                {
                    OSVersion = "8.1";
                }
                else if(OSVersion.StartsWith("6.2"))
                {
                    OSVersion = "8";
                }
                else if(OSVersion.StartsWith("6.1"))
                {
                    OSVersion = "7";
                }
                else if(OSVersion.StartsWith("6.0"))
                {
                    OSVersion = "Vista";
                }
                else if(OSVersion.StartsWith("5.1"))
                {
                    OSVersion = "XP";
                }
            }else if(OS == "Mac OS X"){
                OS = "macOS";
                OSVersion = OSVersion.Replace("_",".");
            }else if(OS == "iPhone OS"){
                OS = "iOS";
                OSVersion = OSVersion.Replace("_",".");
            }

            // 特殊处理浏览器
            if (Browser == "OPR")
            {
                Browser = "Opera";
            }
            else if (Browser == "Edg")
            {
                Browser = "Edge";
            }
            else if (Browser == "MetaSr")
            {
                Browser = "Sogou";
            }
            else if (Browser == "MSIE")
            {
                Browser = "IE";
            }
        }


        /// <summary>
        /// 设置浏览器 Identifiers
        /// </summary>
        /// <param name="identifiers">Identifiers</param>
        /// <returns>HttpUserAgent</returns>
        public HttpUserAgent SetBrowserIdentifiers(List<string> identifiers)
        {
            browserIdentifiers = identifiers;
            browserRegex = new Regex($@"(?:({string.Join("|", browserIdentifiers)})[/\s]?(\d+(\.\d+)*)?)",RegexOptions.RightToLeft);
            return this;
        }

        /// <summary>
        /// 设置操作系统 Identifiers
        /// </summary>
        /// <param name="identifiers">Identifiers</param>
        /// <returns>HttpUserAgent</returns>
        public HttpUserAgent SetOSIdentifiers(List<string> identifiers)
        {
            osIdentifiers = identifiers;
            osRegex = new Regex($@"(?:({string.Join("|", osIdentifiers)})[/\s]?(\d+([._]\d+)*))",RegexOptions.Compiled);
            return this;
        }

    }
}