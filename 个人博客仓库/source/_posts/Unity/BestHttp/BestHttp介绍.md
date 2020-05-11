---
title: BestHttp介绍1
date: 2020-05-11 11:41:32
categories:
- Unity
tags:
- BestHttp
---

# BestHttp 文档翻译

## 介绍

[BestHTTP](https://www.assetstore.unity3d.com/en/#!/content/10872) 是基于 [RFC 2616](https://www.w3.org/Protocols/rfc2616/rfc2616.html) 的 HTTP/1.1 实现 , 它支持几乎所有Unity移动和独立平台 (请参阅 [Supported platforms](#15)). 

我的目标是为Unity创建一个易于使用但功能强大的插件，以利用HTTP / 1.1中的潜力.    本文档是一个快速指南，并非所有功能和属性都可以在这里找到. 如需支持，功能请求或一般性问题，您可以发送电子邮件至besthttp@gmail.com.

## 快速入门

* 1:使用头文件.本文档中的所有示例都没有任何错误检查！在编写时，请确保添加一些空检查

```
using BestHTTP; 
```

* 2:GET 请求,向Web服务器发出请求的最简单方法是创建一个HTTPRequest对象，为其构造函数提供url和回调函数.在我们构造一个新的HTTPRequest对象后，我们唯一需要做的就是使用Send()函数发送请求.
```
{
        HTTPRequest request = new HTTPRequest(new Uri("https://google.com"), OnRequestFinished); request.Send(); 

        new HTTPRequest(new Uri("https://google.com"), (request, response) => Debug.Log("Finished!")).Send(); 
}
void OnRequestFinished(HTTPRequest request, HTTPResponse response) 
{    
        Debug.Log("Request Finished! Text received: " + response.DataAsText); 
} 
```
回调函数始终接收原始HTTPRequest对象和保存服务器响应的HTTPResponse对象。如果出现错误，则HTTPResponse对象为null,需要自己判断,并且请求对象具有Exception属性,该属性可能包含有关错误的额外信息(如果有).
虽然请求总是在不同的线程上处理，但调用回调函数已完成Unity的主线程，所以我们不必做任何线程同步。

* 3:其他请求 上面的例子是简单的GET请求。如果我们没有指定方法，默认情况下所有请求都将是GET请求。构造函数具有另一个参数，可用于指定请求的方法.

```
        HTTPRequest request = new HTTPRequest(new Uri("http://server.com/path"), HTTPMethods.Post, OnRequestFinished); 
        request.AddField("FieldName", "Field Value"); 
        request.Send(); 
```
要在不设置字段的情况下POST任何数据，可以使用RawData属性

```
HTTPRequest request = new HTTPRequest(new Uri("http://server.com/path"), HTTPMethods.Post, OnRequestFinished); 
request.RawData =  Encoding.UTF8.GetBytes("Field Value"); 
request.Send(); 
```

有关其他样品，请查看[Small Code-Samples](#14)部分。

除了GET和POST之外，您还可以使用 ***HEAD，PUT，DELETE，PATCH***  方法:

```
HTTPRequest request = new HTTPRequest(new Uri("http://server.com/path"), HTTPMethods.Head, OnRequestFinished); 
request.Send(); 

HTTPRequest request = new HTTPRequest(new Uri("http://server.com/path"), HTTPMethods.Put, OnRequestFinished); 
request.Send(); 

HTTPRequest request = new HTTPRequest(new Uri("http://server.com/path"), HTTPMethods.Delete, OnRequestFinished); 
request.Send(); 

HTTPRequest request = new HTTPRequest(new Uri("http://server.com/path"), HTTPMethods.Patch, OnRequestFinished); 
request.Send(); 
```
* 4:如何使用下载的数据?可以从HTTPResponse对象的Data属性访问原始字节。我们来看一个如何下载图像的例子:
```
new HTTPRequest(new Uri("http://yourserver.com/path/to/image.png"), (request, response) => 
        { 
                var tex = new Texture2D(0, 0); 
                tex.LoadImage(response.Data); 
                guiTexture.texture = tex; 
        }).Send(); 

new HTTPRequest(new Uri("http://yourserver.com/path/to/image.png"),
(request, response) => guiTexture.texture = response.DataAsTexture2D).Send(); 
```
除 response.DataAsTexture2D 外，还有一个 response.DataAsText 属性可将响应解码为Utf8字符串。将来可能会添加更多数据解码属性。如果您有任何想法，请自己添加吧.

* 5:替换 WWW,使用协程,一般不推荐使用这种方式
```
HTTPRequest request = new HTTPRequest(new Uri("http://server.com"));
request.Send(); 
yield return StartCoroutine(request); 
Debug.Log("Request finished! Downloaded Data:" + request.Response.DataAsText); 
```

## 高级用法
* 1:methodType,我们将向服务器发送什么样的请求。默认的methodType是HTTPMethods.Get
* 2:IsKeepAlive：向服务器指示我们希望tcp连接保持打开状态，因此连续的http请求不需要再次建立连接。如果我们将它保留为默认值true，它可以为我们节省大量时间。如果我们知道我们不会使用通常会将其设置为false的请求。默认值是true。
* 3:disableCache：告诉BestHTTP系统使用或完全跳过缓存机制。如果其值为true，则系统不会检查缓存中是否存储了响应，并且也不会保存响应。默认值为false
* 4:请求一次:
```
public static void RequestAsyncShort(string url, RequestCallBack callBack)
    {
        BestHTTP.HTTPRequest req = new HTTPRequest(new Uri(url), HTTPMethods.Get, 
            (originalRequest, response) =>
            {
                if (originalRequest.State == HTTPRequestStates.Finished)
                {
                    callBack((int) originalRequest.State, response.DataAsText);
                }
                else
                {
                    callBack((int) originalRequest.State, "");
                }
            });
        req.IsKeepAlive = false;
        req.DisableCache = true;
        req.Send();
    }
```

* 5:Best HTTP通过HTTPRequest的Credentials属性支持Basic和Digest身份验证：
```
using BestHTTP.Authentication; 
var request = new HTTPRequest(new Uri("http://yourserver.org/auth-path"), (req, resp) => 
        { 
                if (resp.StatusCode != 401) 
                        Debug.Log("Authenticated"); 
                else 
                        Debug.Log("NOT Authenticated"); 
                Debug.Log(resp.DataAsText); 
        }); 
request.Credentials = new Credentials("usr", "paswd"); 
request.Send(); 
```
* 6:下载流媒体(Download Streaming)
默认情况下，当完全下载并处理服务器的答案时，我们提供给HTTPRequest的构造函数的回调函数将只调用一次。这样，如果我们想要下载更大的文件，我们就会在移动设备上快速耗尽内存。我们的应用程序会崩溃，用户会对我们生气，应用程序会得到很多不好的评级。理所当然。为了避免这种情况，BestHTTP旨在非常容易地处理这个问题：只需将一个标志(属性)切换为true，每次下载预定义数据量时(指定内存中缓存多少字节量)都会调用我们的回调函数。此外，如果我们没有关闭缓存，下载的响应将被缓存，以便下次我们可以从本地缓存流式传输整个响应，而无需更改我们的代码，甚至无需访问Web服务器。 （备注：服务器必须发送有效的缓存头（“Expires”头：请参阅[RFC](https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.21)以允许此操作。）
```
var request = new HTTPRequest(new Uri("http://yourserver.com/bigfile"), (req, resp) => 
{ 
        List<byte[]> fragments = resp.GetStreamedFragments(); 
        // Write out the downloaded data to a file: 
        using (FileStream fs = new FileStream("pathToSave", FileMode.Append)) {
                foreach(byte[] data in fragments) fs.Write(data, 0, data.Length); 
        }
        if (resp.IsStreamingFinished) Debug.Log("Download finished!"); 
}); 
request.UseStreaming = true; 
request.StreamFragmentSize = 1 * 1024 * 1024; // 1 megabyte
request.DisableCache = true; // already saving to a file, so turn off caching request.Send();  
```
>>>
        1):我们将标志 - UseStreaming - 切换为true，因此我们的回调可能被调用多次。
        2):StreamFragmentSize指示在调用回调之前我们想要在内存中缓冲的最大数据量。
        3):每次下载StreamFragmentSize大小的块时都会调用我们的回调，并且当IsStreamingFinished设置为true时再调用一次。
        4):要获取下载的数据，我们必须使用GetStreamedFragments()函数。我们应该将结果保存在临时变量中，因为在此调用中清除了内部缓冲区(返回字节量,清除内存,写入文件)，因此连续调用将为我们提供null结果。
        5):我们在此示例中禁用了缓存，因为我们已经保存了下载的缓存,保存在一个磁盘文件中，并且我们不想占用太多空间。
