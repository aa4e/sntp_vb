Imports System.ComponentModel

Namespace SNTP

    ''' <summary>
    ''' Представляет SNTP пакет.
    ''' </summary>
    ''' <remarks>
    '''                      1                   2                   3
    '''  0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    ''' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ''' |LI | VN  |Mode |    Stratum    |     Poll      |   Precision   |
    ''' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ''' |               +          Root Delay           +               |
    ''' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ''' |               +       Root Dispersion         +               |
    ''' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ''' |               +     Reference Identifier      +               |
    ''' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ''' |               +                               +               |
    ''' |               +   Reference Timestamp (64)    +               |
    ''' |               +                               +               |
    ''' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ''' |               +                               +               |
    ''' |               +   Originate Timestamp (64)    +               |
    ''' |               +                               +               |
    ''' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ''' |               +                               +               |
    ''' |               +    Receive Timestamp (64)     +               |
    ''' |               +                               +               |
    ''' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ''' |               +                               +               |
    ''' |               +    Transmit Timestamp (64)    +               |
    ''' |               +                               +               |
    ''' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ''' |               + Key Identifier (optional) (32)+               |
    ''' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ''' |               +                               +               |
    ''' |               +                               +               |
    ''' |               + Message Digest (optional) (128)               |
    ''' |               +                               +               |
    ''' |               +                               +               |
    ''' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    ''' См. RFC-2030 для подробностей.
    ''' </remarks>
    Public NotInheritable Class SntpData
        Implements INotifyPropertyChanged

        ''' <summary>
        ''' "Сырой" пакет.
        ''' </summary>
        Private Data() As Byte

#Region "CONST"

        Public Const MaximumPacketLength As Integer = 88 '68 'Максимальная длина SNTP пакета в байтах.
        Public Const MinimumPacketLength As Integer = 48 'Минимальная длина SNTP пакета в байтах.

        Public Shared ReadOnly Epoch As DateTime = New DateTime(1900, 1, 1)

        Private Const ReferenceIdentifierOffset As Integer = 12
        Private Const OriginateIndex As Integer = 24
        Private Const ReceiveIndex As Integer = 32
        Private Const ReferenceIndex As Integer = 16
        Private Const TransmitIndex As Integer = 40

#End Region '/CONST

#Region "CTORs"

        Friend Sub New()
            Dim byteArray(MinimumPacketLength - 1) As Byte
            Data = byteArray
        End Sub

        Friend Sub New(bytes As Byte())
            If (bytes.Length >= MinimumPacketLength AndAlso bytes.Length <= MaximumPacketLength) Then
                Data = bytes
            Else
                Throw New ArgumentOutOfRangeException("bytes", $"SNTP пакет должен быть длиной от {MinimumPacketLength} до {MaximumPacketLength}.")
            End If
        End Sub

#End Region '/CTORs

