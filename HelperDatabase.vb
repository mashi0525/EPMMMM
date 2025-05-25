Imports MySql.Data.MySqlClient

Public Class HelperDatabase
    ' ------------------ Create a New Customer ------------------
    Public Shared Function CreateNewCustomer(name As String, birthday As Date, sex As String, address As String, age As Integer) As CustomerResult
        Dim result As New CustomerResult()
        Dim insertQuery As String = "INSERT INTO Customers (name, birthday, sex, address, age) VALUES (@name, @birthday, @sex, @address, @age); SELECT LAST_INSERT_ID();"

        Dim params As New Dictionary(Of String, Object) From {
            {"@name", name},
            {"@birthday", birthday},
            {"@sex", sex},
            {"@address", address},
            {"@age", age}
        }

        Dim customerId As Object = DBHelper.ExecuteScalarQuery(insertQuery, params)

        result.CustomerId = If(customerId IsNot Nothing, Convert.ToInt32(customerId), -1)
        result.ErrorMessage = If(result.CustomerId > 0, String.Empty, "Failed to create customer")

        Return result
    End Function

    ' ------------------ Insert User-Customer Relationship ------------------
    Public Shared Sub InsertUserCustomer(userId As Integer, customerId As Integer)
        Dim query As String = "INSERT INTO UserCustomers (user_id, customer_id) VALUES (@user_id, @customer_id)"
        Dim params As New Dictionary(Of String, Object) From {
            {"@user_id", userId},
            {"@customer_id", customerId}
        }
        DBHelper.ExecuteQuery(query, params)
    End Sub

    ' ------------------ Save Booking Services ------------------
    Public Shared Sub SaveBookingServices(bookingId As Integer, catering As Boolean, clown As Boolean, singer As Boolean, dancer As Boolean, videoke As Boolean)
        ' Delete previous entries
        Dim deleteQuery As String = "DELETE FROM BookingServices WHERE booking_id = @bookingId"
        Dim deleteParams As New Dictionary(Of String, Object) From {{"@bookingId", bookingId}}
        DBHelper.ExecuteQuery(deleteQuery, deleteParams)

        ' Insert selected services dynamically
        Dim insertQuery As String = "INSERT INTO BookingServices (booking_id, service_id) VALUES (@bookingId, @serviceId)"

        Using connection As MySqlConnection = DBHelper.GetConnection()
            Try
                connection.Open()
                Using cmd As New MySqlCommand(insertQuery, connection)
                    cmd.Parameters.AddWithValue("@bookingId", bookingId)

                    Dim serviceSelections As Dictionary(Of Integer, Boolean) = New Dictionary(Of Integer, Boolean) From {
                    {1, catering},
                    {2, clown},
                    {3, singer},
                    {4, dancer},
                    {5, videoke}
                }

                    For Each service In serviceSelections
                        If service.Value Then
                            cmd.Parameters.AddWithValue("@serviceId", service.Key)
                            cmd.ExecuteNonQuery()
                            cmd.Parameters.RemoveAt("@serviceId")
                        End If
                    Next
                End Using
            Catch ex As Exception
                MessageBox.Show("Error saving booking services: " & ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Finally
                connection.Close()
            End Try
        End Using
    End Sub



    ' ------------------ Load Booked Dates ------------------
    Public Shared Function LoadBookedDates(placeId As Integer) As List(Of Date)
        Dim bookedDates As New List(Of Date)
        Dim query As String = "SELECT event_date, event_end_date FROM Bookings WHERE place_id = @place_id"

        Dim params As New Dictionary(Of String, Object) From {{"@place_id", placeId}}

        Dim dt As DataTable = DBHelper.GetDataTable(query, params)
        For Each row As DataRow In dt.Rows
            Dim startDate As Date = Convert.ToDateTime(row("event_date"))
            Dim endDate As Date = Convert.ToDateTime(row("event_end_date"))

            ' Add all dates in the range to the booked list
            For Each d As Date In Enumerable.Range(0, (endDate - startDate).Days + 1).Select(Function(i) startDate.AddDays(i))
                bookedDates.Add(d)
            Next
        Next


        Return bookedDates
    End Function

    ' ------------------ Populate Event Type Combo ------------------
    Public Shared Sub PopulateEventTypeCombo(eventPlaceName As String, cbEventType As ComboBox)
        cbEventType.Items.Clear()

        Dim query As String = "SELECT event_type FROM eventplace WHERE event_place = @event_place"
        Dim params As New Dictionary(Of String, Object) From {{"@event_place", eventPlaceName}}
        Dim dt As DataTable = DBHelper.GetDataTable(query, params)

        For Each row As DataRow In dt.Rows
            Dim eventTypes As String = row("event_type").ToString()
            Dim separatedTypes As String() = eventTypes.Split(","c)
            For Each eventType As String In separatedTypes
                cbEventType.Items.Add(eventType.Trim())
            Next
        Next
    End Sub

    ' ------------------ Place a New Booking ------------------
    Public Shared Function PlaceBooking(customerId As Integer, placeId As Integer, numGuests As Integer, eventDateStart As Date,
                                    eventStartTime As String, eventEndTime As String, totalPrice As Decimal) As Integer

        ' Check for duplicate booking BEFORE inserting
        Dim checkQuery As String = "SELECT COUNT(*) FROM Bookings WHERE place_id = @place_id AND event_date = @event_date"
        Dim checkParams As New Dictionary(Of String, Object) From {{"@place_id", placeId}, {"@event_date", eventDateStart}}

        If Convert.ToInt32(DBHelper.ExecuteScalarQuery(checkQuery, checkParams)) > 0 Then
            Return -1 ' Indicates duplicate booking
        End If

        ' Proceed with booking insertion
        Dim query As String = "INSERT INTO Bookings (customer_id, place_id, num_guests, event_date, event_time, event_end_time, total_price) VALUES (@customer_id, @place_id, @num_guests, @event_date, @event_time, @event_end_time, @total_price); SELECT LAST_INSERT_ID();"

        Dim params As New Dictionary(Of String, Object) From {
        {"@customer_id", customerId},
        {"@place_id", placeId},
        {"@num_guests", numGuests},
        {"@event_date", eventDateStart},
        {"@event_time", eventStartTime},
        {"@event_end_time", eventEndTime},
        {"@total_price", totalPrice}
    }

        Dim bookingId As Object = DBHelper.ExecuteScalarQuery(query, params)
        Return If(bookingId IsNot Nothing, Convert.ToInt32(bookingId), -1)
    End Function


    ' ------------------ Insert Payment Record ------------------
    Public Shared Sub InsertPaymentRecord(bookingId As Integer, customerId As Integer, amountToPay As Decimal)
        Dim query As String = "INSERT INTO payments (booking_id, customer_id, amount_to_pay) VALUES (@booking_id, @customer_id, @amount_to_pay)"
        Dim params As New Dictionary(Of String, Object) From {
            {"@booking_id", bookingId},
            {"@customer_id", customerId},
            {"@amount_to_pay", amountToPay}
        }
        DBHelper.ExecuteQuery(query, params)
    End Sub

    Public Shared Function GetCustomerData(userId As Integer) As DataTable
        Dim query As String = "SELECT name, birthday, sex, address FROM Customers WHERE user_id = @userId"
        Dim parameters As New Dictionary(Of String, Object) From {{"@userId", userId}}
        Return DBHelper.GetDataTable(query, parameters)
    End Function

    Public Shared Function GetLastBooking(userId As Integer) As DataTable
        Dim query As String = "SELECT event_type, num_guests FROM Bookings WHERE customer_id = @userId ORDER BY booking_id DESC LIMIT 1"
        Dim params As New Dictionary(Of String, Object) From {{"@userId", userId}}
        Return DBHelper.GetDataTable(query, params)
    End Function


End Class
