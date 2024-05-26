namespace Sang.UAParser
{
    public record ClientInfo{
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
    }
}