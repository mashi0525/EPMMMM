Imports MySql.Data.MySqlClient
Imports System.Data
Imports System.Globalization
Imports System.IO
Imports System.Windows.Forms.DataVisualization.Charting

Public Class FormAdminCenter

    Private Sub FormAdminCenter_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HelperNavigation.RegisterNewForm(Me)

        lblUsername.Text = CurrentUser.Username

        ' Load all datasets into FlowLayoutPanels and Chart:
        LoadSearchResults()         ' Event Places (with update/delete)
        LoadPendingBookings()         ' Pending Bookings (with Approve/Reject)
        LoadAvailability()            ' Availability of Event Places
        LoadRevenueReports()          ' Revenue per Event Place
        LoadInvoices()                ' Invoices with Accept Payment
        LoadBookedDates()             ' Booked Dates
        LoadCustomerCount()           ' Customer Count
        LoadCustomerRecords()         ' Customer Records
        LoadBookingStatusChart()      ' Booking Status Chart

        ' Set up field indicators and validation for event place data entry.
        Dim labels = {lblEventPlace, lblEventType, lblCapacity, lblPricePerDay, lblFeatures, lblImageUrl, lblOpeningHours, lblClosingHours, lblAvailableDays, lblDescription}
        Dim fields = {txtEventPlace, txtEventType, txtCapacity, txtPricePerDay, txtFeatures, txtImageUrl, txtOpeningHours, txtClosingHours, txtAvailableDays, txtDescription}
        Dim texts = {"Event Place", "Event Type", "Capacity", "Price per Day", "Features", "Image URL", "Opening Hours", "Closing Hours", "Available Days", "Description"}

        HelperValidation.ApplyFieldIndicators(labels, texts)

        For Each field In fields
            AddHandler field.TextChanged, Sub(senderObj, args) HelperValidation.ValidateFieldsInRealTime(fields, labels, texts)
            AddHandler field.Leave, Sub(senderObj, args) HelperValidation.RemoveAsteriskOnInput(senderObj, labels, texts)
        Next
    End Sub

