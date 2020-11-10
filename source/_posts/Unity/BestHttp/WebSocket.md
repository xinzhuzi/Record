---
title: BestHttp介绍2
date: 2020-05-11 11:41:32
top: 101
categories:
- Unity
tags:
- BestHttp
---

# WebSocket

## 介绍
* 1:我们可以通过WebSocket类使用WebSocket功能。我们只需要将服务器的Uri传递给WebSocket的构造函数

* 2:创建


```
        var webSocket = new WebSocket(new Uri("wss://html5labs-interop.cloudapp.net/echo")); 

```
* 3:,OnOpen事件：在建立与服务器的连接时调用。在此事件回调之后，WebSocket的IsOpen属性将为True，直到我们或服务器关闭连接或发生错误。

```
        webSocket.OnOpen += OnWebSocketOpen; 
        private void OnWebSocketOpen(WebSocket webSocket) { Debug.Log("WebSocket Open!"); }
```


* 4:,OnMessage事件：从服务器收到文本消息时调用。

```
        webSocket.OnMessage += OnMessageReceived; 
        private void OnMessageReceived(WebSocket webSocket, string message) { Debug.Log("Text Message received from server: " + message); } 

```


* 5:,OnBinary事件：从服务器收到二进制blob消息时调用。


```
        webSocket.OnBinary += OnBinaryMessageReceived; 
        private void OnBinaryMessageReceived(WebSocket webSocket, byte[] message) { Debug.Log("Binary Message received from server. Length: " + message.Length); }

```


* 6:,OnClosed事件：在客户端或服务器关闭连接时调用，或发生内部错误。当客户端通过Close函数关闭连接时，它可以提供代码和消息，指示关闭的原因。服务器通常会回复我们的代码和消息。


```
        webSocket.OnClosed += OnWebSocketClosed; 
        private void OnWebSocketClosed(WebSocket webSocket, UInt16 code, string message) { Debug.Log("WebSocket Closed!");}

```


* 7:OnError事件：当我们无法连接到服务器时调用，发生内部错误或连接丢失。第二个参数是Exception对象，但它可以为null。在这种情况下，检查WebSocket的InternalRequest应该告诉更多有关该问题的信息。


```
        webSocket.OnError += OnError; 
        private void OnError(WebSocket ws, Exception ex) 
        { 
                string errorMsg = string .Empty; 
                if (ws.InternalRequest.Response != null)
                {
                        errorMsg = string.Format("Status Code from Server: {0} and Message: {1}", ws.InternalRequest.Response.StatusCode, ws.InternalRequest.Response.Message); 
                }
                Debug.Log("An error occured: " + (ex != null ? ex.Message : "Unknown: " + errorMsg)); 
        } 

```


* 8:OnErrorDesc事件：一个更具信息性的事件，此事件在OnError事件之后调用,因为后者仅使用Exception参数调用。但它可以提供更详细的错误报告。


```
        webSocket.OnErrorDesc += OnErrorDesc; 
        void OnErrorDesc(WebSocket ws, string error) { Debug.Log("Error: " + error); } 

```


* 9:在我们将所有事件注册完备之后，我们可以开始连接：

```
        webSocket.Open(); 

```


在此步骤之后，我们将收到一个OnOpen事件的回调，我们可以开始向服务器发送消息。


```
        // 发送字符串: 
        webSocket.Send("Message to the Server"); 

        // 创建二进制流,并填充: 
        byte[] buffer = new byte[length]; 
        //发送二进制流 
        webSocket.Send(buffer); 

```

完成通信后/不需要的时候,关闭链接,无法重用已关闭的WebSocket实例。

```
        webSocket.Close(); 

```
* 10:Ping消息：通过在收到OnOpen事件之前将StartPingThread属性设置为True，可以启动新线程将Ping消息发送到服务器。这样，Ping消息将定期发送到服务器。可以在PingFrequency属性中设置两次ping之间的延迟（默认值为1000ms）.(相当于设置心跳包)
* 11:Pong消息：从插件服务器收到的所有ping消息将自动生成Pong应答。
* 12:Streaming：较长的文本或二进制消息将变得支离破碎。默认情况下，这些片段由插件自动组装。如果我们向WebSocket的OnIncompleteFrame事件注册事件处理程序，则可以覆盖此机制。每次客户端收到不完整的片段时都会调用此事件。这些片段将被插件忽略，它不会尝试组装这些片段，也不会存储它们。此事件可用于实现流式传输体验。(自定义组装消息).

## 使用Socket.IO
* 1:Socket.IO实现使用插件已有的功能。当轮询传输与其所有功能（cookie，连接重用等）一起使用时，它将发送HTTPRequests以获取握手数据，发送和接收数据包。 WebSocket实现用于WebSocket传输
>>>
        1):易于使用和熟悉的api
        2):兼容最新的Socket.IO规范
        3):从轮询传输到websocket传输的无缝升级
        4):断开时自动重新连接
        5):简单高效的二进制数据发送和多种接收方式
        6):在高级模式下使用它的强大工具（切换默认编码器，禁用自动解码等）
