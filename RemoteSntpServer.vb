Imports System.ComponentModel
Imports System.Net

Namespace SNTP

    ''' <summary>
    ''' Класс содержит информацию, необходимую для соединения с удалённым NTP/SNTP сервером.
    ''' </summary>
    <TypeConverter(GetType(ExpandableObjectConverter))>
    Public Class RemoteSntpServer

        ''' <summary>
        ''' Имя сервера по умолчанию.
        ''' </summary>
        Public Const DefaultHostName As String = "time.nist.gov"

        ''' <summary>
        ''' Порт по умолчанию для NTP/SNTP сервера.
        ''' </summary>
        Public Const DefaultPort As Integer = 123

        Public Shared ReadOnly DefaultServer As New RemoteSntpServer()
        Public Shared ReadOnly LocalServer As New RemoteSntpServer("127.0.0.1")
        Public Shared ReadOnly Africa As New RemoteSntpServer("africa.pool.ntp.org")
        Public Shared ReadOnly NTL As New RemoteSntpServer("time.cableol.net")

        Public Shared ReadOnly AppleServers As RemoteSntpServer() = New RemoteSntpServer() {
            New RemoteSntpServer("time.euro.apple.com"),
            New RemoteSntpServer("time1.euro.apple.com")
        }

        Public Shared ReadOnly MicrosoftServers As RemoteSntpServer() = New RemoteSntpServer() {
            New RemoteSntpServer("time.nist.gov"),
            New RemoteSntpServer("time.windows.com"),
            New RemoteSntpServer("time-nw.nist.gov")
        }

        Public Shared ReadOnly AsiaServers As RemoteSntpServer() = New RemoteSntpServer() {
            New RemoteSntpServer("asia.pool.ntp.org"),
            New RemoteSntpServer("0.asia.pool.ntp.org"),
            New RemoteSntpServer("1.asia.pool.ntp.org"),
            New RemoteSntpServer("2.asia.pool.ntp.org"),
            New RemoteSntpServer("3.asia.pool.ntp.org")
        }

        Public Shared ReadOnly AustraliaServers As RemoteSntpServer() = {
            New RemoteSntpServer("au.pool.ntp.org"),
            New RemoteSntpServer("0.au.pool.ntp.org"),
            New RemoteSntpServer("1.au.pool.ntp.org"),
            New RemoteSntpServer("2.au.pool.ntp.org"),
            New RemoteSntpServer("3.au.pool.ntp.org")
        }

        Public Shared ReadOnly CanadaServers As RemoteSntpServer() = {
            New RemoteSntpServer("ca.pool.ntp.org"),
            New RemoteSntpServer("0.ca.pool.ntp.org"),
            New RemoteSntpServer("1.ca.pool.ntp.org"),
            New RemoteSntpServer("2.ca.pool.ntp.org"),
            New RemoteSntpServer("3.ca.pool.ntp.org")
        }

        Public Shared ReadOnly EuropeServers As RemoteSntpServer() = {
            New RemoteSntpServer("europe.pool.ntp.org"),
            New RemoteSntpServer("0.europe.pool.ntp.org"),
            New RemoteSntpServer("1.europe.pool.ntp.org"),
            New RemoteSntpServer("2.europe.pool.ntp.org"),
            New RemoteSntpServer("3.europe.pool.ntp.org")
        }

        Public Shared ReadOnly NorthAmericaServers As RemoteSntpServer() = {
            New RemoteSntpServer("north-america.pool.ntp.org"),
            New RemoteSntpServer("0.north-america.pool.ntp.org"),
            New RemoteSntpServer("1.north-america.pool.ntp.org"),
            New RemoteSntpServer("2.north-america.pool.ntp.org"),
            New RemoteSntpServer("3.north-america.pool.ntp.org")
        }

        Public Shared ReadOnly OceaniaServers As RemoteSntpServer() = {
            New RemoteSntpServer("oceania.pool.ntp.org"),
            New RemoteSntpServer("0.oceania.pool.ntp.org"),
            New RemoteSntpServer("1.oceania.pool.ntp.org"),
            New RemoteSntpServer("2.oceania.pool.ntp.org"),
            New RemoteSntpServer("3.oceania.pool.ntp.org")
        }

        Public Shared ReadOnly PoolServers As RemoteSntpServer() = {
            New RemoteSntpServer("pool.ntp.org"),
            New RemoteSntpServer("0.pool.ntp.org"),
            New RemoteSntpServer("1.pool.ntp.org"),
            New RemoteSntpServer("2.pool.ntp.org")
        }

        Public Shared ReadOnly RussiaServers As RemoteSntpServer() = {
            New RemoteSntpServer("ntp1.vniiftri.ru"),
            New RemoteSntpServer("ntp2.vniiftri.ru"),
            New RemoteSntpServer("ntp3.vniiftri.ru"),
            New RemoteSntpServer("ntp4.vniiftri.ru"),
            New RemoteSntpServer("ntp21.vniiftri.ru"),
            New RemoteSntpServer("ntp1.niiftri.irkutsk.ru"),
            New RemoteSntpServer("ntp2.niiftri.irkutsk.ru"),
            New RemoteSntpServer("vniiftri.khv.ru"),
            New RemoteSntpServer("vniiftri2.khv.ru")
        }

        Public Shared ReadOnly SouthAmericaServers As RemoteSntpServer() = {
            New RemoteSntpServer("south-america.pool.ntp.org"),
            New RemoteSntpServer("0.south-america.pool.ntp.org"),
            New RemoteSntpServer("1.south-america.pool.ntp.org"),
            New RemoteSntpServer("2.south-america.pool.ntp.org"),
            New RemoteSntpServer("3.south-america.pool.ntp.org")
        }

        Public Shared ReadOnly UnitedKingdomServers As RemoteSntpServer() = {
            New RemoteSntpServer("uk.pool.ntp.org"),
            New RemoteSntpServer("0.uk.pool.ntp.org"),
            New RemoteSntpServer("1.uk.pool.ntp.org"),
            New RemoteSntpServer("2.uk.pool.ntp.org"),
            New RemoteSntpServer("3.uk.pool.ntp.org"),
            New RemoteSntpServer("ntp.blueyonder.co.uk")
        }

        Public Shared ReadOnly UnitedStatesServers As RemoteSntpServer() = {
            New RemoteSntpServer("us.pool.ntp.org"),
            New RemoteSntpServer("0.us.pool.ntp.org"),
            New RemoteSntpServer("1.us.pool.ntp.org"),
            New RemoteSntpServer("2.us.pool.ntp.org"),
            New RemoteSntpServer("3.us.pool.ntp.org"),
            New RemoteSntpServer("tock.usno.navy.mil"),
            New RemoteSntpServer("tick.usno.navy.mil"),
            New RemoteSntpServer("ntp1.usno.navy.mil"),
            New RemoteSntpServer("clock.xmission.com"),
            New RemoteSntpServer("ntp1.ja.net")
        }

#Region "CTORs"

        ''' <summary>
        ''' Создаёт экземпляр NTP/SNTP сервера с адресом и портом по умолчанию.
        ''' </summary>
        Public Sub New()
            Me.New(DefaultHostName, DefaultPort)
        End Sub

        ''' <param name="hostNameOrAddress">Имя хоста или адрес сервера.</param>
        ''' <param name="port">Порт (обычно 123).</param>
        Public Sub New(hostNameOrAddress As String, port As Integer)
            Me.HostnameOrAddress = hostNameOrAddress
            Me.Port = port
        End Sub

        ''' <param name="hostNameOrAddress">Имя хоста или адрес сервера.</param>
        Public Sub New(hostNameOrAddress As String)
            Me.New(hostNameOrAddress, DefaultPort)
        End Sub

#End Region '/CTORs

#Region "PROPS"

        ''' <summary>
        ''' Имя хоста или адрес сервера.
        ''' </summary>
        <NotifyParentProperty(True)>
        Public Property HostnameOrAddress As String
            Get
                Return _HostnameOrAddress
            End Get
            Set(ByVal value As String)
                value = value.Trim()
                If String.IsNullOrEmpty(value) Then
                    _HostnameOrAddress = DefaultHostName
                Else
                    _HostnameOrAddress = value
                End If
            End Set
        End Property
        Private _HostnameOrAddress As String = DefaultHostName

        ''' <summary>
        ''' Порт сервера.
        ''' </summary>
        <NotifyParentProperty(True)>
        Public Property Port As Integer
            Get
                Return _Port
            End Get
            Set(ByVal value As Integer)
                If (value >= 0) And (value <= &HFFFF) Then
                    _Port = value
                Else
                    _Port = DefaultPort
                End If
            End Set
        End Property
        Private _Port As Integer = DefaultPort

#End Region '/PROPS

#Region "METHODS"

        ''' <summary>
        ''' Пытается получить удалённую точку сервера.
        ''' </summary>
        Public Function GetIPEndPoint() As IPEndPoint
            If (HostnameOrAddress.ToLower() = "localhost") Then
                Return New IPEndPoint(Dns.GetHostAddresses("127.0.0.1")(0), Port)
            Else
                Return New IPEndPoint(Dns.GetHostAddresses(HostnameOrAddress)(0), Port)
            End If
        End Function

        ''' <summary>
        ''' Возвращает имя хоста и порт сервера.
        ''' </summary>
        Public Overrides Function ToString() As String
            Return $"{HostnameOrAddress}:{Port}"
        End Function

#End Region '/METHODS

    End Class '/RemoteSntpServer

End Namespace