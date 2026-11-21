Public Class Form1

    Private waitingForOperand As Boolean = True
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.KeyPreview = True

        TextBox1.ReadOnly = True
        TextBox1.TabStop = False

        For Each ctrl As Control In Me.Controls
            If TypeOf ctrl Is Button Then
                ctrl.TabStop = False
            End If
        Next

    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click

    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click, Button2.Click, Button3.Click, Button4.Click, Button5.Click, Button6.Click, Button7.Click, Button8.Click, Button9.Click
        Dim button As Button = CType(sender, Button)
        TextBox1.Text += button.Text
    End Sub

    Private Sub Button11_Click(sender As Object, e As EventArgs) Handles Button11.Click
        If TextBox1.Text <> String.Empty Then
            TextBox1.Text += "0"
        End If
    End Sub


    Private Sub multiply_Click(sender As Object, e As EventArgs) Handles multiply.Click, divide.Click, minus.Click, addition.Click

        If TextBox1.Text = String.Empty OrElse TextBox1.Text = "Error" Then Exit Sub

        Dim lastChar As Char = TextBox1.Text(TextBox1.Text.Length - 1)

        If "+-*/".Contains(lastChar) Then
            TextBox1.Text = TextBox1.Text.Substring(0, TextBox1.Text.Length - 1)
        End If

        Dim button As Button = CType(sender, Button)
        TextBox1.Text &= button.Text

    End Sub


    Private Sub Equals_Click(sender As Object, e As EventArgs) Handles Equals.Click
        Try
            Dim result As Double = New DataTable().Compute(TextBox1.Text, Nothing)
            TextBox1.Text = result.ToString()
        Catch ex As Exception
            MessageBox.Show("Invalid expression")
        End Try
    End Sub

    Private Sub Button19_Click(sender As Object, e As EventArgs) Handles Button19.Click
        TextBox1.Clear()
    End Sub

    Private Sub Button12_Click(sender As Object, e As EventArgs) Handles dot.Click
        'If TextBox1.Text = String.Empty Then
        '    TextBox1.Text = "0."
        'ElseIf Not TextBox1.Text.Contains(".") Then
        '    TextBox1.Text += "."
        'End If

        If TextBox1.Text = String.Empty Then
            TextBox1.Text = "0."
        Else
            Dim lastNumber As String = TextBox1.Text.Split("+"c, "-"c, "*"c, "/"c).Last()
            If Not lastNumber.Contains(".") Then
                TextBox1.Text += "."
            End If
        End If
    End Sub

    Private Sub Button20_Click(sender As Object, e As EventArgs) Handles Button20.Click
        If TextBox1.Text = "Error" OrElse TextBox1.Text.Length <= 1 Then
            TextBox1.Text = "0"
        Else
            TextBox1.Text = TextBox1.Text.Substring(0, TextBox1.Text.Length - 1)
        End If

    End Sub

    Private Sub Button18_Click(sender As Object, e As EventArgs) Handles Button18.Click

        TextBox1.Text = "0"

    End Sub

    Private Sub Button17_Click(sender As Object, e As EventArgs) Handles Button17.Click

        Dim number As Decimal

        If Decimal.TryParse(TextBox1.Text, number) Then
            TextBox1.Text = (number / 100D).ToString()
        End If

    End Sub

    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click
        Dim number As Decimal

        If Decimal.TryParse(TextBox1.Text, number) Then
            TextBox1.Text = (-number).ToString()
        End If
    End Sub

    ' Handles numbers, operators and decimal point
    ' Handles numbers, operators and decimal point
    Private Sub Form1_KeyPress(sender As Object, e As KeyPressEventArgs) Handles MyBase.KeyPress

        Select Case e.KeyChar

            Case "0"c To "9"c
                AddKeyboardNumber(e.KeyChar.ToString())
                e.Handled = True

            Case "+"c
                addition.PerformClick()
                e.Handled = True

            Case "-"c
                minus.PerformClick()
                e.Handled = True

            Case "*"c
                multiply.PerformClick()
                e.Handled = True

            Case "/"c
                divide.PerformClick()
                e.Handled = True

            Case "."c, ","c
                dot.PerformClick()
                e.Handled = True

            Case "%"c
                Button17.PerformClick()
                e.Handled = True

        End Select

    End Sub




    ' Handles special keyboard keys
    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        'MessageBox.Show("Keyboard detected: " & e.KeyCode.ToString())

        Select Case e.KeyCode

            Case Keys.Enter
                Equals.PerformClick()

            Case Keys.Back
                Button20.PerformClick()

            Case Keys.Escape
                Button19.PerformClick()

            Case Keys.Delete
                Button18.PerformClick()

            Case Else
                Exit Sub

        End Select

        e.Handled = True
        e.SuppressKeyPress = True

    End Sub

    Protected Overrides Function ProcessDialogKey(keyData As Keys) As Boolean

        Select Case (keyData And Keys.KeyCode)

            Case Keys.Left, Keys.Right, Keys.Up, Keys.Down
                Return True

        End Select

        Return MyBase.ProcessDialogKey(keyData)

    End Function

    Private Sub AddKeyboardNumber(number As String)

        If TextBox1.Text = "Error" Then
            TextBox1.Clear()
        End If

        If TextBox1.Text = "0" Then
            TextBox1.Text = number
        Else
            TextBox1.Text &= number
        End If

    End Sub


End Class

'Public Class CustomButton
'    Inherits Button

'    Protected Overrides Sub OnPaint(pevent As PaintEventArgs)
'        MyBase.OnPaint(pevent)

'        If Not Me.Focused Then
'            Me.FlatStyle = FlatStyle.Flat
'            Me.FlatAppearance.BorderSize = 0
'        End If
'    End Sub
'End Class