#Region "PROPS"

        ''' <summary>
        ''' Время UTC, когда данные пришли от сервера. Не является свойством NTP пакета.
        ''' </summary>
        Public Property DestinationDateTime As DateTime

        ''' <summary>
        ''' Показывает необходимость вставки/удаления корректирующей секунды в последнюю минуту текущих суток.
        ''' </summary>
        ''' <remarks>
        ''' This is a two-bit code warning of an impending leap second to be inserted/deleted in the last minute of the current day, with bit 0 and bit 1, respectively.
        ''' </remarks>
        Public Property LeapIndicator As LeapIndicators
            Get
                Return CType(CByte((Data(0) And &HC0) >> 6), LeapIndicators)
            End Get
            Set(value As LeapIndicators)
                Data(0) = CByte(Data(0) And &H3F)
                Data(0) = Data(0) Or CByte(value << 6)
                NotifyPropertyChanged("LeapIndicator")
            End Set
        End Property

        ''' <summary>
        ''' Номер версии NTP/SNTP.
        ''' </summary>
        ''' <remarks>
        ''' If necessary to distinguish between IPv4, IPv6 and OSI, the encapsulating context must be inspected.
        ''' </remarks>
        Public Property VersionNumber As VersionNumbers
            Get
                Dim v As Integer = (Data(0) And &H38) >> 3
                If [Enum].IsDefined(GetType(VersionNumbers), v) Then
                    Return CType(v, VersionNumbers)
                Else
                    Return VersionNumbers.Version3
                End If
            End Get
            Set(ByVal value As VersionNumbers)
                Data(0) = CByte(Data(0) And &HC7)
                Data(0) = CByte(Data(0) Or (value << 3))
                NotifyPropertyChanged("VersionNumber")
            End Set
        End Property

        ''' <summary>
        ''' Режим.
        ''' </summary>
        ''' <remarks>
        ''' In unicast and anycast modes, the client sets this field to 3 (client) in the request And the server sets it to 4 (server) in the reply. 
        ''' In multicast mode, the server sets this field to 5 (broadcast).
        ''' </remarks>
        Public Property Mode As Modes
            Get
                Return CType(CByte((Data(0) And &H7)), Modes)
            End Get
            Set(ByVal value As Modes)
                Dim m As Byte = CByte(Data(0) And &HF8)
                Data(0) = CByte(m Or value)
                NotifyPropertyChanged("Mode")
            End Set
        End Property

        ''' <summary>
        ''' Уровень часов (stratum level).
        ''' </summary>
        Public Property Stratum As Stratums
            Get
                Return CType(Data(1), Stratums)
            End Get
            Set(value As Stratums)
                Data(1) = CByte(value)
                NotifyPropertyChanged("Stratum")
            End Set
        End Property

        ''' <summary>
        ''' Максимальный интервал опроса между соседними сообщениями, в секундах.
        ''' </summary>
        Public Property PollInterval As Integer
            Get
                Return CInt(Math.Pow(2, Data(2)))
            End Get
            Set(value As Integer)
                SetPollInterval(value)
                NotifyPropertyChanged("PollInterval")
            End Set
        End Property

        Public Sub SetPollInterval(interval As Integer)
            Dim d As Double = Math.Log(interval, 2)
            If (d <= Byte.MaxValue) Then
                'If (d >= 4) AndAlso (d <= 14) Then
                Try
                    Data(2) = CByte(d)
                Catch ex As Exception
                    Data(2) = 0
                End Try
                'Else
                '    Throw New ArgumentException("Допустимые значения от 16 (2^4) до 16284 (2^14) сек; а в большинстве приложений используются от 64 (2^6) до 1024 (2^10) сек.")
                'End If
            End If
        End Sub

        ''' <summary>
        ''' Точность часов, сек.
        ''' </summary>
        ''' <remarks>
        ''' This is an eight-bit signed integer indicating the precision of the local clock, in seconds to the nearest power of two.
        ''' The values that normally appear in this field range from -6 for mains-frequency clocks to -20 for microsecond clocks found in some workstations.
        ''' </remarks>
        Public Property Precision As Double
            Get
                Dim b As Byte = Data(3)
                If (b < 128) Then
                    Return Math.Pow(2, b)
                Else
                    Return Math.Pow(2, b - 256)
                End If
            End Get
            Set(value As Double)
                SetPrecision(value)
                NotifyPropertyChanged("Precision")
            End Set
        End Property

        ''' <summary>
        ''' Устанавливает точность часов.
        ''' </summary>
        ''' <param name="sec">Точность в секундах, обычно от E-6 до E-20.</param>
        Public Sub SetPrecision(sec As Double)
            If sec > 0 Then
                Dim p As Double = Math.Log(sec, 2)
                Data(3) = CByte(p)
            Else
                Data(3) = 0
            End If
        End Sub

        ''' <summary>
        ''' Суммарная задержка до первичного источника (primary reference source), сек.
        ''' </summary>
        ''' <remarks>
        ''' This is a 32-bit signed fixed-point number indicating the total roundtrip delay to the primary reference source, in seconds with fraction point between bits 15 and 16. 
        ''' Note that this variable can take on both positive and negative values, depending on the relative time And frequency offsets. 
        ''' The values that normally appear in this field range from negative values of a few milliseconds to positive values Of several hundred milliseconds.
        ''' </remarks>
        Public Property RootDelay As Double
            Get
                Return SecondsStampToSeconds(4)
            End Get
            Set(value As Double)
                SetRootDelay(value)
                NotifyPropertyChanged("RootDelay")
            End Set
        End Property

        ''' <summary>
        ''' Задаёт значение <see cref="RootDelay"/> в "сыром" виде.
        ''' </summary>
        ''' <param name="code">4 байта.</param>
        Public Sub SetRootDelay(code As Byte())
            If (code.Length = 4) Then
                For i As Integer = 0 To 3
                    Data(4 + i) = code(i)
                Next
            End If
        End Sub

        ''' <summary>
        ''' Задаёт значение <see cref="RootDelay"/> в "сыром" виде.
        ''' </summary>
        ''' <param name="sec">Значение в секундах.</param>
        Public Sub SetRootDelay(sec As Double)
            SecondsToSecondsStamp(sec, 4)
        End Sub

        ''' <summary>
        ''' Номинальная ошибка по отношению к первичному источнику (primary reference source), сек.
        ''' </summary>
        ''' <remarks>
        ''' This is a 32-bit unsigned fixed-point number indicating the nominal error relative to the primary reference source, in seconds, with fraction point between bits 15 And 16. 
        ''' The values that normally appear in this field range from 0 to several hundred milliseconds.
        ''' </remarks>
        Public Property RootDispersion As Double
            Get
                Return SecondsStampToSeconds(8)
            End Get
            Set(value As Double)
                SetRootDispersion(value)
                NotifyPropertyChanged("RootDispersion")
            End Set
        End Property

        Public Sub SetRootDispersion(sec As Double)
            SecondsToSecondsStamp(sec, 4)
        End Sub

        ''' <summary>
        ''' Идентификатор относительного источника (reference source).
        ''' </summary>
        ''' <remarks>
        ''' This is a 32-bit bitstring identifying the particular reference source. 
        ''' In the case of NTP Version 3 or Version 4 stratum-0 (unspecified) or stratum-1 (primary) servers, this is a four-character ASCII string, left justified and zero padded to 32 bits. 
        ''' In NTP Version 3 secondary servers, this is the 32-bit IPv4 address of the reference source. 
        ''' In NTP Version 4 secondary servers, this is the low order 32 bits of the latest transmit timestamp of the reference source. 
        ''' NTP primary (stratum 1) servers should set this field to a code identifying the external reference source according to the following list. 
        ''' If the external reference is one of those listed, the associated code should be used. 
        ''' Codes for sources not listed can be contrived as appropriate.
        ''' </remarks>
        Public Property ReferenceIdentifier As String
            Get
                Dim result As String = String.Empty
                Select Case Stratum
                    'In the case of NTP Version 3 or Version 4 stratum-0 (unspecified) or stratum-1 (primary) servers, 
                    'this is a four-character ASCII string, left justified and zero padded to 32 bits.
                    Case Stratums.Primary, Stratums.Unspecified
                        Dim id As UInteger = 0
                        For i As Integer = 0 To 3
                            id = (id << 8) Or Data(ReferenceIdentifierOffset + i)
                        Next
                        If (Not ReferenceIdentifierDictionary.TryGetValue((CType(id, ReferenceIdentifiers)), result)) Then
                            result = String.Format("{0}{1}{2}{3}",
                                                   Chr(Data(ReferenceIdentifierOffset)),
                                                   Chr(Data(ReferenceIdentifierOffset + 1)),
                                                   Chr(Data(ReferenceIdentifierOffset + 2)),
                                                   Chr(Data(ReferenceIdentifierOffset + 3)))
                        End If
                    Case Stratums.Secondary To Stratums.Secondary15
                        Select Case VersionNumber
                            Case VersionNumbers.Version4
                                'In NTP Version 4 secondary servers, this is the low order 32 bits of the latest transmit timestamp of the reference source.
                                'The code works with the Version 4 spec, but many servers respond as v4 but fill this as v3.
                                result = Timestamp32ToDateTime(ReferenceIdentifierOffset).ToString()
                            Case Else
                                'In NTP Version 3 secondary servers, this is the 32-bit IPv4 address of the reference source.
                                result = String.Format("{0}.{1}.{2}.{3}",
                                                       Data(ReferenceIdentifierOffset),
                                                       Data(ReferenceIdentifierOffset + 1),
                                                       Data(ReferenceIdentifierOffset + 2),
                                                       Data(ReferenceIdentifierOffset + 3)) 'показываем как IP
                        End Select
                End Select
                Return result
            End Get
            Set(value As String)
                SetReferenceIdentifier(value)
                NotifyPropertyChanged("ReferenceIdentifier")
            End Set
        End Property

        ''' <summary>
        ''' Задаёт идентификатор.
        ''' </summary>
        ''' <param name="id"></param>
        ''' <remarks>
        ''' Code   External Reference Source
        ''' --------------------------------------------------------------
        ''' LOCL   uncalibrated local clock used As a primary reference For
        '''        a subnet without external means Of synchronization
        ''' PPS    atomic clock Or other pulse-per-second source
        '''        individually calibrated To national standards
        ''' ACTS   NIST dialup modem service
        ''' USNO   USNO modem service
        ''' PTB    PTB(Germany) modem service
        ''' TDF    Allouis(France) Radio 164 kHz
        ''' DCF    Mainflingen(Germany) Radio 77.5 kHz
        ''' MSF    Rugby(UK) Radio 60 kHz
        ''' WWV    Ft. Collins (US) Radio 2.5, 5, 10, 15, 20 MHz
        ''' WWVB   Boulder(US) Radio 60 kHz
        ''' WWVH   Kaui Hawaii (US) Radio 2.5, 5, 10, 15 MHz
        ''' CHU    Ottawa(Canada) Radio 3330, 7335, 14670 kHz
        ''' LORC   LORAN-C radionavigation system
        ''' OMEG   OMEGA radionavigation system
        ''' GPS    Global Positioning Service
        ''' GOES   Geostationary Orbit Environment Satellite
        ''' </remarks>
        Public Sub SetReferenceIdentifier(id As ReferenceIdentifiers)
            Dim bytes() As Byte = BitConverter.GetBytes(id)
            For i As Integer = 0 To 3
                Data(ReferenceIdentifierOffset + i) = bytes(3 - i)
            Next
        End Sub

        ''' <summary>
        ''' Задаёт идентификатор.
        ''' </summary>
        ''' <param name="id">ASCII строка из 3-х или 4-х символов.</param>
        Public Sub SetReferenceIdentifier(id As String)
            Dim bytes() As Byte = Text.Encoding.ASCII.GetBytes(id)
            If (Not bytes.Length = 4) Then
                ReDim Preserve bytes(3)
            End If
            For i As Integer = 0 To bytes.Length - 1
                Data(ReferenceIdentifierOffset + i) = bytes(i)
            Next
        End Sub

        Public Sub SetReferenceIdentifier(id As Net.IPAddress)
            Dim bytes() As Byte = id.GetAddressBytes()
            For i As Integer = 0 To 3
                Data(ReferenceIdentifierOffset + i) = bytes(i)
            Next
        End Sub

        ''' <summary>
        ''' Время UTC, когда часы в последний раз были скорректированы.
        ''' </summary>
        ''' <remarks>
        ''' This is the time at which the local clock was last set or corrected, in 64-bit timestamp format.
        ''' </remarks>
        Public Property ReferenceTimestamp As DateTime
            Get
                Return TimestampToDateTime(ReferenceIndex)
            End Get
            Set(value As DateTime)
                SetReferenceTimestamp(value)
                NotifyPropertyChanged("ReferenceTimestamp")
            End Set
        End Property

        Public Sub SetReferenceTimestamp(value As DateTime)
            DateTimeToTimestamp(value, ReferenceIndex)
        End Sub

        ''' <summary>
        ''' Время UTC, когда запрос был отправлен от клиента к серверу.
        ''' </summary>
        ''' <remarks>
        ''' This is the time at which the request departed the client for the server, in 64-bit timestamp format.
        ''' </remarks>
        Public Property OriginateTimestamp As DateTime
            Get
                Return TimestampToDateTime(OriginateIndex)
            End Get
            Set(value As DateTime)
                SetOriginateTimestamp(value)
                NotifyPropertyChanged("OriginateTimestamp")
            End Set
        End Property

        Public Sub SetOriginateTimestamp(value As DateTime)
            DateTimeToTimestamp(value, OriginateIndex)
        End Sub

        ''' <summary>
        ''' Время UTC, когда запрос пришёл на сервер.
        ''' </summary>
        ''' <remarks>
        ''' This is the time at which the request arrived at the server, in 64-bit timestamp format.
        ''' </remarks>
        Public Property ReceiveTimestamp As DateTime
            Get
                Return TimestampToDateTime(ReceiveIndex)
            End Get
            Set(value As DateTime)
                SetReceiveTimestamp(value)
                NotifyPropertyChanged("ReceiveTimestamp")
            End Set
        End Property

        Public Sub SetReceiveTimestamp(value As DateTime)
            DateTimeToTimestamp(value, ReceiveIndex)
        End Sub

        ''' <summary>
        ''' Время UTC отправки ответа клиенту от сервера.
        ''' </summary>
        ''' <remarks>
        ''' This is the time at which the reply departed the server for the client, in 64-bit timestamp format.
        ''' </remarks>
        Public Property TransmitTimestamp As DateTime
            Get
                Return TimestampToDateTime(TransmitIndex)
            End Get
            Set(ByVal value As DateTime)
                SetTransmitTimestamp(value)
                NotifyPropertyChanged("TransmitTimestamp")
            End Set
        End Property

        Public Sub SetTransmitTimestamp(time As DateTime)
            DateTimeToTimestamp(time, TransmitIndex)
        End Sub

        ''' <summary>
        ''' Криптографический ключ, используемый для аутентификации (опция).
        ''' </summary>
        ''' <remarks>См. RFC-1305, Appendix C.</remarks>
        Public Property KeyIdentifier As UInteger?
            Get
                If (Data.Length = MaximumPacketLength) Then
                    Return ((CUInt(Data(&H40)) << 24) Or (CUInt(Data(&H41)) << 16) Or (CUInt(Data(&H42)) << 8) Or Data(&H43))
                End If
                Return Nothing
            End Get
            Set(value As UInteger?)
                If (value IsNot Nothing) Then
                    Dim b As Byte() = BitConverter.GetBytes(value.Value)
                    ReDim Preserve Data(MaximumPacketLength - 1)
                    Data(&H40) = b(3)
                    Data(&H41) = b(2)
                    Data(&H42) = b(1)
                    Data(&H43) = b(0)
                End If
                NotifyPropertyChanged("KeyIdentifier")
            End Set
        End Property

        ''' <summary>
        ''' Набор 64-битных ключей DES шифрования, используемых для аутентификации (опция).
        ''' </summary>
        ''' <remarks>
        ''' This is a set of 64-bit DES keys. Each key is constructed as in the Berkeley Unix distributions, 
        ''' which consists of eight octets, where the seven low-order bits of each octet correspond to the DES bits 1-7 
        ''' and the high-order bit corresponds to the DES odd-parity bit 8.
        ''' См. RFC-1305, Appendix C.
        ''' </remarks>
        Public Property MessageDigest As Byte?()
            Get
                If (Data.Length = 88) Then
                    Dim digest(19) As Byte?
                    For i As Integer = 0 To 19
                        digest(i) = CType(Data(i + &H44), Byte?)
                    Next
                    Return digest
                End If
                Return Nothing
            End Get
            Set(value As Byte?())
                If (value IsNot Nothing) AndAlso (value.Length > 0) Then
                    ReDim Preserve Data(MaximumPacketLength - 1)
                    For i As Integer = 0 To 19
                        Data(i + &H44) = value(i).Value
                    Next
                Else
                    ReDim Preserve Data(&H43)
                End If
                NotifyPropertyChanged("MessageDigest")
            End Set
        End Property

        ''' <summary>
        ''' Соответствовать ли стандарту при отправке запроса или ответа.
        ''' </summary>
        Public Property FollowStandard As Boolean
            Get
                Return _FollowStandard
            End Get
            Set(value As Boolean)
                If (_FollowStandard <> value) Then
                    _FollowStandard = value
                    NotifyPropertyChanged("FollowStandard")
                End If
            End Set
        End Property
        Private _FollowStandard As Boolean

