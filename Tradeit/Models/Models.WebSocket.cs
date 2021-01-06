using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkinsExchangers.TradeitGG.Models.WebSocket
{
    public record SocketInfo
    {
        public string sid { get; set; }
        public List<string> upgrades { get; set; }
        public int pingInterval { get; set; }
        public int pingTimeout { get; set; }
    }
    public record TradeInfo
    {
        public Enums.TRADE_INFO Info { get; set; }
        public string SteamID { get; set; }
        public TradeInfo(Enums.TRADE_INFO Info, string SteamID)
        {
            this.Info = Info;
            this.SteamID = SteamID;
        }
    }
    public record StatsInfo
    {
        public int status { get; set; }
        public string total_trades { get; set; }
        public string total_users { get; set; }
        public string users_online { get; set; }
        public StatsInfo(int status, string total_trades, string total_users, string users_online)
        {
            this.status = status;
            this.total_trades = total_trades;
            this.total_users = total_users;
            this.users_online = users_online;
        }
    }
    public record LastTradeInfo
    {
        /// <summary>
        /// SteamID64
        /// </summary>
        public string cid { get; set; }
        /// <summary>
        /// ?ID на Tradeit.gg
        /// </summary>
        public ulong id { get; set; }
        public int v { get; set; }
        public List<LastTradeItem> playerItems { get; set; } = new();
        public List<LastTradeItem> botItems { get; set; } = new();
    }
    public record LastTradeItem
    {
        public string c { get; set; }
        /// <summary>
        /// AppID игры в Steam
        /// </summary>
        [JsonPropertyName("g")] public int game { get; set; }
        [JsonPropertyName("p")] public int price { get; set; }
        public int q { get; set; }
    }
    public record TradeStateInfo
    {
        [JsonPropertyName("steamId")] public string SteamID { get; set; }
        [JsonIgnore]
        public ulong SteamID64
        {
            get { return ulong.Parse(SteamID); }
        }
        /// <summary>
        /// Токен трейда (создаётся при вызове POST /trade)
        /// </summary>
        [JsonPropertyName("token")] public string Token { get; set; }
        /// <summary>
        /// 0 - отправка трейда, 1 - необходимо принять трейд, 2 - необходимо принять в телефоне(т.е. в стиме), 3 - трейд принят (сделка завершена)
        /// </summary>
        [JsonPropertyName("state")] public int State { get; set; }
        [JsonIgnore]
        public Enums.TRADE_STATE E_State
        {
            get { return (Enums.TRADE_STATE)State; }
        }
        /// <summary>
        /// 0-Verified trade - трейд создан, 1-Trade sent:4380396564, 2 - Accepted mobile confirmation:4380396564, 3-Accepted
        /// </summary>
        [JsonPropertyName("msg")] public string Message { get; set; }
        [JsonIgnore]
        public bool Is_Wrong_MSG
        {
            get
            {
                if (Message.Contains("Verified trade") || Message.Contains("Trade sent") ||
                    Message.Contains("Accepted mobile confirmation") || Message.Contains("Accepted")) return false;
                return true;
            }
        }
        [JsonIgnore]
        public uint? trade_id
        {
            get
            {
                string[] splitted = Message.Split(':');
                if (splitted.Length == 2)
                {
                    var id = splitted[1];
                    return uint.Parse(id);
                }
                return null;
            }
        }
    }
}
