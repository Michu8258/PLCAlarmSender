# PLC Alarm Sender
This application monitores alarm bits in Programmable Logic Controller (Siemens S7-300, S7-400, S71200, S71500) and sends SMS messages to defined users (TODO).

Sometimes users of industrial automation devices want to receive some sort of notification in case of any failure on device (alarms), because not always there is somebody near the device. This solution tries to fulfill this task. In case of S7-1200 and S7-1500 PLCs, the PUT/GET communication has to be enabled.

## Main Functionalities
- Online checking for alams in PLC,
- S7 PLC connection manager (connection to multiple PLC from single instance of app),
- Manager of alarm profiles (you can set in which hours and i which days alarm should be send to operator for example),
- SMS Recipients and recipients group manager - you can set the group of users to which message should be send if any alarm occures,
- Alarms management - adding, removing, modifying existing alarms - for every PLC,
- Up to 16 languages of alarm - language can be switched in runtime,
- Application user management (with roles),
- exploring logs (built in app),
- settings of logging (when to delete old logs, etc.),
- export/import of alarms/alarm profiles/message recipients (JSON).

## Used Technologies
- WPF (MVVM with Caliburn.Micro).
- REALM as Database,
- Communication with S7 PLCs - Sharp 7 library,
- Importing alarms from WinCC.

## TODO
I did not do sending SMS itself, because all solutions I found was priced, and I decided not to pay for something I may not even use.
