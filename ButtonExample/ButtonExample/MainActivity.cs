using Android.App;
using Android.Widget;
using Android.OS;
using System;

namespace ButtonExample
{
    [Activity(Label = "ButtonExample", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            RadioButton radioRed = FindViewById<RadioButton>(Resource.Id.radio_red);
            RadioButton radioBlue = FindViewById<RadioButton>(Resource.Id.radio_blue);
        }

        private void RadioButtonClick(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            Toast.MakeText(this, rb.Text, ToastLength.Short).Show();
        }
    }
}

