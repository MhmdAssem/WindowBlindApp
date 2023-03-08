Public Class ImportData



    Private Sub btnClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub

   
    Private Sub btnImport_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnImport.Click
        If txtExcelData.Text = String.Empty Then
            MsgBox("Enter Data to Import !!")
            Exit Sub
        End If
        Me.Close()

    End Sub

    Private Sub ImportData_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        txtExcelData.Text = String.Empty
    End Sub
End Class