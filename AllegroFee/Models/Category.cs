using Newtonsoft.Json;

namespace AllegroFee.Models;

public class Category
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("name")] public string Name { get; set; }
}