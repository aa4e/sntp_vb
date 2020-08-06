Imports System.ComponentModel
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports SNTP.SntpData

Namespace SNTP

    ''' <summary>
    ''' Клиент для получения данных от NTP/SNTP сервера.
    ''' </summary>
    ''' <remarks>
    ''' Режимы работы: multicast, anycast, broadcast. 
    ''' </remarks>
    <DefaultEvent("QueryServerCompleted")>
    <DefaultProperty("RemoteSNTPServer")>
    Public Class SntpClient

#Region "FIELDS"

        Private Const DefaultTimeout As Integer = 5000

#End Region '/FIELDS

#Region "CTORs"

        Public Sub New()
        End Sub

        Public Sub New(server As String, port As Integer)
            Me.New()
            RemoteSntpServer = New RemoteSntpServer(server, port)
        End Sub

#End Region '/CTORs

#Region "PROPS"

        Public Property Request As New SntpData() With {
            .Mode = Modes.Client,
            .VersionNumber = VersionNumbers.Version3,
            .FollowStandard = True,
            .PollInterval = 16
        }

        ''' <summary>
        ''' Удалённый сервер.
        ''' </summary>
        Public Property RemoteSntpServer As RemoteSntpServer
            Get
                Return _RemoteSntpServer
            End Get
            Set(value As RemoteSntpServer)
                _RemoteSntpServer = value
            End Set
        End Property
        Private _RemoteSntpServer As RemoteSntpServer = RemoteSntpServer.DefaultServer

        ''' <summary>
        ''' Таймаут отправки и получения, мс.
        ''' </summary>
        Public Property Timeout As Integer
            Get
                Return _Timeout
            End Get
            Set(ByVal value As Integer)
                If (value < -1) Then
                    value = DefaultTimeout
                End If
                _Timeout = value
            End Set
        End Property
        Private _Timeout As Integer = DefaultTimeout

        ''' <summary>
        ''' Версия NTP/SNTP.
        ''' </summary>
        Public Property VersionNumber As VersionNumbers
            Get
                Return _VersionNumber
            End Get
            Set(value As VersionNumbers)
                If (_VersionNumber <> value) Then
                    _VersionNumber = value
                End If
            End Set
        End Property
        Private _VersionNumber As VersionNumbers = VersionNumbers.Version3

#End Region '/PROPS

#Region "READ-ONLY PROPS"

        ''' <summary>
        ''' Показывает, занят ли SNTP клиент.
        ''' </summary>
        <Browsable(False)>
        Public ReadOnly Property IsBusy As Boolean
            Get
                Return _IsBusy
            End Get
        End Property
        Private _IsBusy As Boolean

#End Region '/READ-ONLY PROPS

#Region "METHODS"

        ''' <summary>
        ''' Событие возникает, когда запрос к серверу завершён успешно.
        ''' </summary>
        Public Event QueryServerCompleted As EventHandler(Of QueryServerCompletedEventArgs)

        Private AsyncOperation As AsyncOperation = Nothing
        Private Delegate Sub QueryServerDelegate()

        ''' <summary>
        ''' Отправляет асинхронный запрос к серверу <see cref="RemoteSntpServer"/>. 
        ''' </summary>
        ''' <returns>
        ''' Возвращает Fasle, если клиент занят. И True, если запрос был успешно отправлен.
        ''' </returns>
        Public Function BeginQueryServer() As Boolean
            Dim result As Boolean = False
            If (Not IsBusy) Then
                _IsBusy = True
                AsyncOperation = AsyncOperationManager.CreateOperation(Nothing)
                Dim queryDeleg As New QueryServerDelegate(AddressOf QueryServerAsync)
                queryDeleg.BeginInvoke(Nothing, Nothing)
                result = True
            End If
            Return result
        End Function

        Private Sub QueryServerAsync()
            SyncLock (Me)
                Dim e As QueryServerCompletedEventArgs = Nothing
                Try
                    e = QueryServer()
                Catch ex As Exception
                    Throw ex
                End Try
                Dim operationCompleted As New SendOrPostCallback(AddressOf AsyncOperationCompleted)
                AsyncOperation.PostOperationCompleted(operationCompleted, e)
            End SyncLock
        End Sub

        Private Sub AsyncOperationCompleted(ByVal arg As Object)
            _IsBusy = False
            OnQueryServerCompleted(DirectCast(arg, QueryServerCompletedEventArgs))
        End Sub

        ''' <summary>
        ''' Вызывает событие <see cref="QueryServerCompleted"/> по получении ответа сервера.
        ''' </summary>
        Private Sub OnQueryServerCompleted(ByVal e As QueryServerCompletedEventArgs)
            If (QueryServerCompletedEvent IsNot Nothing) Then
                RaiseEvent QueryServerCompleted(Me, e)
            End If
        End Sub

        ''' <summary>
        ''' Отправляет синхронный запрос к серверу и получает ответ.
        ''' </summary>
        Private Function QueryServer() As QueryServerCompletedEventArgs
            Dim result As New QueryServerCompletedEventArgs()
            Try
                Using client As New UdpClient()
                    Dim ept As IPEndPoint = RemoteSntpServer.GetIPEndPoint()
                    client.Client.SendTimeout = Timeout
                    client.Client.ReceiveTimeout = Timeout
                    client.Connect(ept)

                    If Request.FollowStandard Then
                        Request.LeapIndicator = LeapIndicators.NoWarning
                        Request.Mode = Modes.Client
                        Request.TransmitTimestamp = DateTime.Now.ToUniversalTime()
                        Request.Stratum = Stratums.Unspecified
                        'Request.PollInterval = 1
                        Request.Precision = 1
                        Request.RootDelay = 0
                        Request.RootDispersion = 0
                        Request.ReferenceIdentifier = ""
                        Request.ReferenceTimestamp = Epoch
                        Request.OriginateTimestamp = Epoch
                        Request.ReceiveTimestamp = Epoch
                    End If

                    client.Send(Request, Request.Length)
                    result.Data = client.Receive(ept)
                    result.Data.DestinationDateTime = DateTime.Now.ToUniversalTime()

                    'Проверка данных:
                    If (Not result.ErrorData.HasError) Then ' (result.Data.Mode = Modes.Server) Then
                        result.Succeeded = True
                    Else
                        result.ErrorData = New ErrorData("Ответ сервера был некорректный")
                    End If
                    Return result
                End Using
            Catch ex As Exception
                result.ErrorData = New ErrorData(ex)
                Return result
            End Try
        End Function

#End Region '/METHODS

    End Class '/SntpClient

End Namespace