#End Region '/PROPS

#Region "READ-ONLY PROPS"

        ''' <summary>
        ''' Длина пакета в байтах.
        ''' </summary>
        Public ReadOnly Property Length As Integer
            Get
                Return Data.Length
            End Get
        End Property

        ''' <summary>
        ''' Разница в секундах между локальным временем и временем сервера.
        ''' </summary>
        ''' <remarks>
        ''' Originate Timestamp     T1   time request sent by client
        ''' Receive Timestamp       T2   time request received by server
        ''' Transmit Timestamp      T3   time reply sent by server
        ''' Destination Timestamp   T4   time reply received by client
        ''' t = ((T2 - T1) + (T3 - T4)) / 2.
        ''' </remarks>
        Public ReadOnly Property LocalClockOffset As Double
            Get
                Dim dt1 As Double = ReceiveTimestamp.Ticks - OriginateTimestamp.Ticks
                Dim dt2 As Double = TransmitTimestamp.Ticks - DestinationDateTime.Ticks
                Return ((dt1 + dt2) / 2) / TimeSpan.TicksPerSecond
            End Get
        End Property

        ''' <summary>
        ''' Суммарная задержка в обе стороны, сек.
        ''' </summary>
        ''' <remarks>
        ''' d = (T4 - T1) - (T2 - T3).
        ''' </remarks>
        Public ReadOnly Property RoundTripDelay As Double
            Get
                Dim dt1 As Double = DestinationDateTime.Ticks - OriginateTimestamp.Ticks
                Dim dt2 As Double = ReceiveTimestamp.Ticks - TransmitTimestamp.Ticks
                Return (dt1 - dt2) / TimeSpan.TicksPerSecond
            End Get
        End Property

        ''' <summary>
        ''' Текстовое описание свойства <see cref="LeapIndicator"/>.
        ''' </summary>
        Public ReadOnly Property LeapIndicatorText As String
            Get
                Dim result As String = String.Empty
                LeapIndicatorDictionary.TryGetValue(LeapIndicator, result)
                Return result
            End Get
        End Property

        ''' <summary>
        ''' Текстовое описание свойства <see cref="Mode"/>.
        ''' </summary>
        Public ReadOnly Property ModeText As String
            Get
                Dim result As String = String.Empty
                ModeDictionary.TryGetValue(Mode, result)
                Return result
            End Get
        End Property

        ''' <summary>
        ''' Текстовое описание уровня часов <see cref="Stratum"/>.
        ''' </summary>
        Public ReadOnly Property StratumText As String
            Get
                Dim result As String = String.Empty
                If (Not StratumDictionary.TryGetValue(Stratum, result)) Then
                    result = "Зарезервировано"
                End If
                Return result
            End Get
        End Property

        ''' <summary>
        ''' Текстовое представление номера версии.
        ''' </summary>
        Public ReadOnly Property VersionNumberText As String
            Get
                Dim result As String = String.Empty
                If (Not VersionNumberDictionary.TryGetValue(VersionNumber, result)) Then
                    result = "Неизвестно"
                End If
                Return result
            End Get
        End Property

#End Region '/READ-ONLY PROPS

#Region "METHODS"

        ''' <summary>
        ''' Описание полей пакета:
        ''' Field Name              Unicast/Anycast          Multicast
        '''                        Request    Reply
        ''' ----------------------------------------------------------
        ''' LI                      0          0-2           0-2
        ''' VN                      1-4        copied from   1-4
        '''                                    request
        ''' Mode                    3          4             5
        ''' Stratum                 0          1-14          1-14
        ''' Poll                    0          ignore        ignore
        ''' Precision               0          ignore        ignore
        ''' Root Delay              0          ignore        ignore
        ''' Root Dispersion         0          ignore        ignore
        ''' Reference Identifier    0          ignore        ignore
        ''' Reference Timestamp     0          ignore        ignore
        ''' Originate Timestamp     0          (see text)    ignore
        ''' Receive Timestamp       0          (see text)    ignore
        ''' Transmit Timestamp (see text)      nonzero       nonzero
        ''' Authenticator           optional   optional      optional
        ''' </summary>
        Public Overrides Function ToString() As String
            Dim sb As New Text.StringBuilder()
            sb.AppendLine($"LI={LeapIndicatorText}")
            sb.AppendLine($"VN={VersionNumberText}")
            sb.AppendLine($"Mode={ModeText}")
            sb.AppendLine($"Stratum={StratumText}")
            sb.AppendLine($"Poll={PollInterval}")
            sb.AppendLine($"Precision={Precision}")
            sb.AppendLine($"Root Delay={RootDelay}")
            sb.AppendLine($"Root Dispersion={RootDispersion}")
            sb.AppendLine($"Reference Identifier={ReferenceIdentifier}")
            sb.AppendLine($"Reference Timestamp={ReferenceTimestamp}.{ReferenceTimestamp.Millisecond.ToString("D3")}")
            sb.AppendLine($"Originate Timestamp={OriginateTimestamp}.{OriginateTimestamp.Millisecond.ToString("D3")}")
            sb.AppendLine($"Receive Timestamp={ReceiveTimestamp}.{ReceiveTimestamp.Millisecond.ToString("D3")}")
            sb.AppendLine($"Transmit Timestamp={TransmitTimestamp}.{TransmitTimestamp.Millisecond.ToString("D3")}")
            sb.AppendLine($"Roundtrip delay={RoundTripDelay}")
            sb.AppendLine($"Local clock offset={LocalClockOffset}")
            If KeyIdentifier.HasValue Then
                sb.AppendLine($"Key Identifier={KeyIdentifier.Value}")
                For Each b As Byte? In MessageDigest
                    sb.Append(b.Value.ToString("x2"))
                    sb.Append(" ")
                Next
                sb.AppendLine()
            End If
            Return sb.ToString()
        End Function

