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
Imports System.Linq

Imports System.Data
Imports System.Collections.Generic
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Spreadsheet
Public Class JobSheet

    Dim GridDataSet As DataSet, Deductiontable As DataTable, Droptable As DataTable, FabricDatable As DataTable, GridPageDataSet As DataSet, GridDataTabel As DataTable
    Dim strDropfilePath, strFabricFilePath, strDedFilepath As String
    Dim strXMLDoc As String, strXLDoc As String, strCreatedXLDoc, StrPrinterName As String
    Private PageCount As Integer
    Private maxRec As Integer
    Private pageSize As Integer
    Private currentPage As Integer
    Private recNo As Integer
    Dim strFabrictable() As String
    Dim strFinishtable() As String
    Shared workbook As IWorkbook
    Shared workbook_Input As IWorkbook
    Dim startRec As Integer
    Shared Sendworkbook As IWorkbook
    Dim Port, BaudRate, Parity, DataBits, StopBits As String
    Dim boolComvalues As Boolean = False
    Private WithEvents moRS232 As Rs232
    Dim DSXL As New DataSet()
    Dim DSInputXL As New DataSet()
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

    Private Sub JobSheet_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try

            intX = Screen.PrimaryScreen.Bounds.Width
            intY = Screen.PrimaryScreen.Bounds.Height

            Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None
            Me.WindowState = FormWindowState.Maximized
            'Me.TopMost = True

            cmbPageSize.SelectedIndex = 6
            pnlGrid.Height = Me.Height - 100
            pnlGrid.Width = Me.Width - 50
            GVDetail.Height = pnlGrid.Height - 20
            GVDetail.Width = pnlGrid.Width - 20



            Me.DoubleBuffered = True
            ChangeControlStyles(GVDetail, ControlStyles.OptimizedDoubleBuffer, True)

            pnlDisplayTitle.Visible = True




            If Reading_ServerINI_Values() = True Then


                If IO.File.Exists(strXLDoc) = False Then
                    WriteError_File("PLEASE CONFIGURE EXCEL DUMP FILE PATH IN SERVER.INI")
                    MsgBox("PLEASE CONFIGURE EXCEL DUMP FILE PATH IN SERVER.INI")
                    Exit Sub
                End If

                If IO.File.Exists(strDedFilepath) = False Then
                    WriteError_File("PLEASE CONFIGURE EXCEL DEDUCTION FILE PATH IN SERVER.INI")
                    MsgBox("PLEASE CONFIGURE EXCEL DEDUCTION FILE PATH IN SERVER.INI")
                    Exit Sub
                End If

                If IO.File.Exists(strFabricFilePath) = False Then
                    WriteError_File("PLEASE CONFIGURE EXCEL FABRIC FILE PATH IN SERVER.INI")
                    MsgBox("PLEASE CONFIGURE EXCEL FABRIC FILE PATH IN SERVER.INI")
                    Exit Sub
                End If

                If IO.File.Exists(strDropfilePath) = False Then
                    WriteError_File("PLEASE CONFIGURE EXCEL DROP FILE PATH IN SERVER.INI")
                    MsgBox("PLEASE CONFIGURE EXCEL DROP FILE PATH IN SERVER.INI")
                    Exit Sub
                End If

                If IO.Directory.Exists(strCreatedXLDoc) = False Then
                    WriteError_File("PLEASE CONFIGURE EXCEL CREATION FILE PATH IN SERVER.INI")
                    MsgBox("PLEASE CONFIGURE EXCEL CREATION FILE PATH IN SERVER.INI")
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




        Catch ex As Exception
            WriteError_File("Form Load Method " & ex.Message.ToString)
        End Try


    End Sub



    Private Function Reading_ServerINI_Values() As Boolean
        Dim Read As Boolean = True
        Dim i As Integer = 0
        Dim j As Integer = 0
        strXMLDoc = String.Empty : strXLDoc = String.Empty : strCreatedXLDoc = String.Empty
        Try
            Dim oRead As System.IO.StreamReader
            oRead = IO.File.OpenText(Application.StartupPath & "\Server.ini")

            While oRead.Peek <> -1
                If i = 0 Then
                    strXLDoc = oRead.ReadLine()
                ElseIf i = 1 Then
                    strDedFilepath = oRead.ReadLine()
                ElseIf i = 2 Then
                    strFabricFilePath = oRead.ReadLine()
                ElseIf i = 3 Then
                    strDropfilePath = oRead.ReadLine()
                ElseIf i = 4 Then
                    strCreatedXLDoc = oRead.ReadLine()
                ElseIf i = 5 Then
                    StrPrinterName = oRead.ReadLine()
                End If
                i = i + 1
            End While
            oRead.Close()

            If strXLDoc = String.Empty Or strDedFilepath = String.Empty Or strFabricFilePath = String.Empty Or strDropfilePath = String.Empty Then
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
        GridDataSet.Tables("Barcode").Columns.Add("Fabric")
        GridDataSet.Tables("Barcode").Columns.Add("Colour")
        GridDataSet.Tables("Barcode").Columns.Add("Drop", GetType(String))
        GridDataSet.Tables("Barcode").Columns.Add("DropGroup")
        GridDataSet.Tables("Barcode").Columns.Add("Width", GetType(String))
        GridDataSet.Tables("Barcode").Columns.Add("CutWidth")
        GridDataSet.Tables("Barcode").Columns.Add("B_RColour")
        GridDataSet.Tables("Barcode").Columns.Add("ImgChecked")
        GridDataSet.Tables("Barcode").Columns.Add("LineNo")
        GridDataSet.Tables("Barcode").Columns.Add("Finish")
        GridDataSet.Tables("Barcode").Columns.Add("CutWidth_hidden", System.Type.GetType("System.Int32"))
        GridDataSet.Tables("Barcode").Columns.Add("Qty")
        GridDataSet.Tables("Barcode").Columns.Add("Tube")
        GridDataSet.Tables("Barcode").Columns.Add("Date-Time")
        GridDataSet.Tables("Barcode").Columns.Add("Spring")
        GridDataSet.Tables("Barcode").Columns.Add("Customer")
        GridDataSet.Tables("Barcode").Columns.Add("Type")
        GridDataSet.Tables("Barcode").Columns.Add("ControlType")
        GridDataSet.Tables("Barcode").Columns.Add("Lathe")
        GridDataSet.Tables("Barcode").Columns.Add("Alpha")
        GridDataSet.Tables("Barcode").Columns.Add("SRLineNumber")
        GridDataSet.Tables("Barcode").Columns.Add("Department")
        GridDataSet.Tables("Barcode").Columns.Add("Total")
        GridDataSet.Tables("Barcode").Columns.Add("DropColour")
        GridDataSet.Tables("Barcode").Columns.Add("Fixing_Type_Bracket_Type")
        GridDataSet.Tables("Barcode").Columns.Add("ControlSide")
        GVDetail.AllowUserToAddRows = False
    End Sub


    Private Sub LoadDataSet(ByVal InputType As String, ByVal FilterText As String)
        'Dim Conn As SqlConnection = New SqlConnection(strConnectionString)

        Try
            Dim DSCBN As New DataSet()

            Dim drarray() As DataRow
            Dim drTemparray() As DataRow
            Dim DR As DataRow
            Dim filterExp As String = String.Empty
            Dim Fbindex As Integer, f As Integer, a As Integer, TotalQty As Integer




            Dim drarrayTemp() As DataRow

            Dim drarrayTempitem() As DataRow
            Try


                If InputType = "CB" Then
                    ' drarray = DSXL.Tables(0).Select("[W/Order NO]='" + FilterText.TrimEnd() + "' AND [Department] <> ''", "[W/Order NO]" + " asc")
                    drarray = DSXL.Tables(0).Select("[W/Order NO]='" + FilterText.TrimEnd() + "' AND [Department] <> ''", "[W/Order NO]")
                Else
                    drarray = DSXL.Tables(0).Select("[Line No.]='" + FilterText + "'")
                    If drarray.Length > 0 Then
                        Dim woNumber As String = drarray(0).ItemArray(1).Trim.ToString()
                        drarray = DSXL.Tables(0).Select("[W/Order NO]='" + woNumber + "' AND [Department] <> '' AND [Width] <= 3000 AND [Drop] <= 2700", "[W/Order NO]" + " asc")


                    End If
                End If
                f = 0
                For i = 0 To (drarray.Length - 1)
                    Try
                        Fbindex = 0
                        DR = GridDataSet.Tables(0).NewRow
                        DR("CBNumber") = drarray(i)("W/Order NO").Trim


                        DR("Fabric") = drarray(i)("Fabric").Trim



                        If drarray(i)("Fabric").ToString.Trim <> String.Empty Then
                            For n As Integer = 0 To FabricDatable.Rows.Count - 1
                                If drarray(i)("Fabric").ToString.ToUpper.Trim.Contains(FabricDatable.Rows(n)("FabricName").ToUpper) Then
                                    Fbindex = 1
                                    TotalQty = 0
                                    'f = f + 1
                                    'a = a + 1
                                    Exit For
                                End If
                            Next
                            f = f + 1
                            a = a + 1
                        Else
                            Fbindex = 0
                            TotalQty = 0

                        End If


                        'WILL ADD CHAR ABCD FOR ITEM NUMBER
                        If InputType = "CB" And Fbindex > 0 Then
                            'f = f + 1
                            'a = a + 1
                            DR("Item") = GetExcelColumnName(f) ' Chr(f).ToString
                            '  DR("Total") = drarray.Length
                            For j = 0 To UBound(drarray)
                                Dim Qty As Integer = Integer.Parse(drarray(j)("Qty").ToString.Trim)
                                TotalQty += Qty
                            Next
                            DR("Total") = TotalQty

                        ElseIf InputType = "LN" And Fbindex > 0 Then
                            drarrayTempitem = DSXL.Tables(0).Select("[W/Order NO]='" + DR("CBNumber") + "' AND [Department] <> '' AND [Width] <= 3000 AND [Drop] <=2700", "[Line No.]" + " asc")
                            a = 1
                            f = 0

                            'WILL ADD CHAR ABCD FOR ITEM NUMBER
                            For j = 0 To UBound(drarrayTempitem)
                                If drarrayTempitem(j)("Line No.").ToString.Trim = drarray(i)("Line No.").ToString.Trim Then
                                    DR("Item") = GetExcelColumnName(f) ' Chr(f).ToString
                                    Exit For
                                End If
                                f = f + 1
                                a = a + 1
                            Next
                            DR("Total") = drarrayTempitem.Length
                        End If




                        If drarray(i)("Drop").ToString <> String.Empty And IsNumeric(drarray(i)("Drop").ToString) Then
                            drTemparray = Droptable.Select(drarray(i)("Drop").ToString + ">=From and " + drarray(i)("Drop").ToString + "<= To")
                            If drTemparray.Length > 0 Then
                                DR("DropGroup") = drTemparray(0)("DropGroup")
                                DR("DropColour") = drTemparray(0)("DropColour")

                            End If

                        End If
                        DR("Drop") = drarray(i)("Drop").ToString()
                        DR("Colour") = drarray(i)("Colour").Trim

                        If IsDBNull(drarray(i)("Width")) = False Then
                            If drarray(i)("Width").ToString() <> 0 Then 'String.Empty Then
                                DR("Width") = drarray(i)("Width").ToString() + "mm"
                            End If
                        Else
                            DR("Width") = 0 'String.Empty
                        End If
                        DR("CutWidth") = GetCutwidth(drarray(i)("Width").ToString, drarray(i)("Bind. Type/# Panels/Rope/Operation").ToString.Trim)
                        DR("CutWidth_hidden") = DR("CutWidth")
                        If DR("CutWidth") <> String.Empty Then
                            DR("CutWidth") = DR("CutWidth") + "mm"
                        End If
                        DR("B_RColour") = drarray(i)("Pull Colour/Bottom Weight/Wand Len").ToString.Trim
                        DR("ImgChecked") = 0





                        DR("Qty") = drarray(i)("Qty").Trim


                        DR("Date-Time") = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") ' drarray(i)("Date-Time Size").Trim
                        DR("Tube") = drarray(i)("Tube Size").Trim
                        'If drarray(i)("SWndr/Clip").ToString.Trim = String.Empty And drarray(i)("Bind. Type/# Panels/Rope/Operation").ToString.Trim = String.Empty Then
                        '    DR("Spring") = "YES"
                        'Else
                        '    DR("Spring") = "NO"
                        'End If

                        If drarray(i)("Bind. Type/# Panels/Rope/Operation").ToString.Trim = "SPRING" Or drarray(i)("Bind. Type/# Panels/Rope/Operation").ToString.Trim = "SPRINGHD" Then
                            DR("Spring") = "YES"
                        Else
                            DR("Spring") = "NO"
                        End If

                        If drarray(i)("Description").ToString.Trim.Length > 6 Then
                            DR("Finish") = drarray(i)("Description").ToString.Trim.Substring(drarray(i)("Description").ToString.Trim.Length - 6)
                        Else
                            DR("Finish") = DR("Finish")
                        End If
                        DR("Colour") = drarray(i)("Colour").Trim
                        DR("ImgChecked") = 0

                        'Data for Label
                        DR("SRLineNumber") = a.ToString
                        If IsDBNull(drarray(i)("Drop")) = False Then
                            If drarray(i)("Width").ToString() <> 0 Then 'String.Empty Then
                                DR("Drop") = drarray(i)("Drop").ToString() + "mm"
                            End If
                        Else
                            DR("Drop") = 0 'String.Empty
                        End If
                        DR("Customer") = drarray(i)("Customer Name 1").Trim
                        DR("Type") = drarray(i)("Track Col/Roll Type/Batten Col").Trim 'STD
                        DR("ControlType") = drarray(i)("Pull Colour/Bottom Weight/Wand Len").Trim '"ICE"
                        DR("Lathe") = DR("Finish").Trim 'Fin 31
                        DR("Alpha") = DR("item") 'A
                        DR("Department") = drarray(i)("Department").Trim
                        If drarray(i)("Cntrl Side").ToString.Trim <> String.Empty Then
                            DR("ControlSide") = drarray(i)("Cntrl Side").ToString.Trim.Substring(0, 1)
                        End If

                        'If drarray(i)("Fabric").ToString.Trim.Length > 6 Then
                        '    DR("Fabric") = drarray(i)("Fabric").ToString.Trim.Substring(drarray(i)("Fabric").ToString.Trim.Length - 6) ' vibe
                        'Else
                        '    DR("Fabric") = drarray(i)("Fabric").ToString.Trim
                        'End If

                        If (Convert.ToInt32(drarray(i)("Width")) <= 3000 And Convert.ToInt32(drarray(i)("Drop")) <= 2700) Then

                            DR("LineNo") = drarray(i)("Line No.").ToString.Trim
                            DR("Fixing_Type_Bracket_Type") = drarray(i)("Fixing Type / Bracket Type").ToString.Trim

                            If (Fbindex > 0) Then
                                If InputType = "CB" Then
                                    GridDataSet.Tables(0).Rows.Add(DR)
                                    If (Convert.ToInt32(drarray(i)("Qty")) > 1) Then
                                        For j = 1 To Convert.ToInt32(drarray(i)("Qty")) - 1
                                            Dim newRow As DataRow
                                            newRow = GridDataSet.Tables(0).NewRow
                                            newRow.ItemArray = DR.ItemArray
                                            a = a + 1
                                            newRow("SRLineNumber") = a.ToString
                                            GridDataSet.Tables(0).Rows.Add(newRow)
                                        Next
                                    End If
                                Else
                                    If drarray(i)("Line No.").ToString.Trim = txtLineNumber.Text Then
                                        GridDataSet.Tables(0).Rows.Add(DR)
                                        If (Convert.ToInt32(drarray(i)("Qty")) > 1) Then
                                            For j = 1 To Convert.ToInt32(drarray(i)("Qty")) - 1
                                                Dim newRow As DataRow
                                                newRow = GridDataSet.Tables(0).NewRow
                                                newRow.ItemArray = DR.ItemArray
                                                a = a + 1
                                                newRow("SRLineNumber") = a.ToString
                                                GridDataSet.Tables(0).Rows.Add(newRow)
                                            Next
                                        End If
                                    End If
                                End If

                            End If
                        End If

                    Catch ex As Exception
                        WriteError_File("Adding CBNumber To Dataset " & ex.Message.ToString)
                    End Try
                Next

                '    End If
                'End If
            Catch ex As Exception
                WriteError_File("CB Number File Reading " & ex.Message.ToString)
            End Try

            GridDataSet.Tables(0).DefaultView.Sort = "DropColour ASC, CutWidth_hidden DESC"
            Dim dt As DataTable = GridDataSet.Tables(0).DefaultView.ToTable()
            GridDataSet = Nothing
            GridDataSet = New DataSet
            GridDataSet.Tables.Add(dt)

            '            Datatable FI = new Datatable();
            'DataView viewFI = new DataView(FI);
            'viewFI.Sort = "ServiceDate, ServiceRoute";
            'DataTable OFI= viewFI.ToTable();
            ' GridDataSet.AcceptChanges()
            'Filters the column data for DataGrid

            'GridDataTabel = GridDataSet.Tables(0).DefaultView.ToTable(False, "CBNumber", "Item", "Fabric", "Colour", "Drop", "DropGroup", "Width", "CutWidth", "B_RColour", "LineNo", "ImgChecked", "CutWidth_hidden", "DropColour")
            '            GridDataTabel = GridDataSet.Tables(0).DefaultView.ToTable(False, "CBNumber", "Item", "Fabric", "Colour", "Drop", "Tube", "Date-Time", "Width", "CutWidth", "B_RColour", "LineNo", "ImgChecked", "CutWidth_hidden", "DropColour")
            GridDataTabel = GridDataSet.Tables(0).DefaultView.ToTable(False, "CBNumber", "Item", "Fabric", "Colour", "Drop", "Tube", "Date-Time", "Width", "CutWidth", "B_RColour", "LineNo", "ImgChecked", "CutWidth_hidden", "DropColour")

            maxRec = GridDataTabel.Rows.Count


            PageCount = RoundUp(Convert.ToDecimal(GridDataTabel.Rows.Count / pageSize).ToString())

        Catch ex As Exception
            WriteError_File("Loading Dataset " & ex.Message.ToString)
            MsgBox(ex.Message, MsgBoxStyle.Information)
        Finally
            'Conn.Close()
        End Try
    End Sub
    Private Function GetExcelColumnName(ByVal columnNumber As Integer) As String
        Dim dividend As Integer = columnNumber
        Dim columnName As String = String.Empty
        Dim modulo As Integer

        While dividend > 0
            modulo = (dividend - 1) Mod 26
            columnName = Convert.ToChar(65 + modulo).ToString() & columnName
            dividend = CInt((dividend - modulo) / 26)
        End While

        Return columnName
    End Function
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
    Private Sub ChangeControlStyles(ByVal ctrl As DataGridView, ByVal flag As ControlStyles, ByVal value As Boolean)
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

                GVDetail.Columns(1).Width = Convert.ToInt64(GVDetail.Width * 0.03)
                GVDetail.Columns(1).HeaderText = "Item"


                GVDetail.Columns(2).Width = Convert.ToInt64(GVDetail.Width * 0.1)
                GVDetail.Columns(2).HeaderText = "Fabric"
                GVDetail.Columns(2).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft

                GVDetail.Columns(3).Width = Convert.ToInt64(GVDetail.Width * 0.1)
                GVDetail.Columns(3).HeaderText = "Colour"

                GVDetail.Columns(4).Width = Convert.ToInt64(GVDetail.Width * 0.07)
                GVDetail.Columns(4).HeaderText = "Drop"
                GVDetail.Columns(4).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft

                GVDetail.Columns(5).Width = Convert.ToInt64(GVDetail.Width * 0.07)
                'GVDetail.Columns(5).HeaderText = "Drop_Group"
                GVDetail.Columns(5).HeaderText = "Tube"
                GVDetail.Columns(5).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter



                GVDetail.Columns(6).Width = Convert.ToInt64(GVDetail.Width * 0.07)
                'GVDetail.Columns(5).HeaderText = "Drop_Group"
                GVDetail.Columns(6).HeaderText = "Date and Time"
                GVDetail.Columns(6).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter

                GVDetail.Columns(7).Width = Convert.ToInt64(GVDetail.Width * 0.07)
                GVDetail.Columns(7).HeaderText = "Width"
                GVDetail.Columns(7).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight

                GVDetail.Columns(8).Width = Convert.ToInt64(GVDetail.Width * 0.1)
                GVDetail.Columns(8).HeaderText = "Cut Width"
                GVDetail.Columns(8).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight

                GVDetail.Columns(9).Width = Convert.ToInt64(GVDetail.Width * 0.1)
                GVDetail.Columns(9).HeaderText = "B/R Colour"

                GVDetail.Columns(10).Width = Convert.ToInt64(GVDetail.Width * 0.1)
                GVDetail.Columns(10).HeaderText = "Line No"

                'Dim ImgChecked As New DataGridViewColumn()
                'GVDetail.Columns.Add("ImgChecked", String.Empty)
                GVDetail.Columns(11).Width = 0
                GVDetail.Columns(11).HeaderText = "ImgChecked"
                GVDetail.Columns(11).Visible = False

                GVDetail.Columns(12).Width = 0
                GVDetail.Columns(12).HeaderText = "CutWidth_hidden"
                GVDetail.Columns(12).Visible = False

                GVDetail.Columns(13).Width = 0
                GVDetail.Columns(13).HeaderText = "DropColour"
                GVDetail.Columns(13).Visible = False

                Dim img As New DataGridViewImageColumn()

                Dim inImg As Image = Image.FromFile(Application.StartupPath & "\blank.png")
                inImg = New Bitmap(Convert.ToInt32(GVDetail.Width * 0.055), inImg.Height)
                img.Image = inImg
                GVDetail.Columns.Add(img)
                img.HeaderText = "Select"
                img.Name = "img"
                GVDetail.Columns(14).Width = Convert.ToInt64(GVDetail.Width * 0.055)
                ' GVDetail.Columns(10).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight



                recNo = 0
                If GridDataTabel.Rows.Count > 0 Then
                    'Copy the rows from the source table to fill the temporary table.
                    For i = startRec To endRec
                        dtTemp.ImportRow(GridDataTabel.Rows(i - 1))

                        If GridDataTabel.Rows(i - 1)(11).ToString = "0" Then
                            Dim inImg1 As Image = Image.FromFile(Application.StartupPath & "\blank.png")
                            inImg1 = New Bitmap(Convert.ToInt32(GVDetail.Width * 0.055), inImg1.Height)
                            GVDetail(14, recNo).Value = inImg1
                        Else
                            GVDetail(14, recNo).Value = New Bitmap(Application.StartupPath & "\TickMark.png")
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

        'Try
        Timer1.Enabled = False

        pnlWait.Location = New Point((Me.DisplayRectangle.Width - pnlWait.Width) / 2, (Me.DisplayRectangle.Height - pnlWait.Height) / 2)

        pnlWait.Visible = True


        GridDatasetInitialize()
        'InitializeWorkbook(strXLDoc, Path.GetExtension(strXLDoc))
        DSXL = ConvertToDataTable_XML() 'ConvertToDataTable(workbook)
        ReadingInputValues()

        pnlWait.Visible = False

    End Sub


    Private Sub ReadingInputValues()

        Dim WB_Ded As IWorkbook = Nothing, WB_Fabric As IWorkbook = Nothing, WB_Drop As IWorkbook = Nothing
        Dim sheet As ISheet
        Dim strext As String = String.Empty
        Dim DS As DataSet
        Droptable = New DataTable()
        Deductiontable = New DataTable()
        FabricDatable = New DataTable()

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

            Using file As New FileStream(strDropfilePath, FileMode.Open, FileAccess.Read)
                strext = Path.GetExtension(strDropfilePath)
                If strext.ToUpper() = (".XLSX") Then
                    WB_Drop = New XSSFWorkbook(file)
                End If
                If strext.ToUpper() = ".XLS" Then
                    WB_Drop = New HSSFWorkbook(file)
                End If
            End Using


            Using file As New FileStream(strFabricFilePath, FileMode.Open, FileAccess.Read)
                strext = Path.GetExtension(strFabricFilePath)
                If strext.ToUpper() = (".XLSX") Then
                    WB_Fabric = New XSSFWorkbook(file)
                End If
                If strext.ToUpper() = ".XLS" Then
                    WB_Fabric = New HSSFWorkbook(file)
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

            Try
                If WB_Fabric IsNot Nothing Then
                    sheet = WB_Fabric.GetSheetAt(0)
                    FabricDatable = GetDSFromSheet(sheet, 0, 10, 20, "FABRIC")
                End If
            Catch ex As Exception
                WB_Fabric.Clear()
                WB_Fabric = Nothing
                sheet = Nothing
            End Try

            Try
                If WB_Drop IsNot Nothing Then
                    sheet = WB_Drop.GetSheetAt(0)
                    Droptable = GetDSFromSheet(sheet, 0, 10, 20, "DROP")
                End If
            Catch ex As Exception
                WB_Drop.Clear()
                WB_Drop = Nothing
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
        For i As Integer = (Sheet.FirstRowNum) To (Sheet.LastRowNum) 'intRowCount
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
                    WriteError_File(inputname + ex.Message.ToString)
                End Try

            End If
        Next
        Return DT
    End Function

    Private Shared Sub InitializeWorkbook(ByVal path As String, ByVal ext As String)

        Using file As New FileStream(path, FileMode.Open, FileAccess.Read)
            If ext.ToUpper() = (".XLSX") Then
                workbook = New XSSFWorkbook(file)
            End If
            If ext.ToUpper() = ".XLS" Then
                workbook = New HSSFWorkbook(file)
            End If
        End Using
    End Sub
    Private Shared Sub InitializeWorkbook_Input(ByVal path As String, ByVal ext As String)

        Using file As New FileStream(path, FileMode.Open, FileAccess.Read)
            If ext.ToUpper() = (".XLSX") Then
                workbook_Input = New XSSFWorkbook(file)
            End If
            If ext.ToUpper() = ".XLS" Then
                workbook_Input = New HSSFWorkbook(file)
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
                        If row.GetCell(j) IsNot Nothing Then
                            dataRow(j) = row.GetCell(j).ToString()
                        Else
                            Try
                                dataRow(j) = row.Cells(j).ToString()
                            Catch ex As Exception

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
    Protected Function ConvertToDataTable_XML() As DataSet
        'Save the uploaded Excel file.
        Dim filePath As String = strXLDoc
        Dim DS As New DataSet()
        Dim dt As New DataTable()
        Dim Value As String
        Value = String.Empty
        Dim blExist = True
        Dim M As Integer = 0
        Try
            'Open the Excel file in Read Mode using OpenXml.
            Using doc As SpreadsheetDocument = SpreadsheetDocument.Open(filePath, False)
                'Read the first Sheet from Excel file.
                Dim sheet As Sheet = doc.WorkbookPart.Workbook.Sheets.GetFirstChild(Of Sheet)()

                'Get the Worksheet instance.
                Dim worksheet As Worksheet = TryCast(doc.WorkbookPart.GetPartById(sheet.Id.Value), WorksheetPart).Worksheet

                'Fetch all the rows present in the Worksheet.
                Dim rows As IEnumerable(Of Row) = worksheet.GetFirstChild(Of SheetData)().Descendants(Of Row)()
                Dim Headerrow As Row = (worksheet.GetFirstChild(Of SheetData)().Descendants(Of Row)())(0)
                'Create a new DataTable.

                'Loop through the Worksheet rows.
                For Each row As Row In rows

                    'Use the first row to add columns to DataTable.
                    If row.RowIndex.Value = 1 Then

                        For index As Integer = 0 To (row.Descendants(Of Cell)()).Count Step 1
                            If (row.Descendants(Of Cell)())(index) Is Nothing Then
                                dt.Columns.Add("Col" & index.ToString())
                            Else
                                dt.Columns.Add(GetValue(doc, (row.Descendants(Of Cell)())(index)))
                            End If
                        Next
                        ' dt.Columns("Width").DataType = GetType(Int32)
                        'dt.Columns("Drop").DataType = GetType(Int32)
                    Else
                        dt.Columns("Width").DataType = GetType(Int32)
                        dt.Columns("Drop").DataType = GetType(Int32)

                        'Add rows to DataTable.
                        Dim DR As DataRow = dt.NewRow()
                        'dt.Rows.Add()
                        Dim a As String = row.Descendants(Of Cell).ToString()

                        For K As Integer = 0 To (Headerrow.Descendants(Of Cell)()).Count Step 1
                            M = row.RowIndex.Value
                            If (row.Descendants(Of Cell)())(K) IsNot Nothing Then
                                Value = GetValue(doc, (row.Descendants(Of Cell)())(K))
                                If Value = String.Empty And (K = 1) Then
                                    blExist = False
                                    Exit For
                                End If
                                If Value = "" And (K = 18 Or K = 19) Then
                                    Value = 0
                                End If
                                DR(K) = Value
                            Else
                                DR(K) = String.Empty
                            End If


                        Next

                        If blExist = True Then
                            dt.Rows.Add(DR)
                        End If
                    End If
                Next
               
                DS.Tables.Add(dt)
            End Using
        Catch ex As Exception
            MsgBox("Load Page Method -" & M & ex.Message.ToString & " " & Value.ToString(), MsgBoxStyle.Information)
            WriteError_File("Load Page Method -" & M & ex.Message.ToString & " " & Value.ToString())
        End Try

        Return DS
    End Function

    Private Function GetValue(ByVal doc As SpreadsheetDocument, ByVal cell As Cell) As String
        Dim value As String = cell.CellValue.InnerText
        If cell.DataType IsNot Nothing AndAlso cell.DataType.Value = CellValues.SharedString Then
            Return doc.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements.GetItem(Integer.Parse(value)).InnerText.Trim()
        End If
        Return value
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
        If MsgBox("ARE YOU SURE YOU WANT TO EXIT APPLICATION", MsgBoxStyle.YesNo, "SOP APPLICATION") = MsgBoxResult.No Then Exit Sub
        If bgWorker.IsBusy Then
            bgWorker.CancelAsync()
            System.Threading.Thread.Sleep(500)
            bgWorker.Dispose()
        End If
        Application.Exit()
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
        'MsgBox("Under Developmet")



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
                Dim DT As DataTable = GridDataSet.Tables(0).DefaultView.ToTable(False, "CBNumber", "Item", "Qty", "Width", "CutWidth", "Tube", "Date-Time", "Spring", "Finish", "Colour", "LineNo").Clone

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
                DR("CutWidth") = drarray(i)("CutWidth")
                DR("Tube") = drarray(i)("Tube").Trim
                DR("Date-Time") = drarray(i)("Date-Time") 'String.Empty
                DR("Spring") = drarray(i)("Spring")
                DR("Finish") = drarray(i)("Finish")
                DR("Colour") = drarray(i)("Colour")
                DR("LineNo") = drarray(i)("LineNo")
                strconcat = drarray(i)("CBNumber") & "@" & drarray(i)("Width")
                strconcat += "@" & drarray(i)("Drop") & "@" & drarray(i)("Customer") & "@" & drarray(i)("Department")
                strconcat += "@" & drarray(i)("Type") & "@" & drarray(i)("Fabric") & "@" & drarray(i)("Colour")
                strconcat += "@" & drarray(i)("ControlType") & "@" & drarray(i)("Lathe")
                strconcat += "@" & drarray(i)("Alpha") & "@" & drarray(i)("CBNumber") & "@" & drarray(i)("SRLineNumber")
                strconcat += "@" & drarray(i)("Total")
                strconcat += "@" & drarray(i)("CutWidth").Replace("mm", "")
                strconcat += "@" & drarray(i)("LineNo")
                strconcat += "@" & drarray(i)("ControlSide")
                DRLabelArray(i) = strconcat
                strRS232Width += drarray(i)("CutWidth").ToString().Replace("mm", "")
                DS.Tables(0).Rows.Add(DR)
                ''GridDataSet.Tables(0).Rows.Remove(drarray(i))
            Next


            Try
                moRS232.Open()
                '// Set state of RTS / DTS
                moRS232.Dtr = False '(chkDTR.CheckState = CheckState.Checked)
                moRS232.Rts = False
                moRS232.PurgeBuffer(Rs232.PurgeBuffers.TxClear Or Rs232.PurgeBuffers.RXClear)
                moRS232.Write(strRS232Width)
                moRS232.Close()

                For i As Integer = 0 To drarray.Count - 1
                    GridDataSet.Tables(0).Rows.Remove(drarray(i))
                Next
            Catch ex As Exception
                DS.Tables(0).Rows.Clear()
                WriteError_File("Writing to RS232 " & ex.Message)
                MsgBox(ex.Message, MsgBoxStyle.Information)

                Exit Sub
            End Try

            ' GridDataTabel = GridDataSet.Tables(0).DefaultView.ToTable(False, "CBNumber", "Item", "Fabric", "Colour", "Drop", "DropGroup", "Width", "CutWidth", "B_RColour", "LineNo", "ImgChecked", "CutWidth_hidden", "DropColour")
            GridDataTabel = GridDataSet.Tables(0).DefaultView.ToTable(False, "CBNumber", "Item", "Fabric", "Colour", "Drop", "Tube", "Date-Time", "Width", "CutWidth", "B_RColour", "LineNo", "ImgChecked", "CutWidth_hidden", "DropColour")



            Dim style1 As ICellStyle = cellStyle1()

            Dim style2 As ICellStyle = cellStyle2()
            Dim style3 As ICellStyle = cellStyle3()

            Try
                Dim sheet1 As XSSFSheet
                If Sendworkbook.GetSheet("Logcut") Is Nothing Then
                    sheet1 = Sendworkbook.CreateSheet("Logcut")
                Else
                    sheet1 = Sendworkbook.GetSheet("Logcut")
                End If

                Dim headerRow0 As IRow = sheet1.CreateRow(0)
                Dim headerColRow1 As IRow = sheet1.CreateRow(1)

                For i As Integer = 0 To DS.Tables(0).Columns.Count - 1
                    headerRow0.CreateCell(i)
                    If i = 0 Then
                        headerRow0.GetCell(0).SetCellValue("Logcut")
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


                Dim FileName As String = strCreatedXLDoc & "\Logcut" & DateTime.Now.ToString("yyyyMMdd") & "_" & DateTime.Now.ToString("HHmmss") & ".xlsx"
                
                Dim sw As FileStream = File.Create(FileName)
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
                    '  StrPrinterName = "Microsoft XPS Document Writer"
                    PrintReport(1, StrPrinterName, strParameterArray, "\PrintLabel.rpt", "Width")
                    PrintReport(1, StrPrinterName, strParameterArray, "\PrintLabelCutWidth.rpt", "CutWidth")

                End If
            Next

            currentPage = 1
        Catch ex As Exception
            WriteError_File("Send Click " & ex.Message)
            MsgBox("Send Click " & ex.Message)
        End Try
        FillGrid()
    End Sub




    Private Sub GVDetail_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles GVDetail.CellContentClick
        Try
            If e.ColumnIndex = GVDetail.Columns.Count - 1 Then

                If GVDetail(11, e.RowIndex).Value = Nothing Or GVDetail(11, e.RowIndex).Value = "0" Then
                    GVDetail(e.ColumnIndex, e.RowIndex).Value = New Bitmap(Application.StartupPath & "\TickMark.png")
                    GVDetail(11, e.RowIndex).Value = "1"
                    GridDataSet.Tables(0).Rows((startRec - 1) + e.RowIndex)("ImgChecked") = "1"
                Else
                    Dim inImg As Image = Image.FromFile(Application.StartupPath & "\blank.png")
                    inImg = New Bitmap(Convert.ToInt32(GVDetail.Width * 0.06), inImg.Height)
                    GVDetail(e.ColumnIndex, e.RowIndex).Value = inImg
                    GVDetail(11, e.RowIndex).Value = "0"
                    GridDataSet.Tables(0).Rows((startRec - 1) + e.RowIndex)("ImgChecked") = "0"
                End If
            End If
        Catch ex As Exception
            WriteError_File("Cell Content Click " & ex.Message)
            MsgBox(ex.Message, MsgBoxStyle.Information)
        End Try
    End Sub



    Private Sub cmdDone_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
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
        GridDataSet.Clear()
        GridDataTabel.Clear()
        If txtLineNumber.Text.Trim <> String.Empty Then
            LoadDataSet("LN", txtLineNumber.Text.Trim)
        ElseIf txtCBNumber.Text.Trim <> String.Empty Then
            LoadDataSet("CB", txtCBNumber.Text)
        End If
        FillGrid()
    End Sub

    Private Sub cmdRefresh_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdRefresh.MouseHover
        ToolTip1.SetToolTip(cmdRefresh, "REFRESH RECORDS")
    End Sub


    Private Sub PrintReport(ByVal strNoCopy As String, ByVal strPrinterName As String, ByVal strParameterArray() As String, ByVal StrReportPath As String, ByVal StrType As String)
        Try


            Dim crDbConnection As New CrystalDecisions.Shared.ConnectionInfo()
            Dim i As Integer


            Dim oRpt As New CrystalDecisions.CrystalReports.Engine.ReportDocument()

            Dim printDoc As New Printing.PrintDocument

            Dim pkSize As New Printing.PaperSize()

            Dim rawKind As Integer = 0

            oRpt.PrintOptions.PaperSize = DirectCast(rawKind, CrystalDecisions.Shared.PaperSize)
            oRpt.FileName = Application.StartupPath & StrReportPath
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
            oRpt.SetParameterValue("@strLineNumber", strParameterArray(12))

            oRpt.SetParameterValue("@Total", strParameterArray(13))
            If StrType.ToUpper = "CUTWIDTH" Then
                oRpt.SetParameterValue("@CutWidth", strParameterArray(14))
                oRpt.SetParameterValue("@ControlSide", strParameterArray(16))
            End If
            oRpt.SetParameterValue("@LineNo", strParameterArray(15))



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
        If width = String.Empty Or IsNumeric(width) = False Then
            GetCutwidth = width
            Exit Function
        Else
            If Val(width) = 0 Then
                GetCutwidth = width
                Exit Function
            End If
        End If


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
            'LoadDataSet()
            'LoadPage()
            '  System.Threading.Thread.Sleep(1000) poorni -- Temporary basis
            '  bgWorker.ReportProgress(100)
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



    Private Sub btnRedoFile_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

        ImportData.ShowDialog()
        If ImportData.txtExcelData.Text = String.Empty Then
            Exit Sub
        End If
        Try
            Dim DR As DataRow = GridDataSet.Tables(0).NewRow()
            Dim DRXLArray() As DataRow
            Dim str() As String = ImportData.txtExcelData.Text.Split("	")
            Dim strWidth As String = String.Empty
            Dim strCutWidth As String = String.Empty
            Dim strQty As String = String.Empty

            Dim j As Integer, f As Integer, int_a As Integer
            If str.Length = 10 Then
                strQty = str(2)
                strWidth = str(3).Replace("mm", "")
                strCutWidth = str(4).Replace("mm", "")
                'If IsNumeric(strQty) = False Or strQty = String.Empty Then
                '    WriteError_File("Invaid QTY !!")
                '    MsgBox("Invaid QTY !!")
                '    Exit Sub
                If IsNumeric(strWidth) = False Or strWidth = String.Empty Then
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

                int_a = 1
                f = 65
                j = 0
                DR("CBNumber") = str(0)
                DR("Item") = str(1)
                DR("Qty") = str(2)
                DR("Width") = str(3)
                DR("CutWidth") = str(4)
                'DR("Tube") = str(5)
                DR("Date-Time") = str(5)
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
                    int_a = int_a + 1
                Next



                DR("SRLineNumber") = int_a
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

                GridDataSet.Tables(0).Rows.Add(DR)
                GridDataSet.AcceptChanges()

                'Filters the column data for DataGrid
                GridDataTabel = GridDataSet.Tables(0).DefaultView.ToTable(False, "CBNumber", "Item", "Qty", "Width", "CutWidth", "Date-Time", "Spring", "Finish", "Colour", "ImgChecked")
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

    'Private Sub txtCBNumber_Enter(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtCBNumber.Enter
    '    If txtCBNumber.Text <> Nothing Then
    '        txtLineNumber.Text = String.Empty
    '        GridDataSet.Clear()
    '        LoadDataSet("CB", txtCBNumber.Text)
    '        FillGrid()
    '    End If
    'End Sub
    'Private Sub txtLineNumber_Enter(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtLineNumber.Enter
    '    If txtLineNumber.Text <> Nothing Then
    '        txtCBNumber.Text = String.Empty
    '        GridDataSet.Clear()
    '        LoadDataSet("LN", txtLineNumber.Text)
    '        FillGrid()
    '    End If
    'End Sub

    Private Sub txtCBNumber_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtCBNumber.KeyPress

        If e.KeyChar = ChrW(Keys.Enter) Then
            If txtCBNumber.Text <> Nothing Then
                If GridDataSet IsNot Nothing Then
                    If GridDataSet.Tables(0).Rows.Count > 0 Then
                        Dim result As DialogResult
                        result = MessageBox.Show("YES, CLEAR THE DATA IN GRID!|NO,APPEND IT", "APPEND OR ADD NEW DATA", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                        If result.ToString.ToUpper = "YES" Then
                            GridDataSet.Clear()
                            GridDataTabel.Clear()
                        End If
                        ' Dim result As Integer = MessageBox.Show("message", "caption", MessageBoxButtons.YesNoCancel)
                    End If
                End If


                txtLineNumber.Text = String.Empty
                LoadDataSet("CB", txtCBNumber.Text)
                FillGrid()
                txtCBNumber.Text = String.Empty
            End If

        End If
    End Sub






    Private Sub btnSort_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSort.Click
        Dim Dview As New DataView(GridDataTabel)
        Dview.Sort = "Fabric ASC, Colour ASC, DropGroup ASC,CutWidth_hidden Desc"
        GridDataTabel = Dview.ToTable
        FillGrid()
    End Sub


    Private Sub btnDeleteRow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDeleteRow.Click
        Dim drarray() As DataRow
        drarray = GridDataSet.Tables(0).Select("ImgChecked='1'")
        If (drarray.Length <= 0) Then
            WriteError_File("Select Records to Delete !!")
            MsgBox("Select Records to Delete !!")
            Exit Sub
        Else

            For i As Integer = 0 To drarray.Length - 1
                GridDataSet.Tables(0).Rows.Remove(drarray(i))

            Next
            'GridDataTabel = GridDataSet.Tables(0).DefaultView.ToTable(False, "CBNumber", "Item", "Fabric", "Colour", "Drop", "DropGroup", "Width", "CutWidth", "B_RColour", "LineNo", "ImgChecked", "CutWidth_hidden", "DropColour")
            GridDataTabel = GridDataSet.Tables(0).DefaultView.ToTable(False, "CBNumber", "Item", "Fabric", "Colour", "Drop", "Tube", "Date-Time", "Width", "CutWidth", "B_RColour", "LineNo", "ImgChecked", "CutWidth_hidden", "DropColour")
            FillGrid()
            MsgBox("Records Deleted Successfully")
        End If

    End Sub

    Private Sub txtLineNumber_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtLineNumber.KeyPress
        If e.KeyChar = ChrW(Keys.Enter) Then
            If txtLineNumber.Text <> Nothing Then
                txtCBNumber.Text = String.Empty
                LoadDataSet("LN", txtLineNumber.Text)
                FillGrid()
                txtLineNumber.Text = String.Empty
            End If
        End If
    End Sub



    Private Sub GVDetail_CellFormatting(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellFormattingEventArgs) Handles GVDetail.CellFormatting
        Try
            If GVDetail(13, e.RowIndex).Value <> Nothing And GVDetail(13, e.RowIndex).Value <> "" Then
                'If e.ColumnIndex = 4 Then
                e.CellStyle.BackColor = System.Drawing.Color.FromName(GVDetail(13, e.RowIndex).Value)
                If GVDetail(13, e.RowIndex).Value.ToString.ToUpper = "BLACK" Or GVDetail(13, e.RowIndex).Value.ToString.ToUpper = "BLUE" Then
                    e.CellStyle.ForeColor = System.Drawing.Color.FromName("White")
                End If

                'End If
            End If
            If GridDataSet.Tables(0).Rows(e.RowIndex)("Fixing_Type_Bracket_Type").ToString().ToUpper() = "SPRING" Then
                e.CellStyle.BackColor = System.Drawing.Color.Green
                e.CellStyle.ForeColor = System.Drawing.Color.White
            End If
            'If e.RowIndex >= 0 Then
            '    e.CellStyle.BackColor = Color.FromName(GridDataTabel.Rows(e.RowIndex)("DropColour"))
            'End If
        Catch ex As Exception
            WriteError_File("Colour Formatting " & ex.Message)
        End Try

    End Sub
End Class

