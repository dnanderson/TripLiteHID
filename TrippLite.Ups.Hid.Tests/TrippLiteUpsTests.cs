using Moq;

namespace TrippLite.Ups.Hid.Tests;

public class TrippLiteUpsTests
{
    private readonly Mock<IHidDevice> _mockDevice;
    private readonly Mock<IHidStream> _mockStream;

    public TrippLiteUpsTests()
    {
        _mockDevice = new Mock<IHidDevice>();
        _mockStream = new Mock<IHidStream>();

        // When Open is called on the mock device, return the mock stream.
        _mockDevice.Setup(d => d.Open()).Returns(_mockStream.Object);
    }

    private void SetupGetFeature(byte reportId, byte[] returnedData)
    {
        _mockStream.Setup(s => s.GetFeature(It.Is<byte[]>(b => b[0] == reportId)))
            .Callback((byte[] buffer) =>
            {
                // The device echoes back the report ID in the first byte,
                // and the actual data follows.
                buffer[0] = reportId;
                Array.Copy(returnedData, 0, buffer, 1, returnedData.Length);
            });
    }

    [Fact]
    public void Constructor_WithDevice_OpensStream()
    {
        // Arrange

        // Act
        var ups = new TrippLiteUps(_mockDevice.Object);

        // Assert
        _mockDevice.Verify(d => d.Open(), Times.Once());
        Assert.NotNull(ups);
    }

    [Fact]
    public void Dispose_DisposesStream()
    {
        // Arrange
        var ups = new TrippLiteUps(_mockDevice.Object);

        // Act
        ups.Dispose();

        // Assert
        _mockStream.Verify(s => s.Dispose(), Times.Once());
    }

    [Fact]
    public void FromIdString_WithInvalidFormat_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => TrippLiteUps.FromIdString("invalid-id"));
    }

    [Fact]
    public void FromIdString_WithNullOrEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => TrippLiteUps.FromIdString(null));
        Assert.Throws<ArgumentException>(() => TrippLiteUps.FromIdString(""));
        Assert.Throws<ArgumentException>(() => TrippLiteUps.FromIdString(" "));
    }

    [Fact]
    public void FromIdString_WithValidId_ButNoDevice_ThrowsInvalidOperationException()
    {
        // This test relies on the fact that FindDevices will return an empty list
        // when run in the test environment without a physical device.
        Assert.Throws<InvalidOperationException>(() => TrippLiteUps.FromIdString("09ae:2012"));
    }

    [Fact]
    public void ConfiguredVoltage_ReturnsCorrectValue()
    {
        // Arrange
        byte reportId = 48;
        byte[] data = { 120 }; // 120V
        SetupGetFeature(reportId, data);
        var ups = new TrippLiteUps(_mockDevice.Object);

        // Act
        var result = ups.ConfiguredVoltage;

        // Assert
        Assert.Equal(120, result);
    }

    [Fact]
    public void ConfiguredFrequency_ReturnsCorrectValue()
    {
        // Arrange
        byte reportId = 2;
        byte[] data = { 60 }; // 60Hz
        SetupGetFeature(reportId, data);
        var ups = new TrippLiteUps(_mockDevice.Object);

        // Act
        var result = ups.ConfiguredFrequency;

        // Assert
        Assert.Equal(60, result);
    }

    [Fact]
    public void ConfiguredPower_ReturnsCorrectValue()
    {
        // Arrange
        byte reportId = 3;
        byte[] data = BitConverter.GetBytes((short)1500); // 1500W
        SetupGetFeature(reportId, data);
        var ups = new TrippLiteUps(_mockDevice.Object);

        // Act
        var result = ups.ConfiguredPower;

        // Assert
        Assert.Equal(1500, result);
    }

    [Fact]
    public void InputVoltage_ReturnsCorrectValue()
    {
        // Arrange
        byte reportId = 24;
        // The value is a 16-bit integer that should be divided by 10.
        // So, 120.5V is stored as 1205.
        byte[] data = BitConverter.GetBytes((short)1205);
        SetupGetFeature(reportId, data);
        var ups = new TrippLiteUps(_mockDevice.Object);

        // Act
        var result = ups.InputVoltage;

        // Assert
        Assert.Equal(120.5, result);
    }

    [Fact]
    public void Status_ReturnsCorrectlyParsedStatus()
    {
        // Arrange
        byte reportId = 50;
        byte statusByte = 0b01000010; // AC Present, Fully Charged
        byte[] data = { statusByte };
        SetupGetFeature(reportId, data);
        var ups = new TrippLiteUps(_mockDevice.Object);

        // Act
        var status = ups.Status;

        // Assert
        Assert.True(status.IsAcPresent);
        Assert.True(status.IsFullyCharged);
        Assert.False(status.IsDischarging);
    }
}
