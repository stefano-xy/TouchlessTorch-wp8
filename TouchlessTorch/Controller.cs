using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Phone.Media.Capture;

namespace TouchlessTorch
{
    public class Controller
    {
        public enum StatusEnum
        {
            Uninitialized,
            Ready,
            CannotFindBackCamera,
            CannotFindValidResolutionForCamera,
            CannotAccessCameraDevice,
            CannotFindTorchOnDevice
        }

        const CameraSensorLocation _location = CameraSensorLocation.Back;

        AudioVideoCaptureDevice _device = null;

        StatusEnum _status = StatusEnum.Uninitialized;

        public Controller()
        {

        }

        public async Task InitAsync()
        {
            Debug.Assert(_status == StatusEnum.Uninitialized);

            if (AudioVideoCaptureDevice.AvailableSensorLocations.Contains(_location))
            {
                var resolutions = AudioVideoCaptureDevice.GetAvailableCaptureResolutions(_location);

                if (resolutions.Count > 0)
                {
                    var resolution = resolutions.First();

                    _device = await AudioVideoCaptureDevice.OpenForVideoOnlyAsync(_location, resolution);

                    if ((object)_device != null)
                    {
                        var values = AudioVideoCaptureDevice.GetSupportedPropertyValues(_location, KnownCameraAudioVideoProperties.VideoTorchMode);

                        if (values.Contains((UInt32)VideoTorchMode.On))
                        {
                            // Done, it looks like we have it!

                            _status = StatusEnum.Ready;
                        }
                        else
                        {
                            _status = StatusEnum.CannotFindTorchOnDevice;
                            _device.Dispose();
                            _device = null;
                        }
                    }
                    else
                    {
                        _status = StatusEnum.CannotAccessCameraDevice;
                    }
                }
                else
                {
                    _status = StatusEnum.CannotFindValidResolutionForCamera;
                }
            }
            else
            {
                _status = StatusEnum.CannotFindBackCamera;
            }
        }

        public void Term()
        {
            if (_status == StatusEnum.Ready)
            {
                _device.Dispose();
                _device = null;
            }

            _status = StatusEnum.Uninitialized;
        }

        public bool Ready
        {
            get { return _status == StatusEnum.Ready; }
        }
        
        public StatusEnum Status
        {
            get { return _status; }
        }

        public bool Enabled
        {
            set
            {
                Debug.Assert(_status == StatusEnum.Ready);
                Debug.Assert((object)_device != null);
                
                if (value)
                {
                    _device.SetProperty(KnownCameraAudioVideoProperties.VideoTorchMode, VideoTorchMode.On);
                    _device.SetProperty(KnownCameraAudioVideoProperties.VideoTorchPower,
                        AudioVideoCaptureDevice.GetSupportedPropertyRange(_location, KnownCameraAudioVideoProperties.VideoTorchPower).Max);
                }
                else
                {
                    _device.SetProperty(KnownCameraAudioVideoProperties.VideoTorchMode, VideoTorchMode.Off);
                    _device.SetProperty(KnownCameraAudioVideoProperties.VideoTorchPower,
                        AudioVideoCaptureDevice.GetSupportedPropertyRange(_location, KnownCameraAudioVideoProperties.VideoTorchPower).Min);
                }
            }
        }
    }
}
