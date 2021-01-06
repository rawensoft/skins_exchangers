using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkinsExchangers.TradeitGG.Models.Inventory
{
    #region Temp\Test
    internal record InventoryGameTemp
    {
        public Dictionary<string, InventorySkins> items { get; set; } = new();
        public List<InventorySticker> stickers { get; set; } = new();
    }
    internal record CachedInventoryGameTemp
    {
        public Dictionary<string, CachedInventorySkins> items { get; set; } = new();
        public List<InventorySticker> stickers { get; set; } = new();
    }
    #endregion

    #region Cached Inventory
    public record CachedInventory
    {
        public List<CachedInventorySkins> items { get; set; } = new();
        public List<InventorySticker> stickers { get; set; } = new();
        public long timestamp { get; set; }

        internal CachedInventory() { }
        public List<CachedInventorySkins> this[Enums.GAMES Game]
        {
            get
            {
                var list = new List<CachedInventorySkins>();
                foreach (var item in items)
                {
                    if (item.Game == Game) list.Add(item);
                }
                return list;
            }
        }
    }
    public record CachedInventorySkins: InventorySkins
    {
        [JsonPropertyName("g")]public Enums.GAMES Game;
        [JsonPropertyName("p")] public int? Price { get; set; }
        [JsonPropertyName("tradable")] public int Tradeble { get; set; } = 0;
        [JsonIgnore] public bool IsTradeble
        {
            get
            {
                if (Tradeble > 0) return true;
                return false;
            }
        }

        internal CachedInventorySkins() { }
        [JsonConstructor]
        public CachedInventorySkins(Enums.GAMES Game, int? Price, int Tradeble)
        {
            this.Game = Game;
            this.Price = Price;
            this.Tradeble = Tradeble;
        }
    }
    #endregion

    public record InventoryBot
    {
        public string steamid { get; set; }
        public long timestamp { get; set; }
        public int value { get; set; }
        public Dictionary<string, int> sticker_prices { get; set; } = new();
        public List<InventorySkins> items { get; set; } = new();
        public List<InventorySticker> stickers { get; set; } = new();

        internal InventoryBot() { }
        [JsonConstructor]
        public InventoryBot(string steamid, long timestamp, int value, Dictionary<string, int> sticker_prices, List<InventorySkins> items, List<InventorySticker> stickers)
        {
            this.steamid = steamid;
            this.timestamp = timestamp;
            this.value = value;
            this.sticker_prices = sticker_prices;
            this.stickers = stickers;
        }

        public List<InventorySkins> this[string market_hash_name]
        {
            get
            {
                var list = new List<InventorySkins>();
                foreach (var item in items)
                {
                    if (item.market_hash_name == market_hash_name) list.Add(item);
                }
                return list;
            }
        }
    }
    public record InventorySkins
    {
        [JsonPropertyName("bot")] public ushort bot { get; set; }
        [JsonPropertyName("id")] public ulong id { get; set; }
        [JsonPropertyName("mhn")] public string market_hash_name { get; set; }
        [JsonPropertyName("d")] public List<InventorySkin> skins { get; set; } = new();
        /// <summary>
        /// (Only CSGO) E.T. MW=MinimalWear
        /// </summary>
        [JsonPropertyName("e")] public string quility { get; set; }
        public int a { get; set; }
        /// <summary>
        /// ?data-max
        /// </summary>
        public int x { get; set; }
        /// <summary>
        /// Ссылка на загрузку картинки: tradeit.gg/static/img/items/35923.png
        /// </summary>
        [JsonPropertyName("q")] public int static_url { get; set; }
        public int hi { get; set; }
        public int ws { get; set; }
        public int cs { get; set; }

        internal InventorySkins() { }
        public InventorySkin this[int index]
        {
            get { return skins[index]; }
        }
    }
    public record InventorySkin
    {
        [JsonPropertyName("i")] public string assetid { get; set; }
        [JsonPropertyName("f")] public float? @float { get; set; }
        [JsonPropertyName("pi")] public int? paint_index { get; set; }
        //public int? ps { get; set; } //неизвестно, что это
        /// <summary>
        /// ?data-url
        /// </summary>
        [JsonPropertyName("u")] public string url { get; set; }
        /// <summary>
        /// Если null, то tradebale=true
        /// </summary>
        [JsonPropertyName("r")] public int? restriction { get; set; }
        [JsonPropertyName("p")] public int? price { get; set; }
        [JsonIgnore] public bool tradeble
        {
            get
            {
                if (restriction.HasValue) return false;
                return true;
            }
        }

        internal InventorySkin() { }
        [JsonConstructor]
        public InventorySkin(string assetid, float? @float, int? paint_index, string url, int? restriction, int? price)
        {
            this.assetid = assetid;
            this.@float = @float;
            this.paint_index = paint_index;
            this.url = url;
            this.restriction = restriction;
            this.price = price;
        }
    }
    public record InventorySticker
    {
        [JsonPropertyName("l")] public string Link { get; set; }
        [JsonPropertyName("n")] public string Name { get; set; }
        internal InventorySticker() { }
        [JsonConstructor]
        public InventorySticker(string Link, string Name)
        {
            this.Link = Link;
            this.Name = Name;
        }
    }
}
