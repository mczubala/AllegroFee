using Newtonsoft.Json;

namespace AllegroFee.Models;

public class Vendor
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("company")] public bool IsCompany { get; set; }

    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("address")] public Address Address { get; set; }

    [JsonProperty("phone")] public string Phone { get; set; }

    [JsonProperty("email")] public string Email { get; set; }

    [JsonProperty("rating")] public Rating Rating { get; set; }
}