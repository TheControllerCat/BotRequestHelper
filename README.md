# BotRequestHelper
API to help manage bot location, distance and pickups.

*The **master** branch is the primary default build branch.*

Archive can be downloaded from the [**BotRequestHelper.zip**](https://github.com/TheControllerCat/BotRequestHelper/raw/master/BotRequestHelper.zip) file.

## Requirements
*   [*.NET Core API with a POST endpoint that accepts and returns data per the above task description.*](#req01)
*   [*API can be run locally and tested using Postman or other similar tools.*](#req02)
*   [*Description of what features, functionality, etc. you would add next and how you would implement them - you shouldn't spend more than an hour on this project, so we want to know what you'd do next (and how you'd do it) if you had more time*](#req03)
*   [*Use git and GitHub for version control.*](#req04)
*   [*Have fun! We're interested in seeing how you approach the challenge and how you solve problems with code. The goal is for you to be successful, so if you have any questions or something doesn't seem clear don't hesitate to ask. Asking questions and seeking clarification isn't a negative indicator about your skills - it shows you care and that you want to do well. Asking questions is always encouraged at SVT Robotics, and our hiring process is no different.*](#req05)

 

## <a name="req01"></a>.NET Core API with a POST endpoint that accepts and returns data per the above task description.

The API code has been configured based on the provided specs for input, processing and output. The package uses a combination of the Newtonsoft and .NET Core includes to handle JSON API work. Since I haven't had the chance to work directly on the development of an API, the implementation was relatively new ground for me, in particular the handling of POST input in a raw JSON format. Originally, the API was written under the assumption that POST FORM parameters would be passed from the client request, however, after further clarification this was corrected and now accepts JSON from the body of the client request.

One the request has been made, the API will made a request to the external list of available robots to get the latest available list of information. This is enumerated into a local List object and compared with the payload coordinate information provided by the user request. Some error checking and event handling is involved at this stage.

Finally, when the best available robot is found (based on distance and battery level), the response is formatted back into a validated JSON and returned to the requesting client. In the event that an error was detected, the API will attempt to return a graceful error code formatted into the correect JSON with the value "-999" in place of the other values as follows:

      [{"robotId":"-999","distanceToGoal":"-999","batteryLevel":"-999"]

## <a name="req02"></a> API can be run locally and tested using Postman or other similar tools.

The following primary tools were used for the development and testing of the API. This is the primary and recommended configuration for verification testing.
* Developed in Visual Studio Community Edition for Mac Version 8.10.4 (build 11) [Visual Studio Website](https://visualstudio.microsoft.com/downloads/)
* Tested using Postman Version 8.7.0 (8.7.0) [Postman Website](https://www.postman.com/downloads/)
* Google Chrome Canary Version 93.0.4568.0 (Official Build) canary (x86_64) [Canary Website](https://www.google.com/chrome/canary/)

### Instructions for running and testing the API

This installation walkthrough assumes that the user is familiar with Git, Github, Visual Studio, Postman and Chrome and that the local workstation has an active internet connection.

1. Ensure that MS Visual Studio is downloaded, installed, and updated on the local workstation. 
2. Ensure that Postman client software is downloaded, installed, and updated on the local workstation.
3. Ensure that Google Chrome Canary is downloaded, installed and updated on the local workstation.
4. Pull from the “master” branch of the Github repo and add the contents of the project to the local hard drive.
5. Open the project solution in MS Visual Studio.

Testing the API is performed with Postman client on the local workstation where Visual Studio is currently open. 

1. At the top of the Visual Studio interface, make sure that “Google Chrome Canary” is selected as the target web browser where the application will open.
2. From the “Run” menu item at the top of the Visual Studio client, select the option “Run Without Debugging” from the dropdown menu. 
3. Visual Studio will run the application and open Canary with a “This page isn’t working” error. This is normal and expected. 
4. In the address bar at the top of the Canary web browser window, copy the address which should be “http://127.0.0.1:50000/api/brhController”
5. Now, open the Postman client application. 
6. Under the Workspaces menu option at the top of the Postman client, select the “+New Workspace” option from the dropdown menu.
7. Name the new workspace “BotRequestHelper”, under “Visibility” select the option for “Personal”, and finally click the button to “Create Workspace”.
8. In the new Postman Workspace page, click the small “+” icon next the the Collections tab on the left side of the screen. This will create a new item called ”New Collection”.
9. Click the ellipsis (three dots) icon next to the words “New Collection”. From the Dropdown select “Add Request”. 
10. Click on the “GET New Request” item.
11. On the main part of the Postman screen, past the web address from Canary into the “Enter request URL” bar.
12. In the dropdown menu to the left of the address bar where it reads “GET”, click the dropdown and select the “POST” option.
13. Under the address bar, click on the menu option for “Body”.
14. Under that menu bar, click on the option for “raw” and change the dropdown from “Text” to “JSON”.

The testing environment should now be configured and ready for testing. Using Postman local client, copy the following JSON strings into the Body>raw(JSON) text area, then click the "Send" button next to the address bar.

#### Testing basic functinality
This tests the basic functionality of the API.

**Input:**

    [{"loadId": "231","x": "5","y": "3"}]
    
**Expected Response:**

    [{"robotId":"4","distanceToGoal":"5","batteryLevel":"37"}] 

#### Test for second closest with highest battery charge

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

## <a name="req03"></a> Description of what features, functionality, etc. you would add next and how you would implement them.

1. First I would definitely clean up and streamline my own code. Using what I already knew of C# and C languages in general, I did a lot of experimental ideas that didn't work out based on how an API should work. There are whole member functions of the primary class that are only for development and unit testing. I've left them in place in case anyone coming behind is interested in seeing what I was thinking as I was going.
2. Next, add more error checking and security. Most of the security can probably be handled by simply turning on SSL and using an encrypted connection. While this isn't unbreakable, it's useful for many situations. 
3. Another item that would help with security and error checking is the add an event log. This would probably be handled by a local database (MS Sql or MySQL) where each request along with the result would be logged and backed up. This could also include identifying and nonrepudiation data such as the IP address of the requesting client, a hashed client ID that would be used as an identifier for the requesting user, maybe even a priority token if high traffic is a concern. 
4. A hashed client ID passed in by the client request could also be useful as a sort of handshaking passkey. If a server side list of pre-approved client IDs are kept in a Sql shadow table or JSON, those could be compared to the ID being submitted by the client request. If the ID isn’t on the pre-approved whitelist, the request is denied.
5. Another option would be based on navigation cases and events. Since the real world isn’t a flat 2D field without any obstacles, there are some navigation and geometry issues that have to come up for the deployment of these helpful Robots.
   * The physical dimensions of the bot have to be taken into account (length, width, height, weight). This could be handled by creating a master list of makes and models of each Robot and what size categories they fall into. This will be useful for later comparison to the physical work area.
   * The carrying capacity of the robots. Some are specialized for specific types of jobs while some are more generalized workers. A master database of makes and models should also include the job and payload capacity of the Robots being searched. This will need to be made in comparison the the weight, size and fagility of the payload being requested.
   * Which also means there will need to be a database or API to handle a list of the physical properties of the payloads being picked up. A lot of this information will probably be on file already and it will be a matter of connecting the middleware and getting permissions.
   * The physical space of the operating field will need to be mapped with coordinates or vectors marking out the physical geometry of the space where the Robots will be operating. 
6. Instead of a read only static list of available robots, the list of available workers will be changing in real time. The positions of the Robots have all changed in the time it took for the original client request to be made. 
   * Have an additional flag on the robots list that marks an individual robot as either “Busy”, “Available” or “Unavailable” (with other options possible). If the robot right next to a requested pickup is currently already on another task, then it can’t perform the job and the next available robot will need to be queried for availability.
   * Expanding on this, there could be a “Job Queue” where incoming client requests enter a holding pattern until an available Robot can perform the job as requested. This is useful when there are more client requests than Robots available to perform the work. 
7. A separate monitoring admin page that tracks the location, deployment and status of all the available worker Robots. This would just be a formatted web page that returns readable, useful information about the requests being made to a user and would mostly pull the same data as the API.


## <a name="req04"></a> Use git and GitHub for version control.

The GitHub repo can be found here:
[https://github.com/TheControllerCat/BotRequestHelper](https://github.com/TheControllerCat/BotRequestHelper)

### Deliverables Checklist
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

## <a name="req05"></a> Have fun! We're interested in seeing how you approach the challenge and how you solve problems with code.

Thanks for your attention and I hope you like my work.
