using AllegroFee.Models;
using Newtonsoft.Json;

namespace AllegroFee.Responses;

public class ParametersResponse
{
    [JsonProperty("parameters")]
    public Parameters Parameters { get; set; }
}