>>>
* 2:使用.如果要连接到Socket.IO服务，可以使用BestHTTP.SocketIO.SocketManager类来完成。首先，您必须创建一个SocketManager实例

```
        using System; using BestHTTP; 
        using BestHTTP.SocketIO; 
        var manager = new SocketManager(new Uri("http://chat.socket.io/socket.io/")); 
```

* 3:Url中的/socket.io/路径非常重要，默认情况下，Socket.IO服务器将侦听此查询。所以不要忘记测试！
* 4:Connecting to namespaces ,默认情况下，SocketManager将在连接到服务器时连接到根（“/”）命名空间。您可以通过SocketManager的Socket属性访问它：

```
        Socket root = manager.Socket; 
```

可以通过GetSocket（'/ nspName'）函数或通过manager的indexer属性访问非默认名称空间：

```
        Socket nsp = manager["/customNamespace"]; 
        // 等价于: 
        Socket nsp = manager.GetSocket("/customNamespace"); 
```


首次访问命名空间将启动内部连接过程
* 4:Subscribing  and receiving events ,您可以订阅预定义和自定义事件。预定义事件是“连接”，“连接”，“事件”，“断开连接”，“重新连接”，“重新连接”，“重新连接”，“重新连接失败”，“错误”。("connect", "connecting", "event", "disconnect", "reconnect", "reconnecting", "reconnect_attempt", "reconnect_failed", "error". )自定义事件是程序员定义的事件，服务器将发送给您的客户端。您可以通过调用套接字的On函数来订阅事件：
```
        manager.Socket.On("login", OnLogin); 
        manager.Socket.On("new message", OnNewMessage); 

        void OnLogin(Socket socket, Packet packet, params object[] args) 
        { 
                //Socket参数将是服务器发送此事件的namespace-socket对象
                //Packet参数包含事件的内部分组数据。数据包可用于访问服务器发送的二进制数据，或使用自定义Json解析器lib解码有效负载数据。稍后会详细介绍。
                //Args参数是一个可变长度数组，包含来自数据包有效负载数据的解码对象。使用默认的Json编码器，这些参数可以是“原始”类型（int，double，string）或对象列表（List对象）或Dictionary字符串，对象对象。
        } 

```
```
        //服务器上面的代码写法,在一个 node.js 的服务器上面
        socket.emit('message', ‘MyNick’, ‘Msg to the client’); 
        //客户端接收
        // subscribe to the "message" event 
        manager.Socket.On("message", OnMessage); 
        // event handler 
        void OnMessage(Socket socket, Packet packet, params object[] args) 
        { 
                // args[0] is the nick of the sender 
                // args[1] is the message 
                Debug.Log(string.Format("Message from {0}: {1}", args[0], args[1])); 
        } 

```
>>>
        ●“connect”：命名空间打开时发送。 
        ●“connecting”：当SocketManager开始连接到socket.io服务器时发送。 
        ●“event”：在自定义（程序员定义的）事件上发送。 
        ●“disconnect”：当传输断开，SocketManager关闭，Socket关闭或在握手数据中指定的给定时间内没有从服务器收到Pong消息时发送。 
        ●“reconnect”：插件成功重新连接到socket.io服务器时发送。 
        ●“reconnecting”：当插件尝试重新连接到socket.io服务器时发送。 
        ●“reconnect_attempt”：当插件尝试重新连接到socket.io服务器时发送。 
        ●“reconnect_failed”：重新连接尝试无法连接到服务器并且ReconnectAttempt达到选项“ReconnectionAttempts”值时发送。 
        ●“error”：在服务器或内部插件错误上发送。事件的唯一参数是BestHTTP.SocketIO.Error对象。
        ● Once：您可以订阅仅被调用一次的事件。manager.Socket.Once("connect", OnConnected); 
        ● Off:您可以删除所有活动订阅，或只删除一个
        // 删除所有的回调事件
        manager.Socket.Off(); 
        //从"connect"事件中删除所有回调
        manager.Socket.Off("connect"); 
        //从"connect"事件中删除OnConnected回调
        manager.Socket.Off("connect", OnConnected); 
>>>

