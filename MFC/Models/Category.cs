using Newtonsoft.Json;

namespace MFC.Models;

public class Category
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("name")] public string Name { get; set; }
}