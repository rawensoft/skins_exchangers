using SkinsExchangers.TradeitGG.Models;
using System;
using System.Threading.Tasks;
using SkinsExchangers.Extensions;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using SkinsExchangers.TradeitGG.Models.WebSocket;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using H.Engine.IO;
namespace SkinsExchangers.TradeitGG
{
    public class Tradeit
    {
        #region Events
        public delegate void OnTradeInfoChangedHandler(Tradeit sender, int Count, int Value);
        /// <summary>
        /// Срабатывает при изменении TradeCount или TradeValue
        /// </summary>
        public event OnTradeInfoChangedHandler OnTradeInfoChanged;
        public delegate void OnBalanceChangedHandler(Tradeit sender, int balance);
        /// <summary>
        /// Срабатывает при изменении баланса. !!!Только при действиях внутри класса, сокет ничего не даёт.
        /// </summary>
        public event OnBalanceChangedHandler OnBalanceChanged;
        public delegate void OnStatsHandler(Tradeit sender, StatsInfo Info);
        /// <summary>
        /// Вызывается при обновление статистики сайта, т.е. количество трейдов, людей онлайн...
        /// </summary>
        public event OnStatsHandler OnStats;
        public delegate void OnLastTradeHandler(Tradeit sender, LastTradeInfo Info);
        /// <summary>
        /// Вызывается при успешном трейде от всех людей
        /// </summary>
        public event OnLastTradeHandler OnLastTrade;
        public delegate void OnTradeCanceledHandler(Tradeit sender, TradeInfo Info);
        /// <summary>
        /// Вызывается при отменёном трейд от всех людей
        /// </summary>
        public event OnTradeCanceledHandler OnTradeCanceled;
        public delegate void OnTradeStateHandler(Tradeit sender, TradeStateInfo Info);
        /// <summary>
        /// Вызывается при изменении состояния трейда текущего аккаунта
        /// </summary>
        public event OnTradeStateHandler OnTradeState;
        public delegate void OnWebSocketClosedHandler(Tradeit sender);
        /// <summary>
        /// Вызывается при закрытии соединения Веб-Сокета
        /// </summary>
        public event OnWebSocketClosedHandler OnWebSocketClosed;
        #endregion

        public IWebProxy Proxy { get; set; }
        private List<TradeItemSale> _mTradeItems = new();
        public SessionData Session { get; private set; }

        #region Profile Info
        public string TradeURL { get; set; }
        public bool LoggedIn { get; private set; } = false;
        public bool IsNewUser { get; private set; } = false;
        /// <summary>
        /// Для получения cachedinv
        /// </summary>
        public string socketid { get; private set; }
        /// <summary>
        /// SteamID64
        /// </summary>
        public string playerid { get; private set; }
        public bool PayerEnabled { get; private set; } = false;
        public string Username { get; private set; }
        private int _Balance = 0;
        public int Balance
        {
            get { return _Balance; }
            private set
            {
                _Balance = value;
                OnBalanceChanged?.Invoke(this, value);
            }
        }
        public int Accepted_TOS { get; private set; } = 0;
        private int _TradeCount = 0;
        /// <summary>
        /// Количество принятых трейдов
        /// </summary>
        public int TradeCount
        {
            get { return _TradeCount; }
            private set
            {
                _TradeCount = value;
                OnTradeInfoChanged?.Invoke(this, value, TradeValue);
            }
        }
        private int _TradeValue = 0;
        /// <summary>
        /// Количество отправленных трейдов
        /// </summary>
        public int TradeValue
        {
            get { return _TradeValue; }
            private set
            {
                _TradeValue = value;
                OnTradeInfoChanged?.Invoke(this, TradeCount, value);
            }
        }
        /// <summary>
        /// Время регистрации в мс
        /// </summary>
        public ulong Register_TimeStamp { get; private set; } = 0;
        public bool Bonus { get; private set; } = false;
        private string Json_URL;
        #endregion

