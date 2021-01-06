using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using SkinsExchangers.Extensions;
using SkinsExchangers.TradeitGG.Models;
using SkinsExchangers.TradeitGG.Models.Inventory;

namespace SkinsExchangers.TradeitGG.Classes
{
    public class Inventory
    {
        public List<InventoryBot> InventoryItems { get; private set; } = new();
        public static async Task<Inventory> Load(SessionData Session, Enums.GAMES Game, string url_base = "https://inventory.tradeit.gg/sinv", IWebProxy proxy = null)
        {
            string url = $"{url_base}/{Game.ToInt32()}";
            var (success, json, _) = await Web.Downloader.GetTradeitAsync(url, Session, proxy, "https://tradeit.gg/");
            if (!success) return null;
            var list = new List<InventoryBot>();
            var dict = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
            ushort index = 0;
            for (int i = 0; i < dict.Count; i++)
            {
                var d_bot = dict[i];
                foreach (var item in d_bot)
                {
                    var jtoken = (JsonElement)d_bot[item.Key];
                    var bot = new InventoryBot();
                    if (jtoken.ValueKind == JsonValueKind.Number)
                    {
                        if(item.Key.ToLower() == "timestamp") bot.timestamp = jtoken.GetInt64();
                        else if(item.Key.ToLower() == "value") bot.value = jtoken.GetInt32();
                    }
                    else if(jtoken.ValueKind == JsonValueKind.String && item.Key == "steamid")
                    {
                        bot.steamid = jtoken.GetString();
                    }
                    else if(jtoken.ValueKind == JsonValueKind.Object)
                    {
                        if (item.Key.ToLower() == "440" || item.Key.ToLower() == "570" || item.Key.ToLower() == "730" || item.Key.ToLower() == "753" || item.Key.ToLower() == "252490")
                        {
                            var app = JsonSerializer.Deserialize<InventoryGameTemp>(jtoken.GetRawText());
                            foreach (var game_item in app.items)
                            {
                                var item0 = app.items[game_item.Key];
                                item0.bot = index;
                                item0.market_hash_name = game_item.Key.Split('_')[1];
                                item0.id = ulong.Parse(game_item.Key.Split('_')[0]);
                                bot.items.Add(item0);
                            }
                            bot.stickers = app.stickers;
                        }
                        else if(item.Key.ToLower() == "sticker_prices")
                        {
                            var stikers = JsonSerializer.Deserialize<Dictionary<string, int>>(jtoken.GetRawText());
                            bot.sticker_prices = stikers;
                        }
                    }
                    list.Add(bot);
                }
            }
            GC.Collect();
            return new Inventory() { InventoryItems = list };
        }
        public static int GetContextID(int appid)
        {
            if (appid == 433850) return 1;
            else if (appid == 753) return 6;
            return 2;
        }
        public List<InventorySkins> GetItemsByPriceToLower(int price)
        {
            var list = new List<InventorySkins>();
            for (int i = 0; i < InventoryItems.Count; i++)
            {
                for (int x = 0; x < InventoryItems[i].items.Count; x++)
                {
                    for (int y = 0; y < InventoryItems[i].items[x].skins.Count; y++)
                    {
                        if (InventoryItems[i].items[x].skins[y].price.HasValue && InventoryItems[i].items[x].skins[y].price.Value <= price)
                        {
                            list.Add(InventoryItems[i].items[x]);
                            break;
                        }
                    }
                }
            }
            return list;
        }
    }
}