#Region "Data Loading using HelperResultsDisplay (FlowLayoutPanels)"

    '--- Load Event Places (Search Results)
    Private Sub LoadSearchResults()
        Dim query As String = "SELECT place_id, event_place, event_type, capacity, price_per_day, description, image_url, " &
                              "CASE WHEN EXISTS (SELECT 1 FROM bookings WHERE bookings.place_id = eventplace.place_id) " &
                              "THEN 'Booked' ELSE 'Available' END AS status " &
                              "FROM eventplace WHERE 1=1"
        Dim dt As New DataTable()
        Using connection As MySqlConnection = DBHelper.GetConnection()
            Using cmd As New MySqlCommand(query, connection)
                Dim adapter As New MySqlDataAdapter(cmd)
                Try
                    connection.Open()
                    dt.Clear()
                    adapter.Fill(dt)
                Catch ex As Exception
                    MessageBox.Show("Error loading event places: " & ex.Message)
                End Try
            End Using
        End Using
        For Each col As DataColumn In dt.Columns
            Debug.Print("Column: " & col.ColumnName)
        Next

        Dim currentRoleIsAdmin As Boolean = CurrentUser.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
        HelperResultsDisplay.PopulateEventPlaces(flpEventPlaces, dt, Nothing, AddressOf btnUpdate_Click, AddressOf btnDelete_Click, currentRoleIsAdmin)

    End Sub

    '--- Load Pending Bookings
    Private Sub LoadPendingBookings()
        Dim query As String = "SELECT b.booking_id, c.name, e.event_place, b.event_date, b.total_price, b.status " &
                              "FROM bookings b " &
                              "JOIN customers c ON b.customer_id = c.customer_id " &
                              "JOIN eventplace e ON b.place_id = e.place_id " &
                              "WHERE b.status='Pending' ORDER BY b.event_date ASC"
        Dim dt As DataTable = DBHelper.GetDataTable(query, New Dictionary(Of String, Object))
        HelperResultsDisplay.PopulatePendingBookings(flpPendingBookings, dt, AddressOf ApproveBooking_Click, AddressOf RejectBooking_Click)
    End Sub

    '--- Load Availability of Event Places
    Private Sub LoadAvailability()
        Dim query As String = "SELECT e.event_place, " &
                              "CASE WHEN EXISTS (SELECT 1 FROM bookings b WHERE b.place_id = e.place_id AND b.status='Approved') " &
                              "THEN 'Booked' ELSE 'Available' END AS Availability " &
                              "FROM eventplace e"
        Dim dt As DataTable = DBHelper.GetDataTable(query, New Dictionary(Of String, Object))
        HelperResultsDisplay.PopulateAvailability(flpAvailability, dt)
    End Sub

    '--- Load Revenue Reports
    Private Sub LoadRevenueReports()
        Dim query As String = "SELECT e.event_place, SUM(b.total_price) AS total_revenue " &
                              "FROM eventplace e " &
                              "JOIN bookings b ON e.place_id = b.place_id " &
                              "WHERE b.status='Approved' " &
                              "GROUP BY e.place_id"
        Dim dt As DataTable = DBHelper.GetDataTable(query, New Dictionary(Of String, Object))
        HelperResultsDisplay.PopulateRevenueReports(flpRevenueReports, dt)
    End Sub

    '--- Load Invoices
    Private Sub LoadInvoices()
        Dim query As String = "SELECT invoice_id, user_id, event_place, total_amount, payment_status, invoice_data " &
                              "FROM invoices WHERE payment_status='Pending' ORDER BY invoice_data ASC"
        Dim dt As DataTable = DBHelper.GetDataTable(query, New Dictionary(Of String, Object))
        HelperResultsDisplay.PopulateInvoices(flpInvoices, dt, AddressOf AcceptPayment_Click)
    End Sub

    '--- Load Booked Dates
    Private Sub LoadBookedDates()
        Dim query As String = "SELECT DISTINCT event_date FROM bookings WHERE status='Approved'"
        Dim dt As DataTable = DBHelper.GetDataTable(query, New Dictionary(Of String, Object))
        HelperResultsDisplay.PopulateBookedDates(flpBookedDates, dt)
    End Sub

    '--- Load Customer Count
    Private Sub LoadCustomerCount()
        Dim query As String = "SELECT COUNT(*) FROM customers"
        Dim count As Object = DBHelper.ExecuteScalarQuery(query, New Dictionary(Of String, Object))
        lblNumCustomersContainer.Text = If(count IsNot Nothing, count.ToString(), "0")
    End Sub

    '--- Load Customer Records
    Private Sub LoadCustomerRecords()
        Dim query As String = "SELECT customer_id, name, age, birthday, sex, address FROM customers ORDER BY name ASC"
        Dim dt As DataTable = DBHelper.GetDataTable(query, New Dictionary(Of String, Object))
        HelperResultsDisplay.PopulateCustomerRecords(flpCustomerRecords, dt)
    End Sub

    '--- Load Booking Status Chart
    Private Sub LoadBookingStatusChart()
        chartTotalStatus.Series.Clear()
        Dim statusSeries As New Series("BookingStatus") With {
            .ChartType = SeriesChartType.Bar,
            .LabelFormat = "0"
        }
        chartTotalStatus.Series.Add(statusSeries)
        Dim query As String = "SELECT status, COUNT(*) AS count FROM bookings GROUP BY status"
        Dim dt As DataTable = DBHelper.GetDataTable(query, New Dictionary(Of String, Object))
        For Each row As DataRow In dt.Rows
            statusSeries.Points.AddXY(row("status").ToString(), Convert.ToInt32(row("count")))
        Next
        chartTotalStatus.ChartAreas(0).AxisY.Interval = 1
        chartTotalStatus.ChartAreas(0).AxisY.LabelStyle.Format = "0"
    End Sub

#End Region