        #region WebSocket
        private EngineIoClient engineIO;
        private DateTime _WS_Started = DateTime.UtcNow;
        private DateTime _WS_Closed = DateTime.UtcNow;
        /// <summary>
        /// Сколько прошло с момента открытия соединения WebSocket
        /// </summary>
        public TimeSpan WS_Stared
        {
            get { return DateTime.UtcNow - _WS_Started; }
        }
        /// <summary>
        /// Сколько работает WebSocket (если соединение закрыто, то считается время закрытия)
        /// </summary>
        public TimeSpan WS_Worked
        {
            get 
            { 
                if(IsWSAlive) return DateTime.UtcNow - _WS_Started;
                return _WS_Closed - _WS_Started;
            }
        }
        private Timer timer;
        public bool IsWSAlive
        {
            get
            {
                if (engineIO != null && engineIO.WebSocketClient.Socket.State == System.Net.WebSockets.WebSocketState.Open) return true;
                return false;
            }
        }
        #endregion

        private Tradeit() { }
        public static async Task<Tradeit> Load(SessionData Session, IWebProxy Proxy)
        {
            if (Session == null) return null;
            var tradeit = new Tradeit();
            tradeit = await UpdateInfo(Session, tradeit, Proxy);
            tradeit.Session = Session;
            tradeit.Proxy = Proxy;
            return tradeit;
        }

