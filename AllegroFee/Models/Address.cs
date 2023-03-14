using Newtonsoft.Json;

namespace AllegroFee.Models;

public class Address
{
    [JsonProperty("city")] public string City { get; set; }

    [JsonProperty("postcode")] public string Postcode { get; set; }

    [JsonProperty("country")] public string Country { get; set; }

    [JsonProperty("street")] public string Street { get; set; }

    [JsonProperty("buildingNumber")] public string BuildingNumber { get; set; }
}