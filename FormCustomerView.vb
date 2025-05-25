Public Class FormCustomerView
    Public Sub New(customerId As Integer)
        ' Store or use customerId
    End Sub

    Private Sub btnBack_Click(sender As Object, e As EventArgs) Handles btnBack.Click
        GoBack(Me)
    End Sub

    Private Sub btnNext_Click(sender As Object, e As EventArgs) Handles btnNext.Click
        GoNext(Me)
    End Sub


End Class