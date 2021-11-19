'======================================================VisualMonsters.cba.pl========================================================
Public Class Form1
    'Main board size (pitch size)
    Dim pHeight As Integer = 7
    Dim pWidth As Integer = 11
    'The variable holds currently selected panel
    Dim CurrentlySelectedPanel As Panel
    'variable stores game graphics
    Dim PitchBitmap As Bitmap
    Dim PitchGrapfic As Graphics

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'We set the size and position of the playing field
        Main_panel.Size = New Size(pWidth * 49 + 50, pHeight * 49 + 50)
        Main_panel.Location = New Point((Panel1.Width - Main_panel.Width) / 2, (Panel1.Height - Main_panel.Height) / 2)
        'We create a bitmap and generate graphics on it
        PitchBitmap = New Bitmap(Main_panel.Width, Main_panel.Height)
        PitchGrapfic = Graphics.FromImage(PitchBitmap)

        'Generate playground graphics
        GeneratePitchLine()
        'Generates active game elements (points on the board to click)
        GenerateMainPanels()

        'Selects the center of the board as the starting point
        CurrentlySelectedPanel = LocationList(Math.Ceiling((pWidth - 1) / 2))(Math.Ceiling((pHeight) / 2))
        'Activates the points where we can pass the ball
        UnlockPanel(CurrentlySelectedPanel)
        'Adds the goal panels
        AddGoal()

        Dim start As New System.Drawing.SolidBrush(System.Drawing.Color.Black) ' StartPoin color
        PitchGrapfic.FillEllipse(start, New Rectangle(CurrentlySelectedPanel.Location.X + 5, CurrentlySelectedPanel.Location.Y + 5, 10, 10))
        'Gives background to our picturebox (our graphics)
        Main_panel.Image = PitchBitmap
    End Sub

#Region "Generate pitch graphic"
    Private Sub GeneratePitchLine()
        'Pitch center
        Dim pCenter_W As Integer = Math.Floor((pHeight) / 2)
        Dim pCenter_H As Integer = Math.Ceiling((pWidth) / 2)

        'pitch lines
        Dim thinLine As Pen = New Pen(Color.FromArgb(191, 218, 229), 1) 'thin line
        Dim ThickLine As Pen = New Pen(Color.White, 3) 'thick line

        'pitch background
        Dim pitchColor As New System.Drawing.SolidBrush(System.Drawing.Color.MediumSeaGreen)
        PitchGrapfic.FillRectangle(pitchColor, New Rectangle(0, 0, PitchBitmap.Width, PitchBitmap.Height))

        'Generate lines
        For i As Integer = 0 To pWidth + 1
            For j As Integer = 0 To pHeight + 1
                PitchGrapfic.DrawLine(thinLine, 0, i * 49, Main_panel.Width, i * 49)
                PitchGrapfic.DrawLine(thinLine, i * 49, 0, i * 49, Main_panel.Height)
            Next
        Next
        'Generate sidelines
        PitchGrapfic.DrawLine(ThickLine, 49, 0, Main_panel.Width - 49, 0)
        PitchGrapfic.DrawLine(ThickLine, 49, (pHeight + 1) * 49, Main_panel.Width - 49, (pHeight + 1) * 49)

        PitchGrapfic.DrawLine(ThickLine, 49, 0, 49, pCenter_W * 49)
        PitchGrapfic.DrawLine(ThickLine, Main_panel.Width - 49, 0, Main_panel.Width - 49, pCenter_W * 49)
        PitchGrapfic.DrawLine(ThickLine, 49, (pHeight + 1 - pCenter_W) * 49, 49, (pHeight + 1) * 49)
        PitchGrapfic.DrawLine(ThickLine, Main_panel.Width - 49, (pHeight + 1 - pCenter_W) * 49, Main_panel.Width - 49, (pHeight + 1) * 49)

        PitchGrapfic.DrawLine(ThickLine, 0, pCenter_W * 49, 49, pCenter_W * 49)
        PitchGrapfic.DrawLine(ThickLine, 0, (pCenter_W + 2) * 49, 49, (pCenter_W + 2) * 49)
        PitchGrapfic.DrawLine(ThickLine, Main_panel.Width - 49, pCenter_W * 49, Main_panel.Width, pCenter_W * 49)
        PitchGrapfic.DrawLine(ThickLine, Main_panel.Width - 49, (pCenter_W + 2) * 49, Main_panel.Width, (pCenter_W + 2) * 49)

        PitchGrapfic.DrawLine(ThickLine, pCenter_H * 49, 0, pCenter_H * 49, (pHeight + 1) * 49)

        'generate pitch points
        Dim pola As New System.Drawing.SolidBrush(Color.FromArgb(150, Color.White))
        Dim aut As New System.Drawing.SolidBrush(System.Drawing.Color.White)
        For i As Integer = 1 To pWidth
            For j As Integer = 0 To pHeight + 1
                If i = 1 Or i = pWidth Or j = 0 Or j = (pHeight + 1) Then
                    If j = Math.Ceiling(pHeight / 2) Then
                        PitchGrapfic.FillEllipse(pola, New Rectangle(i * 49 - 5, j * 49 - 5, 10, 10))
                    Else
                        PitchGrapfic.FillEllipse(aut, New Rectangle(i * 49 - 5, j * 49 - 5, 10, 10))
                    End If
                Else
                    PitchGrapfic.FillEllipse(pola, New Rectangle(i * 49 - 5, j * 49 - 5, 10, 10))
                End If
            Next
        Next

        'generate goal points
        Dim bramka As New System.Drawing.SolidBrush(System.Drawing.Color.Red)
        PitchGrapfic.FillEllipse(bramka, New Rectangle(0 - 5, (pCenter_W + 1) * 49 - 5, 10, 10))
        PitchGrapfic.FillEllipse(bramka, New Rectangle((pWidth + 1) * 49 - 5, (pCenter_W + 1) * 49 - 5, 10, 10))
    End Sub
