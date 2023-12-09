using Newtonsoft.Json;

namespace MFC.Models;

public class Promotion
{
    [JsonProperty("emphasized")]
    public bool Emphasized { get; set; }
    [JsonProperty("bold")]
    public bool Bold { get; set; }
    [JsonProperty("highlight")]
    public bool Highlight { get; set; }
    [JsonProperty("departmentPage")]
    public bool DepartmentPage { get; set; }
    [JsonProperty("emphasizedHighlightBoldPackage")]
    public bool EmphasizedHighlightBoldPackage { get; set; }
}