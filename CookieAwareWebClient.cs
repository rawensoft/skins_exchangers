using SteamAuth;
using System;
using System.IO;
using System.Net;
using System.Reflection;

public class CookieAwareWebClient : WebClient
{
	public CookieContainer CookieContainer { get; set; } = new CookieContainer();
	public string proxyadress { get; set; }

	public void add_steam_cookies(UserLogin user)
	{
		this.CookieContainer.Add(new Cookie("steamLoginSecure", user.Session.SteamLoginSecure, "/", ".steamcommunity.com")
		{
			HttpOnly = true,
			Secure = true
		});
		this.CookieContainer.Add(new Cookie("Steam_Language", "english", "/", ".steamcommunity.com"));
		this.CookieContainer.Add(new Cookie("sessionid", user.Session.SessionID, "/", ".steamcommunity.com"));
	}
	public void add_steam_cookies_1(UserLogin user)
	{
		this.CookieContainer.Add(new Cookie("steamLoginSecure", user.Session.SteamLoginSecure, "/", ".store.steampowered.com")
		{
			HttpOnly = true,
			Secure = true
		});
		this.CookieContainer.Add(new Cookie("Steam_Language", "english", "/", ".store.steampowered.com"));
		this.CookieContainer.Add(new Cookie("sessionid", user.Session.SessionID, "/", ".store.steampowered.com"));
	}
	public void add_meow_cookies(string name, string value)
	{
		this.CookieContainer.Add(new Cookie(name, value, "/", ".meowskins.ru"));
	}
	public string PutCode(string url, string param)
	{
		string res = "";
		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
		bool flag = request != null;
		if (flag)
		{
			bool flag2 = this.proxyadress != null;
			if (flag2)
			{
				string adress = this.proxyadress.Split(new char[]
				{
					':'
				})[0];
				int port = int.Parse(this.proxyadress.Split(new char[]
				{
					':'
				})[1]);
				WebProxy proxy = (WebProxy)(request.Proxy = new WebProxy(adress, port));
			}
			bool flag3 = this.referer != "";
			if (flag3)
			{
				request.Referer = this.referer;
				this.referer = "";
			}
			bool flag4 = this.Host != "";
			if (flag4)
			{
				request.Host = this.Host;
				this.Host = "";
			}
			bool flag5 = !this.clear_headers;
			if (flag5)
			{
				request.ServicePoint.Expect100Continue = false;
				//request.ServicePoint.GetType().GetProperty("HttpBehaviour", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(request.ServicePoint, 0, null);
				request.CookieContainer = this.CookieContainer;
				request.Accept = "*/*";
				request.Headers.Add("Content-Encoding: gzip");
				request.Headers.Add("X-CSRF-TOKEN: " + this.csrftoken);
				request.Headers.Add("Accept-Language: ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
				request.UserAgent = this.useragent;
				request.Headers.Add("X-Requested-With: XMLHttpRequest");
				request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
				request.Headers.Add("Origin: https://store.steampowered.com");
				this.clear_headers = false;
			}
			else
			{
				request.CookieContainer = this.CookieContainer;
			}
		}
		request.ContentType = "application/json";
		request.Method = "POST";
		using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
		{
			streamWriter.Write(param);
			streamWriter.Flush();
			streamWriter.Close();
		}
		HttpWebResponse httpResponse = (HttpWebResponse)request.GetResponse();
		using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
		{
			res = streamReader.ReadToEnd();
		}
		return res;
	}
	protected override WebRequest GetWebRequest(Uri uri)
	{
		WebRequest request = base.GetWebRequest(uri);
		bool flag = request is HttpWebRequest;
		if (flag)
		{
			bool flag2 = this.proxyadress != null;
			if (flag2)
			{
				string adress = this.proxyadress.Split(new char[]
				{
					':'
				})[0];
				int port = int.Parse(this.proxyadress.Split(new char[]
				{
					':'
				})[1]);
				WebProxy proxy = (WebProxy)((request as HttpWebRequest).Proxy = new WebProxy(adress, port));
			}
			bool flag3 = this.referer != "";
			if (flag3)
			{
				(request as HttpWebRequest).Referer = this.referer;
				this.referer = "";
			}
			bool flag4 = this.Host != "";
			if (flag4)
			{
				(request as HttpWebRequest).Host = this.Host;
				this.Host = "";
			}
			bool flag5 = !this.clear_headers;
			if (flag5)
			{
				(request as HttpWebRequest).ServicePoint.Expect100Continue = false;
				//(request as HttpWebRequest).ServicePoint.GetType().GetProperty("HttpBehaviour", BindingFlags.Instance | BindingFlags.NonPublic).SetValue((request as HttpWebRequest).ServicePoint, 0, null);
				(request as HttpWebRequest).CookieContainer = this.CookieContainer;
				(request as HttpWebRequest).Accept = "*/*";
				(request as HttpWebRequest).Headers.Add("Accept-Language: ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
				(request as HttpWebRequest).UserAgent = this.useragent;
				(request as HttpWebRequest).Headers.Add("X-Requested-With: XMLHttpRequest");
				(request as HttpWebRequest).ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
				(request as HttpWebRequest).Headers.Add("Origin: https://store.steampowered.com");
				this.clear_headers = false;
			}
			else
			{
				(request as HttpWebRequest).CookieContainer = this.CookieContainer;
			}
		}
		return request;
	}

	public string referer = "";
	public string csrftoken = "";
	public string Host = "";
	public string useragent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36 OPR/58.0.3135.79";
	public bool clear_headers;
}