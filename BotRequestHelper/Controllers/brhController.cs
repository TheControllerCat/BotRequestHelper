using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Json;
using BotRequestHelper.Models;
using Microsoft.AspNetCore.Http;
using BotRequestHelper.TestData;


namespace BotRequestHelper
{
    [Route("api/BrhController")]
    [ApiController]
    public class BrhController : Controller
    {
        //==================================================
        // Class Scope Variables
        //==================================================

        private readonly int debugFlag = 0; // +++++ DEBUG DEV FLAG, 1=TRUE, 0=FALSE +++++
        private readonly int closeRangeDist = 10; // Robot close range distance.
        private readonly string robotsApiUrl = "https://svtrobotics.free.beeceptor.com/robots";
        //private readonly string robotsApiUrl = "https://60c8ed887dafc90017ffbd56.mockapi.io/robots";

        //==================================================
        // Generate Random Test Coords
        //==================================================

        private int GetRandCoord()
        {
            int returnVal = 0;
            Random rnd = new Random();

            // Get a random integer value between 1 and 100 for testing.
            returnVal = rnd.Next(1, 100);

            // Return test integer.
            return returnVal;
        }

        //==================================================
        // Validate JSON
        //==================================================

        private bool ValidateJson(string psJsonString)
        {
            bool returnVal = false;

            // Parse a string of JSON data to make sure it's formatted correctly.
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

            // Return T or F.
            return returnVal;
        }

        //==================================================
        // Format Output
        //==================================================

        private string FormatResponse(int RobotId, double DistanceToGoal, int BatteryLevel)
        {
            // Add JSON furniture for item formatting.
            string returnVal = "{\"robotId\":\"" + RobotId.ToString()
                            + "\",\"distanceToGoal\":\"" + DistanceToGoal.ToString()
                            + "\",\"batteryLevel\":\"" + BatteryLevel.ToString()
                            + "\"}";
            if (debugFlag == 1)
            {
                returnVal += "\r";
            }

            // Return formatted JSON item.
            return returnVal;
        }

        //==================================================
        // Calulate Distance
        //==================================================

        private double CalcRobotDist(int x1, int x2, int y1, int y2)
        {
            // Calculate distance formula.
            var returnVal = Math.Round(Math.Sqrt((Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2))), 2);

            // Return calculated distance.
            return returnVal;
        }

        //==================================================
        // Parse Form Input
        //==================================================

        private int ParseFormRequestInput(string psFieldName)
        {
            int returnVal = -999;

            // Used for GET requests.
            /*
            //Check that client input isn't an empty string.
            if (HttpContext.Request.Query["" + psFieldName + ""].ToString() != "")
            {
                //Check and client input is formatted as a valid integer.
                if (int.TryParse(HttpContext.Request.Query["" + psFieldName + ""].ToString(), out int n))
                {
                    returnVal = Convert.ToInt32(HttpContext.Request.Query["" + psFieldName + ""].ToString());
                }
                else
                {
                    returnVal = -999;
                }
            }
            */

            if (HttpContext.Request.Form["" + psFieldName + ""].Count > 0)
            {

                // Used for POST requests.
                //Check that client form input isn't an empty string.
                if (HttpContext.Request.Form["" + psFieldName + ""].ToString() != "")
                {
                    //Check and client form input is formatted as a valid integer.
                    if (int.TryParse(HttpContext.Request.Form["" + psFieldName + ""].ToString(), out int n))
                    {
                        // Check that client form input is greater than zero. 
                        if (Convert.ToInt32(HttpContext.Request.Form["" + psFieldName + ""].ToString()) > 0)
                        {
                            returnVal = Convert.ToInt32(HttpContext.Request.Form["" + psFieldName + ""].ToString());
                        }
                    }
                }

            }
            // Return parsed client input.
            return returnVal;
        }

        //==================================================
        // Query List of Robots
        //==================================================

