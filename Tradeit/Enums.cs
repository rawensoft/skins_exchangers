using System;
using System.Collections.Generic;
using System.Text;

namespace SkinsExchangers.TradeitGG.Enums
{
    public enum TRADE_INFO
    {
        None,
        Canceled
    }
    public enum GAMES 
    { 
        CSGO = 730,
        DOTA2 = 570,
        RUST = 252490,
        TF2 = 440,
        STEAM = 753
    }
    public enum TRADE_STATE
    {
        /// <summary>
        /// Проверка трейда
        /// </summary>
        Verify,
        /// <summary>
        /// Трейд отправлен и доступен trade_id
        /// </summary>
        Sent,
        /// <summary>
        /// Необходимо принять трейд по trade_id
        /// </summary>
        NeedAccept,
        /// <summary>
        /// Трейд принят
        /// </summary>
        Accepted
    }
}