>>>
下载进度
```
var request = new HTTPRequest(new Uri(address), OnFinished); 
request.OnProgress = OnDownloadProgress; 
request.Send(); 
void OnDownloadProgress(HTTPRequest request, int downloaded, int length) 
{ 
        float progressPercent = (downloaded / (float)length) * 100.0f;
        Debug.Log("Downloaded: " + progressPercent.ToString("F2") + "%"); 
} 
```

* 7:上传流媒体(Upload Streaming)
使用HTTPRequest对象的UploadStream属性设置上传的数据流Stream。当上传完成并且DisposeUploadStream为true时，插件将调用Stream流上的Dispose()函数。如果流的长度未知，则UseUploadStreamLength属性应设置为false。在这种情况下，插件将使用分块传输编码从流中发送数据:
```
var request = new HTTPRequest(new Uri(address), HTTPMethods.Post, OnUploadFinished); 
request.UploadStream = new FileStream("File_To.Upload", FileMode.Open); 
request.Send(); 
```
上传进度回调,要跟踪和显示上传进度，您可以使用HTTPRequest类的OnUploadProgress事件。 OnUploadProgress可以与RawData，表单（通过AddField和AddBinaryData）以及UploadStream一起使用。

```
var request = new HTTPRequest(new Uri(address), HTTPMethods.Post, OnFinished); 
request.RawData =  Encoding.UTF8.GetBytes("Field Value");
request.OnUploadProgress = OnUploadProgress; 
request.Send(); 
void OnUploadProgress(HTTPRequest request, long uploaded, long length)
{ 
        float progressPercent = (uploaded / (float)length) * 100.0f;
        Debug.Log("Uploaded: " + progressPercent.ToString("F2") + "%"); 
}
```

