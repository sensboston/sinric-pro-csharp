# Using Alexa with C#
Ever wanted to use Alexa with C#? The Sinric Pro smart skill makes it easy.

This project is a C# adaptation for communicating with the Sinric Pro service, implementing their API.

Set up a Sinric Pro account here: https://sinric.pro/index.html

See the full API documentation here: https://help.sinric.pro/

## Simulated Smart Lock
The included example (so far) simulates a smart lock with 2-way communication.
* It can generate events such as "locked" or "unlocked" or "jammed" from your PC
* It can receive lock/unlock commands from the Sinric Pro smart skill

This is useful for setting off Alexa routines, or commanding your PC to perform spoken actions from anywhere in the world...

## Getting started
First, you will need a valid Sinric account. Create a fake 'Smart Lock' device in the Sinric Dashboard.

Next, you will need to copy appsettings.json to appsettings.private.json
* Fill in the details of your account & device
* Set the build action: 'Content'
* Copy to output directory: 'Copy if newer'

Follow the instructions to link the Sinric smart skill to your Alexa account.
Go into your Alexa app and perform 'Discover Devices' -- then you can create routines using the fake Smart Lock.

To see something happen, open the Sinric dashboard and click 'Lock' or 'Unlock'
To set off an event, uncomment one of the 'SetNewState' lines, or hook into some other event on your PC ...


# Using Alexa with Python
If Python is your thing, the official Sinric Github repo has plenty of examples here:
https://github.com/sinricpro/python-sdk

# Using Alexa with esp32/8266
See the official esp32/8266 Sinric Github repo here: https://github.com/sinricpro/esp8266-esp32-sdk