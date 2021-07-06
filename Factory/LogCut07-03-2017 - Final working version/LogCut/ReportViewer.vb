Imports System.Data.SqlClient
Imports System.Data.OleDb
Imports System.Data.Odbc
Imports System.Windows.Forms
Imports System.Windows.Forms.TabControl
Imports System.Data
Imports System.IO

Public Class ReportViewer
    Dim conn As OdbcConnection, con As OleDbConnection
    Dim Query As String, Query1 As String, myCommand As OdbcCommand
    Dim MyDataAdapter As OdbcDataAdapter, MyDataAdapter1 As OdbcDataAdapter
    Dim DS As DataSet, DS1 As DataSet
    Public strParameterArray() As String

    Private Sub ReportViewer_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try


            Dim crDbConnection As New CrystalDecisions.Shared.ConnectionInfo()

            reportDocument1.FileName = Application.StartupPath & "\PrintLabel.rpt"
            'reportDocument1.SetDataSource(DS.Tables(0))

            reportDocument1.SetParameterValue("@CBNumber", strParameterArray(0))
            reportDocument1.SetParameterValue("@Width", strParameterArray(1) + " mm")
            reportDocument1.SetParameterValue("@Drop", strParameterArray(2) + " mm")
            reportDocument1.SetParameterValue("@Customer", strParameterArray(3))
            reportDocument1.SetParameterValue("@Department", strParameterArray(4))
            reportDocument1.SetParameterValue("@Type", strParameterArray(5))
            reportDocument1.SetParameterValue("@Fabric", strParameterArray(6))
            reportDocument1.SetParameterValue("@Color", strParameterArray(7))
            reportDocument1.SetParameterValue("@ControlType", strParameterArray(8))
            reportDocument1.SetParameterValue("@Lathe", strParameterArray(9))
            reportDocument1.SetParameterValue("@Alpha", strParameterArray(10))
            reportDocument1.SetParameterValue("@Barcode", strParameterArray(11))
            reportDocument1.SetParameterValue("@LineNumber", strParameterArray(12))
            reportDocument1.SetParameterValue("@Total", strParameterArray(13))


            CrystalReportViewer1.ReportSource = reportDocument1
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Information)
        End Try
    End Sub
End Class