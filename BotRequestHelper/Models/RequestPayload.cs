using System;
using Newtonsoft.Json;

namespace BotRequestHelper.Models
{
    public class RequestPayload
    {
        [JsonProperty("loadId")]
        public int LoadId { get; set; }

        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }
    }
}
