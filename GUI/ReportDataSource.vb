
Namespace Microsoft.Reporting.WinForms
    Class ReportDataSource

        Private _totalProfitForAllTimeDataTable As POSDS.TotalProfitForAllTimeDataTable
        Private _p1 As String
        Private _itemsTotalDataTable As POSDS.ItemsTotalDataTable

        Sub New(ByVal p1 As String, ByVal totalProfitForAllTimeDataTable As POSDS.TotalProfitForAllTimeDataTable)
            ' TODO: Complete member initialization 
            _p1 = p1
            _totalProfitForAllTimeDataTable = totalProfitForAllTimeDataTable
        End Sub

        Sub New(ByVal p1 As String, ByVal itemsTotalDataTable As POSDS.ItemsTotalDataTable)
            ' TODO: Complete member initialization 
            _p1 = p1
            _itemsTotalDataTable = itemsTotalDataTable
        End Sub

    End Class
End Namespace
