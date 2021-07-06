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
        Me.btnRedoFile = New System.Windows.Forms.Button
        Me.cmdRefresh = New System.Windows.Forms.Button
        Me.cmdDone = New System.Windows.Forms.Button
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
        Me.GroupBox1.Margin = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Padding = New System.Windows.Forms.Padding(4)
        Me.GroupBox1.Size = New System.Drawing.Size(1381, 92)
        Me.GroupBox1.TabIndex = 4
        Me.GroupBox1.TabStop = False
        '
        'pnlDisplayTitle
        '
        Me.pnlDisplayTitle.Controls.Add(Me.lbl0)
        Me.pnlDisplayTitle.Dock = System.Windows.Forms.DockStyle.Left
        Me.pnlDisplayTitle.Location = New System.Drawing.Point(4, 19)
        Me.pnlDisplayTitle.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlDisplayTitle.Name = "pnlDisplayTitle"
        Me.pnlDisplayTitle.Size = New System.Drawing.Size(307, 69)
        Me.pnlDisplayTitle.TabIndex = 82
        Me.pnlDisplayTitle.Visible = False
        '
        'lbl0
        '
        Me.lbl0.AutoSize = True
        Me.lbl0.Font = New System.Drawing.Font("Microsoft Sans Serif", 25.2!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl0.Location = New System.Drawing.Point(17, 5)
        Me.lbl0.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.lbl0.Name = "lbl0"
        Me.lbl0.Size = New System.Drawing.Size(305, 51)
        Me.lbl0.TabIndex = 1
        Me.lbl0.Text = "EZYSTOP 2.0"
        Me.lbl0.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'pnlDocument
        '
        Me.pnlDocument.Controls.Add(Me.btnRedoFile)
        Me.pnlDocument.Controls.Add(Me.cmdRefresh)
        Me.pnlDocument.Controls.Add(Me.cmdDone)
        Me.pnlDocument.Controls.Add(Me.cmdClose)
        Me.pnlDocument.Controls.Add(Me.cmdSend)
        Me.pnlDocument.Controls.Add(Me.cmdNext)
        Me.pnlDocument.Controls.Add(Me.cmdPrevious)
        Me.pnlDocument.Controls.Add(Me.cmbPageSize)
        Me.pnlDocument.Dock = System.Windows.Forms.DockStyle.Right
        Me.pnlDocument.Location = New System.Drawing.Point(330, 19)
        Me.pnlDocument.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlDocument.Name = "pnlDocument"
        Me.pnlDocument.Size = New System.Drawing.Size(1047, 69)
        Me.pnlDocument.TabIndex = 81
        '
        'btnRedoFile
        '
        Me.btnRedoFile.Cursor = System.Windows.Forms.Cursors.Hand
        Me.btnRedoFile.FlatAppearance.BorderSize = 0
        Me.btnRedoFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnRedoFile.Image = CType(resources.GetObject("btnRedoFile.Image"), System.Drawing.Image)
        Me.btnRedoFile.Location = New System.Drawing.Point(631, 4)
        Me.btnRedoFile.Margin = New System.Windows.Forms.Padding(4)
        Me.btnRedoFile.Name = "btnRedoFile"
        Me.btnRedoFile.Size = New System.Drawing.Size(145, 59)
        Me.btnRedoFile.TabIndex = 87
        Me.btnRedoFile.UseVisualStyleBackColor = True
        '
        'cmdRefresh
        '
        Me.cmdRefresh.Cursor = System.Windows.Forms.Cursors.Hand
        Me.cmdRefresh.FlatAppearance.BorderSize = 0
        Me.cmdRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdRefresh.Image = CType(resources.GetObject("cmdRefresh.Image"), System.Drawing.Image)
        Me.cmdRefresh.Location = New System.Drawing.Point(776, 2)
        Me.cmdRefresh.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdRefresh.Name = "cmdRefresh"
        Me.cmdRefresh.Size = New System.Drawing.Size(148, 62)
        Me.cmdRefresh.TabIndex = 86
        Me.cmdRefresh.UseVisualStyleBackColor = True
        '
        'cmdDone
        '
        Me.cmdDone.Cursor = System.Windows.Forms.Cursors.Hand
        Me.cmdDone.FlatAppearance.BorderSize = 0
        Me.cmdDone.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdDone.Image = CType(resources.GetObject("cmdDone.Image"), System.Drawing.Image)
        Me.cmdDone.Location = New System.Drawing.Point(476, 4)
        Me.cmdDone.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdDone.Name = "cmdDone"
        Me.cmdDone.Size = New System.Drawing.Size(161, 59)
        Me.cmdDone.TabIndex = 85
        Me.cmdDone.UseVisualStyleBackColor = True
        '
        'cmdClose
        '
        Me.cmdClose.Cursor = System.Windows.Forms.Cursors.Hand
        Me.cmdClose.FlatAppearance.BorderSize = 0
        Me.cmdClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdClose.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdClose.Image = CType(resources.GetObject("cmdClose.Image"), System.Drawing.Image)
        Me.cmdClose.Location = New System.Drawing.Point(917, 6)
        Me.cmdClose.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdClose.Name = "cmdClose"
        Me.cmdClose.Size = New System.Drawing.Size(117, 54)
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
        Me.cmdSend.Location = New System.Drawing.Point(371, 4)
        Me.cmdSend.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdSend.Name = "cmdSend"
        Me.cmdSend.Size = New System.Drawing.Size(115, 60)
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
        Me.cmdNext.Location = New System.Drawing.Point(273, 2)
        Me.cmdNext.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdNext.Name = "cmdNext"
        Me.cmdNext.Size = New System.Drawing.Size(97, 62)
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
        Me.cmdPrevious.Location = New System.Drawing.Point(32, 2)
        Me.cmdPrevious.Margin = New System.Windows.Forms.Padding(4)
        Me.cmdPrevious.Name = "cmdPrevious"
        Me.cmdPrevious.Size = New System.Drawing.Size(88, 62)
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
        Me.cmbPageSize.Location = New System.Drawing.Point(147, 17)
        Me.cmbPageSize.Margin = New System.Windows.Forms.Padding(4)
        Me.cmbPageSize.Name = "cmbPageSize"
        Me.cmbPageSize.Size = New System.Drawing.Size(119, 28)
        Me.cmbPageSize.TabIndex = 76
        '
        'GVDetail
        '
        DataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window
        DataGridViewCellStyle4.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        DataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText
        DataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Info
        DataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.InfoText
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
        Me.GVDetail.Margin = New System.Windows.Forms.Padding(4)
        Me.GVDetail.Name = "GVDetail"
        Me.GVDetail.ReadOnly = True
        Me.GVDetail.RowTemplate.Height = 40
        Me.GVDetail.Size = New System.Drawing.Size(1099, 342)
        Me.GVDetail.TabIndex = 7
        '
        'Timer1
        '
        Me.Timer1.Interval = 10000
        '
        'pnlGrid
        '
        Me.pnlGrid.Controls.Add(Me.GVDetail)
        Me.pnlGrid.Location = New System.Drawing.Point(16, 100)
        Me.pnlGrid.Margin = New System.Windows.Forms.Padding(4)
        Me.pnlGrid.Name = "pnlGrid"
        Me.pnlGrid.Size = New System.Drawing.Size(1216, 362)
        Me.pnlGrid.TabIndex = 8
        '
        'bgWorker
        '
        Me.bgWorker.WorkerReportsProgress = True
        Me.bgWorker.WorkerSupportsCancellation = True
        '
        'JobSheet
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1381, 960)
        Me.Controls.Add(Me.pnlGrid)
        Me.Controls.Add(Me.GroupBox1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4)
        Me.Name = "JobSheet"
        Me.Text = "SOP APPLICATION"
        Me.GroupBox1.ResumeLayout(False)
        Me.pnlDisplayTitle.ResumeLayout(False)
        Me.pnlDisplayTitle.PerformLayout()
        Me.pnlDocument.ResumeLayout(False)
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
    Friend WithEvents cmdDone As System.Windows.Forms.Button
    Friend WithEvents bgWorker As System.ComponentModel.BackgroundWorker
    Friend WithEvents btnRedoFile As System.Windows.Forms.Button

End Class
