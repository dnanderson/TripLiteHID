using HidSharp;

namespace TrippLite.Ups.Hid
{
    public class HidDeviceWrapper : IHidDevice
    {
        private readonly HidDevice _device;

        public HidDeviceWrapper(HidDevice device)
        {
            _device = device;
        }

        public string DevicePath => _device.DevicePath;

        public IHidStream Open()
        {
            return new HidStreamWrapper(_device.Open());
        }
    }
}