#End Region '/METHODS

#Region "CLOSED METHODS"

        ''' <summary>
        ''' Преобразует время <paramref name="dateTime"/> в 64-битный формат NTP и заполняет <see cref="Data"/>, начиная с байта <paramref name="startIndex"/>.
        ''' </summary>
        ''' <param name="dateTime"></param>
        ''' <param name="startIndex"></param>
        Protected Sub DateTimeToTimestamp(ByVal dateTime As DateTime, ByVal startIndex As Integer)
            Dim ticks As UInt64 = CULng((dateTime - Epoch).Ticks)
            Dim seconds As UInt64 = CULng(ticks / TimeSpan.TicksPerSecond)
            Dim fractions As UInt64 = CULng(((ticks Mod TimeSpan.TicksPerSecond) * &H100000000L) / TimeSpan.TicksPerSecond)
            For i As Integer = 3 To 0 Step -1
                Data(startIndex + i) = CByte(seconds And &HFFUL)
                seconds >>= 8
            Next
            For i As Integer = 7 To 4 Step -1
                Data(startIndex + i) = CByte(fractions And &HFFUL)
                fractions >>= 8
            Next
        End Sub

        ''' <summary>
        ''' Преобразует 32-битный код секунд, начиная с байта <paramref name="startIndex"/> данных <see cref="Data"/>, в секунды.
        ''' </summary>
        ''' <param name="startIndex">Индекс байта в пакете, откуда начинается метка времени. </param>
        ''' <remarks>Метка секунд хранится в формате 32-битного числа, где: биты с 0 по 15 - целая часть, а с 16 до 31 - дробная часть.</remarks>
        Private Function SecondsStampToSeconds(ByVal startIndex As Integer) As Double
            Dim seconds As UInt64 = 0
            For i As Integer = 0 To 1
                seconds = (seconds << 8) Or Data(startIndex + i)
            Next
            Dim fractions As UInt64 = 0
            For i As Integer = 2 To 3
                fractions = (fractions << 8) Or Data(startIndex + i)
            Next
            Dim ticks As UInt64 = CULng((seconds * TimeSpan.TicksPerSecond) + (fractions * TimeSpan.TicksPerSecond / &H10000L))
            Dim ts As Double = ticks / TimeSpan.TicksPerSecond
            Return ts
        End Function

        Private Sub SecondsToSecondsStamp(seconds As Double, startIndex As Integer)
            'TODO Перевести секунды в 32-битную метку времени.

        End Sub

        ''' <summary>
        ''' Пробразует 32-разрядное значение в метку времени ().
        ''' </summary>
        ''' <param name="startIndex">Начальный индекс байта в сыром массиве <see cref="Data"/>, откуда начинается 32-битное значение <see cref="ReferenceIdentifier"/>.</param>
        ''' <remarks>
        ''' In NTP Version 4 secondary servers, this is the low order 32 bits of the latest transmit timestamp of the reference source (см.стр.9 RFC-2030).
        ''' </remarks>
        Private Function Timestamp32ToDateTime(ByVal startIndex As Integer) As DateTime
            Dim seconds As UInteger = 0
            For i As Integer = 0 To 3
                seconds = (seconds << 8) Or Data(startIndex + i)
            Next
            Dim ticks As ULong = CULng(seconds * TimeSpan.TicksPerSecond)
            Return Epoch.Add(TimeSpan.FromTicks(CLng(ticks)))
        End Function

        ''' <summary>
        ''' Преобразует 64-битное значение из массива данных <see cref="Data"/>, начиная с байта <paramref name="startIndex"/>, в время.
        ''' </summary>
        ''' <param name="startIndex">Начальный индекс байта в сыром массиве <see cref="Data"/>, откуда начинается 64-битное значение метки времени.</param>
        ''' <remarks>
        '''                     1                   2                   3
        ''' 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
        ''' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ''' |                           Seconds                             |
        ''' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ''' |                  Seconds Fraction (0-padded)                  |
        ''' +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
        ''' </remarks>
        Private Function TimestampToDateTime(ByVal startIndex As Integer) As DateTime
            Dim seconds As UInteger = 0
            For i As Integer = 0 To 3
                seconds = (seconds << 8) Or Data(startIndex + i)
            Next
            Dim fraction As UInteger = 0
            For i As Integer = 4 To 7
                fraction = (fraction << 8) Or Data(startIndex + i)
            Next
            'Dim ticks As UInt64 = CULng((seconds * TimeSpan.TicksPerSecond) + (fraction * TimeSpan.TicksPerSecond / &H100000000L))
            'Dim d1 As DateTime = Epoch.Add(TimeSpan.FromTicks(CLng(ticks)))
            Dim fractionTicks As UInteger = CUInt(fraction * TimeSpan.TicksPerSecond / (UInteger.MaxValue + 1))
            Dim d As DateTime = Epoch.Add(TimeSpan.FromSeconds(seconds)).AddTicks(fractionTicks)
            Return d
        End Function

