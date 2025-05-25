Imports System.Globalization

Public Class HelperValidation
    ' ------------------ Validate Date Selection ------------------
    Public Shared Function IsValidDateSelection(eventStartDate As DateTimePicker, eventEndDate As DateTimePicker) As Boolean
        If eventStartDate.Value.Date < Date.Today Then
            MessageBox.Show("Event start date cannot be in the past.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        If eventEndDate.Value.Date < eventStartDate.Value.Date Then
            MessageBox.Show("Event end date must be after the start date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        Return True
    End Function

    ' ------------------ Validate Customer Information ------------------
    Public Shared Function IsValidCustomerInfo(txtName As TextBox, dtpBirthday As DateTimePicker, cmbSex As ComboBox, txtAddress As TextBox) As Boolean
        If String.IsNullOrWhiteSpace(txtName.Text) Then
            MessageBox.Show("Customer name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        If dtpBirthday.Value.Date > Date.Today Then
            MessageBox.Show("Birthday cannot be in the future.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        If String.IsNullOrWhiteSpace(cmbSex.Text) Then
            MessageBox.Show("Please select a gender.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        If String.IsNullOrWhiteSpace(txtAddress.Text) Then
            MessageBox.Show("Address is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        Return True
    End Function

    ' ------------------ Validate Time Selection ------------------
    Public Shared Function IsValidTimeSelection(cbStartHour As ComboBox, cbStartMinutes As ComboBox, cbStartAMPM As ComboBox,
                                                cbEndHour As ComboBox, cbEndMinutes As ComboBox, cbEndAMPM As ComboBox,
                                                openingHours As String, closingHours As String) As Boolean
        Dim eventStartTime As DateTime
        Dim eventEndTime As DateTime
        Dim openingTime As DateTime
        Dim closingTime As DateTime

        ' Attempt to parse times
        If Not DateTime.TryParse($"{cbStartHour.Text}:{cbStartMinutes.Text} {cbStartAMPM.Text}", eventStartTime) OrElse
           Not DateTime.TryParse($"{cbEndHour.Text}:{cbEndMinutes.Text} {cbEndAMPM.Text}", eventEndTime) OrElse
           Not DateTime.TryParse(openingHours, openingTime) OrElse
           Not DateTime.TryParse(closingHours, closingTime) Then
            MessageBox.Show("Invalid time format. Please select a valid time.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        ' Adjust closing time to handle overnight scenarios
        If closingTime < openingTime Then closingTime = closingTime.AddDays(1)
        If eventEndTime < eventStartTime Then eventEndTime = eventEndTime.AddDays(1)

        Return True
    End Function

    ' ------------------ Validate Booking Details (Merged from BookingValidationHelper) ------------------
    Public Shared Sub ValidateBooking(cbEventType As ComboBox, dtpEventDateStart As DateTimePicker, dtpEventDateEnd As DateTimePicker,
                                      cbStartHour As ComboBox, cbStartMinutes As ComboBox, cbStartAMPM As ComboBox,
                                      cbEndHour As ComboBox, cbEndMinutes As ComboBox, cbEndAMPM As ComboBox,
                                      chkOutsideAvailableHours As CheckBox, lblTotalPricePaymentContainer As Label, tcDetails As TabControl, placeId As Integer)

        If String.IsNullOrWhiteSpace(cbEventType.Text) Then
            MessageBox.Show("Please select an event type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        If dtpEventDateEnd.Value.Date < dtpEventDateStart.Value.Date Then
            MessageBox.Show("Event end date must be after the start date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        Dim checkQuery As String = "SELECT COUNT(*) FROM Bookings WHERE place_id = @PlaceId AND (event_date BETWEEN @EventDateStart AND @EventDateEnd)"
        Dim checkParams As New Dictionary(Of String, Object) From {
            {"@PlaceId", placeId},
            {"@EventDateStart", dtpEventDateStart.Value.Date},
            {"@EventDateEnd", dtpEventDateEnd.Value.Date}
        }

        Dim existingBookings As Integer = Convert.ToInt32(DBHelper.ExecuteScalarQuery(checkQuery, checkParams))

        If existingBookings > 0 Then
            MessageBox.Show("This event place is already booked during your selected date range. Please choose a different date or venue.",
                            "Booking Conflict", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Exit Sub
        End If

        tcDetails.SelectedTab = tcDetails.TabPages("tpCustomerDetails")
    End Sub
End Class