#End Region

#Region "Generate pitch points"
    'Keeps lists of all fields
    Dim LocationList As New List(Of List(Of Panel))
    'Keeps lists of sidelines points
    Dim SpecialLocationList As New List(Of Panel)
    'keeps special panels at the goal
    Dim SpecialGoalLocationList_leftGoal As New List(Of Panel)
    Dim SpecialGoalLocationList_RightGoal As New List(Of Panel)

    Private Sub GenerateMainPanels()
        For i As Integer = 1 To pWidth
            Dim PanelList As New List(Of Panel)
            For j As Integer = 0 To pHeight + 1
                Dim ManiPanel As New Panel
                With ManiPanel
                    .Location = New Point(i * 49 - 10, j * 49 - 10)
                    .Size = New Size(20, 20)
                    .BackColor = Color.Transparent
                    .Name = "panL_" + i.ToString + j.ToString
                    .Cursor = Cursors.Hand
                    AddHandler .Click, AddressOf pan_Click
                End With

                If i = 1 Or i = pWidth Or j = 0 Or j = (pHeight + 1) Then
                    If j = Math.Ceiling(pHeight / 2) Then
                        PanelList.Add(ManiPanel) 'Adds a single panel at the goal (bright green color in the tutorial picture)
                    Else
                        'Add sidepanels (green color in the tutorial picture)
                        If (i = 1 And j = 0) Or (i = 1 And j = (pHeight + 1)) Or (i = pWidth And j = 0) Or (i = pWidth And j = (pHeight + 1)) Then
                            ManiPanel.Visible = False
                            ManiPanel.Enabled = False
                            'If the panel is on the corner of the pitch, we block him
                        End If
                        SpecialLocationList.Add(ManiPanel)
                        PanelList.Add(ManiPanel)
                    End If
                Else
                    PanelList.Add(ManiPanel) 'Adds all panels (red color in the tutorial image)
                End If
                'Adds points at the goal (yellow color in the tutorial image)
                If i = 1 Or i = pWidth Or j = 0 Or j = (pHeight + 1) Then
                    If j = Math.Ceiling(pHeight / 2) Or j = Math.Ceiling(pHeight / 2) - 1 Or j = Math.Ceiling(pHeight / 2) + 1 Then
                        If i = 1 Then
                            SpecialGoalLocationList_leftGoal.Add(ManiPanel)
                        Else
                            SpecialGoalLocationList_RightGoal.Add(ManiPanel)
                        End If

                    End If
                End If
                Main_panel.Controls.Add(ManiPanel) ' place elements at the pitch
            Next
            LocationList.Add(PanelList)
        Next
    End Sub
#End Region


#Region "Add goals"
    Dim Goal_1 As New Panel
    Dim Goal_2 As New Panel

    Private Sub AddGoal()

        Dim Mid As Integer = Math.Floor((pHeight) / 2)
        With Goal_1
            .Location = New Point(0 - 10, (Mid + 1) * 49 - 10)
            .Size = New Size(20, 20)
            .BackColor = Color.Transparent
            .Name = "gol1"
            .Cursor = Cursors.Hand
            .Visible = False
            AddHandler .Click, AddressOf Goal_Click
        End With
        With Goal_2
            .Location = New Point((pWidth + 1) * 49 - 10, (Mid + 1) * 49 - 10)
            .Size = New Size(20, 20)
            .BackColor = Color.Transparent
            .Name = "gol2"
            .Cursor = Cursors.Hand
            .Visible = False
            AddHandler .Click, AddressOf Goal_Click
        End With

        Main_panel.Controls.Add(Goal_1)
        Main_panel.Controls.Add(Goal_2)
    End Sub

    Public Sub Goal_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Main_panel.Enabled = False 'block main panel
        DrawLine(DirectCast(sender, Panel))
        MsgBox("You won!!")
    End Sub
