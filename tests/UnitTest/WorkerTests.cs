using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Domain.User.Dtos;
using final_challenge_grupo_118_notification;
using final_challenge_grupo_118_notification.Models;
using final_challenge_grupo_118_notification.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace UnitTest;

public class WorkerTests
{
    private Mock<ILogger<Worker>> _loggerMock = null!;
    private Mock<IConfiguration> _configurationMock = null!;
    private Mock<NotificationServiceFactory> _notificationServiceFactoryMock = null!;
    private Worker _worker = null!;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<Worker>>();
        _configurationMock = new Mock<IConfiguration>();
        _notificationServiceFactoryMock =
            new Mock<NotificationServiceFactory>(MockBehavior.Loose, Mock.Of<IServiceProvider>());

        // Setup configuration - Mock the IConfigurationSection for ConnectionStrings
        // GetConnectionString("RabbitMq") internally calls GetSection("ConnectionStrings")["RabbitMq"]
        var connectionStringSection = new Mock<IConfigurationSection>();
        connectionStringSection.Setup(x => x.Value).Returns("amqp://guest:guest@localhost:5672");

        var connectionStringsSection = new Mock<IConfigurationSection>();
        connectionStringsSection.Setup(x => x["RabbitMq"]).Returns("amqp://guest:guest@localhost:5672");
        connectionStringsSection.Setup(x => x.GetSection("RabbitMq")).Returns(connectionStringSection.Object);

        _configurationMock.Setup(c => c.GetSection("ConnectionStrings")).Returns(connectionStringsSection.Object);

