Imports System.IO
Imports System.Windows.Forms
Imports System.Drawing
Imports System.Linq
Imports System.Text.RegularExpressions

Public Class HelperResultsDisplay

    ' Generic method to populate any FlowLayoutPanel from a DataTable,
    ' using a delegate to create each Panel.
    Public Shared Sub PopulateFlowPanel(ByVal flp As FlowLayoutPanel, ByVal dt As DataTable, ByVal panelCreator As Func(Of DataRow, Panel))
        flp.WrapContents = True
        flp.AutoScroll = True
        flp.Controls.Clear()

        For Each row As DataRow In dt.Rows
            Dim panel As Panel = panelCreator(row)
            flp.Controls.Add(panel)
        Next
    End Sub

    '--- Specialized method for Event Places ---
    Public Shared Sub PopulateEventPlaces(ByVal flpResults As FlowLayoutPanel, ByVal dt As DataTable,
                                            ByVal btnBookHandler As EventHandler,
                                            ByVal btnUpdateHandler As EventHandler,
                                            ByVal btnDeleteHandler As EventHandler,
                                            ByVal isAdmin As Boolean)
        Dim scrollbarWidth As Integer = SystemInformation.VerticalScrollBarWidth
        Dim availableWidth As Integer = flpResults.Width - scrollbarWidth - (10 * 6)
        Dim panelWidth As Integer = availableWidth \ 3
        Dim panelHeight As Integer = 280

        Dim toolTip As New ToolTip()

        Dim createPanel As Func(Of DataRow, Panel) = Function(row As DataRow)
                                                         Dim panel As New Panel()
                                                         panel.Size = New Size(panelWidth, panelHeight)
                                                         panel.BorderStyle = BorderStyle.FixedSingle
                                                         panel.Margin = New Padding(10)

                                                         ' PictureBox for event image
                                                         Dim pb As New PictureBox()
                                                         pb.Size = New Size(panelWidth, 140)
                                                         pb.Location = New Point(0, 0)
                                                         pb.SizeMode = PictureBoxSizeMode.StretchImage
                                                         Dim imagePath As String = row("image_url").ToString().Trim()
                                                         Dim defaultImagePath As String = "C:\event images\No Image.png"
                                                         If String.IsNullOrEmpty(imagePath) OrElse Not IO.File.Exists(imagePath) Then
                                                             imagePath = defaultImagePath
                                                         End If
                                                         Try
                                                             pb.Image = Image.FromFile(imagePath)
                                                         Catch ex As Exception
                                                             pb.Image = Nothing
                                                         End Try

                                                         ' Label for event place name
                                                         Dim lblName As New Label()
                                                         lblName.AutoSize = False
                                                         lblName.Size = New Size(panelWidth - 20, 22)
                                                         lblName.Location = New Point(5, 140)
                                                         lblName.Text = row("event_place").ToString()
                                                         lblName.Font = New Font("Poppins", 10, FontStyle.Bold)

                                                         ' Label for capacity
                                                         Dim lblCapacity As New Label()
                                                         lblCapacity.AutoSize = False
                                                         lblCapacity.Size = New Size(panelWidth - 20, 20)
                                                         lblCapacity.Location = New Point(5, 165)
                                                         lblCapacity.Text = "Capacity: " & row("capacity").ToString()

                                                         ' Label for price per day
                                                         Dim lblPrice As New Label()
                                                         lblPrice.AutoSize = False
                                                         lblPrice.Size = New Size(panelWidth - 20, 20)
                                                         lblPrice.Location = New Point(5, 185)
                                                         lblPrice.Text = "Price per Day: " & row("price_per_day").ToString()

                                                         ' Label for event types
                                                         Dim fullEventTypesText As String = row("event_type").ToString()
                                                         Dim eventTypesList As String() = Regex.Split(fullEventTypesText, "\s*,\s*")
                                                         Dim displayEventTypes As String = String.Join(", ", eventTypesList.Take(3))
                                                         If eventTypesList.Length > 3 Then
                                                             displayEventTypes &= "..."
                                                         End If
                                                         Dim lblEventType As New Label()
                                                         lblEventType.AutoSize = False
                                                         lblEventType.Size = New Size(panelWidth - 20, 20)
                                                         lblEventType.Location = New Point(5, 205)
                                                         lblEventType.Text = "Event Types: " & displayEventTypes
                                                         toolTip.SetToolTip(lblEventType, fullEventTypesText)

                                                         ' Label for booking status
                                                         Dim lblStatus As New Label()
                                                         lblStatus.AutoSize = False
                                                         lblStatus.Size = New Size(panelWidth - 20, 20)
                                                         lblStatus.Location = New Point(5, 225)
                                                         Dim colName As String = dt.Columns.Cast(Of DataColumn)() _
                                                         .FirstOrDefault(Function(c) String.Equals(c.ColumnName, "status", StringComparison.OrdinalIgnoreCase))?.ColumnName
                                                         If Not String.IsNullOrEmpty(colName) Then
                                                             lblStatus.Text = If(row(colName).ToString() = "Booked", "Status: 🔴 Booked", "Status: 🟢 Available")
                                                         Else
                                                             lblStatus.Text = "Status: Unknown"
                                                         End If

                                                         ' Action Button(s): either Book or Update/Delete buttons
                                                         If Not isAdmin Then
                                                             Dim btnBook As New Button()
                                                             btnBook.Text = "Book Now"
                                                             btnBook.Size = New Size(panelWidth - 20, 30)
                                                             btnBook.Location = New Point(10, panelHeight - 40)
                                                             btnBook.Tag = row
                                                             AddHandler btnBook.Click, btnBookHandler
                                                             panel.Controls.Add(btnBook)
                                                         Else
                                                             ' Create update and delete buttons for admin
                                                             Dim btnUpdate As New Button()
                                                             btnUpdate.Text = "Update"
                                                             btnUpdate.Size = New Size(80, 30)
                                                             btnUpdate.Location = New Point(5, 245)
                                                             btnUpdate.Tag = row

                                                             Dim btnDelete As New Button()
                                                             btnDelete.Text = "Delete"
                                                             btnDelete.Size = New Size(80, 30)
                                                             btnDelete.Location = New Point(90, 245)
                                                             btnDelete.Tag = row

                                                             AddHandler btnUpdate.Click, btnUpdateHandler
                                                             AddHandler btnDelete.Click, btnDeleteHandler

                                                             panel.Controls.Add(btnUpdate)
                                                             panel.Controls.Add(btnDelete)
                                                         End If

                                                         panel.Controls.Add(pb)
                                                         panel.Controls.Add(lblName)
                                                         panel.Controls.Add(lblCapacity)
                                                         panel.Controls.Add(lblPrice)
                                                         panel.Controls.Add(lblEventType)
                                                         panel.Controls.Add(lblStatus)

                                                         Return panel
                                                     End Function

        PopulateFlowPanel(flpResults, dt, createPanel)
    End Sub

    '--- Specialized method for Pending Bookings (with Approve and Reject handlers) ---
    Public Shared Sub PopulatePendingBookings(ByVal flpPendingBookings As FlowLayoutPanel, ByVal dt As DataTable,
                                                ByVal approveHandler As EventHandler, ByVal rejectHandler As EventHandler)
        Dim createPanel As Func(Of DataRow, Panel) = Function(row As DataRow)
                                                         Dim panel As New Panel()
                                                         panel.Size = New Size(300, 80)
                                                         panel.BorderStyle = BorderStyle.FixedSingle
                                                         panel.Margin = New Padding(10)

                                                         Dim lblName As New Label With {
                                                             .Text = "Customer: " & row("name").ToString(),
                                                             .Location = New Point(5, 5),
                                                             .AutoSize = True
                                                         }
                                                         Dim lblEvent As New Label With {
                                                             .Text = "Event: " & row("event_place").ToString(),
                                                             .Location = New Point(5, 25),
                                                             .AutoSize = True
                                                         }
                                                         Dim lblDate As New Label With {
                                                             .Text = "Date: " & row("event_date").ToString(),
                                                             .Location = New Point(5, 45),
                                                             .AutoSize = True
                                                         }
                                                         panel.Controls.Add(lblName)
                                                         panel.Controls.Add(lblEvent)
                                                         panel.Controls.Add(lblDate)

                                                         ' Approve button
                                                         Dim btnApprove As New Button()
                                                         btnApprove.Text = "Approve"
                                                         btnApprove.Size = New Size(75, 25)
                                                         btnApprove.Location = New Point(200, 5)
                                                         btnApprove.Tag = row
                                                         AddHandler btnApprove.Click, approveHandler
                                                         panel.Controls.Add(btnApprove)

                                                         ' Reject button
                                                         Dim btnReject As New Button()
                                                         btnReject.Text = "Reject"
                                                         btnReject.Size = New Size(75, 25)
                                                         btnReject.Location = New Point(200, 35)
                                                         btnReject.Tag = row
                                                         AddHandler btnReject.Click, rejectHandler
                                                         panel.Controls.Add(btnReject)

                                                         ' Set background color based on urgency
                                                         Dim eventDate As DateTime
                                                         If DateTime.TryParse(row("event_date").ToString(), eventDate) Then
                                                             Dim daysUntilEvent As Integer = (eventDate - DateTime.Now).Days
                                                             If daysUntilEvent <= 2 Then
                                                                 panel.BackColor = Color.Orange
                                                             ElseIf daysUntilEvent <= 7 Then
                                                                 panel.BackColor = Color.Yellow
                                                             Else
                                                                 panel.BackColor = Color.LightGreen
                                                             End If
                                                         Else
                                                             panel.BackColor = Color.LightGray
                                                         End If

                                                         Return panel
                                                     End Function
        PopulateFlowPanel(flpPendingBookings, dt, createPanel)
    End Sub

    '--- Specialized method for Event Place Availability ---
    Public Shared Sub PopulateAvailability(ByVal flpAvailability As FlowLayoutPanel, ByVal dt As DataTable)
        Dim createPanel As Func(Of DataRow, Panel) = Function(row As DataRow)
                                                         Dim panel As New Panel()
                                                         panel.Size = New Size(200, 50)
                                                         panel.BorderStyle = BorderStyle.FixedSingle
                                                         panel.Margin = New Padding(10)

                                                         Dim lblEventPlace As New Label With {
                                                             .Text = row("event_place").ToString(),
                                                             .Location = New Point(5, 5),
                                                             .AutoSize = True
                                                         }
                                                         Dim lblStatus As New Label With {
                                                             .Text = "Status: " & row("Availability").ToString(),
                                                             .Location = New Point(5, 25),
                                                             .AutoSize = True
                                                         }
                                                         panel.Controls.Add(lblEventPlace)
                                                         panel.Controls.Add(lblStatus)
                                                         Return panel
                                                     End Function
        PopulateFlowPanel(flpAvailability, dt, createPanel)
    End Sub

    '--- Specialized method for Revenue Reports ---
    Public Shared Sub PopulateRevenueReports(ByVal flpRevenueReports As FlowLayoutPanel, ByVal dt As DataTable)
        Dim createPanel As Func(Of DataRow, Panel) = Function(row As DataRow)
                                                         Dim panel As New Panel()
                                                         panel.Size = New Size(250, 60)
                                                         panel.BorderStyle = BorderStyle.FixedSingle
                                                         panel.Margin = New Padding(10)

                                                         Dim lblEvent As New Label With {
                                                             .Text = row("event_place").ToString(),
                                                             .Location = New Point(5, 5),
                                                             .AutoSize = True
                                                         }
                                                         Dim lblRevenue As New Label With {
                                                             .Text = "Revenue: " & row("total_revenue").ToString(),
                                                             .Location = New Point(5, 30),
                                                             .AutoSize = True
                                                         }
                                                         panel.Controls.Add(lblEvent)
                                                         panel.Controls.Add(lblRevenue)
                                                         Return panel
                                                     End Function
        PopulateFlowPanel(flpRevenueReports, dt, createPanel)
    End Sub

    '--- Specialized method for Invoices (with Accept Payment handler) ---
    Public Shared Sub PopulateInvoices(ByVal flpInvoices As FlowLayoutPanel, ByVal dt As DataTable, ByVal paymentHandler As EventHandler)
        Dim createPanel As Func(Of DataRow, Panel) = Function(row As DataRow)
                                                         Dim panel As New Panel()
                                                         panel.Size = New Size(250, 80)
                                                         panel.BorderStyle = BorderStyle.FixedSingle
                                                         panel.Margin = New Padding(10)

                                                         Dim lblInvoiceID As New Label With {
                                                             .Text = "Invoice #" & row("invoice_id").ToString(),
                                                             .Location = New Point(5, 5),
                                                             .AutoSize = True
                                                         }
                                                         Dim lblEventPlace As New Label With {
                                                             .Text = "Event: " & row("event_place").ToString(),
                                                             .Location = New Point(5, 25),
                                                             .AutoSize = True
                                                         }
                                                         Dim lblAmount As New Label With {
                                                             .Text = "Amount: " & row("total_amount").ToString(),
                                                             .Location = New Point(5, 45),
                                                             .AutoSize = True
                                                         }
                                                         panel.Controls.Add(lblInvoiceID)
                                                         panel.Controls.Add(lblEventPlace)
                                                         panel.Controls.Add(lblAmount)

                                                         Dim btnPayment As New Button()
                                                         btnPayment.Text = "Accept Payment"
                                                         btnPayment.Size = New Size(100, 25)
                                                         btnPayment.Location = New Point(135, 30)
                                                         btnPayment.Tag = row
                                                         AddHandler btnPayment.Click, paymentHandler
                                                         panel.Controls.Add(btnPayment)

                                                         Return panel
                                                     End Function
        PopulateFlowPanel(flpInvoices, dt, createPanel)
    End Sub

    '--- Specialized method for Customer Records ---
    Public Shared Sub PopulateCustomerRecords(ByVal flpCustomerRecords As FlowLayoutPanel, ByVal dt As DataTable)
        Dim createPanel As Func(Of DataRow, Panel) = Function(row As DataRow)
                                                         Dim panel As New Panel()
                                                         panel.Size = New Size(300, 80)
                                                         panel.BorderStyle = BorderStyle.FixedSingle
                                                         panel.Margin = New Padding(10)

                                                         Dim lblName As New Label With {
                                                             .Text = row("name").ToString(),
                                                             .Location = New Point(5, 5),
                                                             .AutoSize = True
                                                         }
                                                         Dim lblAge As New Label With {
                                                             .Text = "Age: " & row("age").ToString(),
                                                             .Location = New Point(5, 25),
                                                             .AutoSize = True
                                                         }
                                                         Dim lblAddress As New Label With {
                                                             .Text = "Address: " & row("address").ToString(),
                                                             .Location = New Point(5, 45),
                                                             .AutoSize = True
                                                         }
                                                         panel.Controls.Add(lblName)
                                                         panel.Controls.Add(lblAge)
                                                         panel.Controls.Add(lblAddress)
                                                         Return panel
                                                     End Function
        PopulateFlowPanel(flpCustomerRecords, dt, createPanel)
    End Sub

    '--- Specialized method for Booked Dates ---
    Public Shared Sub PopulateBookedDates(ByVal flpBookedDates As FlowLayoutPanel, ByVal dt As DataTable)
        Dim createPanel As Func(Of DataRow, Panel) = Function(row As DataRow)
                                                         Dim panel As New Panel()
                                                         panel.Size = New Size(150, 40)
                                                         panel.BorderStyle = BorderStyle.FixedSingle
                                                         panel.Margin = New Padding(5)

                                                         Dim lblDate As New Label()
                                                         lblDate.AutoSize = True
                                                         lblDate.Location = New Point(5, 10)
                                                         lblDate.Text = "Date: " & row("event_date").ToString()
                                                         panel.Controls.Add(lblDate)
                                                         Return panel
                                                     End Function
        PopulateFlowPanel(flpBookedDates, dt, createPanel)
    End Sub

End Class
