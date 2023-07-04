# Create a request
```csharp
var mbClient = new MbClient();
var requestPacket = mbClient.BuildRequest(1, "428673", MbDataType.Float);
```

# Send it
```csharp
using (var tcpClient = new TcpClient())
{
    tcpClient.Connect("192.168.1.50", 502);

    var stream = tcpClient.GetStream();

    stream.Write(requestPacket, 0, requestPacket.Length);

    var responsePacket = new Byte[260];

    stream.Read(responsePacket, 0, responsePacket.Length);

    var fulfilledRequest = mbClient.ProcessResponse(responsePacket);

    stream.Close();
}
```
