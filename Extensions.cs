using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinsExchangers.Extensions
{
    public static class ExtensionMethods
    {
        public static int SwitchSign(this int value)
        {
            string val = value.ToString();
            if (val[0] == '-') return int.Parse(val.Remove(0, 1));
            return int.Parse($"-{value}");
        }
        public static int ToInt32(this TradeitGG.Enums.GAMES Game)
        {
            if (Game == TradeitGG.Enums.GAMES.CSGO) return 730;
            else if (Game == TradeitGG.Enums.GAMES.DOTA2) return 570;
            else if (Game == TradeitGG.Enums.GAMES.RUST) return 252490;
            else if (Game == TradeitGG.Enums.GAMES.STEAM) return 753;
            else if (Game == TradeitGG.Enums.GAMES.TF2) return 440;
            return 0;
        }
        public static string GenerateString(int x)
        {
            string pass = "";
            var r = new Random();
            while (pass.Length < x)
            {
                char c = (char)r.Next(33, 125);
                if (char.IsLetterOrDigit(c)) pass += c;
            }
            return pass;
        }
        public static bool ContainsOnlyDigit(this String strSource)
        {
            for (int i = 0; i < strSource.Length; i++)
            {
                if (!Char.IsDigit(strSource[i])) return false;
            }
            return true;
        }
        public static string GetOnlyDigit(this String strSource)
        {
            string str = "";
            for (int i = 0; i < strSource.Length; i++)
            {
                if (Char.IsDigit(strSource[i])) str += strSource[i];
            }
            return str;
        }
        public static string Formatted(this DateTime dt)
        {
            //string data = string.Format("{0}:{1}:{2} {3}.{4}.{5}", dt.Hour, minut, second, dt.Day, month, dt.Year);
            string data = dt.ToString("dd.MM.yyyy HH:mm:ss");
            return data;
        }
        public static int ToTimeStamp(this DateTime dt)
        {
            var unixTimestamp = (int)(dt.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return unixTimestamp;
        }
        public static string ToStringQuery(this short? value)
        {
            if (value.HasValue && value.Value != 0) return value.Value.ToString();
            return "";
        }
        public static string ToStringQuery(this int? value)
        {
            if (value.HasValue && value.Value != 0) return value.Value.ToString();
            return "";
        }
        public static DateTime ToDateTime(this int seconds)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var timeSpan = TimeSpan.FromSeconds(seconds);
            epoch = epoch.Add(timeSpan);
            return epoch.ToLocalTime();
        }
        public static int ToInt32(this bool value)
        {
            if (value == true) return 1;
            return 0;
        }
        public static string GetBetween(this string strSource, string strStart, string strEnd, string replace = null, int x = 0)
        {
            if (strSource == null) return null;
            int Start, End;
            if (strStart == null || strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                if (x > 0)
                {
                    string[] splitted = strSource.Split(strStart);
                    return splitted[x].Split(strEnd)[0];
                }
                string retString = null;
                if (strStart == null)
                {
                    strStart = "";
                    for (int i = 0; i < strSource.Length; i++)
                    {
                        if (char.IsDigit(strSource[i])) break;
                        strStart += strSource[i];
                    }
                }
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                try
                {
                    string data = strSource.Substring(Start, End - Start);
                    retString = data;
                }
                catch (Exception)
                {
                    retString = replace;
                }
                if (retString == null) return replace;
                return retString;
            }
            else return replace;
        }
    }
}
