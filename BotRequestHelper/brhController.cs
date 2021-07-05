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



namespace BotRequestHelper
{
    [Route("api/BrhController")]
    [ApiController]
    public class BrhController : Controller
    {

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
        // Format Ourput
        //==================================================

        private string FormatResponse(int RobotId, double DistanceToGoal, int BatteryLevel)
        {
            return "{\"robotId\":\"" + RobotId.ToString()
                            + "\",\"distanceToGoal\":\"" + DistanceToGoal.ToString()
                            + "\",\"batteryLevel\":\"" + BatteryLevel.ToString()
                            + "\"}"; //\r
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
        // Do the Work
        //==================================================

        // GET: /<controller>/
        [HttpGet]
        public IActionResult Get()
        {

            //------------------------------------------------------------
            // Local Variables
            //------------------------------------------------------------

            int debugFlag = 0;

            int loadId; //Arbitrary ID of the load which needs to be moved.
            int loadX; //Current x coordinate of the load which needs to be moved.
            int loadY; //Current y coordinate of the load which needs to be moved.

            string ReturnVal = "";
            //string TempVal = "";
            string RawJsonString = "";

            //------------------------------------------------------------
            // Request Input
            //------------------------------------------------------------

            loadId = 231;
            loadX = GetRandCoord(); //5;
            loadY = GetRandCoord(); //3; 

            //------------------------------------------------------------
            // Query List of Robots
            //------------------------------------------------------------

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

                RawJsonString = responseFromServer;
            }

            // Close the response.
            response.Close();

            //------------------------------------------------------------
            // Test if Robots JSON is valid.
            //------------------------------------------------------------


            if (!ValidateJson(RawJsonString))
            { ReturnVal += " ERROR INVALID JSON!"; }

            /*
                try
                {
                    var tempObj = JsonValue.Parse(RawJsonString);
                    //ReturnVal = RawJsonString;
                }
                catch (FormatException fex)
                {
                    ReturnVal = "" + " ERROR INVALID " + fex;
                }
                catch (Exception ex) //some other exception
                {
                    ReturnVal = "" + " ERROR OTHER " + ex;
                }
            */

            //------------------------------------------------------------
            // Convert Raw JSON string to Object
            //------------------------------------------------------------

            // Convert raw text Json into List of Robot objects
            //var details = JsonValue.Parse(RawJsonString);
            var tempJsonObj = JsonConvert.DeserializeObject<List<Robot>>(RawJsonString);

            int tempCount = 0;
            double tempDist = 0;

            List<RobotClientResponse> listRobotClientResponse = new List<RobotClientResponse>();

            //------------------------------------------------------------
            // Loop Through the Available Robots Into A List
            //------------------------------------------------------------

            foreach (var e in tempJsonObj)
            {

                tempCount++;
                tempDist = CalcRobotDist(e.X, loadX, e.Y, loadY);

                var tempRCR = new RobotClientResponse();

                tempRCR.RobotId = e.RobotId;
                tempRCR.DistanceToGoal = tempDist;
                tempRCR.BatteryLevel = e.BatteryLevel;
                tempRCR.Count = tempCount;


                listRobotClientResponse.Add(tempRCR);
                
            }

            //------------------------------------------------------------
            // List By Distance, for testing and debugging
            //------------------------------------------------------------

            if (debugFlag == 1)
            {
                var allRobotsByRange = from s in listRobotClientResponse
                                       orderby s.DistanceToGoal
                                       select s;

                foreach (var rbr in allRobotsByRange)
                {
                    ReturnVal += "(RBR)" + FormatResponse(rbr.RobotId, rbr.DistanceToGoal, rbr.BatteryLevel) + "\r";
                }

                ReturnVal += "========================================\r";

            }

            //------------------------------------------------------------
            // Get IDs Of Robots In Range With Battery Reserve
            //------------------------------------------------------------

            int closeRangeDist = 10;

            // Get the closest robots under closeRangeDist (default to 10) units ordered by highest battery level.
            var checkDistInRange = from s in listRobotClientResponse
                                    where s.DistanceToGoal <= closeRangeDist
                                    orderby s.BatteryLevel descending
                                    select s;

            if (checkDistInRange.Count() > 1)
            {
                // If at least one Bobot within 10 distance, get the ID with highest battery reserve.
                var searchDistInRange = (from s in listRobotClientResponse
                                         where s.DistanceToGoal <= closeRangeDist
                                         orderby s.BatteryLevel descending
                                         select s).FirstOrDefault();

                ReturnVal += "" + FormatResponse(searchDistInRange.RobotId, searchDistInRange.DistanceToGoal, searchDistInRange.BatteryLevel);

            }
            else
            {
                // if no Robots are within closeRangeDist distance, get ID of closest first available.
                var searchMinDist = listRobotClientResponse.OrderBy(i => i.DistanceToGoal).FirstOrDefault();

                ReturnVal += "" + FormatResponse(searchMinDist.RobotId, searchMinDist.DistanceToGoal, searchMinDist.BatteryLevel);

            }


            //------------------------------------------------------------
            // Output to Client
            //------------------------------------------------------------

            ReturnVal = "["+ ReturnVal + "]";

            return Ok(""+ ReturnVal + "");


        }
    }

}
