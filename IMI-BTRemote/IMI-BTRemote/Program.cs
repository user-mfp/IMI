using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;

namespace IMI_BTRemote
{
    public partial class Program
    {
        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/


            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");

            bluetooth.SetDeviceName("Gadgeteer");

            bluetooth.BluetoothStateChanged += new Bluetooth.BluetoothStateChangedHandler(bluetooth_BluetoothStateChanged);
            bluetooth.DataReceived += new Bluetooth.DataReceivedHandler(bluetooth_DataReceived);

            //The timer gives the device enough time to initialize.
            Gadgeteer.Timer timer = new Gadgeteer.Timer(1000, Gadgeteer.Timer.BehaviorType.RunOnce);
            timer.Tick += new Gadgeteer.Timer.TickEventHandler(timer_Tick);
            timer.Start();

        }

        void timer_Tick(Gadgeteer.Timer timer)
        {
            //You only need to enter pairing mode once with a device. After you pair for the first time, it will
            //automatically connect in the future.
            if (!bluetooth.IsConnected)
                bluetooth.ClientMode.EnterPairingMode();
        }

        void bluetooth_BluetoothStateChanged(Bluetooth sender, Bluetooth.BluetoothState btState)
        {
            Debug.Print(btState.ToString());
        }

        void bluetooth_DataReceived(Bluetooth sender, string data)
        {
            Debug.Print(data);
            sender.ClientMode.SendLine(data); //echoes the data back to the device.
        }
    }
}
