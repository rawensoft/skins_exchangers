using System;
using System.Net;
using System.Text.Json.Serialization;
namespace SkinsExchangers.Web
{
    public class Proxy : IWebProxy
    {
        public bool UseProxy { get; set; } = true;
        public string IP { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        [JsonIgnore]
        public bool IsCredentials
        {
            get
            {
                if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password)) return true;
                return false;
            }
        }
        [JsonIgnore] 
        public ICredentials Credentials
        {
            get
            {
                if (!IsCredentials) return null;
                return new NetworkCredential(Username, Password);
            }
            set
            {
                if(value != null)
                {
                    var cred = (NetworkCredential)value;
                    Username = cred.UserName;
                    Password = cred.Password;
                    return;
                }
                Username = null;
                Password = null;
            }
        }

        public Proxy(string IP, int Port, bool UseProxy = true)
        {
            this.IP = IP;
            this.Port = Port;
            this.UseProxy = UseProxy;
        }
        public Proxy(string IP, int Port, string Username, string Password, bool UserProxy = true) : this(IP, Port, UserProxy)
        {
            this.Username = Username;
            this.Password = Password;
        }
        public Uri GetProxy(Uri destination)
        {
            return new Uri($"http://{IP}:{Port}");
        }
        public bool IsBypassed(Uri host)
        {
            if (!UseProxy) return true;
            return false;
        }
    }
}
