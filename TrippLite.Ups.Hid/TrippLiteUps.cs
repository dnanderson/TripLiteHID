using HidSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace TrippLite.Ups.Hid
{
    /// <summary>
    /// Provides a driver for interacting with a TrippLite UPS device over USB HID.
    /// </summary>
    public class TrippLiteUps : IDisposable
    {
        private readonly IHidDevice _device;
        private IHidStream _stream;

        /// <summary>
        /// The default TrippLite Vendor ID.
        /// </summary>
        public const int DefaultVendorId = 0x09ae;

        /// <summary>
        /// The Product ID for the TrippLite model this driver was based on.
        /// Other models may have different Product IDs.
        /// </summary>
        public const int DefaultProductId = 0x2012;

        /// <summary>
        /// Gets the underlying path of the HID device.
        /// </summary>
        public string DevicePath => _device.DevicePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrippLiteUps"/> class by automatically finding the first available TrippLite UPS.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if no compatible TrippLite device can be found.</exception>
        public TrippLiteUps()
            : this(FindDevices(DefaultVendorId).FirstOrDefault() ?? throw new InvalidOperationException("Could not find any connected TrippLite devices. Please specify VID/PID if they differ from the default."))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrippLiteUps"/> class using the specified HID device.
        /// </summary>
        /// <param name="device">The HID device to connect to.</param>
        /// <exception cref="ArgumentNullException">Thrown if the device is null.</exception>
        public TrippLiteUps(HidDevice device)
            : this(new HidDeviceWrapper(device))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrippLiteUps"/> class using the specified HID device interface.
        /// This constructor is ideal for dependency injection and testing.
        /// </summary>
        /// <param name="device">The HID device interface to connect to.</param>
        /// <exception cref="ArgumentNullException">Thrown if the device is null.</exception>
        public TrippLiteUps(IHidDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            Open();
        }

        /// <summary>
        /// Finds all connected HID devices that match the given Vendor and Product IDs.
        /// </summary>
        /// <param name="vendorId">The Vendor ID to search for.</param>
        /// <param name="productId">An optional Product ID to filter by. If null, all products from the given vendor are returned.</param>
        /// <returns>An enumerable collection of matching <see cref="HidDevice"/>s.</returns>
        public static IEnumerable<HidDevice> FindDevices(int vendorId = DefaultVendorId, int? productId = null)
        {
            var deviceList = DeviceList.Local;
            return productId.HasValue
                ? deviceList.GetHidDevices(vendorId, productId.Value)
                : deviceList.GetHidDevices(vendorId);
        }

        /// <summary>
        /// Creates a new <see cref="TrippLiteUps"/> instance from a device ID string in "VID:PID" format.
        /// </summary>
        /// <param name="deviceId">The device ID string (e.g., "09ae:2012"). IDs are parsed as hexadecimal.</param>
        /// <returns>A new <see cref="TrippLiteUps"/> instance connected to the specified device.</returns>
        /// <exception cref="ArgumentException">Thrown if the device ID string is null or empty.</exception>
        /// <exception cref="FormatException">Thrown if the device ID string is not in the correct 'VID:PID' format or contains invalid hex characters.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a device with the specified VID and PID cannot be found.</exception>
        public static TrippLiteUps FromIdString(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentException("Device ID string cannot be null or empty.", nameof(deviceId));

            var parts = deviceId.Split(':');
            if (parts.Length != 2)
                throw new FormatException("Device ID string is not in the correct format 'vendorId:productId'.");

            try
            {
                var vendorId = Convert.ToInt32(parts[0], 16);
                var productId = Convert.ToInt32(parts[1], 16);

                var device = FindDevices(vendorId, productId).FirstOrDefault();
                if (device == null)
                    throw new InvalidOperationException($"Could not find a connected device with ID {deviceId}.");

                return new TrippLiteUps(device);
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            {
                throw new FormatException("Invalid vendor or product ID in device ID string. Ensure they are valid hexadecimal numbers.", ex);
            }
        }

        private void Open()
        {
            _stream = _device.Open();
            // Set a reasonable timeout to prevent indefinite blocking.
            _stream.ReadTimeout = 3000;
        }

        #region Properties

        /// <summary>Gets the configured nominal output voltage of the UPS.</summary>
        public int ConfiguredVoltage => ReadByte(48);

        /// <summary>Gets the configured nominal frequency of the UPS.</summary>
        public int ConfiguredFrequency => ReadByte(2);

        /// <summary>Gets the configured power rating of the UPS in watts.</summary>
        public int ConfiguredPower => ReadInt16(3);

        /// <summary>Gets the current status of the UPS.</summary>
        public UpsStatus Status => new UpsStatus(ReadFeature(50, 1)[0]);

        /// <summary>Gets the current input voltage.</summary>
        public double InputVoltage => ReadFloatFromInt16(24);

        /// <summary>Gets the current input frequency.</summary>
        public double InputFrequency => ReadFloatFromInt16(25);

        /// <summary>Gets the current output voltage.</summary>
        public double OutputVoltage => ReadFloatFromInt16(27);

        /// <summary>Gets the current output power in watts.</summary>
        public int OutputPower => ReadInt16(71);

        /// <summary>Gets the battery health as a percentage.</summary>
        public int Health => ReadByte(52);

        /// <summary>Gets the estimated time to empty the battery in minutes.</summary>
        public int TimeToEmpty => ReadInt16(53);

        #endregion

        #region HID Communication

        private byte[] ReadFeature(byte reportId, int bytesToRead, int retries = 3)
        {
            if (_stream == null) throw new InvalidOperationException("Device connection is closed.");

            var buffer = new byte[bytesToRead + 1];
            buffer[0] = reportId;

            while (true)
            {
                try
                {
                    _stream.GetFeature(buffer);
                    if (buffer[0] != reportId)
                    {
                        throw new IOException($"Received unexpected report ID {buffer[0]} when expecting {reportId}.");
                    }
                    var result = new byte[bytesToRead];
                    Array.Copy(buffer, 1, result, 0, bytesToRead);
                    return result;
                }
                catch (IOException)
                {
                    if (--retries > 0)
                    {
                        Thread.Sleep(50); // Wait a moment before retrying
                        continue;
                    }
                    throw;
                }
            }
        }

        private int ReadInt16(byte reportId)
        {
            var data = ReadFeature(reportId, 2);
            return BitConverter.ToInt16(data, 0);
        }

        private double ReadFloatFromInt16(byte reportId)
        {
            // Per the Python example, the float is a 16-bit integer divided by 10.0
            return ReadInt16(reportId) / 10.0;
        }

        private int ReadByte(byte reportId)
        {
            var data = ReadFeature(reportId, 1);
            return data[0];
        }

        #endregion

        /// <summary>
        /// Releases all resources used by the <see cref="TrippLiteUps"/> object.
        /// </summary>
        public void Dispose()
        {
            _stream?.Dispose();
            _stream = null;
        }
    }
}