* 5:Sending events ,您可以使用“Emit”功能发送事件。您必须将事件名称作为第一个参数和可选的其他参数传递。这些将被编码为json并将被发送到服务器。您可以选择设置一个回调函数，该函数将在服务器处理事件时被调用（您必须正确设置服务器代码才能发回回调函数。有关更多信息，请参阅Socket.IO服务器端文档）。
```
        // 发送携带 2 个参数的事件给服务器
        manager.Socket.Emit("message", "userName", "message"); 

        // 发送携带 2 个参数的并有回调事件的事件给服务器
        manager.Socket.Emit("custom event", OnAckCallback, "param 1", "param 2"); 
        void OnAckCallback(Socket socket, Packet originalPacket, params object[] args) { Debug.Log("OnAckCallback!"); } 
```
您可以通过调用套接字的EmitAck函数向服务器发回确认。您必须传递原始数据包和任何可选数据,您可以保留对数据包的引用，并从其他位置调用EmitAck:
```
        manager["/customNamespace"].On("customEvent", (socket, packet, args) => { socket.EmitAck(packet, "Event", "Received", "Successfully"); }); 
```
* 6:发送二进制数据有 2 种方法
1):通过传递给Emit函数，插件将扫描参数，如果找到参数，它将把它转换为二进制附件（如Socket.IO 1.0中所介绍的）。这是最有效的方法，因为它不会将字节数组转换为客户端的Base64编码字符串，并在服务器端转换为二进制。
```
        byte[] data = new byte[10]; 
        manager.Socket.Emit("eventWithBinary", "textual param", data); 
```
2):如果二进制数据作为字段或属性嵌入对象中，则Json编码器必须支持转换。默认的Json编码器无法将嵌入的二进制数据转换为Json，您必须使用更高级的Json解析器库（如'JSON .NET For Unity' - http://u3d.as/5q2）
* 7:接收二进制数据
在Socket.IO服务器中，当二进制数据发送到客户端时，它将用Json对象（{'_ placeholder'：true，'num'：xyz}）替换数据，并将二进制数据发送到另一个数据包中。在客户端，这些数据包将被收集并合并到一个数据包中。二进制数据将位于数据包的Attachments属性中。
1):在这里你也可以选择使用这个数据包：
在事件处理程序中，您可以通过数据包的Attachments属性访问所有二进制数据,autoDecodePayload默认为 true
```
        Socket.On("frame", OnFrame); 
        void OnFrame(Socket socket, Packet packet, params object[] args) { texture.LoadImage(packet.Attachments[0]); }
```
2):第二个选项与前一个选项几乎相同，略有改进：我们不会将发送的Json字符串解码为c＃对象。我们可以这样做，因为我们知道服务器只发送了二进制数据，此事件没有其他信息。因此，我们将让插件知道不解码有效负载
```
        //订阅“frame”事件，并将autoDecodePayload标志设置为false,不让插件自动解码
        Socket.On("frame", OnFrame, /*autoDecodePayload:*/ false); 
        void OnFrame(Socket socket, Packet packet, params object[] args) { texture.LoadImage(packet.Attachments[0]); } 

```
3):我们可以将'{'_placeholder'：true，'num'：xyz}'字符串替换为附件列表中附件的索引。
```
        Socket.On("frame", OnFrame, /*autoDecodePayload:*/ false); 
        void OnFrame(Socket socket, Packet packet, params object[] args) 
        { 
                //用索引替换Json对象
                packet.ReconstructAttachmentAsIndex(); 
                // 现在，将Payload解码为 object[]
                args = packet.Decode(socket.Manager.Encoder); 
                // args现在只包含一个索引号（可能为0） 
                byte[] data = packet.Attachments[Convert.ToInt32(args[0])]; texture.LoadImage(data); 
        } 
```
4):我们可以用附件中转换为Base64编码字符串的二进制数据替换'{'_ placeholder'：true，'num'：xyz}'字符串。当高级Json解析器必须将其设置为对象的字段或属性时，它可以将其转换为字节数组
```
        Socket.On("frame", OnFrame, /*autoDecodePayload:*/ false); 
        void OnFrame(Socket socket, Packet packet, params object[] args) 
        { 
                // 用Base64编码的字符串替换Json对象 packet.ReconstructAttachmentAsBase64(); 
                // 现在，将Payload解码为object[]
                args = packet.Decode(socket.Manager.Encoder); 
                // args现在包含一个Base64编码的字符串
                byte[] data = Convert.FromBase64String(args[0] as string); texture.LoadImage(data); 
        }
```
* 8:设置默认的Json编码器, 您可以通过将SocketManager的静态DefaultEncoder设置为新的编码器来更改默认的Json编码器。在此步骤之后，所有新创建的SocketManager将使用此编码器。或者，您可以直接将SocketManager对象的Encoder属性设置为编码器。
编写自定义Json编码器:如果由于各种原因想要更改默认的Json编码器，首先必须编写一个新的Json编码器。为此，您必须编写一个新类，该类从BestHTTP.SocketIO.JsonEncoders命名空间实现IJsonEncoder。剥离的IJsonEncoder非常小，你必须只实现两个功能：
```
        public interface IJsonEncoder 
        { 
                List<object> Decode(string json); 
                string Encode(List<object> obj); 
        } 
```
Decode函数必须将给定的json字符串解码为对象列表。由于Socket.IO协议的性质，发送的json是一个数组，第一个元素是事件的名称。Encode函数用于编码客户端要发送给服务器的数据。此列表的结构与Decode相同：列表的第一个元素是事件的名称，任何其他元素是用户发送的参数。例子:
```
        using LitJson; 
        public sealed class LitJsonEncoder : IJsonEncoder 
        { 
                public List<object> Decode(string json) 
                { 
                        JsonReader reader = new JsonReader(json); 
                        return JsonMapper.ToObject<List<object>>(reader); 
                } 
                public string Encode(List<object> obj) 
                { 
                        JsonWriter writer = new JsonWriter(); 
                        JsonMapper.ToJson(obj, writer); 
                        return writer.ToString(); 
                } 
        } 
```

