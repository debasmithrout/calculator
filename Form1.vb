
Imports System.Data
Imports System.Globalization
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.Win32

Public Class Form1

    ' CALCULATOR DATA AND STATE

    Private ReadOnly calculationHistory As New List(Of String)
    Private memoryValue As Double = 0

    Private themeMode As String = "System"
    Private calculatorMode As String = "Standard"
    Private angleMode As String = "DEG"
    Private scientificNotation As Boolean = False
    Private secondMode As Boolean = False
    Private sidebarMode As String = "History"
    Private sidebarVisible As Boolean = False

    Private pendingOperation As String = String.Empty
    Private pendingValue As Double
    Private pendingInputStarted As Boolean = False
    Private replaceDisplayOnNextDigit As Boolean = False

    Private appLayout As TableLayoutPanel
    Private calculatorLayout As TableLayoutPanel
    Private headerPanel As TableLayoutPanel
    Private displayPanel As Panel
    Private topControlsPanel As TableLayoutPanel
    Private memoryPanel As TableLayoutPanel
    Private scientificToolbar As TableLayoutPanel
    Private standardKeypadPanel As TableLayoutPanel
    Private scientificKeypadPanel As TableLayoutPanel
    Private keypadHostPanel As Panel
    Private sidebarPanel As TableLayoutPanel
    Private sidebarContentPanel As Panel

    Private headerTitle As Label
    Private menuButton As NoFocusButton
    Private historyButton As NoFocusButton
    Private displayBox As TextBox

    Private sidebarHistoryTab As NoFocusButton
    Private sidebarMemoryTab As NoFocusButton
    Private sidebarList As ListBox
    Private sidebarEmptyLabel As Label

    Private angleButton As NoFocusButton
    Private formatButton As NoFocusButton

    Private memoryMenu As ContextMenuStrip
    Private navigationMenu As ContextMenuStrip
    Private trigonometryMenu As ContextMenuStrip
    Private functionMenu As ContextMenuStrip
    Private standardMenuItem As ToolStripMenuItem
    Private scientificMenuItem As ToolStripMenuItem

    Private ReadOnly scientificButtons As New Dictionary(Of String, NoFocusButton)

    Private displayFont As Font
    Private buttonFont As Font

    Private Const ThemeRegistryPath As String = "Software\MinimalCalculator"


    ' FORM LOAD

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.KeyPreview = True
        Me.WindowState = FormWindowState.Maximized

        displayFont = TextBox1.Font
        buttonFont = Button1.Font
        displayBox = TextBox1

        If String.IsNullOrWhiteSpace(displayBox.Text) Then displayBox.Text = "0"

        LoadThemePreference()
        BuildApplicationLayout()
        BuildNavigationMenu()
        BuildMemoryMenu()
        BuildScientificMenus()
        ApplyAppTheme()

        AddHandler SystemEvents.UserPreferenceChanged, AddressOf SystemThemeChanged

    End Sub


    ' BUILD COMPLETE APPLICATION LAYOUT

    Private Sub BuildApplicationLayout()

        appLayout = New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 1,
            .Margin = New Padding(0),
            .Padding = New Padding(0)
        }

        appLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        appLayout.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 0))

        calculatorLayout = New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 6,
            .Padding = New Padding(8),
            .Margin = New Padding(0)
        }

        calculatorLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 44))
        calculatorLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 35))
        calculatorLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 0))
        calculatorLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 38))
        calculatorLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 0))
        calculatorLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 65))

        BuildHeader()
        BuildDisplay()
        BuildAngleAndFormatControls()
        BuildMemoryPanel()
        BuildScientificToolbar()
        BuildStandardKeypad()
        BuildScientificKeypad()
        BuildSidebar()

        calculatorLayout.Controls.Add(headerPanel, 0, 0)
        calculatorLayout.Controls.Add(displayPanel, 0, 1)
        calculatorLayout.Controls.Add(topControlsPanel, 0, 2)
        calculatorLayout.Controls.Add(memoryPanel, 0, 3)
        calculatorLayout.Controls.Add(scientificToolbar, 0, 4)

        keypadHostPanel = New Panel With {
            .Dock = DockStyle.Fill,
            .Margin = New Padding(0)
        }

        keypadHostPanel.Controls.Add(standardKeypadPanel)
        keypadHostPanel.Controls.Add(scientificKeypadPanel)

        calculatorLayout.Controls.Add(keypadHostPanel, 0, 5)

        appLayout.Controls.Add(calculatorLayout, 0, 0)
        appLayout.Controls.Add(sidebarPanel, 1, 0)

        Me.Controls.Add(appLayout)
        appLayout.BringToFront()

        SetCalculatorMode(calculatorMode)

    End Sub


    ' HEADER

    Private Sub BuildHeader()

        headerPanel = New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 3,
            .RowCount = 1,
            .Margin = New Padding(0)
        }

        headerPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 48))
        headerPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        headerPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 90))

        menuButton = New NoFocusButton With {
            .Text = "☰",
            .Dock = DockStyle.Fill,
            .Margin = New Padding(2),
            .TabStop = False,
            .Font = New Font("Segoe UI Symbol", 12)
        }

        AddHandler menuButton.Click, AddressOf ShowNavigationMenu

        headerTitle = New Label With {
            .Text = calculatorMode,
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleLeft,
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .Padding = New Padding(8, 0, 0, 0)
        }

        historyButton = New NoFocusButton With {
            .Text = "History",
            .Dock = DockStyle.Fill,
            .Margin = New Padding(2),
            .TabStop = False,
            .Font = buttonFont
        }

        AddHandler historyButton.Click, AddressOf ShowHistory

        headerPanel.Controls.Add(menuButton, 0, 0)
        headerPanel.Controls.Add(headerTitle, 1, 0)
        headerPanel.Controls.Add(historyButton, 2, 0)

    End Sub


    ' DISPLAY

    Private Sub BuildDisplay()

        displayPanel = New Panel With {
            .Dock = DockStyle.Fill,
            .Margin = New Padding(0)
        }

        TextBox1.ReadOnly = True
        TextBox1.TabStop = False
        TextBox1.BorderStyle = BorderStyle.None
        TextBox1.TextAlign = HorizontalAlignment.Right
        TextBox1.Dock = DockStyle.Fill
        TextBox1.Font = displayFont
        TextBox1.Margin = New Padding(8)

        displayPanel.Controls.Add(TextBox1)

    End Sub


    ' DEG / RAD AND F-E ROW

    Private Sub BuildAngleAndFormatControls()

        topControlsPanel = New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 4,
            .RowCount = 1,
            .Margin = New Padding(0),
            .Visible = False
        }

        topControlsPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 76))
        topControlsPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 76))
        topControlsPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        topControlsPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 0))

        angleButton = CreateButton("DEG", "ANGLE")
        formatButton = CreateButton("F-E", "FORMAT")

        AddHandler angleButton.Click, AddressOf ToggleAngleMode
        AddHandler formatButton.Click, AddressOf ToggleScientificFormat

        topControlsPanel.Controls.Add(angleButton, 0, 0)
        topControlsPanel.Controls.Add(formatButton, 1, 0)

    End Sub


    Private Sub ToggleAngleMode(sender As Object, e As EventArgs)

        angleMode = If(angleMode = "DEG", "RAD", "DEG")
        angleButton.Text = angleMode

    End Sub


    Private Sub ToggleScientificFormat(sender As Object, e As EventArgs)

        scientificNotation = Not scientificNotation

        Dim value As Double

        If TryReadDisplay(value) Then
            displayBox.Text = FormatNumber(value)
        End If

        formatButton.Text = If(scientificNotation, "F-E ✓", "F-E")

    End Sub


    ' MEMORY PANEL: MC / MR / M+ / M- / MS / M∨

    Private Sub BuildMemoryPanel()

        memoryPanel = New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 6,
            .RowCount = 1,
            .Margin = New Padding(0)
        }

        For i As Integer = 1 To 6
            memoryPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F / 6))
        Next

        AddMemoryButton("MC", 0, AddressOf MemoryClear_Click)
        AddMemoryButton("MR", 1, AddressOf MemoryRecall_Click)
        AddMemoryButton("M+", 2, AddressOf MemoryAdd_Click)
        AddMemoryButton("M−", 3, AddressOf MemorySubtract_Click)
        AddMemoryButton("MS", 4, AddressOf MemoryStore_Click)

        Dim moreButton As NoFocusButton = CreateButton("M∨", "MEMORY_MENU")
        AddHandler moreButton.Click, AddressOf ShowMemoryMenu

        memoryPanel.Controls.Add(moreButton, 5, 0)

    End Sub


    Private Sub AddMemoryButton(textValue As String, column As Integer, handler As EventHandler)

        Dim btn As NoFocusButton = CreateButton(textValue, textValue)

        AddHandler btn.Click, handler
        memoryPanel.Controls.Add(btn, column, 0)

    End Sub


    Private Sub BuildMemoryMenu()

        memoryMenu = New ContextMenuStrip With {
            .ShowImageMargin = False,
            .Font = New Font("Segoe UI", 10)
        }

        Dim valueItem As New ToolStripMenuItem("Memory: 0") With {
            .Enabled = False
        }

        Dim recallItem As New ToolStripMenuItem("Recall memory")
        Dim clearItem As New ToolStripMenuItem("Clear memory")

        AddHandler memoryMenu.Opening,
            Sub(s, e)
                valueItem.Text = "Memory: " & FormatNumber(memoryValue)
                ApplyMenuTheme(memoryMenu)
            End Sub

        AddHandler recallItem.Click,
            Sub(s, e)
                displayBox.Text = FormatNumber(memoryValue)
                ResetPendingOperation()
            End Sub

        AddHandler clearItem.Click,
            Sub(s, e)
                memoryValue = 0
                RefreshSidebar()
            End Sub

        memoryMenu.Items.Add(valueItem)
        memoryMenu.Items.Add(New ToolStripSeparator())
        memoryMenu.Items.Add(recallItem)
        memoryMenu.Items.Add(clearItem)

    End Sub


    Private Sub ShowMemoryMenu(sender As Object, e As EventArgs)

        If memoryMenu Is Nothing Then Return

        Dim control As Control = DirectCast(sender, Control)
        memoryMenu.Show(control, 0, control.Height)

    End Sub


    Private Sub MemoryStore_Click(sender As Object, e As EventArgs)

        Dim value As Double

        If TryReadDisplay(value) Then
            memoryValue = value
            RefreshSidebar()
        End If

    End Sub


    Private Sub MemoryAdd_Click(sender As Object, e As EventArgs)

        Dim value As Double

        If TryReadDisplay(value) Then
            memoryValue += value
            RefreshSidebar()
        End If

    End Sub


    Private Sub MemorySubtract_Click(sender As Object, e As EventArgs)

        Dim value As Double

        If TryReadDisplay(value) Then
            memoryValue -= value
            RefreshSidebar()
        End If

    End Sub


    Private Sub MemoryRecall_Click(sender As Object, e As EventArgs)

        displayBox.Text = FormatNumber(memoryValue)
        ResetPendingOperation()

    End Sub


    Private Sub MemoryClear_Click(sender As Object, e As EventArgs)

        memoryValue = 0
        RefreshSidebar()

    End Sub


    ' SCIENTIFIC TOOLBAR

    Private Sub BuildScientificToolbar()

        scientificToolbar = New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 1,
            .Margin = New Padding(0),
            .Visible = False
        }

        scientificToolbar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        scientificToolbar.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))

        Dim trigButton As NoFocusButton = CreateButton("△ Trigonometry ▾", "TRIG_MENU")
        Dim functionButton As NoFocusButton = CreateButton("ƒ Function ▾", "FUNCTION_MENU")

        AddHandler trigButton.Click, AddressOf ShowTrigonometryMenu
        AddHandler functionButton.Click, AddressOf ShowFunctionMenu

        scientificToolbar.Controls.Add(trigButton, 0, 0)
        scientificToolbar.Controls.Add(functionButton, 1, 0)

    End Sub


    ' SCIENTIFIC DROPDOWN MENUS
    ' Menus are created once and reused instead of being disposed after every click.

    Private Sub BuildScientificMenus()

        trigonometryMenu = New ContextMenuStrip With {
            .ShowImageMargin = False,
            .Font = New Font("Segoe UI", 10)
        }

        functionMenu = New ContextMenuStrip With {
            .ShowImageMargin = False,
            .Font = New Font("Segoe UI", 10)
        }

    End Sub


    Private Sub ShowTrigonometryMenu(sender As Object, e As EventArgs)

        If trigonometryMenu Is Nothing OrElse trigonometryMenu.IsDisposed Then Return
        If Not TypeOf sender Is Control Then Return

        trigonometryMenu.Items.Clear()

        Dim names As String()

        If secondMode Then
            names = {"asin", "acos", "atan", "asinh", "acosh", "atanh"}
        Else
            names = {"sin", "cos", "tan", "sinh", "cosh", "tanh"}
        End If

        For Each functionName As String In names

            Dim item As New ToolStripMenuItem(functionName) With {
                .Tag = functionName
            }

            AddHandler item.Click, AddressOf ScientificMenuItem_Click
            trigonometryMenu.Items.Add(item)

        Next

        ApplyMenuTheme(trigonometryMenu)

        Dim control As Control = DirectCast(sender, Control)
        trigonometryMenu.Show(control, 0, control.Height)

    End Sub


    Private Sub ShowFunctionMenu(sender As Object, e As EventArgs)

        If functionMenu Is Nothing OrElse functionMenu.IsDisposed Then Return
        If Not TypeOf sender Is Control Then Return

        functionMenu.Items.Clear()

        Dim functions As New Dictionary(Of String, String) From {
            {"Square root", "sqrt"},
            {"Cube root", "cbrt"},
            {"Square", "square"},
            {"Cube", "cube"},
            {"Reciprocal", "reciprocal"},
            {"Absolute value", "absolute"},
            {"Factorial", "factorial"},
            {"10 to the power of x", "10power"},
            {"e to the power of x", "exp"},
            {"Common logarithm", "log"},
            {"Natural logarithm", "ln"}
        }

        For Each pair As KeyValuePair(Of String, String) In functions

            Dim item As New ToolStripMenuItem(pair.Key) With {
                .Tag = pair.Value
            }

            AddHandler item.Click, AddressOf ScientificMenuItem_Click
            functionMenu.Items.Add(item)

        Next

        ApplyMenuTheme(functionMenu)

        Dim control As Control = DirectCast(sender, Control)
        functionMenu.Show(control, 0, control.Height)

    End Sub


    Private Sub ScientificMenuItem_Click(sender As Object, e As EventArgs)

        Dim item As ToolStripMenuItem = TryCast(sender, ToolStripMenuItem)
        If item Is Nothing OrElse item.Tag Is Nothing Then Return

        ApplyUnaryFunction(CStr(item.Tag))

    End Sub


    ' STANDARD KEYPAD

    Private Sub BuildStandardKeypad()

        standardKeypadPanel = New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 4,
            .RowCount = 6,
            .Margin = New Padding(0),
            .Visible = True
        }

        For i As Integer = 1 To 4
            standardKeypadPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 25))
        Next

        For i As Integer = 1 To 6
            standardKeypadPanel.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F / 6))
        Next

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

        Button20.Text = "⌫"

        divide.Text = "÷"
        divide.Tag = "/"

        multiply.Text = "×"
        multiply.Tag = "*"

        minus.Text = "−"
        minus.Tag = "-"

        addition.Tag = "+"

        Button17.Tag = "%"
        Button18.Tag = "CE"
        Button19.Tag = "C"
        Button20.Tag = "⌫"

        Button1.Tag = "7"
        Button2.Tag = "8"
        Button3.Tag = "9"

        Button4.Tag = "4"
        Button5.Tag = "5"
        Button6.Tag = "6"

        Button7.Tag = "1"
        Button8.Tag = "2"
        Button9.Tag = "3"

        Button10.Tag = "+/−"
        Button11.Tag = "0"
        dot.Tag = "."
        Equals.Tag = "="

        For Each btn As Button In buttons
            AddHandler btn.Click, AddressOf StandardButton_Click
        Next

        standardKeypadPanel.Controls.Add(Button17, 0, 0)
        standardKeypadPanel.Controls.Add(Button18, 1, 0)
        standardKeypadPanel.Controls.Add(Button19, 2, 0)
        standardKeypadPanel.Controls.Add(Button20, 3, 0)

        standardKeypadPanel.Controls.Add(Button1, 0, 1)
        standardKeypadPanel.Controls.Add(Button2, 1, 1)
        standardKeypadPanel.Controls.Add(Button3, 2, 1)
        standardKeypadPanel.Controls.Add(divide, 3, 1)

        standardKeypadPanel.Controls.Add(Button4, 0, 2)
        standardKeypadPanel.Controls.Add(Button5, 1, 2)
        standardKeypadPanel.Controls.Add(Button6, 2, 2)
        standardKeypadPanel.Controls.Add(multiply, 3, 2)

        standardKeypadPanel.Controls.Add(Button7, 0, 3)
        standardKeypadPanel.Controls.Add(Button8, 1, 3)
        standardKeypadPanel.Controls.Add(Button9, 2, 3)
        standardKeypadPanel.Controls.Add(minus, 3, 3)

        standardKeypadPanel.Controls.Add(Button10, 0, 4)
        standardKeypadPanel.Controls.Add(Button11, 1, 4)
        standardKeypadPanel.Controls.Add(dot, 2, 4)
        standardKeypadPanel.Controls.Add(addition, 3, 4)

        standardKeypadPanel.Controls.Add(Equals, 0, 5)
        standardKeypadPanel.SetColumnSpan(Equals, 4)

    End Sub


    ' SCIENTIFIC KEYPAD - FIVE COLUMNS / SEVEN ROWS

    Private Sub BuildScientificKeypad()

        scientificKeypadPanel = New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 5,
            .RowCount = 7,
            .Margin = New Padding(0),
            .Visible = False
        }

        For i As Integer = 1 To 5
            scientificKeypadPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 20))
        Next

        For i As Integer = 1 To 7
            scientificKeypadPanel.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F / 7))
        Next

        Dim keys As String(,) = {
            {"2nd", "π", "e", "C", "⌫"},
            {"x²", "1/x", "|x|", "exp", "mod"},
            {"³√x", "(", ")", "n!", "÷"},
            {"xʸ", "7", "8", "9", "×"},
            {"10ˣ", "4", "5", "6", "−"},
            {"log", "1", "2", "3", "+"},
            {"ln", "+/−", "0", ".", "="}
        }

        For row As Integer = 0 To 6

            For column As Integer = 0 To 4

                Dim keyText As String = keys(row, column)
                Dim button As NoFocusButton = CreateButton(keyText, keyText)

                scientificKeypadPanel.Controls.Add(button, column, row)

                If keyText = "2nd" Then
                    scientificButtons("2nd") = button
                ElseIf keyText = "x²" Then
                    scientificButtons("x²") = button
                ElseIf keyText = "10ˣ" Then
                    scientificButtons("10ˣ") = button
                End If

            Next

        Next

    End Sub


    ' SIDEBAR - HISTORY / MEMORY

    Private Sub BuildSidebar()

        sidebarPanel = New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 1,
            .RowCount = 2,
            .Padding = New Padding(8),
            .Margin = New Padding(0),
            .Visible = False
        }

        sidebarPanel.RowStyles.Add(New RowStyle(SizeType.Absolute, 42))
        sidebarPanel.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

        Dim tabs As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .RowCount = 1,
            .Margin = New Padding(0)
        }

        tabs.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        tabs.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))

        sidebarHistoryTab = CreateButton("History", "SIDEBAR_HISTORY")
        sidebarMemoryTab = CreateButton("Memory", "SIDEBAR_MEMORY")

        AddHandler sidebarHistoryTab.Click,
            Sub(s, e)
                ShowSidebarHistory()
            End Sub

        AddHandler sidebarMemoryTab.Click,
            Sub(s, e)
                ShowSidebarMemory()
            End Sub

        tabs.Controls.Add(sidebarHistoryTab, 0, 0)
        tabs.Controls.Add(sidebarMemoryTab, 1, 0)

        sidebarList = New ListBox With {
            .Dock = DockStyle.Fill,
            .BorderStyle = BorderStyle.None,
            .Font = New Font("Segoe UI", 10),
            .IntegralHeight = False
        }

        sidebarEmptyLabel = New Label With {
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleCenter,
            .Text = "There's no history yet."
        }

        sidebarContentPanel = New Panel With {
            .Dock = DockStyle.Fill,
            .Margin = New Padding(0)
        }

        sidebarContentPanel.Controls.Add(sidebarList)
        sidebarContentPanel.Controls.Add(sidebarEmptyLabel)

        sidebarPanel.Controls.Add(tabs, 0, 0)
        sidebarPanel.Controls.Add(sidebarContentPanel, 0, 1)

        AddHandler sidebarList.DoubleClick, AddressOf SidebarList_DoubleClick

        ShowSidebarHistory()

    End Sub


    Private Sub ShowSidebarHistory()

        sidebarMode = "History"
        sidebarList.Items.Clear()

        For Each entry As String In calculationHistory
            sidebarList.Items.Add(entry)
        Next

        sidebarEmptyLabel.Visible = (calculationHistory.Count = 0)
        sidebarEmptyLabel.Text = "There's no history yet."
        sidebarList.Visible = (calculationHistory.Count > 0)

        UpdateSidebarTabs()

    End Sub


    Private Sub ShowSidebarMemory()

        sidebarMode = "Memory"
        sidebarList.Items.Clear()

        If memoryValue = 0 Then
            sidebarList.Items.Add("No memory stored")
        Else
            sidebarList.Items.Add("M = " & FormatNumber(memoryValue))
        End If

        sidebarEmptyLabel.Visible = False
        sidebarList.Visible = True

        UpdateSidebarTabs()

    End Sub


    Private Sub UpdateSidebarTabs()

        If sidebarHistoryTab Is Nothing OrElse sidebarMemoryTab Is Nothing Then Return

        Dim dark As Boolean = IsDarkModeActive()
        Dim selectedColor As Color = Color.FromArgb(0, 120, 215)
        Dim normalColor As Color = If(dark, Color.FromArgb(48, 55, 51), Color.FromArgb(235, 235, 235))

        sidebarHistoryTab.BackColor = If(sidebarMode = "History", selectedColor, normalColor)
        sidebarMemoryTab.BackColor = If(sidebarMode = "Memory", selectedColor, normalColor)

        sidebarHistoryTab.ForeColor = If(sidebarMode = "History" OrElse dark, Color.White, Color.Black)
        sidebarMemoryTab.ForeColor = If(sidebarMode = "Memory" OrElse dark, Color.White, Color.Black)

    End Sub


    Private Sub RefreshSidebar()

        If sidebarList Is Nothing Then Return

        If sidebarMode = "Memory" Then
            ShowSidebarMemory()
        Else
            ShowSidebarHistory()
        End If

    End Sub


    Private Sub SidebarList_DoubleClick(sender As Object, e As EventArgs)

        If sidebarMode = "History" AndAlso sidebarList.SelectedItem IsNot Nothing Then
            LoadHistoryResult(CStr(sidebarList.SelectedItem))
        End If

    End Sub


    ' KEYBOARD SUPPORT

    Private Sub Form1_KeyPress(sender As Object, e As KeyPressEventArgs) Handles MyBase.KeyPress

        If Char.IsDigit(e.KeyChar) Then
            AddNumber(e.KeyChar.ToString())
            e.Handled = True
            Return
        End If

        Select Case e.KeyChar
            Case "+"c : AddOperator("+")
            Case "-"c : AddOperator("−")
            Case "*"c : AddOperator("×")
            Case "/"c : AddOperator("÷")
            Case "."c, ","c : AddDecimalPoint()
            Case "%"c : ApplyUnaryFunction("percent")
            Case Else : Return
        End Select

        e.Handled = True

    End Sub


    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown

        Select Case e.KeyCode
            Case Keys.Enter : EvaluateExpression()
            Case Keys.Back : Backspace()
            Case Keys.Escape : ClearAll()
            Case Keys.Delete : ClearEntry()
            Case Else : Return
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


    ' STANDARD DESIGNER BUTTON ROUTER

    Private Sub StandardButton_Click(sender As Object, e As EventArgs)

        Dim action As String = CStr(DirectCast(sender, Button).Tag)

        Select Case action

            Case "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
                AddNumber(action)

            Case "."
                AddDecimalPoint()

            Case "+/−"
                ChangeSign()

            Case "+", "-", "*", "/"
                AddOperator(action)

            Case "%"
                ApplyUnaryFunction("percent")

            Case "CE"
                ClearEntry()

            Case "C"
                ClearAll()

            Case "⌫"
                Backspace()

            Case "="
                EvaluateExpression()

        End Select

    End Sub


    ' COMMON BUTTON FACTORY

    Private Function CreateButton(textValue As String, action As String) As NoFocusButton

        Dim button As New NoFocusButton With {
            .Text = textValue,
            .Tag = action,
            .Dock = DockStyle.Fill,
            .Margin = New Padding(3),
            .TabStop = False,
            .Font = buttonFont,
            .UseVisualStyleBackColor = False,
            .FlatStyle = FlatStyle.Flat
        }

        button.FlatAppearance.BorderSize = 1
        AddHandler button.Click, AddressOf DynamicButton_Click

        Return button

    End Function


    ' DYNAMIC SCIENTIFIC BUTTON ROUTER

    Private Sub DynamicButton_Click(sender As Object, e As EventArgs)

        Dim action As String = CStr(DirectCast(sender, Button).Tag)

        Select Case action

            Case "2nd"
                secondMode = Not secondMode
                UpdateSecondModeButtons()

            Case "π"
                SetConstant(Math.PI)

            Case "e"
                SetConstant(Math.E)

            Case "C"
                ClearAll()

            Case "⌫"
                Backspace()

            Case "x²", "x³"
                ApplyUnaryFunction(If(secondMode, "cube", "square"))

            Case "1/x"
                ApplyUnaryFunction("reciprocal")

            Case "|x|"
                ApplyUnaryFunction("absolute")

            Case "exp"
                ApplyUnaryFunction("exp")

            Case "mod"
                StartPendingOperation("mod")

            Case "³√x"
                ApplyUnaryFunction("cbrt")

            Case "(", ")"
                AddParenthesis(action)

            Case "n!"
                ApplyUnaryFunction("factorial")

            Case "÷", "×", "−", "+"
                AddOperator(action)

            Case "xʸ"
                StartPendingOperation("power")

            Case "10ˣ", "2ˣ"
                ApplyUnaryFunction(If(secondMode, "2power", "10power"))

            Case "log"
                ApplyUnaryFunction("log")

            Case "ln"
                ApplyUnaryFunction("ln")

            Case "7", "8", "9", "4", "5", "6", "1", "2", "3", "0"
                AddNumber(action)

            Case "+/−"
                ChangeSign()

            Case "."
                AddDecimalPoint()

            Case "="
                EvaluateExpression()

        End Select

    End Sub


    ' UPDATE SECOND-FUNCTION LABELS

    Private Sub UpdateSecondModeButtons()

        If scientificButtons.ContainsKey("2nd") Then
            scientificButtons("2nd").Text = If(secondMode, "2nd ✓", "2nd")
        End If

        If scientificButtons.ContainsKey("x²") Then
            scientificButtons("x²").Text = If(secondMode, "x³", "x²")
        End If

        If scientificButtons.ContainsKey("10ˣ") Then
            scientificButtons("10ˣ").Text = If(secondMode, "2ˣ", "10ˣ")
        End If

    End Sub


    ' NUMBER ENTRY

    Private Sub AddNumber(digit As String)

        If displayBox.Text = "Error" Then
            ClearAll()
        End If

        If pendingOperation <> String.Empty Then

            If Not pendingInputStarted OrElse displayBox.Text = "0" Then
                displayBox.Text = digit
            Else
                displayBox.Text &= digit
            End If

            pendingInputStarted = True
            Return

        End If

        If replaceDisplayOnNextDigit OrElse displayBox.Text = "0" OrElse displayBox.Text = String.Empty Then
            displayBox.Text = digit
            replaceDisplayOnNextDigit = False
        Else
            displayBox.Text &= digit
        End If

    End Sub


    Private Sub AddKeyboardNumber(number As String)
        AddNumber(number)
    End Sub


    Private Sub AddDecimalPoint()

        If displayBox.Text = "Error" Then ClearAll()

        If pendingOperation <> String.Empty Then

            If Not pendingInputStarted Then
                displayBox.Text = "0."
            ElseIf Not displayBox.Text.Contains(".") AndAlso Not displayBox.Text.Contains("E") Then
                displayBox.Text &= "."
            End If

            pendingInputStarted = True
            Return

        End If

        If replaceDisplayOnNextDigit OrElse displayBox.Text = String.Empty Then
            displayBox.Text = "0."
            replaceDisplayOnNextDigit = False
        ElseIf Not displayBox.Text.Contains(".") AndAlso Not displayBox.Text.Contains("E") Then
            displayBox.Text &= "."
        End If

    End Sub


    Private Sub AddOperator(symbol As String)

        If displayBox.Text = String.Empty OrElse displayBox.Text = "Error" Then Return

        If pendingOperation <> String.Empty Then

            If Not pendingInputStarted Then Return

            EvaluateExpression()
            If displayBox.Text = "Error" Then Return

        End If

        Dim op As String = symbol

        Select Case symbol
            Case "−" : op = "-"
            Case "×" : op = "*"
            Case "÷" : op = "/"
        End Select

        Dim lastChar As Char = displayBox.Text(displayBox.Text.Length - 1)

        If "+-*/×÷−".Contains(lastChar) Then
            displayBox.Text = displayBox.Text.Substring(0, displayBox.Text.Length - 1)
        End If

        displayBox.Text &= op
        replaceDisplayOnNextDigit = False

    End Sub


    Private Sub StartPendingOperation(operation As String)

        Dim value As Double
        If Not TryReadDisplay(value) Then Return

        pendingValue = value
        pendingOperation = operation
        pendingInputStarted = False
        displayBox.Text = "0"
        replaceDisplayOnNextDigit = False

    End Sub


    ' EVALUATE EXPRESSION

    Private Sub EvaluateExpression()

        If String.IsNullOrWhiteSpace(displayBox.Text) Then Return

        If pendingOperation <> String.Empty Then

            If Not pendingInputStarted Then Return

            Dim rightValue As Double

            If Not TryReadDisplay(rightValue) Then
                displayBox.Text = "Error"
                ResetPendingOperation()
                Return
            End If

            Dim expression As String =
                FormatNumber(pendingValue) &
                If(pendingOperation = "power", " ^ ", " mod ") &
                FormatNumber(rightValue)

            Try

                Dim result As Double

                If pendingOperation = "power" Then

                    result = Math.Pow(pendingValue, rightValue)

                Else

                    If rightValue = 0 Then Throw New DivideByZeroException()
                    result = pendingValue Mod rightValue

                End If

                If Double.IsNaN(result) OrElse Double.IsInfinity(result) Then
                    Throw New ArithmeticException()
                End If

                displayBox.Text = FormatNumber(result)
                RecordHistory(expression, displayBox.Text)
                replaceDisplayOnNextDigit = True

            Catch ex As Exception

                displayBox.Text = "Error"

            Finally

                ResetPendingOperation()

            End Try

            Return

        End If

        Dim expressionText As String = displayBox.Text

        Dim normalized As String =
            expressionText.Replace("×", "*").
                           Replace("÷", "/").
                           Replace("−", "-")

        Try

            Dim result As Double = Convert.ToDouble(
                New DataTable().Compute(normalized, Nothing),
                CultureInfo.CurrentCulture
            )

            If Double.IsNaN(result) OrElse Double.IsInfinity(result) Then
                Throw New ArithmeticException()
            End If

            displayBox.Text = FormatNumber(result)

            RecordHistory(expressionText, displayBox.Text)

            replaceDisplayOnNextDigit = True

        Catch ex As Exception

            displayBox.Text = "Error"

        End Try

    End Sub


    Private Sub ResetPendingOperation()

        pendingOperation = String.Empty
        pendingInputStarted = False

    End Sub


    Private Sub ClearAll()

        displayBox.Text = "0"

        ResetPendingOperation()

        replaceDisplayOnNextDigit = False

    End Sub


    Private Sub ClearEntry()

        displayBox.Text = "0"
        replaceDisplayOnNextDigit = False

        If pendingOperation <> String.Empty Then
            pendingInputStarted = False
        End If

    End Sub


    Private Sub Backspace()

        If displayBox.Text = "Error" OrElse displayBox.Text.Length <= 1 Then

            displayBox.Text = "0"

        Else

            displayBox.Text =
                displayBox.Text.Substring(0, displayBox.Text.Length - 1)

        End If

        replaceDisplayOnNextDigit = False

    End Sub


    Private Sub ChangeSign()

        Dim value As Double
        If Not TryReadDisplay(value) Then Return

        displayBox.Text = FormatNumber(-value)

        If pendingOperation <> String.Empty Then
            pendingInputStarted = True
        End If

        replaceDisplayOnNextDigit = False

    End Sub


    Private Sub AddParenthesis(value As String)

        If displayBox.Text = "Error" Then ClearAll()

        If value = "(" Then

            If displayBox.Text = "0" OrElse replaceDisplayOnNextDigit Then
                displayBox.Text = "("
            Else
                displayBox.Text &= "("
            End If

        Else

            displayBox.Text &= ")"

        End If

        replaceDisplayOnNextDigit = False

    End Sub


    Private Sub SetConstant(value As Double)

        Dim constantText As String = FormatNumber(value)

        If displayBox.Text = "0" OrElse displayBox.Text = String.Empty OrElse replaceDisplayOnNextDigit Then

            displayBox.Text = constantText

        Else

            Dim lastChar As Char = displayBox.Text(displayBox.Text.Length - 1)

            If "+-*/(×÷−".Contains(lastChar) Then
                displayBox.Text &= constantText
            Else
                displayBox.Text = constantText
            End If

        End If

        If pendingOperation <> String.Empty Then
            pendingInputStarted = True
        End If

        replaceDisplayOnNextDigit = False

    End Sub


    Private Function TryReadDisplay(ByRef value As Double) As Boolean

        Return Double.TryParse(
            displayBox.Text,
            NumberStyles.Float,
            CultureInfo.CurrentCulture,
            value
        )

    End Function


    Private Function FormatNumber(value As Double) As String

        If scientificNotation Then
            Return value.ToString("E6", CultureInfo.CurrentCulture)
        End If

        Return value.ToString("G15", CultureInfo.CurrentCulture)

    End Function


    ' SAVE CALCULATION HISTORY

    Private Sub RecordHistory(expression As String, result As String)

        calculationHistory.Insert(0, expression & " = " & result)

        If calculationHistory.Count > 100 Then
            calculationHistory.RemoveAt(calculationHistory.Count - 1)
        End If

        RefreshSidebar()

    End Sub


    ' SCIENTIFIC FUNCTION CALCULATIONS

    Private Sub ApplyUnaryFunction(action As String)

        Dim value As Double

        If Not TryReadDisplay(value) Then
            displayBox.Text = "Error"
            Return
        End If

        Dim original As String = displayBox.Text
        Dim result As Double

        Try

            Select Case action

                Case "sin"
                    result = Math.Sin(ToRadians(value))

                Case "cos"
                    result = Math.Cos(ToRadians(value))

                Case "tan"

                    Dim halfTurn As Double = If(angleMode = "DEG", 180.0, 2.0 * Math.PI)
                    Dim quarterTurn As Double = If(angleMode = "DEG", 90.0, Math.PI / 2.0)
                    Dim normalized As Double = value Mod halfTurn

                    If Math.Abs(Math.Abs(normalized) - quarterTurn) < 0.000000001 Then
                        Throw New ArithmeticException()
                    End If

                    result = Math.Tan(ToRadians(value))

                Case "asin"

                    If Math.Abs(value) > 1 Then Throw New ArithmeticException()

                    result = FromRadians(Math.Asin(value))

                Case "acos"

                    If Math.Abs(value) > 1 Then Throw New ArithmeticException()

                    result = FromRadians(Math.Acos(value))

                Case "atan"

                    result = FromRadians(Math.Atan(value))

                Case "sinh"

                    result = Math.Sinh(value)

                Case "cosh"

                    result = Math.Cosh(value)

                Case "tanh"

                    result = Math.Tanh(value)

                Case "asinh"

                    result = Math.Log(value + Math.Sqrt(value * value + 1))

                Case "acosh"

                    If value < 1 Then Throw New ArithmeticException()

                    result = Math.Log(value + Math.Sqrt(value * value - 1))

                Case "atanh"

                    If Math.Abs(value) >= 1 Then Throw New ArithmeticException()

                    result = 0.5 * Math.Log((1 + value) / (1 - value))

                Case "sqrt"

                    If value < 0 Then Throw New ArithmeticException()

                    result = Math.Sqrt(value)

                Case "cbrt"

                    result = Math.Sign(value) * Math.Pow(Math.Abs(value), 1.0 / 3.0)

                Case "square"

                    result = Math.Pow(value, 2)

                Case "cube"

                    result = Math.Pow(value, 3)

                Case "reciprocal"

                    If value = 0 Then Throw New DivideByZeroException()

                    result = 1.0 / value

                Case "absolute"

                    result = Math.Abs(value)

                Case "exp"

                    result = Math.Exp(value)

                Case "10power"

                    result = Math.Pow(10, value)

                Case "2power"

                    result = Math.Pow(2, value)

                Case "log"

                    If value <= 0 Then Throw New ArithmeticException()

                    result = Math.Log10(value)

                Case "ln"

                    If value <= 0 Then Throw New ArithmeticException()

                    result = Math.Log(value)

                Case "factorial"

                    If value < 0 OrElse
                       value <> Math.Truncate(value) OrElse
                       value > 170 Then

                        Throw New ArithmeticException()

                    End If

                    result = 1

                    For i As Integer = 2 To CInt(value)
                        result *= i
                    Next

                Case "percent"

                    result = value / 100.0

                Case Else

                    Return

            End Select

            If Double.IsNaN(result) OrElse Double.IsInfinity(result) Then
                Throw New ArithmeticException()
            End If

            displayBox.Text = FormatNumber(result)

            RecordHistory(action & "(" & original & ")", displayBox.Text)

            If pendingOperation <> String.Empty Then
                pendingInputStarted = True
            End If

            replaceDisplayOnNextDigit = True

        Catch ex As Exception

            displayBox.Text = "Error"

        End Try

    End Sub


    Private Function ToRadians(value As Double) As Double

        If angleMode = "DEG" Then Return value * Math.PI / 180.0

        Return value

    End Function


    Private Function FromRadians(value As Double) As Double

        If angleMode = "DEG" Then Return value * 180.0 / Math.PI

        Return value

    End Function


    ' NAVIGATION MENU - LARGER OPTIONS

    Private Sub BuildNavigationMenu()

        navigationMenu = New ContextMenuStrip With {
            .ShowImageMargin = False,
            .AutoSize = False,
            .Size = New Size(250, 145),
            .Font = New Font("Segoe UI", 12.0F),
            .Padding = New Padding(4)
        }

        standardMenuItem = New ToolStripMenuItem("Standard") With {
            .AutoSize = False,
            .Size = New Size(240, 42),
            .Padding = New Padding(12, 5, 12, 5)
        }

        scientificMenuItem = New ToolStripMenuItem("Scientific") With {
            .AutoSize = False,
            .Size = New Size(240, 42),
            .Padding = New Padding(12, 5, 12, 5)
        }

        Dim settingsItem As New ToolStripMenuItem("Settings") With {
            .AutoSize = False,
            .Size = New Size(240, 42),
            .Padding = New Padding(12, 5, 12, 5)
        }

        AddHandler standardMenuItem.Click,
            Sub(s, e)
                SetCalculatorMode("Standard")
            End Sub

        AddHandler scientificMenuItem.Click,
            Sub(s, e)
                SetCalculatorMode("Scientific")
            End Sub

        AddHandler settingsItem.Click, AddressOf ShowSettings

        navigationMenu.Items.Add(standardMenuItem)
        navigationMenu.Items.Add(scientificMenuItem)
        navigationMenu.Items.Add(settingsItem)

        UpdateNavigationMenu()

    End Sub


    Private Sub ShowNavigationMenu(sender As Object, e As EventArgs)

        UpdateNavigationMenu()
        navigationMenu.Show(menuButton, 0, menuButton.Height)

    End Sub


    Private Sub UpdateNavigationMenu()

        If navigationMenu Is Nothing Then Return

        standardMenuItem.Checked = (calculatorMode = "Standard")
        scientificMenuItem.Checked = (calculatorMode = "Scientific")

        ApplyMenuTheme(navigationMenu)

    End Sub


    Private Sub ApplyMenuTheme(menu As ContextMenuStrip)

        Dim dark As Boolean = IsDarkModeActive()

        menu.BackColor = If(dark, Color.FromArgb(40, 40, 40), Color.White)
        menu.ForeColor = If(dark, Color.White, Color.Black)

        For Each item As ToolStripItem In menu.Items
            item.BackColor = menu.BackColor
            item.ForeColor = menu.ForeColor
        Next

    End Sub


    ' SWITCH STANDARD / SCIENTIFIC MODE

    Private Sub SetCalculatorMode(mode As String)

        If mode <> "Standard" AndAlso mode <> "Scientific" Then Return

        calculatorMode = mode
        headerTitle.Text = mode

        Dim scientific As Boolean = (mode = "Scientific")

        topControlsPanel.Visible = scientific
        scientificToolbar.Visible = scientific
        standardKeypadPanel.Visible = Not scientific
        scientificKeypadPanel.Visible = scientific

        calculatorLayout.RowStyles(1).Height = If(scientific, 40, 42)
        calculatorLayout.RowStyles(2).Height = If(scientific, 34, 0)
        calculatorLayout.RowStyles(4).Height = If(scientific, 40, 0)
        calculatorLayout.RowStyles(5).Height = 58

        If scientific Then
            SetSidebarVisible(True)
            ShowSidebarHistory()
        Else
            SetSidebarVisible(False)
        End If

        UpdateNavigationMenu()
        calculatorLayout.PerformLayout()

    End Sub


    Private Sub SetSidebarVisible(visible As Boolean)

        sidebarVisible = visible
        sidebarPanel.Visible = visible

        appLayout.ColumnStyles(1).SizeType = SizeType.Absolute
        appLayout.ColumnStyles(1).Width = If(visible, 300, 0)

        appLayout.PerformLayout()

    End Sub


    ' HISTORY BUTTON AND RIGHT SIDEBAR

    Private Sub ShowHistory(sender As Object, e As EventArgs)

        If Not sidebarVisible Then SetSidebarVisible(True)

        ShowSidebarHistory()

    End Sub


    ' HISTORY RESULT

    Private Sub LoadHistoryResult(entry As String)

        Dim separator As Integer = entry.LastIndexOf(" = ")

        If separator < 0 Then Return

        displayBox.Text = entry.Substring(separator + 3)

        ResetPendingOperation()
        replaceDisplayOnNextDigit = True

    End Sub


    ' SETTINGS PAGE

    Private Sub ShowSettings(sender As Object, e As EventArgs)

        Using settingsWindow As New Form()

            settingsWindow.Text = "Settings"
            settingsWindow.Size = New Size(430, 455)
            settingsWindow.StartPosition = FormStartPosition.CenterParent
            settingsWindow.FormBorderStyle = FormBorderStyle.FixedDialog
            settingsWindow.MaximizeBox = False
            settingsWindow.MinimizeBox = False
            settingsWindow.ShowInTaskbar = False

            Dim layout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 1,
                .RowCount = 3,
                .Padding = New Padding(22)
            }

            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 55))
            layout.RowStyles.Add(New RowStyle(SizeType.Absolute, 42))
            layout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))

            Dim title As New Label With {
                .Text = "Settings",
                .Dock = DockStyle.Fill,
                .Font = New Font("Segoe UI", 19, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleLeft
            }

            Dim appearance As New Label With {
                .Text = "Appearance",
                .Dock = DockStyle.Fill,
                .Font = New Font("Segoe UI", 11, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleLeft
            }

            Dim card As New Panel With {
                .Dock = DockStyle.Fill
            }

            Dim appTheme As New Label With {
                .Text = "App theme",
                .Location = New Point(18, 15),
                .AutoSize = True,
                .Font = New Font("Segoe UI", 11, FontStyle.Bold)
            }

            Dim description As New Label With {
                .Text = "Select which app theme to display",
                .Location = New Point(18, 43),
                .AutoSize = True
            }

            Dim light As New RadioButton With {
                .Text = "Light",
                .Location = New Point(24, 95),
                .AutoSize = True
            }

            Dim dark As New RadioButton With {
                .Text = "Dark",
                .Location = New Point(24, 140),
                .AutoSize = True
            }

            Dim system As New RadioButton With {
                .Text = "Use system setting",
                .Location = New Point(24, 185),
                .AutoSize = True
            }

            Select Case themeMode
                Case "Light" : light.Checked = True
                Case "Dark" : dark.Checked = True
                Case Else : system.Checked = True
            End Select

            AddHandler light.CheckedChanged,
                Sub(s, args)

                    If light.Checked Then
                        SetThemeMode("Light")
                        ApplyWindowTheme(settingsWindow)
                    End If

                End Sub

            AddHandler dark.CheckedChanged,
                Sub(s, args)

                    If dark.Checked Then
                        SetThemeMode("Dark")
                        ApplyWindowTheme(settingsWindow)
                    End If

                End Sub

            AddHandler system.CheckedChanged,
                Sub(s, args)

                    If system.Checked Then
                        SetThemeMode("System")
                        ApplyWindowTheme(settingsWindow)
                    End If

                End Sub

            card.Controls.Add(appTheme)
            card.Controls.Add(description)
            card.Controls.Add(light)
            card.Controls.Add(dark)
            card.Controls.Add(system)

            layout.Controls.Add(title, 0, 0)
            layout.Controls.Add(appearance, 0, 1)
            layout.Controls.Add(card, 0, 2)

            settingsWindow.Controls.Add(layout)

            ApplyWindowTheme(settingsWindow)
            settingsWindow.ShowDialog(Me)

        End Using

    End Sub


    Private Sub ApplyWindowTheme(form As Form)

        Dim dark As Boolean = IsDarkModeActive()

        form.BackColor = If(dark, Color.FromArgb(30, 30, 30), Color.FromArgb(245, 245, 245))
        form.ForeColor = If(dark, Color.White, Color.Black)

        ApplyControlTheme(form, dark)

    End Sub


    Private Sub ApplyControlTheme(parent As Control, dark As Boolean)

        For Each ctrl As Control In parent.Controls

            If TypeOf ctrl Is Panel Then

                ctrl.BackColor = If(dark, Color.FromArgb(40, 48, 44), Color.FromArgb(235, 235, 235))

            ElseIf TypeOf ctrl Is Label OrElse TypeOf ctrl Is RadioButton Then

                ctrl.ForeColor = If(dark, Color.White, Color.FromArgb(30, 30, 30))

            End If

            If ctrl.HasChildren Then
                ApplyControlTheme(ctrl, dark)
            End If

        Next

    End Sub


    Private Sub SetThemeMode(mode As String)

        If mode <> "System" AndAlso mode <> "Dark" AndAlso mode <> "Light" Then Return

        themeMode = mode
        SaveThemePreference()
        ApplyAppTheme()

    End Sub


    Private Sub LoadThemePreference()

        themeMode = "System"

        Try

            Using key As RegistryKey = Registry.CurrentUser.OpenSubKey(ThemeRegistryPath, False)

                If key IsNot Nothing Then

                    Dim saved As String = Convert.ToString(key.GetValue("ThemeMode", "System"))

                    Select Case saved.Trim().ToLowerInvariant()
                        Case "dark" : themeMode = "Dark"
                        Case "light" : themeMode = "Light"
                        Case Else : themeMode = "System"
                    End Select

                End If

            End Using

        Catch

            themeMode = "System"

        End Try

    End Sub


    Private Sub SaveThemePreference()

        Try

            Using key As RegistryKey = Registry.CurrentUser.CreateSubKey(ThemeRegistryPath)

                If key IsNot Nothing Then
                    key.SetValue("ThemeMode", themeMode, RegistryValueKind.String)
                End If

            End Using

        Catch
            ' Current session theme remains active.
        End Try

    End Sub


    Private Function IsSystemDarkMode() As Boolean

        Try

            Using key As RegistryKey = Registry.CurrentUser.OpenSubKey(
                "Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"
            )

                If key Is Nothing Then Return False

                Dim value As Object = key.GetValue("SystemUsesLightTheme")

                If value Is Nothing Then Return False

                Return Convert.ToInt32(value) = 0

            End Using

        Catch

            Return False

        End Try

    End Function


    Private Function IsDarkModeActive() As Boolean

        Select Case themeMode
            Case "Dark" : Return True
            Case "Light" : Return False
            Case Else : Return IsSystemDarkMode()
        End Select

    End Function


    ' APPLY APP THEME

    Private Sub ApplyAppTheme()

        Dim dark As Boolean = IsDarkModeActive()

        Me.BackColor = If(dark, Color.FromArgb(25, 35, 30), Color.FromArgb(245, 245, 245))
        Me.ForeColor = If(dark, Color.White, Color.FromArgb(30, 30, 30))

        If displayBox IsNot Nothing Then

            displayBox.BackColor = If(dark, Color.FromArgb(22, 22, 22), Color.White)
            displayBox.ForeColor = If(dark, Color.White, Color.Black)

        End If

        ApplyCalculatorControlTheme(Me, dark)

        If sidebarList IsNot Nothing Then

            sidebarList.BackColor = If(dark, Color.FromArgb(22, 22, 22), Color.White)
            sidebarList.ForeColor = If(dark, Color.White, Color.Black)

        End If

        UpdateSidebarTabs()

        If navigationMenu IsNot Nothing Then ApplyMenuTheme(navigationMenu)
        If memoryMenu IsNot Nothing Then ApplyMenuTheme(memoryMenu)
        If trigonometryMenu IsNot Nothing Then ApplyMenuTheme(trigonometryMenu)
        If functionMenu IsNot Nothing Then ApplyMenuTheme(functionMenu)

    End Sub


    Private Sub ApplyCalculatorControlTheme(parent As Control, dark As Boolean)

        For Each ctrl As Control In parent.Controls

            If TypeOf ctrl Is TableLayoutPanel OrElse TypeOf ctrl Is Panel Then

                ctrl.BackColor = If(dark, Color.FromArgb(25, 35, 30), Color.FromArgb(245, 245, 245))

            End If

            If TypeOf ctrl Is Label Then

                ctrl.ForeColor = If(dark, Color.White, Color.FromArgb(30, 30, 30))

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

                Else

                    btn.BackColor = If(dark, Color.FromArgb(48, 55, 51), Color.FromArgb(235, 235, 235))
                    btn.ForeColor = If(dark, Color.White, Color.FromArgb(30, 30, 30))
                    btn.FlatAppearance.BorderColor = If(dark, Color.FromArgb(65, 75, 69), Color.FromArgb(200, 200, 200))

                End If

            End If

            If ctrl.HasChildren Then
                ApplyCalculatorControlTheme(ctrl, dark)
            End If

        Next

    End Sub


    ' THEME CHANGE EVENT

    Private Sub SystemThemeChanged(sender As Object, e As UserPreferenceChangedEventArgs)

        If Me.IsDisposed OrElse Not Me.IsHandleCreated Then Return
        If themeMode <> "System" Then Return

        Try

            Me.BeginInvoke(New MethodInvoker(AddressOf ApplyAppTheme))

        Catch ex As InvalidOperationException

            ' Form is closing.

        End Try

    End Sub


    Private Sub Form1_Activated(sender As Object, e As EventArgs) Handles MyBase.Activated

        If themeMode = "System" Then ApplyAppTheme()

    End Sub


    Private Sub Form1_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed

        RemoveHandler SystemEvents.UserPreferenceChanged, AddressOf SystemThemeChanged

        If navigationMenu IsNot Nothing Then navigationMenu.Dispose()
        If memoryMenu IsNot Nothing Then memoryMenu.Dispose()
        If trigonometryMenu IsNot Nothing Then trigonometryMenu.Dispose()
        If functionMenu IsNot Nothing Then functionMenu.Dispose()

    End Sub

End Class
