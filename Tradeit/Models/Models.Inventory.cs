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
        public Enums.GAMES Game;
        [JsonPropertyName("p")] public int? price { get; set; }
        [JsonPropertyName("tradable")]
        public int tradeble { get; set; } = 0;

        internal CachedInventorySkins() { }
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
        public int? ps { get; set; }
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
    }
    public record InventorySticker
    {
        [JsonPropertyName("l")] public string Link { get; set; }
        [JsonPropertyName("n")] public string Name { get; set; }
        internal InventorySticker() { }
    }
}