* 9:AutoDecodePayload属性,
已经在“接收二进制数据”中讨论过AutoDecodePayload，但是您不仅可以按event设置此值，还可以设置每个socket的值。socket具有AutoDecodePayload属性，该属性用作事件订阅的默认值。其默认值为true - 所有Payload都已解码并分派给事件订阅者。如果设置为false，插件将不进行解码，您必须自己完成。
你不想每次都抛出args：当然！您可以在Socket对象上设置AutoDecodePayload，并且可以使用您喜欢的Json解析器将Packet的Payload解码为强类型对象。但请记住，Payload将包含事件的名称，它是一个json数组。示例Payload如下所示：'['eventName'，{'field'：'stringValue'}，{'field'：1.0}]'。

* 10:Error handling  发生服务器端或客户端错误时发出“错误”事件。事件的第一个参数是Error对象。这将包含Code属性中的错误代码和Message属性中的字符串消息。此类中的ToString（）函数已被重写，您可以使用此函数写出其内容。
```
        Socket.On(SocketIOEventTypes.Error, OnError); 
        void OnError(Socket socket, Packet packet, params object[] args) 
        { 
                Error error = args[0] as Error; 
                switch (error.Code) 
                { 
                        case SocketIOErrors.User: 
                                Debug.Log("Exception in an event handler!"); 
                        break; 
                        case SocketIOErrors.Internal: 
                                Debug.Log("Internal error!"); 
                        break; 
                        default: 
                                Debug.Log("Server error!"); break; 
                } 
                Debug.Log(error.ToString()); 
        } 
```

* 11:SocketOptions类中的可用选项,您可以将SocketOptions实例传递给SocketManager的构造函数。您可以更改以下选项：
>>>
        1):Reconnection：断开连接后是否自动重新连接。其默认值为true
        2):ReconnectionAttempts：放弃前的尝试次数。它的默认值是Int.MaxValu
        3):ReconnectionDelay：在尝试重新连接之前最初等待的时间。受+/- RandomizationFactor影响。例如，默认初始延迟将在500ms到1500ms之间。其默认值为10000毫秒。
        4):ReconnectionDelayMax：重新连接之间等待的最长时间。如上所述，每次尝试都会增加重新连接延迟以及随机化。其默认值为5000毫秒。
        5):RandomizationFactor：它可用于控制ReconnectionDelay范围。其默认值为0.5，可以在0..1值之间设置
        6)Timeout:发出“connect_error”和“connect_timeout”事件之前的连接超时。它不是底层tcp套接字的连接超时，而是socket.io协议。其默认值为20000ms
        7):AutoConnect：通过将此设置为false，您必须在决定适当时调用SocketManager的Open（）。
        8):ConnectWith：So​​cketManager将尝试连接到此属性的传输集。它可以是TransportTypes.Polling或TransportTypes.WebSocket
>>>

# SignalR
* 1:像Socket.IO这样的SignalR实现使用了插件的基本功能。 HTTPRequests和WebSockets用于连接和通信连接池。 Cookie随请求一起发送，记录器用于记录有关协议和错误的信息,SignalR实现的功能简要列表：
>>>
        1):兼容最新的SignalR服务器实现
        2):好用的 API
        3):传输回调
        4):重新连接逻辑
        5):支持所有Hub功能
>>>
```
        using BestHTTP.SignalR;
        Uri uri = new Uri("http://besthttpsignalr.azurewebsites.net/raw-connection/");
        //通过仅将服务器的uri传递给构造函数来创建没有集线器的连接。
        Connection signalRConnection = new Connection(uri); 
        //通过将集线器名称传递给构造函数来创建与集线器的连接。
        Connection signalRConnection = new Connection(uri, "hub1", "hub2", "hubN"); 
        //通过将Hub对象传递给构造函数来创建与Hub的连接。
        Hub hub1 = new Hub("hub1"); 
        Hub hub2 = new Hub("hub2"); 
        Hub hubN = new Hub("hubN"); 
        Connection signalRConnection = new Connection(uri, hub1, hub2, hubN); 
        //创建Connection之后，我们可以通过调用Open（）函数开始连接到服务器
        signalRConnection.Open(); 
```
* 2:Handling general events Connection类允许您订阅多个事件。这些事件如下：
```
        //OnConnected：当连接类成功连接并且SignalR协议用于通信时，将触发此事件。
        signalRConnection.OnConnected += (con) => Debug.Log("Connected to the SignalR server!"); 

        //OnClosed：当SignalR协议关闭时，将触发此事件，并且不再发送或接收更多消息。
        signalRConnection.OnClosed += (con) => Debug.Log("Connection Closed"); 
        //OnError：发生错误时调用。如果连接已打开，插件将尝试重新连接，否则连接将关闭。
        signalRConnection.OnError += (conn, err) => Debug.Log("Error: " + err); 

        //OnReconnecting：启动重新连接尝试时会触发此事件。在此事件之后，将调用OnError或OnReconnected事件。可以在OnReconnected / OnClosed事件之前触发多个OnReconnecting-OnError事件对，因为插件将尝试在给定时间内多次重新连接。
        signalRConnection.OnReconnecting += (con) => Debug.Log("Reconnecting"); 

        //OnReconnected：重新连接尝试成功时触发。
        signalRConnection.OnReconnecting += (con) => Debug.Log("Reconnected"); 
        //OnStateChnaged：连接状态发生变化时触发。事件处理程序将同时接收旧状态和新状态。
        signalRConnection.OnStateChanged += (conn, oldState, newState) => Debug.Log(string.Format("State Changed {0} -> {1}", oldState, newState)); 

        //OnNonHubMessage：当服务器向客户端发送非集线器消息时触发。客户端应该知道服务器期望的消息类型，并且应该相应地转换接收的对象。
        signalRConnection.OnNonHubMessage + =（con，data）= Debug.Log（'来自服务器的消息：'+ data.ToString（））;

        //RequestPreparator：为每个发出并将发送到服务器的HTTPRequest调用此委托。它可用于进一步自定义请求。
        signalRConnection.RequestPreparator = (con, req, type) => req.Timeout = TimeSpan.FromSeconds(30); 

```

