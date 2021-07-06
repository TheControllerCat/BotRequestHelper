using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using System.Json;
using BotRequestHelper.Models;
using Microsoft.AspNetCore.Http;


namespace BotRequestHelper
{
    [Route("api/BrhController")]
    [ApiController]
    public class BrhController : Controller
    {
        //==================================================
        // Class Scope Variables
        //==================================================

        private readonly int debugFlag = 0; // +++++DEBUG DEV FLAG, 1=TRUE, 0=FALSE +++++
        private readonly int closeRangeDist = 10;

        //==================================================
        // Generate Random Test Coords
        //==================================================

        private int GetRandCoord()
        {
            int returnVal = 0;

            Random rnd = new Random();

            returnVal = rnd.Next(1, 100);

            return returnVal;
        }

        //==================================================
        // Validate JSON
        //==================================================

        private bool ValidateJson(string psJsonString)
        {
            bool returnVal = false;

            try
            {
                var tempObj = JsonValue.Parse(psJsonString);
                returnVal = true;
            }
            catch (FormatException)
            {
                returnVal = false;
            }
            catch (Exception) //some other exception
            {
                returnVal = false;
            }

            return returnVal;
        }

        //==================================================
        // Format Output
        //==================================================

        private string FormatResponse(int RobotId, double DistanceToGoal, int BatteryLevel)
        {
            string returnVal = "{\"robotId\":\"" + RobotId.ToString()
                            + "\",\"distanceToGoal\":\"" + DistanceToGoal.ToString()
                            + "\",\"batteryLevel\":\"" + BatteryLevel.ToString()
                            + "\"}";
            if (debugFlag == 1)
            {
                returnVal += "\r";
            }

            return returnVal;
        }

        //==================================================
        // Calulate Distance
        //==================================================

        private double CalcRobotDist(int x1, int x2, int y1, int y2)
        {
            var returnVal = Math.Round(Math.Sqrt((Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2))), 2);

            return returnVal;
        }

        //==================================================
        // Parse Form Input
        //==================================================

        private int ParseFormRequestInput(string psFieldName)
        {
            int returnVal = 0;

            if (HttpContext.Request.Query["" + psFieldName + ""].ToString() != "")
            {
                returnVal = Convert.ToInt32(HttpContext.Request.Query["" + psFieldName + ""].ToString());
            }

            return returnVal;
        }

        //==================================================
        // Query List of Robots
        //==================================================

        private string getRobotsRaw()
        {
            string returnVal = "";

            // Create a request for the URL.
            WebRequest request = WebRequest.Create(
              "https://60c8ed887dafc90017ffbd56.mockapi.io/robots");
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;

            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);

            // Get the stream containing content returned by the server.
            // The using block ensures the stream is automatically closed.
            using (Stream dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                // Display the content.

                returnVal = responseFromServer;
            }

            // Close the response.
            response.Close();

            //------------------------------------------------------------
            // Test if Robots JSON is valid.
            //------------------------------------------------------------

            if (!ValidateJson(returnVal))
            { returnVal += " ERROR INVALID JSON!"; }

