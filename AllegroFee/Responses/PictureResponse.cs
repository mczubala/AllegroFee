using AllegroFee.Models;
using Newtonsoft.Json;

namespace AllegroFee.Responses;

public class PictureResponse
{
    [JsonProperty("pictures")] 
    public Pictures Pictures { get; set; }
}