* 3:Sending non-Hub  messages 
```
        //将非集线器消息发送到服务器很容易，因为调用连接对象上的函数：
        signalRConnection.Send(new { Type = "Broadcast", Value = "Hello SignalR World!" }); 

        //此函数将使用Connection的JsonEncoder将给定对象编码为Json字符串，并将其发送到服务器。已编码的Json字符串可以使用SendJson函数发送
        signalRConnection.SendJson("{ Type: ‘Broadcast’, Value: ‘Hello SignalR World!’ }"); 
```

* 4:Hubs,为了在客户端上定义Hub可以从服务器调用的方法，并调用a上的方法
服务器上的集线器必须将集线器添加到Connection对象。这可以通过将集线器名称或集线器实例添加到Connection构造函数来完成，在“连接类”部分中进行了演示
```
        //可以通过索引或名称通过Connection对象访问Hub实例。
        Hub hub = signalRConnection[0]; 
        Hub hub = signalRConnection["hubName"]; 

        // 注册服务器可调用方法,要处理服务器可调用方法调用，我们必须调用集线器的On函数：
        signalRConnection["hubName"].On("joined", Joined); 
        void Joined(Hub hub, MethodCallMessage msg) { Debug.log(string.Format("{0} joined at {1}", msg.Arguments[0], msg.Arguments[1])); }
```
MethodCallMessage是服务器发送的对象，包含以下属性：
>>>
        Hub：包含方法必须调用的集线器名称的字符串。
        Method：包含方法名称的字符串
        Arguments：包含方法调用参数的对象数组。它可以是一个空数组。
        State：包含其他自定义数据的字典
>>>
该插件将使用Hub和Method属性将消息路由到正确的集线器和事件处理程序。处理方法调用的函数只能使用Arguments和State属性。

