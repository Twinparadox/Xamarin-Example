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
    [Activity(Label = "SimpleLighter", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private bool isDisplayOn = false;
        private bool isFlashOn = false;

        private bool hasFlash;

        private Camera camera;
        private Parameters mParams;
        private MediaPlayer player;

        private ImageButton btnDisplay;
        private ImageButton btnFlash;

        private float prevBrightness;

        private WindowManagerLayoutParams lp;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            
            btnDisplay = FindViewById<ImageButton>(Resource.Id.btnDisplay);
            btnFlash = FindViewById<ImageButton>(Resource.Id.btnFlash);

            hasFlash = ApplicationContext.PackageManager.HasSystemFeature(Android.Content.PM.PackageManager.FeatureCameraFlash);
            lp = Window.Attributes;

            SettingButtonSize();
            GetCamera();

            btnDisplay.Click += DisplayChange;
            btnFlash.Click += FlashChange;
        }

        // 디스플레이 버튼 클릭 이벤트
        private void DisplayChange(object sender, EventArgs e)
        {
            if(isDisplayOn)
            {
                lp.ScreenBrightness = prevBrightness;
                isDisplayOn = false;
                ChangeButton();
            }
            else
            {
                prevBrightness = lp.ScreenBrightness;
                lp.ScreenBrightness = -100;
                isDisplayOn = true;
                ChangeButton();
            }
        }

        // 플래시 버튼 클릭 이벤트
        private void FlashChange(object sender, EventArgs e)
        {
            if (!hasFlash)
            {
                AlertDialog alert = new AlertDialog.Builder(this).Create();
                alert.SetTitle("오류");
                alert.SetMessage("기기에 카메라 플래시가 존재하지 않습니다.");
                alert.Show();
            }
            FlashLight();
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
        private void FlashLight()
        {
            if (camera == null || mParams == null)
                return;
            if (isFlashOn)
            {
                mParams = camera.GetParameters();
                mParams.FlashMode = Parameters.FlashModeOff;
                camera.SetParameters(mParams);
                camera.StartPreview();
                isFlashOn = false;
                ChangeButton();
            }
            else
            {
                mParams = camera.GetParameters();
                mParams.FlashMode = Parameters.FlashModeTorch;
                camera.SetParameters(mParams);
                camera.StartPreview();
                isFlashOn = true;
                ChangeButton();
            }
        }

        // 버튼 크기 조정
        private void SettingButtonSize()
        {
            var metrics = Resources.DisplayMetrics;
            var heightInDp = ConvertPixelsToDp(metrics.HeightPixels);

            btnDisplay.SetMaxHeight(heightInDp / 2 - 1);
            btnDisplay.SetMinimumHeight(heightInDp / 2 - 1);
            btnFlash.SetMaxHeight(heightInDp / 2 - 1);
            btnFlash.SetMinimumHeight(heightInDp / 2 - 1);
        }

        // 픽셀에서 Dp로
        private int ConvertPixelsToDp(float pixelValue)
        {
            var dp = (int)((pixelValue) / Resources.DisplayMetrics.Density);
            return dp;
        }

        // 버튼 이미지 변경
        private void ChangeButton()
        {
            if (isDisplayOn)
                btnDisplay.SetImageResource(Resource.Drawable.DisplayOFF);
            else
                btnDisplay.SetImageResource(Resource.Drawable.DisplayON);
            if (isFlashOn)
                btnFlash.SetImageResource(Resource.Drawable.FlashLightOFF);
            else
                btnFlash.SetImageResource(Resource.Drawable.FlashLightON);
        }
    }
}

