using System;

namespace TrippLite.Ups.Hid
{
    public interface IHidStream : IDisposable
    {
        void GetFeature(byte[] buffer);
        int ReadTimeout { get; set; }
    }
}
