Attribute VB_Name = "BoschDatabaseMacros"
Option Explicit

' ============================================================================
' Bosch Product Database - VBA Macros Module
' ============================================================================
' Import this module into your Excel file:
' 1. Open Excel file
' 2. Press Alt+F11 (Visual Basic Editor)
' 3. File → Import File → Select this .vba file
' 4. Save workbook as .xlsm (macro-enabled)
' ============================================================================

' Paths - UPDATE THESE TO MATCH YOUR SETUP
Const SCRIPTS_PATH As String = "E:\CURSOR\__MATT MEDIA MANAGER ERP\Bosch Products DB in excel\"
Const EXCEL_PATH As String = "M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\Bosch_Product_Database.xlsm"
Const SCAN_BAT As String = "scan_database.bat"
Const POPULATE_BAT As String = "populate_excel.bat"

' ============================================================================
' Button Actions
' ============================================================================

Sub ScanDatabase()
    ' Runs scan_database.bat to scan JSONs and create products.json
    Dim batPath As String
    Dim cmdLine As String
    
    batPath = SCRIPTS_PATH & SCAN_BAT
    
    If Dir(batPath) = "" Then
        MsgBox "Script not found: " & batPath, vbExclamation
        Exit Sub
    End If
    
    Application.ScreenUpdating = False
    Application.StatusBar = "Scanning database..."
    
    ' Change to scripts directory and run batch file
    cmdLine = "cmd.exe /c cd /d """ & SCRIPTS_PATH & """ && """ & batPath & """"
    Shell cmdLine, vbNormalFocus
    
    MsgBox "Scanner started! Wait for it to complete, then click REFRESH EXCEL.", vbInformation
    Application.StatusBar = False
    Application.ScreenUpdating = True
End Sub

Sub RefreshExcel()
    ' Runs populate_excel.bat to update Excel from products.json
    Dim batPath As String
    Dim cmdLine As String
    
    batPath = SCRIPTS_PATH & POPULATE_BAT
    
    If Dir(batPath) = "" Then
        MsgBox "Script not found: " & batPath, vbExclamation
        Exit Sub
    End If
    
    ' Save and close this workbook first
    Application.DisplayAlerts = False
    ThisWorkbook.Save
    
    MsgBox "Excel will close and refresh. Reopen the file after the script completes.", vbInformation
    
    ' Change to scripts directory and run batch file
    cmdLine = "cmd.exe /c cd /d """ & SCRIPTS_PATH & """ && """ & batPath & """"
    Shell cmdLine, vbNormalFocus
    
    ' Close workbook
    ThisWorkbook.Close SaveChanges:=False
End Sub

Sub InsertPreviews()
    ' Inserts preview images into cells from file paths
    Dim ws As Worksheet
    Dim lastRow As Long
    Dim r As Long
    Dim previewPath As String
    Dim previewCol As Integer
    Dim cell As Range
    Dim shp As Shape
    Dim targetCell As Range
    
    Set ws = ActiveSheet
    previewCol = 8 ' COL_PRODUCT_PREVIEW
    
    ' Find last row
    lastRow = ws.Cells(ws.Rows.Count, 1).End(xlUp).Row
    
    If lastRow < 3 Then
        MsgBox "No products found!", vbExclamation
        Exit Sub
    End If
    
    Application.ScreenUpdating = False
    Application.StatusBar = "Inserting preview images..."
    
    Dim count As Integer
    count = 0
    
    ' Loop through data rows (starting from row 3)
    For r = 3 To lastRow
        Set targetCell = ws.Cells(r, previewCol)
        
        ' Get preview path from hyperlink or cell value
        previewPath = ""
        If targetCell.Hyperlinks.Count > 0 Then
            previewPath = Replace(targetCell.Hyperlinks(1).Address, "file:///", "")
            previewPath = Replace(previewPath, "/", "\")
        End If
        
        ' Insert image if file exists
        If previewPath <> "" And Dir(previewPath) <> "" Then
            ' Delete existing image if any
            On Error Resume Next
            For Each shp In ws.Shapes
                If Not Intersect(Range(shp.TopLeftCell.Address), targetCell) Is Nothing Then
                    shp.Delete
                End If
            Next shp
            On Error GoTo 0
            
            ' Insert new image
            On Error Resume Next
            Set shp = ws.Shapes.AddPicture( _
                Filename:=previewPath, _
                LinkToFile:=False, _
                SaveWithDocument:=True, _
                Left:=targetCell.Left + 2, _
                Top:=targetCell.Top + 2, _
                Width:=targetCell.Width - 4, _
                Height:=targetCell.Height - 4)
            
            If Err.Number = 0 Then
                ' Lock aspect ratio and fit to cell
                shp.LockAspectRatio = msoTrue
                If shp.Width > targetCell.Width - 4 Then
                    shp.Width = targetCell.Width - 4
                End If
                If shp.Height > targetCell.Height - 4 Then
                    shp.Height = targetCell.Height - 4
                End If
                count = count + 1
            End If
            On Error GoTo 0
        End If
        
        Application.StatusBar = "Inserting images... " & r - 2 & " of " & lastRow - 2
    Next r
    
    Application.StatusBar = False
    Application.ScreenUpdating = True
    
    MsgBox "Inserted " & count & " preview images!", vbInformation
End Sub

Sub ExportNotes()
    ' Exports product names and notes to CSV
    Dim ws As Worksheet
    Dim lastRow As Long
    Dim r As Long
    Dim csvPath As String
    Dim fso As Object
    Dim txtFile As Object
    Dim productName As String
    Dim notes As String
    Dim count As Integer
    
    Set ws = ActiveSheet
    
    ' Find last row
    lastRow = ws.Cells(ws.Rows.Count, 1).End(xlUp).Row
    
    If lastRow < 3 Then
        MsgBox "No products found!", vbExclamation
        Exit Sub
    End If
    
    ' Create CSV file
    csvPath = SCRIPTS_PATH & "product_notes_" & Format(Now, "yyyymmdd_hhmmss") & ".csv"
    
    Set fso = CreateObject("Scripting.FileSystemObject")
    Set txtFile = fso.CreateTextFile(csvPath, True)
    
    ' Write header
    txtFile.WriteLine "Product Name,Notes"
    
    count = 0
    ' Loop through data rows
    For r = 3 To lastRow
        productName = ws.Cells(r, 1).Value ' COL_PRODUCT_NAME
        notes = ws.Cells(r, 22).Value ' COL_NOTES
        
        If notes <> "" Then
            txtFile.WriteLine Chr(34) & productName & Chr(34) & "," & Chr(34) & notes & Chr(34)
            count = count + 1
        End If
    Next r
    
    txtFile.Close
    
    MsgBox "Exported " & count & " notes to: " & vbCrLf & csvPath, vbInformation
    
    ' Open folder
    Shell "explorer.exe /select," & csvPath, vbNormalFocus
End Sub

' ============================================================================
' Worksheet Events
' ============================================================================
' Add this to the Worksheet module (not this one):
'
' Private Sub Worksheet_SelectionChange(ByVal Target As Range)
'     If Target.Row = 1 And Target.Column >= 1 And Target.Column <= 12 Then
'         Select Case Target.Column
'             Case 1 To 3: ScanDatabase
'             Case 4 To 6: RefreshExcel
'             Case 7 To 9: InsertPreviews
'             Case 10 To 12: ExportNotes
'         End Select
'     End If
' End Sub
' ============================================================================

Sub CreateDropdowns()
    ' Creates data validation dropdowns for holder and graphic holder columns
    ' Run this once after populating Excel
    
    Dim ws As Worksheet
    Dim lastRow As Long
    
    Set ws = ActiveSheet
    lastRow = ws.Cells(ws.Rows.Count, 1).End(xlUp).Row
    
    If lastRow < 3 Then Exit Sub
    
    ' This is a placeholder - dropdowns will be populated dynamically
    ' based on available holders from the Holders folder
    
    MsgBox "Dropdown functionality will be implemented when scanning holder files.", vbInformation
End Sub
