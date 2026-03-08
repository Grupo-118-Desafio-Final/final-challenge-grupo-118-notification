using System;
using System.Threading.Tasks;
using final_challenge_grupo_118_notification.Models;
using final_challenge_grupo_118_notification.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace UnitTest;

public class EmailServiceTests
{
    private Mock<ILogger<EmailService>> _loggerMock;
    private Mock<IOptions<EmailSettings>> _settingsMock;
    private EmailService _emailService;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<EmailService>>();
        _settingsMock = new Mock<IOptions<EmailSettings>>();
        
        var settings = new EmailSettings
        {
            SmtpServer = "invalid-server",
            Port = 587,
            SenderName = "Test Sender",
            SenderEmail = "test@example.com",
            Username = "user",
            Password = "password"
        };
        
        _settingsMock.Setup(s => s.Value).Returns(settings);

        _emailService = new EmailService(_loggerMock.Object, _settingsMock.Object);
    }

    [Test]
    public async Task SendAsync_ShouldLogError_WhenSmtpConnectionFails()
    {
        // Arrange
        var contentMessage = new ContentMessage
        {
            Recipient = "recipient@example.com",
            Subject = "Test Subject",
            Content = "Test Content"
        };

        // Act
        await _emailService.SendAsync(contentMessage);

        // Assert
        // Verify that logger was called with Error level
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Test]
    public async Task SendEmailAsync_ShouldLogError_WhenSmtpConnectionFails()
    {
        // Arrange
        var toEmail = "recipient@example.com";
        var subject = "Test Subject";
        var body = "Test Content";

        // Act
        await _emailService.SendEmailAsync(toEmail, subject, body);

        // Assert
        // Verify that logger was called with Error level
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
