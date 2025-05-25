Imports System.IO
Imports System.Windows.Forms ' For ToolTip support

Public Class HelperResultsDisplay
    Public Shared Sub PopulateResults(flpResults As FlowLayoutPanel, dt As DataTable, btnClickHandler As EventHandler)

        Dim scrollbarWidth As Integer = SystemInformation.VerticalScrollBarWidth
        Dim availableWidth As Integer = 721 - scrollbarWidth - (10 * 6)
        Dim panelWidth As Integer = availableWidth \ 3
        Dim panelHeight As Integer = 250

        Dim toolTip As New ToolTip() ' Create ToolTip instance

        flpResults.WrapContents = True
        flpResults.AutoScroll = True

        For Each row As DataRow In dt.Rows
            Dim panel As New Panel()
            panel.Size = New Size(panelWidth, panelHeight)
            panel.BorderStyle = BorderStyle.FixedSingle
            panel.Margin = New Padding(10)

            Dim pb As New PictureBox()
            pb.Size = New Size(panelWidth, 140)
            pb.Location = New Point(0, 0)
            pb.SizeMode = PictureBoxSizeMode.StretchImage

            Dim imagePath As String = row("image_url").ToString().Trim()
            Dim defaultImagePath As String = "C:\event images\No Image.png"

            If String.IsNullOrEmpty(imagePath) OrElse Not File.Exists(imagePath) Then
                imagePath = defaultImagePath
            End If

            Try
                If File.Exists(imagePath) Then
                    pb.Image = Image.FromFile(imagePath)
                Else
                    MessageBox.Show("File not found: " & imagePath)
                    pb.Image = Nothing
                End If
            Catch ex As Exception
                MessageBox.Show("Error loading image: " & imagePath & vbCrLf & ex.Message)
                pb.Image = Nothing
            End Try

            Dim lblName As New Label()
            lblName.AutoSize = False
            lblName.Size = New Size(panelWidth - 20, 22)
            lblName.Location = New Point(5, 140)
            lblName.Text = row("event_place").ToString()
            lblName.Font = New Font("Poppins", 10, FontStyle.Bold)
            lblName.ForeColor = Color.Black

            Dim lblCapacity As New Label()
            lblCapacity.AutoSize = False
            lblCapacity.Size = New Size(panelWidth - 20, 20)
            lblCapacity.Location = New Point(5, 165)
            lblCapacity.Text = "Capacity: " & row("capacity").ToString()
            lblCapacity.Font = New Font("Poppins", 8)
            lblCapacity.ForeColor = Color.Black

            Dim lblPrice As New Label()
            lblPrice.AutoSize = False
            lblPrice.Size = New Size(panelWidth - 20, 20)
            lblPrice.Location = New Point(5, 185)
            lblPrice.Text = "Price per Day: " & row("price_per_day").ToString()
            lblPrice.Font = New Font("Poppins", 8)
            lblPrice.ForeColor = Color.Black

            Dim fullEventTypesText As String = row("event_type").ToString()
            Dim eventTypesList As String() = System.Text.RegularExpressions.Regex.Split(fullEventTypesText, "\s*,\s*")
            Dim displayEventTypes As String = String.Join(", ", eventTypesList.Take(3)) ' Show first 3 event types

            If eventTypesList.Length > 3 Then
                displayEventTypes &= "..."
            End If

            Dim lblEventType As New Label()
            lblEventType.AutoSize = False
            lblEventType.Size = New Size(panelWidth - 20, 20)
            lblEventType.Location = New Point(5, 205)
            lblEventType.Text = "Event Types: " & displayEventTypes
            lblEventType.Font = New Font("Poppins", 8)
            lblEventType.ForeColor = Color.Black

            ' Set ToolTip to show full event types on hover
            toolTip.SetToolTip(lblEventType, fullEventTypesText)

            Dim btnBook As New Button()
            btnBook.Text = "Book Now"
            btnBook.Size = New Size(80, 30)
            btnBook.Location = New Point(panelWidth - 85, 215)
            btnBook.Tag = row

            AddHandler btnBook.Click, btnClickHandler

            panel.Controls.Add(btnBook)
            panel.Controls.Add(pb)
            panel.Controls.Add(lblName)
            panel.Controls.Add(lblCapacity)
            panel.Controls.Add(lblPrice)
            panel.Controls.Add(lblEventType)

            flpResults.Controls.Add(panel)
        Next
    End Sub
End Class
