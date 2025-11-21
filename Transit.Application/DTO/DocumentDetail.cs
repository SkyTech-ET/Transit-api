namespace Transit.Application;

public class DocumentDetail
{
    public long Id { get; set; }
    public string? BachNumber { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string VendorAddress { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomFields { get; set; } = string.Empty;// dictionary of custom fields
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public double SubTotal { get; set; }
    public double Tax { get; set; }
    public double Total { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public long OrganizationId { get; set; }
    public DateTime AnalysisStartTime { get; set; }
    public DateTime AnalysisEndTime { get; set; }
    public double CategorizeSeconds { get; set; }
    public int PagesCount { get; set; }
    public List<DocumentColumnsDTO> DocumentColumns { get; set; } = new List<DocumentColumnsDTO>();

}


public class DocumentColumnsDTO
{
    public string ColumnName { get; set; }
    public string ColumnValue { get; set; }
}

public class DocumentResponse
{
    public string BachNumber { get; set; }
    public List<DocumentDetail> Documents { get; set; } = new List<DocumentDetail>();

}

public class TableDocumentResponse
{
    public string BachNumber { get; set; }
    public List<TableDocumentDetail> Documents { get; set; } = new List<TableDocumentDetail>();

}

public class DocumentDetailOpenAI
{
    public long Id { get; set; }
    public string Category { get; set; }
    public string VendorName { get; set; }
    public string VendorAddress { get; set; }
    public string CustomerName { get; set; }
    public string CustomerId { get; set; }
    public string CustomerAddress { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public double SubTotal { get; set; }
    public double Tax { get; set; }
    public double Total { get; set; }
    public string InvoiceNumber { get; set; }
    public string PurchaseOrderNumber { get; set; }
    public string? KeyValuePairs { get; set; }
    public Dictionary<string, object> CustomColumns { get; set; } = new Dictionary<string, object>();
}


public class DictionaryEntry
{
    public string Key { get; set; }
    public string Value { get; set; }
}

public class TableDocumentDetail
{
    public long Id { get; set; }
    public string? FileName { get; set; }
    public string BatchNumber { get; set; }
    public long OrganizationId { get; set; }
    public DateTime AnalysisStartDate { get; set; }
    public DateTime AnalysisEndDate { get; set; }
    public int PagesCount { get; set; }

    public ICollection<TableDTO> Tables { get; set; }
}

public class TableDTO
{
    public long Id { get; set; }
    private string[] _tableHeaders;
    private string[][] _contents;
    private string[][] _summarizedValue;
    private string[][] _categorizedValue;

    // Property that filters out null values when setting TableHeaders
    public string[] TableHeaders
    {
        get => _tableHeaders;
        set => _tableHeaders = value?.Where(header => header != null).ToArray(); // Remove null values
    }
    public string[][] Contents
    {
        get => _contents; // Getter for Contents
        set => _contents = value ?? Array.Empty<string[]>(); // Initialize to an empty jagged array if null
    }

    public string[][] SummarizedValue
    {
        get => _summarizedValue; // Getter for Contents
        set => _summarizedValue = value ?? Array.Empty<string[]>(); // Initialize to an empty jagged array if null
    }

    public string[][] Category
    {
        get => _categorizedValue; // Getter for Contents
        set => _categorizedValue = value ?? Array.Empty<string[]>(); // Initialize to an empty jagged array if null
    }
    public string Summary { get; set; }
    public string Confidences { get; set; }
    public string[] CustomColumns { get; set; }
}

public class TableData
{
    private string[] _tableHeaders;
    private string[][] _contents;

    // Property that filters out null values when setting TableHeaders
    public string[] TableHeaders
    {
        get => _tableHeaders;
        set => _tableHeaders = value?.Where(header => header != null).ToArray(); // Remove null values
    }
    public string[][] Contents
    {
        get => _contents; // Getter for Contents
        set => _contents = value ?? Array.Empty<string[]>(); // Initialize to an empty jagged array if null
    }

}

public class TableDetailResult
{
    private string[] _tableHeaders;
    private string[][] _contents;

    // Property that filters out null values when setting TableHeaders
    public string[] TableHeaders
    {
        get => _tableHeaders;
        set => _tableHeaders = value?.Where(header => header != null).ToArray(); // Remove null values
    }

    public string[][] RowContent
    {
        get => _contents; // Getter for Contents
        set => _contents = value ?? Array.Empty<string[]>(); // Initialize to an empty jagged array if null
    }
}
