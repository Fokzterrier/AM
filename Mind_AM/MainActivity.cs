
namespace Mind_AM
{
    using Android.App;
    using Android.OS;
    using Android.Runtime;
    using Android.Widget;
    using Android.Hardware;
    using Infrastructure;
    using Constants;
    using System.Threading;

    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]

    public class MainActivity : Activity,ISensorEventListener
    {
        SensorManager sm;
        Sensor s;
        bool programRunning = false;
        private MindstormCommunicator communicator = new MindstormCommunicator();
        float proximityValue;
        protected override void OnCreate(Bundle bundle) 
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.main);

            communicator.Connect();

            sm = (SensorManager)this.GetSystemService(SensorService);
            if (sm.GetSensorList(SensorType.Proximity).Count != 0)
            {
                s = sm.GetDefaultSensor(SensorType.Proximity);
                sm.RegisterListener(this, s, SensorDelay.Fastest);
            }


            var buttonTestSound = FindViewById<Button>(Resource.Id.ButtonTestSound);
        
            buttonTestSound.Click += (s, e) =>
            {
                var message = MindstormCommandService.GetToneMessage(3500, 1000);

                communicator.WriteMessage(message);
            };

            var buttonStartMotor = FindViewById<Button>(Resource.Id.ButtonStartMotor);

            buttonStartMotor.Click += (s, e) =>
            {
                var speed = 50;
                programRunning = true;
                MoveForward(speed);
            };

            var buttonStopMotor = FindViewById<Button>(Resource.Id.ButtonStopMotor);

            buttonStopMotor.Click += (s, e) =>
            {
                programRunning = false;
                Stop();
            };


            var buttonReverse = FindViewById<Button>(Resource.Id.ButtonReverse);

            buttonReverse.Click += (s, e) =>
            {
                var speed = -50;
                MoveReverse(speed);
            };

        }


        void ISensorEventListener.OnSensorChanged(SensorEvent e)
        {
            e.Sensor = sm.GetDefaultSensor(SensorType.Proximity);
            proximityValue = e.Values[0];
            Toast.MakeText(this, proximityValue.ToString("0.00"), ToastLength.Short).Show();
            if (programRunning)
            {
                if (proximityValue == 0)
                {
                    sm.UnregisterListener(this);
                    var message = MindstormCommandService.GetToneMessage(1500, 1500);
                    communicator.WriteMessage(message);
                    MoveReverse(-50);
                    sm.RegisterListener(this, s, SensorDelay.Fastest);

                }
            }
        }
        void ISensorEventListener.OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
            //throw new NotImplementedException();
        }

        void MoveForward(int speed)
        {
            var message = MindstormCommandService.GetMotorMessage(MindstormComponents.MotorA, speed);
            var message2 = MindstormCommandService.GetMotorMessage(MindstormComponents.MotorB, speed - 10);

            communicator.WriteMessage(message);
            communicator.WriteMessage(message2);
        }

        void Stop()
        {
            var message = MindstormCommandService.GetMotorMessage(MindstormComponents.MotorA, 0);
            var message2 = MindstormCommandService.GetMotorMessage(MindstormComponents.MotorB, 0);

            communicator.WriteMessage(message);
            communicator.WriteMessage(message2);
        }

        void MoveReverse(int speed)
        {
            MoveForward(speed);
            Thread.Sleep(1000);
            var message = MindstormCommandService.GetMotorMessage(MindstormComponents.MotorA, 0);
            communicator.WriteMessage(message);
            Thread.Sleep(1150);
            var message2 = MindstormCommandService.GetMotorMessage(MindstormComponents.MotorB, 0);
            communicator.WriteMessage(message2);
            MoveForward(50);
        }
    }
}