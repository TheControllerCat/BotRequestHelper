using System;
using Newtonsoft.Json;

namespace BotRequestHelper.Models
{
    public struct RobotClientResponse
    {

        public int RobotId { get; set; }

        public double DistanceToGoal { get; set; }

        public int BatteryLevel { get; set; }

        public int Count { get; set; }

    }

}