* 8:缓存

缓存也基于HTTP / 1.1 RFC。它使用标头来存储和验证响应。缓存机制在幕后工作，我们唯一要做的就是决定是否要启用或禁用它。如果缓存的响应具有带有未来日期的“Expires”标头，则BestHTTP将使用缓存的响应，而不对服务器进行验证。这意味着我们不必启动与服务器的任何tcp连接。这可以节省我们的时间，带宽和离线工作。 ***(这段话的意思相当于本地数据缓存,请求服务器,不会真的连接服务器,会从本地取出原来已经请求过的数据,只要这个数据没有过期,则直接返回给使用者)***

虽然缓存是自动的，但我们可以控制它，或者我们可以使用HTTPCacheService类的公共函数获取一些信息：
>>>
        1):BeginClear(),它将开始在单独的线程上清除整个缓存
        2):BeginMaintainence(),有了这个函数的帮助，我们可以根据上次访问时间删除缓存的条目。它删除上次访问时间早于指定时间的条目。我们还可以使用此函数来控制缓存大小：
                HTTPCacheService.BeginMaintainence(new HTTPCacheMaintananceParams(TimeSpan.FromDays(14), 50 * 1024 * 1024)); 
        3):GetCacheSize(),将以字节为单位返回缓存的大小。
        4):GetCacheEntryCount(),将返回缓存中存储的条目数。可以使用float avgSize = GetCacheSize()/（float）GetCacheEntryCount()公式计算平均缓存条目大小。