* 5:Call server-side methods 
调用服务器端方法可以通过调用Hub的Call函数来完成。调用函数重载以满足每个需求。 Call函数是非阻塞函数，它们不会阻塞，直到服务器发回有关该调用的任何消息。
* 6:重载函数:
Call（string method，params object [] args）：这可以用来以一种即发即弃的方式调用服务器端函数。我们不会收到有关方法调用成功或失败的任何消息。可以在没有任何'args'参数的情况下调用此函数来调用无参数方法
```
        //在没有任何参数的情况下调用服务器端函数
        signalRConnection["hubName"].Call("Ping"); 
        //使用两个字符串参数调用服务器端函数：'param1'和'param2'
        signalRConnection["hubName"].Call("Message", "param1", "param2"); 
```
Call（string method ，OnMethodResultDelegate onResult，params object [] args）：此函数可以用作前一个函数，但是函数可以作为第二个参数传递，该参数将在成功调用服务器端函数时调用。
```
        signalRConnection["hubName"].Call("GetValue", OnGetValueDone); 
        void OnGetValueDone(Hub hub, ClientMessage originalMessage, ResultMessage result) { Debug.Log("GetValue executed on the server. Return value of the function:" + result.ReturnValue.ToString()); } 
```
此回调函数接收调用此函数的Hub，发送到服务器的原始ClientMessage消息以及由于方法调用而由服务器发送的ResultMessage实例。 ResultMessage对象包含ReturnValue和State属性。               
如果方法的返回类型为void，则ReturnValue为null.
Call（string method，OnMethodResultDelegate onResult，OnMethodFailedDelegate onError，params object [] args）：此函数可用于指定当方法无法在服务器上运行时将调用的回调。由于方法调用中存在未找到的方法，错误的参数或未处理的异常，因此可能会发生故障
```
        signalRConnection["hubName"].Call("GetValue", OnGetValueDone, OnGetValueFailed); 
        void OnGetValueFailed(Hub hub, ClientMessage originalMessage, FailureMessage error) 
        { 
                Debug.Log("GetValue failed. Error message from the server: " + error.ErrorMessage); 
        } 
```
 FailureMessage包含以下属性：
 >>>
        ○ IsHubError：如果是Hub错误，则为True。 
        ○ ErrorMessage：有关错误本身的简短消息。 
        ○ StackTrace：如果在服务器上打开了详细的错误报告，则它包含错误的堆栈跟踪。
        ○ AdditionalData：如果它不为null，则它包含有关错误的其他信息。
 >>>
 Call（string method，OnMethodResultDelegate onResult，OnMethodFailedDelegate onError，OnMethodProgressDelegate onProgress，params object [] args）：此函数可用于向服务器端方法调用添加其他进度消息处理程序。对于长时间运行的作业，服务器可以将进度消息发送到客户端。
 ```
        signalRConnection["hubName"].Call("GetValue", OnGetValueDone, OnGetValueFailed, OnGetValueProgress); 
        void OnGetValueProgress(Hub hub, ClientMessage originalMessage, ProgressMessage progress) 
        { 
                Debug.Log(string.Format("GetValue progressed: {0}%", progress.Progress)); 
        }
 ```
 当插件收到ResultMessage或FailureMessage时，它不会为这些消息之后的ProgressMessages提供服务。

 * 7:使用Hub类作为继承的基类,Hub类可以用作封装集线器功能的基类。
 ```
        class SampleHub : Hub 
        { 
                // 默认构造函数。每个集线器都必须有一个有效的名称. 
                public SampleHub() :base("SampleHub") 
                { 
                        // 注册服务器可调用函数 
                        base.On("ClientFunction", ClientFunctionImplementation); 
                }
                // 私有函数实现服务器可调用函数
                private void ClientFunctionImplementation(Hub hub, MethodCallMessage msg) 
                { 
                // TODO: implement 
                } 
                // 包装函数调用服务器端函数.
                public void ServerFunction(string argument) 
                { 
                        base.Call("ServerFunction", argument); 
                } 
        }
        //可以实例化此SampleHub并将其传递给Connection的构造函数：
        SampleHub sampleHub = new SampleHub(); Connection signalRConnection = new Connection(Uri, sampleHub); 

 ```

 * 8:Authentication
 Connection类具有AuthenticationProvider属性，可以将其设置为实现IAuthenticationProvider接口的对象,实现者必须实现以下属性和功能
 >>>
        ● bool IsPreAuthRequired：如果在Connection类向服务器发出任何请求之前必须运行身份验证，则返回true的属性。示例：cookie身份验证器必须返回false，因为它必须发送用户凭据并接收必须随请求一起发送的cookie。 
        ● StartAuthentication：仅在IsPreAuthRequired为true时才需要的函数。否则它不会被调用。 
        ● PrepareRequest：使用请求和请求类型枚举调用的函数。此函数可用于在将请求发送到服务器之前准备。 
        ● OnAuthenticationSucceded：IsPreAuthRequired为true且身份验证过程成功时必须调用的事件。 
        ● OnAuthenticationFailed：IsPreAuthRequired为true且身份验证过程失败时必须调用的事件。
 >>>
 一个非常简单的基于Header的身份验证器看起来像这样：
 ```
        class HeaderAuthenticator : IAuthenticationProvider 
        { 
                public string User { get; private set; } 
                public string Roles { get; private set; } 
                // 此类身份验证不需要预先验证步骤
                public bool IsPreAuthRequired { get { return false; } } 
                //未使用的事件，因为IsPreAuthRequired为false 
                public event OnAuthenticationSuccededDelegate OnAuthenticationSucceded; 
                //未使用的事件，因为IsPreAuthRequired为false
                public event OnAuthenticationFailedDelegate OnAuthenticationFailed; 
                // 使用用户名和角色初始化身份验证器的构造函数.
                public HeaderAuthenticator(string  user, string roles) 
                { 
                        this.User = user; this.Roles = roles; 
                } 
                //未使用的事件，因为IsPreAuthRequired为false             
                public void StartAuthentication() { } 
                // 通过向其添加两个标头来准备请求
                public void PrepareRequest(BestHTTP.HTTPRequest request, RequestTypes type) 
                { 
                        request.SetHeader("username", this.User); request.SetHeader("roles", this.Roles); 
                }
        }
 ```
 与Socket.IO的Manager类一样，SignalR的Connection类具有JsonEncoder属性，也可以设置静态Connection.DefaultEncoder。 JsonEncoder必须从BestHTTP.SignalR.JsonEncoders命名空间实现IJsonEncoder接口。该软件包包含一个LitJsonEncoder示例，也可用于某些示例

 ## Server-Sent Events

