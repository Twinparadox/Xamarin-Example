using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Android.Views;
using Java.Lang;
using Android.Hardware;
using static Android.Hardware.Camera;
using Android.Util;
using Android.Media;

namespace SimpleLighter
{
    [Activity(Label = "SimpleLighter", MainLauncher = true, Icon = "@drawable/SimpleLighterIcon", Theme = "@android:style/Theme.NoTitleBar")]
    public class MainActivity : Activity
    {
        private bool isFlashOn = false;

        private bool hasFlash;

        private Camera camera;
        private Parameters mParams;
        private MediaPlayer player;

        private LinearLayout layout;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            
            layout = FindViewById<LinearLayout>(Resource.Id.layout);

            hasFlash = ApplicationContext.PackageManager.HasSystemFeature(Android.Content.PM.PackageManager.FeatureCameraFlash);

            GetCamera();
            Toast.MakeText(this, "플래시를 켜려면 짧게 터치해주세요.", ToastLength.Long).Show();
            layout.LongClick += delegate
            {
                TurnOnFlash();
            };
            layout.Click += delegate
            {
                TurnOffFlash();
            };
        }

        // 플래시 버튼 클릭 이벤트
        private void TurnOnFlash()
        {
            if (!hasFlash)
            {
                AlertDialog alert = new AlertDialog.Builder(this).Create();
                alert.SetTitle("오류");
                alert.SetMessage("기기에 카메라 플래시가 존재하지 않습니다.");
                alert.Show();
                return;
            }
            else
            {
                FlashLight(true);
            }
        }

        private void TurnOffFlash()
        {
            if (!hasFlash)
            {
                AlertDialog alert = new AlertDialog.Builder(this).Create();
                alert.SetTitle("오류");
                alert.SetMessage("기기에 카메라 플래시가 존재하지 않습니다.");
                alert.Show();
                return;
            }
            else
            {
                FlashLight(false);
            }
        }

        // 카메라
        private void GetCamera()
        {
            if (camera == null)
            {
                try
                {
                    camera = Camera.Open();
                    mParams = camera.GetParameters();
                }
                catch (RuntimeException ex)
                {
                    Log.Info("Error", ex.Message);
                }
            }
        }

        // 플래시
        private void FlashLight(bool flash)
        {
            if (camera == null || mParams == null)
                return;
            if (isFlashOn || flash)
            {
                mParams = camera.GetParameters();
                mParams.FlashMode = Parameters.FlashModeOff;
                camera.SetParameters(mParams);
                camera.StartPreview();
                isFlashOn = false;
                ChangeBackground();
                Toast.MakeText(this, "플래시를 켜려면 짧게 터치해주세요.", ToastLength.Long).Show();
            }
            else
            {
                mParams = camera.GetParameters();
                mParams.FlashMode = Parameters.FlashModeTorch;
                camera.SetParameters(mParams);
                camera.StartPreview();
                isFlashOn = true;
                ChangeBackground();
                Toast.MakeText(this, "플래시를 끄려면 길게 터치해주세요.", ToastLength.Long).Show();
            }
        }

        // 버튼 이미지 및 색상 변경
        private void ChangeBackground()
        {
            if (isFlashOn)
            {
                layout.SetBackgroundColor(Android.Graphics.Color.White);
            }

            else
            {
                layout.SetBackgroundColor(Android.Graphics.Color.Black);
            }
        }
    }
}

