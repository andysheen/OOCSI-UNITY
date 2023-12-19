# OOSCI for Unity

This simple helper script was created to enable the use of [OOSCI](https://github.com/iddi/oocsi) and Unity. The script allows you to send and receive multi channel data. Tested with Unity 2022.3.2f1.

# How to use (Quick Start)

- Clone the repository into your Assets folder.
- Create an empty object and add `OOSCI.cs` script to the object.
- Update your the Server, Port, Unique ID and Channel names (image below).
- To send data, press arrow keys Up, Down and Left to increment three integers, sending to three separate channels.
- Received data on each of the three channels will be displayed on the debug window.
  <img width="332" alt="Screenshot 2023-12-19 at 15 50 07" src="https://github.com/andysheen/OOSCI-UNITY/assets/20442164/2bbebe03-92b6-4c8a-97cf-06e0e9b2f288">


# How to update the data sets

- This script uses Unity's jsonUtility object for wider compatibility. jsonUtility requires users to create a class matching the JSON structure. To update this to match your requirements, edit the `OOSCIMessage.cs` file.
- To add or reduce the number of channels subscribed to, use the `SubscribeToChannel()` function. All incoming data is handled in `processOOSCIMessage()`.