* 1:Server-Sent Events是一种基于字符串的单向协议。数据来自服务器，没有选项可以向服务器发送任何内容。它是使用最新的草案实现的。虽然协议的名称是Server-Sent Events，但类本身名为EventSource,发生错误时，一旦发送LastEventId，插件将尝试重新连接，让服务器发送任何我们应该收到的缓冲消息
```
        //The EventSource class 
        //EventSource类位于BestHTTP.ServerSentEvents命名空间中：
        using BestHTTP.ServerSentEvents; 
        var sse = new EventSource(new Uri("http://server.com")); 
```
* 2:Properties,这些是EventSource类的公开公开属性：
>>>
        ● Uri：这是协议尝试连接的端点。它是通过构造函数设置的。 
        ● State：EventSource对象的当前状态。 
        ● ReconnectionTime：等待尝试重新连接尝试的时间。它的默认值是2秒。 
        ● LastEventId：最后收到的事件的id。如果没有收到任何事件ID，它将为null。 
        ● InternalRequest：将在Open函数中发送的内部HTTPRequest对象。
>>>

* 3:事件
```
        //OnOpen：成功升级协议时调用它
        eventSource.OnOpen += OnEventSourceOpened; 
        void OnEventSourceOpened(EventSource source) { Debug.log("EventSource Opened!"); } 

        //OnMessage：当客户端从服务器收到新消息时调用它。此函数将接收一个Message对象，该对象包含Data属性中消息的有效内容。每次客户端收到消息时都会调用此事件，即使消息具有有效的事件名称，我们也为此事件分配了一个事件处理程序！
        eventSource.OnMessage += OnEventSourceMessage;
        void OnEventSourceMessage(EventSource source, Message msg) { Debug.log("Message: " + msg.Data); }

        // OnError：在连接到服务器或处理数据流时遇到错误时调用
        eventSource.OnError += OnEventSourceError; 
        void OnEventSourceError(EventSource source, string error) { Debug.log("Error: " + error); }

        //OnRetry：在插件尝试重新连接到服务器之前调用此函数。如果函数返回false，则不会进行任何尝试，并且将关闭EventSource。
        eventSource.OnRetry += OnEventSourceRetry; 
        bool OnEventSourceRetry(EventSource source) { // disable retry return false; }

        //OnClosed：当EventSource关闭时，将调用此事件。
        eventSource.OnClosed += OnEventSourceClosed; 
        void OnEventSourceClosed(EventSource source) { Debug.log("EventSource Closed!"); } 

        //OnStateChanged：每次State属性更改时调用。
        eventSource.OnStateChanged += OnEventSourceStateChanged; 
        void OnEventSourceStateChanged(EventSource source, States oldState, States newState) { Debug.log(string.Format("State Changed {0} => {1}", oldSate, newState))); }
```

* 4:Functions,这些是EventSource对象的公共函数。
```
        //Open: 调用此函数，插件将开始连接到服务器并升级到Server-Sent Events协议。
        EventSource eventSource = new EventSource(new Uri("http://server.com")); 
        eventSource.Open(); 

        // On:使用此功能，客户端可以订阅事件
        eventSource.On("userLogon", OnUserLoggedIn); 
        void OnUserLoggedIn(EventSource source, Message msg) { Debug.log(msg.Data); }

        //Off:它可用于取消订阅活动。
        eventSource.Off("userLogon"); 

        //Close: 此函数将开始关闭EventSource对象。
        eventSource.Close(); 
```
* 5:Message,Message类是一个逻辑单元，包含服务器可以发送的所有信息,Properties:
>>>
     ● Id：已发送事件的ID。如果没有发送id，则可以为null。它被插件使用。 
     ● 事件：事件的名称。如果没有发送事件名称，则可以为null。 
     ● 数据：消息的实际有效负载。 
     ● 重试：服务器发送插件在重新连接尝试之前应等待的时间。它被插件使用。
>>>