        /// <summary>
        /// Открывает новое соединение с WS. Если текущее соединение активно, то закрывает его и открывает новое.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartWebSocket()
        {
            if (IsWSAlive)
            {
                engineIO.Closed -= EngineIO_Closed;
                engineIO.WebSocketClient.TextReceived -= WebSocketClient_TextReceived;
                engineIO.WebSocketClient.Connected -= WebSocketClient_Connected;
                await engineIO.CloseAsync();
                engineIO.Dispose();
            }
            var (success, html, cookie) = await Web.Downloader.GetTradeitAsync($"https://socket.tradeit.gg/socket.io/?EIO=3&transport=polling", Session, Proxy, "https://tradeit.gg/");
            for (; ; )
            {
                if (char.IsDigit(html[0]) || html[0] == ':') html = html.Remove(0, 1);
                else if (char.IsDigit(html[html.Length - 1]) || html[html.Length - 1] == ':') html = html.Remove(html.Length - 1, 1);
                else break;
            } //обрезаем всё лишнее
            var info = JsonSerializer.Deserialize<SocketInfo>(html);
            var (success_get, data, _) = await Web.Downloader.GetTradeitAsync($"https://socket.tradeit.gg/socket.io/?EIO=3&transport=polling&sid={info.sid}", Session, Proxy, "https://tradeit.gg/");
            
            engineIO = null;
            engineIO = new EngineIoClient("socket.io");
            var options = engineIO.WebSocketClient.Socket.Options;

            if (System.IO.File.Exists("FiddlerRoot.cer"))
            {
                var certificate = new X509Certificate("FiddlerRoot.cer");
                options.ClientCertificates.Add(certificate);
            }
            options.SetRequestHeader("Accept-Encoding", "br");
            options.SetRequestHeader("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            options.SetRequestHeader("Cookie", $"{Session.ToStringCookie()}io={info.sid};" );
            options.SetRequestHeader("Host", "socket.tradeit.gg");
            options.SetRequestHeader("Origin", "https://tradeit.gg/");
            options.SetRequestHeader("User-Agent", Web.Downloader.UserAgentChrome);
            options.SetRequestHeader("Cache-Control", "no-cache");
            options.SetRequestHeader("Pragma", "no-cache");
            engineIO.Proxy = Proxy;
            engineIO.WebSocketClient.TextReceived += WebSocketClient_TextReceived;
            engineIO.WebSocketClient.Connected += WebSocketClient_Connected;
            engineIO.Closed += EngineIO_Closed;
            string url = $"wss://socket.tradeit.gg/?sid={info.sid}";
            engineIO.OpenAsync(new Uri(url)); // без await потому, что это в цикле и работает пока State == Open
            timer = new Timer(async delegate {
                await engineIO.WebSocketClient.SendTextAsync("2");
            }, null, TimeSpan.FromMilliseconds(info.pingInterval), TimeSpan.FromMilliseconds(info.pingInterval));
            return true;
        }
        public async Task<bool> StopWebSocket()
        {
            if (IsWSAlive)
            {
                engineIO.Closed -= EngineIO_Closed;
                engineIO.WebSocketClient.TextReceived -= WebSocketClient_TextReceived;
                engineIO.WebSocketClient.Connected -= WebSocketClient_Connected;
                await engineIO.CloseAsync();
                engineIO.Dispose();
                return true;
            }
            return false;
        }
        private void WebSocketClient_Connected(object sender, EventArgs e)
        {
            _WS_Started = DateTime.UtcNow;
            _WS_Closed = DateTime.UtcNow;
            engineIO.WebSocketClient.SendTextAsync("2probe");
        }
        private async void WebSocketClient_TextReceived(object sender, H.WebSockets.Utilities.DataEventArgs<string> e)
        {
            if (e.Value == "3probe")
            {
                await engineIO.SendMessageAsync("5");
                await engineIO.SendMessageAsync($"42[\"steamid\",\"{playerid}_{socketid}\"]");
            }
            else
            {
                string data = e.Value;
                if (data.StartsWith("42")) data = data.Remove(0, 2);
                if (data.StartsWith("[\"lastTrade") && OnLastTrade != null)
                {
                    data = data.Replace("\"lastTrade\",", "");
                    var list = JsonSerializer.Deserialize<List<LastTradeInfo>>(data);
                    OnLastTrade?.Invoke(this, list[0]);
                }
                else if (data.StartsWith("[\"stats") && OnStats != null)
                {
                    data = data.Replace("\"stats\",", "");
                    var list = JsonSerializer.Deserialize<List<StatsInfo>>(data);
                    OnStats?.Invoke(this, list[0]);
                }
                else if (data.StartsWith("[\"canceledtrade") && OnTradeCanceled != null)
                {
                    var list = JsonSerializer.Deserialize<List<string>>(data);
                    var e_info = list.Contains("canceledtrade") ? Enums.TRADE_INFO.Canceled : Enums.TRADE_INFO.None;
                    string steamid = list.Count > 1 ? list[1] : "0";
                    var info = new TradeInfo(e_info, steamid);
                    OnTradeCanceled?.Invoke(this, info);
                }
                else if (data.StartsWith("[\"tradestate") && OnTradeState != null)
                {
                    data = data.Replace("\"tradestate\",", "");
                    var list = JsonSerializer.Deserialize<List<TradeStateInfo>>(data);
                    foreach (var trade in list)
                    {
                        if(trade.E_State == Enums.TRADE_STATE.Accepted)
                        {
                            for (int i = 0; i < _mTradeItems.Count; i++)
                            {
                                var item = _mTradeItems[i];
                                if (item.token == trade.Token)
                                {
                                    Balance -= item.price;
                                    _mTradeItems.Remove(item);
                                    break;
                                }
                            }
                            TradeValue++;
                        }
                        else if(trade.E_State == Enums.TRADE_STATE.Sent)
                        {
                            TradeCount++;
                        }
                    }
                    OnTradeState?.Invoke(this, list[0]);
                }
            }
        }
        private void EngineIO_Closed(object sender, H.WebSockets.Args.WebSocketCloseEventArgs e)
        {
            _WS_Closed = DateTime.UtcNow;
            engineIO.Closed -= EngineIO_Closed;
            engineIO.WebSocketClient.TextReceived -= WebSocketClient_TextReceived;
            engineIO.WebSocketClient.Connected -= WebSocketClient_Connected;
            OnWebSocketClosed?.Invoke(this);
        }

        /// <summary>
        /// Обновление информации о профиле с сайта
        /// </summary>
        /// <returns></returns>
        public async Task<Tradeit> UpdateInfo()
        {
            return await UpdateInfo(Session, this);
        }
        private static async Task<Tradeit> UpdateInfo(SessionData Session, Tradeit tradeit, IWebProxy Proxy = null)
        {
            BrowserEmulation browser = new();
            browser.add_tradeit_cookies(Session);
            browser.webClient.Proxy = Proxy;
            string html;
            try
            {
                html = await Task.Run(delegate { return browser.getRequest("https://tradeit.gg/?welcome=back"); });
            }
            catch (Exception)
            {
                return null;
            }
            tradeit.LoggedIn = html.Contains("var loggedin = true;");
            if (!tradeit.LoggedIn) return tradeit;
            tradeit.Bonus = html.Contains("var bonus = true;");
            tradeit.IsNewUser = html.Contains("var isNewUser = `` == 'true';");
            tradeit.PayerEnabled = html.Contains("var payeer_enabled = `true`;");
            tradeit.Balance = int.Parse(html.GetBetween("var balance = ", ";"));
            tradeit.Accepted_TOS = int.Parse(html.GetBetween("var accepted_tos = ", ";"));
            tradeit.TradeCount = int.Parse(html.GetBetween("var trade_count = ", ";"));
            tradeit.TradeValue = int.Parse(html.GetBetween("var trade_value = ", ";"));
            tradeit.Register_TimeStamp = ulong.Parse(html.GetBetween("var register_timestamp = ", ";"));
            tradeit.socketid = html.GetBetween("var socketid = `", "`;");
            tradeit.Json_URL = html.GetBetween("var jsonurl = \"", "\";");
            tradeit.Username = html.GetBetween("var username = `", "`;");
            tradeit.playerid = html.GetBetween("var playerid = `", "`;");
            return tradeit;
        }
        public void SetSession(SessionData Session)
        {
            this.Session = Session;
        }

        public async Task<Classes.Inventory> GetInventory(Enums.GAMES Game)
        {
            return await Classes.Inventory.Load(Session, Game, Json_URL, Proxy);
        }
        public async Task<List<int>> GetInventSize(Enums.GAMES Game)
        {
            if (!LoggedIn) return new();
            BrowserEmulation browser = new();
            browser.add_tradeit_cookies(Session);
            browser.webClient.Proxy = Proxy;
            string json;
            try
            {
                json = await Task.Run(delegate { return browser.getRequest($"https://tradeit.gg/invsize{Game.ToInt32()}"); });
                var obj = JsonSerializer.Deserialize<List<int>>(json);
                return obj;
            }
            catch (Exception)
            {
                return new();
            }
        }
        public async Task<List<bool>> GetActiveBots()
        {
            if (!LoggedIn) return new();
            BrowserEmulation browser = new();
            browser.add_tradeit_cookies(Session);
            browser.webClient.Proxy = Proxy;
            string json;
            try
            {
                json = await Task.Run(delegate { return browser.getRequest("https://tradeit.gg/activebots"); });
                var obj = JsonSerializer.Deserialize<List<bool>>(json);
                return obj;
            }
            catch (Exception)
            {
                return new();
            }
        }

        public async Task<TradeOffer> Trade(Models.Inventory.CachedInventorySkins your_skin, int index_of_your_skin, Models.Inventory.InventorySkins trade_skin, int index_of_trade_skin, Enums.GAMES Game)
        {
            return await Trade(your_skin, index_of_your_skin, trade_skin, index_of_trade_skin, Game);
        }
        public async Task<TradeOffer> Trade(Models.Inventory.CachedInventorySkins your_skin, int index_of_your_skin, int bot_id, Enums.GAMES Game)
        {
            return await Trade(your_skin, index_of_your_skin, null, 0, Game, bot_id);
        }
        private async Task<TradeOffer> Trade(Models.Inventory.CachedInventorySkins your_skin, int index_of_your_skin, Models.Inventory.InventorySkins trade_skin, int index_of_trade_skin, Enums.GAMES Game, int bot)
        {
            if (!LoggedIn) return new TradeOffer(); //если не выполнен вход
            else if (string.IsNullOrEmpty(TradeURL)) return new TradeOffer(); //если не указан TradeURL
            else if (your_skin.skins.Count - 1 > index_of_your_skin) return new TradeOffer(); //если ArgumentOutOfIndex
            else if (!your_skin.Price.HasValue || your_skin.Price == 0) return new TradeOffer(); // если нет цены на ваш скин
            if (trade_skin != null)
            {
                if (trade_skin.skins.Count - 1 > index_of_trade_skin) return new TradeOffer(); //если ArgumentOutOfIndex
                else if (!trade_skin.skins[index_of_trade_skin].price.HasValue) return new TradeOffer(); //если нет цены на предмет бота
            }
            
            int token = new Random().Next(192845, int.MaxValue);
            var g_int32 = Game.ToInt32();
            string s_hash_name = trade_skin != null ? System.Web.HttpUtility.UrlEncode(trade_skin.market_hash_name) : null;
            string c_hash_name = System.Web.HttpUtility.UrlEncode(your_skin.market_hash_name);
            int cprice = your_skin.Price.Value;
            int sprice = trade_skin != null ? trade_skin.skins[index_of_trade_skin].price.Value : 0;
            int bot_id = trade_skin != null ? trade_skin.bot : bot;

            var content = $"сselected%5B%5D={your_skin.bot}_{g_int32}_{your_skin.id}_{your_skin.skins[index_of_your_skin].assetid}_{c_hash_name}";
            if (trade_skin != null) content += $"sselected%5B%5D={trade_skin.bot}_{g_int32}_{trade_skin.id}_{trade_skin.skins[index_of_trade_skin].assetid}_{s_hash_name}";
            content += $"&botid={bot_id}";
            content += $"&tradeurl={System.Web.HttpUtility.UrlEncode(TradeURL)}";
            content += $"&sprice={cprice}";
            content += $"&сprice={sprice}";
            content += $"&token={token}";
            content += $"&bonus=0";
            if (trade_skin != null) content += $"&delayed=false";
            content += $"&locale=ru";

            string content_type = "application/x-www-form-urlencoded; charset=UTF-8";
            string referer = "https://tradeit.gg/?welcome=back";
            string url = "https://tradeit.gg/trade";
            var (success, data, _) = await Web.Downloader.PostTradeitAsync(url, content, content_type, Session, Proxy, referer);
            if (!success) return new TradeOffer();

            var amount = your_skin.skins[index_of_your_skin].price.Value - trade_skin.skins[index_of_trade_skin].price.Value; 
            _mTradeItems.Add(new TradeItemSale(token.ToString(), amount.SwitchSign())); //Обязательно нужно поменять знак числа, иначе при необходимости добавить баланс будет его отнимание

            if (data.Split('b').Length > 1 && !string.IsNullOrEmpty(data.Split('b')[1]))
            {
                return new TradeOffer(true, data.Split('b')[1], data.Split('b')[0], token);
            }
            else
            {
                return new TradeOffer(true, "0", data.Replace("b", ""), token);
            }
        }
        public BuyItem BuyUserItem(string assetid, int price, int type = 2)
        {
            if (!LoggedIn) return new BuyItem() { error = "LoggedIn False" };
            else if (string.IsNullOrEmpty(TradeURL)) return new BuyItem();
            string url = "https://tradeit.gg/buyuseritems";
            //var content = $"items=%5B%7B%22type%22%3A%22{type}%22%2C%22asset_id%22%3A%{assetid}%22%2C%22price%22%3A{price}%7D%5D";
            //string content_type = "application/x-www-form-urlencoded; charset=UTF-8";
            //string referer = "https://tradeit.gg/?welcome=back";
            //var (success, json, _) = await Web.Downloader.PostTradeitAsync(url, Web.Downloader.UserAgentChrome, null, Session, content, content_type, referer);
            //if (!success) return new BuyItem();
            BrowserEmulation browser = new();
            browser.add_tradeit_cookies(Session);
            browser.webClient.Proxy = Proxy;
            string json = null;
            string content = $"items=[{{\"type\":\"{type}\",\"asset_id\":\"{assetid}\",\"price\":{price}}}]";
            try
            {
                json = browser.postRequest(url, content);
            }
            catch (Exception)
            {
            }
            if(json == null) return new BuyItem();
            var obj = JsonSerializer.Deserialize<BuyItem>(json);
            if (obj.success)
            {
                this.Balance -= price;
            }
            return obj;
        }
        public async Task<WithdrawItem> WithdrawFromInv(string assetid)
        {
            if (!LoggedIn) return new WithdrawItem() { error = "LoggedIn False" };
            else if (string.IsNullOrEmpty(TradeURL)) return new WithdrawItem();
            int token = new Random().Next(10000, 99999);
            string url = "https://tradeit.gg/withdrawfrominv";
            var content = $"asset_id={assetid}";
            content += $"&tradeurl={System.Web.HttpUtility.UrlEncode(TradeURL)}";
            content += $"&token={token}";
            string content_type = "application/x-www-form-urlencoded; charset=UTF-8";
            string referer = "https://tradeit.gg/inventory";
            var (success, json, _) = await Web.Downloader.PostTradeitAsync(url, content, content_type, Session, Proxy, referer);
            if (!success) return new WithdrawItem();
            var obj = JsonSerializer.Deserialize<WithdrawItem>(json);
            obj.token = token.ToString();
            if (obj.success && IsWSAlive) _mTradeItems.Add(new TradeItemSale(token.ToString(), 0));
            return obj;
        }
        public async Task<BuyItem> Reserve(Models.Inventory.InventorySkins trade_skin, int index_of_trade_skin)
        {
            if (!LoggedIn) return new BuyItem(); //если не выполнен вход
            else if (trade_skin.skins.Count - 1 > index_of_trade_skin) return new BuyItem(); //если ArgumentOutOfIndex
            else if (!trade_skin.skins[index_of_trade_skin].price.HasValue) return new BuyItem(); //если нет цены на предмет бота

            int sprice = trade_skin.skins[index_of_trade_skin].price.Value;
            var content = $"sselected%5B%5D={trade_skin.skins[index_of_trade_skin].assetid}_{trade_skin.bot}&value={sprice}";
            string content_type = "application/x-www-form-urlencoded; charset=UTF-8";
            string referer = "https://tradeit.gg/?welcome=back";
            string url = "https://tradeit.gg/reserve";

            var (success, json, _) = await Web.Downloader.PostTradeitAsync(url, content, content_type, Session, Proxy, referer);
            if (!success) return new BuyItem();
            var obj = JsonSerializer.Deserialize<BuyItem>(json);
            if (obj.success) this.Balance -= sprice;
            return obj;
        }
        public async Task<Models.Inventory.CachedInventory> GetCachedInventory()
        {
            string url = $"https://inventory.tradeit.gg/cinv/cached?token={socketid}";
            var (success, json, _) = await Web.Downloader.GetTradeitAsync(url, Session, Proxy, "https://tradeit.gg/");
            if (!success) return new Models.Inventory.CachedInventory();
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            ushort index = 0;
            var cached = new Models.Inventory.CachedInventory();
            foreach (var item in dict)
            {
                var jtoken = (JsonElement)dict[item.Key];
                if (jtoken.ValueKind == JsonValueKind.Number && item.Key.ToLower() == "timestamp")
                {
                    cached.timestamp = jtoken.GetInt64();
                }
                else if (jtoken.ValueKind == JsonValueKind.Object)
                {
                    if (item.Key.ToLower() == "440" || item.Key.ToLower() == "570" || item.Key.ToLower() == "730" || item.Key.ToLower() == "753" || item.Key.ToLower() == "252490")
                    {
                        var app = JsonSerializer.Deserialize<Models.Inventory.CachedInventoryGameTemp>(jtoken.GetRawText());
                        foreach (var game_item in app.items)
                        {
                            var item0 = app.items[game_item.Key];
                            item0.bot = index;
                            item0.market_hash_name = game_item.Key.Split('_')[1];
                            item0.id = ulong.Parse(game_item.Key.Split('_')[0]);
                            var cache_item = new Models.Inventory.CachedInventorySkins() {
                                a = item0.a,
                                bot = item0.bot,
                                cs = item0.cs,
                                hi = item0.hi,
                                id = item0.id,
                                market_hash_name = item0.market_hash_name,
                                quility = item0.quility,
                                skins = item0.skins,
                                static_url = item0.static_url,
                                Price = item0.Price,
                                ws = item0.ws,
                                x = item0.x
                            };
                            cache_item.Game = (Enums.GAMES)int.Parse(item.Key);
                            cached.items.Add(cache_item);
                        }
                        cached.stickers = app.stickers;
                    }
                }
            }
            GC.Collect(); //вызываем очистку, иначе она не сработает
            return cached;
        }
    }
}
