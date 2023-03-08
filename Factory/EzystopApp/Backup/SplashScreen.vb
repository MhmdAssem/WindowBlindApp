Public NotInheritable Class SplashScreen
    Private Sub SplashScreen_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub


    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Timer1.Enabled = False
        Me.Dispose(False)
        JobSheet.Show()



        'If ConnectFileDB() = True Then
        '    Me.Dispose(False)
        '    SOPFileOpen.Show()
        'Else
        '    Timer1.Enabled = False
        '    Me.Dispose(False)
        '    DSN.Show()
        'End If
    End Sub

    Public Function ConnectFileDB(Optional ByVal intDBType As Integer = 1) As Boolean

        Try
            Dim oRead As System.IO.StreamReader

            If IO.File.Exists(Application.StartupPath & "\Server.ini") Then
                oRead = IO.File.OpenText(Application.StartupPath & "\Server.ini")
                While oRead.Peek <> -1
                    strIPCName = oRead.ReadLine()
                    ConnectFileDB = True
                End While
                oRead.Close()
            Else
                ConnectFileDB = False
            End If
        Catch ex As Exception
            ConnectFileDB = False
        End Try
    End Function
End Class
