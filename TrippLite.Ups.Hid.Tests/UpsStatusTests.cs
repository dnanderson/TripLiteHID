namespace TrippLite.Ups.Hid.Tests;

public class UpsStatusTests
{
    [Theory]
    [InlineData(0b00000001, true)]
    [InlineData(0b00000000, false)]
    [InlineData(0b11111110, false)]
    public void IsShutdownImminent_IsCorrect(byte statusByte, bool expected)
    {
        var status = new UpsStatus(statusByte);
        Assert.Equal(expected, status.IsShutdownImminent);
    }

    [Theory]
    [InlineData(0b00000010, true)]
    [InlineData(0b00000000, false)]
    [InlineData(0b11111101, false)]
    public void IsAcPresent_IsCorrect(byte statusByte, bool expected)
    {
        var status = new UpsStatus(statusByte);
        Assert.Equal(expected, status.IsAcPresent);
    }

    [Theory]
    [InlineData(0b00000100, true)]
    [InlineData(0b00000000, false)]
    [InlineData(0b11111011, false)]
    public void IsCharging_IsCorrect(byte statusByte, bool expected)
    {
        var status = new UpsStatus(statusByte);
        Assert.Equal(expected, status.IsCharging);
    }

    [Theory]
    [InlineData(0b00001000, true)]
    [InlineData(0b00000000, false)]
    [InlineData(0b11110111, false)]
    public void IsDischarging_IsCorrect(byte statusByte, bool expected)
    {
        var status = new UpsStatus(statusByte);
        Assert.Equal(expected, status.IsDischarging);
    }

    [Theory]
    [InlineData(0b00010000, true)]
    [InlineData(0b00000000, false)]
    [InlineData(0b11101111, false)]
    public void NeedsReplacement_IsCorrect(byte statusByte, bool expected)
    {
        var status = new UpsStatus(statusByte);
        Assert.Equal(expected, status.NeedsReplacement);
    }

    [Theory]
    [InlineData(0b00100000, true)]
    [InlineData(0b00000000, false)]
    [InlineData(0b11011111, false)]
    public void IsBelowRemainingCapacity_IsCorrect(byte statusByte, bool expected)
    {
        var status = new UpsStatus(statusByte);
        Assert.Equal(expected, status.IsBelowRemainingCapacity);
    }

    [Theory]
    [InlineData(0b01000000, true)]
    [InlineData(0b00000000, false)]
    [InlineData(0b10111111, false)]
    public void IsFullyCharged_IsCorrect(byte statusByte, bool expected)
    {
        var status = new UpsStatus(statusByte);
        Assert.Equal(expected, status.IsFullyCharged);
    }

    [Theory]
    [InlineData(0b10000000, true)]
    [InlineData(0b00000000, false)]
    [InlineData(0b01111111, false)]
    public void IsFullyDischarged_IsCorrect(byte statusByte, bool expected)
    {
        var status = new UpsStatus(statusByte);
        Assert.Equal(expected, status.IsFullyDischarged);
    }

    [Fact]
    public void AllFlags_AreSetCorrectly()
    {
        var status = new UpsStatus(0b11111111);
        Assert.True(status.IsShutdownImminent);
        Assert.True(status.IsAcPresent);
        Assert.True(status.IsCharging);
        Assert.True(status.IsDischarging);
        Assert.True(status.NeedsReplacement);
        Assert.True(status.IsBelowRemainingCapacity);
        Assert.True(status.IsFullyCharged);
        Assert.True(status.IsFullyDischarged);
    }

    [Fact]
    public void NoFlags_AreSetCorrectly()
    {
        var status = new UpsStatus(0b00000000);
        Assert.False(status.IsShutdownImminent);
        Assert.False(status.IsAcPresent);
        Assert.False(status.IsCharging);
        Assert.False(status.IsDischarging);
        Assert.False(status.NeedsReplacement);
        Assert.False(status.IsBelowRemainingCapacity);
        Assert.False(status.IsFullyCharged);
        Assert.False(status.IsFullyDischarged);
    }
}