>>>

* 9:Cookie ,处理cookie操作对程序员来说是透明的。设置请求Cookie标头以及解析和维护响应的Set-Cookie标头由插件自动完成。有很多关于cookie的Global Settings 。有关详细信息，请参阅 [Global Settings](#13) 部分。
它可以以各种方式控制
>>>
        1):使用每一次的请求对象HTTPRequest.IsCookiesEnabled属性以及全局的HTTPManager.IsCookiesEnabled属性来禁用
        2):可以通过调用CookieJar.Clear()函数从Cookie Jar中删除Cookie
        3):可以通过响应的Cookies属性访问从服务器发送的新cookie。
>>>
可以通过将Cookie添加到Cookie列表中将Cookie添加到HTTPRequest：
```
var request = new HTTPRequest(new Uri(address), OnFinished);
request.Cookies.Add(new Cookie("Name", "Value")); 
request.Send(); 
```
这些cookie将与服务器发送的cookie合并。如果在请求或HTTPManager中将IsCookiesEnabled设置为false，则仅发送这些用户设置的cookie

* 10:代理,HTTPProxy对象可以设置为HTTPRequest的Proxy属性。这样，请求将通过给定的代理。
```
request.Proxy = new HTTPProxy(new Uri("http://localhost:3128")); 
```
您也可以设置全局代理，因此您不必手动将其设置为所有请求。请参阅[Global Settings](#13)一章

* 11:终止请求,您可以通过调用HTTPRequest对象的Abort（）函数来中止正在进行的请求
```
request = new HTTPRequest(new Uri("http://yourserver.com/bigfile"), (req, resp) => { ... }); 
request.Send(); 
// And after some time: 
request.Abort(); //将调用回调函数，并且响应对象(resp)将为null。
```
* 12:超时,2 种情况,第一种是:ConnectTimeout,使用此属性，您可以控制等待在应用程序和远程服务器之间建立连接的时间。其默认值为20秒
```
request = new HTTPRequest(new Uri("http://yourserver.com/"), (req, resp) => { ... }); 
request.ConnectTimeout = TimeSpan.FromSeconds(2); 
request.Send(); 
```
第二种:Timeout 使用此属性，您可以控制等待处理请求的时间（此时已连接到服务器,正在等待服务器响应,发送请求和下载响应）。其默认值为60秒。
```
request = new HTTPRequest(new Uri("http://yourserver.com/"), (req, resp) => { ... }); 
request.Timeout = TimeSpan.FromSeconds(10); 
request.Send(); 



string url = "http://besthttp.azurewebsites.net/api/LeaderboardTest?from=0&count=10"; 
HTTPRequest request = new HTTPRequest(new Uri(url), (req, resp) => 
 { 
         switch (req.State) 
         { 
                 // The request finished without any problem. 
                 case HTTPRequestStates.Finished: 
                        Debug.Log("Request Finished Successfully!\n" + resp.DataAsText); 
                        break; 
                        // The request finished with an unexpected error. 
                // The request's Exception property may contain more information about the error. 
                case HTTPRequestStates.Error: 
                        Debug.LogError("Request Finished with Error! " + (req.Exception != null ? (req.Exception.Message + "\n" + req.Exception.StackTrace) : "No Exception")); 
                break; 
                // The request aborted, initiated by the user. 
                case HTTPRequestStates.Aborted: 
                        Debug.LogWarning("Request Aborted!"); 
                break; 
                // Ceonnecting to the server timed out. 
                case HTTPRequestStates.ConnectionTimedOut: 
                        Debug.LogError("Connection Timed Out!"); 
                break; 
                // The request didn't finished in the given time. 
                case HTTPRequestStates.TimedOut: 
                        Debug.LogError("Processing the request Timed Out!"); break; 
        } 
}); 
// Very little time, for testing purposes: 
request.ConnectTimeout = TimeSpan.FromMilliseconds(2); 
request.Timeout = TimeSpan.FromSeconds(5); 
request.IsKeepAlive = false; 
request.DisableCache = true; 
request.Send(); 
```

* 13:Request States 请求状态,所有请求都有一个包含其内部状态的State属性。可能的状态如下：
>>>
        1):Initial:请求的初始状态。使用此状态不会调用任何回调
        2):Queued:在队列中等待处理。使用此状态不会调用任何回调
        3):Processing:开始处理请求。在此状态下，客户端将发送请求，并解析响应。使用此状态不会调用任何回调。
        4):Finished:请求完成没有问题。解析完成的响应后，可以使用结果。将使用有效的响应对象调用用户定义的回调。请求的Exception属性将为null。
        5):Error:请求在插件中以意外错误结束。将使用null响应对象调用用户定义的回调。请求的Exception属性可能包含有关错误的更多信息，但它可以为null。
        6):Aborted: 请求由客户端中止（HTTPRequest的Abort（）函数）。将使用null响应调用用户定义的回调。请求的Exception属性将为null。
        7):ConnectionTimedOut：连接到服务器超时。将使用null响应调用用户定义的回调。请求的Exception属性将为null。
        8):TimedOut：请求未在给定时间内完成。将使用null响应调用用户定义的回调。请求的Exception属性将为null。
