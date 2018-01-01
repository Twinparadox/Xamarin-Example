using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Util;
using Java.Lang;
using Java.Util;

namespace BluetoothChat
{
    class BluetoothChatService
    {
        private const string name = "BluetoothChat";

        private static UUID myUUID = UUID.FromString("fa87c0d0-afac-11de-8a39-0800200c9a66");

        protected BluetoothAdapter adpater;
        protected Handler handler;
        private AcceptThread acceptThread;
        protected ConnectThread connectThread;
        private ConnectedThread connectedThread;
        protected int state;

        public enum EState
        {
            NONE,
            LISTEN,
            CONNECTING,
            CONNECTED
        };

    }
}