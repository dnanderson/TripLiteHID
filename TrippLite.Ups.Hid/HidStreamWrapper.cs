using HidSharp;
using System;

namespace TrippLite.Ups.Hid
{
    public class HidStreamWrapper : IHidStream
    {
        private readonly HidStream _stream;

        public HidStreamWrapper(HidStream stream)
        {
            _stream = stream;
        }

        public int ReadTimeout
        {
            get => _stream.ReadTimeout;
            set => _stream.ReadTimeout = value;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public void GetFeature(byte[] buffer)
        {
            _stream.GetFeature(buffer);
        }
    }
}
