# Skins Exchangers
Библиотека для лёгкого управления обменниками скинов Steam.

Реализованы следующие обменники:
* [tradeit.gg](https://tradeit.gg)

## ТРЕБОВАНИЯ

* [SteamAuth](https://github.com/geel9/SteamAuth/tree/master/SteamAuth)
* [H.Socket.IO](https://github.com/HavenDV/H.Socket.IO)  
		Заменить префиксы в `H.Engine.IO.EngineIoPacket` на:
	* `OpenPrefix = "2probe";`
	* `ClosePrefix = "41";`
	* `PingPrefix = "2";`
	* `PongPrefix = "3";`
	* `MessagePrefix = "";`
	* `UpgradePrefix = "5";`
	* `NoopPrefix = "6";`

## Примеры

Вход в аккаунт
```
	var session = await TradeitGG.Models.SessionData.AuthAsync(SteamAuth.SessionData, IWebProxy);
    var tradeit = await TradeitGG.Tradeit.Load(session, IWebProxy);
    if (tradeit != null && tradeit.LoggedIn)
    {
    	tradeit.TradeURL = ""; //Ваша ссылка на обмен
        
        //TODO вход выполнен
    }
```
ВебСокет
```
	tradeit.OnTradeState += (TradeitGG.Tradeit sender, TradeitGG.Models.WebSocket.TradeStateInfo Info) => { 
    	MessageBox.Show($"{Info.Message}\nState: {Info.State}"); 
    };  //Изменение состояния трейдов текущего аккаунта
    
	tradeit.OnWebSocketClosed += (TradeitGG.Tradeit sender) => { 
		MessageBox.Show($"Worked {tradeit.WS_Worked.TotalSeconds}"); 
    };
        
    if(await tradeit.StartWebSocket())
    {
    	//TODO сокет запустился
    }
```
Прокси, только http(s). Для отладки через fiddler4 нужно:
*	IP: 127.0.0.1 
*	Port: 8888 
*	Конфигурация DEBUG
*	В основной папке должен лежать сертификат *FiddlerRoot.cer*
```
var proxy = new Web.Proxy(IP, Port, Username, Pass);
tradeit.Proxy = proxy;
```  
Диаграмма покупки предметов tradeit
![alt-текст](https://github.com/rawensoft/skins_exchangers/raw/main/api_tradeit.png "Текст заголовка логотипа 1")
