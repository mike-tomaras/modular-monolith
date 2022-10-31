namespace Incepted.Shared.ValueTypes;

public  class ContentType
{
    private readonly IDictionary<string, string> _validContentTypes = 
        new Dictionary<string, string>()
        { 
            { ".pdf", "application/pdf" },
            { ".doc", "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".csv", "text/csv" },
            { ".txt", "text/plain" },
        };

    public string Value { get; private init; }

    public ContentType(string fileExtension)
    {
        if (string.IsNullOrEmpty(fileExtension)) 
            throw new ArgumentException("File extension can't be empty when getting content type", nameof(fileExtension));
        
        Value = GetContentType(fileExtension.ToLower());
    }

    private string GetContentType(string fileExtension)
    {
        if (!_validContentTypes.ContainsKey(fileExtension))
            throw new ArgumentException("File extension is not mapped to a valid content type", nameof(fileExtension));

        return _validContentTypes[fileExtension];
    }

    public override string ToString()
    {
        return Value;
    }
}