#End Region

    Public Sub pan_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        UnlockPanel(DirectCast(sender, Panel))
        DirectCast(sender, Panel).Visible = False
        DrawLine(DirectCast(sender, Panel))
    End Sub


    Dim Count As Integer ' variable check if you have place to move
    Dim LinesList As New List(Of Tuple(Of Panel, Panel))

    Private Sub DrawLine(ByVal pane As Panel)

        PitchBitmap = New Bitmap(Main_panel.Width, Main_panel.Height)
        PitchGrapfic = Graphics.FromImage(PitchBitmap)

        CurrentlySelectedPanel.Visible = False

        GeneratePitchLine()
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'Unlock the goal of the ball is in nearest zone.
        If SpecialGoalLocationList_RightGoal.Contains(pane) Then
            Goal_2.Visible = True
        Else
            Goal_2.Visible = False
        End If
        If SpecialGoalLocationList_leftGoal.Contains(pane) Then
            Goal_1.Visible = True
        Else
            Goal_1.Visible = False
        End If
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

        If SpecialLocationList.Contains(pane) Then
            'block special panels when the ball is in the sideline
            Dim mojindeks As Integer = SpecialLocationList.IndexOf(pane)
            If Not mojindeks - 2 < 0 Then
                SpecialLocationList(mojindeks - 2).Visible = False
            End If
            SpecialLocationList(mojindeks + 2).Visible = False
            SpecialLocationList(mojindeks - 1).Visible = False
            SpecialLocationList(mojindeks + 1).Visible = False
            Count -= 2
        End If
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

        LinesList.Add(Tuple.Create(CurrentlySelectedPanel, pane))
        ' Blocks linked panels (ie those lines are already added)
        For i As Integer = 0 To LinesList.Count - 1
            If LinesList(i).Item1.Name = pane.Name Or LinesList(i).Item2.Name = pane.Name Then
                LinesList(i).Item2.Visible = False
                LinesList(i).Item1.Visible = False
                Count -= 1
            End If
        Next

        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Dim myPen As Pen = New Pen(Color.Black, 2) ' kolor lini już dodanych
        'Draws already added lines
        For i As Integer = 0 To LinesList.Count - 1
            PitchGrapfic.DrawLine(myPen, LinesList(i).Item1.Location.X + 10, LinesList(i).Item1.Location.Y + 10, LinesList(i).Item2.Location.X + 10, LinesList(i).Item2.Location.Y + 10)
        Next
        PitchGrapfic.DrawLine(myPen, CurrentlySelectedPanel.Location.X + 10, CurrentlySelectedPanel.Location.Y + 10, pane.Location.X + 10, pane.Location.Y + 10)
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        'draw the ball
        PitchGrapfic.FillEllipse(New System.Drawing.SolidBrush(Color.Gray), New Rectangle(pane.Location.X + 5, pane.Location.Y + 5, 10, 10))
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        Main_panel.Image = PitchBitmap
        CurrentlySelectedPanel = pane
        ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        If Count <= 1 Then
            MessageBox.Show("You have no place to move, " + " you lost the game!!", " You have no place to move !!")
            Main_panel.Enabled = False
        End If
    End Sub

    Private Sub UnlockPanel(ByRef WybranyPanel As Panel)
        Dim pozycjax As Integer = 0
        Dim pozycjay As Integer = 0
        Count = 0
        'Get the current panel position
        For i As Integer = 0 To LocationList.Count - 1
            For j As Integer = 0 To LocationList(i).Count - 1
                If LocationList(i)(j).Name = WybranyPanel.Name Then
                    pozycjax = i
                    pozycjay = j
                End If
            Next
        Next
        'It unlocks the nearby panels, then draws the method and puts restrictions
        For i As Integer = 0 To LocationList.Count - 1
            For j As Integer = 0 To LocationList(i).Count - 1
                If i >= pozycjax - 1 And i <= pozycjax + 1 Then
                    If j >= pozycjay - 1 And j <= pozycjay + 1 Then
                        LocationList(i)(j).Visible = True
                        Count += 1
                    Else
                        LocationList(i)(j).Visible = False
                    End If
                Else
                    LocationList(i)(j).Visible = False
                End If
            Next
        Next
    End Sub
End Class
