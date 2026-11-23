
Imports System.Data
Imports Microsoft.Win32

Public Class Form1

    Private calculationHistory As New List(Of String)

    ' ==========================================
    ' FORM LOAD
    ' ==========================================

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.KeyPreview = True
        Me.WindowState = FormWindowState.Maximized

        TextBox1.ReadOnly = True
        TextBox1.TabStop = False
        TextBox1.TextAlign = HorizontalAlignment.Right

        For Each ctrl As Control In Me.Controls
            If TypeOf ctrl Is Button Then
                ctrl.TabStop = False
            End If
        Next

        BuildResponsiveLayout()

        AddHandler SystemEvents.UserPreferenceChanged, AddressOf SystemThemeChanged

        ApplySystemTheme()

    End Sub


    ' ==========================================
    ' NUMBER BUTTONS
    ' ==========================================

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click, Button2.Click, Button3.Click, Button4.Click, Button5.Click, Button6.Click, Button7.Click, Button8.Click, Button9.Click

        Dim button As Button = CType(sender, Button)
        AddKeyboardNumber(button.Text)

    End Sub


    Private Sub Button11_Click(sender As Object, e As EventArgs) Handles Button11.Click

        AddKeyboardNumber("0")

    End Sub


    ' ==========================================
    ' OPERATORS
    ' ==========================================

    Private Sub multiply_Click(sender As Object, e As EventArgs) Handles multiply.Click, divide.Click, minus.Click, addition.Click

        If TextBox1.Text = String.Empty OrElse TextBox1.Text = "Error" Then Exit Sub

        Dim lastChar As Char = TextBox1.Text(TextBox1.Text.Length - 1)

        If "+-*/".Contains(lastChar) Then
            TextBox1.Text = TextBox1.Text.Substring(0, TextBox1.Text.Length - 1)
        End If

        Dim button As Button = CType(sender, Button)
        TextBox1.Text &= button.Text

    End Sub


    ' ==========================================
    ' EQUALS BUTTON + HISTORY
    ' ==========================================

    Private Sub Equals_Click(sender As Object, e As EventArgs) Handles Equals.Click

        If String.IsNullOrWhiteSpace(TextBox1.Text) Then Exit Sub

        Dim expression As String = TextBox1.Text

        Try

            Dim result As Double = Convert.ToDouble(
                New DataTable().Compute(expression, Nothing)
            )

            Dim resultText As String = result.ToString()

            TextBox1.Text = resultText

            ' Save successful calculation

            calculationHistory.Insert(0, expression & " = " & resultText)

            ' Maximum 100 history records

            If calculationHistory.Count > 100 Then
                calculationHistory.RemoveAt(calculationHistory.Count - 1)
            End If

        Catch ex As Exception

            TextBox1.Text = "Error"

        End Try

    End Sub


    ' ==========================================
    ' CLEAR BUTTONS
    ' ==========================================

    Private Sub Button19_Click(sender As Object, e As EventArgs) Handles Button19.Click

        TextBox1.Clear()

    End Sub


    Private Sub Button18_Click(sender As Object, e As EventArgs) Handles Button18.Click

        TextBox1.Text = "0"

    End Sub


    ' ==========================================
    ' DECIMAL BUTTON
    ' ==========================================

    Private Sub Button12_Click(sender As Object, e As EventArgs) Handles dot.Click

        If TextBox1.Text = String.Empty OrElse TextBox1.Text = "Error" Then

            TextBox1.Text = "0."

        Else

            Dim parts As String() = TextBox1.Text.Split("+"c, "-"c, "*"c, "/"c)
            Dim lastNumber As String = parts(parts.Length - 1)

            If Not lastNumber.Contains(".") Then
                TextBox1.Text &= "."
            End If

        End If

    End Sub


    ' ==========================================
    ' BACKSPACE BUTTON
    ' ==========================================

    Private Sub Button20_Click(sender As Object, e As EventArgs) Handles Button20.Click

        If TextBox1.Text = "Error" OrElse TextBox1.Text.Length <= 1 Then

            TextBox1.Text = "0"

        Else

            TextBox1.Text = TextBox1.Text.Substring(0, TextBox1.Text.Length - 1)

        End If

    End Sub


    ' ==========================================
    ' PERCENTAGE BUTTON
    ' ==========================================

    Private Sub Button17_Click(sender As Object, e As EventArgs) Handles Button17.Click

        Dim number As Decimal

        If Decimal.TryParse(TextBox1.Text, number) Then
            TextBox1.Text = (number / 100D).ToString()
        End If

    End Sub


    ' ==========================================
    ' PLUS / MINUS BUTTON
    ' ==========================================

    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click

        Dim number As Decimal

        If Decimal.TryParse(TextBox1.Text, number) Then
            TextBox1.Text = (-number).ToString()
        End If

    End Sub


    ' ==========================================
    ' KEYBOARD SUPPORT - NUMBERS & OPERATORS
    ' ==========================================

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


    ' ==========================================
    ' KEYBOARD SUPPORT - SPECIAL KEYS
    ' ==========================================

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown

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


    ' ==========================================
    ' PREVENT ARROW KEY BUTTON FOCUS
    ' ==========================================

    Protected Overrides Function ProcessDialogKey(keyData As Keys) As Boolean

        Select Case (keyData And Keys.KeyCode)

            Case Keys.Left, Keys.Right, Keys.Up, Keys.Down

                Return True

        End Select

        Return MyBase.ProcessDialogKey(keyData)

    End Function


    ' ==========================================
    ' KEYBOARD NUMBER HELPER
    ' ==========================================

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


    ' ==========================================
    ' RESPONSIVE LAYOUT
    ' FONT SIZES REMAIN UNCHANGED
    ' ==========================================

    Private Sub BuildResponsiveLayout()

        Me.SuspendLayout()

        Dim mainLayout As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 2,
            .Padding = New Padding(8),
            .Margin = New Padding(0)
        }

        mainLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 22))
        mainLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 78))

        ' Display panel

        Dim displayPanel As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 1,
            .Margin = New Padding(0)
        }

        displayPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        displayPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 85))

        ' Display textbox

        TextBox1.Dock = DockStyle.Fill
        TextBox1.Margin = New Padding(8)

        ' History button

        Dim historyButton As New NoFocusButton With {
            .Text = "History",
            .Dock = DockStyle.Fill,
            .Margin = New Padding(3),
            .TabStop = False
        }

        AddHandler historyButton.Click, AddressOf ShowHistory

        displayPanel.Controls.Add(TextBox1, 0, 0)
        displayPanel.Controls.Add(historyButton, 1, 0)

        ' Keypad

        Dim keypad As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 4,
            .RowCount = 6,
            .Margin = New Padding(0)
        }

        For i As Integer = 1 To 4

            keypad.ColumnStyles.Add(
                New ColumnStyle(SizeType.Percent, 25)
            )

        Next

        For i As Integer = 1 To 6

            keypad.RowStyles.Add(
                New RowStyle(SizeType.Percent, 100.0F / 6)
            )

        Next

        ' Calculator buttons

        Dim buttons As Button() = {
            Button17, Button18, Button19, Button20,
            Button1, Button2, Button3, divide,
            Button4, Button5, Button6, multiply,
            Button7, Button8, Button9, minus,
            Button10, Button11, dot, addition,
            Equals
        }

        For Each btn As Button In buttons

            btn.Dock = DockStyle.Fill
            btn.Margin = New Padding(3)
            btn.TabStop = False

        Next

        ' First row

        keypad.Controls.Add(Button17, 0, 0)
        keypad.Controls.Add(Button18, 1, 0)
        keypad.Controls.Add(Button19, 2, 0)
        keypad.Controls.Add(Button20, 3, 0)

        ' Second row

        keypad.Controls.Add(Button1, 0, 1)
        keypad.Controls.Add(Button2, 1, 1)
        keypad.Controls.Add(Button3, 2, 1)
        keypad.Controls.Add(divide, 3, 1)

        ' Third row

        keypad.Controls.Add(Button4, 0, 2)
        keypad.Controls.Add(Button5, 1, 2)
        keypad.Controls.Add(Button6, 2, 2)
        keypad.Controls.Add(multiply, 3, 2)

        ' Fourth row

        keypad.Controls.Add(Button7, 0, 3)
        keypad.Controls.Add(Button8, 1, 3)
        keypad.Controls.Add(Button9, 2, 3)
        keypad.Controls.Add(minus, 3, 3)

        ' Fifth row

        keypad.Controls.Add(Button10, 0, 4)
        keypad.Controls.Add(Button11, 1, 4)
        keypad.Controls.Add(dot, 2, 4)
        keypad.Controls.Add(addition, 3, 4)

        ' Equals button

        keypad.Controls.Add(Equals, 0, 5)
        keypad.SetColumnSpan(Equals, 4)

        ' Add panels

        mainLayout.Controls.Add(displayPanel, 0, 0)
        mainLayout.Controls.Add(keypad, 0, 1)

        Me.Controls.Add(mainLayout)
        mainLayout.BringToFront()

        Me.ResumeLayout(True)

    End Sub


    ' ==========================================
    ' CALCULATION HISTORY WINDOW
    ' ==========================================

    Private Sub ShowHistory(sender As Object, e As EventArgs)

        Using historyWindow As New Form()

            historyWindow.Text = "Calculation History"
            historyWindow.Size = New Size(400, 460)
            historyWindow.MinimumSize = New Size(400, 460)
            historyWindow.StartPosition = FormStartPosition.CenterParent
            historyWindow.FormBorderStyle = FormBorderStyle.FixedDialog
            historyWindow.MaximizeBox = False
            historyWindow.MinimizeBox = False
            historyWindow.ShowInTaskbar = False

            Dim darkMode As Boolean = IsSystemDarkMode()

            If darkMode Then

                historyWindow.BackColor = Color.FromArgb(30, 30, 30)
                historyWindow.ForeColor = Color.White

            Else

                historyWindow.BackColor = Color.FromArgb(245, 245, 245)
                historyWindow.ForeColor = Color.Black

            End If

            ' Main layout

            Dim layout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 3,
                .Padding = New Padding(10)
            }

            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 35))
            layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 45))

            ' Information label

            Dim infoLabel As New Label With {
                .Text = "Double-click a calculation to reuse its result.",
                .Dock = DockStyle.Fill,
                .TextAlign = ContentAlignment.MiddleLeft
            }

            ' History list

            Dim historyList As New ListBox With {
                .Dock = DockStyle.Fill,
                .BorderStyle = BorderStyle.None,
                .IntegralHeight = False,
                .Font = New Font("Segoe UI", 11)
            }

            If darkMode Then

                historyList.BackColor = Color.FromArgb(22, 22, 22)
                historyList.ForeColor = Color.White

            Else

                historyList.BackColor = Color.White
                historyList.ForeColor = Color.Black

            End If

            ' Load history entries

            If calculationHistory.Count > 0 Then

                historyList.Items.AddRange(calculationHistory.ToArray())

            Else

                infoLabel.Text = "No calculations yet."

            End If

            ' Button panel

            Dim buttonPanel As New FlowLayoutPanel With {
                .Dock = DockStyle.Fill,
                .FlowDirection = FlowDirection.RightToLeft,
                .WrapContents = False
            }

            Dim useButton As New Button With {
                .Text = "Use Result",
                .Size = New Size(100, 32),
                .Enabled = False
            }

            Dim clearButton As New Button With {
                .Text = "Clear",
                .Size = New Size(75, 32)
            }

            Dim closeButton As New Button With {
                .Text = "Close",
                .Size = New Size(75, 32)
            }

            ' Button styling

            For Each btn As Button In {useButton, clearButton, closeButton}

                btn.FlatStyle = FlatStyle.Flat
                btn.UseVisualStyleBackColor = False
                btn.Font = New Font("Segoe UI", 9)

                If darkMode Then

                    btn.BackColor = Color.FromArgb(48, 48, 48)
                    btn.ForeColor = Color.White

                Else

                    btn.BackColor = Color.FromArgb(235, 235, 235)
                    btn.ForeColor = Color.Black

                End If

            Next

            useButton.BackColor = Color.FromArgb(0, 120, 215)
            useButton.ForeColor = Color.White

            ' Enable Use Result when an item is selected

            AddHandler historyList.SelectedIndexChanged,
                Sub(s, args)

                    useButton.Enabled = historyList.SelectedItem IsNot Nothing

                End Sub

            ' Use selected result

            AddHandler useButton.Click,
                Sub(s, args)

                    If historyList.SelectedItem IsNot Nothing Then

                        LoadHistoryResult(CStr(historyList.SelectedItem))
                        historyWindow.Close()

                    End If

                End Sub

            ' Double-click to reuse result

            AddHandler historyList.DoubleClick,
                Sub(s, args)

                    If historyList.SelectedItem IsNot Nothing Then

                        LoadHistoryResult(CStr(historyList.SelectedItem))
                        historyWindow.Close()

                    End If

                End Sub

            ' Clear history

            AddHandler clearButton.Click,
                Sub(s, args)

                    calculationHistory.Clear()
                    historyList.Items.Clear()
                    infoLabel.Text = "No calculations yet."
                    useButton.Enabled = False

                End Sub

            ' Close history window

            AddHandler closeButton.Click,
                Sub(s, args)

                    historyWindow.Close()

                End Sub

            ' Add controls

            buttonPanel.Controls.Add(closeButton)
            buttonPanel.Controls.Add(clearButton)
            buttonPanel.Controls.Add(useButton)

            layout.Controls.Add(infoLabel, 0, 0)
            layout.Controls.Add(historyList, 0, 1)
            layout.Controls.Add(buttonPanel, 0, 2)

            historyWindow.Controls.Add(layout)

            historyWindow.ShowDialog(Me)

        End Using

    End Sub


    ' ==========================================
    ' LOAD RESULT FROM HISTORY
    ' ==========================================

    Private Sub LoadHistoryResult(entry As String)

        Dim separator As Integer = entry.LastIndexOf(" = ")

        If separator < 0 Then Exit Sub

        TextBox1.Text = entry.Substring(separator + 3)

    End Sub


    ' ==========================================
    ' DETECT WINDOWS DARK / LIGHT MODE
    ' ==========================================

    Private Function IsSystemDarkMode() As Boolean

        Try

            Using key As RegistryKey = Registry.CurrentUser.OpenSubKey(
                "Software\Microsoft\Windows\CurrentVersion\Themes\Personalize")

                If key Is Nothing Then Return False

                Dim value As Object = key.GetValue("SystemUsesLightTheme")

                If value Is Nothing Then Return False

                Return Convert.ToInt32(value) = 0

            End Using

        Catch

            Return False

        End Try

    End Function


    ' ==========================================
    ' APPLY SYSTEM THEME
    ' ==========================================

    Private Sub ApplySystemTheme()

        Dim darkMode As Boolean = IsSystemDarkMode()

        If darkMode Then

            Me.BackColor = Color.FromArgb(30, 30, 30)
            Me.ForeColor = Color.White

            TextBox1.BackColor = Color.FromArgb(22, 22, 22)
            TextBox1.ForeColor = Color.White

        Else

            Me.BackColor = Color.FromArgb(245, 245, 245)
            Me.ForeColor = Color.FromArgb(30, 30, 30)

            TextBox1.BackColor = Color.White
            TextBox1.ForeColor = Color.Black

        End If

        ApplyButtonTheme(Me, darkMode)

    End Sub


    ' ==========================================
    ' APPLY THEME TO NESTED BUTTONS
    ' ==========================================

    Private Sub ApplyButtonTheme(parent As Control, darkMode As Boolean)

        For Each ctrl As Control In parent.Controls

            If TypeOf ctrl Is TableLayoutPanel Then

                If darkMode Then

                    ctrl.BackColor = Color.FromArgb(30, 30, 30)

                Else

                    ctrl.BackColor = Color.FromArgb(245, 245, 245)

                End If

            End If

            If TypeOf ctrl Is Button Then

                Dim btn As Button = DirectCast(ctrl, Button)

                btn.UseVisualStyleBackColor = False
                btn.FlatStyle = FlatStyle.Flat
                btn.FlatAppearance.BorderSize = 1

                If btn.Text = "=" Then

                    btn.BackColor = Color.FromArgb(0, 120, 215)
                    btn.ForeColor = Color.White
                    btn.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 215)

                ElseIf btn.Text = "History" Then

                    If darkMode Then

                        btn.BackColor = Color.FromArgb(48, 48, 48)
                        btn.ForeColor = Color.White
                        btn.FlatAppearance.BorderColor = Color.FromArgb(75, 75, 75)

                    Else

                        btn.BackColor = Color.FromArgb(235, 235, 235)
                        btn.ForeColor = Color.FromArgb(30, 30, 30)
                        btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200)

                    End If

                ElseIf darkMode Then

                    btn.BackColor = Color.FromArgb(48, 48, 48)
                    btn.ForeColor = Color.White
                    btn.FlatAppearance.BorderColor = Color.FromArgb(75, 75, 75)

                Else

                    btn.BackColor = Color.FromArgb(235, 235, 235)
                    btn.ForeColor = Color.FromArgb(30, 30, 30)
                    btn.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200)

                End If

            End If

            If ctrl.HasChildren Then

                ApplyButtonTheme(ctrl, darkMode)

            End If

        Next

    End Sub


    ' ==========================================
    ' WINDOWS THEME CHANGE EVENT
    ' ==========================================

    Private Sub SystemThemeChanged(sender As Object, e As UserPreferenceChangedEventArgs)

        If Me.IsDisposed OrElse Not Me.IsHandleCreated Then Return

        Try

            Me.BeginInvoke(New MethodInvoker(AddressOf ApplySystemTheme))

        Catch ex As InvalidOperationException

            ' Form is closing

        End Try

    End Sub


    ' ==========================================
    ' REFRESH THEME WHEN FORM BECOMES ACTIVE
    ' ==========================================

    Private Sub Form1_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated

        ApplySystemTheme()

    End Sub


    ' ==========================================
    ' REMOVE EVENT HANDLER WHEN FORM CLOSES
    ' ==========================================

    Private Sub Form1_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed

        RemoveHandler SystemEvents.UserPreferenceChanged, AddressOf SystemThemeChanged

    End Sub

End Class