#End Region '/CLOSED METHODS

#Region "CONVERSION OPERATORS"

        Public Shared Widening Operator CType(ByVal sntpPacket As SntpData) As Byte()
            Return sntpPacket.Data
        End Operator

        Public Shared Widening Operator CType(ByVal byteArray As Byte()) As SntpData
            Return New SntpData(byteArray)
        End Operator

#End Region '/CONVERSION OPERATORS

#Region "DICTS"

        Public ReadOnly Property LeapIndicatorDictionary As New Dictionary(Of LeapIndicators, String) From {
            {LeapIndicators.NoWarning, "0, Без коррекции"},
            {LeapIndicators.LastMinute61Seconds, "1, Последняя минута суток имеет 61 секунду"},
            {LeapIndicators.LastMinute59Seconds, "2, Последняя минута суток имеет 59 секунд"},
            {LeapIndicators.Alarm, "3, Внимание (часы не синхронизированы)"}
        }
        Public ReadOnly Property ModeDictionary As New Dictionary(Of Modes, String) From {
            {Modes.Reserved, "0, Зарезервировано"},
            {Modes.SymmetricActive, "1, Симметричный активный"},
            {Modes.SymmetricPassive, "2, Симметричный пассивный"},
            {Modes.Client, "3, Клиент"},
            {Modes.Server, "4, Сервер"},
            {Modes.Broadcast, "5, Широковещательное"},
            {Modes.ReservedNtpControl, "6, Зарезервировано для управляющих сообщений NTP"},
            {Modes.ReservedPrivate, "7, Зарезервировано для частного использования"}
        }
        Public ReadOnly Property StratumDictionary As New Dictionary(Of Stratums, String) From {
            {Stratums.Unspecified, "0, Не задано или недоступно"},
            {Stratums.Primary, "1, Первичный (например, атомные часы)"},
            {Stratums.Secondary, "2, Вторичный (через NTP/SNTP)"},
            {Stratums.Secondary3, "3, Вторичный (через NTP/SNTP)"},
            {Stratums.Secondary4, "4, Вторичный (через NTP/SNTP)"},
            {Stratums.Secondary5, "5, Вторичный (через NTP/SNTP)"},
            {Stratums.Secondary6, "6, Вторичный (через NTP/SNTP)"},
            {Stratums.Secondary7, "7, Вторичный (через NTP/SNTP)"},
            {Stratums.Secondary8, "8, Вторичный (через NTP/SNTP)"},
            {Stratums.Secondary9, "9, Вторичный (через NTP/SNTP)"},
            {Stratums.Secondary10, "10, Вторичный (через NTP/SNTP)"},
            {Stratums.Secondary11, "11, Вторичный (через NTP/SNTP)"},
            {Stratums.Secondary12, "12, Вторичный (через NTP/SNTP)"},
            {Stratums.Secondary13, "13, Вторичный (через NTP/SNTP)"},
            {Stratums.Secondary14, "14, Вторичный (через NTP/SNTP)"},
            {Stratums.Secondary15, "15, Вторичный (через NTP/SNTP)"}
        }
        Public ReadOnly Property VersionNumberDictionary As New Dictionary(Of VersionNumbers, String) From {
            {VersionNumbers.Version3, "Версия 3 (только IPv4)"},
            {VersionNumbers.Version4, "Версия 4 (IPv4, IPv6 и OSI)"}
        }
        Public ReadOnly Property ReferenceIdentifierDictionary As New Dictionary(Of ReferenceIdentifiers, String) From {
            {ReferenceIdentifiers.ACTS, $"{ReferenceIdentifiers.ACTS}, NIST dialup modem service"},
            {ReferenceIdentifiers.CHU, $"{ReferenceIdentifiers.CHU}, Оттава (Канада), Радио 3330, 7335, 14670 кГц"},
            {ReferenceIdentifiers.DCF, $"{ReferenceIdentifiers.DCF}, Майнфлинген (Германия), Радио 77.5 кГц"},
            {ReferenceIdentifiers.GOES, $"{ReferenceIdentifiers.GOES}, Геостационарный орбитальный экологический спутник"},
            {ReferenceIdentifiers.GPS, $"{ReferenceIdentifiers.GPS}, Глобальная система позиционирования"},
            {ReferenceIdentifiers.LOCL, $"{ReferenceIdentifiers.LOCL}, Некалиброванные локальные часы"},
            {ReferenceIdentifiers.LORC, $"{ReferenceIdentifiers.LORC}, Радионавигационная система LORAN-C"},
            {ReferenceIdentifiers.MSF, $"{ReferenceIdentifiers.MSF}, Рагби (Англия), Радио 60 кГц"},
            {ReferenceIdentifiers.OMEG, $"{ReferenceIdentifiers.OMEG}, Радионавигационная система OMEGA"},
            {ReferenceIdentifiers.PPS, $"{ReferenceIdentifiers.PPS}, Атомные часы или иной источник PPS, откалиброванный в соответствии с национальными стандартами"},
            {ReferenceIdentifiers.PTB, $"{ReferenceIdentifiers.PTB}, PTB (Германия) modem service"},
            {ReferenceIdentifiers.TDF, $"{ReferenceIdentifiers.TDF}, Allouis (Франция), Радио 164 кГц"},
            {ReferenceIdentifiers.USNO, $"{ReferenceIdentifiers.USNO}, U.S. Naval Observatory modem service"},
            {ReferenceIdentifiers.WWV, $"{ReferenceIdentifiers.WWV}, Форт Колиинс (США), Радио 2.5, 5, 10, 15, 20 МГц"},
            {ReferenceIdentifiers.WWVB, $"{ReferenceIdentifiers.WWVB}, Боулдер (США), Радио 60 кГц"},
            {ReferenceIdentifiers.WWVH, $"{ReferenceIdentifiers.WWVH}, Kaui Гавайи (США), Радио 2.5, 5, 10, 15 МГц"}
        }

