using System;
using Newtonsoft.Json;

namespace BotRequestHelper.Models
{
    public class Robot
    {
        [JsonProperty("robotId")]
        public int RobotId { get; set; }

        [JsonProperty("batteryLevel")]
        public int BatteryLevel { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        [JsonProperty("x")]
        public int X { get; set; }
    }

}