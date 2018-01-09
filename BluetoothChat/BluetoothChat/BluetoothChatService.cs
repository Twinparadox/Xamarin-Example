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
            if (connectedThread != null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }

            if (acceptThread == null)
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
            if (state == (int)EState.CONNECTING)
            {
                if (connectedThread != null)
                {
                    connectThread.Cancel();
                    connectThread = null;
                }
            }

            if (connectedThread != null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }

            connectThread = new ConnectThread(device, this);
            connectThread.Start();

            SetState((int)EState.CONNECTING);
        }

        // manage bluetooth connection
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Connected(BluetoothSocket socket, BluetoothDevice device)
        {
            if (connectThread != null)
            {
                connectThread.Cancel();
                connectThread = null;
            }

            if (connectedThread != null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }

            if (acceptThread != null)
            {
                acceptThread.Cancel();
                acceptThread = null;
            }

            connectedThread = new ConnectedThread(socket, this);
            connectedThread.Start();

            // send connected device to the UI Activity
            var msg = handler.ObtainMessage((int)BluetoothChat.EMessage.DEVICE_NAME);
            Bundle bundle = new Bundle();
            bundle.PutString(BluetoothChat.DEVICE_NAME, device.Name);
            msg.Data = bundle;

            SetState((int)EState.CONNECTED);
        }

        // stop all threads
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Stop()
        {
            if(connectThread!=null)
            {
                connectThread.Cancel();
                connectThread = null;
            }

            if(connectedThread!=null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }

            if(acceptThread!=null)
            {
                acceptThread.Cancel();
                acceptThread = null;
            }

            SetState((int)EState.NONE);
        }

        // write ConnectedThread using by unsychronized way
        public void Write(byte[] outMsg)
        {
            ConnectedThread t;
            lock(this)
            {
                if (state != (int)EState.CONNECTED)
                    return;
                t = connectedThread;
            }

            t.Write(outMsg);
        }

        // indicate the connnection failure and notify
        private void ConnectedFailed()
        {
            SetState((int)EState.LISTEN);

            // message
            var msg = handler.ObtainMessage((int)BluetoothChat.EMessage.TOAST);
            Bundle bundle = new Bundle();
            bundle.PutString(BluetoothChat.TOAST, "디바이스 연결 실패");
            msg.Data = bundle;
            handler.SendMessage(msg);
        }

        // indicate the lost connection and notify
        public void ConnectionLost()
        {
            SetState((int)EState.LISTEN);

            // message
            var msg = handler.ObtainMessage((int)BluetoothChat.EMessage.TOAST);
            Bundle bundle = new Bundle();
            bundle.PutString(BluetoothChat.TOAST, "디바이스 연결 끊김");
            msg.Data = bundle;
            handler.SendMessage(msg);
        }

        // AcceptThread;runs while listening for incoming connections.
        // convert to .NET thread
        // server-side client behavior
        private class AcceptThread : Thread
        {
            // local server socket
            private BluetoothServerSocket mServerSocket;
            private BluetoothChatService service;

            public AcceptThread(BluetoothChatService service)
            {
                service = service;
                BluetoothServerSocket tmp = null;
                try
                {
                    // create new server socket
                    tmp = service.adpater.ListenUsingRfcommWithServiceRecord(name, myUUID);
                }
                catch (Java.IO.IOExeption e)
                {
                }
                mServerSocket = tmp;
            }

            public override void Run()
            {
                name = "AcceptThread";
                BluetoothSocket socket = null;

                while(service.state!=(int)BluetoothChatService.EState.CONNECTED)
                {
                    try
                    {
                        socket = mServerSocket.Accept();
                    }
                    catch (Java.IO.IOException e)
                    {
                        break;
                    }

                    if(socekt!=null)
                    {
                        lock(this)
                        {
                            switch (service.state)
                            {
                                case EState.LISTEN:
                                case EState.CONNECTING:
                                    service.Connected(socket, socket.RemoteDevice);
                                    break;
                                case EState.NONE:
                                case EState.CONNECTED:
                                    try
                                    {
                                        socket.Close();
                                    }
                                    catch (Java.IO.IOException e)
                                    {
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            public void Cancel()
            {
                try
                {
                    mServerSocket.Close();
                }
                catch(Java.IO.IOException e)
                {
                }
            }
        }
    }
}