            return returnVal;
        }

        //==================================================
        // Main Body
        //==================================================

        private string HelpRobots()
        {

            //------------------------------------------------------------
            // Local Variables
            //------------------------------------------------------------

            string returnVal = ""; //The string dump for the output collection.

            int loadId; //Arbitrary ID of the load which needs to be moved.
            int loadX; //Current x coordinate of the load which needs to be moved.
            int loadY; //Current y coordinate of the load which needs to be moved.

            //------------------------------------------------------------
            // Request Input
            //------------------------------------------------------------

            if (debugFlag == 1)
            {
                // Get Random integers for the coordinate testing.
                loadId = 231;
                loadX = GetRandCoord();
                loadY = GetRandCoord();

                returnVal += "loadId:" + loadId + ",loadX:" + loadX + ",loadY:" + loadY + "\r";
            }
            else
            {
                // Parse and Validate the client request input.
                loadId = ParseFormRequestInput("loadId");
                loadX = ParseFormRequestInput("x");
                loadY = ParseFormRequestInput("y");
            }

            //------------------------------------------------------------
            // Query JSON List of Robots
            //------------------------------------------------------------

            string RawJsonString = getRobotsRaw();

            //------------------------------------------------------------
            // Convert Raw JSON string to Object
            //------------------------------------------------------------

            // Convert raw text Json into List of Robot objects
            var tempJsonObj = JsonConvert.DeserializeObject<List<Robot>>(RawJsonString);

            // Set up the empty RobotClientResponse Struct List
            List<RobotClientResponse> listRobotClientResponse = new List<RobotClientResponse>();

            //------------------------------------------------------------
            // Loop Through the Available Robots Into A List
            //------------------------------------------------------------

            // Manual counter for debugging.
            int tempCount = 0;

            //Loop through List of Robot Objects that contains the Robot Json feed.
            foreach (var e in tempJsonObj)
            {
                tempCount++;

                // Set up a single instance of the RobotClientResponse Struct.
                var tempRCR = new RobotClientResponse();

                // Populate from the Robots List current item
                tempRCR.RobotId = e.RobotId;
                tempRCR.DistanceToGoal = CalcRobotDist(e.X, loadX, e.Y, loadY);
                tempRCR.BatteryLevel = e.BatteryLevel;
                tempRCR.Count = tempCount;

                // Add current item to the RobotClientResponse list.
                listRobotClientResponse.Add(tempRCR);
            }

            //------------------------------------------------------------
            // Outputs All Robots Ordered By Distance, for testing and debugging
            //------------------------------------------------------------

            if (debugFlag == 1)
            {
                var allRobotsByRange = from s in listRobotClientResponse
                                       orderby s.DistanceToGoal
                                       select s;

                foreach (var rbr in allRobotsByRange)
                {
                    returnVal += "(RBR)" + FormatResponse(rbr.RobotId, rbr.DistanceToGoal, rbr.BatteryLevel) + "\r";
                }

                returnVal += "========================================\r";
            }

            //------------------------------------------------------------
            // Get IDs Of Robots In Range With Battery Reserve
            //------------------------------------------------------------

            // Get the closest robots under closeRangeDist (default to 10) units ordered by highest battery level.
            var checkDistInRange = from s in listRobotClientResponse
                                   where s.DistanceToGoal <= closeRangeDist
                                   orderby s.BatteryLevel descending
                                   select s;

            if (checkDistInRange.Count() > 1)
            {
                // If at least one Bobot within closeRangeDist (default to 10), get the ID with highest battery reserve.
                var searchDistInRange = (from s in listRobotClientResponse
                                         where s.DistanceToGoal <= closeRangeDist
                                         orderby s.BatteryLevel descending
                                         select s).FirstOrDefault();

                // Add Result to the collection.
                returnVal += "" + FormatResponse(searchDistInRange.RobotId, searchDistInRange.DistanceToGoal, searchDistInRange.BatteryLevel);
            }
            else
            {
                // if no Robots are within closeRangeDist distance (default to 10), get ID of closest first available.
                var searchMinDist = listRobotClientResponse.OrderBy(i => i.DistanceToGoal).FirstOrDefault();

                // Add Result to the collection.
                returnVal += "" + FormatResponse(searchMinDist.RobotId, searchMinDist.DistanceToGoal, searchMinDist.BatteryLevel);
            }


            //------------------------------------------------------------
            // Output to Client
            //------------------------------------------------------------

            // Format for API Endpoint JSON.
            returnVal = "[" + returnVal + "]";

            // Final JSON validation of collection.
            if (!ValidateJson(returnVal))
            { returnVal += " ERROR INVALID JSON!"; }


            return returnVal;
        }

        //==================================================
        // Do the Work With A Get Request
        //==================================================

        // GET: /<controller>/
        [HttpGet]
        public IActionResult Get()
        {

            // Call the Class Main and Return collection request to the client.
            return Ok("" + HelpRobots() + "");

            // End Of Line.
        }
    }

}
