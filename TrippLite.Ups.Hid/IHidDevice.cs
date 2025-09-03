namespace TrippLite.Ups.Hid
{
    public interface IHidDevice
    {
        IHidStream Open();
        string DevicePath { get; }
    }
}
