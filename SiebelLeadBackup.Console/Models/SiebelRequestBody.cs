using System.Text.Json.Serialization;

namespace SiebelLeadBackup.Console.Models;

public class SiebelRequestBody
{
    [JsonPropertyName("Body")]
    public SiebelBodyContent Body { get; set; } = new();
}

public class SiebelBodyContent
{
    [JsonPropertyName("Freetext1")]
    public string Freetext1 { get; set; } = string.Empty;
    
    [JsonPropertyName("Freetext2")]
    public string Freetext2 { get; set; } = string.Empty;
    
    [JsonPropertyName("Freetext3")]
    public string Freetext3 { get; set; } = string.Empty;
    
    [JsonPropertyName("Freetext4")]
    public string Freetext4 { get; set; } = string.Empty;
    
    [JsonPropertyName("Freetext5")]
    public string Freetext5 { get; set; } = string.Empty;
    
    [JsonPropertyName("Freetext6")]
    public string Freetext6 { get; set; } = string.Empty;
    
    [JsonPropertyName("Freetext7")]
    public string Freetext7 { get; set; } = string.Empty;
    
    [JsonPropertyName("Freetext8")]
    public string Freetext8 { get; set; } = string.Empty;
    
    [JsonPropertyName("Freetext9")]
    public string Freetext9 { get; set; } = string.Empty;
}
