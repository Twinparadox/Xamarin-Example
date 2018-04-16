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
using Android;
using Android.Content.PM;
using Android.Support.V4.Content;
using Android.Support.V4.App;
using Android.Support.Design.Widget;
using System.Threading.Tasks;

namespace SimpleLighter
{
    [Activity(Label = "SimpleLighter", MainLauncher = true, Icon = "@drawable/SimpleLighterIcon", Theme = "@android:style/Theme.NoTitleBar")]
    public class MainActivity : Activity
    {
        private bool isFlashOn = false;

        private bool hasCamera;
        private bool hasFlash;

        private Camera camera;
        private Parameters mParams;
        private MediaPlayer player;

        private LinearLayout layout;

        readonly string[] permissions = 
        {
            Manifest.Permission.Camera
        };
        
        private enum EPermission
        {
            PCAMERA
        };

        private Permission[] resultPermissions;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            
            layout = FindViewById<LinearLayout>(Resource.Id.layout);

            hasFlash = ApplicationContext.PackageManager.HasSystemFeature(Android.Content.PM.PackageManager.FeatureCameraFlash);
            
            if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Denied && resultPermissions==null)
            {
                GetCameraPermission();
            }

            Toast.MakeText(this, "플래시를 켜려면 짧게 터치해주세요.", ToastLength.Long).Show();
            GetCamera();
            layout.LongClick += delegate
            {
                TurnOffFlash();
            };
            layout.Click += delegate
            {
                TurnOnFlash();
            };
        }

        private void GetCameraPermission()
        {
            if ((int)Build.VERSION.SdkInt >= 23)
            {
                Toast.MakeText(this, "SDK 23 이상", ToastLength.Long).Show();
                RequestPermissions(permissions, (int)EPermission.PCAMERA);
                OnRequestPermissionsResult((int)EPermission.PCAMERA, permissions, resultPermissions);
            }
            else
            {
                Toast.MakeText(this, "카메라 접근을 승인해주세요.", ToastLength.Long).Show();
            }
        }

        // 플래시 버튼 클릭 이벤트
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
                Toast.MakeText(this, "플래시를 켜려면 짧게 터치해주세요.", ToastLength.Long).Show();
                FlashLight(true);
            }
        }

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
                Toast.MakeText(this, "플래시를 끄려면 길게 터치해주세요.", ToastLength.Long).Show();
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
            {
                return;
            }
            if (isFlashOn || flash)
            {
                mParams = camera.GetParameters();
                mParams.FlashMode = Parameters.FlashModeOff;
                camera.SetParameters(mParams);
                camera.StartPreview();
                isFlashOn = false;
                ChangeBackground();
            }
            else
            {
                mParams = camera.GetParameters();
                mParams.FlashMode = Parameters.FlashModeTorch;
                camera.SetParameters(mParams);
                camera.StartPreview();
                isFlashOn = true;
                ChangeBackground();
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

