# BotRequestHelper
API to help manage bot location, distance and pickups.

*The **master** branch is the primary default build branch.*

## Requirements
*   *.NET Core API with a POST endpoint that accepts and returns data per the above task description.*
*   *API can be run locally and tested using Postman or other similar tools.* 
*   *Description of what features, functionality, etc. you would add next and how you would implement them - you shouldn't spend more than an hour on this project, so we want to know what you'd do next (and how you'd do it) if you had more time*
*   *Use git and GitHub for version control.*
*   *Have fun! We're interested in seeing how you approach the challenge and how you solve problems with code. The goal is for you to be successful, so if you have any questions or something doesn't seem clear don't hesitate to ask. Asking questions and seeking clarification isn't a negative indicator about your skills - it shows you care and that you want to do well. Asking questions is always encouraged at SVT Robotics, and our hiring process is no different.*

## Deliverables Checklist
*   *API written in .NET Core.*
    *   **Done and tested.**   
*   *API accepts POST and returns data per above requirements.*
    *   **Done and tested.**      
*   *Repo README has instructions for running and testing the API.*
    *   **Done.**      
*   *Repo README has information about what you'd do next, per above requirements.*
    *   **Done.**      
*   *Create a new GitHub repo and share it.*
    *   **Done.**    

## .NET Core API with a POST endpoint that accepts and returns data per the above task description.

The API code has been configured based on the provided specs for input, processing and output. The package uses a combination of the Newtonsoft and .NET Core includes to handle JSON API work. Since I haven't had the chance to work directly on the development of an API, the implementation was relatively new ground for me, in particular the handling of POST input in a raw JSON format. Originally, the API was written under the assumption that POST FORM parameters would be passed from the client request, however, after further clarification this was corrected and now accepts JSON from the body of the client request.

One the request has been made, the API will made a request to the external list of available robots to get the latest available list of information. This is enumerated into a local List object and compared with the payload coordinate information provided by the user request. Some error checking and event handling is involved at this stage.

Finally, when the best available robot is found (based on distance and battery level), the response is formatted back into a validated JSON and returned to the requesting client. In the event that an error was detected, the API will attempt to return a graceful error code formatted into the correect JSON with the value "-999" in place of the other values as follows:

      [{"robotId":"-999","distanceToGoal":"-999","batteryLevel":"-999"]


## API can be run locally and tested using Postman or other similar tools.

The following primary tools were used for the development and testing of the API. This is the primary and recommended configuration for verification testing.
* Developed in Visual Studio Community Edition for Mac Version 8.10.4 (build 11)
* Tested using Postman Version 8.7.0 (8.7.0). 

## Instructions for running and testing the API

## Testing
### Testing basic functinality

**Input:**

    [{"loadId": "231","x": "5","y": "3"}]
    
**Expected Response:**

    [{"robotId":"4","distanceToGoal":"5","batteryLevel":"37"}] 

### Test for second closest with highest battery charge

**Input:**

    [{"loadId": "231","x": "5","y": "33"}]
    
**Expected Response:**
This set of coordinates puts Robots #75, #88, #48, #6, #69, #24, #78, and #45 within the 10 distance unit range of the target payload.

    {"robotId":"75","distanceToGoal":"0","batteryLevel":"97"}
    {"robotId":"88","distanceToGoal":"2","batteryLevel":"98"}
    {"robotId":"48","distanceToGoal":"4.12","batteryLevel":"78"}
    {"robotId":"6","distanceToGoal":"5.66","batteryLevel":"92"}
    {"robotId":"69","distanceToGoal":"6.32","batteryLevel":"41"}
    {"robotId":"24","distanceToGoal":"6.4","batteryLevel":"79"}
    {"robotId":"78","distanceToGoal":"7","batteryLevel":"87"}
    {"robotId":"45","distanceToGoal":"9.9","batteryLevel":"56"}

However, Robot #88 has the highest battery level, so, the id for that rebot is returned.

    [{"robotId":"88","distanceToGoal":"2","batteryLevel":"98"}]

## Information about what you'd do next, per above requirements
