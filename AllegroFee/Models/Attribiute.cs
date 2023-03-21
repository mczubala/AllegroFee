using Newtonsoft.Json;
using AllegroFee.Controllers;

namespace AllegroFee.Models;

public class Attribute
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("values")] public List<AttributeValue> Values { get; set; }
}