#Region "Event Handlers for Booking Approval/Payment"

    '--- Approve individual booking
    Private Sub ApproveBooking_Click(sender As Object, e As EventArgs)
        Dim btn As Button = DirectCast(sender, Button)
        Dim row As DataRow = DirectCast(btn.Tag, DataRow)
        Dim bookingId As Object = row("booking_id")
        Dim rowsAffected As Integer = DBHelper.ExecuteQuery("UPDATE bookings SET status='Approved' WHERE booking_id=@id",
                                                              New Dictionary(Of String, Object) From {{"@id", bookingId}})
        If rowsAffected > 0 Then
            MessageBox.Show("Booking approved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            MessageBox.Show("Failed to approve booking.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
        LoadPendingBookings()
        LoadBookingStatusChart()
        LoadAvailability()
    End Sub

    '--- Reject individual booking
    Private Sub RejectBooking_Click(sender As Object, e As EventArgs)
        Dim btn As Button = DirectCast(sender, Button)
        Dim row As DataRow = DirectCast(btn.Tag, DataRow)
        Dim bookingId As Object = row("booking_id")
        Dim rowsAffected As Integer = DBHelper.ExecuteQuery("UPDATE bookings SET status='Rejected' WHERE booking_id=@id",
                                                              New Dictionary(Of String, Object) From {{"@id", bookingId}})
        If rowsAffected > 0 Then
            MessageBox.Show("Booking rejected successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            MessageBox.Show("Failed to reject booking.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
        LoadPendingBookings()
        LoadBookingStatusChart()
        LoadAvailability()
    End Sub

    '--- Accept payment for an invoice
    Private Sub AcceptPayment_Click(sender As Object, e As EventArgs)
        Dim btn As Button = DirectCast(sender, Button)
        Dim row As DataRow = DirectCast(btn.Tag, DataRow)
        Dim invoiceId As Object = row("invoice_id")
        Dim rowsAffected As Integer = DBHelper.ExecuteQuery("UPDATE invoices SET payment_status='Paid' WHERE invoice_id=@id",
                                                              New Dictionary(Of String, Object) From {{"@id", invoiceId}})
        If rowsAffected > 0 Then
            MessageBox.Show("Payment accepted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            MessageBox.Show("Failed to accept payment.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
        LoadInvoices()
    End Sub

#End Region

#Region "Event Place Add/Update/Delete and Field Validation"

    Private Function EventPlaceExists(eventPlaceName As String) As Boolean
        Dim result As Object = DBHelper.ExecuteScalarQuery("SELECT COUNT(*) FROM eventplace WHERE event_place=@name",
                                                            New Dictionary(Of String, Object) From {{"@name", eventPlaceName}})
        Return Convert.ToInt32(result) > 0
    End Function

    '--- Add a new event place
    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        If Not HelperValidation.ValidateOpeningClosingHours(txtOpeningHours.Text, txtClosingHours.Text) Then Exit Sub

        Dim openingHours As String = HelperValidation.FormatTime(txtOpeningHours.Text)
        Dim closingHours As String = HelperValidation.FormatTime(txtClosingHours.Text)
        If String.IsNullOrEmpty(openingHours) OrElse String.IsNullOrEmpty(closingHours) Then Exit Sub

        If EventPlaceExists(txtEventPlace.Text) Then
            MessageBox.Show("Event place already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        DBHelper.ExecuteQuery("INSERT INTO eventplace (event_place, event_type, capacity, features, price_per_day, description, image_url, opening_hours, closing_hours, available_days) " &
                              "VALUES (@event_place, @event_type, @capacity, @features, @price_per_day, @description, @image_url, @opening_hours, @closing_hours, @available_days)",
                              New Dictionary(Of String, Object) From {
                                  {"@event_place", txtEventPlace.Text},
                                  {"@event_type", txtEventType.Text},
                                  {"@capacity", If(Not IsNumeric(txtCapacity.Text), DBNull.Value, txtCapacity.Text)},
                                  {"@features", txtFeatures.Text},
                                  {"@price_per_day", If(Not IsNumeric(txtPricePerDay.Text), DBNull.Value, txtPricePerDay.Text)},
                                  {"@description", txtDescription.Text},
                                  {"@image_url", txtImageUrl.Text},
                                  {"@opening_hours", openingHours},
                                  {"@closing_hours", closingHours},
                                  {"@available_days", txtAvailableDays.Text}
                              })
        LoadSearchResults()
    End Sub

    '--- Update the selected event place
    Private Sub btnUpdate_Click(sender As Object, e As EventArgs) Handles btnUpdate.Click
        If String.IsNullOrWhiteSpace(txtPlaceID.Text) Then
            MessageBox.Show("Please select an event place to update.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Dim openingHours As String = HelperValidation.FormatTime(txtOpeningHours.Text)
        Dim closingHours As String = HelperValidation.FormatTime(txtClosingHours.Text)
        If String.IsNullOrEmpty(openingHours) OrElse String.IsNullOrEmpty(closingHours) Then Exit Sub

        Dim query As String = "UPDATE eventplace SET event_place = @event_place, event_type = @event_type, capacity = @capacity, features = @features, price_per_day = @price_per_day, description = @description, image_url = @image_url, opening_hours = @opening_hours, closing_hours = @closing_hours, available_days = @available_days WHERE place_id = @place_id"
        Dim parameters As New Dictionary(Of String, Object) From {
            {"@place_id", txtPlaceID.Text},
            {"@event_place", txtEventPlace.Text},
            {"@event_type", txtEventType.Text},
            {"@capacity", If(Not IsNumeric(txtCapacity.Text), DBNull.Value, txtCapacity.Text)},
            {"@features", txtFeatures.Text},
            {"@price_per_day", If(Not IsNumeric(txtPricePerDay.Text), DBNull.Value, txtPricePerDay.Text)},
            {"@description", txtDescription.Text},
            {"@image_url", txtImageUrl.Text},
            {"@opening_hours", openingHours},
            {"@closing_hours", closingHours},
            {"@available_days", txtAvailableDays.Text}
        }

        Dim rowsAffected As Integer = DBHelper.ExecuteQuery(query, parameters)
        If rowsAffected > 0 Then
            MessageBox.Show("Event place updated successfully.")
            LoadSearchResults()
        Else
            MessageBox.Show("Update failed. No rows were affected. Please check your input.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    '--- Delete the selected event place (if no active bookings)
    Private Function HasActiveBookings(placeID As String) As Boolean
        Dim result As Object = DBHelper.ExecuteScalarQuery("SELECT COUNT(*) FROM bookings WHERE place_id=@id AND status='Approved'",
                                                            New Dictionary(Of String, Object) From {{"@id", placeID}})
        Return Convert.ToInt32(result) > 0
    End Function

    Private Sub btnDelete_Click(sender As Object, e As EventArgs) Handles btnDelete.Click
        If HasActiveBookings(txtPlaceID.Text) Then
            MessageBox.Show("Cannot delete event place with active bookings.", "Deletion Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        DBHelper.ExecuteQuery("DELETE FROM eventplace WHERE place_id = @place_id", New Dictionary(Of String, Object) From {{"@place_id", txtPlaceID.Text}})
        LoadSearchResults()
    End Sub

    '--- Field Validation ---
    Private Sub txtCapacity_Validating(sender As Object, e As EventArgs) Handles txtCapacity.Leave
        HelperValidation.IsValidNumericField(txtCapacity, lblErrorCapacity, "Capacity must be a number.")
    End Sub

    Private Sub txtPricePerDay_Validating(sender As Object, e As EventArgs) Handles txtPricePerDay.Leave
        HelperValidation.IsValidNumericField(txtPricePerDay, lblErrorPrice, "Price must be a number.")
    End Sub

    Private Sub txtCapacity_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtCapacity.KeyPress, txtPricePerDay.KeyPress
        HelperValidation.NumericOnly_KeyPress(sender, e)
    End Sub

    Private Sub NumericOnly_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtCapacity.KeyPress, txtPricePerDay.KeyPress
        If Not Char.IsControl(e.KeyChar) AndAlso Not Char.IsDigit(e.KeyChar) Then
            e.Handled = True
        End If
    End Sub

#End Region

    '--- Log Out ---
    Private Sub btnLogOut_Click(sender As Object, e As EventArgs) Handles btnLogOut.Click
        LogError($"User {CurrentUser.UserID} logged out.")
        CurrentUser.Username = String.Empty
        CurrentUser.UserID = 0
        CurrentUser.Email = String.Empty
        Me.Hide()
        FormLogIn.Show()
    End Sub

    '--- Additional Legend (optional)
    Private Sub AddLegend()
        Dim legend As New Label With {
            .Text = "Red = Booked | Green = Available",
            .ForeColor = Color.Black,
            .AutoSize = True,
            .Font = New Font("Arial", 10, FontStyle.Bold),
            .Location = New Point(flpAvailability.Left, flpAvailability.Bottom + 10)
        }
        Me.Controls.Add(legend)
    End Sub

    Private Sub LogError(errorMessage As String)
        Using writer As New StreamWriter("error_log.txt", True)
            writer.WriteLine($"{DateTime.Now}: {errorMessage}")
        End Using
    End Sub

    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        HelperNavigation.GoBack(Me)
    End Sub

    Private Sub btnNext_Click(sender As Object, e As EventArgs) Handles btnNext.Click
        HelperNavigation.GoNext(Me)
    End Sub

End Class
