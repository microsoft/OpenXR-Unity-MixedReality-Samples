# Holographic App Remoting Instructions

App remoting scenario helps user to run the BasicSample app locally on the PC(UWP/Win32) and render it to HL2(or a different PC) running HAR(Holographic App Remoting) player app.

Basic Sample can be used in App Remoting Scenario using two different modes - connect and listen. 

|Remote App running on PC  | HAR player app running on HL2|
|--------------------------|------------------------------|
|Connect mode              |   Listen mode                |
|Listen mode               |   Connect mode               |

## The following are the possible configurations:
- Build remote app as "UWP", and use "Connect()" on remote app running on PC and listen using custom HAR player app on HL2.
- Build remote app as "UWP", and use "Listen()" on remote app running on PC and connect using custom HAR player app/HAR player app on HL2.
- Build remote app as "standalone" win32 app, and use "Connect()" on remote app running on PC and listen using custom HAR player app on HL2.
- Build remote app as "standalone" win32 app, and use "Listen()" on remote app running on PC and connect using custom HAR player app/HAR player app on HL2.

### The following app is required before proceeding to use HAR. 
- [custom HAR Sample player](https://github.com/microsoft/MixedReality-HolographicRemoting-Samples/blob/main/player/sample/SamplePlayer.sln) that runs on HL2.
- HAR player app that is available in Microsoft Store only works with Connect() by listening. If you need to test Listen() by connecting, you need to use the custom HAR player app above.

### For all the configurations listed above, open the BasicSample project in Unity, 
- Make sure to turn on "Holographic Remoting Remote app feature group" in "XR Plugin Management"under "Project Settings".
- Make sure to uncheck "Initialize XR on Startup" for HAR as shown below.
![xr-plugin-management](Readme/xr-plugin-management.png)

### For building UWP remote app that runs on the PC (connect/listen configuration):
- Turn on the below highlighted "internet" capabilities in the UWP manifest before proceeding to build. They are under "Project Settings → Player→ Capabilities"

![UWP-player-capabilities](Readme/UWP-player-capabilities.png)

- Build the x64 UWP app to run on the Local Machine (PC) as shown below.

![UWP-build-config](Readme/UWP-build-config.png)

- Give firewall network permissions for VS 2019 to run OpenXR+UnityBasicSample. Go to Control Panel\System and Security\Windows Defender Firewall\Allowed apps and find VS2019 in the allowed app list and make sure to give permission to all network types.
![uwp-vs-firewall-permissions](Readme/uwp-vs-firewall-permissions.png)

- After the build, open the "OpenXR+UnityBasicSample" using VS2019 in the above built config.
![uwp-run-vs-sol](Readme/uwp-run-vs-sol.png)

- Make sure the highlighted Debug settings are enabled. Right click on OpenXR+UnityBasicSample solution→ Properties → Configuration Properties → Debugging

![uwp-vs-sol-settings](Readme/uwp-vs-sol-settings.png)

- Run the OpenXR+UnityBasicSample app by hitting  the play button in VS solution. You will be able to see the BasicSample app run on the PC. 

### For building Standalone Win32 remote app that runs on the PC (connect/listen configuration):
- Build the standalone win32 app to run on the Local Machine (PC) as shown below

![win32-build-config](Readme/win32-build-config.png)

- Give firewall network permissions for OpenXR+UnityBasicSample.exe built using unity above to connect to HAR Sample Player app. Go to Control Panel\System and Security\Windows Defender Firewall\Allowed apps. 
Click "Change settings" and "Allow another app" to add OpenXR+UnityBasicSample.exe to the list of allowed apps. Make sure to give permission to all network types.
![win32-firewall-permissions](Readme/win32-firewall-permissions.png)

- Click on the OpenXR+UnityBasicSample.exe to run it on the PC.


### Scenario 1: Run BasicSample app in Listen Mode and HAR player app in Connect Mode:

- Click Listen on the 2D UI screen in BasicSample ap that is running on PC
![app-remoting-flat-ui](Readme/app-remoting-flat-ui.png)

- Now open the custom HAR Sample Player app using VS 2019 with the following configuration to run on the HL2 attached to the PC using USB cable.
![har-sample-player-run-sol](Readme/har-sample-player-run-sol.png)

- Give in the IP address of the host PC as command line argument to the SamplePlayer app. Right click on SamplePlayer solution → Properties→ Configuration Properties → Debugging → Command Line Arguments
![har-sample-player-app-connecting](Readme/har-sample-player-app-connecting.png)

- Deploy the SamplePlayer solution on HL2 by hitting the play button in VS solution. You will be able to see the HAR Sample player running on HL2, displaying that it is "Connecting to IP address of the PC"

- Once the connection is established, the HAR sample player app running on HL2 will start "Receiving".

### Scenario 2: Run HAR player app in Listen Mode and BasicSample app in Connect Mode:

- Skip the below three steps and run the HAR player app if it is already installed on the HL2 from store.

- Open the custom HAR Sample Player app using VS 2019 with the following configuration to run on the HL2 attached to the PC using USB cable.

![har-sample-player-run-sol](Readme/har-sample-player-run-sol.png)

- Give in the command line argument as "-listen" to the SamplePlayer app. Right click on Sample Player solution → Properties→ Configuration Properties → Debugging → Command Line Arguments
![har-sample-player-app-listening](Readme/har-sample-player-app-listening.png)

- Deploy the SamplePlayer solution on HL2 by hitting the play button in VS solution. You will be able to see the HAR Sample player running on HL2, displaying that it is "waiting for a connection on IP address of HL2"

- Now in the Basic Sample app that is running on the host PC  give in the IP address of HL2 displayed above and hit "connect"
![app-remoting-flat-ui](Readme/app-remoting-flat-ui.png)

- Once the connection is established, the HAR sample player app running on HL2 will start "Receiving".