        private string GetRobotsRaw()
        {
            string returnVal = "";

            // Ping the Robots list API for available Robots.
            try
            {
                // Create a request for the URL.
                WebRequest request = WebRequest.Create(robotsApiUrl);

                // If required by the server, set the credentials.
                request.Credentials = CredentialCache.DefaultCredentials;

                // Get the response.
                WebResponse response = request.GetResponse();

                // Get the stream containing content returned by the server.
                // The using block ensures the stream is automatically closed.
                using (Stream dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    string responseFromServer = reader.ReadToEnd();
                    // Add content to local collection.
                    returnVal = responseFromServer;
                }

                // Close the response.
                response.Close();
            }
            catch
            {
                // If the Robots API is unavailable, pull Robots list from backup data.
                var tempTestData = new TestDataClass();
                returnVal = tempTestData.GetTestDataBots();
            }
            // Test if Robots JSON is valid.
            if (!ValidateJson(returnVal))
            { returnVal += " ERROR INVALID JSON!"; }

            // Return collection of rwa JSON String data.
            return returnVal;
        }

        //==================================================
        // Enumerate and Evaluate Robots
        //==================================================

        private string SearchRobots(int psLoadX, int psLoadY)
        {
            string returnVal = ""; // Collection for output.
            int tempCount = 0; // Manual counter for debugging.

            //------------------------------------------------------------
            // Query JSON List of Robots
            //------------------------------------------------------------

            string RawJsonString = GetRobotsRaw();

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

            //Loop through List of Robot Objects that contains the Robot Json feed.
            foreach (var e in tempJsonObj)
            {
                tempCount++;

                // Set up a single instance of the RobotClientResponse Struct.
                var tempRCR = new RobotClientResponse();

                // Populate from the Robots List current item
                tempRCR.RobotId = e.RobotId;
                tempRCR.DistanceToGoal = CalcRobotDist(e.X, psLoadX, e.Y, psLoadY);
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
            // Return collection.
            //------------------------------------------------------------

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

            string returnVal = ""; //The string for the output collection.

            int loadId = -999; //Arbitrary ID of the load which needs to be moved.
            int loadX = -999; //Current x coordinate of the load which needs to be moved.
            int loadY = -999; //Current y coordinate of the load which needs to be moved.

            //------------------------------------------------------------
            // Request Input
            //------------------------------------------------------------

            if (debugFlag == 1)
            {
                // Get Random integers for the coordinate testing.
                /*
                loadId = GetRandCoord() + GetRandCoord();
                loadX = GetRandCoord();
                loadY = GetRandCoord();
                */

                if (HttpContext.Request.Form != null)
                {
                    loadId = ParseFormRequestInput("loadId");
                    loadX = ParseFormRequestInput("x");
                    loadY = ParseFormRequestInput("y");
                }


                returnVal += "{loadId:" + loadId + ",loadX:" + loadX + ",loadY:" + loadY + "}\r";
            }
            else
            {
                // Parse and Validate the client request input.
                loadId = ParseFormRequestInput("loadId");
                loadX = ParseFormRequestInput("x");
                loadY = ParseFormRequestInput("y");

            }

            //------------------------------------------------------------
            // Double Check Parse Client Request Input
            //------------------------------------------------------------

            // Invalid client input will return a warning value of -999
            if ((loadX == -999) || (loadY == -999))
            {
                // On detection of bad input, a valid JSON error in generated.
                returnVal += FormatResponse(-999, -999, -999);
            }
            else
            {
                // If input appears valid, run the Robot Search.
                returnVal += SearchRobots(loadX, loadY);
            }

            //------------------------------------------------------------
            // Output to Client
            //------------------------------------------------------------

            // Format for API Endpoint JSON.
            returnVal = "[" + returnVal + "]";

            // Final JSON validation of collection.
            if (!ValidateJson(returnVal))
            {
                if (debugFlag == 1)
                {
                    returnVal += " ERROR INVALID JSON!";
                }
                else
                {
                    returnVal = "[" + FormatResponse(-999, -999, -999) + "]";
                }
            }

            // Return collection for client response.
            return returnVal;
        }

        //==================================================
        // Do the Work With A GET Request
        //==================================================

        /*
        // GET: /<controller>/
        [HttpGet]
        public IActionResult Get()
        {

            // Call the Class Main and Return collection request to the client.
            return Ok("" + HelpRobots() + "");

            // End Of Line.
        }
        */

        //==================================================
        // Do the Work With A POST Request
        //==================================================

        // POST: /<controller>/
        [HttpPost]
        public IActionResult Post()
        {

            // Call the Class Main and Return collection request to the client.
            return Ok("" + HelpRobots() + "");

            // End Of Line.
        }
    }

}