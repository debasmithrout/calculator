Public Class NoFocusButton
    Inherits Button

    Public Sub New()
        MyBase.New()
        Me.TabStop = False
    End Sub

    Protected Overrides ReadOnly Property ShowFocusCues As Boolean
        Get
            Return False
        End Get
    End Property

End Class