Imports MySql.Data.MySqlClient
Imports System.Globalization
Imports System.Text
Imports System.IO

Public Class FormBooking
    ' ------------------ Properties ------------------
    Public Property PlaceId As Integer
    Public Property EventPlaceName As String
    Public Property EventPlaceCapacity As Integer
    Public Property BasePricePerDay As Decimal
    Public Property EventPlaceFeatures As String
    Public Property EventVenueType As String
    Public Property OpeningHours As String
    Public Property ClosingHours As String
    Public Property AvailableDays As String
    Public Property EventPlaceDescription As String
    Public Property EventTime As String
    Public Property VoucherDiscount As Decimal = 0
    Public Property EventPlaceImageUrl As String

    Private _customerId As Integer
    Public Property FirstName As String
    Public Property LastName As String
    Dim bookedDates As New List(Of Date)

    ' ------------------ Constructor ------------------
    Public Sub New(customerId As Integer, placeId As Integer)
        InitializeComponent()
        _customerId = customerId
        Me.PlaceId = placeId
    End Sub

    ' ------------------ Form Load ------------------
    Private Sub FormBooking_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lblEventPlace.Text = EventPlaceName
        lblPlaceIDContainer.Text = PlaceId.ToString()
        lblCapacityContainer.Text = EventPlaceCapacity.ToString()
        lblPricePerDayContainer.Text = "₱" & BasePricePerDay.ToString("F2")
        lblFeaturesContainer.Text = EventPlaceFeatures
        lblDescriptionContainer.AutoSize = False
        lblDescriptionContainer.Size = New Size(Panel1.Width - 20, 0)
        lblDescriptionContainer.MaximumSize = New Size(Panel1.Width - 20, 0)
        lblDescriptionContainer.TextAlign = ContentAlignment.TopLeft

        lblHoursContainer.Text = If(String.IsNullOrWhiteSpace(OpeningHours) OrElse String.IsNullOrWhiteSpace(ClosingHours),
                    "N/A",
                    $"{DateTime.Parse(OpeningHours):hh:mm tt} - {DateTime.Parse(ClosingHours):hh:mm tt}")

        lblAvailableDaysContainer.Text = $"Available: {HelperEvent.FormatAvailableDays(AvailableDays)}"

        dtpEventDateStart.Value = Date.Today
        dtpEventDateStart.MinDate = Date.Today
        dtpEventDateEnd.MinDate = Date.Today
        cbSameDayEvent.Checked = True
        dtpEventDateEnd.Value = dtpEventDateStart.Value
        dtpEventDateEnd.Enabled = False
        lblEventDatePaymentContainer.Text = dtpEventDateStart.Value.ToShortDateString()

        HelperDatabase.PopulateEventTypeCombo(EventPlaceName, cbEventType)
        bookedDates = HelperDatabase.LoadBookedDates(PlaceId)

        HelperUI.LoadEventPlaceImage(EventPlaceImageUrl, pb)
        Dim customerData As DataTable = HelperDatabase.GetCustomerData(CurrentUser.UserID)

        If customerData.Rows.Count > 0 Then
            txtCustomerName.Text = customerData.Rows(0)("name").ToString()
            dtpBirthday.Value = Convert.ToDateTime(customerData.Rows(0)("birthday"))
            cmbSex.Text = customerData.Rows(0)("sex").ToString()
            txtAddress.Text = customerData.Rows(0)("address").ToString()
        Else
            MessageBox.Show("Customer information not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    Private Sub btnBookingProceed_Click(sender As Object, e As EventArgs)
        HelperValidation.ValidateBooking(cbEventType, dtpEventDateStart, dtpEventDateEnd, cbStartHour, cbStartMinutes, cbStartAMPM,
                                         cbEndHour, cbEndMinutes, cbEndAMPM, chkOutsideAvailableHours, lblTotalPricePaymentContainer, tcDetails, PlaceId)
    End Sub


    Private Sub btnCustomerProceed_Click(sender As Object, e As EventArgs) Handles btnCustomerProceed.Click
        If Not HelperValidation.IsValidCustomerInfo(txtCustomerName, dtpBirthday, cmbSex, txtAddress) Then Exit Sub
        PopulatePaymentDetails()
        tcDetails.SelectedTab = tpPaymentDetails
    End Sub

    Private Sub btnPlaceBooking_Click(sender As Object, e As EventArgs) Handles btnPlaceBooking.Click
        Dim numGuests As Integer
        If Not Integer.TryParse(txtNumGuests.Text, numGuests) Then numGuests = 0

        Dim finalTotalPrice As Decimal = HelperPrice.ComputeFinalPrice(numGuests, EventPlaceCapacity, BasePricePerDay,
                                                                 dtpEventDateStart, dtpEventDateEnd, VoucherDiscount,
                                                                 chkOutsideAvailableHours, cbStartHour, cbStartMinutes, cbStartAMPM,
                                                                 cbEndHour, cbEndMinutes, cbEndAMPM, OpeningHours, ClosingHours,
                                                                 chkCatering, chkClown, chkSinger, chkDancer, chkVideoke)


        Dim bookingId As Integer = HelperDatabase.PlaceBooking(CurrentUser.CustomerId, PlaceId, txtNumGuests.Text,
                                                               dtpEventDateStart.Value.Date, cbStartHour.Text, cbEndHour.Text, finalTotalPrice)

        If bookingId > 0 Then
            HelperDatabase.SaveBookingServices(bookingId, chkCatering, chkClown, chkSinger, chkDancer, chkVideoke)
            HelperDatabase.InsertPaymentRecord(bookingId, CurrentUser.CustomerId, finalTotalPrice)

            MessageBox.Show("Booking and payment recorded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            MessageBox.Show("Booking failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If

    End Sub

    Private Sub PopulatePaymentDetails()
        lblCustomerContainer.Text = txtCustomerName.Text
        lblEventPlacePaymentContainer.Text = lblEventPlace.Text
        lblEventTypePaymentContainer.Text = cbEventType.Text
        lblNumGuestsPaymentContainer.Text = txtNumGuests.Text

        lblEventDatePaymentContainer.Text = If(cbSameDayEvent.Checked, dtpEventDateStart.Value.ToShortDateString(),
                                               $"{dtpEventDateStart.Value.ToShortDateString()} - {dtpEventDateEnd.Value.ToShortDateString()}")

        lblEventTimePaymentContainer.Text = $"{cbStartHour.Text}:{cbStartMinutes.Text} {cbStartAMPM.Text} - {cbEndHour.Text}:{cbEndMinutes.Text} {cbEndAMPM.Text}"

        Dim numGuests As Integer
        If Not Integer.TryParse(txtNumGuests.Text, numGuests) Then numGuests = 0

        txtTotalPrice.Text = "₱" & HelperPrice.ComputeFinalPrice(numGuests, EventPlaceCapacity, BasePricePerDay,
                                                           dtpEventDateStart, dtpEventDateEnd, VoucherDiscount,
                                                           chkOutsideAvailableHours, cbStartHour, cbStartMinutes, cbStartAMPM,
                                                           cbEndHour, cbEndMinutes, cbEndAMPM, OpeningHours, ClosingHours,
                                                           chkCatering, chkClown, chkSinger, chkDancer, chkVideoke).ToString("F2")


        lblTotalPricePaymentContainer.Text = txtTotalPrice.Text
    End Sub

    Private Sub tcDetails_Selecting(sender As Object, e As TabControlCancelEventArgs) Handles tcDetails.Selecting
        HelperUI.ValidateTabSelection(e, tcDetails, tpCustomerDetails, tpPaymentDetails, cbEventType, txtNumGuests, dtpEventDateStart, dtpEventDateEnd, cbStartHour,
                                      cbStartMinutes, cbStartAMPM, cbEndHour, cbEndMinutes, cbEndAMPM, chkOutsideAvailableHours, txtCustomerName, dtpBirthday, cmbSex, txtAddress,
                                      OpeningHours, ClosingHours)
    End Sub

    Private Sub cbEndHour_SelectedIndexChanged(sender As Object, e As EventArgs)
        HelperEvent.HandleEndHourChange(cbStartHour, cbStartMinutes, cbStartAMPM, cbEndHour, cbEndMinutes, cbEndAMPM, dtpEventDateStart, dtpEventDateEnd, cbSameDayEvent)
    End Sub

    Private Sub dtpEventDateStart_ValueChanged(sender As Object, e As EventArgs)
        HelperEvent.HandleEventStartDateChange(dtpEventDateStart, dtpEventDateEnd, cbSameDayEvent)
    End Sub

    Private Sub UpdateTotalPrice() Handles cbEndHour.SelectedIndexChanged, cbEndMinutes.SelectedIndexChanged, cbEndAMPM.SelectedIndexChanged,
                                    cbStartHour.SelectedIndexChanged, cbStartMinutes.SelectedIndexChanged, cbStartAMPM.SelectedIndexChanged,
                                    txtNumGuests.TextChanged, chkCatering.CheckedChanged, chkClown.CheckedChanged,
                                    chkSinger.CheckedChanged, chkDancer.CheckedChanged, chkVideoke.CheckedChanged,
                                    chkOutsideAvailableHours.CheckedChanged

        HelperPrice.UpdateTotalPrice(txtNumGuests, chkCatering, chkClown, chkSinger, chkDancer, chkVideoke, chkOutsideAvailableHours, cbStartHour,
                                   cbStartMinutes, cbStartAMPM, cbEndHour, cbEndMinutes, cbEndAMPM, OpeningHours, ClosingHours,
                                   dtpEventDateStart, dtpEventDateEnd, EventPlaceCapacity, BasePricePerDay, VoucherDiscount,
                                   lblTotalPricePaymentContainer, lblPriceBreakdown, txtTotalPrice)
    End Sub

    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        GoBack(Me)
    End Sub

    Private Sub btnNext_Click(sender As Object, e As EventArgs) Handles btnNext.Click
        GoNext(Me)
    End Sub


End Class
