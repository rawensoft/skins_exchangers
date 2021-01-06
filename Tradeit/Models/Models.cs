using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SkinsExchangers.TradeitGG.Models
{
    public class SessionData
    {
        public string __cfduid { get; set; }
        public string sessionid { get; set; }
        private SessionData() { }
        [JsonConstructor]
        public SessionData(string __cfduid, string sessionid)
        {
            this.__cfduid = __cfduid;
            this.sessionid = sessionid;
        }
        public static async Task<SessionData> AuthAsync(SteamAuth.SessionData Session, IWebProxy Proxy)
        {
            if (Session == null) return null;
            string url = "https://steamcommunity.com/openid/login?openid.mode=checkid_setup&openid.ns=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0&openid.ns.sreg=http%3A%2F%2Fopenid.net%2Fextensions%2Fsreg%2F1.1&openid.sreg.optional=nickname%2Cemail%2Cfullname%2Cdob%2Cgender%2Cpostcode%2Ccountry%2Clanguage%2Ctimezone&openid.ns.ax=http%3A%2F%2Fopenid.net%2Fsrv%2Fax%2F1.0&openid.ax.mode=fetch_request&openid.ax.type.fullname=http%3A%2F%2Faxschema.org%2FnamePerson&openid.ax.type.firstname=http%3A%2F%2Faxschema.org%2FnamePerson%2Ffirst&openid.ax.type.lastname=http%3A%2F%2Faxschema.org%2FnamePerson%2Flast&openid.ax.type.email=http%3A%2F%2Faxschema.org%2Fcontact%2Femail&openid.ax.required=fullname%2Cfirstname%2Clastname%2Cemail&openid.identity=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0%2Fidentifier_select&openid.claimed_id=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0%2Fidentifier_select&openid.return_to=https%3A%2F%2Fsteamtrade.gg%2Fauth%2Fsteam%2Freturn&openid.realm=https%3A%2F%2Fsteamtrade.gg%2F";
            BrowserEmulation browser = new();
            browser.add_steam_cookies(Session);
            browser.add_steam_cookies_1(Session);
            browser.webClient.Proxy = Proxy;
            HtmlParser htmlParser = new HtmlParser();
            string html;
            try
            {
                html = await Task.Run(delegate { return browser.getRequest(url); });
            }
            catch (Exception)
            {
                return null;
            }

            IHtmlDocument htmlDocument = htmlParser.ParseDocument(html);
            string nonce = htmlDocument.QuerySelector("input[name='nonce']").GetAttribute("value");
            string openidparams = htmlDocument.QuerySelector("input[name='openidparams']").GetAttribute("value");
            string param_nickname = "action=steam_openid_login&openid.mode=checkid_setup&openidparams=" + openidparams + "&nonce=" + nonce;
            try
            {
                await Task.Run(delegate { browser.postRequest("https://steamcommunity.com/openid/login", param_nickname); });
            }
            catch (Exception)
            {
                return null;
            }

            var cookie = browser.webClient.CookieContainer.GetCookies(new Uri("https://tradeit.gg/"));
            var ses = new SessionData();
            foreach (Cookie item in cookie)
            {
                if (item.Name == "__cfduid") ses.__cfduid = item.Value;
                else if (item.Name == "sessionid") ses.sessionid = item.Value;
            }
            return ses;
        }
        public string ToStringCookie()
        {
            if(string.IsNullOrEmpty(sessionid) && !string.IsNullOrEmpty(__cfduid)) return $"__cfduid={__cfduid}; ";
            return $"__cfduid={__cfduid}; sessionid={sessionid}; ";
        }
    }

    public record TradeOffer
    {
        public bool Success { get; private set; } = false;
        public ulong SteamID64 { get; private set; } = 0;
        public ulong SteamID { get; private set; } = 0;
        public int Token { get; private set; } = 0;
        [JsonConstructor]
        public TradeOffer(bool Success = false, string SteamID64 = "0", string SteamID = "0", int Token = 0)
        {
            this.Success = Success;
            this.Token = Token;
            this.SteamID64 = ulong.Parse(SteamID64);
            this.SteamID = ulong.Parse(SteamID);
        }
    }
    public record BuyItem
    {
        public bool success { get; set; } = false;
        public string error { get; set; } = "None";
        internal BuyItem() { }
        [JsonConstructor]
        public BuyItem(bool success, string error)
        {
            this.success = success;
            this.error = error;
        }
    }
    public record WithdrawItem
    {
        public bool success { get; set; } = false;
        public string error { get; set; } = "None";
        public string token { get; set; } = "";
        internal WithdrawItem() { }
        [JsonConstructor]
        public WithdrawItem(bool success, string error, string token)
        {
            this.success = success;
            this.error = error;
            this.token = token;
        }
    }
    internal record TradeItemSale
    {
        public string token;
        public int price;
        [JsonConstructor]
        public TradeItemSale(string token, int price)
        {
            this.token = token;
            this.price = price;
        }
        internal TradeItemSale() { }
    }
}
