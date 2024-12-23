﻿using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public abstract partial class MachineVisionViewModelBase
    {
        VideoCapture _topCameraCapture;
        VideoCapture _bottomCameraCapture;

        Object _videoCaptureLocker = new object();

        public enum MVLocatorState
        {
            Idle,
            MachineFidicual,
            BoardFidicual1,
            BoardFidicual2,
            Default,
            NozzleCalibration,
            WorkHome,

        }

        private MVLocatorState _mvLocatorState = MVLocatorState.Default;
        public MVLocatorState LocatorState
        {
            get => _mvLocatorState;
            set => _mvLocatorState = value;
        }

        private string _status;
        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        private VideoCapture InitCapture(int cameraIndex)
        {
            try
            {
                return new VideoCapture(cameraIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open camera: " + ex.Message);
                return null;
            }
        }

        private bool _running;

        private double _lastTopBrightness = -9999;
        private double _lastBottomBrightness = -9999;


        private double _lastTopFocus = -9999;
        private double _lastBottomFocus = -9999;

        private double _lastTopExposure = -9999;
        private double _lastBottomExposure = -9999;

        private double _lastTopContrast = -9999;
        private double _lastBottomContrast = -9999;

        public virtual bool UseTopCamera { get; set; } = true;
        public virtual bool UseBottomCamera { get; set; } = false;

        private async void StartImageRecognization()
        {
            _running = true;

            while (_running)
            {
                if (!UseTopCamera && !UseBottomCamera)
                {
                    PrimaryCapturedImage = new BitmapImage(new Uri("/Imgs/TestPattern.jpg", UriKind.Relative));
                    SecondaryCapturedImage = new BitmapImage(new Uri("/Imgs/TestPattern.jpg", UriKind.Relative));
                }
                else if (UseTopCamera && _topCameraCapture != null)
                {
                    //_topCameraCapture.Set(Emgu.CV.CvEnum.CapProp.AutoExposure, 1);
                    _topCameraCapture.Set(Emgu.CV.CvEnum.CapProp.Autofocus, 1);

                    if (_lastTopBrightness != _topCameraProfile.Brightness)
                    {
                        _topCameraCapture.Set(Emgu.CV.CvEnum.CapProp.Brightness, _topCameraProfile.Brightness);
                        _lastTopBrightness = _topCameraProfile.Brightness;
                    }

                    if (_lastTopFocus != _topCameraProfile.Focus)
                    {

                        _topCameraCapture.Set(Emgu.CV.CvEnum.CapProp.Focus, _topCameraProfile.Focus);
                        _lastTopFocus = _topCameraProfile.Focus;
                    }

                    if (_lastTopContrast != _topCameraProfile.Contrast)
                    {
                        _topCameraCapture.Set(Emgu.CV.CvEnum.CapProp.Contrast, _topCameraProfile.Contrast);
                        _lastTopContrast = _topCameraProfile.Contrast;
                    }

                    if (_lastTopExposure != _topCameraProfile.Exposure)
                    {
                        _topCameraCapture.Set(Emgu.CV.CvEnum.CapProp.Exposure, _topCameraProfile.Exposure);
                        _lastTopExposure = _topCameraProfile.Exposure;
                    }

                    HasFrame = true;

                    if (LoadingMask)
                    {
                        LoadingMask = false;
                    }

                    if (UseTopCamera)
                    {
                        using (var originalFrame = _topCameraCapture.QueryFrame())
                        {
                            if (originalFrame != null)
                            {

                                using (var img = originalFrame.ToImage<Bgr, byte>())
                                {
                                    using (var results = PerformShapeDetection(img))
                                    {
                                        PrimaryCapturedImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(results);
                                    }
                                }

                                if (PictureInPicture && _bottomCameraCapture != null)
                                {
                                    using (var childFrame = _bottomCameraCapture.QueryFrame())
                                    {
                                        SecondaryCapturedImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(childFrame);
                                    }
                                }
                                else
                                {
                                    SecondaryCapturedImage = null;
                                }
                            }
                        }
                    }
                }
                else if (UseBottomCamera && _bottomCameraCapture != null)
                {
                    //  _bottomCameraCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);

                    if (_lastBottomBrightness != _bottomCameraProfile.Brightness)
                    {
                        _bottomCameraCapture.Set(Emgu.CV.CvEnum.CapProp.Brightness, _bottomCameraProfile.Brightness);
                        _lastBottomBrightness = _bottomCameraProfile.Brightness;
                    }

                    if (_lastBottomFocus != _bottomCameraProfile.Focus)
                    {
                        _bottomCameraCapture.Set(Emgu.CV.CvEnum.CapProp.Focus, _bottomCameraProfile.Focus);
                        _lastBottomFocus = _bottomCameraProfile.Focus;
                    }

                    if (_lastBottomContrast != _bottomCameraProfile.Contrast)
                    {
                        _bottomCameraCapture.Set(Emgu.CV.CvEnum.CapProp.Contrast, _bottomCameraProfile.Contrast);
                        _lastBottomContrast = _bottomCameraProfile.Contrast;
                    }

                    

                    if (_lastBottomExposure != _bottomCameraProfile.Exposure)
                    {
                        _bottomCameraCapture.Set(Emgu.CV.CvEnum.CapProp.Exposure, _bottomCameraProfile.Exposure);
                        _lastBottomExposure = _bottomCameraProfile.Exposure;
                    }

                    if (UseBottomCamera)
                    {
                        using (var originalFrame = _bottomCameraCapture.QueryFrame())
                            if (originalFrame != null)
                            {
                                using (var results = PerformShapeDetection(originalFrame.ToImage<Bgr, byte>()))
                                {
                                    PrimaryCapturedImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(results);
                                }

                                if (PictureInPicture && _topCameraCapture != null)
                                {
                                    using (var innerFrame = _topCameraCapture.QueryFrame())
                                    {
                                        SecondaryCapturedImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(innerFrame);
                                    }
                                }
                                else
                                {
                                    SecondaryCapturedImage = null;
                                }
                            }
                    }

                    HasFrame = true;

                    if (LoadingMask)
                    {
                        LoadingMask = false;
                    }
                }

                await Task.Delay(50);
            }


            HasFrame = false;
        }

        public void StartCapture()
        {
            if (_topCameraCapture != null || _bottomCameraCapture != null)
            {
                return;
            }

            if (Machine.Settings.PositioningCamera == null && Machine.Settings.PartInspectionCamera == null)
            {
                MessageBox.Show("Please Select a Camera");
                new SettingsWindow(Machine, Machine.Settings, 2).ShowDialog();
                return;
            }

            try
            {
                LoadingMask = true;

                var positionCameraIndex = Machine.Settings.PositioningCamera == null ? null : (int?)Machine.Settings.PositioningCamera.CameraIndex;
                var inspectionCameraIndex = Machine.Settings.PartInspectionCamera == null ? null : (int?)Machine.Settings.PartInspectionCamera.CameraIndex;

                if (positionCameraIndex.HasValue && inspectionCameraIndex.HasValue)
                {
                    if (positionCameraIndex.Value < inspectionCameraIndex.Value)
                    {
                        _topCameraCapture = InitCapture(Machine.Settings.PositioningCamera.CameraIndex);
                        _bottomCameraCapture = InitCapture(Machine.Settings.PartInspectionCamera.CameraIndex);
                    }
                    else
                    {
                        _bottomCameraCapture = InitCapture(Machine.Settings.PartInspectionCamera.CameraIndex);
                        _topCameraCapture = InitCapture(Machine.Settings.PositioningCamera.CameraIndex);                        
                    }

                    _topCameraCapture.Set(Emgu.CV.CvEnum.CapProp.FrameWidth, 1920);
                    _topCameraCapture.Set(Emgu.CV.CvEnum.CapProp.FrameHeight, 1080);

                    _topCameraCapture.Set(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);
                    _bottomCameraCapture.Set(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);

                    StartImageRecognization();
                }
                else if (positionCameraIndex.HasValue)
                {
                    _topCameraCapture = InitCapture(Machine.Settings.PositioningCamera.CameraIndex);
                    StartImageRecognization();
                }
                else if (inspectionCameraIndex.HasValue)
                {
                    _bottomCameraCapture = InitCapture(Machine.Settings.PartInspectionCamera.CameraIndex);
                    StartImageRecognization();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not start video, please restart your application: " + ex.Message);
            }

        }

        public void StopCapture()
        {
            try
            {
                _running = false;

                lock (_videoCaptureLocker)
                {
                    if (_topCameraCapture != null)
                    {
                        _topCameraCapture.Stop();
                        _topCameraCapture.Dispose();
                        _topCameraCapture = null;
                    }

                    if (_bottomCameraCapture != null)
                    {
                        _bottomCameraCapture.Stop();
                        _bottomCameraCapture.Dispose();
                        _bottomCameraCapture = null;
                    }
                }

                PrimaryCapturedImage = new BitmapImage(new Uri("pack://application:,,/Imgs/TestPattern.jpg"));
                SecondaryCapturedImage = new BitmapImage(new Uri("pack://application:,,/Imgs/TestPattern.jpg"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Shutting Down Video, please restart the application." + ex.Message);
            }
            finally
            {
            }
        }

    }
}
