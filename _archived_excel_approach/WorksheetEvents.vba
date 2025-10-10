' ============================================================================
' Worksheet Events - Paste this in the Worksheet Module
' ============================================================================
' Instructions:
' 1. Open VBA Editor (Alt+F11)
' 2. In Project Explorer, expand Microsoft Excel Objects
' 3. Double-click the worksheet (Sheet1 or "Product Database")
' 4. Paste this entire code
' ============================================================================

Option Explicit

Private Sub Worksheet_SelectionChange(ByVal Target As Range)
    ' Handle button clicks in row 1
    If Target.Row = 1 And Target.Column >= 1 And Target.Column <= 12 Then
        Application.EnableEvents = False
        Select Case Target.Column
            Case 1 To 3
                BoschDatabaseMacros.ScanDatabase
            Case 4 To 6
                BoschDatabaseMacros.RefreshExcel
            Case 7 To 9
                BoschDatabaseMacros.InsertPreviewThumbnails
            Case 10 To 12
                BoschDatabaseMacros.ExportNotes
        End Select
        Application.EnableEvents = True
    End If
End Sub

Private Sub Worksheet_Change(ByVal Target As Range)
    ' Auto-update when dropdown selections change
    Dim r As Long
    
    ' Only process if single cell changed
    If Target.Cells.Count > 1 Then Exit Sub
    
    ' Only process columns J (10) or K (11) - Holder Variant and Color
    If Target.Column <> 10 And Target.Column <> 11 Then Exit Sub
    
    ' Only process data rows (row 3 and below)
    If Target.Row < 3 Then Exit Sub
    
    ' Disable events to prevent recursion
    Application.EnableEvents = False
    Application.ScreenUpdating = False
    
    ' Update holder info for this row
    r = Target.Row
    BoschDatabaseMacros.UpdateHolderInfo r
    
    ' Re-enable events
    Application.EnableEvents = True
    Application.ScreenUpdating = True
End Sub
