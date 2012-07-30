Public Class MainWindow

    Dim ReceiptImage As Bitmap
    Dim CurrentReceipt As New Receipt


    ' the form loads and initialization should happen
    Private Sub MainWindow_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load


        Try


            ' get the password from the user
            Dim PSWWin As New PasswordPicker

            ' if the user hits the exit button then stop execution
            If PSWWin.ShowDialog <> Windows.Forms.DialogResult.OK Then
                End
            End If

            ' get the password
            Dim PSW As String = PSWWin.TextBox1.Text

            ' get the password from the database
            Dim TA As New POSDSTableAdapters.ValuesTableAdapter
            Dim TB = TA.GetDataByKey("password")
            Dim DBPSW As String = TB.Rows(0).Item(1)

            ' check that passwords match
            If PSW <> DBPSW Then
                MsgBox("invalid password", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
                End
            End If

            ' load the items information from db into the dataset
            ItemsTA.Fill(MyDataset.Items)

            ' load restorant tables into the dataset
            RestorantTableTA.Fill(MyDataset.RestorantTable)

            ' the printer name should appear
            Dim VTA As New POSDSTableAdapters.ValuesTableAdapter
            Dim Result = VTA.GetDataByKey("printer")
            TextBox5.Text = Result.Rows(0).Item(1) & ""

            Result = VTA.GetDataByKey("unit width")
            TextBox6.Text = Result.Rows(0).Item(1) & ""

            Result = VTA.GetDataByKey("unit height")
            TextBox7.Text = Result.Rows(0).Item(1) & ""

            Result = VTA.GetDataByKey("font size")
            TextBox8.Text = Result.Rows(0).Item(1) & ""

            ' fill the settings page with test data
            DGV3.Rows.Add("", "tea", "", 20, 30, 20 * 30)
            DGV3.Rows.Add("", "pen", "", 1, 12, 1 * 12)
            DGV3.Rows.Add("", "cup", "", 5, 7, 7 * 5)

            PB.Image = DrawReceipt(DGV3.Rows, 838, "2012-01-01", "Third Table", 123, TextBox6.Text, TextBox7.Text, TextBox8.Text)

        Catch ex As Exception

            ' handle the error
            MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            End
        End Try

        Me.RV.RefreshReport()
        Me.RV.RefreshReport()
    End Sub

    ' change the password
    Private Sub ChangePasswordToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChangePasswordToolStripMenuItem.Click
        Dim PSWChange As New ChangePassword
        PSWChange.ShowDialog()
    End Sub

    ' add item to the db
    Private Sub AddItemToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AddItemToolStripMenuItem.Click
        Dim AddItemWindow As New AddItem
        If AddItemWindow.ShowDialog = Windows.Forms.DialogResult.OK Then
            ' load the information of items from db
            ItemsTA.Fill(MyDataset.Items)
        End If
    End Sub

    ' used to select an item
    Private Sub EditItemToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles EditItemToolStripMenuItem.Click

        ' make sure an item is selected
        If DGV.SelectedRows.Count = 0 Then
            Exit Sub
        End If

        ' get the barcode of the item
        Dim Barcode = DGV.SelectedRows(0).Cells(0).Value

        ' create the edit window
        Dim EditItemWindow As New EditItem

        ' fill the window with information
        EditItemWindow.FillItemInfo(Barcode)

        If EditItemWindow.ShowDialog = Windows.Forms.DialogResult.OK Then
            ' load the information of items from db
            ItemsTA.Fill(MyDataset.Items)
        End If
    End Sub

    ' this one is used to remove an item
    Private Sub RemoveItemToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RemoveItemToolStripMenuItem.Click

        ' make sure a single item is being selected
        If DGV.SelectedRows.Count = 0 Then
            Exit Sub
        End If

        ' get the barcode of the item
        Dim Barcode As String = DGV.SelectedRows(0).Cells(0).Value

        ' remove the item
        Try
            ItemsTA.DeleteByBarcode(Barcode)
            ItemsTA.Fill(MyDataset.Items)
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
        End Try
    End Sub


    ' checks if the return key is pressed
    Private Sub TextBox1_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles TextBox1.KeyPress
        If Button1.Enabled Then
            If e.KeyChar = Chr(13) Then
                Button1_Click(Nothing, Nothing)
            End If
        End If
    End Sub


    ' this one is used to detect the barcode item when the text change and display its information
    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox1.TextChanged
        Try
            ' step 01: create the table adapter
            Dim TA As New POSDSTableAdapters.ItemsTableAdapter
            Dim TB = TA.GetDataByBarcode(TextBox1.Text)

            ' step 02: check if no item is found
            If TB.Rows.Count = 0 Then
                TextBox2.Text = ""
                TextBox3.Text = ""
                Button1.Enabled = False
                Exit Sub
            End If

            ' step 03: display the information in the textboxes
            Button1.Enabled = True
            Dim R As POS.POSDS.ItemsRow = TB.Rows(0)
            TextBox2.Text = R.ItemName
            TextBox3.Text = R.SellPrice
            Button1.Tag = R
        Catch ex As Exception
            ' display error message
            MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
        End Try
    End Sub


    ' this will be used to add an item to the recipt details
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        ' get the details of the item
        Dim R As POS.POSDS.ItemsRow = Button1.Tag


        ' add item to the receipt
        CurrentReceipt.AddReceiptDetail(R.Barcode)

        ' update db
        CurrentReceipt.UpdateDB()

        ' fill the gui
        FillGUI()

        TextBox1.Text = ""

    End Sub


    ' remove item from the recipt
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        If DGV2.SelectedRows.Count = 0 Then
            Exit Sub
        End If

        ' get barcode
        Dim Barcode = DGV2.SelectedRows(0).Cells(0).Value

        ' remove
        CurrentReceipt.RemoveItemFromReceipt(Barcode)


        CurrentReceipt.UpdateDB()

        ' display
        FillGUI()
    End Sub


    ' save the recipt
    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        If ComboBox2.Text = "" Then
            MsgBox("You should select a table", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            ComboBox2.Focus()
            Exit Sub
        End If

        CurrentReceipt.Paid = "yes"
        CurrentReceipt.ReceiptDate = Now
        CurrentReceipt.UpdateDB()


        ' use the printer
        If TextBox5.Text <> "" Then

            If PB.Image Is Nothing Then
                MsgBox("The settings for the receipt size is wrong, the receipt will no be printed", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            Else
                ' draw the receipt
                ReceiptImage = DrawReceipt(DGV2.Rows, CurrentReceipt.ReceiptNo, Format(Now.Date, "yyyy-MM-dd"), ComboBox2.Text, TextBox4.Text, TextBox6.Text, TextBox7.Text, TextBox8.Text)

                ' print the receipt
                PrintDoc.PrinterSettings.PrinterName = TextBox5.Text
                PrintDoc.Print()
            End If

        Else
            MsgBox("Du hast keinen Drucker für eingestellt!", MsgBoxStyle.OkOnly, "Warnig")
        End If


        CurrentReceipt = New Receipt

        CurrentReceipt.LoadReceipt(ComboBox2.Text)

        FillGUI()


    End Sub



    ' function Draw Receipt
    Public Function DrawReceipt(ByVal Rows As DataGridViewRowCollection, ByVal ReceiptNo As String, ByVal ReceiptDate As String, ByVal TableNo As String, ByVal ReceiptTotal As Decimal, ByVal UnitWidth As Integer, ByVal UnitHeight As Integer, ByVal FontSize As Integer) As Bitmap



        Dim ReceiptWidth As Integer = 13 * UnitWidth
        Dim ReceiptDetailsHeight As Integer = Rows.Count * UnitHeight
        Dim ReceiptHeight As Integer = 7 * UnitWidth + ReceiptDetailsHeight

        ' create the bitmap
        Dim BMP As New Bitmap(ReceiptWidth + 1, ReceiptHeight, Imaging.PixelFormat.Format24bppRgb)

        ' create the graphics object
        Dim GR As Graphics = Graphics.FromImage(BMP)

        ' fill the image with color white
        GR.Clear(Color.White)


        ' draw the basic lines

        ' draw the headers
        Dim LNHeaderYStart = 4 * UnitHeight
        Dim LNDetailsStart = LNHeaderYStart + UnitHeight

        GR.DrawRectangle(Pens.Black, UnitWidth * 0, LNHeaderYStart, UnitWidth, UnitHeight)
        GR.DrawRectangle(Pens.Black, UnitWidth * 1, LNHeaderYStart, UnitWidth * 5, UnitHeight)
        GR.DrawRectangle(Pens.Black, UnitWidth * 6, LNHeaderYStart, UnitWidth * 2, UnitHeight)
        GR.DrawRectangle(Pens.Black, UnitWidth * 8, LNHeaderYStart, UnitWidth * 2, UnitHeight)
        GR.DrawRectangle(Pens.Black, UnitWidth * 10, LNHeaderYStart, UnitWidth * 3, UnitHeight)


        ' draw the details part
        GR.DrawRectangle(Pens.Black, UnitWidth * 0, LNDetailsStart, UnitWidth * 1, ReceiptDetailsHeight)
        GR.DrawRectangle(Pens.Black, UnitWidth * 1, LNDetailsStart, UnitWidth * 5, ReceiptDetailsHeight)
        GR.DrawRectangle(Pens.Black, UnitWidth * 6, LNDetailsStart, UnitWidth * 2, ReceiptDetailsHeight)
        GR.DrawRectangle(Pens.Black, UnitWidth * 8, LNDetailsStart, UnitWidth * 2, ReceiptDetailsHeight)
        GR.DrawRectangle(Pens.Black, UnitWidth * 10, LNDetailsStart, UnitWidth * 3, ReceiptDetailsHeight)

        ' fill the header with some text
        Dim FNT As New Font("Times", FontSize, FontStyle.Bold)

        GR.DrawString("no", FNT, Brushes.Black, UnitWidth * 0, LNHeaderYStart)
        GR.DrawString("item", FNT, Brushes.Black, UnitWidth * 1, LNHeaderYStart)
        GR.DrawString("price", FNT, Brushes.Black, UnitWidth * 6, LNHeaderYStart)
        GR.DrawString("count", FNT, Brushes.Black, UnitWidth * 8, LNHeaderYStart)
        GR.DrawString("sum", FNT, Brushes.Black, UnitWidth * 10, LNHeaderYStart)


        ' final part is to render the text on the recipt
        Dim I As Integer
        For I = 0 To Rows.Count - 1

            ' find the y
            Dim YLOC = UnitHeight * I + LNDetailsStart

            ' render the values
            GR.DrawString(I + 1, FNT, Brushes.Black, UnitWidth * 0, YLOC)
            GR.DrawString(Rows(I).Cells(1).Value, FNT, Brushes.Black, UnitWidth * 1, YLOC)
            GR.DrawString(Rows(I).Cells(3).Value, FNT, Brushes.Black, UnitWidth * 6, YLOC)
            GR.DrawString(Rows(I).Cells(4).Value, FNT, Brushes.Black, UnitWidth * 8, YLOC)
            GR.DrawString(Rows(I).Cells(5).Value, FNT, Brushes.Black, UnitWidth * 10, YLOC)


        Next

        ' render the total
        GR.DrawString("Total:" & ReceiptTotal, FNT, Brushes.Black, 0, LNDetailsStart + ReceiptDetailsHeight)

        ' write the recipt no and the receipt date
        GR.DrawString("Receipt No:" & ReceiptNo, FNT, Brushes.Black, 0, 0)
        GR.DrawString("Receipt Date:" & ReceiptDate, FNT, Brushes.Black, 0, UnitHeight)
        GR.DrawString("Table No:" & TableNo, FNT, Brushes.Black, 0, UnitHeight * 2)


        ' end the drawing
        Return BMP
    End Function





    ' this one is used to print a document
    Private Sub PrintDoc_PrintPage(ByVal sender As System.Object, ByVal e As System.Drawing.Printing.PrintPageEventArgs) Handles PrintDoc.PrintPage
        e.Graphics.DrawImage(ReceiptImage, 0, 0, ReceiptImage.Width, ReceiptImage.Height)
        e.HasMorePages = False
    End Sub

    ' used to select a printer
    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        If PrintDLG.ShowDialog = Windows.Forms.DialogResult.Cancel Then
            Exit Sub
        End If

        TextBox5.Text = PrintDLG.PrinterSettings.PrinterName

        ' save the printer name in the database
        Try
            Dim VTA As New POSDSTableAdapters.ValuesTableAdapter
            VTA.UpdateDbVar(TextBox5.Text, "printer")
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
        End Try
    End Sub



    ' draw the receipt
    Private Sub TextBox6_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox6.TextChanged
        DrawReceiptPreview()

        If PB.Image IsNot Nothing Then
            Try
                Dim VTA As New POSDSTableAdapters.ValuesTableAdapter
                VTA.UpdateDbVar(TextBox6.Text, "unit width")
            Catch ex As Exception
                MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            End Try
        End If

    End Sub

    Public Sub DrawReceiptPreview()

        ' check the width is valid
        If Not IsNumeric(TextBox6.Text) Then
            PB.Image = Nothing
            Exit Sub
        End If

        Dim L As Double = Double.Parse(TextBox6.Text)
        If Math.Truncate(L) <> L Then
            PB.Image = Nothing
            Exit Sub
        End If

        If L <= 0 Then
            PB.Image = Nothing
            Exit Sub
        End If

        ' check the height is valid
        If Not IsNumeric(TextBox7.Text) Then
            PB.Image = Nothing
            Exit Sub
        End If

        L = Double.Parse(TextBox7.Text)
        If Math.Truncate(L) <> L Then
            PB.Image = Nothing
            Exit Sub
        End If

        If L <= 0 Then
            PB.Image = Nothing
            Exit Sub
        End If

        ' check the font size
        If Not IsNumeric(TextBox8.Text) Then
            PB.Image = Nothing
            Exit Sub
        End If

        L = Double.Parse(TextBox8.Text)
        If Math.Truncate(L) <> L Then
            PB.Image = Nothing
            Exit Sub
        End If

        If L <= 0 Then
            PB.Image = Nothing
            Exit Sub
        End If

        Try
            PB.Image = DrawReceipt(DGV3.Rows, 838, "2012-01-01", ComboBox2.Text, 123, TextBox6.Text, TextBox7.Text, TextBox8.Text)
        Catch ex As Exception
            PB.Image = Nothing
        End Try

    End Sub

    Private Sub TextBox7_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox7.TextChanged
        DrawReceiptPreview()
        If PB.Image IsNot Nothing Then
            Try
                Dim VTA As New POSDSTableAdapters.ValuesTableAdapter
                VTA.UpdateDbVar(TextBox7.Text, "unit height")
            Catch ex As Exception
                MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            End Try
        End If

    End Sub

    Private Sub TextBox8_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBox8.TextChanged
        DrawReceiptPreview()
        If PB.Image IsNot Nothing Then
            Try
                Dim VTA As New POSDSTableAdapters.ValuesTableAdapter
                VTA.UpdateDbVar(TextBox8.Text, "font size")
            Catch ex As Exception
                MsgBox(ex.Message, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            End Try
        End If

    End Sub

    Private Sub DGV3_CellEndEdit(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DGV3.CellEndEdit
        DrawReceiptPreview()
    End Sub

    ' save backup
    Private Sub BackupDBToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BackupDBToolStripMenuItem.Click
        Dim SFD As New SaveFileDialog
        SFD.Filter = "*.backup|*.backup"
        If SFD.ShowDialog = Windows.Forms.DialogResult.Cancel Then
            Exit Sub
        End If
        If BackupRestoreModule.SaveDB(SFD.FileName) Then
            MsgBox("Backup completed successfully", MsgBoxStyle.OkOnly Or MsgBoxStyle.Information, "OK")
        Else
            MsgBox("Unable to make backup", MsgBoxStyle.OkOnly Or MsgBoxStyle.Critical, "Error")
        End If
    End Sub

    Private Sub RestoreDBToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RestoreDBToolStripMenuItem.Click
        Dim OFD As New OpenFileDialog
        OFD.Filter = "*.backup|*.backup"
        If OFD.ShowDialog = Windows.Forms.DialogResult.Cancel Then
            Exit Sub
        End If
        If Not LoadDB(OFD.FileName) Then
            MsgBox("Error restoring the Database", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
        Else
            MsgBox("Restore successful", MsgBoxStyle.Information Or MsgBoxStyle.OkOnly, "OK")
            End
        End If
    End Sub

    ' this method is used to add a table
    Private Sub AddTableToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AddTableToolStripMenuItem.Click
        Dim NewTableName As String = InputBox("Enter the new table name/no")
        Dim RTA As New POSDSTableAdapters.RestorantTableTableAdapter
        Try
            RTA.Insert(NewTableName)
            RTA.Fill(Me.MyDataset.RestorantTable)
        Catch ex As Exception
            MsgBox("Unable of adding the new table", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            Exit Sub
        End Try
    End Sub

    ' this method is used to remove a tbale
    Private Sub RemoveTableToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RemoveTableToolStripMenuItem.Click
        ' check if there is a record
        If DGV4.SelectedRows.Count = 0 Then
            MsgBox("There are no tables to remove", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            Exit Sub
        End If

        Dim NewTableName As String = DGV4.SelectedRows(0).Cells(0).Value
        Dim RTA As New POSDSTableAdapters.RestorantTableTableAdapter
        Try
            RTA.Delete(NewTableName)
            RTA.Fill(Me.MyDataset.RestorantTable)
        Catch ex As Exception
            MsgBox("Unable of deleting the table", MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Error")
            Exit Sub
        End Try
    End Sub

    ' when a table is being selected, its receipt information should be displayed
    Private Sub ComboBox2_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox2.SelectedIndexChanged

        Dim TableNo As String
        TableNo = ComboBox2.Text

        If TableNo = "" Then
            Exit Sub
        End If

        ' load the receipt
        CurrentReceipt = New Receipt
        CurrentReceipt.LoadReceipt(TableNo)

        ' fill the gui
        FillGUI()

    End Sub

    ' this method is used to display receipt information on the window
    Public Sub FillGUI()
        TextBox4.Text = CurrentReceipt.ReceiptTotal
        CurrentReceipt.FillDGVWithReceiptInfo(DGV2)
    End Sub

    Private Sub MenuStrip1_ItemClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ToolStripItemClickedEventArgs) Handles MenuStrip1.ItemClicked

    End Sub

    Private Sub TablesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TablesToolStripMenuItem.Click

    End Sub
End Class
