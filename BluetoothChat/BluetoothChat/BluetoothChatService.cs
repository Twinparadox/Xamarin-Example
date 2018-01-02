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
using System.Runtime.CompilerServices;

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

        // constructor
        public BluetoothChatService(Context context, Handler handler)
        {
            adpater = BluetoothAdapter.DefaultAdapter;
            state = (int)EState.NONE;
            handler = handler;
        }

        // set state
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void SetState(int state)
        {
            state = state;
            handler.ObtainMessage((int)BluetoothChat.EMessage.STATE_CHANGE, state, -1).SendToTarget();
        }

        // get state
        [MethodImpl(MethodImplOptions.Synchronized)]
        public int GetData()
        {
            return state;
        }

        // start chat service
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Start()
        {
            // cancel connecting thread
            if (connectThread != null)
            {
                connectThread.Cancel();
                connectThread = null;
            }

            // cancel connected thread
            if(connectedThread!=null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }

            if(acceptThread==null)
            {
                acceptThread = new AcceptThread(this);
                acceptThread.Start();
            }

            SetState((int)EState.LISTEN);
        }

        // initiate connection
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Connect(BluetoothDevice device)
        {
            if(state==(int)EState.CONNECTING)
            {
                if(connectedThread!=null)
                {
                    connectThread.Cancel();
                    connectThread = null;
                }
            }

            if(connectedThread!=null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }

            connectThread = new ConnectThread(device, this);
            connectThread.Start();

            SetState((int)EState.CONNECTING);
        }
    }
}