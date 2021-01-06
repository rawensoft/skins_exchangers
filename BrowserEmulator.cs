using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using SteamAuth;

public class BrowserEmulation
{
	public CookieAwareWebClient webClient = new CookieAwareWebClient();
	public void add_steam_cookies(SessionData Session)
	{
		this.webClient.CookieContainer.Add(new Cookie("steamLoginSecure", Session.SteamLoginSecure, "/", ".steamcommunity.com")
		{
			HttpOnly = true,
			Secure = true
		});
		this.webClient.CookieContainer.Add(new Cookie("Steam_Language", "english", "/", ".steamcommunity.com"));
		this.webClient.CookieContainer.Add(new Cookie("sessionid", Session.SessionID, "/", ".steamcommunity.com"));
	}
	public void add_steam_cookies_1(SessionData Session)
	{
		this.webClient.CookieContainer.Add(new Cookie("steamLoginSecure", Session.SteamLoginSecure, "/", ".store.steampowered.com")
		{
			HttpOnly = true,
			Secure = true
		});
		this.webClient.CookieContainer.Add(new Cookie("Steam_Language", "english", "/", ".store.steampowered.com"));
		this.webClient.CookieContainer.Add(new Cookie("sessionid", Session.SessionID, "/", ".store.steampowered.com"));
	}
	public void add_tradeit_cookies(SkinsExchangers.TradeitGG.Models.SessionData Session)
	{
		this.webClient.CookieContainer.Add(new Cookie("sessionid", Session.sessionid, "/", "tradeit.gg")
		{
			HttpOnly = true
		});
		this.webClient.CookieContainer.Add(new Cookie("__cfduid", Session.__cfduid, "/", ".tradeit.gg")
		{
			HttpOnly = true,
			Secure = true
		});
	}
	public void setProxy(string adress, int port, string username = null, string pass = null)
	{
		WebProxy wp = new WebProxy(adress, port);
		if(username != null && pass != null) wp.Credentials = new NetworkCredential(username, pass);
		this.webClient.Proxy = wp;
	}
	public string checkproxy()
	{
		return this.getRequest("https://whatismyip.network/");
	}

	public bool downloadImage(string url, string fiename)
	{
		bool flag = !Directory.Exists("images");
		if (flag)
		{
			Directory.CreateDirectory("images");
		}
		try
		{
			this.webClient.DownloadFile(new Uri(url), "images/" + fiename);
			return true;
		}
		catch (Exception)
		{
		}
		return false;
	}
	public string getRequest(string url)
	{
        try
        {
			return this.webClient.DownloadString(url);
		}
        catch (WebException ex)
        {
            if (ex.Message.Contains("302"))
            {
				return this.getRequest(ex.Response.Headers["Location"]);
            }
        }
		return null;
	}
	public string postRequest(string url, string param)
	{
		NameValueCollection values = new NameValueCollection();
		bool flag = param != "";
		if (flag && !param.StartsWith("{\""))
		{
			string[] array = param.Split(new char[]
			{
				'&'
			});
			foreach (string text in array)
			{
				values[text.Split(new char[]
				{
					'='
				})[0]] = Uri.UnescapeDataString(text.Split(new char[]
				{
					'='
				})[1]);
			}
		}
        try
        {
			byte[] HtmlResult = this.webClient.UploadValues(url, "POST", values);
			return Encoding.Default.GetString(HtmlResult);
		}
        catch (WebException ex)
        {
            if (ex.Message.Contains("302"))
            {
				return getRequest(ex.Response.Headers["Location"]);
			}
			return null;
        }
	}
	public string postRequestM(string url, string param)
	{
		NameValueCollection values = new NameValueCollection();
		string[] array = param.Split(new char[]
		{
			'&'
		});
		foreach (string text in array)
		{
			values[text.Split(new char[]
			{
				':'
			})[0]] = text.Split(new char[]
			{
				':'
			})[1];
		}
		byte[] HtmlResult = this.webClient.UploadValues(url, "POST", values);
		return Encoding.Default.GetString(HtmlResult);
	}
	public string postRequest(string url, string file, string param)
	{
		NameValueCollection values = new NameValueCollection();
		string[] array = param.Split(new char[]
		{
			'&'
		});
		foreach (string text in array)
		{
			values[text.Split(new char[]
			{
				'='
			})[0]] = text.Split(new char[]
			{
				'='
			})[1];
		}
		byte[] HtmlResult = this.webClient.UploadValues(url, "POST", values);
		return Encoding.Default.GetString(HtmlResult);
	}
	public string UploadFileEx(string uploadfile, string url, string fileFormName, string contenttype, NameValueCollection querystring, CookieContainer cookies)
	{
		bool flag = fileFormName == null || fileFormName.Length == 0;
		if (flag)
		{
			fileFormName = "file";
		}
		bool flag2 = contenttype == null || contenttype.Length == 0;
		if (flag2)
		{
			contenttype = "application/octet-stream";
		}
		string postdata = "?";
		bool flag3 = querystring != null;
		if (flag3)
		{
			foreach (object obj in querystring.Keys)
			{
				string key = (string)obj;
				postdata = string.Concat(new string[]
				{
					postdata,
					key,
					"=",
					querystring.Get(key),
					"&"
				});
			}
		}
		Uri requestUri = new Uri(url + postdata);
		string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
		HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(requestUri);
		webrequest.CookieContainer = cookies;
		webrequest.ContentType = "multipart/form-data; boundary=" + boundary;
		webrequest.Method = "POST";
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("--");
		stringBuilder.Append(boundary);
		stringBuilder.Append("\r\n");
		stringBuilder.Append("Content-Disposition: form-data; name=\"");
		stringBuilder.Append(fileFormName);
		stringBuilder.Append("\"; filename=\"");
		stringBuilder.Append(Path.GetFileName(uploadfile));
		stringBuilder.Append("\"");
		stringBuilder.Append("\r\n");
		stringBuilder.Append("Content-Type: ");
		stringBuilder.Append(contenttype);
		stringBuilder.Append("\r\n");
		stringBuilder.Append("\r\n");
		string postHeader = stringBuilder.ToString();
		byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);
		byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
		FileStream fileStream = new FileStream(uploadfile, FileMode.Open, FileAccess.Read);
		long num = webrequest.ContentLength = (long)((int)((long)postHeaderBytes.Length)) + fileStream.Length + (long)((int)((long)boundaryBytes.Length));
		Stream requestStream = webrequest.GetRequestStream();
		requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
		byte[] buffer = new byte[checked((uint)Math.Min(4096, (int)fileStream.Length))];
		int bytesRead;
		while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
		{
			requestStream.Write(buffer, 0, bytesRead);
		}
		requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
		return new StreamReader(webrequest.GetResponse().GetResponseStream()).ReadToEnd();
	}

}