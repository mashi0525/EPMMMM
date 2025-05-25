Imports MySql.Data.MySqlClient
Imports System.Globalization
Imports System.Text
Imports System.IO

Public Class HelperBooking
    ' ------------------ Database Methods ------------------
    Public Shared Function GetCustomerData(userId As Integer) As DataTable
        Dim query As String = "SELECT name, birthday, sex, address FROM Customers WHERE user_id = @userId"
        Dim parameters As New Dictionary(Of String, Object) From {{"@userId", userId}}
        Return DBHelper.GetDataTable(query, parameters)
    End Function

    Public Shared Function GetBookedDates(placeId As Integer) As List(Of Date)
        Dim bookedDates As New List(Of Date)
        Dim query As String = "SELECT DISTINCT event_date FROM Bookings WHERE place_id = @place_id"

        Using connection As MySqlConnection = DBHelper.GetConnection()
            Using cmd As New MySqlCommand(query, connection)
                cmd.Parameters.AddWithValue("@place_id", placeId)
                connection.Open()
                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        bookedDates.Add(Convert.ToDateTime(reader("event_date")))
                    End While
                End Using
            End Using
        End Using

        Return bookedDates
    End Function

    ' ------------------ UI & Validation Methods ------------------
    Public Shared Sub LoadCustomerData(userId As Integer, txtName As TextBox, dtpBirthday As DateTimePicker, cmbSex As ComboBox, txtAddress As TextBox)
        Dim dt As DataTable = GetCustomerData(userId)
        If dt.Rows.Count > 0 Then
            txtName.Text = dt.Rows(0)("name").ToString()
            dtpBirthday.Value = Convert.ToDateTime(dt.Rows(0)("birthday"))
            cmbSex.Text = dt.Rows(0)("sex").ToString()
            txtAddress.Text = dt.Rows(0)("address").ToString()
        Else
            MessageBox.Show("Customer information not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
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

    Public Shared Function IsValidTimeSelection(cbEndHour As ComboBox, cbEndMinutes As ComboBox, cbEndAMPM As ComboBox) As Boolean
        Dim eventEndTime As DateTime
        Dim timeFormat As String = "h:mm tt"

        If String.IsNullOrWhiteSpace(cbEndHour.Text) OrElse String.IsNullOrWhiteSpace(cbEndMinutes.Text) OrElse String.IsNullOrWhiteSpace(cbEndAMPM.Text) Then
            Return False
        End If

        If Not DateTime.TryParseExact($"{cbEndHour.Text}:{cbEndMinutes.Text} {cbEndAMPM.Text}", timeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, eventEndTime) Then
            Return False
        End If

        Return True
    End Function
End Class
