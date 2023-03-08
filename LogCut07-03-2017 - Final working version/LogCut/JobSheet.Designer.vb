<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class JobSheet
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(JobSheet))
        Dim DataGridViewCellStyle4 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle
        Dim DataGridViewCellStyle5 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle
        Dim DataGridViewCellStyle6 As System.Windows.Forms.DataGridViewCellStyle = New System.Windows.Forms.DataGridViewCellStyle
        Me.GroupBox1 = New System.Windows.Forms.GroupBox
        Me.pnlDisplayTitle = New System.Windows.Forms.Panel
        Me.lbl0 = New System.Windows.Forms.Label
        Me.pnlDocument = New System.Windows.Forms.Panel
        Me.btnDeleteRow = New System.Windows.Forms.Button
        Me.btnSort = New System.Windows.Forms.Button
        Me.lblLineNo = New System.Windows.Forms.Label
        Me.lblCbnumber = New System.Windows.Forms.Label
        Me.txtLineNumber = New System.Windows.Forms.TextBox
        Me.txtCBNumber = New System.Windows.Forms.TextBox
        Me.cmdRefresh = New System.Windows.Forms.Button
        Me.cmdClose = New System.Windows.Forms.Button
        Me.cmdSend = New System.Windows.Forms.Button
        Me.cmdNext = New System.Windows.Forms.Button
        Me.cmdPrevious = New System.Windows.Forms.Button
        Me.cmbPageSize = New System.Windows.Forms.ComboBox
        Me.GVDetail = New System.Windows.Forms.DataGridView
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.pnlGrid = New System.Windows.Forms.Panel
        Me.bgWorker = New System.ComponentModel.BackgroundWorker
        Me.pnlWait = New System.Windows.Forms.Panel
        Me.GroupBox1.SuspendLayout()
        Me.pnlDisplayTitle.SuspendLayout()
        Me.pnlDocument.SuspendLayout()
        CType(Me.GVDetail, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlGrid.SuspendLayout()
        Me.SuspendLayout()
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.pnlDisplayTitle)
        Me.GroupBox1.Controls.Add(Me.pnlDocument)
        Me.GroupBox1.Dock = System.Windows.Forms.DockStyle.Top
        Me.GroupBox1.Location = New System.Drawing.Point(0, 0)
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.GroupBox1.Size = New System.Drawing.Size(1368, 98)
        Me.GroupBox1.TabIndex = 4
        Me.GroupBox1.TabStop = False
        '
        'pnlDisplayTitle
        '
        Me.pnlDisplayTitle.Controls.Add(Me.lbl0)
        Me.pnlDisplayTitle.Dock = System.Windows.Forms.DockStyle.Left
        Me.pnlDisplayTitle.Location = New System.Drawing.Point(4, 19)
        Me.pnlDisplayTitle.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.pnlDisplayTitle.Name = "pnlDisplayTitle"
        Me.pnlDisplayTitle.Size = New System.Drawing.Size(123, 75)
        Me.pnlDisplayTitle.TabIndex = 82
        Me.pnlDisplayTitle.Visible = False
        '
        'lbl0
        '
        Me.lbl0.AutoSize = True
        Me.lbl0.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl0.Location = New System.Drawing.Point(4, 6)
        Me.lbl0.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lbl0.Name = "lbl0"
        Me.lbl0.Size = New System.Drawing.Size(116, 58)
        Me.lbl0.TabIndex = 2
        Me.lbl0.Text = "Log Cut " & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "2.0"
        Me.lbl0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'pnlDocument
        '
        Me.pnlDocument.Controls.Add(Me.btnDeleteRow)
        Me.pnlDocument.Controls.Add(Me.btnSort)
        Me.pnlDocument.Controls.Add(Me.lblLineNo)
        Me.pnlDocument.Controls.Add(Me.lblCbnumber)
        Me.pnlDocument.Controls.Add(Me.txtLineNumber)
        Me.pnlDocument.Controls.Add(Me.txtCBNumber)
        Me.pnlDocument.Controls.Add(Me.cmdRefresh)
        Me.pnlDocument.Controls.Add(Me.cmdClose)
        Me.pnlDocument.Controls.Add(Me.cmdSend)
        Me.pnlDocument.Controls.Add(Me.cmdNext)
        Me.pnlDocument.Controls.Add(Me.cmdPrevious)
        Me.pnlDocument.Controls.Add(Me.cmbPageSize)
        Me.pnlDocument.Dock = System.Windows.Forms.DockStyle.Right
        Me.pnlDocument.Location = New System.Drawing.Point(121, 19)
        Me.pnlDocument.Margin = New System.Windows.Forms.Padding(0)
        Me.pnlDocument.Name = "pnlDocument"
        Me.pnlDocument.Size = New System.Drawing.Size(1243, 75)
        Me.pnlDocument.TabIndex = 81
        '
        'btnDeleteRow
        '
        Me.btnDeleteRow.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.btnDeleteRow.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnDeleteRow.FlatAppearance.BorderSize = 0
        Me.btnDeleteRow.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnDeleteRow.Image = CType(resources.GetObject("btnDeleteRow.Image"), System.Drawing.Image)
        Me.btnDeleteRow.Location = New System.Drawing.Point(799, 6)
        Me.btnDeleteRow.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.btnDeleteRow.Name = "btnDeleteRow"
        Me.btnDeleteRow.Size = New System.Drawing.Size(184, 60)
        Me.btnDeleteRow.TabIndex = 95
        Me.btnDeleteRow.UseVisualStyleBackColor = True
        '
        'btnSort
        '
        Me.btnSort.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.btnSort.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnSort.FlatAppearance.BorderSize = 0
        Me.btnSort.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnSort.Image = CType(resources.GetObject("btnSort.Image"), System.Drawing.Image)
        Me.btnSort.Location = New System.Drawing.Point(712, 6)
        Me.btnSort.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.btnSort.Name = "btnSort"
        Me.btnSort.Size = New System.Drawing.Size(85, 60)
        Me.btnSort.TabIndex = 94
        Me.btnSort.UseVisualStyleBackColor = True
        '
        'lblLineNo
        '
        Me.lblLineNo.AutoSize = True
        Me.lblLineNo.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLineNo.Location = New System.Drawing.Point(413, 4)
        Me.lblLineNo.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblLineNo.Name = "lblLineNo"
        Me.lblLineNo.Size = New System.Drawing.Size(95, 20)
        Me.lblLineNo.TabIndex = 91
        Me.lblLineNo.Text = "LineNumber"
        '
        'lblCbnumber
        '
        Me.lblCbnumber.AutoSize = True
        Me.lblCbnumber.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCbnumber.Location = New System.Drawing.Point(245, 5)
        Me.lblCbnumber.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lblCbnumber.Name = "lblCbnumber"
        Me.lblCbnumber.Size = New System.Drawing.Size(87, 20)
        Me.lblCbnumber.TabIndex = 90
        Me.lblCbnumber.Text = "CBNumber"
        '
        'txtLineNumber
        '
        Me.txtLineNumber.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtLineNumber.Location = New System.Drawing.Point(407, 30)
        Me.txtLineNumber.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txtLineNumber.Name = "txtLineNumber"
        Me.txtLineNumber.Size = New System.Drawing.Size(159, 36)
        Me.txtLineNumber.TabIndex = 1
        '
        'txtCBNumber
        '
        Me.txtCBNumber.Font = New System.Drawing.Font("Microsoft Sans Serif", 15.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtCBNumber.Location = New System.Drawing.Point(240, 30)
        Me.txtCBNumber.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.txtCBNumber.Name = "txtCBNumber"
        Me.txtCBNumber.Size = New System.Drawing.Size(155, 36)
        Me.txtCBNumber.TabIndex = 0
        '
        'cmdRefresh
        '
        Me.cmdRefresh.Cursor = System.Windows.Forms.Cursors.Hand
        Me.cmdRefresh.FlatAppearance.BorderSize = 0
        Me.cmdRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdRefresh.Image = CType(resources.GetObject("cmdRefresh.Image"), System.Drawing.Image)
        Me.cmdRefresh.Location = New System.Drawing.Point(991, 5)
        Me.cmdRefresh.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.cmdRefresh.Name = "cmdRefresh"
        Me.cmdRefresh.Size = New System.Drawing.Size(133, 62)
        Me.cmdRefresh.TabIndex = 86
        Me.cmdRefresh.UseVisualStyleBackColor = True
        '
        'cmdClose
        '
        Me.cmdClose.Cursor = System.Windows.Forms.Cursors.Hand
        Me.cmdClose.FlatAppearance.BorderSize = 0
        Me.cmdClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdClose.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdClose.Image = CType(resources.GetObject("cmdClose.Image"), System.Drawing.Image)
        Me.cmdClose.Location = New System.Drawing.Point(1125, 9)
        Me.cmdClose.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.cmdClose.Name = "cmdClose"
        Me.cmdClose.Size = New System.Drawing.Size(115, 54)
        Me.cmdClose.TabIndex = 83
        Me.cmdClose.UseVisualStyleBackColor = True
        '
        'cmdSend
        '
        Me.cmdSend.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None
        Me.cmdSend.Cursor = System.Windows.Forms.Cursors.Hand
        Me.cmdSend.FlatAppearance.BorderSize = 0
        Me.cmdSend.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdSend.Image = CType(resources.GetObject("cmdSend.Image"), System.Drawing.Image)
        Me.cmdSend.Location = New System.Drawing.Point(604, 6)
        Me.cmdSend.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.cmdSend.Name = "cmdSend"
        Me.cmdSend.Size = New System.Drawing.Size(107, 60)
        Me.cmdSend.TabIndex = 82
        Me.cmdSend.UseVisualStyleBackColor = True
        '
        'cmdNext
        '
        Me.cmdNext.Cursor = System.Windows.Forms.Cursors.Hand
        Me.cmdNext.FlatAppearance.BorderSize = 0
        Me.cmdNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdNext.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdNext.Image = CType(resources.GetObject("cmdNext.Image"), System.Drawing.Image)
        Me.cmdNext.Location = New System.Drawing.Point(181, 6)
        Me.cmdNext.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.cmdNext.Name = "cmdNext"
        Me.cmdNext.Size = New System.Drawing.Size(47, 43)
        Me.cmdNext.TabIndex = 78
        Me.cmdNext.UseVisualStyleBackColor = True
        '
        'cmdPrevious
        '
        Me.cmdPrevious.Cursor = System.Windows.Forms.Cursors.Hand
        Me.cmdPrevious.FlatAppearance.BorderSize = 0
        Me.cmdPrevious.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdPrevious.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdPrevious.Image = CType(resources.GetObject("cmdPrevious.Image"), System.Drawing.Image)
        Me.cmdPrevious.Location = New System.Drawing.Point(5, 4)
        Me.cmdPrevious.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.cmdPrevious.Name = "cmdPrevious"
        Me.cmdPrevious.Size = New System.Drawing.Size(41, 44)
        Me.cmdPrevious.TabIndex = 9
        Me.cmdPrevious.UseVisualStyleBackColor = True
        '
        'cmbPageSize
        '
        Me.cmbPageSize.Cursor = System.Windows.Forms.Cursors.Hand
        Me.cmbPageSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbPageSize.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmbPageSize.FormattingEnabled = True
        Me.cmbPageSize.Items.AddRange(New Object() {"2", "5", "10", "15", "20", "25", "30"})
        Me.cmbPageSize.Location = New System.Drawing.Point(53, 11)
        Me.cmbPageSize.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.cmbPageSize.Name = "cmbPageSize"
        Me.cmbPageSize.Size = New System.Drawing.Size(119, 28)
        Me.cmbPageSize.TabIndex = 76
        '
        'GVDetail
        '
        DataGridViewCellStyle4.BackColor = System.Drawing.Color.LightGray
        DataGridViewCellStyle4.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle4.ForeColor = System.Drawing.Color.Black
        DataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.DarkGray
        DataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.Black
        DataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.GVDetail.AlternatingRowsDefaultCellStyle = DataGridViewCellStyle4
        DataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle5.BackColor = System.Drawing.Color.SteelBlue
        DataGridViewCellStyle5.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle5.ForeColor = System.Drawing.Color.White
        DataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight
        DataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText
        DataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.GVDetail.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle5
        Me.GVDetail.ColumnHeadersHeight = 50
        Me.GVDetail.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing
        Me.GVDetail.Cursor = System.Windows.Forms.Cursors.Hand
        DataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft
        DataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle6.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Info
        DataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.InfoText
        DataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.[True]
        Me.GVDetail.DefaultCellStyle = DataGridViewCellStyle6
        Me.GVDetail.Location = New System.Drawing.Point(11, 9)
        Me.GVDetail.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.GVDetail.Name = "GVDetail"
        Me.GVDetail.ReadOnly = True
        Me.GVDetail.RowTemplate.Height = 40
        Me.GVDetail.Size = New System.Drawing.Size(1099, 342)
        Me.GVDetail.TabIndex = 7
        '
        'Timer1
        '
        Me.Timer1.Interval = 1000
        '
        'pnlGrid
        '
        Me.pnlGrid.Controls.Add(Me.GVDetail)
        Me.pnlGrid.Location = New System.Drawing.Point(16, 106)
        Me.pnlGrid.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.pnlGrid.Name = "pnlGrid"
        Me.pnlGrid.Size = New System.Drawing.Size(1216, 362)
        Me.pnlGrid.TabIndex = 8
        '
        'bgWorker
        '
        Me.bgWorker.WorkerReportsProgress = True
        Me.bgWorker.WorkerSupportsCancellation = True
        '
        'pnlWait
        '
        Me.pnlWait.BackgroundImage = CType(resources.GetObject("pnlWait.BackgroundImage"), System.Drawing.Image)
        Me.pnlWait.Location = New System.Drawing.Point(173, 522)
        Me.pnlWait.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.pnlWait.Name = "pnlWait"
        Me.pnlWait.Size = New System.Drawing.Size(135, 124)
        Me.pnlWait.TabIndex = 9
        Me.pnlWait.Visible = False
        '
        'JobSheet
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1368, 960)
        Me.Controls.Add(Me.pnlWait)
        Me.Controls.Add(Me.pnlGrid)
        Me.Controls.Add(Me.GroupBox1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4, 4, 4, 4)
        Me.Name = "JobSheet"
        Me.Text = "SOP APPLICATION"
        Me.GroupBox1.ResumeLayout(False)
        Me.pnlDisplayTitle.ResumeLayout(False)
        Me.pnlDisplayTitle.PerformLayout()
        Me.pnlDocument.ResumeLayout(False)
        Me.pnlDocument.PerformLayout()
        CType(Me.GVDetail, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlGrid.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents cmdNext As System.Windows.Forms.Button
    Friend WithEvents cmbPageSize As System.Windows.Forms.ComboBox
    Friend WithEvents cmdPrevious As System.Windows.Forms.Button
    Friend WithEvents GVDetail As System.Windows.Forms.DataGridView
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents pnlDocument As System.Windows.Forms.Panel
    Friend WithEvents pnlGrid As System.Windows.Forms.Panel
    Friend WithEvents lbl0 As System.Windows.Forms.Label
    Friend WithEvents pnlDisplayTitle As System.Windows.Forms.Panel
    Friend WithEvents cmdSend As System.Windows.Forms.Button
    Friend WithEvents cmdClose As System.Windows.Forms.Button
    Friend WithEvents cmdRefresh As System.Windows.Forms.Button
    Friend WithEvents bgWorker As System.ComponentModel.BackgroundWorker
    Friend WithEvents txtCBNumber As System.Windows.Forms.TextBox
    Friend WithEvents txtLineNumber As System.Windows.Forms.TextBox
    Friend WithEvents lblCbnumber As System.Windows.Forms.Label
    Friend WithEvents lblLineNo As System.Windows.Forms.Label
    Friend WithEvents btnSort As System.Windows.Forms.Button
    Friend WithEvents btnDeleteRow As System.Windows.Forms.Button
    Friend WithEvents pnlWait As System.Windows.Forms.Panel

End Class