>>>

* 14:请求的优先级Request Priority ,可以通过HTTPRequest的Priority属性更改请求的优先级。与较低优先级请求相比，将从请求队列中选择更高优先级的请求。
```
var request = new HTTPRequest(new Uri("https://google.com"), ...);
request.Priority = -1; 
request.Send(); 
```
* 15:服务器证书验证,Server Certificate  Validation 可以通过实现ICertificateVerifyer接口并将其设置为HTTPRequest的CustomCertificateVerifyer来验证服务器发送的证书：
```
using System; 
using Org.BouncyCastle.Crypto.Tls; 
using Org.BouncyCastle.Asn1.X509; 
class CustomVerifier : ICertificateVerifyer 
{ 
        public bool IsValid(Uri serverUri, X509CertificateStructure[] certs) 
        { 
        // TODO: Return false, if validation fails return true; 
        } 
} 
var request = new HTTPRequest(new Uri("https://google.com"), ...);
request.CustomCertificateVerifyer = new CustomVerifier(); 
request.UseAlternateSSL = true; 
request.Send(); 
```
* 16:控制重定向,Control Redirections ,重定向由插件自动处理，但有时我们必须在向我们重定向到的uri发出新请求之前进行更改。我们可以在HTTPRequest的OnBeforeRedirection事件处理程序中进行这些更改。在插件向新uri发出新请求之前调用此事件。函数的返回值将控制重定向：如果为false，则重定向将被中止,子线程中调用
```
var request = new HTTPRequest(uri, HTTPMethods.Post); 
request.AddField("field", "data"); 
request.OnBeforeRedirection += OnBeforeRedirect; 
request.Send(); 
bool OnBeforeRedirect(HTTPRequest req, HTTPResponse resp, Uri redirectUri) 
{ 
        if (req.MethodType == HTTPMethods.Post && resp.StatusCode == 302) 
        { 
                req.MethodType = HTTPMethods.Get; 
                // Don't send more data than needed. 
                // So we will delete our already processed form data.
                req.Clear(); 
        } 
        return true; 
} 
```
* 17:统计(Statistics),您可以使用HTTPManager.GetGeneralStatistics函数获取有关底层插件的一些统计信息：
```
GeneralStatistics stats = HTTPManager.GetGeneralStatistics(StatisticsQueryFlags.All); Debug.Log(stats.ActiveConnections); 
```
>>>
        1):Connections：将返回基于连接的统计信息。这些是以下内容：
                A:RequestsInQueue：队列中等待空闲连接的请求数。
                B:Connections：插件跟踪的HTTPConnection实例数。这是以下所有连接的总和
                B:ActiveConnections：活动连接数。这些连接当前正在处理请求。
                C:FreeConnections：免费连接数。这些连接完成了请求，他们正在等待另一个请求或回收。
                D:RecycledConnections：回收连接数。这些连接将尽快删除。
        2):Cache：基于缓存的统计信息这些是以下内容：
                A:CacheEntityCount：缓存响应的数量。
                B:CacheSize：缓存响应的总和大小
        3):Cookie：基于Cookie的统计信息。这些是以下内容
                A:CookieCount：Cookie Jar中的Cookie数量
                B:CookieJarSize：Cookie Jar中Cookie的总和大小
