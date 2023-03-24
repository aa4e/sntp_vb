# sntp_vb
**SNTP** `client` and `server` on VB.NET.

## Usage

- SNTP client:

```vbnet
Dim client As New SntpClient(ServerAddress, ServerPort)
AddHandler client.QueryServerCompleted, AddressOf QueryServerCompletedHandler
Client.BeginQueryServer()

Private Sub QueryServerCompletedHandler(sender As Object, e As QueryServerCompletedEventArgs)
  Debug.WriteLine($"Answer received: {e.Data}")
End Sub
```

- SNTP server:

```vbnet
Dim server As New SntpServer()
AddHandler server.RequestReceived, AddressOf RequestReceivedHandler
AddHandler server.StateChanged, AddressOf StateChangedHandler
server.StartServer()

Private Sub StateChangedHandler(state As Boolean, message As String)
  ...
End Sub

Private Sub RequestReceivedHandler(client As IPEndPoint, request As SntpData)
  ...
End Sub
```
