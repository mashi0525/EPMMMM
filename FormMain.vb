Imports System.Data.SqlClient
Imports MySql.Data.MySqlClient

Public Class FormMain
    Private nextForm As Form
    Private WithEvents TimerHide As New Timer
    Private WithEvents TimerShow As New Timer

    Private Sub FormMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        pnlFilter.Left = Me.Width 
        pnlFilter.Visible = False

        TimerHide.Interval = 5
        TimerShow.Interval = 5


        pnlFilter.Left = Me.Width
        pnlFilter.Visible = False


        EnableDoubleBuffering(pnlFilter)
        EnableDoubleBuffering(flpResults)
        EnableDoubleBuffering(Me)

        PopulateVenueCombo()
        LoadSearchResults()
        UpdatePanelVisibility()
        AdjustResultsPanel()
    End Sub


    Private Sub btnFilter_Click(sender As Object, e As EventArgs) Handles btnFilter.Click
        If pnlFilter.Visible Then
            TimerHide.Start()
        Else
            pnlFilter.Visible = True
            TimerShow.Start()
        End If
    End Sub


    Private Sub TimerHide_Tick(sender As Object, e As EventArgs) Handles TimerHide.Tick
        Me.SuspendLayout()
        If pnlFilter.Left < Me.Width Then
            pnlFilter.Left += 20
        Else
            TimerHide.Stop()
            Application.DoEvents()
            pnlFilter.Visible = False
            Me.ResumeLayout()
            AdjustResultsPanel()
        End If
    End Sub


    Private Sub TimerShow_Tick(sender As Object, e As EventArgs) Handles TimerShow.Tick
        Me.SuspendLayout()
        If pnlFilter.Left > Me.Width - pnlFilter.Width - 10 Then
            pnlFilter.Left -= 20
        Else
            TimerShow.Stop()
            Me.ResumeLayout()
            AdjustResultsPanel()
        End If
    End Sub


    Private Sub AdjustResultsPanel()
        If pnlFilter.Visible Then
            flpResults.Width = Me.Width - pnlFilter.Width
        Else
            flpResults.Width = Me.Width
            pnlFilter.Left = Me.Width
        End If
    End Sub

    Public Sub EnableDoubleBuffering(ByVal ctrl As Control)
        Dim doubleBufferProp As System.Reflection.PropertyInfo = ctrl.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic Or System.Reflection.BindingFlags.Instance)
        If doubleBufferProp IsNot Nothing Then doubleBufferProp.SetValue(ctrl, True, Nothing)
    End Sub

    Private Sub UpdatePanelVisibility()
        If Not String.IsNullOrEmpty(CurrentUser.Username) Then
            pnlAccount.Dock = DockStyle.None


            pnlSignUpLogIn.Visible = False
            pnlAccount.Visible = True
            pnlAccount.BringToFront()

            lblUsername.Text = CurrentUser.Username
        Else
            pnlSignUpLogIn.Dock = DockStyle.None

            pnlAccount.Visible = False
            pnlSignUpLogIn.Visible = True
            pnlSignUpLogIn.BringToFront()
        End If
    End Sub

    Private Sub PopulateVenueCombo()
        Dim query As String = "SELECT DISTINCT event_place, opening_hours, closing_hours, available_days FROM eventplace ORDER BY event_place ASC"
        Using connection As MySqlConnection = DBHelper.GetConnection()
            Using cmd As New MySqlCommand(query, connection)
                Dim dt As New DataTable()
                Dim adapter As New MySqlDataAdapter(cmd)
                Try
                    connection.Open()
                    adapter.Fill(dt)
                Catch ex As Exception
                    MessageBox.Show("Error loading venue types: " & ex.Message)
                End Try
            End Using
        End Using
    End Sub


    Private ReadOnly EventTypeMapping As New Dictionary(Of String, List(Of String)) From {
    {"Classes & Workshops", New List(Of String) From {"Cooking", "Fitness", "Coffee Workshop", "Tea Workshop"}},
    {"Corporate Event", New List(Of String) From {"Dining", "Party"}},
    {"Formal Meetings & Team Gatherings", New List(Of String) From {"Conference", "Interview", "Sales Meeting", "Team Meeting", "Offsite Meeting", "Team Bonding", "Training"}},
    {"Parties & Celebrations", New List(Of String) From {"Anniversary", "Baby Shower", "Birthday Party", "Holiday & Festive Celebrations", "Deepavali", "Hari Raya", "Year-End Party", "Graduation Party", "Dinner & Dance", "Lunch/Dinner", "Prom"}},
    {"Shoots & Productions", New List(Of String) From {"Green Screen Shoot", "Live Stream", "Live Webinar", "Photo Shoot", "Video Production"}},
    {"Weddings & Related Events", New List(Of String) From {"Bachelor/Bachelorette Party", "Bridal Shower", "Ceremony", "Engagement", "Proposal", "Reception", "Solemnization", "Wedding"}}
}


    Private Sub LoadSearchResults()
        Dim query As String = "SELECT * FROM eventplace WHERE 1=1"

        Dim selectedDays As New List(Of String)
        For Each selectedItem As String In clbAvailableOn.CheckedItems
            selectedDays.Add(selectedItem)
        Next

        If selectedDays.Count > 0 Then
            Dim dayConditions As New List(Of String)
            For Each day In selectedDays
                dayConditions.Add($"FIND_IN_SET('{day}', available_days) > 0")
            Next
            query &= " AND (" & String.Join(" OR ", dayConditions) & ")"
        End If

        Dim selectedEventTypes As New List(Of String)
        For Each selectedItem In clbEventType.CheckedItems
            Dim trimmedItem As String = selectedItem.ToString().Trim()

            If EventTypeMapping.ContainsKey(trimmedItem) Then
                selectedEventTypes.AddRange(EventTypeMapping(trimmedItem))
            Else
                selectedEventTypes.Add(trimmedItem)
            End If
        Next

        If selectedEventTypes.Count > 0 Then
            Dim typeConditions As New List(Of String)
            For Each eventType In selectedEventTypes
                typeConditions.Add($"event_type LIKE '%{eventType}%'")
            Next
            query &= " AND (" & String.Join(" OR ", typeConditions) & ")"
        End If

        Dim minCapacity, maxCapacity As Integer
        If Integer.TryParse(txtMinCapacity.Text, minCapacity) Then query &= " AND capacity >= @minCapacity"
        If Integer.TryParse(txtMaxCapacity.Text, maxCapacity) Then query &= " AND capacity <= @maxCapacity"

        Dim minPrice, maxPrice As Decimal
        If Decimal.TryParse(txtMinPrice.Text, minPrice) Then query &= " AND price_per_day >= @minPrice"
        If Decimal.TryParse(txtMaxPrice.Text, maxPrice) Then query &= " AND price_per_day <= @maxPrice"

        Dim sortColumn As String = ""
        Dim sortDirection As String = ""

        Select Case cbSort.SelectedItem?.ToString()
            Case "Alphabetical (A-Z)"
                sortColumn = "event_place"
                sortDirection = "ASC"
            Case "Alphabetical (Z-A)"
                sortColumn = "event_place"
                sortDirection = "DESC"
            Case "Capacity (Lowest to Highest)"
                sortColumn = "capacity"
                sortDirection = "ASC"
            Case "Capacity (Highest to Lowest)"
                sortColumn = "capacity"
                sortDirection = "DESC"
            Case "Price (Lowest to Highest)"
                sortColumn = "price_per_day"
                sortDirection = "ASC"
            Case "Price (Highest to Lowest)"
                sortColumn = "price_per_day"
                sortDirection = "DESC"
        End Select

        If Not String.IsNullOrEmpty(sortColumn) Then
            query &= $" ORDER BY {sortColumn} {sortDirection}"
        End If

        Dim dt As New DataTable()
        Using connection As MySqlConnection = DBHelper.GetConnection()
            Using cmd As New MySqlCommand(query, connection)
                If Integer.TryParse(txtMinCapacity.Text, minCapacity) Then cmd.Parameters.AddWithValue("@minCapacity", minCapacity)
                If Integer.TryParse(txtMaxCapacity.Text, maxCapacity) Then cmd.Parameters.AddWithValue("@maxCapacity", maxCapacity)
                If Decimal.TryParse(txtMinPrice.Text, minPrice) Then cmd.Parameters.AddWithValue("@minPrice", minPrice)
                If Decimal.TryParse(txtMaxPrice.Text, maxPrice) Then cmd.Parameters.AddWithValue("@maxPrice", maxPrice)

                Dim adapter As New MySqlDataAdapter(cmd)
                Try
                    connection.Open()
                    dt.Clear()
                    adapter.Fill(dt)
                Catch ex As Exception
                    MessageBox.Show("Error loading venues: " & ex.Message)
                End Try
            End Using
        End Using

        flpResults.Controls.Clear()
        HelperResultsDisplay.PopulateResults(flpResults, dt, AddressOf btnBook_Click)
    End Sub

    Private customerId As Integer = -1
    Public Sub SetCustomerId(newCustomerId As Integer)
        customerId = newCustomerId
    End Sub


    Private Sub btnBook_Click(sender As Object, e As EventArgs)
        If String.IsNullOrEmpty(CurrentUser.Username) Then
            Dim result As DialogResult = MessageBox.Show("You need to log in to book an event place. Proceed to login?", "Login Required", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)

            If result = DialogResult.Yes Then
                Dim loginForm As New FormLogIn()
                loginForm.ShowDialog()
                If String.IsNullOrEmpty(CurrentUser.Username) Then
                    loginForm.ShowDialog()
                    Exit Sub
                End If
            Else
                Exit Sub
            End If
        End If


        If CurrentUser.Role = "Admin" Then
            MessageBox.Show("Admins cannot book an event place. Redirecting to Admin Center.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning)

            If Application.OpenForms.OfType(Of FormAdminCenter).Any() Then
                Me.Hide()
                Exit Sub
            End If

            Dim adminForm As New FormAdminCenter()
            adminForm.Show()
            Me.Hide()
            Exit Sub
        End If

        Dim btn As Button = CType(sender, Button)
        Dim row As DataRow = CType(btn.Tag, DataRow)
        Dim placeId As Integer = CInt(row("place_id"))
        Dim capacity As Integer = CInt(row("capacity"))
        Dim pricePerDay As Decimal = CDec(row("price_per_day"))

        Dim bookingForm As New FormBooking(CurrentUser.UserID, placeId) With {
    .EventPlaceName = row("event_place").ToString(),
    .EventPlaceCapacity = CInt(row("capacity")),
    .BasePricePerDay = CDec(row("price_per_day")),
    .EventPlaceFeatures = row("features").ToString(),
    .EventPlaceDescription = row("description").ToString(),
    .OpeningHours = row("opening_hours").ToString(),
    .ClosingHours = row("closing_hours").ToString(),
    .AvailableDays = row("available_days").ToString(),
    .EventPlaceImageUrl = row("image_url").ToString()
}

        bookingForm.ShowDialog()
        Me.Hide()
    End Sub


    Private Sub btnApply_Click(sender As Object, e As EventArgs) Handles btnApply.Click
        LoadSearchResults()
    End Sub

    Private Sub btnCustomerView_Click(sender As Object, e As EventArgs) Handles btnCustomerView.Click
        Debug.WriteLine($"Attempting to open CustomerView with CustomerId: {CurrentUser.CustomerId}")

        Dim customerView As New FormCustomerView(CurrentUser.CustomerId)
        customerView.ShowDialog()
        Me.Hide()
    End Sub



    Private Sub btnSignUp_Click(sender As Object, e As EventArgs) Handles btnSignUp.Click
        Dim signUpForm As New FormSignUp()
        signUpForm.Show()
        Me.Hide()
    End Sub
    Private Sub btnLogIn_Click(sender As Object, e As EventArgs) Handles btnLogIn.Click
        Dim loginForm As New FormLogIn()
        loginForm.ShowDialog()


        If CurrentUser.CustomerId > 0 Then
            UpdatePanelVisibility()
            pnlAccount.Visible = True
            pnlSignUpLogIn.Visible = False
            Me.Refresh()
            Application.DoEvents()
        Else
            MessageBox.Show("Login failed or customer not found.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub


    Private Sub txtField_Enter(sender As Object, e As EventArgs) Handles txtMinCapacity.Enter, txtMaxCapacity.Enter, txtMinPrice.Enter, txtMaxPrice.Enter
        Dim txt As TextBox = CType(sender, TextBox)
        If txt.Text = "Min" OrElse txt.Text = "Max" Then
            txt.Text = ""
        End If
    End Sub


    Private Sub txtField_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtMinCapacity.KeyPress, txtMaxCapacity.KeyPress, txtMinPrice.KeyPress, txtMaxPrice.KeyPress
        If Not Char.IsControl(e.KeyChar) AndAlso Not Char.IsDigit(e.KeyChar) Then
            e.Handled = True
        End If
    End Sub

    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        Dim searchText As String = txtSearch.Text.Trim()


        Dim query As String = "SELECT * FROM eventplace WHERE event_place LIKE @search OR event_type LIKE @search"

        Dim dt As New DataTable()
        Using connection As MySqlConnection = DBHelper.GetConnection()
            Using cmd As New MySqlCommand(query, connection)
                cmd.Parameters.AddWithValue("@search", "%" & searchText & "%")

                Dim adapter As New MySqlDataAdapter(cmd)
                Try
                    connection.Open()
                    dt.Clear()
                    adapter.Fill(dt)
                Catch ex As Exception
                    MessageBox.Show("Error performing search: " & ex.Message)
                End Try
            End Using
        End Using

        flpResults.Controls.Clear()
        HelperResultsDisplay.PopulateResults(flpResults, dt, AddressOf btnBook_Click)
    End Sub

    Private Sub txtSearch_KeyDown(sender As Object, e As KeyEventArgs) Handles txtSearch.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            btnSearch.PerformClick()
        End If
    End Sub

    Private Sub btnLogOut_Click(sender As Object, e As EventArgs) Handles btnLogOut.Click
        CurrentUser.UserID = -1
        CurrentUser.Username = String.Empty
        CurrentUser.Email = String.Empty

        CurrentUser.Role = String.Empty
        CurrentUser.CustomerId = -1

        pnlSignUpLogIn.Visible = True
        pnlAccount.Visible = False
        btnSignUp.Visible = True
        btnLogIn.Visible = True
        btnCustomerView.Visible = False

        UpdatePanelVisibility()
        Me.Refresh()
        Application.DoEvents()
    End Sub


    Private Sub ClearFilters()
        clbEventType.ClearSelected()
        txtMinCapacity.Text = "Min"
        txtMaxCapacity.Text = "Max"
        txtMinPrice.Text = "Min"
        txtMaxPrice.Text = "Max"
        cbSort.SelectedIndex = -1
    End Sub

    Private Sub btnClearFilters_Click(sender As Object, e As EventArgs) Handles btnClearFilters.Click
        ClearFilters()
        For i As Integer = 0 To clbEventType.Items.Count - 1
            clbEventType.SetItemChecked(i, False)
        Next
        For i As Integer = 0 To clbAvailableOn.Items.Count - 1
            clbAvailableOn.SetItemChecked(i, False)
        Next

        LoadSearchResults()

    End Sub

    Public Sub DisplayAdminUsername()

        Dim query As String = "SELECT username FROM Admins WHERE admin_id=@adminId"
        Dim parameters As New Dictionary(Of String, Object) From {
    {"@adminId", CurrentUser.UserID} ' Ensure this holds the correct admin ID
}

        Dim dt As DataTable = DBHelper.GetDataTable(query, parameters)

        If dt.Rows.Count > 0 Then
            lblUsername.Text = dt.Rows(0)("username").ToString() ' Update Admin Name label dynamically
        End If
    End Sub


    Private Sub PopulateComboBoxes()
        cbSort.Items.Clear()
        cbSort.Items.Add("Select...")
        cbSort.Items.AddRange(New String() {
        "Alphabetical (A-Z)", "Alphabetical (Z-A)",
        "Capacity (Lowest to Highest)", "Capacity (Highest to Lowest)",
        "Price (Lowest to Highest)", "Price (Highest to Lowest)"
    })

        cbSort.SelectedIndex = 0
    End Sub


    Private Sub cbSort_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbSort.SelectedIndexChanged
        If cbSort.SelectedItem IsNot Nothing AndAlso cbSort.SelectedItem.ToString() <> "Select..." Then
            LoadSearchResults()
            Me.Refresh()
        End If
    End Sub

    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        HelperNavigation.GoBack(Me)
    End Sub

    Private Sub btnNext_Click(sender As Object, e As EventArgs) Handles btnNext.Click
        HelperNavigation.GoNext(Me)
    End Sub

End Class