#End Region '/DICTS

#Region "ИНТЕРФЙЕСЫ"

        Public Event PropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged
        Private Sub NotifyPropertyChanged(ByVal propName As String)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propName))
        End Sub

#End Region '/ИНТЕРФЙЕСЫ

    End Class '/SNTPData

#Region "ENUMS"

    ''' <summary>
    ''' Типы коррекции последней секунды текущих суток.
    ''' </summary>
    Public Enum LeapIndicators
        NoWarning = 0
        LastMinute61Seconds = 1
        LastMinute59Seconds = 2
        Alarm = 3
    End Enum

    ''' <summary>
    ''' Версии часов сервера.
    ''' </summary>
    Public Enum Stratums
        Unspecified = 0
        Primary = 1
        Secondary = 2
        Secondary3 = 3
        Secondary4 = 4
        Secondary5 = 5
        Secondary6 = 6
        Secondary7 = 7
        Secondary8 = 8
        Secondary9 = 9
        Secondary10 = 10
        Secondary11 = 11
        Secondary12 = 12
        Secondary13 = 13
        Secondary14 = 14
        Secondary15 = 15
    End Enum

    ''' <summary>
    ''' Версии проткола NTP/SNTP.
    ''' </summary>
    Public Enum VersionNumbers
        Version3 = 3
        Version4 = 4
    End Enum

    ''' <summary>
    ''' Идентификатор источника.
    ''' </summary>
    Public Enum ReferenceIdentifiers As UInteger
        ''' <summary>
        ''' Uncalibrated local clock used as a primary reference for a subnet without external means of synchronization.
        ''' </summary>
        LOCL = (Asc("L"c) << 24) + (Asc("O"c) << 16) + (Asc("C"c) << 8) + Asc("L"c)

        ''' <summary>
        ''' Atomic clock or other pulse-per-second source individually calibrated to national standards.
        ''' </summary>
        PPS = (Asc("P"c) << 24) + (Asc("P"c) << 16) + (Asc("S"c) << 8)

        ''' <summary>
        ''' NIST dialup modem service.
        ''' </summary>
        ACTS = (Asc("A"c) << 24) + (Asc("C"c) << 16) + (Asc("T"c) << 8) + Asc("S"c)

        ''' <summary>
        ''' U.S. Naval Observatory modem service.
        ''' </summary>
        USNO = (Asc("U"c) << 24) + (Asc("S"c) << 16) + (Asc("N"c) << 8) + Asc("O"c)

        ''' <summary>
        ''' PTB (Germany) modem service.
        ''' </summary>
        PTB = (Asc("P"c) << 24) + (Asc("T"c) << 16) + (Asc("B"c) << 8)

        ''' <summary>
        ''' Allouis (France) Radio 164 kHz.
        ''' </summary>
        TDF = (Asc("T"c) << 24) + (Asc("D"c) << 16) + (Asc("F"c) << 8)

        ''' <summary>
        ''' Mainflingen (Germany) Radio 77.5 kHz.
        ''' </summary>
        DCF = (Asc("D"c) << 24) + (Asc("C"c) << 16) + (Asc("F"c) << 8)

        ''' <summary>
        ''' Rugby (UK) Radio 60 kHz.
        ''' </summary>
        MSF = (Asc("M"c) << 24) + (Asc("S"c) << 16) + (Asc("F"c) << 8)

        ''' <summary>
        ''' Ft. Collins (US) Radio 2.5, 5, 10, 15, 20 MHz.
        ''' </summary>
        WWV = (Asc("W"c) << 24) + (Asc("W"c) << 16) + (Asc("V"c) << 8)

        ''' <summary>
        ''' Boulder (US) Radio 60 kHz.
        ''' </summary>
        WWVB = (Asc("W"c) << 24) + (Asc("W"c) << 16) + (Asc("V"c) << 8) + Asc("B"c)

        ''' <summary>
        ''' Kaui Hawaii (US) Radio 2.5, 5, 10, 15 MHz.
        ''' </summary>
        WWVH = (Asc("W"c) << 24) + (Asc("W"c) << 16) + (Asc("V"c) << 8) + Asc("H"c)

        ''' <summary>
        ''' Ottawa (Canada) Radio 3330, 7335, 14670 kHz.
        ''' </summary>
        CHU = (Asc("C"c) << 24) + (Asc("H"c) << 16) + (Asc("U"c) << 8)

        ''' <summary>
        ''' LORAN-C radionavigation system.
        ''' </summary>
        LORC = (Asc("L"c) << 24) + (Asc("O"c) << 16) + (Asc("R"c) << 8) + Asc("C"c)

        ''' <summary>
        ''' OMEGA radionavigation system.
        ''' </summary>
        OMEG = (Asc("O"c) << 24) + (Asc("M"c) << 16) + (Asc("E"c) << 8) + Asc("G"c)

        ''' <summary>
        ''' Global Positioning Service.
        ''' </summary>
        GPS = (Asc("G"c) << 24) + (Asc("P"c) << 16) + (Asc("S"c) << 8)

        ''' <summary>
        ''' Geostationary Orbit Environment Satellite.
        ''' </summary>
        GOES = (Asc("G"c) << 24) + (Asc("E"c) << 16) + (Asc("O"c) << 8) + Asc("S"c)
    End Enum

    ''' <summary>
    ''' Идентификаторы режима.
    ''' </summary>
    ''' <remarks>
    ''' В режимах unicast и anycast клиент выставляет поле <see cref="SntpData.Mode"/> в <see cref="Modes.Client"/> в запросе и сервер выставляет в ответе <see cref="Modes.Server"/>.
    ''' В режиме multicast сервер выставляет поле <see cref="SntpData.Mode"/> в <see cref="Modes.Broadcast"/>.
    ''' </remarks>
    Public Enum Modes
        Reserved = 0
        SymmetricActive = 1
        SymmetricPassive = 2
        Client = 3
        Server = 4
        Broadcast = 5
        ''' <summary>
        ''' Зарезервировано для управляющих сообщений NTP.
        ''' </summary>
        ReservedNtpControl = 6
        ''' <summary>
        ''' Зарезервировано для частного использования.
        ''' </summary>
        ReservedPrivate = 7
    End Enum

#End Region '/ENUMS

End Namespace