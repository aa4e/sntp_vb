Imports System.ComponentModel
Imports System.Net
Imports System.Net.Sockets

Namespace SNTP

    ''' <summary>
    ''' Локальный NTP/SNTP сервер.
    ''' </summary>
    Public Class SntpServer

#Region "CTOR"

        ''' <summary>
        ''' Создаёт NTP сервер.
        ''' </summary>
        ''' <param name="port">Порт, на котором сервер будет слушать запросы от клиентов.</param>
        Public Sub New(Optional port As Integer = 123)
            Me.Port = port
            Response.ReferenceIdentifier = "LOCL"
        End Sub

#End Region '/CTOR

#Region "PROPS"

        ''' <summary>
        ''' Ответ сервера.
        ''' </summary>
        Public Property Response As New SntpData() With {
            .Mode = Modes.Server,
            .PollInterval = 16,
            .FollowStandard = True
        }

        ''' <summary>
        ''' Режим работы сервера.
        ''' </summary>
        Public Property Mode As ServerModes = ServerModes.Unicast

#End Region '/PROPS

#Region "READ-ONLY PROPS"

        ''' <summary>
        ''' Порт, на котором сервер будет слушать запросы.
        ''' </summary>
        Public ReadOnly Port As Integer = 123

        ''' <summary>
        ''' Запущен ли NTP сервер.
        ''' </summary>
        Public ReadOnly Property IsRunning As Boolean
            Get
                If (ListenWorker IsNot Nothing) Then
                    Return ListenWorker.IsBusy
                End If
                Return False
            End Get
        End Property

        Public ReadOnly Property ServerModesDictionary As New Dictionary(Of ServerModes, String) From {
            {ServerModes.Unicast, "Unicast"},
            {ServerModes.Anycast, "Anycast"},
            {ServerModes.Broadcast, "Broadcast"}
        }

#End Region '/READ-ONLY PROPS

#Region "METHODS"

        ''' <summary>
        ''' Событие получения запроса от клиента.
        ''' </summary>
        Public Event RequestReceived(client As IPEndPoint, request As SntpData)
        ''' <summary>
        ''' Событие, возникающее при запуске/останове сервера.
        ''' </summary>
        Public Event StateChanged(state As Boolean, message As String)

        Private WithEvents ListenWorker As BackgroundWorker
        Private WithEvents UpdateReferenceClockTimer As New Timers.Timer()

        ''' <summary>
        ''' Запускает сервер (если он не запущен).
        ''' </summary>
        Public Sub StartServer()
            If (ListenWorker Is Nothing) OrElse (Not ListenWorker.IsBusy) Then
                RestartUpdateTimer()
                ListenWorker = New BackgroundWorker() With {.WorkerSupportsCancellation = True, .WorkerReportsProgress = True}
                ListenWorker.RunWorkerAsync()
                RaiseEvent StateChanged(True, $"{Now} Сервер запущен на порте {Port}.")
            End If
        End Sub

        ''' <summary>
        ''' Останавливает сервер.
        ''' </summary>
        Public Sub StopServer()
            If ListenWorker.IsBusy Then
                ListenWorker.CancelAsync()
                RaiseEvent StateChanged(False, $"{Now} Сервер остановлен.")
            End If
        End Sub

#End Region '/METHODS

#Region "ОЖИДАНИЕ ЗАПРОСОВ"

        Private Sub ListenDoWork(sender As Object, e As DoWorkEventArgs) Handles ListenWorker.DoWork
            Try
                Using udp As New UdpClient(Port)
                    Dim endPt As New IPEndPoint(IPAddress.Any, Port)
                    udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, True)
                    Do
                        If (udp.Available > 0) Then
                            SyncLock (Me)
                                Dim request As Byte() = udp.Receive(endPt)
                                ListenWorker.ReportProgress(0, request)
                                UpdateResponse(request)
                                udp.Send(Response, Response.Length, endPt)
                            End SyncLock
                        End If
                    Loop Until ListenWorker.CancellationPending
                End Using
            Catch ex As Exception
                Debug.WriteLine(ex)
                RaiseEvent StateChanged(False, $"{Now} {ex.Message}.")
                ListenWorker.CancelAsync()
                'Throw
            End Try
        End Sub

        ''' <summary>
        ''' Обновляет ответ в соответствии с данными, пришедшими от клиента.
        ''' </summary>
        ''' <param name="req">Запрос от клиента.</param>
        Private Sub UpdateResponse(req As Byte())
            If Response.FollowStandard Then
                Dim request As SntpData = CType(req, SntpData)
                If (Response.PollInterval <> request.PollInterval) Then
                    Response.PollInterval = request.PollInterval
                    RestartUpdateTimer()
                End If
                If (Response.LeapIndicator = LeapIndicators.NoWarning) OrElse (Response.LeapIndicator = LeapIndicators.Alarm) Then
                    Response.LeapIndicator = Response.LeapIndicator
                Else
                    Response.LeapIndicator = LeapIndicators.NoWarning
                End If
                Response.VersionNumber = request.VersionNumber
                Response.OriginateTimestamp = request.TransmitTimestamp
                Select Case request.Mode
                    Case Modes.Client
                        Response.Mode = Modes.Server
                    Case Else
                        Response.Mode = Modes.SymmetricPassive
                End Select
                Response.Stratum = Stratums.Primary
                Response.RootDelay = 0
                Response.RootDispersion = 0
                Response.TransmitTimestamp = DateTime.Now.ToUniversalTime()
            End If
        End Sub

        Private Sub ListenReportProgress(sender As Object, e As ProgressChangedEventArgs) Handles ListenWorker.ProgressChanged
            Try
                Dim request As SntpData = CType(CType(e.UserState, Byte()), SntpData)
                RaiseEvent RequestReceived(Nothing, CType(request, SntpData))
            Catch ex As Exception
                Debug.WriteLine(ex)
            End Try
        End Sub

        Private Sub ListenStop(sender As Object, e As RunWorkerCompletedEventArgs) Handles ListenWorker.RunWorkerCompleted
            UpdateReferenceClockTimer.Stop()
            RaiseEvent StateChanged(False, $"{Now} Сервер остановлен.")
        End Sub

#End Region '/ОЖИДАНИЕ ЗАПРОСОВ

#Region "ОБНОВЛЕНИЕ ЛОКАЛЬНОГО ВРЕМЕНИ"

        ''' <summary>
        ''' Перезапускает таймер обновления локального времени.
        ''' </summary>
        Private Sub RestartUpdateTimer()
            UpdateReferenceTime(Nothing, Nothing)
            UpdateReferenceClockTimer.Stop()
            UpdateReferenceClockTimer.Interval = Response.PollInterval * 1000
            UpdateReferenceClockTimer.Start()
        End Sub

        Private Sub UpdateReferenceTime(sender As Object, e As Timers.ElapsedEventArgs) Handles UpdateReferenceClockTimer.Elapsed
            Response.ReferenceTimestamp = GetReferenceTime()
        End Sub

        ''' <summary>
        ''' Вовзвращает точное время, полученное от первичного источника.
        ''' </summary>
        Private Function GetReferenceTime() As Date
            'TODO Здесь будет "честное" обновление локальных часов.
            Return Now
        End Function

#End Region '/ОБНОВЛЕНИЕ ЛОКАЛЬНОГО ВРЕМЕНИ


    End Class '/SntpServer

#Region "ENUMS"

    Public Enum ServerModes As Integer
        Unicast
        Anycast
        Broadcast
    End Enum

#End Region '/ENUMS

End Namespace