using Android.App;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using Android.Views;
using Java.Lang;
using Android.Content;
using Android.Views.InputMethods;

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
            M_STATE_CHANGE=1,
            M_READ=2,
            M_WRITE=3,
            M_DEVICE_NAME=4,
            M_TOAST=5
        }

        // Request codes
        private enum ERequest
        {
            R_CONNECT_DEVICE=1,
            R_ENABLE_BT=2
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
        private BluetoothCahtService chatService = null;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

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
                StartActivityForResult(enableIntent, (int)ERequest.R_CONNECT_DEVICE);
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
                if (chatService.GetState() == BluetoothChatService.STATE_NONE)
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
                      var message = new String(((TextView)sender).Text);
                      SendMessage(message);
                  }
              };

            btnSend.Click += delegate (object sender, EventArgs e)
              {
                  var message = String(lvChatlog.Text);
                  SendMessage(message);
              };

            // Initialize the chatService to perform bluetooth connections
            chatService = new BluetoothCahtService(this, new MyHandler(this));

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
        
        private void SendMessage(String message)
        {
            // check if connection is available
            if(chatService.GetState()!=BluetoothChatService.STATE_CONNECTED)
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
                base.HandleMessage(msg);
            }
        }
    }
}

