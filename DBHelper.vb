Imports System.Data.SqlClient
Imports MySql.Data.MySqlClient

Module DBHelper
    Private ReadOnly connectionString As String = "Server=localhost;Database=event_management;User ID=root;Password=Arianne123;Pooling=True;"

    Public Function GetConnection() As MySqlConnection
        Return New MySqlConnection(connectionString)
    End Function

    Public Function ExecuteQuery(ByVal query As String, ByVal parameters As Dictionary(Of String, Object)) As Integer
        Dim rowsAffected As Integer = 0
        Using connection As MySqlConnection = GetConnection()
            Try
                connection.Open()
                Using command As New MySqlCommand(query, connection)
                    For Each param In parameters
                        command.Parameters.AddWithValue(param.Key, param.Value)
                    Next
                    rowsAffected = command.ExecuteNonQuery() ' Get affected rows
                End Using
            Catch ex As MySqlException
                MessageBox.Show("Database error: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using
        Return rowsAffected
    End Function



    Public Function ExecuteScalarQuery(ByVal query As String, ByVal parameters As Dictionary(Of String, Object)) As Object
        Dim result As Object = Nothing
        Using connection As MySqlConnection = GetConnection()
            Try
                connection.Open()
                Using command As New MySqlCommand(query, connection)
                    For Each param In parameters
                        command.Parameters.AddWithValue(param.Key, param.Value)
                    Next
                    result = command.ExecuteScalar()
                End Using
            Catch ex As MySqlException
                Console.WriteLine("Database error: " & ex.Message)
            End Try
        End Using
        Return result
    End Function

    Public Function GetDataTable(query As String, parameters As Dictionary(Of String, Object)) As DataTable
        Dim dt As New DataTable()
        Using connection As MySqlConnection = GetConnection()
            Try
                connection.Open()
                Using cmd As New MySqlCommand(query, connection)
                    For Each param In parameters
                        cmd.Parameters.AddWithValue(param.Key, param.Value)
                    Next
                    Dim adapter As New MySqlDataAdapter(cmd)
                    adapter.Fill(dt)
                End Using
            Catch ex As MySqlException
                Console.WriteLine("Database error: " & ex.Message)
            End Try
        End Using
        Return dt
    End Function


End Module
