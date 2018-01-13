using Android.App;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using Android.Views;
using Java.Lang;
using Android.Content;
using Android.Views.InputMethods;
using System;

namespace BluetoothChat
{
    [Activity(Label = "BluetoothChat", MainLauncher = true)]
    public class BluetoothChat : Activity
    {
        private const string TAG = "BluetoothChat";
        private const bool Debug = true;

        // Message types
        public enum EMessage
        {
            STATE_CHANGE=1,
            READ=2,
            WRITE=3,
            DEVICE_NAME=4,
            TOAST=5
        }

        // Request codes
        private enum ERequest
        {
            CONNECT_DEVICE=1,
            ENABLE_BT=2
        }

        // Key
        public const string DEVICE_NAME = "device_name";
        public const string TOAST = "toast";

        // Layout
        protected TextView title;
        private ListView lvChatlog;
        private EditText edittextMeesage;
        private Button btnSend;

        // Connected device name
        protected string connectedDeviceName = null;

        // var for the chat
        protected ArrayAdapter<string> conversationArrayAdapter;
        private StringBuffer outStringBuffer;
        private BluetoothAdapter bluetoothAdapter = null;
        private BluetoothChatService chatService = null;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            title = FindViewById<TextView>(Resource.Id.titleLeft);
            title.SetText(Resource.String.app_name);
            title = FindViewById<TextView>(Resource.Id.titleRight);

            edittextMeesage = FindViewById<EditText>(Resource.Id.edittextMessage);
            lvChatlog = FindViewById<ListView>(Resource.Id.lvChatlog);
            btnSend = FindViewById<Button>(Resource.Id.btnSend);

            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            // check if the device supports bluetooth
            if (bluetoothAdapter == null)
            {
                Toast.MakeText(this, "블루투스를 지원하지 않는 기기입니다.", ToastLength.Long).Show();
                Finish();
                return;
            }                
        }

        protected override void OnStart()
        {
            base.OnStart();

            // check if the bluetooth option is not on
            if(!bluetoothAdapter.IsEnabled)
            {
                Intent enableIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(enableIntent, (int)ERequest.CONNECT_DEVICE);
            }
            else
            {
                if (chatService == null)
                    SetupChat();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            if(chatService!=null)
            {
                if (chatService.GetState() == BluetoothChatService.STATE.NONE)
                    chatService.Start();
            }
        }

        private void SetupChat()
        {
            conversationArrayAdapter = new ArrayAdapter<string>(this, Resource.Layout.message);
            lvChatlog.Adapter = conversationArrayAdapter;

            edittextMeesage.EditorAction += delegate (object sender, TextView.EditorActionEventArgs e)
              {
                  if (e.ActionId == ImeAction.ImeNull && e.Event.Action == KeyEventActions.Up)
                  {
                      var message = new Java.Lang.String(((TextView)sender).Text);
                      SendMessage(message);
                  }
              };

            btnSend.Click += delegate (object sender, EventArgs e)
              {
                  var message = new Java.Lang.String(lvChatlog.Text);
                  SendMessage(message);
              };

            // Initialize the chatService to perform bluetooth connections
            chatService = new BluetoothChatService(this, new MyHandler(this));

            // Initialize the buffer for outgoing messages
            outStringBuffer = new StringBuffer("");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Stop the Chat services
            if (chatService != null)
                chatService.Stop();
        }
        
        private void SendMessage(Java.Lang.String message)
        {
            // check if connection is available
            if(chatService.GetState()!=BluetoothChatService.STATE.CONNECTED)
            {
                Toast.MakeText(this, Resource.String.not_connected, ToastLength.Short).Show();
                return;
            }

            // check message
            if(message.Length()>0)
            {
                // get the message and write
                byte[] send = message.GetBytes();
                chatService.Write(send);

                // clear buffer and edittext
                outStringBuffer.SetLength(0);
                edittextMeesage.Text = string.Empty;
            }
        }

        private class MyHandler : Handler
        {
            BluetoothChat bluetoothChat;

            public MyHandler(BluetoothChat chat)
            {
                bluetoothChat = chat;
            }

            public override void HandleMessage(Message msg)
            {
                switch (msg.What)
                {
                    case (int)EMessage.STATE_CHANGE:
                        switch (msg.Arg1)
                        {
                            case (int)BluetoothChatService.EState.CONNECTED:
                                bluetoothChat.title.SetText(Resource.String.titleConnected);
                                bluetoothChat.title.Append(bluetoothChat.connectedDeviceName);
                                bluetoothChat.conversationArrayAdapter.Clear();
                                break;
                            case (int)BluetoothChatService.EState.CONNECTING:
                                break;
                            case (int)BluetoothChatService.EState.LISTEN:
                                break;
                            case (int)BluetoothChatService.EState.NONE:
                                break;
                            default:
                                break;
                        }
                        break;
                    case (int)EMessage.WRITE:
                        byte[] writeBuf = (byte[])msg.Obj;
                        // construct a string
                        var writeMessage = new Java.Lang.String(writeBuf);
                        bluetoothChat.conversationArrayAdapter.Add("ME: " + writeMessage);
                        break;
                    case (int)EMessage.READ:
                        byte[] readBuf = (byte[])msg.Obj;
                        // construct a string
                        var readMessage = new Java.Lang.String(readBuf, 0, msg.Arg1);
                        bluetoothChat.conversationArrayAdapter.Add(bluetoothChat.connectedDeviceName + ": ", readMessage);
                        break;
                    case (int)EMessage.DEVICE_NAME:
                        // save connected device
                        bluetoothChat.connectedDeviceName = msg.Data.GetString(DEVICE_NAME);
                        Toast.MakeText(Application.Context, "Connected to " + bluetoothChat.connectedDeviceName, ToastLength.Short).Show();
                        break;
                    case (int)EMessage.TOAST:
                        Toast.MakeText(ApplicationException.Context, msg.Data.GetString(TOAST), ToastLength.Short).Show();
                        break;
                    default:
                        break;
                }
            }
        }

        protected override void OnActivityResult(int requestCode, Result resulteCode, Intent data)
        {
            switch (requestCode)
            {
                case (int)ERequest.CONNECT_DEVICE:
                    if (resultCode == Result.Ok)
                    {
                        var address = data.Extras.GetString(DeviceListActivity.EXTRA_DEVICE_ADDRESS);
                        BluetoothDevice device = bluetoothAdapter.GetRemoteDevice(address);
                        chatService.Connect(device);
                    }
                    break;
                case (int)ERequest.ENABLE_BT:
                    if (resultCode == Result.Ok)
                    {
                        SetupChat();
                    }
                    else
                    {
                        Toast.MakeText(this, Resource.String.bt_not_enabled_leaving, ToastLength.Short).Show();
                        Finish();
                    }
                    break;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.option_menu, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.scan:
                    var serverIntent = new Intent(this, typeof(DeviceListActivity));
                    StartActivityForResult(serverIntent, REQUEST_CONNECT_DEVICE);
                    return true;
                case Resource.Id.discoverable:
                    EnsureDiscoverable();
                    return true;
            }
            return false;
        }
    }
}