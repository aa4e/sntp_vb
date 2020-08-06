Namespace SNTP

    ''' <summary>
    ''' Аргументы, передаваемые в событии <see cref="SntpClient.QueryServerCompleted"/>.
    ''' </summary>
    Public Class QueryServerCompletedEventArgs
        Inherits EventArgs

#Region "CTOR"

        Friend Sub New()
            Me.ErrorData = New ErrorData()
        End Sub

#End Region '/CTOR

#Region "PROPS"

        ''' <summary>
        ''' Данные, возвращаемые сервером.
        ''' </summary>
        Public Property Data As SntpData

        ''' <summary>
        ''' Информация об ошибке (если она была).
        ''' </summary>
        Public Property ErrorData As ErrorData

        ''' <summary>
        ''' Был ли запрос к серверу завершён успешно. 
        ''' </summary>
        ''' <remarks>
        ''' Возможно, что происходили другие ошибки, не связанные с запрсом к серверу.
        ''' Тогда необходимо проверить значение <see cref="ErrorData"/> несмотря на то, что <see cref="Succeeded" /> будет True.
        ''' </remarks>
        Public Property Succeeded As Boolean

#End Region '/PROPS

    End Class '/QueryServerCompletedEventArgs

End Namespace