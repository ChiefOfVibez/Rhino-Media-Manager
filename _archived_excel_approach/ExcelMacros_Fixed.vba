Attribute VB_Name = "BoschDatabaseMacros"
Option Explicit

' ============================================================================
' Bosch Product Database - Fixed VBA Macros Module
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
            previewPath = cell.Hyperlinks(1).Address
            ' Clean up the path
            previewPath = Replace(previewPath, "file:///", "")
            previewPath = Replace(previewPath, "/", "\")
        End If
        
        If previewPath <> "" And Dir(previewPath) <> "" Then
            ' Delete existing shapes in this cell
            On Error Resume Next
            Dim tempShp As Shape
            For Each tempShp In ws.Shapes
                If Not Intersect(Range(tempShp.TopLeftCell.Address), cell) Is Nothing Then
                    tempShp.Delete
                End If
            Next tempShp
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
                
                ' Make shape clickable - open full size image
                shp.OnAction = "OpenFullSizeImage"
                
                ' Store image path in shape name for later retrieval
                shp.Name = "Preview_Row" & r
                shp.AlternativeText = previewPath
                
                count = count + 1
            End If
            On Error GoTo 0
        End If
        
        Application.StatusBar = "Inserting thumbnails... " & r - 2 & " of " & lastRow - 2
    Next r
    
    Application.StatusBar = False
    Application.ScreenUpdating = True
    
    MsgBox "Inserted " & count & " preview thumbnails! Click images to view full size.", vbInformation
End Sub

Sub OpenFullSizeImage()
    ' Opens the full-size image when clicking thumbnail
    Dim shp As Shape
    Dim imagePath As String
    
    On Error Resume Next
    Set shp = ActiveSheet.Shapes(Application.Caller)
    
    If Not shp Is Nothing Then
        imagePath = shp.AlternativeText
        If imagePath <> "" And Dir(imagePath) <> "" Then
            ' Open image with default viewer
            Shell "explorer.exe """ & imagePath & """", vbNormalFocus
        End If
    End If
    On Error GoTo 0
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
' Auto-Update Functions - SIMPLIFIED APPROACH
' ============================================================================

Sub UpdateHolderInfo(ByVal targetRow As Long)
    ' Updates Cod Articol, Holder Preview, and Holder Link based on J+K selection
    ' Uses Python script to do the heavy lifting
    
    On Error GoTo ErrorHandler
    
    Dim ws As Worksheet
    Dim variant As String
    Dim color As String
    Dim productName As String
    Dim folderPath As String
    
    Set ws = ActiveSheet
    
    ' Get selected variant and color
    variant = Trim(ws.Cells(targetRow, COL_HOLDER_VARIANT).Value)
    color = Trim(ws.Cells(targetRow, COL_COLOR_VARIANT).Value)
    productName = Trim(ws.Cells(targetRow, 1).Value)
    folderPath = Trim(ws.Cells(targetRow, COL_FOLDER_PATH).Value)
    
    If variant = "" Or color = "" Or folderPath = "" Then
        Exit Sub
    End If
    
    ' Find JSON file
    Dim jsonFile As String
    jsonFile = Dir(folderPath & "\*.json")
    If jsonFile = "" Then
        MsgBox "JSON file not found in: " & folderPath, vbExclamation
        Exit Sub
    End If
    
    Dim jsonPath As String
    jsonPath = folderPath & "\" & jsonFile
    
    ' Read and parse JSON using simple text search
    Dim holders As Collection
    Set holders = ParseHoldersFromJSON(jsonPath)
    
    If holders Is Nothing Then
        MsgBox "Could not parse JSON file: " & jsonPath, vbExclamation
        Exit Sub
    End If
    
    ' Find matching holder
    Dim holder As Dictionary
    Dim i As Long
    Dim found As Boolean
    found = False
    
    For i = 1 To holders.Count
        Set holder = holders(i)
        If holder("variant") = variant And holder("color") = color Then
            found = True
            Exit For
        End If
    Next i
    
    If Not found Then
        MsgBox "Holder not found: " & variant & " " & color, vbExclamation
        Exit Sub
    End If
    
    ' Update Cod Articol
    ws.Cells(targetRow, COL_COD_ARTICOL).Value = holder("codArticol")
    
    ' Update Holder Preview link
    Dim previewPath As String
    previewPath = holder("preview")
    
    If previewPath <> "" And Dir(previewPath) <> "" Then
        ws.Cells(targetRow, COL_HOLDER_PREVIEW).ClearContents
        ws.Cells(targetRow, COL_HOLDER_PREVIEW).Hyperlinks.Delete
        
        ws.Hyperlinks.Add _
            Anchor:=ws.Cells(targetRow, COL_HOLDER_PREVIEW), _
            Address:=previewPath, _
            TextToDisplay:="🖼️ View"
        
        ws.Cells(targetRow, COL_HOLDER_PREVIEW).Font.color = RGB(5, 99, 193)
        ws.Cells(targetRow, COL_HOLDER_PREVIEW).Font.Underline = xlUnderlineStyleSingle
    End If
    
    ' Update Holder .3dm link
    Dim holderPath As String
    holderPath = holder("fullPath")
    
    If holderPath <> "" And Dir(holderPath) <> "" Then
        ws.Cells(targetRow, COL_OPEN_HOLDER).ClearContents
        ws.Cells(targetRow, COL_OPEN_HOLDER).Hyperlinks.Delete
        
        ws.Hyperlinks.Add _
            Anchor:=ws.Cells(targetRow, COL_OPEN_HOLDER), _
            Address:=holderPath, _
            TextToDisplay:="📦 Open .3dm"
        
        ws.Cells(targetRow, COL_OPEN_HOLDER).Font.color = RGB(5, 99, 193)
        ws.Cells(targetRow, COL_OPEN_HOLDER).Font.Underline = xlUnderlineStyleSingle
    Else
        MsgBox "Holder file not found: " & holderPath, vbExclamation
    End If
    
    Exit Sub
    
ErrorHandler:
    MsgBox "Error updating holder info: " & Err.Description & vbCrLf & _
           "Product: " & productName & vbCrLf & _
           "Variant: " & variant & vbCrLf & _
           "Color: " & color, vbCritical
End Sub

Function ParseHoldersFromJSON(jsonPath As String) As Collection
    ' Simple JSON parser for holders array
    ' Returns Collection of Dictionary objects
    
    On Error GoTo ErrorHandler
    
    Dim fso As Object
    Dim txtStream As Object
    Dim jsonText As String
    Dim holders As New Collection
    
    ' Read file
    Set fso = CreateObject("Scripting.FileSystemObject")
    Set txtStream = fso.OpenTextFile(jsonPath, 1)
    jsonText = txtStream.ReadAll
    txtStream.Close
    
    ' Find holders array
    Dim startPos As Long
    Dim endPos As Long
    Dim holdersText As String
    
    startPos = InStr(jsonText, """holders"":")
    If startPos = 0 Then
        Set ParseHoldersFromJSON = Nothing
        Exit Function
    End If
    
    ' Find the opening bracket
    startPos = InStr(startPos, jsonText, "[")
    If startPos = 0 Then
        Set ParseHoldersFromJSON = Nothing
        Exit Function
    End If
    
    ' Find matching closing bracket (simple approach)
    endPos = InStr(startPos, jsonText, "]")
    If endPos = 0 Then
        Set ParseHoldersFromJSON = Nothing
        Exit Function
    End If
    
    holdersText = Mid(jsonText, startPos + 1, endPos - startPos - 1)
    
    ' Split by objects
    Dim holderObjects() As String
    holderObjects = Split(holdersText, "},{")
    
    Dim i As Long
    For i = LBound(holderObjects) To UBound(holderObjects)
        Dim holderText As String
        holderText = holderObjects(i)
        
        ' Clean up
        holderText = Replace(holderText, "{", "")
        holderText = Replace(holderText, "}", "")
        
        ' Parse this holder object
        Dim holder As Object
        Set holder = CreateObject("Scripting.Dictionary")
        
        holder("variant") = ExtractJSONValue(holderText, "variant")
        holder("color") = ExtractJSONValue(holderText, "color")
        holder("codArticol") = ExtractJSONValue(holderText, "codArticol")
        holder("fullPath") = ExtractJSONValue(holderText, "fullPath")
        holder("preview") = ExtractJSONValue(holderText, "preview")
        
        holders.Add holder
    Next i
    
    Set ParseHoldersFromJSON = holders
    Exit Function
    
ErrorHandler:
    Set ParseHoldersFromJSON = Nothing
End Function

Function ExtractJSONValue(jsonText As String, fieldName As String) As String
    ' Extract value for a field from JSON text
    Dim searchStr As String
    Dim startPos As Long
    Dim endPos As Long
    Dim value As String
    
    searchStr = """" & fieldName & """:"
    startPos = InStr(jsonText, searchStr)
    
    If startPos = 0 Then
        ExtractJSONValue = ""
        Exit Function
    End If
    
    ' Find the opening quote of the value
    startPos = InStr(startPos + Len(searchStr), jsonText, """")
    If startPos = 0 Then
        ExtractJSONValue = ""
        Exit Function
    End If
    
    ' Find the closing quote
    endPos = InStr(startPos + 1, jsonText, """")
    If endPos = 0 Then
        ExtractJSONValue = ""
        Exit Function
    End If
    
    value = Mid(jsonText, startPos + 1, endPos - startPos - 1)
    
    ' Unescape common JSON escapes
    value = Replace(value, "\\", "\")
    value = Replace(value, "\/", "/")
    
    ExtractJSONValue = value
End Function
