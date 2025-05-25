Imports System.Globalization

Public Class HelperValidation

    ' Set initial labels (removes asterisk)
    Public Shared Sub ApplyFieldIndicators(labelControls As Label(), labelTexts As String())
        For i As Integer = 0 To labelControls.Length - 1
            labelControls(i).Text = labelTexts(i)
        Next
    End Sub

    ' Show asterisk if field is empty
    Public Shared Sub ShowAsteriskOnMissedFields(fieldControls As TextBox(), labelControls As Label(), labelTexts As String())
        For i As Integer = 0 To fieldControls.Length - 1
            If String.IsNullOrWhiteSpace(fieldControls(i).Text) Then
                labelControls(i).Text = $"{labelTexts(i)} *"
            End If
        Next
    End Sub

    ' Remove asterisk when input is detected
    Public Shared Sub RemoveAsteriskOnInput(sender As Object, labelControls As Label(), labelTexts As String())
        Dim txtBox As TextBox = TryCast(sender, TextBox)
        If txtBox IsNot Nothing Then
            Dim index As Integer = Array.IndexOf(labelControls.Select(Function(lbl) lbl.Name).ToArray(), txtBox.Tag?.ToString())
            If index >= 0 Then labelControls(index).Text = labelTexts(index)
        End If
    End Sub

    ' Real-time validation for fields
    Public Shared Sub ValidateFieldsInRealTime(fieldControls As TextBox(), labelControls As Label(), labelTexts As String())
        For i As Integer = 0 To fieldControls.Length - 1
            labelControls(i).Text = If(String.IsNullOrWhiteSpace(fieldControls(i).Text), $"{labelTexts(i)} *", labelTexts(i))
        Next
    End Sub
    ' ------------------ Helper Method for Validation Messages ------------------
    Private Shared Sub ShowValidationError(targetControl As Control, message As String)
        If targetControl IsNot Nothing AndAlso TypeOf targetControl.Tag Is Label Then
            Dim errorLabel As Label = CType(targetControl.Tag, Label)
            errorLabel.Text = message
            errorLabel.ForeColor = Color.Red
            errorLabel.Visible = True
        Else
            targetControl.BackColor = Color.LightPink
        End If
    End Sub

    Public Shared Sub HideValidationError(targetControl As Control)
        If targetControl IsNot Nothing AndAlso TypeOf targetControl.Tag Is Label Then
            Dim errorLabel As Label = CType(targetControl.Tag, Label)
            errorLabel.Text = ""
            errorLabel.Visible = False
        Else
            targetControl.BackColor = Color.White
        End If
    End Sub

    ' ------------------ Validate Numeric Fields ------------------
    Public Shared Function IsValidNumericField(txtControl As TextBox, errorLabel As Label, errorMessage As String) As Boolean
        If String.IsNullOrWhiteSpace(txtControl.Text) OrElse Not IsNumeric(txtControl.Text) Then
            ShowValidationError(txtControl, errorMessage)
            Return False
        End If

        HideValidationError(txtControl)
        Return True
    End Function

    ' ------------------ Validate Date Selection ------------------
    Public Shared Function IsValidDateSelection(eventStartDate As DateTimePicker, eventEndDate As DateTimePicker) As Boolean
        If eventStartDate.Value.Date < Date.Today Then
            ShowValidationError(eventStartDate, "Event start date cannot be in the past.")
            Return False
        End If

        If eventEndDate.Value.Date < eventStartDate.Value.Date Then
            ShowValidationError(eventEndDate, "Event end date must be after the start date.")
            Return False
        End If

        Return True
    End Function

    ' ------------------ Validate Customer Information ------------------
    Public Shared Function IsValidCustomerInfo(txtName As TextBox, dtpBirthday As DateTimePicker, cmbSex As ComboBox, txtAddress As TextBox) As Boolean
        If String.IsNullOrWhiteSpace(txtName.Text) Then
            ShowValidationError(txtName, "Customer name is required.")
            Return False
        End If

        If dtpBirthday.Value.Date > Date.Today Then
            ShowValidationError(dtpBirthday, "Birthday cannot be in the future.")
            Return False
        End If

        If String.IsNullOrWhiteSpace(cmbSex.Text) Then
            ShowValidationError(cmbSex, "Please select a gender.")
            Return False
        End If

        If String.IsNullOrWhiteSpace(txtAddress.Text) Then
            ShowValidationError(txtAddress, "Address is required.")
            Return False
        End If

        Return True
    End Function

    ' ------------------ Validate Numeric Input in KeyPress ------------------
    Public Shared Sub NumericOnly_KeyPress(sender As Object, e As KeyPressEventArgs)
        If Not Char.IsControl(e.KeyChar) AndAlso Not Char.IsDigit(e.KeyChar) Then
            e.Handled = True
        End If
    End Sub

    ' ------------------ Validate Time Selection ------------------
    Public Shared Function IsValidTimeSelection(cbStartHour As ComboBox, cbStartMinutes As ComboBox, cbStartAMPM As ComboBox,
                                            cbEndHour As ComboBox, cbEndMinutes As ComboBox, cbEndAMPM As ComboBox,
                                            openingHours As String, closingHours As String) As Boolean
        Dim eventStartTime As DateTime
        Dim eventEndTime As DateTime
        Dim openingTime As DateTime = DateTime.Parse(openingHours)
        Dim closingTime As DateTime = DateTime.Parse(closingHours)

        If Not DateTime.TryParseExact($"{cbStartHour.Text}:{cbStartMinutes.Text} {cbStartAMPM.Text}", "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, eventStartTime) OrElse
       Not DateTime.TryParseExact($"{cbEndHour.Text}:{cbEndMinutes.Text} {cbEndAMPM.Text}", "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, eventEndTime) Then
            ShowValidationError(cbEndHour, "Invalid time format. Use HH:MM AM/PM.")
            Return False
        End If

        If eventEndTime < eventStartTime Then
            ShowValidationError(cbEndHour, "End time must be later than start time.")
            Return False
        End If

        If eventStartTime < openingTime Then
            ShowValidationError(cbStartHour, $"Opening hours start at {openingTime.ToString("h:mm tt")}. Adjust your start time.")
            Return False
        End If

        If eventEndTime > closingTime Then
            ShowValidationError(cbEndHour, $"Closing hours end at {closingTime.ToString("h:mm tt")}. Adjust your end time.")
            Return False
        End If

        Return True
    End Function


    ' ------------------ Check for Booking Date Conflicts ------------------
    Public Shared Function IsDateConflict(placeId As Integer, eventStart As Date, eventEnd As Date, dtpEventDateStart As DateTimePicker) As Boolean
        Dim query As String = "SELECT event_date FROM Bookings WHERE place_id = @PlaceId AND (event_date BETWEEN @EventDateStart AND @EventDateEnd)"
        Dim params As New Dictionary(Of String, Object) From {
        {"@PlaceId", placeId},
        {"@EventDateStart", eventStart},
        {"@EventDateEnd", eventEnd}
    }

        Dim conflictDates As DataTable = DBHelper.GetDataTable(query, params)
        If conflictDates.Rows.Count > 0 Then
            ShowValidationError(dtpEventDateStart, "This date range is unavailable. Please select another date.")
            Return True
        End If

        Return False
    End Function

    ' ------------------ Validate Booking Details with Tab Selection Logic ------------------
    Public Shared Sub ValidateBooking(e As TabControlCancelEventArgs, tcDetails As TabControl, tpCustomerDetails As TabPage, tpPaymentDetails As TabPage,
                                      cbEventType As ComboBox, txtNumGuests As TextBox, dtpEventDateStart As DateTimePicker, dtpEventDateEnd As DateTimePicker,
                                      cbStartHour As ComboBox, cbStartMinutes As ComboBox, cbStartAMPM As ComboBox,
                                      cbEndHour As ComboBox, cbEndMinutes As ComboBox, cbEndAMPM As ComboBox,
                                      chkOutsideAvailableHours As CheckBox, txtCustomerName As TextBox, dtpBirthday As DateTimePicker, cmbSex As ComboBox,
                                      txtAddress As TextBox, openingHours As String, closingHours As String, placeId As Integer)

        If Not IsValidTimeSelection(cbStartHour, cbStartMinutes, cbStartAMPM, cbEndHour, cbEndMinutes, cbEndAMPM, openingHours, closingHours) Then
            e.Cancel = True
            Exit Sub
        End If

        If e.TabPage Is tpCustomerDetails Then
            If String.IsNullOrWhiteSpace(cbEventType.Text) OrElse String.IsNullOrWhiteSpace(txtNumGuests.Text) OrElse Not IsNumeric(txtNumGuests.Text) OrElse
               dtpEventDateStart.Value.Date < Date.Today OrElse dtpEventDateEnd.Value.Date < dtpEventDateStart.Value.Date Then
                ShowValidationError(txtNumGuests, "Please complete all fields before proceeding.")
                e.Cancel = True
                Exit Sub
            End If

            Dim eventStartTime As DateTime = DateTime.ParseExact($"{cbStartHour.Text}:{cbStartMinutes.Text} {cbStartAMPM.Text}", "h:mm tt", CultureInfo.InvariantCulture)
            Dim eventEndTime As DateTime = DateTime.ParseExact($"{cbEndHour.Text}:{cbEndMinutes.Text} {cbEndAMPM.Text}", "h:mm tt", CultureInfo.InvariantCulture)
            Dim openingTime As DateTime = DateTime.Parse(openingHours)
            Dim closingTime As DateTime = DateTime.Parse(closingHours)

            If closingTime < openingTime Then closingTime = closingTime.AddDays(1)
            If eventEndTime < eventStartTime Then eventEndTime = eventEndTime.AddDays(1)

            If (eventStartTime < openingTime OrElse eventEndTime > closingTime) AndAlso Not chkOutsideAvailableHours.Checked Then
                ShowValidationError(chkOutsideAvailableHours, "Your selected time is outside available hours. Adjust your schedule or check 'Book outside available hours' to proceed.")
                e.Cancel = True
                Exit Sub
            End If
        End If

        If e.TabPage Is tpPaymentDetails Then
            If Not IsValidCustomerInfo(txtCustomerName, dtpBirthday, cmbSex, txtAddress) Then
                e.Cancel = True
                Exit Sub
            End If
        End If
    End Sub

    Public Shared Function FormatTime(inputTime As String) As String
        Try
            Dim formattedTime As DateTime = DateTime.ParseExact(inputTime, "hh:mm tt", CultureInfo.InvariantCulture)
            Return formattedTime.ToString("HH:mm:ss")
        Catch ex As FormatException
            Return String.Empty
        End Try
    End Function

    Public Shared Function ValidateOpeningClosingHours(openingTime As String, closingTime As String) As Boolean
        Dim openingHours As String = FormatTime(openingTime)
        Dim closingHours As String = FormatTime(closingTime)

        If String.IsNullOrEmpty(openingHours) OrElse String.IsNullOrEmpty(closingHours) Then
            MessageBox.Show("Invalid time format. Ensure both opening and closing hours are set correctly.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        Dim open As DateTime = DateTime.ParseExact(openingHours, "HH:mm:ss", CultureInfo.InvariantCulture)
        Dim close As DateTime = DateTime.ParseExact(closingHours, "HH:mm:ss", CultureInfo.InvariantCulture)

        If close <= open Then
            MessageBox.Show("Closing time must be later than opening time.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End If

        Return True
    End Function


End Class
