Imports MySql.Data.MySqlClient
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions

Public Class FormSignUp
    Private passwordVisible As Boolean = True
    Private Sub ApplyFieldIndicators()
        lblFirstName.Text = "First Name"
        lblLastName.Text = "Last Name"
        lblUsername.Text = "Username"
        lblEmail.Text = "Email"
        lblPassword.Text = "Password"
        lblConfirmPassword.Text = "Confirm Password"
        lblRole.Text = "Role"
    End Sub

    Private Sub ShowAsteriskOnMissedFields()
        If String.IsNullOrWhiteSpace(txtFirstName.Text) Then lblFirstName.Text = "First Name *"
        If String.IsNullOrWhiteSpace(txtLastName.Text) Then lblLastName.Text = "Last Name *"
        If String.IsNullOrWhiteSpace(txtUsername.Text) Then lblUsername.Text = "Username *"
        If String.IsNullOrWhiteSpace(txtEmail.Text) Then lblEmail.Text = "Email *"
        If String.IsNullOrWhiteSpace(txtPass.Text) Then lblPassword.Text = "Password *"
        If String.IsNullOrWhiteSpace(txtConfPass.Text) Then lblConfirmPassword.Text = "Confirm Password *"
        If cbRole.SelectedItem Is Nothing Then lblRole.Text = "Role *"
    End Sub

    Private Sub RemoveAsteriskOnInput(sender As Object, e As EventArgs)
        Dim txtBox As TextBox = CType(sender, TextBox)

        Select Case txtBox.Name
            Case "txtFirstName" : lblFirstName.Text = "First Name"
            Case "txtLastName" : lblLastName.Text = "Last Name"
            Case "txtUsername" : lblUsername.Text = "Username"
            Case "txtEmail" : lblEmail.Text = "Email"
            Case "txtPass" : lblPassword.Text = "Password"
            Case "txtConfPass" : lblConfirmPassword.Text = "Confirm Password"
        End Select
    End Sub



    Private Sub FormSignUp_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideErrorLabels()
        txtAdminCode.Visible = False
        lblAdminCode.Visible = False
        ApplyFieldIndicators()

        AddHandler txtFirstName.TextChanged, AddressOf RemoveAsteriskOnInput
        AddHandler txtLastName.TextChanged, AddressOf RemoveAsteriskOnInput
        AddHandler txtUsername.TextChanged, AddressOf RemoveAsteriskOnInput
        AddHandler txtEmail.TextChanged, AddressOf RemoveAsteriskOnInput
        AddHandler txtPass.TextChanged, AddressOf RemoveAsteriskOnInput
        AddHandler txtConfPass.TextChanged, AddressOf RemoveAsteriskOnInput

        btnShowPass.Image = epm1.My.Resources.Resources.BttnHide
        btnShowConfPass.Image = epm1.My.Resources.Resources.BttnHide


    End Sub

    Private Sub btnSignUp_Click(sender As Object, e As EventArgs) Handles btnSignUp.Click
        HideErrorLabels()
        ShowAsteriskOnMissedFields()

        Dim missingFields As Boolean = False

        If String.IsNullOrWhiteSpace(txtFirstName.Text) Then
            SetMissingFieldIndicator(txtFirstName)
            missingFields = True
        End If

        If String.IsNullOrWhiteSpace(txtLastName.Text) Then
            SetMissingFieldIndicator(txtLastName)
            missingFields = True
        End If

        If String.IsNullOrWhiteSpace(txtUsername.Text) Then
            SetMissingFieldIndicator(txtUsername)
            missingFields = True
        End If

        If String.IsNullOrWhiteSpace(txtEmail.Text) Then
            SetMissingFieldIndicator(txtEmail)
            missingFields = True
        End If

        If String.IsNullOrWhiteSpace(txtPass.Text) Then
            SetMissingFieldIndicator(txtPass)
            missingFields = True
        End If

        If String.IsNullOrWhiteSpace(txtConfPass.Text) Then
            SetMissingFieldIndicator(txtConfPass)
            missingFields = True
        End If

        If cbRole.SelectedItem Is Nothing Then
            lblRequiredMessage.Visible = True
            missingFields = True
        End If

        If missingFields Then Return

        ValidateUserInput()
    End Sub

    Private Sub ValidateUserInput()
        If txtPass.Text <> txtConfPass.Text Then
            lblPasswordError.Text = "Passwords do not match!"
            lblPasswordError.Visible = True
            Return
        End If

        Dim strengthMessage As String = CheckPasswordStrength(txtPass.Text)
        lblPwStrength.Text = strengthMessage
        lblPwStrength.Visible = True

        Dim checkUserQuery As String = "SELECT COUNT(*) FROM Users WHERE username = @uname OR email = @email"
        Dim checkParameters As New Dictionary(Of String, Object) From {
            {"@uname", txtUsername.Text},
            {"@email", txtEmail.Text}
        }

        Try
            Dim userExists As Integer = Convert.ToInt32(DBHelper.ExecuteScalarQuery(checkUserQuery, checkParameters))

            If userExists > 0 Then
                lblUsernameError.Text = "Username already exists! Try " & txtUsername.Text & "123"
                lblUsernameError.Visible = True
                lblEmailError.Visible = True
                Return
            End If

            If cbRole.SelectedItem.ToString() = "Admin" Then
                Dim storedAdminCode As String = GetAdminCodeFromDatabase(txtUsername.Text)
                If txtAdminCode.Text <> storedAdminCode Then
                    lblAdminCodeError.Text = "Invalid admin authentication code."
                    lblAdminCodeError.Visible = True
                    Return
                End If
            End If

            Dim hashedPassword As String = HashPassword(txtPass.Text)

            Dim query As String = "INSERT INTO Users (first_name, last_name, username, email, password_hash, role) VALUES (@fname, @lname, @uname, @email, @pass, @role)"
            Dim parameters As New Dictionary(Of String, Object) From {
                {"@fname", txtFirstName.Text},
                {"@lname", txtLastName.Text},
                {"@uname", txtUsername.Text},
                {"@email", txtEmail.Text},
                {"@pass", hashedPassword},
                {"@role", cbRole.SelectedItem.ToString()}
            }

            DBHelper.ExecuteQuery(query, parameters)
            MessageBox.Show("Account created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)

            Me.Hide()
            Dim loginForm As New FormLogIn()
            loginForm.Show()

        Catch ex As MySqlException
            If ex.Number = 1062 Then
                lblEmailError.Visible = True
            Else
                MessageBox.Show("Database error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Catch ex As Exception
            MessageBox.Show("Unexpected error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function GetAdminCodeFromDatabase(username As String) As String
        Dim query As String = "SELECT admin_code FROM Users WHERE username = @uname"
        Dim parameters As New Dictionary(Of String, Object) From {
            {"@uname", username}
        }
        Return Convert.ToString(DBHelper.ExecuteScalarQuery(query, parameters))
    End Function

    Private Sub SetMissingFieldIndicator(txtBox As TextBox)
        txtBox.Text = "Required"
        txtBox.ForeColor = Color.Gray
        AddHandler txtBox.GotFocus, AddressOf RemovePlaceholder
        AddHandler txtBox.LostFocus, AddressOf RestorePlaceholder
    End Sub

    Private Sub RemovePlaceholder(sender As Object, e As EventArgs)
        Dim txtBox As TextBox = CType(sender, TextBox)
        If txtBox.Text = "Required" Then
            txtBox.Text = ""
            txtBox.ForeColor = Color.Black
        End If
    End Sub

    Private Sub RestorePlaceholder(sender As Object, e As EventArgs)
        Dim txtBox As TextBox = CType(sender, TextBox)
        If String.IsNullOrWhiteSpace(txtBox.Text) Then
            txtBox.Text = "Required"
            txtBox.ForeColor = Color.Gray
        End If
    End Sub

    Private Sub HideErrorLabels()
        lblRequiredMessage.Visible = False
        lblUsernameError.Visible = False
        lblEmailError.Visible = False
        lblPasswordError.Visible = False
        lblPwStrength.Visible = False
        lblAdminCodeError.Visible = False
    End Sub

    Private Function HashPassword(password As String) As String
        Dim sha256 As SHA256 = SHA256.Create()
        Dim hashedBytes As Byte() = sha256.ComputeHash(Encoding.UTF8.GetBytes(password))
        Return Convert.ToBase64String(hashedBytes)
    End Function

    Private Function CheckPasswordStrength(password As String) As String
        Dim lengthCriteria As Integer = password.Length
        Dim upperCriteria As Boolean = Regex.IsMatch(password, "[A-Z]")
        Dim lowerCriteria As Boolean = Regex.IsMatch(password, "[a-z]")
        Dim numberCriteria As Boolean = Regex.IsMatch(password, "\d")
        Dim specialCriteria As Boolean = Regex.IsMatch(password, "[^a-zA-Z0-9]")

        If lengthCriteria < 8 Then
            Return "Weak Password! Use a longer password with mixed characters."
        End If

        If lengthCriteria >= 8 AndAlso lengthCriteria < 12 AndAlso (upperCriteria OrElse lowerCriteria OrElse numberCriteria) Then
            Return "Moderate Password! Consider adding special characters and increasing length."
        End If

        If lengthCriteria >= 12 AndAlso upperCriteria AndAlso lowerCriteria AndAlso numberCriteria AndAlso specialCriteria Then
            Return "Strong Password! Your password is secure."
        End If

        Return "Weak Password! Must be at least 8 characters, include uppercase, lowercase, number, and special character."
    End Function


    Private Sub cbRole_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbRole.SelectedIndexChanged
        lblRole.Text = "Role"
        If cbRole.SelectedItem.ToString() = "Admin" Then
            txtAdminCode.Visible = True
            lblAdminCode.Visible = True
        Else
            txtAdminCode.Visible = False
            lblAdminCode.Visible = False
        End If
    End Sub

    Private Sub lnklblLogIn_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles lnklblLogIn.LinkClicked
        Dim loginForm As New FormLogIn()
        loginForm.Show()
        Me.Hide()
    End Sub

    Private Sub MoveToNextControl(sender As Object, e As KeyEventArgs) Handles txtFirstName.KeyDown, txtLastName.KeyDown, txtUsername.KeyDown, txtEmail.KeyDown, txtPass.KeyDown, txtConfPass.KeyDown, cbRole.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            If sender Is cbRole Then
                btnSignUp.PerformClick()
            Else
                SelectNextControl(DirectCast(sender, Control), True, True, True, True)
            End If
        End If
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

    Private Sub btnShowConfPass_Click(sender As Object, e As EventArgs) Handles btnShowConfPass.Click

        passwordVisible = Not passwordVisible
        btnShowConfPass.Image = epm1.My.Resources.Resources.BttnHide

        If passwordVisible Then
            btnShowConfPass.Image = Nothing
            txtConfPass.PasswordChar = ControlChars.NullChar
            btnShowConfPass.Image = epm1.My.Resources.Resources.BttnSeek
        Else
            txtConfPass.PasswordChar = "*"c
            btnShowConfPass.Image = Nothing
            btnShowConfPass.Image = epm1.My.Resources.Resources.BttnHide
        End If

    End Sub

    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        GoBack(Me)
    End Sub

    Private Sub btnNext_Click(sender As Object, e As EventArgs) Handles btnNext.Click
        GoNext(Me)
    End Sub
End Class