## 简单例子
● Upload a picture using forms 
```
        var request = new HTTPRequest(new Uri("http://server.com"), HTTPMethods.Post, onFinished); 
        request.AddBinaryData("image", texture.EncodeToPNG(), "image.png"); 
        request.Send(); 
```
● Upload a picture without forms, sending only the raw data 
```
        var request = new HTTPRequest(new Uri("http://server.com"), HTTPMethods.Post, onFinished); 
        request.SetHeader("Content-Type", "image/png"); 
        request.Raw = texture.EncodeToPNG(); 
        request.Send(); 
```
● Add custom header 
```
        var request = new HTTPRequest(new Uri("http://server.com"), HTTPMethods.Post, onFinished); 
        request.SetHeader("Content-Type", "application/json; charset=UTF-8"); 
        request.RawData = UTF8Encoding.GetBytes(ToJson(data)); 
        request.Send(); 
```
● Display  download progress 
```
        var request = new HTTPRequest(new Uri("http://serveroflargefile.net/path"), (req, resp) => { Debug.Log("Finished!"); }); 
        request.OnProgress += (req, down, length) => Debug.Log(string.Format("Progress: {0:P2}", down / (float)length)); 
        request.Send(); 
```
● Abort a request 
```
        var request = new HTTPRequest(new Uri(address), (req, resp) => { // State should be HTTPRequestStates.Aborted if we call Abort() before // it’s finishes Debug.Log(req.State); }); 
        request.Send(); 
        request.Abort();
```
● 可恢复下载的范围请求,第一个请求是获取服务器功能的Head请求。当支持范围请求时，将调用DownloadCallback函数。在这个函数中，我们将创建一个新的实际请求来获取内容的块，并将回调函数设置为此函数。当前下载位置保存到PlayerPrefs，因此即使在应用程序重新启动后也可以恢复下载。
```
        private const int ChunkSize = 1024 * 1024; // 1 MiB - should be bigger! 
        private string saveTo = "downloaded.bin"; 
        void StartDownload(string url) 
        {
        var headRequest = new HTTPRequest(new Uri(url), HTTPMethods.Head, (request, response) => 
        {
                if (response == null) Debug.LogError("Response null. Server unreachable? Try again later."); 
                else {
                        if (response.StatusCode == 416) Debug.LogError("Requested range not satisfiable"); else if (response.StatusCode == 200) 
                        Debug.LogError("Partial content doesn't supported by the server, content can be downloaded as a whole."); 
                        else if (response.HasHeaderWithValue("accept-ranges","none")) Debug.LogError("Server doesn't supports the 'Range' header! The file can't be downloaded in parts."); 
                        else DownloadCallback(request, response);         
                }
        }  
        // Range header for our head request 
        int startPos = PlayerPrefs.GetInt("LastDownloadPosition",0); 
        headRequest.SetRangeHeader(startPos, startPos + ChunkSize); 
        headRequest.DisableCache = true; headRequest.Send(); 
        } 

        void DownloadCallback(HTTPRequest request, HTTPResponse response) 
        {
                if (response == null) { Debug.LogError("Response null. Server unreachable, or connection lost? Try again later."); return; } var range = response.GetRange(); 
                if (range == null) { Debug.LogError("No 'Content-Range' header returned from the server!"); return; } 
                else if (!range.IsValid) { Debug.LogError("No valid 'Content-Range' header returned from the server!"); return; } 
                if (request.MethodType != HTTPMethods.Head) 
                { 
                        string path = Path.Combine(Application.temporaryCachePath,saveTo); 
                        using (FileStream fs = new FileStream(path, FileMode.Append)) fs.Write(response.Data, 0, response.Data.Length); 
                        PlayerPrefs.SetInt("LastDownloadPosition", range.LastBytePos); 
                        Debug.LogWarning(string.Format("Download Status: {0}-{1}/{2}", range.FirstBytePos, range.LastBytePos, range.ContentLength)); 
                        if (range.LastBytePos == range.ContentLength - 1) { Debug.LogWarning("Download finished!"); return; } 
                }
                var downloadRequest = new HTTPRequest(request.Uri, HTTPMethods.Get, /*isKeepAlive:*/ true, DownloadCallback); 
                int nextPos = 0; 
                if (request.MethodType != HTTPMethods.Head) nextPos = range.LastBytePos + 1; else nextPos = PlayerPrefs.GetInt("LastDownloadPosition", 0);
                downloadRequest.SetRangeHeader(nextPos, nextPos + ChunkSize); 
                downloadRequest.DisableCache = true;    
                downloadRequest.Send(); 
        } 

```

## 其他

* 1:禁用功能
>>>
        ●BESTHTTP_DISABLE_COOKIES：使用此定义可以禁用所有与cookie相关的代码。不会进行cookie解析，保存和发送。 
        ●BESTHTTP_DISABLE_CACHING：使用此定义可以禁用所有与缓存相关的代码。不会进行缓存或缓存验证。 
        ●BESTHTTP_DISABLE_SERVERSENT_EVENTS：可以使用此功能禁用服务器发送的事件。 SignalR不会回退到此。 
        ●BESTHTTP_DISABLE_WEBSOCKET：可以使用此禁用Websocket。 SignalR和Socket.IO不会使用此协议。 
        ●BESTHTTP_DISABLE_SIGNALR：将禁用整个SignalR实施。 
        ●BESTHTTP_DISABLE_SIGNALR_CORE：将禁用SignalR Core实施。 
        ●BESTHTTP_DISABLE_SOCKETIO：将禁用整个Socket.IO实现。 
        ●BESTHTTP_DISABLE_ALTERNATE_SSL：如果您没有为WebSocket使用HTTPS或WSS，或者您对默认实现感到满意，则可以禁用备用ssl处理程序。 
        ●BESTHTTP_DISABLE_UNITY_FORM：您可以删除对Unity的WWWForm的依赖。
>>>
* 2:支持的平台
>>>
        ● WebGL
        ● iOS
        ● Android
        ● Windows Phone 10
        ● WinRT / Metro / Windows应用商店应用8.1,10•Windows，Linux和Mac独立版
>>>
* 3:在Android，iOS和桌面平台上.net的Net SslStream用于HTTPS。这可以处理各种证书，但有些证书可能会失败。要提供备用解决方案BouncyCastle捆绑在插件中，您可以通过在HTTPRequest对象上将UseAlternateSSL设置为true来使用它。但它也可能在一些认证上失败。在Windows Phone 8.1（及更高版本）和WinRT（Windows应用商店应用程序）上，安全的Tls 1.2协议将处理连接。

