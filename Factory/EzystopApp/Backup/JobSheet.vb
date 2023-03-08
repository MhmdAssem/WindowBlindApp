Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports System.IO
Imports System
Imports NPOI.XSSF.UserModel
Imports NPOI.SS.UserModel
Imports NPOI.HSSF.UserModel
Imports NPOI.HSSF.Util
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Reflection
Imports System.Xml
Imports System.Configuration


Public Class JobSheet

    Dim GridDataSet As DataSet, Deductiontable As DataTable, GridTimerDataSet As DataSet, GridPageDataSet As DataSet, GridDataTabel As DataTable, strXMLDoc As String, strDedFilepath As String, strXLDoc As String, strXLFDoc As String, strCreatedXLDoc, StrPrinterName As String
    Dim SpringDeduction As Integer
    Private PageCount As Integer
    Private maxRec As Integer
    Private pageSize As Integer
    Private currentPage As Integer
    Private recNo As Integer
    Dim lblLabel(10) As Label
    Dim txtText(10) As TextBox
    Dim blnFileRead As Boolean

    Shared workbook As IWorkbook
    Shared workbook1 As IWorkbook
    Dim startRec As Integer
    Shared Sendworkbook As IWorkbook
    Dim Port, BaudRate, Parity, DataBits, StopBits As String
    Dim boolComvalues As Boolean = False
    Private WithEvents moRS232 As Rs232
    Dim DSXL As New DataSet()
    Dim DSFXL As New DataSet()

    Delegate Sub LoadPageCallback()

    Private Shared vposition As Integer

    Private Shared hposition As Integer



    Private Structure KBDLLHOOKSTRUCT
        Public key As Keys
        Public scanCode As Integer
        Public flags As Integer
        Public time As Integer
        Public extra As IntPtr
    End Structure

    'System level functions to be used for hook and unhook keyboard input
    Private Delegate Function LowLevelKeyboardProc(ByVal nCode As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
    <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
    Private Shared Function SetWindowsHookEx(ByVal id As Integer, ByVal callback As LowLevelKeyboardProc, ByVal hMod As IntPtr, ByVal dwThreadId As UInteger) As IntPtr
    End Function
    <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
    Private Shared Function UnhookWindowsHookEx(ByVal hook As IntPtr) As Boolean
    End Function
    <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
    Private Shared Function CallNextHookEx(ByVal hook As IntPtr, ByVal nCode As Integer, ByVal wp As IntPtr, ByVal lp As IntPtr) As IntPtr
    End Function
    <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
    Private Shared Function GetModuleHandle(ByVal name As String) As IntPtr
    End Function
    <DllImport("user32.dll", CharSet:=CharSet.Auto)> _
    Private Shared Function GetAsyncKeyState(ByVal key As Keys) As Short
    End Function
    <DllImport("user32.dll")> _
    Private Shared Function GetLastInputInfo(ByRef plii As LASTINPUTINFO) As Boolean
    End Function

    <StructLayout(LayoutKind.Sequential)> _
    Private Structure LASTINPUTINFO
        Public Shared ReadOnly SizeOf As Integer = Marshal.SizeOf(GetType(LASTINPUTINFO))

        <MarshalAs(UnmanagedType.U4)> _
        Public cbSize As Integer
        <MarshalAs(UnmanagedType.U4)> _
        Public dwTime As Integer
    End Structure
    'Declaring Global objects
    Private ptrHook As IntPtr
    Private objKeyboardProcess As LowLevelKeyboardProc

    Private Sub JobSheet_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        If ALT_F4 = True Then
            e.Cancel = True
            Return
        Else
            End
        End If
    End Sub

    Private Sub JobSheet_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles MyBase.KeyDown
        ALT_F4 = (e.KeyCode.Equals(Keys.F4) And e.Alt = True)
    End Sub
    Private Sub ReadingInputValues()

        Dim WB_Ded As IWorkbook = Nothing, WB_Fabric As IWorkbook = Nothing, WB_Drop As IWorkbook = Nothing
        Dim sheet As ISheet
        Dim strext As String = String.Empty
        Dim DS As DataSet

        Deductiontable = New DataTable()


        Try


            Using file As New FileStream(strDedFilepath, FileMode.Open, FileAccess.Read)
                strext = Path.GetExtension(strDedFilepath)
                If strext.ToUpper() = (".XLSX") Then
                    WB_Ded = New XSSFWorkbook(file)
                End If
                If strext.ToUpper() = ".XLS" Then
                    WB_Ded = New HSSFWorkbook(file)
                End If
            End Using







            Try
                If WB_Ded IsNot Nothing Then
                    sheet = WB_Ded.GetSheetAt(0)
                    Deductiontable = GetDSFromSheet(sheet, 0, 10, 20, "DEDUCTION")
                End If
            Catch ex As Exception
            Finally
                WB_Ded.Clear()
                WB_Ded = Nothing
                sheet = Nothing

            End Try





        Catch ex As Exception
        Finally

        End Try

    End Sub
    Protected Function GetDSFromSheet(ByRef Sheet, ByVal intCellstart, ByVal inCellEnd, ByVal intRowCount, ByVal inputname) As DataTable
        Dim headerRow As IRow, row As IRow
        Dim dataRow1 As DataRow = Nothing
        Dim DT As New DataTable
        'rows = Sheet.GetRowEnumerator()
        headerRow = Sheet.GetRow(0)
        For i As Integer = intCellstart To inCellEnd
            If headerRow.GetCell(i) IsNot Nothing Then
                If headerRow.GetCell(i).ToString <> String.Empty Then
                    Dim column As New DataColumn(headerRow.GetCell(i).StringCellValue.Trim)
                    DT.Columns.Add(column)
                    If headerRow.GetCell(i).ToString.ToUpper.Contains("FROM") Or headerRow.GetCell(i).ToString.ToUpper.Contains("TO") Or headerRow.GetCell(i).ToString.ToUpper.Contains("DEDUCTION") Then
                        column.DataType = System.Type.GetType("System.Int32")
                    End If
                End If
            End If
        Next
        Sheet.RemoveRow(headerRow)
        Dim rowCount As Integer = 20
        For i As Integer = (Sheet.FirstRowNum) To (Sheet.LastRowNum)
            row = Sheet.GetRow(i)
            dataRow1 = DT.NewRow()
            If row IsNot Nothing Then
                Try

                    For k As Integer = 0 To DT.Columns.Count - 1
                        If row.GetCell(k) IsNot Nothing Then
                            dataRow1(k) = row.GetCell(k).ToString().Trim.ToUpper
                        Else
                            Try
                                dataRow1(k) = row.Cells(k).ToString().Trim.ToUpper
                            Catch ex As Exception

                            End Try
                        End If
                    Next
                    If Convert.ToString(dataRow1(0)).Trim <> String.Empty Then
                        DT.Rows.Add(dataRow1)
                    End If
                Catch ex As Exception
                    ' WriteError_File(inputname + ex.Message.ToString)
                End Try

            End If
        Next
        Return DT
    End Function


    Private Sub JobSheet_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try

            intX = Screen.PrimaryScreen.Bounds.Width
            intY = Screen.PrimaryScreen.Bounds.Height

            Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None
            Me.WindowState = FormWindowState.Maximized
            'Me.TopMost = True

            cmbPageSize.SelectedIndex = 4
            pnlGrid.Height = Me.Height - 100
            pnlGrid.Width = Me.Width - 50
            GVDetail.Height = pnlGrid.Height - 20
            GVDetail.Width = pnlGrid.Width - 20



            Me.DoubleBuffered = True
            ChangeControlStyles(GVDetail, ControlStyles.OptimizedDoubleBuffer, True)

            pnlDisplayTitle.Visible = True




            If Reading_ServerINI_Values() = True Then

                If IO.Directory.Exists(strXMLDoc) = False Then
                    WriteError_File("PLEASE CONFIGURE XML FILE FOLDER PATH IN SERVER.INI")
                    MsgBox("PLEASE CONFIGURE XML FILE FOLDER PATH IN SERVER.INI")
                    Exit Sub
                End If


                If IO.File.Exists(strDedFilepath) = False Then
                    ' WriteError_File("PLEASE CONFIGURE EXCEL DEDUCTION FILE PATH IN SERVER.INI")
                    MsgBox("PLEASE CONFIGURE EXCEL DEDUCTION FILE PATH IN SERVER.INI")
                    Exit Sub
                End If


                If IO.File.Exists(strXLDoc) = False Then
                    WriteError_File("PLEASE CONFIGURE EXCEL DUMP FILE PATH IN SERVER.INI")
                    MsgBox("PLEASE CONFIGURE EXCEL DUMP FILE PATH IN SERVER.INI")
                    Exit Sub
                End If


                If IO.Directory.Exists(strCreatedXLDoc) = False Then
                    WriteError_File("PLEASE CONFIGURE EXCEL CREATION FILE PATH IN SERVER.INI")
                    MsgBox("PLEASE CONFIGURE EXCEL CREATION FILE PATH IN SERVER.INI")
                    Exit Sub
                End If
                If IO.File.Exists(strXLFDoc) = False Then
                    WriteError_File("PLEASE CONFIGURE FABRIC EXCEL FILE PATH IN SERVER.INI")
                    MsgBox("PLEASE CONFIGURE FABRIC EXCEL FILE PATH IN SERVER.INI")
                    Exit Sub
                End If
                Timer1.Enabled = True
            Else
                WriteError_File("PLEASE CONFIGURE FOLDER DETAILS")
                MsgBox("PLEASE CONFIGURE FOLDER DETAILS")
                Exit Sub
            End If
            If Reading_Com_INI_Values() = True Then
                boolComvalues = True
                moRS232 = New Rs232()
                With moRS232
                    .Port = CInt(Port)
                    .BaudRate = Int32.Parse(BaudRate)
                    .DataBit = 8
                    .StopBit = Rs232.DataStopBit.StopBit_1
                    If Parity.ToUpper = "NONE" Then
                        .Parity = Rs232.DataParity.Parity_None
                    ElseIf Parity.ToUpper = "EVEN" Then
                        .Parity = Rs232.DataParity.Parity_Even
                    ElseIf Parity.ToUpper = "ODD" Then
                        .Parity = Rs232.DataParity.Parity_Odd
                    End If
                    .Timeout = 3000 'Int32.Parse(txtTimeout.Text)
                End With
            Else
                boolComvalues = False
                WriteError_File("PLEASE CONFIGURE COMM. PORT VALUES")
                MsgBox("PLEASE CONFIGURE COMM. PORT VALUES")
                Exit Sub
            End If

            ReadingInputValues()


        Catch ex As Exception
            WriteError_File("Form Load Method " & ex.Message.ToString)
        End Try


    End Sub



    Private Function Reading_ServerINI_Values() As Boolean
        Dim Read As Boolean = True
        Dim i As Integer = 0
        Dim j As Integer = 0
        strXMLDoc = String.Empty : strXLDoc = String.Empty : strCreatedXLDoc = String.Empty : strXLFDoc = String.Empty : strDedFilepath = String.Empty

        Try
            Dim oRead As System.IO.StreamReader
            oRead = IO.File.OpenText(Application.StartupPath & "\Server.ini")

            While oRead.Peek <> -1
                If i = 0 Then
                    strXMLDoc = oRead.ReadLine()
                ElseIf i = 1 Then
                    strXLDoc = oRead.ReadLine()
                ElseIf i = 2 Then
                    strCreatedXLDoc = oRead.ReadLine()
                ElseIf i = 3 Then
                    StrPrinterName = oRead.ReadLine()
                ElseIf i = 4 Then
                    strXLFDoc = oRead.ReadLine()
                ElseIf i = 5 Then
                    strDedFilepath = oRead.ReadLine()
                ElseIf i = 6 Then
                    SpringDeduction = oRead.ReadLine()
                End If


                i = i + 1
            End While
            oRead.Close()

            If strXMLDoc = String.Empty Or strXLDoc = String.Empty Or strXLFDoc = String.Empty Then
                Read = False
            End If
        Catch ex As Exception
            WriteError_File("Reading ServerINI " & ex.Message.ToString)
            Read = False
        End Try
        Return Read
    End Function
    Private Function Reading_Com_INI_Values() As Boolean
        Dim Read As Boolean = True
        Dim i As Integer = 0
        Port = String.Empty : BaudRate = String.Empty : Parity = String.Empty
        Try

            Dim oRead As System.IO.StreamReader
            oRead = IO.File.OpenText(Application.StartupPath & "\Comm.ini")

            While oRead.Peek <> -1
                If i = 0 Then
                    Port = oRead.ReadLine()
                ElseIf i = 1 Then
                    BaudRate = oRead.ReadLine()
                ElseIf i = 2 Then
                    Parity = oRead.ReadLine()
                End If
                i = i + 1
            End While
            If Port = String.Empty Or BaudRate = String.Empty Or Parity = String.Empty Then
                Read = False
            End If

        Catch ex As Exception
            WriteError_File("Reading COMM-INI " & ex.Message.ToString)
            Read = False
        End Try
        Return Read
    End Function
    Private Sub KillExplorer()

        Dim taskKill As ProcessStartInfo = New ProcessStartInfo("taskkill", "/F /IM explorer.exe")
        taskKill.WindowStyle = ProcessWindowStyle.Hidden
        Dim Process As Process = New Process()
        Process.StartInfo = taskKill
        Process.Start()
        Process.WaitForExit()

    End Sub

    Private Sub GridDatasetInitialize()
        GVDetail.DataSource = Nothing
        GVDetail.Columns.Clear()
        GridDataSet = New DataSet
        GridDataSet.Tables.Add("Barcode")
        GridDataSet.Tables("Barcode").Columns.Add("CBNumber")
        GridDataSet.Tables("Barcode").Columns.Add("Item")
        GridDataSet.Tables("Barcode").Columns.Add("Qty")
        GridDataSet.Tables("Barcode").Columns.Add("Width")
        GridDataSet.Tables("Barcode").Columns.Add("Cut Width")
        GridDataSet.Tables("Barcode").Columns.Add("Tube")
        GridDataSet.Tables("Barcode").Columns.Add("Spring")
        GridDataSet.Tables("Barcode").Columns.Add("Finish")
        GridDataSet.Tables("Barcode").Columns.Add("Colour")
        GridDataSet.Tables("Barcode").Columns.Add("ImgChecked")
        GridDataSet.Tables("Barcode").Columns.Add("Drop")
        GridDataSet.Tables("Barcode").Columns.Add("Customer")
        GridDataSet.Tables("Barcode").Columns.Add("Type")
        GridDataSet.Tables("Barcode").Columns.Add("ControlType")
        GridDataSet.Tables("Barcode").Columns.Add("Lathe")
        GridDataSet.Tables("Barcode").Columns.Add("Alpha")
        GridDataSet.Tables("Barcode").Columns.Add("SRLineNumber")
        GridDataSet.Tables("Barcode").Columns.Add("Department")
        GridDataSet.Tables("Barcode").Columns.Add("Fabric")
        GridDataSet.Tables("Barcode").Columns.Add("Total")
        GridDataSet.Tables("Barcode").Columns.Add("LineNo")
        GVDetail.AllowUserToAddRows = False


    End Sub

    Public Function ColumnNumberToLetter(ByVal ColumnNumber As Integer) As String
        If ColumnNumber > 26 Then
            Return (ChrW(Math.Floor((CDbl(ColumnNumber) - 1) / 26) + 64)).ToString() + (ChrW(((ColumnNumber - 1) Mod 26) + 65)).ToString()
        End If
        Return (ChrW(ColumnNumber + 64)).ToString()
    End Function


    Private Sub LoadDataSet()
        'Dim Conn As SqlConnection = New SqlConnection(strConnectionString)

        Try
            Dim DSXML As New DataSet()
            Dim TotalQty As Integer

            Dim drarray() As DataRow
            Dim drFabric() As DataRow

            Dim filterExp As String = String.Empty
            Dim j As Integer, f As Integer, a As Integer
            'CHECK IF FILE EXISIT



            Dim fixml As New System.IO.DirectoryInfo(strXMLDoc)
            Dim filesxml = fixml.GetFiles.ToList

            'Dim firstxl As New System.IO.FileStream(strXLDoc, FileMode.Open)
            'Dim filesxl = fixl.GetFiles.ToList
            'Dim firstxl = (From file In filesxl Select file Order By file.CreationTime Ascending).FirstOrDefault



            'If firstxl = 0 And firstxl Is Nothing Then
            '    MsgBox("Excel File Not Exist !!")
            '    Exit Sub
            'End If

            'If filesxml.Count > 0 Then
            '    'LOAD EXCEL DATASET
            '    InitializeWorkbook(strXLDoc, Path.GetExtension(strXLDoc))
            '    DSXL = ConvertToDataTable(workbook)
            'End If

            For k As Integer = filesxml.Count - 1 To 0 Step -1
                Try
                    'GET XML DATASET
                    DSXML = GetXMLDataset(filesxml(k).FullName)
                    If DSXML.Tables.Count > 0 Then
                        If DSXML.Tables(0).Rows.Count > 0 Then
                            TotalQty = 0
                            'COMPARE LINE NUMBER
                            drarray = DSXL.Tables(0).Select("[Line No.]='" + DSXML.Tables(0).Rows(0)(0).ToString + "'")

                            For i = 0 To (drarray.Length - 1)
                                Try
                                    Dim DR As DataRow = GridDataSet.Tables(0).NewRow
                                    DR("CBNumber") = drarray(i)("W/Order NO").Trim

                                    ' Dim drarrayTemp() As DataRow = DSXL.Tables(0).Select("[W/Order NO]='" + DR("CBNumber") + "'", "[Line No.]" + " asc")
                                    Dim drarrayTemp() As DataRow = DSXL.Tables(0).Select("[W/Order NO]='" + DR("CBNumber") + "'")

                                    For j = 0 To UBound(drarrayTemp)
                                        If drarrayTemp(j)("Department").ToString.Trim <> "" Then
                                            Dim Qty As Integer = Integer.Parse(drarrayTemp(j)("Qty").ToString.Trim)
                                            TotalQty += Qty
                                        End If
                                        
                                    Next
                                    '15592142
                                    a = 1
                                    f = 65
                                    j = 1
                                    'WILL ADD CHAR ABCD FOR ITEM NUMBER
                                    For j = 0 To UBound(drarrayTemp)
                                        ' Dim Qty As Integer = Integer.Parse(drarrayTemp(j)("Qty").ToString.Trim)
                                        ' TotalQty += Qty
                                        'If drarrayTemp(j)("Line No.").ToString.Trim = drarray(i)("Line No.").ToString.Trim Then
                                        '    DR("Item") = ColumnNumberToLetter(f)    'Chnages done by Dhaval for Charater
                                        '    Exit For
                                        'End If
                                        If drarrayTemp(j)("Line No.").ToString.Trim = drarray(i)("Line No.").ToString.Trim Then
                                            DR("Item") = Chr(f).ToString
                                            Exit For
                                        End If

                                        f = f + 1

                                        If drarrayTemp(j)("Department").ToString.Trim <> "" Then
                                            Dim Qty As Integer = Integer.Parse(drarrayTemp(j)("Qty").ToString.Trim)
                                            a = a + Qty
                                        End If


                                    Next
                                    DR("Qty") = drarray(i)("Qty").Trim
                                    If IsDBNull(drarray(i)("Width")) = False Then
                                        If drarray(i)("Width").ToString().Trim <> String.Empty Then
                                            DR("Width") = drarray(i)("Width").Trim + "mm"
                                        End If
                                    Else
                                        DR("Width") = String.Empty
                                    End If

                                   

                                    DR("Tube") = drarray(i)("Tube Size").Trim

                                    If drarray(i)("Bind. Type/# Panels/Rope/Operation").ToString.Trim = "SPRING" Or drarray(i)("Bind. Type/# Panels/Rope/Operation").ToString.Trim = "SPRINGHD" Then
                                        DR("Spring") = "YES"
                                        'DR("Cut Width") = (Convert.ToInt32(drarray(i)("Width").ToString.Trim) - SpringDeduction).ToString + "mm"
                                        DR("Cut Width") = GetCutwidth(drarray(i)("Width").ToString.Trim, drarray(i)("Bind. Type/# Panels/Rope/Operation").ToString.Trim).Trim()
                                    Else
                                        DR("Spring") = "NO"
                                        DR("Cut Width") = GetCutwidth(drarray(i)("Width").ToString.Trim, drarray(i)("Bind. Type/# Panels/Rope/Operation").ToString.Trim).Trim
                                        If DR("Cut Width") <> String.Empty Then
                                            DR("Cut Width") = DR("Cut Width") + "mm"
                                        End If

                                    End If

                                    If drarray(i)("Description").ToString.Trim.Length > 6 Then
                                        DR("Finish") = drarray(i)("Description").ToString.Trim.Substring(drarray(i)("Description").ToString.Trim.Length - 6)
                                    Else
                                        DR("Finish") = DR("Finish")
                                    End If

                                    'TODO IF Statment

                                    If drarray(i)("Pull Type / Control Type /Draw Type").ToString.Trim = "OVAL ALUMINIUM BOTTOM BAR" Then
                                        DR("Colour") = drarray(i)("Pull Colour/Bottom Weight/Wand Len").Trim
                                    Else
                                        drFabric = DSFXL.Tables(0).Select("[Fabric]='" + drarray(i)("Fabric").ToString.Trim + "'")
                                        If drFabric.Count > 0 Then
                                            DR("Colour") = "PVC Lathe"
                                        Else
                                            DR("Colour") = "Lathe"
                                        End If
                                    End If


                                    DR("ImgChecked") = 0

                                    'Data for Label
                                    DR("SRLineNumber") = a.ToString
                                    If IsDBNull(drarray(i)("Drop")) = False Then
                                        If drarray(i)("Width").ToString().Trim <> String.Empty Then
                                            DR("Drop") = drarray(i)("Drop").Trim + "mm"
                                        End If
                                    Else
                                        DR("Drop") = String.Empty
                                    End If
                                    DR("Customer") = drarray(i)("Customer Name 1").Trim
                                    DR("Type") = drarray(i)("Track Col/Roll Type/Batten Col").Trim 'STD
                                    DR("ControlType") = drarray(i)("Pull Colour/Bottom Weight/Wand Len").Trim '"ICE"
                                    DR("Lathe") = DR("Finish").Trim 'Fin 31
                                    DR("Alpha") = DR("item").Trim 'A
                                    DR("Department") = drarray(i)("Department").Trim
                                    If drarray(i)("Fabric").ToString.Trim.Length > 6 Then
                                        DR("Fabric") = drarray(i)("Fabric").ToString.TrimEnd.Substring(0, drarray(i)("Fabric").ToString.TrimEnd.Length - 6) ' vibe
                                    Else
                                        DR("Fabric") = drarray(i)("Fabric").ToString.Trim
                                    End If
                                    DR("Total") = TotalQty 'drarrayTemp.Length

                                    DR("LineNo") = drarray(i)("Line No.").ToString.Trim
                                    GridDataSet.Tables(0).Rows.Add(DR)
                                Catch ex As Exception
                                    WriteError_File("Adding XML To Dataset " & ex.Message.ToString)
                                End Try
                            Next

                        End If
                    End If
                Catch ex As Exception
                    WriteError_File("XML File Reading " & ex.Message.ToString)
                End Try
                filesxml(k).Delete()
            Next
            GridDataSet.AcceptChanges()

            'Filters the column data for DataGrid
            GridDataTabel = GridDataSet.Tables(0).DefaultView.ToTable(False, "CBNumber", "Item", "Qty", "Width", "Cut Width", "Tube", "Spring", "Finish", "Colour", "ImgChecked")
            maxRec = GridDataTabel.Rows.Count

            PageCount = RoundUp(Convert.ToDecimal(GridDataTabel.Rows.Count / pageSize).ToString())

        Catch ex As Exception
            WriteError_File("Loading Dataset " & ex.Message.ToString)
            MsgBox(ex.Message, MsgBoxStyle.Information)
        Finally
            'Conn.Close()
        End Try
    End Sub
    Function RoundUp(ByVal X As Double) As Long

        If X = Math.Round(X, 0) Then

            RoundUp = X

        Else

            RoundUp = Math.Round(X + 0.5, 0)

        End If

    End Function

    Protected Function GetXMLDataset(ByVal Filename As String) As DataSet
        Dim ds As New DataSet
        Dim fsReadXml As New System.IO.FileStream(Filename, System.IO.FileMode.Open)
        'Dim lines As String() = IO.File.ReadAllLines(Application.StartupPath & "\Documents\SOPSystem.txt")
        Try
            ds.ReadXml(fsReadXml)
        Catch ex As Exception
            WriteError_File("Reading XML To Dataset " & ex.Message.ToString)
            MessageBox.Show(ex.ToString())
        Finally
            fsReadXml.Close()
        End Try
        Return ds
    End Function
    Private Sub cmdNext_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdNext.Click
        'If cmdPrevious.Enabled = False Then cmdPrevious.Enabled = True

        'If pageSize = 0 Then
        '    cmdNext.Enabled = False
        '    Exit Sub
        'End If

        'currentPage = currentPage + 1

        'If currentPage > PageCount Then
        '    currentPage = PageCount
        '    'Check if you are already at the last page.
        '    If recNo = maxRec Then
        '        cmdNext.Enabled = False
        '        Exit Sub
        '    End If
        'End If

        If cmdPrevious.Enabled = False Then cmdPrevious.Enabled = True

        currentPage = currentPage + 1
        If currentPage >= PageCount Then
            currentPage = PageCount
            cmdNext.Enabled = False
        End If

        LoadPage()
    End Sub

    Private Sub FillGrid()
        Try


            'Set the start and max records. 
            pageSize = cmbPageSize.Text
            maxRec = GridDataTabel.Rows.Count
            PageCount = maxRec \ pageSize

            ' Adjust the page number if the last page contains a partial page.
            If (maxRec Mod pageSize) > 0 Then
                PageCount = PageCount + 1
            End If

            'Initial seeings
            currentPage = 1
            recNo = 0

            LoadPage()



        Catch ex As Exception

        End Try
    End Sub

    Private Sub GetvSBarPos(ByVal dg As DataGridView)
        Try
            vposition = dg.FirstDisplayedScrollingRowIndex
        Catch ex As Exception

        End Try
    End Sub
    Private Sub SetvSBarPos(ByVal dg As DataGridView)
        Try
            dg.FirstDisplayedScrollingRowIndex() = vposition
        Catch ex As Exception

        End Try
    End Sub
    Private Sub GethSBarPos(ByVal dg As DataGridView)
        Try
            hposition = dg.FirstDisplayedScrollingColumnIndex
        Catch ex As Exception

        End Try
    End Sub
    Private Sub SethSBarPos(ByVal dg As DataGridView)
        Try
            dg.FirstDisplayedScrollingColumnIndex = hposition
        Catch ex As Exception

        End Try


    End Sub
    Private Sub ChangeControlStyles(ByVal ctrl As Control, ByVal flag As ControlStyles, ByVal value As Boolean)
        Dim method As MethodInfo = ctrl.[GetType]().GetMethod("SetStyle", BindingFlags.Instance Or BindingFlags.NonPublic)
        If method IsNot Nothing Then
            method.Invoke(ctrl, New Object() {flag, value})
        End If

    End Sub

    Private Sub LoadPage()
        Try


            If GVDetail.InvokeRequired Then
                Dim d As New LoadPageCallback(AddressOf LoadPage)
                Me.Invoke(d)
            Else

                GetvSBarPos(GVDetail)

                GethSBarPos(GVDetail)

                Dim i As Integer

                Dim endRec As Integer
                Dim dtTemp As DataTable

                'Duplicate or clone the source table to create the temporary table.

                GVDetail.DataSource = Nothing
                GVDetail.Columns.Clear()

                dtTemp = GridDataTabel.Clone

                If currentPage = PageCount Then
                    endRec = maxRec
                Else
                    endRec = pageSize * currentPage
                End If


                If currentPage = 1 Then
                    startRec = 1
                Else
                    startRec = (pageSize * (currentPage - 1)) + 1
                End If




                GVDetail.DataSource = dtTemp
                'GVDetail.DataMember = "Barcode"



                GVDetail.Columns(0).Width = Convert.ToInt64(GVDetail.Width * 0.1)
                GVDetail.Columns(0).HeaderText = "CB Number"

                GVDetail.Columns(1).Width = Convert.ToInt64(GVDetail.Width * 0.1)
                GVDetail.Columns(1).HeaderText = "Item"

                GVDetail.Columns(2).Width = Convert.ToInt64(GVDetail.Width * 0.05)
                GVDetail.Columns(2).HeaderText = "Qty"
                GVDetail.Columns(2).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight

                GVDetail.Columns(3).Width = Convert.ToInt64(GVDetail.Width * 0.05)
                GVDetail.Columns(3).HeaderText = "Width"
                GVDetail.Columns(3).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight

                GVDetail.Columns(4).Width = Convert.ToInt64(GVDetail.Width * 0.1)
                GVDetail.Columns(4).HeaderText = "Cut Width"


                GVDetail.Columns(5).Width = Convert.ToInt64(GVDetail.Width * 0.1)
                GVDetail.Columns(5).HeaderText = "Tube"
                ' GVDetail.Columns(5).DefaultCellStyle.ForeColor = Color.Red





                GVDetail.Columns(6).Width = Convert.ToInt64(GVDetail.Width * 0.1)
                GVDetail.Columns(6).HeaderText = "Spring"

                GVDetail.Columns(7).Width = Convert.ToInt64(GVDetail.Width * 0.1)
                GVDetail.Columns(7).HeaderText = "Finish"

                GVDetail.Columns(8).Width = Convert.ToInt64(GVDetail.Width * 0.18)
                GVDetail.Columns(8).HeaderText = "Colour"




                'Dim ImgChecked As New DataGridViewColumn()
                'GVDetail.Columns.Add("ImgChecked", String.Empty)
                GVDetail.Columns(9).Width = 0
                GVDetail.Columns(9).HeaderText = "ImgChecked"
                GVDetail.Columns(9).Visible = False



                Dim img As New DataGridViewImageColumn()

                Dim inImg As Image = Image.FromFile(Application.StartupPath & "\blank.png")
                inImg = New Bitmap(Convert.ToInt32(GVDetail.Width * 0.055), inImg.Height)
                img.Image = inImg
                GVDetail.Columns.Add(img)
                img.HeaderText = "Select"
                img.Name = "img"
                GVDetail.Columns(10).Width = Convert.ToInt64(GVDetail.Width * 0.055)
                ' GVDetail.Columns(10).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight

                recNo = 0
                If GridDataTabel.Rows.Count > 0 Then
                    'Copy the rows from the source table to fill the temporary table.
                    For i = startRec To endRec
                        dtTemp.ImportRow(GridDataTabel.Rows(i - 1))

                        If GridDataTabel.Rows(i - 1)(9).ToString = "0" Then
                            Dim inImg1 As Image = Image.FromFile(Application.StartupPath & "\blank.png")
                            inImg1 = New Bitmap(Convert.ToInt32(GVDetail.Width * 0.055), inImg1.Height)
                            GVDetail(10, recNo).Value = inImg1
                            GVDetail(5, recNo).Style.ForeColor = Color.Red
                        Else
                            GVDetail(10, recNo).Value = New Bitmap(Application.StartupPath & "\TickMark.png")
                            GVDetail.Columns(5).DefaultCellStyle.ForeColor = Color.Red
                        End If
                        recNo = recNo + 1
                    Next
                End If

                'GVDetail.Refresh()
                SetvSBarPos(GVDetail)
                SethSBarPos(GVDetail)

            End If
        Catch ex As Exception
            WriteError_File("Load Page Method " & ex.Message.ToString)
        End Try


    End Sub


    Private Sub cmdPrevious_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdPrevious.Click

        If cmdNext.Enabled = False Then cmdNext.Enabled = True

        currentPage = currentPage - 1

        If currentPage <= 1 Then
            currentPage = 1
            cmdPrevious.Enabled = False
        End If

        LoadPage()
    End Sub




    Public Sub New()
        Dim objCurrentModule As ProcessModule = Process.GetCurrentProcess().MainModule
        'Get Current Module
        objKeyboardProcess = New LowLevelKeyboardProc(AddressOf captureKey)
        'Assign callback function each time keyboard process
        ptrHook = SetWindowsHookEx(13, objKeyboardProcess, GetModuleHandle(objCurrentModule.ModuleName), 0)
        'Setting Hook of Keyboard Process for current module
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Private Function captureKey(ByVal nCode As Integer, ByVal wp As IntPtr, ByVal lp As IntPtr) As IntPtr
        If nCode >= 0 Then
            Dim objKeyInfo As KBDLLHOOKSTRUCT = DirectCast(Marshal.PtrToStructure(lp, GetType(KBDLLHOOKSTRUCT)), KBDLLHOOKSTRUCT)
            If objKeyInfo.key = Keys.RWin OrElse objKeyInfo.key = Keys.LWin Then
                ' Disabling Windows keys
                Return CType(1, IntPtr)
            End If
            If objKeyInfo.key = Keys.ControlKey OrElse objKeyInfo.key = Keys.Escape Then
                ' Disabling Ctrl + Esc keys
                Return CType(1, IntPtr)
            End If
        End If
        Return CallNextHookEx(ptrHook, nCode, wp, lp)
    End Function

    Private Sub cmbPageSize_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbPageSize.SelectedIndexChanged
        If cmdNext.Enabled = False Then cmdNext.Enabled = True
        If cmdPrevious.Enabled = False Then cmdPrevious.Enabled = True
        FillGrid()

    End Sub





    Private Sub cmdNext_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdNext.MouseHover
        ToolTip1.SetToolTip(cmdNext, "NEXT RECORDS")
    End Sub





    Private Sub cmdPrevious_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdPrevious.MouseHover
        ToolTip1.SetToolTip(cmdNext, "PREVIOUS RECORDS")
    End Sub

    Private Sub cmdClose_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs)
        ToolTip1.SetToolTip(cmdClose, "CLOSE APPLICATION")
    End Sub





    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick

        Try
            Timer1.Enabled = False

            GridDatasetInitialize()

            'If Path.GetExtension(strXLDoc).ToUpper() = (".XLSM") Then
            '    ConvertFilePath(strXLDoc)
            'End If

            'strXLDoc = Path.ChangeExtension(strXLDoc, "xlsx")
            'TODO: 
            InitialiseFabricWorkbook(strXLFDoc, Path.GetExtension(strXLFDoc))
            InitializeWorkbook(strXLDoc, Path.GetExtension(strXLDoc))
            DSXL = ConvertToDataTable(workbook)
            'TODO: 
            DSFXL = ConvertToDataTable1(workbook1)

            LoadDataSet()
            FillGrid()

            bgWorker.RunWorkerAsync()
        Catch ex As Exception
            WriteError_File("Background Process " & ex.Message.ToString)
            MsgBox(ex.Message, MsgBoxStyle.Information)
        End Try

    End Sub
    Private Sub ConvertFilePath(ByVal filename As String)
        Dim workbook As XSSFWorkbook

        'read original xlsm file into workbook
        Using file As New FileStream(filename, FileMode.Open, FileAccess.Read)
            workbook = New XSSFWorkbook(file)
        End Using

        'change file extension to xlsx and save in a new location
        filename = Path.ChangeExtension(filename, "xlsx")
        If Not Directory.Exists("NewFile") Then
            Directory.CreateDirectory("NewFile")
        End If
        Dim stream As New FileStream((filename), FileMode.Create, System.IO.FileAccess.Write)
        workbook.Write(stream)
        stream.Close()

        'read the newly created file from the new location
        Using file As New FileStream(filename, FileMode.Open, FileAccess.Read)
            workbook = New XSSFWorkbook(file)
        End Using
    End Sub
    'Private Sub cmdBack_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
    '    Dim objWriter As System.IO.StreamWriter
    '    objWriter = Nothing
    '    Dim strStringName As String

    '    If IO.File.Exists(Application.StartupPath & "\TextDisplay.txt") = True Then
    '        IO.File.Delete(Application.StartupPath & "\TextDisplay.txt")
    '    End If

    '    objWriter = New System.IO.StreamWriter((Application.StartupPath & "\TextDisplay.txt").ToString)

    '    strStringName = "Title|" & lbl0.Text
    '    objWriter.WriteLine(strStringName)
    '    For i = 1 To 5
    '        strStringName = lblLabel(i).Text & "|" & txtText(i).Text
    '        objWriter.WriteLine(strStringName)
    '    Next
    '    objWriter.Dispose()
    '    pnlGrid.Visible = True
    '    pnlDocument.Visible = True
    '    'pnlDisplayTitle.Visible = False
    'End Sub

    'Private Sub CreateDisplayControl()
    '    Dim strFileData() As String
    '    Dim i As Integer
    '    Try
    '        If IO.File.Exists(Application.StartupPath & "\TextDisplay.txt") = True Then
    '            Dim lines As String() = IO.File.ReadAllLines(Application.StartupPath & "\TextDisplay.txt")



    '            If UBound(lines) > 0 Then
    '                strFileData = lines(0).Split("|")
    '                lbl0.Text = strFileData(1)
    '            End If

    '            For i = 1 To UBound(lines)
    '                strFileData = lines(i).Split("|")
    '                lblLabel(i).Text = strFileData(0)
    '                txtText(i).Text = strFileData(1)
    '            Next

    '        End If
    '    Catch ex As Exception

    '    End Try
    'End Sub

    Private Shared Sub InitialiseFabricWorkbook(ByVal path As String, ByVal ext As String)

        Using file As New FileStream(path, FileMode.Open, FileAccess.Read)
            If ext.ToUpper() = (".XLSX") Then
                workbook1 = New XSSFWorkbook(file)
            End If
            If ext.ToUpper() = ".XLS" Then
                workbook1 = New HSSFWorkbook(file)
            End If
            If ext.ToUpper() = ".XLSM" Then

                workbook1 = New XSSFWorkbook(file)
            End If
        End Using
    End Sub


    Private Shared Sub InitializeWorkbook(ByVal path As String, ByVal ext As String)

        Using file As New FileStream(path, FileMode.Open, FileAccess.Read)
            If ext.ToUpper() = (".XLSX") Then
                workbook = New XSSFWorkbook(file)
            End If
            If ext.ToUpper() = ".XLS" Then
                workbook = New HSSFWorkbook(file)
            End If
            If ext.ToUpper() = ".XLSM" Then

                workbook = New XSSFWorkbook(file)
            End If
        End Using
    End Sub
    Private Shared Sub InitializeWorkbookCreate(ByVal path As String)

        Using file As New FileStream(path, FileMode.Open, FileAccess.Read)

            Sendworkbook = New XSSFWorkbook(file)

        End Using
    End Sub
    Protected Function ConvertToDataTable(ByVal workbook As IWorkbook) As DataSet
        Dim DS As New DataSet()
        Dim dt As New DataTable()
        Try
            Dim sheet As ISheet = workbook.GetSheetAt(0)

            Dim rows As System.Collections.IEnumerator = sheet.GetRowEnumerator()


            Dim headerRow As IRow = Nothing


            headerRow = sheet.GetRow(0)


            Dim cellCount As Integer = headerRow.LastCellNum
            Dim cellNo1 As Integer = headerRow.FirstCellNum

            For i As Integer = cellNo1 To cellCount - 1
                If headerRow.GetCell(i) IsNot Nothing Then
                    Dim column As New DataColumn(headerRow.GetCell(i).StringCellValue.Trim)
                    dt.Columns.Add(column)
                    cellCount = dt.Columns.Count
                End If
            Next


            sheet.RemoveRow(headerRow)

            Dim rowCount As Integer = sheet.LastRowNum
            For i As Integer = (sheet.FirstRowNum) To sheet.LastRowNum
                Dim row As IRow = sheet.GetRow(i)
                Dim dataRow As DataRow = dt.NewRow()
                If row IsNot Nothing Then
                    For j As Integer = cellNo1 To cellCount - 1
                        'Try
                        '    dataRow(j) = row.Cells(j).ToString()
                        'Catch ex As Exception
                        '    Exit For


                        'End Try
                        If row.GetCell(j) IsNot Nothing Then
                            dataRow(j) = row.GetCell(j).ToString()
                        Else
                            Try
                                dataRow(j) = row.Cells(j).ToString()
                                'Continue For
                            Catch ex As Exception
                                Exit For
                            End Try
                        End If
                    Next
                    If Convert.ToString(dataRow("W/Order NO")) <> String.Empty Then
                        dt.Rows.Add(dataRow)
                    End If
                End If
            Next
            DS.Tables.Add(dt)
        Catch ex As Exception
            MsgBox(ex.Message.ToString, MsgBoxStyle.Information)
            WriteError_File("Load Page Method " & ex.Message.ToString)
        End Try


        Return DS

    End Function
    Protected Function ConvertToDataTable1(ByVal workbook As IWorkbook) As DataSet
        Dim DS As New DataSet()
        Dim dt As New DataTable()
        Try
            Dim sheet As ISheet = workbook.GetSheetAt(0)

            Dim rows As System.Collections.IEnumerator = sheet.GetRowEnumerator()


            Dim headerRow As IRow = Nothing


            headerRow = sheet.GetRow(0)


            Dim cellCount As Integer = headerRow.LastCellNum
            Dim cellNo1 As Integer = headerRow.FirstCellNum

            For i As Integer = cellNo1 To cellCount - 1
                If headerRow.GetCell(i) IsNot Nothing Then
                    Dim column As New DataColumn(headerRow.GetCell(i).StringCellValue.Trim)
                    dt.Columns.Add(column)
                    cellCount = dt.Columns.Count
                End If
            Next


            sheet.RemoveRow(headerRow)

            Dim rowCount As Integer = sheet.LastRowNum
            For i As Integer = (sheet.FirstRowNum) To sheet.LastRowNum
                Dim row As IRow = sheet.GetRow(i)
                Dim dataRow As DataRow = dt.NewRow()
                If row IsNot Nothing Then
                    For j As Integer = cellNo1 To cellCount - 1
                        'Try
                        '    dataRow(j) = row.Cells(j).ToString()
                        'Catch ex As Exception
                        '    Exit For


                        'End Try
                        If row.GetCell(j) IsNot Nothing Then
                            dataRow(j) = row.GetCell(j).ToString()
                        Else
                            Try
                                dataRow(j) = row.Cells(j).ToString()
                                'Continue For
                            Catch ex As Exception
                                Exit For
                            End Try
                        End If
                    Next
                    If Convert.ToString(dataRow("Fabric")) <> String.Empty Then
                        dt.Rows.Add(dataRow)
                    End If
                End If
            Next
            DS.Tables.Add(dt)
        Catch ex As Exception
            MsgBox(ex.Message.ToString, MsgBoxStyle.Information)
            WriteError_File("Load Page Method " & ex.Message.ToString)
        End Try


        Return DS

    End Function

    Protected Function ConvertToDataTableSend(ByVal workbook As IWorkbook) As DataSet
        Dim DS As New DataSet()
        Dim dt As New DataTable()

        Try
            Dim sheet As ISheet = workbook.GetSheetAt(0)

            Dim rows As System.Collections.IEnumerator = sheet.GetRowEnumerator()

            Dim headingRow As IRow = Nothing
            Dim headerRow As IRow = Nothing

            headingRow = sheet.GetRow(0)
            headerRow = sheet.GetRow(1)

            Dim cellCount As Integer = headerRow.LastCellNum

            For i As Integer = headerRow.FirstCellNum To cellCount - 1
                If headerRow.GetCell(i) IsNot Nothing Then
                    Dim column As New DataColumn(headerRow.GetCell(i).StringCellValue.Trim)
                    dt.Columns.Add(column)
                    cellCount = dt.Columns.Count
                End If
            Next

            sheet.RemoveRow(headingRow)
            sheet.RemoveRow(headerRow)

            Dim rowCount As Integer = sheet.LastRowNum
            For i As Integer = (sheet.FirstRowNum) To sheet.LastRowNum
                Dim row As IRow = sheet.GetRow(i)
                Dim dataRow As DataRow = dt.NewRow()
                If row IsNot Nothing Then
                    For j As Integer = row.FirstCellNum To cellCount - 1
                        If row.GetCell(j) IsNot Nothing Then
                            dataRow(j) = row.GetCell(j).ToString()
                        End If
                    Next
                    dt.Rows.Add(dataRow)
                End If
            Next

            DS.Tables.Add(dt)
        Catch ex As Exception

        End Try

        Return DS

    End Function

    Private Sub cmdClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdClose.Click
        If MsgBox("ARE YOU SURE YOU WANT TO EXIT APPLICATION", MsgBoxStyle.YesNo, "EzyStop APPLICATION") = MsgBoxResult.No Then Exit Sub
        If bgWorker.IsBusy Then
            bgWorker.CancelAsync()
            System.Threading.Thread.Sleep(500)
            bgWorker.Dispose()
        End If
        If GVDetail.Rows.Count > 0 Then

            For i = 1 To GridDataSet.Tables(0).Rows.Count
                Try
                    Dim strGUID As String
                    strGUID = System.Guid.NewGuid().ToString
                    Dim time As DateTime = DateTime.Now
                    Dim filename As String = strXMLDoc & "\XML-" & strGUID & ".xml"
                    Dim xw As New XmlTextWriter(filename, System.Text.Encoding.UTF8)
                    xw.Formatting = Formatting.Indented
                    xw.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'")
                    xw.WriteStartElement("OrderObject")
                    xw.WriteStartElement("OrderNumber")
                    xw.WriteString(GridDataSet.Tables(0).Rows(i - 1)("LineNo").ToString())

                    xw.WriteEndElement()
                    xw.WriteEndElement()
                    xw.Close()


                Catch ex As System.IO.FileNotFoundException
                    WriteError_File("Exit Page Method " & ex.Message.ToString)

                End Try
            Next

        End If

        Application.Exit()
    End Sub
    Private Sub writexml(ByVal orderNo As [String])





        
      


    End Sub

    Private Function cellStyle1() As ICellStyle
        Dim font1 As IFont = Sendworkbook.CreateFont()
        font1.Color = HSSFColor.Red.Index
        font1.IsItalic = True
        font1.Underline = FontUnderlineType.Double
        font1.FontHeightInPoints = 20

        Dim style1 As ICellStyle = Sendworkbook.CreateCellStyle()
        style1.SetFont(font1)
        Return style1
    End Function
    Private Function cellStyle2() As ICellStyle
        Dim font1 As IFont = Sendworkbook.CreateFont()
        font1.Color = HSSFColor.Plum.Index
        font1.Boldweight = Convert.ToInt16(FontBoldWeight.Bold)
        font1.FontHeightInPoints = 15
        Dim style2 As ICellStyle = Sendworkbook.CreateCellStyle()
        style2.SetFont(font1)
        Return style2
    End Function
    Private Function cellStyle3() As ICellStyle
        Dim font1 As IFont = Sendworkbook.CreateFont()

        Dim style3 As ICellStyle = Sendworkbook.CreateCellStyle()
        style3.SetFont(font1)
        style3.BorderBottom = BorderStyle.Thin
        style3.BorderTop = BorderStyle.Thin
        style3.BorderLeft = BorderStyle.Thin
        style3.BorderRight = BorderStyle.Thin
        style3.WrapText = True
        Return style3
    End Function




    Private Sub cmdSend_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSend.Click
        Dim FileExist As Boolean = False
        Dim DS As New DataSet
        Dim fixl As New System.IO.DirectoryInfo(strCreatedXLDoc)
        Dim filesxl = fixl.GetFiles.ToList
        Dim firstxl = (From file In filesxl Select file Where file.CreationTime.Date = Today.Date).FirstOrDefault
        Dim drarray() As DataRow
        Dim strDataReceived As String = String.Empty, strErrorMessage As String = String.Empty, strRS232Width As String = String.Empty

        Try

            drarray = GridDataSet.Tables(0).Select("ImgChecked='1'")
            If (drarray.Length <= 0) Then
                WriteError_File("Select Records to Send !!")
                MsgBox("Select Records to Send !!")
                Exit Sub
            End If
            If (StrPrinterName Is Nothing Or StrPrinterName = String.Empty) Then
                WriteError_File("Invalid Printer !!")
                MsgBox("Invalid Printer !!")
                Exit Sub
            End If
            If filesxl.Count > 0 And firstxl IsNot Nothing Then
                InitializeWorkbookCreate(firstxl.FullName)
                DS = ConvertToDataTableSend(Sendworkbook)
                File.Delete(firstxl.FullName)
                Sendworkbook = New XSSFWorkbook()
            Else
                Dim DT As DataTable = GridDataSet.Tables(0).DefaultView.ToTable(False, "CBNumber", "Item", "Qty", "Width", "Cut Width", "Tube", "Spring", "Finish", "Colour", "LineNo").Clone

                DS.Tables.Add(DT)

                Sendworkbook = New XSSFWorkbook()
            End If

            Dim DRLabelArray(drarray.Count - 1) As String
            Dim strconcat As String = String.Empty
            For i As Integer = 0 To drarray.Count - 1
                Dim DR As DataRow = DS.Tables(0).NewRow

                DR("CBNumber") = drarray(i)("CBNumber")
                DR("Item") = drarray(i)("Item")
                DR("Qty") = drarray(i)("Qty")
                DR("Width") = drarray(i)("Width")
                DR("Cut Width") = drarray(i)("Cut Width")
                DR("Tube") = String.Empty
                DR("Spring") = drarray(i)("Spring")
                DR("Finish") = drarray(i)("Finish")
                DR("Colour") = drarray(i)("Colour")
                DR("LineNo") = drarray(i)("LineNo")
                strconcat = drarray(i)("CBNumber") & "@" & drarray(i)("Width")
                strconcat += "@" & drarray(i)("Drop") & "@" & drarray(i)("Customer") & "@" & drarray(i)("Department")
                strconcat += "@" & drarray(i)("Type") & "@" & drarray(i)("Fabric") & "@" & drarray(i)("Colour")
                'strconcat += "@" & drarray(i)("ControlType") & "@" & drarray(i)("Lathe")
                strconcat += "@ " & "@" & drarray(i)("Lathe")
                strconcat += "@" & drarray(i)("Alpha") & "@" & drarray(i)("CBNumber") '& "@" '& drarray(i)("SRLineNumber")
                If (drarray(i)("Qty") > 1) Then
                    strconcat += "@" & drarray(i)("SRLineNumber") & ",#"
                Else
                    strconcat += "@" & drarray(i)("SRLineNumber")
                End If

                strconcat += "@" & drarray(i)("Total")
                DRLabelArray(i) = strconcat
                strRS232Width += drarray(i)("Cut Width").ToString().Replace("mm", "") + "#"
                DS.Tables(0).Rows.Add(DR)
                GridDataSet.Tables(0).Rows.Remove(drarray(i))
            Next

            DS.AcceptChanges()

            Try
                moRS232.Open()
                '// Set state of RTS / DTS
                moRS232.Dtr = False '(chkDTR.CheckState = CheckState.Checked)
                moRS232.Rts = False
                moRS232.PurgeBuffer(Rs232.PurgeBuffers.TxClear Or Rs232.PurgeBuffers.RXClear)
                moRS232.Write(strRS232Width)
                moRS232.Close()
            Catch ex As Exception
                WriteError_File("Writing to RS232 " & ex.Message)
                MsgBox(ex.Message, MsgBoxStyle.Information)

                Exit Sub
            End Try




            Dim style1 As ICellStyle = cellStyle1()

            Dim style2 As ICellStyle = cellStyle2()
            Dim style3 As ICellStyle = cellStyle3()

            Try
                Dim sheet1 As XSSFSheet
                If Sendworkbook.GetSheet("Ezystop") Is Nothing Then
                    sheet1 = Sendworkbook.CreateSheet("Ezystop")
                Else
                    sheet1 = Sendworkbook.GetSheet("Ezystop")
                End If

                Dim headerRow0 As IRow = sheet1.CreateRow(0)
                Dim headerColRow1 As IRow = sheet1.CreateRow(1)

                For i As Integer = 0 To DS.Tables(0).Columns.Count - 1
                    headerRow0.CreateCell(i)
                    If i = 0 Then
                        headerRow0.GetCell(0).SetCellValue("Ezystop")
                        headerRow0.GetCell(0).CellStyle = style1
                        headerRow0.GetCell(0).CellStyle.Alignment = HorizontalAlignment.Center
                        headerRow0.GetCell(0).CellStyle.VerticalAlignment = VerticalAlignment.Center
                    End If
                    headerColRow1.CreateCell(i).SetCellValue(DS.Tables(0).Columns(i).ColumnName.ToString)
                    headerColRow1.GetCell(i).CellStyle = style2
                    sheet1.AutoSizeColumn(headerColRow1.GetCell(i).ColumnIndex)
                    sheet1.SetColumnWidth(headerColRow1.GetCell(i).ColumnIndex, 75 * 50)

                    headerColRow1.GetCell(i).CellStyle.BorderBottom = BorderStyle.Thin
                    headerColRow1.GetCell(i).CellStyle.BorderTop = BorderStyle.Thin
                    headerColRow1.GetCell(i).CellStyle.BorderLeft = BorderStyle.Thin
                    headerColRow1.GetCell(i).CellStyle.BorderRight = BorderStyle.Thin
                Next
                sheet1.AddMergedRegion(New NPOI.SS.Util.CellRangeAddress(0, 0, 0, 8))

                For i As Integer = 0 To DS.Tables(0).Rows.Count - 1
                    Dim wrow As IRow = sheet1.CreateRow(i + 2)
                    For j As Integer = 0 To DS.Tables(0).Columns.Count - 1
                        Dim wCell As ICell = wrow.CreateCell(j)
                        wCell.SetCellValue(DS.Tables(0).Rows(i)(j).ToString)
                        wCell.CellStyle = style3
                    Next
                Next


                Dim sw As FileStream = File.Create(strCreatedXLDoc & "\Ezystop" & DateTime.Now.ToString("dd-MMM-yyyy") & ".xlsx")
                Sendworkbook.Write(sw)
                sw.Close()

            Catch ex As Exception
                WriteError_File("Writing To Excel " & ex.Message)
                MsgBox(ex.Message)
                Exit Sub
            End Try


            Dim strpara() As String
            Dim strParameterArray() As String
            For k As Integer = 0 To DRLabelArray.Count - 1
                strParameterArray = DRLabelArray(k).ToString().Split("@")
                strpara = strParameterArray

                If UBound(strpara) = 0 Then
                    End
                Else

                    PrintReport(1, StrPrinterName, strParameterArray)


                End If
            Next

            currentPage = 1
        Catch ex As Exception
            WriteError_File("Send Click " & ex.Message)
            MsgBox("Send Click " & ex.Message)
        End Try

    End Sub




    Private Sub GVDetail_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles GVDetail.CellContentClick
        Try
            If e.ColumnIndex = GVDetail.Columns.Count - 1 Then

                If GVDetail(9, e.RowIndex).Value = Nothing Or GVDetail(9, e.RowIndex).Value = "0" Then
                    GVDetail(e.ColumnIndex, e.RowIndex).Value = New Bitmap(Application.StartupPath & "\TickMark.png")
                    GVDetail(9, e.RowIndex).Value = "1"
                    GridDataSet.Tables(0).Rows((startRec - 1) + e.RowIndex)("ImgChecked") = "1"
                Else
                    Dim inImg As Image = Image.FromFile(Application.StartupPath & "\blank.png")
                    inImg = New Bitmap(Convert.ToInt32(GVDetail.Width * 0.06), inImg.Height)
                    GVDetail(e.ColumnIndex, e.RowIndex).Value = inImg
                    GVDetail(9, e.RowIndex).Value = "0"
                    GridDataSet.Tables(0).Rows((startRec - 1) + e.RowIndex)("ImgChecked") = "0"
                End If
            End If
        Catch ex As Exception
            WriteError_File("Cell Content Click " & ex.Message)
            MsgBox(ex.Message, MsgBoxStyle.Information)
        End Try
    End Sub



    Private Sub cmdDone_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdDone.Click
        Dim fixl As New System.IO.DirectoryInfo(strCreatedXLDoc) ' & "#toolbar=0&navpanes=0"
        Dim filesxl = fixl.GetFiles.ToList
        Dim firstxl = (From file In filesxl Select file Where file.CreationTime.Date = Today.Date).FirstOrDefault

        If filesxl.Count > 0 Then
            If firstxl IsNot Nothing Then
                If IO.File.Exists(firstxl.FullName) = True Then
                    Process.Start(firstxl.FullName)
                End If

            End If
        Else

        End If
        Exit Sub

    End Sub


    Private Sub cmdRefresh_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdRefresh.Click
        InitializeWorkbook(strXLDoc, Path.GetExtension(strXLDoc))
        DSXL = ConvertToDataTable(workbook)
        LoadDataSet()
        FillGrid()
    End Sub

    Private Sub cmdRefresh_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdRefresh.MouseHover
        ToolTip1.SetToolTip(cmdRefresh, "REFRESH RECORDS")
    End Sub


    Private Sub PrintReport(ByVal strNoCopy As String, ByVal strPrinterName As String, ByVal strParameterArray() As String)
        Try


            Dim crDbConnection As New CrystalDecisions.Shared.ConnectionInfo()
            Dim i As Integer


            Dim oRpt As New CrystalDecisions.CrystalReports.Engine.ReportDocument()

            Dim printDoc As New Printing.PrintDocument

            Dim pkSize As New Printing.PaperSize()

            Dim rawKind As Integer = 0

            oRpt.PrintOptions.PaperSize = DirectCast(rawKind, CrystalDecisions.Shared.PaperSize)
            oRpt.FileName = Application.StartupPath & "\PrintLabel.rpt"
            oRpt.SetParameterValue("@CBNumber", strParameterArray(0))
            oRpt.SetParameterValue("@Width", strParameterArray(1))
            oRpt.SetParameterValue("@Drop", strParameterArray(2))
            If (Len(strParameterArray(3)) > 20) Then
                oRpt.SetParameterValue("@Customer", strParameterArray(3).ToString.Substring(0, 20))
            Else
                oRpt.SetParameterValue("@Customer", strParameterArray(3))
            End If

            If (Len(strParameterArray(4)) > 10) Then
                oRpt.SetParameterValue("@Department", strParameterArray(4).ToString.Substring(0, 10))
            Else
                oRpt.SetParameterValue("@Department", strParameterArray(4))
            End If

            oRpt.SetParameterValue("@Type", strParameterArray(5))

            If (Len(strParameterArray(6)) > 12) Then
                oRpt.SetParameterValue("@Fabric", strParameterArray(6).ToString.Substring(0, 12))
            Else
                oRpt.SetParameterValue("@Fabric", strParameterArray(6))
            End If

            If (Len(strParameterArray(7)) > 12) Then
                oRpt.SetParameterValue("@Color", strParameterArray(7).ToString.Substring(0, 12))
            Else
                oRpt.SetParameterValue("@Color", strParameterArray(7))
            End If

            If (Len(strParameterArray(8)) > 6) Then
                oRpt.SetParameterValue("@ControlType", strParameterArray(8).ToString.Substring(0, 6))
            Else
                oRpt.SetParameterValue("@ControlType", strParameterArray(8))
            End If

            oRpt.SetParameterValue("@Lathe", strParameterArray(9))
            oRpt.SetParameterValue("@Alpha", strParameterArray(10))
            oRpt.SetParameterValue("@Barcode", strParameterArray(11))
            oRpt.SetParameterValue("@LineNumber", strParameterArray(12))
            oRpt.SetParameterValue("@Total", strParameterArray(13))


            oRpt.PrintOptions.PrinterName = strPrinterName
            i = 0
            For i = 1 To CInt(strNoCopy)
                Try

                    oRpt.PrintToPrinter(1, False, 0, 0)
                Catch ex As Exception
                End Try
            Next i
        Catch ex As Exception
            WriteError_File("Printing Report " & ex.Message)
            MsgBox(ex.Message)
        End Try
    End Sub

    Public Shared Function DefaultPrinterName() As String
        Dim oPS As New System.Drawing.Printing.PrinterSettings

        Try
            DefaultPrinterName = oPS.PrinterName
        Catch ex As System.Exception
            DefaultPrinterName = ""
        Finally
            oPS = Nothing
        End Try
    End Function

    Protected Function GetCutwidth(ByVal width As String, ByVal value As String) As String
        Dim cutwidth As String = "0"
        If width = String.Empty And IsNumeric(width) = False Then
            GetCutwidth = width
            Exit Function
        Else
            If Val(width) = 0 Then
                GetCutwidth = width
                Exit Function
            End If
        End If

        'If value = "Easylink Control" And Convert.ToDouble(TranslateNumber(width)) > 24 Then
        '    cutwidth = (Convert.ToDouble(TranslateNumber(width)) - 24).ToString
        'ElseIf value = "Easylink Centre" And Convert.ToDouble(TranslateNumber(width)) > 19 Then
        '    cutwidth = (Convert.ToDouble(TranslateNumber(width)) - 19).ToString
        'ElseIf value = "Easylink Idler" And Convert.ToDouble(TranslateNumber(width)) > 23 Then
        '    cutwidth = (Convert.ToDouble(TranslateNumber(width)) - 23).ToString
        'ElseIf (value = "Motorised" Or value = "WT Motor" Or value = "RTS Motor" Or value = "VS Motor" Or value = "VSWT Motor") And Convert.ToDouble(TranslateNumber(width)) > 33 Then
        '    cutwidth = (Convert.ToDouble(TranslateNumber(width)) - 33).ToString
        'ElseIf (value = "Wire free" Or value = "Motor Control") And Convert.ToDouble(TranslateNumber(width)) > 29 Then
        '    cutwidth = (Convert.ToDouble(TranslateNumber(width)) - 29).ToString
        'ElseIf value = "Wire free Control" And Convert.ToDouble(TranslateNumber(width)) > 25 Then
        '    cutwidth = (Convert.ToDouble(TranslateNumber(width)) - 25).ToString
        'Else
        '    cutwidth = width - 29
        'End If

        Dim DR() As DataRow
        If value.Trim = String.Empty Then
            value = "BLANK"
        End If

        DR = Deductiontable.Select("OperationType='" + value.Trim + "'")

        If DR.Length > 0 Then
            cutwidth = (Convert.ToDouble(TranslateNumber(width)) - DR(0)("Deduction").ToString())
        Else
            cutwidth = width

        End If


        Return cutwidth
    End Function
    Private Function TranslateNumber(ByVal strValue As String) As String
        If Not IsNothing(strValue) Then
            If Len(strValue) > 0 Then
                TranslateNumber = CStr(CDbl(strValue))
            Else
                TranslateNumber = "0"
            End If
        Else
            TranslateNumber = "0"
        End If
    End Function

    Private Sub bgWorker_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles bgWorker.DoWork

        If bgWorker.CancellationPending = True Then
            e.Cancel = True
        Else
            bgWorker.ReportProgress(0)
            LoadDataSet()
            LoadPage()
            System.Threading.Thread.Sleep(1000)
            bgWorker.ReportProgress(100)
        End If
    End Sub

    Private Sub bgWorker_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles bgWorker.RunWorkerCompleted
        bgWorker.RunWorkerAsync()

    End Sub
    Private Sub WriteError_File(ByVal ErrorMsg As String)
        Dim strErrorFile As String = Application.StartupPath & "\Error.ini"
        Dim fileExists As Boolean = File.Exists(strErrorFile)
        Using sw As New StreamWriter(File.Open(strErrorFile, FileMode.Append))
            sw.WriteLine( _
                IIf(fileExists, _
                    "Error Message is  Occured at-- " & DateTime.Now & "--" & ErrorMsg, _
                   "Start Error Log for today" & Date.Today.Date & Environment.NewLine() & ReplaceStr(ErrorMsg)))
        End Using
    End Sub
    Public Function ReplaceStr(ByVal str As String) As String
        ReplaceStr = Replace(Trim(str), "'", "''''")
    End Function



    Private Sub btnRedoFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRedoFile.Click

        ImportData.ShowDialog()
        If ImportData.txtExcelData.Text = String.Empty Then
            Exit Sub
        End If
        Try
            Dim DR As DataRow = GridDataSet.Tables(0).NewRow()
            Dim DRXLArray() As DataRow
            Dim str() As String = ImportData.txtExcelData.Text.Split(vbTab)
            Dim strWidth As String = String.Empty
            Dim strCutWidth As String = String.Empty
            Dim strQty As String = String.Empty

            Dim j As Integer, f As Integer, a As Integer
            If str.Length = 10 Then
                strQty = str(2)
                strWidth = str(3).Replace("mm", "")
                strCutWidth = str(4).Replace("mm", "")
                If IsNumeric(strQty) = False Or strQty = String.Empty Then
                    WriteError_File("Invaid QTY !!")
                    MsgBox("Invaid QTY !!")
                    Exit Sub
                ElseIf IsNumeric(strWidth) = False Or strWidth = String.Empty Then
                    WriteError_File("Invaid Width !!")
                    MsgBox("Invaid Width !!")
                    Exit Sub
                ElseIf IsNumeric(strCutWidth) = False Or strCutWidth = String.Empty Then
                    WriteError_File("Invaid Cut Width !!")
                    MsgBox("Invaid Cut Width !!")
                    Exit Sub
                End If

                DRXLArray = DSXL.Tables(0).Select("[Line No.]='" + str(9).ToString.Trim + "'")
                If DRXLArray.Length = 0 Then
                    WriteError_File("Invaid Line No. !!")
                    MsgBox("Invaid Line No. !!")
                    Exit Sub
                End If

                a = 1
                f = 65
                j = 0
                DR("CBNumber") = str(0)
                DR("Item") = str(1)
                DR("Qty") = str(2)
                DR("Width") = str(3)
                DR("Cut Width") = str(4)
                DR("Tube") = str(5)
                DR("Spring") = str(6)
                DR("Finish") = str(7)
                DR("Colour") = str(8)
                DR("LineNo") = str(9)
                DR("ImgChecked") = 0

                Dim drarrayTemp() As DataRow = DSXL.Tables(0).Select("[W/Order NO]='" + DR("CBNumber") + "'", "[Line No.]" + " asc")

                If drarrayTemp.Length = 0 Then
                    WriteError_File("Invaid CBNumber. !!")
                    MsgBox("Invaid CBNumber. !!")
                    Exit Sub
                End If
                'WILL ADD CHAR ABCD FOR ITEM NUMBER
                For j = 0 To UBound(drarrayTemp)
                    If drarrayTemp(j)("Line No.").ToString.Trim = DRXLArray(0)("Line No.").ToString.Trim Then
                        DR("Item") = Chr(f).ToString
                        Exit For
                    End If
                    f = f + 1
                    a = a + 1
                Next



                DR("SRLineNumber") = a
                If IsDBNull(DRXLArray(0)("Drop")) = False Then
                    If DRXLArray(0)("Width").ToString().Trim <> String.Empty Then
                        DR("Drop") = DRXLArray(0)("Drop").Trim + "mm"
                    End If
                Else
                    DR("Drop") = String.Empty
                End If
                DR("Customer") = DRXLArray(0)("Customer Name 1").Trim
                DR("Type") = DRXLArray(0)("Track Col/Roll Type/Batten Col").Trim 'STD
                DR("ControlType") = DRXLArray(0)("Pull Colour/Bottom Weight/Wand Len").Trim '"ICE"
                DR("Lathe") = DR("Finish").Trim 'Fin 31
                DR("Alpha") = DR("item").Trim
                DR("Department") = DRXLArray
                DR("Fabric") = DRXLArray(0)("Department").Trim
                DR("Total") = drarrayTemp.Length

                GridDataSet.Tables(0).Rows.InsertAt(DR, 0)
                GridDataSet.AcceptChanges()

                'Filters the column data for DataGrid
                GridDataTabel = GridDataSet.Tables(0).DefaultView.ToTable(False, "CBNumber", "Item", "Qty", "Width", "Cut Width", "Tube", "Spring", "Finish", "Colour", "ImgChecked")
                maxRec = GridDataTabel.Rows.Count

                PageCount = RoundUp(Convert.ToDecimal(GridDataTabel.Rows.Count / pageSize).ToString())
            Else
                WriteError_File("Redo File Error Column Missing !!")
                MsgBox("Redo File Error Column Missing !!")
                Exit Sub
            End If
        Catch ex As Exception
            WriteError_File("Redo File " & ex.Message)
            MsgBox(ex.Message)
        End Try
    End Sub
End Class

