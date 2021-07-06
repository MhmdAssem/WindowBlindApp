Imports System
Imports System.Data.OleDb

Module moduleDeclaration
    Public GridDataSet As DataSet, GridTimerDataSet As DataSet, GridPageDataSet As DataSet, GridDataTabel As DataTable, strXMLDoc As String, strXLDoc As String, strCreatedXLDoc, StrPrinterName As String
    Public PageCount As Integer
    Public maxRec As Integer
    Public pageSize As Integer
    Public currentPage As Integer
    Public recNo As Integer
    Public lblLabel(10) As Label
    Public txtText(10) As TextBox
    Public blnFileRead As Boolean

    Public startRec As Integer
  
    Public Port, BaudRate, Parity, DataBits, StopBits As String
    Public boolComvalues As Boolean = False
    Public WithEvents moRS232 As Rs232

    Public m_ThreadList As New ArrayList
    Public m_thMain As System.Threading.Thread





    Public Function ReplaceStr(ByVal strString As String)
        ReplaceStr = Replace(strString, "'", "''")
    End Function







    Public Function CalDecimalValue(ByVal intFirstValue As Integer, ByVal intSecondValue As Integer) As String
        If intSecondValue < 0 Then
            CalDecimalValue = (65536 * intFirstValue) + 65536 + intSecondValue
        Else
            CalDecimalValue = (65536 * intFirstValue) + intSecondValue
        End If
    End Function

    Public Function CalSingleDecimalValue(ByVal intValue As Integer) As String
        'If intValue < 0 Then
        '    CalSingleDecimalValue = 65536 + intValue
        'Else
        CalSingleDecimalValue = intValue
        'End If
    End Function


    Public Function GetResolutionValue(ByVal dblValue As Integer, ByVal intResolution As Integer) As Double
        If intResolution = 0 Then
            GetResolutionValue = dblValue
        ElseIf intResolution = 1 Then
            GetResolutionValue = dblValue * 0.1
        ElseIf intResolution = 2 Then
            GetResolutionValue = dblValue * 0.01
        ElseIf intResolution = 3 Then
            GetResolutionValue = dblValue * 0.001
        ElseIf intResolution = 4 Then
            GetResolutionValue = dblValue * 0.0001
        End If
    End Function
    Public Function GetResolution(ByVal intResolution As Integer) As Double
        If intResolution = 0 Then
            GetResolution = 1
        ElseIf intResolution = 1 Then
            GetResolution = 0.1
        ElseIf intResolution = 2 Then
            GetResolution = 0.01
        ElseIf intResolution = 3 Then
            GetResolution = 0.001
        ElseIf intResolution = 4 Then
            GetResolution = 0.0001
        End If
    End Function


    Public Function TranslateDateTime(ByVal strValue As Object) As String
        If Not IsDBNull(strValue) Then
            Dim dtValue As Date
            dtValue = CDate(strValue)
            TranslateDateTime = CStr(dtValue.Year) & "/" & CStr(dtValue.Month) & "/" & CStr(dtValue.Day) & " " & CStr(dtValue.Hour) & ":" & CStr(dtValue.Minute) & ":" & CStr(dtValue.Second)
        Else
            TranslateDateTime = "NUll"
        End If
    End Function


    Public Function MakeString(ByVal strString As String, ByVal intStringlength As Integer)

        Dim m As Integer

        For m = Len(strString) To intStringlength - 1
            strString = "0" & strString
        Next
        MakeString = strString
    End Function
    Public Function DecimalToBinary(ByVal DecimalNum As Long) As String
        Dim tmp As String
        Dim n As Long

        n = DecimalNum

        tmp = Trim(Str(n Mod 2))
        n = n \ 2

        Do While n <> 0
            tmp = Trim(Str(n Mod 2)) & tmp
            n = n \ 2
        Loop



        If Len(tmp) < 15 Then
            For n = Len(tmp) To 15
                tmp = "0" & tmp
            Next n
        End If

        DecimalToBinary = tmp


    End Function
End Module