>>>


## Global Settings 
* 1:使用以下属性，我们可以更改一些默认值，否则应在HTTPRequest的构造函数中指定。因此，大多数这些属性都是节省时间的快捷方式。
* 2:这些更改将影响其值更改后创建的所有请求。可以通过HTTPManager类的静态属性更改默认值:
>>>
        1):MaxConnectionPerServer：允许唯一主机的连接数。 http://example.org和https://example.org被视为两个独立的服务器。默认值为4。
        2):KeepAliveDefaultValue：HTTPRequest的IsKeepAlive属性的默认值。如果IsKeepAlive为false，则将在每个请求之前设置与服务器的tcp连接，并在其之后立即关闭。如果连续请求很少，则应将其更改为false。赋予HTTPRequest构造函数的值将仅覆盖此请求的此值。默认值是true。
        3):IsCachingDisabled：使用此属性，我们可以全局禁用或启用缓存服务。赋予HTTPRequest构造函数的值将仅覆盖此请求的此值。默认值是true。
        4):MaxConnectionIdleTime：指定BestHTTP在完成最后一次请求后销毁连接之前应等待的空闲时间。默认值为2分钟。
        5):IsCookiesEnabled：使用此选项，可以启用或禁用所有Cookie操作。默认值是true。
        6):CookieJarSize：使用此选项可以控制Cookie存储的大小。默认值为10485760（10 MB）.
        7):EnablePrivateBrowsing：如果启用此选项，则不会将Cookie写入磁盘。默认值为false
        8):ConnectTimeout：使用此选项，您可以设置HTTPRequests的默认ConnectTimeout值。默认值为20秒。
        9):RequestTimeout：使用此选项，您可以设置HTTPRequests.Timeout的默认超时值。默认值为60秒。
        10):RootCacheFolderProvider：默认情况下，插件会将所有缓存和cookie数据保存在Application.persistentDataPath返回的路径下。您可以为此委托指定一个函数，以返回自定义根路径以定义新路径。这个代理将在子线程上调用！
        11):Proxy：所有HTTPRequests的全局默认代理。 HTTPRequest的代理仍然可以按请求进行更改。默认值为null
        12):Logger：ILogger实现，能够控制将记录有关插件内部的信息，以及如何记录这些信息
        13):DefaultCertificateVerifyer：可以将ICertificateVerifyer实现设置为此属性。之后创建的所有新请求将在使用安全协议且请求的UseAlternateSSL为true时使用此验证程序。 ICertificateVerifyer实现可用于实现服务器证书验证。
        14):UseAlternateSSLDefaultValue：可以通过此属性更改HTTPRequest的UseAlternateSSL的默认值。
        15):HTTPManager.MaxConnectionPerServer = 10; HTTPManager.RequestTimeout = TimeSpan.FromSeconds(120); 
>>>

## 关于线程
* 1:因为插件内部使用线程并行处理所有请求，所以所有共享资源（缓存，cookie等）都是在设计和实现线程安全时考虑的。
* 2:调用请求的回调函数和所有其他回调（如WebSocket的回调）都是在Unity的主线程上回调（如Unity的事件：awake, start, update, etc），因此您不必进行任何线程同步。
* 3:在多个线程上创建，发送请求也是安全的，但是你应该调用BestHTTP.HTTPManager.Setup();在从Unity的一个事件（例如，awake，start）发送任何请求之前发挥作用。