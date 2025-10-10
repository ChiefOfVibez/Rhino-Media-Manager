Attribute VB_Name = "BoschDatabaseMacros"
Option Explicit

' ============================================================================
' Bosch Product Database - Enhanced VBA Macros Module
' ============================================================================

' Paths
Const SCRIPTS_PATH As String = "E:\CURSOR\__MATT MEDIA MANAGER ERP\Bosch Products DB in excel\"
Const EXCEL_PATH As String = "M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__\Bosch_Product_Database.xlsm"
Const SCAN_BAT As String = "scan_database.bat"
Const POPULATE_BAT As String = "populate_excel.bat"

' Column constants
Const COL_COD_ARTICOL As Integer = 4        ' D
Const COL_PRODUCT_PREVIEW As Integer = 8    ' H
Const COL_HOLDER_VARIANT As Integer = 10    ' J
Const COL_COLOR_VARIANT As Integer = 11     ' K
Const COL_HOLDER_PREVIEW As Integer = 12    ' L
Const COL_OPEN_HOLDER As Integer = 13       ' M
Const COL_FOLDER_PATH As Integer = 18       ' R (hidden)

' ============================================================================
' Button Actions
' ============================================================================

Sub ScanDatabase()
    Dim batPath As String
    Dim cmdLine As String
    
    batPath = SCRIPTS_PATH & SCAN_BAT
    
    If Dir(batPath) = "" Then
        MsgBox "Script not found: " & batPath, vbExclamation
        Exit Sub
    End If
    
    Application.ScreenUpdating = False
    Application.StatusBar = "Scanning database..."
    
    cmdLine = "cmd.exe /c cd /d """ & SCRIPTS_PATH & """ && """ & batPath & """"
    Shell cmdLine, vbNormalFocus
    
    MsgBox "Scanner started! Wait for it to complete, then click REFRESH EXCEL.", vbInformation
    Application.StatusBar = False
    Application.ScreenUpdating = True
End Sub

Sub RefreshExcel()
    Dim batPath As String
    Dim cmdLine As String
    
    batPath = SCRIPTS_PATH & POPULATE_BAT
    
    If Dir(batPath) = "" Then
        MsgBox "Script not found: " & batPath, vbExclamation
        Exit Sub
    End If
    
    Application.DisplayAlerts = False
    ThisWorkbook.Save
    
    MsgBox "Excel will close and refresh. Reopen the file after the script completes.", vbInformation
    
    cmdLine = "cmd.exe /c cd /d """ & SCRIPTS_PATH & """ && """ & batPath & """"
    Shell cmdLine, vbNormalFocus
    
    ThisWorkbook.Close SaveChanges:=False
End Sub

Sub InsertPreviewThumbnails()
    ' Inserts preview thumbnails in column H with clickable links
    Dim ws As Worksheet
    Dim lastRow As Long
    Dim r As Long
    Dim cell As Range
    Dim previewPath As String
    Dim shp As Shape
    Dim count As Integer
    
    Set ws = ActiveSheet
    lastRow = ws.Cells(ws.Rows.Count, 1).End(xlUp).Row
    
    If lastRow < 3 Then
        MsgBox "No products found!", vbExclamation
        Exit Sub
    End If
    
    Application.ScreenUpdating = False
    Application.StatusBar = "Inserting preview thumbnails..."
    
    count = 0
    
    For r = 3 To lastRow
        Set cell = ws.Cells(r, COL_PRODUCT_PREVIEW)
        
        ' Get preview path from hyperlink
        previewPath = ""
        If cell.Hyperlinks.Count > 0 Then
            previewPath = Replace(cell.Hyperlinks(1).Address, "file:///", "")
            previewPath = Replace(previewPath, "/", "\")
        End If
        
        If previewPath <> "" And Dir(previewPath) <> "" Then
            ' Delete existing shapes in this cell
            On Error Resume Next
            For Each shp In ws.Shapes
                If Not Intersect(Range(shp.TopLeftCell.Address), cell) Is Nothing Then
                    shp.Delete
                End If
            Next shp
            On Error GoTo 0
            
            ' Insert new thumbnail
            On Error Resume Next
            Set shp = ws.Shapes.AddPicture( _
                Filename:=previewPath, _
                LinkToFile:=False, _
                SaveWithDocument:=True, _
                Left:=cell.Left + 2, _
                Top:=cell.Top + 2, _
                Width:=-1, _
                Height:=-1)
            
            If Err.Number = 0 Then
                ' Resize to fit cell
                shp.LockAspectRatio = msoTrue
                If shp.Width > cell.Width - 4 Then
                    shp.Width = cell.Width - 4
                End If
                If shp.Height > cell.Height - 4 Then
                    shp.Height = cell.Height - 4
                End If
                
                ' Center in cell
                shp.Left = cell.Left + (cell.Width - shp.Width) / 2
                shp.Top = cell.Top + (cell.Height - shp.Height) / 2
                
                ' Keep hyperlink active by clearing cell value but keeping hyperlink
                cell.Value = ""
                
                count = count + 1
            End If
            On Error GoTo 0
        End If
        
        Application.StatusBar = "Inserting thumbnails... " & r - 2 & " of " & lastRow - 2
    Next r
    
    Application.StatusBar = False
    Application.ScreenUpdating = True
    
    MsgBox "Inserted " & count & " preview thumbnails!", vbInformation
End Sub

Sub ExportNotes()
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
    lastRow = ws.Cells(ws.Rows.Count, 1).End(xlUp).Row
    
    If lastRow < 3 Then
        MsgBox "No products found!", vbExclamation
        Exit Sub
    End If
    
    csvPath = SCRIPTS_PATH & "product_notes_" & Format(Now, "yyyymmdd_hhmmss") & ".csv"
    
    Set fso = CreateObject("Scripting.FileSystemObject")
    Set txtFile = fso.CreateTextFile(csvPath, True)
    
    txtFile.WriteLine "Product Name,Notes"
    
    count = 0
    For r = 3 To lastRow
        productName = ws.Cells(r, 1).Value
        notes = ws.Cells(r, 17).Value ' COL_NOTES
        
        If notes <> "" Then
            txtFile.WriteLine Chr(34) & productName & Chr(34) & "," & Chr(34) & notes & Chr(34)
            count = count + 1
        End If
    Next r
    
    txtFile.Close
    
    MsgBox "Exported " & count & " notes to: " & vbCrLf & csvPath, vbInformation
    Shell "explorer.exe /select," & csvPath, vbNormalFocus
End Sub

' ============================================================================
' Auto-Update Functions
' ============================================================================

Sub UpdateHolderInfo(ByVal targetRow As Long)
    ' Updates Cod Articol, Holder Preview, and Holder Link based on J+K selection
    ' This reads the JSON file to get holder data
    
    Dim ws As Worksheet
    Dim variant As String
    Dim color As String
    Dim folderPath As String
    Dim jsonPath As String
    Dim jsonContent As String
    Dim fso As Object
    Dim txtStream As Object
    
    Set ws = ActiveSheet
    
    ' Get selected variant and color
    variant = ws.Cells(targetRow, COL_HOLDER_VARIANT).Value
    color = ws.Cells(targetRow, COL_COLOR_VARIANT).Value
    
    ' Get folder path (hidden column R)
    folderPath = ws.Cells(targetRow, COL_FOLDER_PATH).Value
    
    If variant = "" Or color = "" Or folderPath = "" Then Exit Sub
    
    ' Find JSON file in folder
    jsonPath = Dir(folderPath & "\*.json")
    If jsonPath = "" Then Exit Sub
    
    jsonPath = folderPath & "\" & jsonPath
    
    ' Read JSON file
    Set fso = CreateObject("Scripting.FileSystemObject")
    Set txtStream = fso.OpenTextFile(jsonPath, 1) ' 1 = ForReading
    jsonContent = txtStream.ReadAll
    txtStream.Close
    
    ' Parse JSON and find matching holder
    ' This is a simple string search - for production, use proper JSON parser
    Dim codArticol As String
    Dim holderPreview As String
    Dim holderFullPath As String
    
    ' Extract holder data (simplified parsing)
    codArticol = ExtractHolderData(jsonContent, variant, color, "codArticol")
    holderPreview = ExtractHolderData(jsonContent, variant, color, "preview")
    holderFullPath = ExtractHolderData(jsonContent, variant, color, "fullPath")
    
    ' Update cells
    ws.Cells(targetRow, COL_COD_ARTICOL).Value = codArticol
    
    ' Update holder preview link
    If holderPreview <> "" Then
        ws.Cells(targetRow, COL_HOLDER_PREVIEW).ClearContents
        ws.Cells(targetRow, COL_HOLDER_PREVIEW).Hyperlinks.Delete
        ws.Cells(targetRow, COL_HOLDER_PREVIEW).Value = "🖼️ View"
        ws.Hyperlinks.Add Anchor:=ws.Cells(targetRow, COL_HOLDER_PREVIEW), _
            Address:=holderPreview, _
            TextToDisplay:="🖼️ View"
        ws.Cells(targetRow, COL_HOLDER_PREVIEW).Font.color = RGB(5, 99, 193)
        ws.Cells(targetRow, COL_HOLDER_PREVIEW).Font.Underline = xlUnderlineStyleSingle
    End If
    
    ' Update holder .3dm link
    If holderFullPath <> "" Then
        ws.Cells(targetRow, COL_OPEN_HOLDER).ClearContents
        ws.Cells(targetRow, COL_OPEN_HOLDER).Hyperlinks.Delete
        ws.Cells(targetRow, COL_OPEN_HOLDER).Value = "📦 Open .3dm"
        ws.Hyperlinks.Add Anchor:=ws.Cells(targetRow, COL_OPEN_HOLDER), _
            Address:=holderFullPath, _
            TextToDisplay:="📦 Open .3dm"
        ws.Cells(targetRow, COL_OPEN_HOLDER).Font.color = RGB(5, 99, 193)
        ws.Cells(targetRow, COL_OPEN_HOLDER).Font.Underline = xlUnderlineStyleSingle
    End If
End Sub

Function ExtractHolderData(jsonContent As String, variant As String, color As String, fieldName As String) As String
    ' Simple JSON parsing to extract holder data
    ' Finds the holder object matching variant and color, then extracts the field
    
    Dim startPos As Long
    Dim endPos As Long
    Dim holderBlock As String
    Dim searchStr As String
    Dim fieldValue As String
    
    ' Find holder block with matching variant and color
    searchStr = """variant"": """ & variant & """"
    startPos = InStr(jsonContent, searchStr)
    
    If startPos > 0 Then
        ' Check if this holder also has matching color
        Dim colorStr As String
        colorStr = """color"": """ & color & """"
        
        ' Extract holder block (simplified - assumes holder is within next 500 chars)
        holderBlock = Mid(jsonContent, startPos, 500)
        
        If InStr(holderBlock, colorStr) > 0 Then
            ' Extract field value
            searchStr = """" & fieldName & """: """
            startPos = InStr(holderBlock, searchStr)
            
            If startPos > 0 Then
                startPos = startPos + Len(searchStr)
                endPos = InStr(startPos, holderBlock, """")
                
                If endPos > startPos Then
                    fieldValue = Mid(holderBlock, startPos, endPos - startPos)
                    ExtractHolderData = fieldValue
                    Exit Function
                End If
            End If
        End If
    End If
    
    ExtractHolderData = ""
End Function
