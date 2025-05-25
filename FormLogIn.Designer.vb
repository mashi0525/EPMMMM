<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormLogIn
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
        Me.lblPassword = New System.Windows.Forms.Label()
        Me.lblEmail = New System.Windows.Forms.Label()
        Me.btnLogIn = New System.Windows.Forms.Button()
        Me.btnShowPass = New System.Windows.Forms.Button()
        Me.txtEmail = New System.Windows.Forms.TextBox()
        Me.lnklblSignUp = New System.Windows.Forms.LinkLabel()
        Me.txtPass = New System.Windows.Forms.TextBox()
        Me.lblGeneralError = New System.Windows.Forms.Label()
        Me.btnNext = New System.Windows.Forms.Button()
        Me.btnBack = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'lblPassword
        '
        Me.lblPassword.AutoSize = True
        Me.lblPassword.BackColor = System.Drawing.Color.Transparent
        Me.lblPassword.Font = New System.Drawing.Font("Cinzel", 8.249999!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPassword.ForeColor = System.Drawing.Color.Black
        Me.lblPassword.Location = New System.Drawing.Point(311, 237)
        Me.lblPassword.Name = "lblPassword"
        Me.lblPassword.Size = New System.Drawing.Size(66, 15)
        Me.lblPassword.TabIndex = 40
        Me.lblPassword.Text = "Password"
        '
        'lblEmail
        '
        Me.lblEmail.AutoSize = True
        Me.lblEmail.BackColor = System.Drawing.Color.Transparent
        Me.lblEmail.Font = New System.Drawing.Font("Cinzel", 8.249999!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblEmail.ForeColor = System.Drawing.Color.Black
        Me.lblEmail.Location = New System.Drawing.Point(312, 190)
        Me.lblEmail.Name = "lblEmail"
        Me.lblEmail.Size = New System.Drawing.Size(44, 15)
        Me.lblEmail.TabIndex = 39
        Me.lblEmail.Text = "E-mail"
        '
        'btnLogIn
        '
        Me.btnLogIn.BackColor = System.Drawing.Color.Transparent
        Me.btnLogIn.BackgroundImage = Global.epm1.My.Resources.Resources.BttnLogIn
        Me.btnLogIn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.btnLogIn.FlatAppearance.BorderSize = 0
        Me.btnLogIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnLogIn.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnLogIn.ForeColor = System.Drawing.Color.Black
        Me.btnLogIn.Location = New System.Drawing.Point(408, 384)
        Me.btnLogIn.Name = "btnLogIn"
        Me.btnLogIn.Size = New System.Drawing.Size(129, 31)
        Me.btnLogIn.TabIndex = 36
        Me.btnLogIn.UseVisualStyleBackColor = False
        '
        'btnShowPass
        '
        Me.btnShowPass.BackColor = System.Drawing.Color.Transparent
        Me.btnShowPass.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center
        Me.btnShowPass.FlatAppearance.BorderSize = 0
        Me.btnShowPass.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnShowPass.Font = New System.Drawing.Font("Lucida Bright", 7.875!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnShowPass.ForeColor = System.Drawing.Color.Black
        Me.btnShowPass.Location = New System.Drawing.Point(599, 251)
        Me.btnShowPass.Name = "btnShowPass"
        Me.btnShowPass.Size = New System.Drawing.Size(26, 23)
        Me.btnShowPass.TabIndex = 38
        Me.btnShowPass.UseVisualStyleBackColor = False
        '
        'txtEmail
        '
        Me.txtEmail.BackColor = System.Drawing.SystemColors.Control
        Me.txtEmail.Font = New System.Drawing.Font("Poppins", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtEmail.ForeColor = System.Drawing.Color.DimGray
        Me.txtEmail.Location = New System.Drawing.Point(312, 209)
        Me.txtEmail.Name = "txtEmail"
        Me.txtEmail.Size = New System.Drawing.Size(316, 24)
        Me.txtEmail.TabIndex = 32
        '
        'lnklblSignUp
        '
        Me.lnklblSignUp.AutoSize = True
        Me.lnklblSignUp.BackColor = System.Drawing.Color.Transparent
        Me.lnklblSignUp.Font = New System.Drawing.Font("Poppins", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lnklblSignUp.ForeColor = System.Drawing.Color.Black
        Me.lnklblSignUp.Location = New System.Drawing.Point(310, 289)
        Me.lnklblSignUp.Name = "lnklblSignUp"
        Me.lnklblSignUp.Size = New System.Drawing.Size(182, 19)
        Me.lnklblSignUp.TabIndex = 35
        Me.lnklblSignUp.TabStop = True
        Me.lnklblSignUp.Text = "Don't have an account? Sign Up"
        '
        'txtPass
        '
        Me.txtPass.BackColor = System.Drawing.SystemColors.Control
        Me.txtPass.Font = New System.Drawing.Font("Poppins", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtPass.ForeColor = System.Drawing.Color.DimGray
        Me.txtPass.Location = New System.Drawing.Point(312, 255)
        Me.txtPass.Name = "txtPass"
        Me.txtPass.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.txtPass.Size = New System.Drawing.Size(284, 24)
        Me.txtPass.TabIndex = 33
        '
        'lblGeneralError
        '
        Me.lblGeneralError.AutoSize = True
        Me.lblGeneralError.BackColor = System.Drawing.SystemColors.Control
        Me.lblGeneralError.Font = New System.Drawing.Font("Poppins", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblGeneralError.Location = New System.Drawing.Point(516, 288)
        Me.lblGeneralError.Name = "lblGeneralError"
        Me.lblGeneralError.Size = New System.Drawing.Size(112, 19)
        Me.lblGeneralError.TabIndex = 42
        Me.lblGeneralError.Text = "Invalid credentials."
        '
        'btnNext
        '
        Me.btnNext.Location = New System.Drawing.Point(62, 12)
        Me.btnNext.Name = "btnNext"
        Me.btnNext.Size = New System.Drawing.Size(42, 23)
        Me.btnNext.TabIndex = 89
        Me.btnNext.Text = "→"
        Me.btnNext.UseVisualStyleBackColor = True
        '
        'btnBack
        '
        Me.btnBack.Location = New System.Drawing.Point(14, 11)
        Me.btnBack.Name = "btnBack"
        Me.btnBack.Size = New System.Drawing.Size(42, 23)
        Me.btnBack.TabIndex = 88
        Me.btnBack.Text = "←"
        Me.btnBack.UseVisualStyleBackColor = True
        '
        'FormLogIn
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackgroundImage = Global.epm1.My.Resources.Resources.LogIn_no_lbl_
        Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
        Me.ClientSize = New System.Drawing.Size(944, 501)
        Me.Controls.Add(Me.btnNext)
        Me.Controls.Add(Me.btnBack)
        Me.Controls.Add(Me.lblGeneralError)
        Me.Controls.Add(Me.lblPassword)
        Me.Controls.Add(Me.lblEmail)
        Me.Controls.Add(Me.btnLogIn)
        Me.Controls.Add(Me.btnShowPass)
        Me.Controls.Add(Me.txtEmail)
        Me.Controls.Add(Me.lnklblSignUp)
        Me.Controls.Add(Me.txtPass)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormLogIn"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "FormLogIn"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblPassword As Label
    Friend WithEvents lblEmail As Label
    Friend WithEvents btnLogIn As Button
    Friend WithEvents btnShowPass As Button
    Friend WithEvents txtEmail As TextBox
    Friend WithEvents lnklblSignUp As LinkLabel
    Friend WithEvents txtPass As TextBox
    Friend WithEvents lblGeneralError As Label
    Friend WithEvents btnNext As Button
    Friend WithEvents btnBack As Button
End Class
