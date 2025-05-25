Imports System.Security.Cryptography
Imports System.Text

Public Class FormLogIn
    Private passwordVisible As Boolean = True

    Private Sub FormLogIn_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideErrorLabels()
        ResetFieldIndicators()

        AddHandler txtEmail.TextChanged, AddressOf RemoveAsteriskOnInput
        AddHandler txtPass.TextChanged, AddressOf RemoveAsteriskOnInput

        HideErrorLabels()
        ResetFieldIndicators()

        AddHandler txtEmail.TextChanged, AddressOf RemoveAsteriskOnInput
        AddHandler txtPass.TextChanged, AddressOf RemoveAsteriskOnInput

        ' Set initial password character and icon
        passwordVisible = False
        txtPass.PasswordChar = "*"c
        btnShowPass.Image = epm1.My.Resources.Resources.BttnHide


    End Sub

    Private Sub btnLogIn_Click(sender As Object, e As EventArgs) Handles btnLogIn.Click
        HideErrorLabels()
        ResetFieldIndicators()

        Dim missingFields As Boolean = False

        If String.IsNullOrWhiteSpace(txtEmail.Text) Then MarkFieldAsMissing(lblEmail) : missingFields = True
        If String.IsNullOrWhiteSpace(txtPass.Text) Then MarkFieldAsMissing(lblPassword) : missingFields = True

        If missingFields Then Return

        ValidateUserLogin()
    End Sub

    Private Sub ValidateUserLogin()
        Dim query As String = "SELECT user_id, username, email, password_hash, role FROM Users WHERE BINARY email = @email"
        Dim parameters As New Dictionary(Of String, Object) From {{"@email", txtEmail.Text}}
        Dim dt As DataTable = DBHelper.GetDataTable(query, parameters)

        If dt.Rows.Count > 0 Then
            Dim storedHash As String = dt.Rows(0)("password_hash").ToString()

            If VerifyPassword(txtPass.Text, storedHash) Then
                ' Store user details securely
                CurrentUser.UserID = CInt(dt.Rows(0)("user_id"))
                CurrentUser.Username = dt.Rows(0)("username").ToString()
                CurrentUser.Email = dt.Rows(0)("email").ToString()
                CurrentUser.Role = dt.Rows(0)("role").ToString()

                lblGeneralError.Visible = False
                MessageBox.Show("Login successful!", "Welcome " & CurrentUser.Username, MessageBoxButtons.OK, MessageBoxIcon.Information)

                ' Redirect based on role
                Select Case CurrentUser.Role
                    Case "Admin"
                        Me.Hide()
                        Dim adminForm As New FormAdminCenter()
                        adminForm.Show()
                    Case "User"
                        Me.Hide()
                        Dim mainForm As New FormMain()
                        mainForm.Show()
                    Case Else
                        lblGeneralError.Text = "Invalid role detected! Contact support."
                        lblGeneralError.Visible = True
                End Select
            Else
                lblGeneralError.Text = "Invalid credentials."
                lblGeneralError.Visible = True
            End If
        Else
            lblGeneralError.Text = "Invalid credentials."
            lblGeneralError.Visible = True
        End If
    End Sub

    Private Function VerifyPassword(inputPassword As String, storedHash As String) As Boolean
        Using sha256 As SHA256 = SHA256.Create()
            Dim inputHash As Byte() = sha256.ComputeHash(Encoding.UTF8.GetBytes(inputPassword))
            Return Convert.ToBase64String(inputHash) = storedHash
        End Using
    End Function

    Private Sub ResetFieldIndicators()
        lblEmail.Text = "Email"
        lblPassword.Text = "Password"
    End Sub

    Private Sub MarkFieldAsMissing(lbl As Label)
        lbl.Text &= " *"
        lbl.ForeColor = Color.Red
    End Sub

    Private Sub RemoveAsteriskOnInput(sender As Object, e As EventArgs)
        Dim txtBox As TextBox = CType(sender, TextBox)

        Select Case txtBox.Name
            Case "txtEmail" : lblEmail.Text = "Email"
            Case "txtPass" : lblPassword.Text = "Password"
        End Select
    End Sub

    Private Sub btnShowPass_Click(sender As Object, e As EventArgs) Handles btnShowPass.Click

        passwordVisible = Not passwordVisible
        btnShowPass.Image = epm1.My.Resources.Resources.BttnHide

        If passwordVisible Then
            btnShowPass.Image = Nothing
            txtPass.PasswordChar = ControlChars.NullChar
            btnShowPass.Image = epm1.My.Resources.Resources.BttnSeek
        Else
            txtPass.PasswordChar = "*"c
            btnShowPass.Image = Nothing
            btnShowPass.Image = epm1.My.Resources.Resources.BttnHide
        End If

    End Sub

    Private Sub lnklblSignUp_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles lnklblSignUp.LinkClicked
        Dim signUpForm As New FormSignUp()
        Me.Hide()
        signUpForm.ShowDialog()
    End Sub

    Private Sub MoveToNextControl(sender As Object, e As KeyEventArgs) Handles txtEmail.KeyDown, txtPass.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            If sender Is txtPass Then
                btnLogIn.PerformClick()
            Else
                SelectNextControl(DirectCast(sender, Control), True, True, True, True)
            End If
        End If
    End Sub

    Private Sub HideErrorLabels()
        lblGeneralError.Visible = False
    End Sub

    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        GoBack(Me)
    End Sub

    Private Sub btnNext_Click(sender As Object, e As EventArgs) Handles btnNext.Click
        GoNext(Me)
    End Sub

End Class
