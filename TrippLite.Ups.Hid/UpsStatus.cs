using System;

namespace TrippLite.Ups.Hid
{
    /// <summary>
    /// Represents the collective status of the UPS, read from a single feature report.
    /// </summary>
    public class UpsStatus
    {
        private readonly byte _statusByte;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpsStatus"/> class.
        /// </summary>
        /// <param name="statusByte">The byte containing the status flags from the device.</param>
        internal UpsStatus(byte statusByte)
        {
            _statusByte = statusByte;
        }

        /// <summary>Gets a value indicating whether the UPS has signaled that a shutdown is imminent.</summary>
        public bool IsShutdownImminent => (_statusByte & (1 << 0)) != 0;

        /// <summary>Gets a value indicating whether AC power is currently present.</summary>
        public bool IsAcPresent => (_statusByte & (1 << 1)) != 0;

        /// <summary>Gets a value indicating whether the battery is currently charging.</summary>
        public bool IsCharging => (_statusByte & (1 << 2)) != 0;

        /// <summary>Gets a value indicating whether the battery is currently discharging (on battery power).</summary>
        public bool IsDischarging => (_statusByte & (1 << 3)) != 0;

        /// <summary>Gets a value indicating whether the UPS has signaled that the battery needs replacement.</summary>
        public bool NeedsReplacement => (_statusByte & (1 << 4)) != 0;

        /// <summary>Gets a value indicating whether the battery capacity is below the configured remaining capacity threshold.</summary>
        public bool IsBelowRemainingCapacity => (_statusByte & (1 << 5)) != 0;

        /// <summary>Gets a value indicating whether the battery is fully charged.</summary>
        public bool IsFullyCharged => (_statusByte & (1 << 6)) != 0;

        /// <summary>Gets a value indicating whether the battery is fully discharged.</summary>
        public bool IsFullyDischarged => (_statusByte & (1 << 7)) != 0;
    }
}