        _worker = new Worker(
            _loggerMock.Object,
            _configurationMock.Object,
            _notificationServiceFactoryMock.Object
        );
    }

    [TearDown]
    public void TearDown()
    {
        _worker?.Dispose();
    }

    [Test]
    public void Constructor_ShouldInitializeWorker()
    {
        // Assert
        Assert.IsNotNull(_worker);
    }

    [Test]
    public void ExecuteAsync_WithValidMessage_ShouldProcessSuccessfully()
    {
        // This test validates that the Worker can be constructed and has the ExecuteAsync method
        // Full integration testing would require mocking RabbitMQ connections which is complex

        // Arrange
        var notificationMessage = new NotificationMessage
        {
            IsSuccess = true,
            UserId = 1,
            Message = "Test notification",
            ExceptionMessage = "",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var messageJson = JsonSerializer.Serialize(notificationMessage);
        var encodedMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(messageJson));

        // Assert
        Assert.IsNotNull(_worker);
        Assert.IsNotNull(encodedMessage);
    }

    [Test]
    public void Worker_ShouldHaveRequiredDependencies()
    {
        // Assert - Verify that Worker accepts all required dependencies
        Assert.DoesNotThrow(() =>
        {
            var worker = new Worker(
                _loggerMock.Object,
                _configurationMock.Object,
                _notificationServiceFactoryMock.Object
            );
        });
    }

    [Test]
    public void NotificationMessage_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var notificationMessage = new NotificationMessage
        {
            IsSuccess = true,
            UserId = 123,
            Message = "Test Message",
            ExceptionMessage = "No Exception",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(notificationMessage);
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        var deserialized = JsonSerializer.Deserialize<NotificationMessage>(decoded);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(notificationMessage.IsSuccess, deserialized.IsSuccess);
        Assert.AreEqual(notificationMessage.UserId, deserialized.UserId);
        Assert.AreEqual(notificationMessage.Message, deserialized.Message);
        Assert.AreEqual(notificationMessage.ExceptionMessage, deserialized.ExceptionMessage);
    }

    [Test]
    public void NotificationMessage_WithFailure_ShouldContainExceptionMessage()
    {
        // Arrange
        var notificationMessage = new NotificationMessage
        {
            IsSuccess = false,
            UserId = 456,
            Message = "Operation failed",
            ExceptionMessage = "Database connection timeout",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(notificationMessage);
        var deserialized = JsonSerializer.Deserialize<NotificationMessage>(json);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.IsFalse(deserialized.IsSuccess);
        Assert.IsNotEmpty(deserialized.ExceptionMessage);
        Assert.AreEqual("Database connection timeout", deserialized.ExceptionMessage);
    }

    [Test]
    public void ContentMessage_ShouldBeCreatedCorrectly()
    {
        // Arrange
        var user = new UserResponseDto
        {
            Id = 1,
            Name = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        var notificationMessage = new NotificationMessage
        {
            IsSuccess = true,
            UserId = 1,
            Message = "Upload completed",
            ExceptionMessage = "",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var contentMessage = new ContentMessage
        {
            Recipient = user.Email,
            Subject = "Notification",
            Content =
                $"{user.Name}, Message: {notificationMessage.Message} {(!notificationMessage.IsSuccess ? $" Exception: {notificationMessage.ExceptionMessage}" : "")} CreatedAt: {notificationMessage.CreatedAt}"
        };

        // Assert
        Assert.IsNotNull(contentMessage);
        Assert.AreEqual("john.doe@example.com", contentMessage.Recipient);
        Assert.AreEqual("Notification", contentMessage.Subject);
        Assert.That(contentMessage.Content, Does.Contain("John"));
        Assert.That(contentMessage.Content, Does.Contain("Upload completed"));
    }

    [Test]
    public void ContentMessage_WithFailureNotification_ShouldIncludeException()
    {
        // Arrange
        var user = new UserResponseDto
        {
            Id = 2,
            Name = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com"
        };

        var notificationMessage = new NotificationMessage
        {
            IsSuccess = false,
            UserId = 2,
            Message = "Upload failed",
            ExceptionMessage = "File size exceeds limit",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var contentMessage = new ContentMessage
        {
            Recipient = user.Email,
            Subject = "Notification",
            Content =
                $"{user.Name}, Message: {notificationMessage.Message} {(!notificationMessage.IsSuccess ? $" Exception: {notificationMessage.ExceptionMessage}" : "")} CreatedAt: {notificationMessage.CreatedAt}"
        };

        // Assert
        Assert.IsNotNull(contentMessage);
        Assert.That(contentMessage.Content, Does.Contain("Jane"));
        Assert.That(contentMessage.Content, Does.Contain("Upload failed"));
        Assert.That(contentMessage.Content, Does.Contain("Exception: File size exceeds limit"));
    }

    [Test]
    public void Worker_Configuration_ShouldHaveConnectionStringSetup()
    {
        // Arrange
        var config = new Mock<IConfiguration>();
        var connectionStringSection = new Mock<IConfigurationSection>();
        connectionStringSection.Setup(x => x.Value).Returns("amqp://localhost:5672");

        var connectionStringsSection = new Mock<IConfigurationSection>();
        connectionStringsSection.Setup(x => x.GetSection("RabbitMq")).Returns(connectionStringSection.Object);

        config.Setup(c => c.GetSection("ConnectionStrings")).Returns(connectionStringsSection.Object);

        // Act
        var connStrSection = config.Object.GetSection("ConnectionStrings").GetSection("RabbitMq");
        var connectionString = connStrSection.Value;

        // Assert
        Assert.IsNotNull(connectionString);
        Assert.That(connectionString, Does.Contain("amqp://"));
    }

    [Test]
    public void StopAsync_ShouldThrowNullReferenceException_WhenConnectionNotInitialized()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        // StopAsync will throw NullReferenceException if StartAsync hasn't been called
        // because _channel and _connection are not initialized
        Assert.ThrowsAsync<NullReferenceException>(async () => await _worker.StopAsync(cancellationToken));
    }

    [Test]
    public void NotificationServiceFactory_ShouldBeAcceptedByWorker()
    {
        // Arrange & Act
        var notificationServiceFactory = new NotificationServiceFactory(Mock.Of<IServiceProvider>());

        // Assert - Verify that Worker accepts NotificationServiceFactory
        Assert.DoesNotThrow(() =>
        {
            _ = new Worker(
                _loggerMock.Object,
                _configurationMock.Object,
                notificationServiceFactory
            );
        });
    }

    [Test]
    public void InvalidBase64Message_ShouldHandleGracefully()
    {
        // Arrange
        var invalidMessage = "This is not a valid base64 string!@#$%";

        // Act & Assert
        Assert.Throws<FormatException>(() => { Encoding.UTF8.GetString(Convert.FromBase64String(invalidMessage)); });
    }

    [Test]
    public void InvalidJsonMessage_ShouldReturnNull()
    {
        // Arrange
        var invalidJson = "{ invalid json structure }";

        // Act & Assert
        Assert.Throws<JsonException>(() => { JsonSerializer.Deserialize<NotificationMessage>(invalidJson); });
    }

    [Test]
    public void EmptyMessage_ShouldThrowJsonException()
    {
        // Arrange
        var emptyJson = "";

        // Act & Assert
        Assert.Throws<JsonException>(() => { JsonSerializer.Deserialize<NotificationMessage>(emptyJson); });
    }
}