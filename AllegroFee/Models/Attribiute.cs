using Newtonsoft.Json;
using YourProject.Controllers;

namespace AllegroFee.Models;

public class Attribute
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("values")] public List<AttributeValue> Values { get; set; }
}