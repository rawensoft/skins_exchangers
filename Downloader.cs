using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using SteamAuth;
using System.IO;

namespace SkinsExchangers.Web
{
    public static class Downloader
    {
        public static string UserAgentMobile = "Mozilla/5.0 (Linux; U; Android 4.1.1; en-us; Google Nexus 4 - 4.1.1 - API 16 - 768x1280 Build/JRO03S) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30";
        public static string UserAgentSteam = "Mozilla/5.0 (Windows; U; Windows NT 10.0; en-US; Valve Steam Client/default/1607131459; ) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.117 Safari/537.36";
        public static string UserAgentChrome = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36";
        public static async Task<(bool, string, string)> PostTradeitAsync(string url, string content, string content_type, TradeitGG.Models.SessionData session, IWebProxy proxy = null, string referer = null, string UserAgent = null)
        {
            if (UserAgent == null) UserAgent = UserAgentChrome;
            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.Brotli;
            request.Method = "POST";
            request.Accept = "*/*";
            if (referer != null) request.Referer = referer;
            request.UserAgent = UserAgent;
            request.Headers.Add("accept-language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            request.Headers.Add("cache-control", "no-cache");
            request.Headers.Add("cookie", session.ToStringCookie() + "emailPopupShowed=true; playercache=");
            request.Headers.Add("dnt", "1");
            request.Headers.Add("origin", "https://tradeit.gg");
            request.Headers.Add("pragma", "no-cache");
            request.Headers.Add("sec-fetch-dest", "empty");
            request.Headers.Add("sec-fetch-mode", "cors");
            request.Headers.Add("sec-fetch-site", "same-origin");
            request.Headers.Add("x-requested-with", "XMLHttpRequest");
            request.Proxy = proxy;

            request.ContentType = content_type;
            request.ContentLength = content.Length;
            StreamWriter requestStream = new StreamWriter(await request.GetRequestStreamAsync());
            requestStream.Write(content);
            requestStream.Close();

            try
            {
                var response = await request.GetResponseAsync();
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    string data = await sr.ReadToEndAsync();
                    string cookie = null;
                    string[] cookies = response.Headers.GetValues("Set-Cookie");
                    if (cookies != null && cookies.Count() > 0)
                    {
                        cookie = "";
                        foreach (var item in cookies) cookie += item + "; ";
                        return (true, data, cookie);
                    }
                    return (true, data, null);
                }
            }
            catch (Exception)
            {
                return (false, null, null);
            }
        }
        public static async Task<(bool, string, string)> GetTradeitAsync(string url, TradeitGG.Models.SessionData session, IWebProxy proxy = null, string referer = null, string UserAgent = null)
        {
            if (UserAgent == null) UserAgent = UserAgentChrome;
            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.Brotli;
            request.Method = "GET";
            request.Accept = "*/*";
            if (referer != null) request.Referer = referer;
            request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("Cookie", session.ToStringCookie());
            request.Headers.Add("DNT", "1");
            request.Headers.Add("Origin", "https://tradeit.gg");
            request.Headers.Add("Pragma", "no-cache");
            request.Headers.Add("Sec-Fetch-Dest", "empty");
            request.Headers.Add("Sec-Fetch-Mode", "cors");
            request.Headers.Add("Sec-Fetch-Site", "same-site");
            request.UserAgent = UserAgent;
            request.Proxy = proxy;

            try
            {
                var response = await request.GetResponseAsync();
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    string data = await sr.ReadToEndAsync();
                    string cookie = null;
                    string[] cookies = response.Headers.GetValues("Set-Cookie");
                    if (cookies != null && cookies.Count() > 0)
                    {
                        cookie = "";
                        foreach (var item in cookies) cookie += item + "; ";
                        return (true, data, cookie);
                    }
                    return (true, data, null);
                }
            }
            catch (Exception)
            {
                return (false, null, null);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns>Host, Origin</returns>
        private static (string, string) GetHostOrigin(string url)
        {
            if (url.StartsWith("https://steamcommunity.com")) return ("steamcommunity.com", "https://steamcommunity.com");
            if (url.StartsWith("https://steampowered.com")) return ("steampowered.com", "https://steampowered.com");
            if (url.StartsWith("https://store.steampowered.com")) return ("store.steampowered.com", "https://store.steampowered.com");
            return (null, null);
        }
    }
}
