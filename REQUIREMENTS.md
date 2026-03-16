1. Assuming you already download “pmdg-aircraft-77f-liveries” addon and extract it into your MSFS 2020 Community folder.

2. Extract the livery .zip contents into a separate new folder inside “C:\Users\USER\AppData\Local\Packages\Microsoft.FlightSimulator_8wekyb3d8bbwe\LocalCache\Packages\Community\pmdg-aircraft-77f-liveries\SimObjects\Airplanes”, give it a unique folder name (for exampl in this situation, the .zip name is "KOREAN AIR (HL7203)", the folder name is the name KOREAN AIR (HL7203). )

3. If you did it correctly, the aircraft.cfg file should be located at “C:\Users\USER\AppData\Local\Packages\Microsoft.FlightSimulator_8wekyb3d8bbwe\LocalCache\Packages\Community\pmdg-aircraft-77f-liveries\SimObjects\Airplanes\KOREAN AIR (HL7203)\aircraft.cfg”

4. Navigate to your new “C:\Users\USER\AppData\Local\Packages\Microsoft.FlightSimulator_8wekyb3d8bbwe\LocalCache\Packages\Community\pmdg-aircraft-77f-liveries\SimObjects\Airplanes\KOREAN AIR (HL7203)” folder

5. Open the “livery.json” file with a text editor and copy the value right next to “atcId”, but leave out the quotes (for example, on “atcId”: “testId”, you copy testId)

6. Rename the “options.ini” file to the value you copied (for example testId.ini)

7. Assuming you already did a flight with the aircraft, copy the renamed .ini file into the following folder:

Microsoft Store version:
C:\Users\USERNAME\AppData\Local\Packages\Microsoft.FlightSimulator_8wekyb3d8bbwe\LocalState\Packages\pmdg-aircraft-77f\work\Aircraft

Steam version:
C:\Users\USERNAME\AppData\Roaming\Microsoft Flight Simulator\Packages\pmdg-aircraft-77f\work\Aircraft

8. Lastly, inside the “C:\Users\USER\AppData\Local\Packages\Microsoft.FlightSimulator_8wekyb3d8bbwe\LocalCache\Packages\Community\pmdg-aircraft-77f-liveries” folder, drag the layout.json file onto the MSFSLayoutGenerator.exe file (or launch GEN_LAYOUT.bat)