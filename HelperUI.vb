Imports System.Globalization
Imports System.IO
Imports System.Drawing


Public Class HelperUI
    ' ------------------ Update Payment Details ------------------
    Public Shared Sub UpdatePaymentDetails(lblCustomerContainer As Label, lblEventPlacePaymentContainer As Label, lblEventTypePaymentContainer As Label,
                                           cbEventType As ComboBox, lblNumGuestsPaymentContainer As Label, txtNumGuests As TextBox,
                                           cbSameDayEvent As CheckBox, dtpEventDateStart As DateTimePicker, dtpEventDateEnd As DateTimePicker,
                                           lblEventDatePaymentContainer As Label, cbStartHour As ComboBox, cbStartMinutes As ComboBox, cbStartAMPM As ComboBox,
                                           cbEndHour As ComboBox, cbEndMinutes As ComboBox, cbEndAMPM As ComboBox,
                                           lblEventTimePaymentContainer As Label, lblTotalPricePaymentContainer As Label, txtTotalPrice As TextBox, txtCustomerName As TextBox)

        lblCustomerContainer.Text = txtCustomerName.Text
        lblEventPlacePaymentContainer.Text = lblEventPlacePaymentContainer.Text
        lblEventTypePaymentContainer.Text = cbEventType.Text
        lblNumGuestsPaymentContainer.Text = txtNumGuests.Text

        If cbSameDayEvent.Checked Then
            lblEventDatePaymentContainer.Text = dtpEventDateStart.Value.ToShortDateString()
        Else
            lblEventDatePaymentContainer.Text = $"{dtpEventDateStart.Value.ToShortDateString()} - {dtpEventDateEnd.Value.ToShortDateString()}"
        End If

        Dim startTime As String = $"{cbStartHour.Text}:{cbStartMinutes.Text} {cbStartAMPM.Text}"
        Dim endTime As String = $"{cbEndHour.Text}:{cbEndMinutes.Text} {cbEndAMPM.Text}"
        lblEventTimePaymentContainer.Text = $"{startTime} - {endTime}"

        lblTotalPricePaymentContainer.Text = txtTotalPrice.Text
    End Sub

    ' ------------------ Validate Tab Selection ------------------
    Public Shared Sub ValidateTabSelection(e As TabControlCancelEventArgs, tcDetails As TabControl, tpCustomerDetails As TabPage, tpPaymentDetails As TabPage,
                                           cbEventType As ComboBox, txtNumGuests As TextBox, dtpEventDateStart As DateTimePicker, dtpEventDateEnd As DateTimePicker,
                                           cbStartHour As ComboBox, cbStartMinutes As ComboBox, cbStartAMPM As ComboBox,
                                           cbEndHour As ComboBox, cbEndMinutes As ComboBox, cbEndAMPM As ComboBox,
                                           chkOutsideAvailableHours As CheckBox, txtCustomerName As TextBox, dtpBirthday As DateTimePicker, cmbSex As ComboBox,
                                           txtAddress As TextBox, openingHours As String, closingHours As String)

        Dim timeFormat As String = "hh:mm tt"
        Dim eventStartTime As DateTime
        Dim eventEndTime As DateTime
        Dim openingTime As DateTime
        Dim closingTime As DateTime

        If Not DateTime.TryParseExact($"{cbStartHour.Text}:{cbStartMinutes.Text} {cbStartAMPM.Text}", timeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, eventStartTime) OrElse
           Not DateTime.TryParseExact($"{cbEndHour.Text}:{cbEndMinutes.Text} {cbEndAMPM.Text}", timeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, eventEndTime) OrElse
           Not DateTime.TryParse(openingHours, openingTime) OrElse
           Not DateTime.TryParse(closingHours, closingTime) Then
            MessageBox.Show("Invalid time format. Please select a valid time.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            e.Cancel = True
            Exit Sub
        End If

        If closingTime < openingTime Then closingTime = closingTime.AddDays(1)
        If eventEndTime < eventStartTime Then eventEndTime = eventEndTime.AddDays(1)

        If e.TabPage Is tpCustomerDetails Then
            If String.IsNullOrWhiteSpace(cbEventType.Text) OrElse String.IsNullOrWhiteSpace(txtNumGuests.Text) OrElse Not IsNumeric(txtNumGuests.Text) OrElse
               dtpEventDateStart.Value.Date < Date.Today OrElse dtpEventDateEnd.Value.Date < dtpEventDateStart.Value.Date OrElse
               String.IsNullOrWhiteSpace(cbStartHour.Text) OrElse String.IsNullOrWhiteSpace(cbStartMinutes.Text) OrElse String.IsNullOrWhiteSpace(cbStartAMPM.Text) OrElse
               String.IsNullOrWhiteSpace(cbEndHour.Text) OrElse String.IsNullOrWhiteSpace(cbEndMinutes.Text) OrElse String.IsNullOrWhiteSpace(cbEndAMPM.Text) Then
                MessageBox.Show("Please complete all fields before proceeding.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                e.Cancel = True
                Exit Sub
            End If

            If (eventStartTime < openingTime OrElse eventEndTime > closingTime) AndAlso Not chkOutsideAvailableHours.Checked Then
                MessageBox.Show("Your selected time is outside the venue's available hours. Please adjust your schedule or check 'Book outside available hours' to proceed.",
                                "Time Restriction", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                e.Cancel = True
                Exit Sub
            End If
        End If

        If e.TabPage Is tpPaymentDetails Then
            If String.IsNullOrWhiteSpace(txtCustomerName.Text) OrElse dtpBirthday.Value.Date > Date.Today OrElse
               String.IsNullOrWhiteSpace(cmbSex.Text) OrElse String.IsNullOrWhiteSpace(txtAddress.Text) Then
                MessageBox.Show("Please complete all fields before proceeding.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                e.Cancel = True
                Exit Sub
            End If
        End If
    End Sub
    Public Shared Sub LoadEventPlaceImage(imgPath As String, pb As PictureBox)
        pb.SizeMode = PictureBoxSizeMode.StretchImage
        Dim defaultImagePath As String = "C:\event images\No Image.png"
        Dim fullImagePath As String = If(String.IsNullOrWhiteSpace(imgPath), defaultImagePath, imgPath.Trim())

        Try
            If File.Exists(fullImagePath) Then
                pb.Image = Image.FromFile(fullImagePath)
            Else
                MessageBox.Show("File not found: " & fullImagePath & ". Using fallback image.")
                pb.Image = Image.FromFile(defaultImagePath)
            End If
        Catch ex As Exception
            MessageBox.Show("Error loading image: " & ex.Message, "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            pb.Image = Nothing
        End Try
    End Sub

End Class
