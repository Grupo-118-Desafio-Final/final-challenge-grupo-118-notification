using System;
using final_challenge_grupo_118_notification.Models;
using final_challenge_grupo_118_notification.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace UnitTest
{
    public class NotificationServiceFactoryTests
    {
        private Mock<IServiceProvider> _serviceProviderMock;
        private NotificationServiceFactory _factory;

        [SetUp]
        public void Setup()
        {
            _serviceProviderMock = new Mock<IServiceProvider>();
            _factory = new NotificationServiceFactory(_serviceProviderMock.Object);
        }

        [Test]
        public void GetService_Email_ReturnsEmailService()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EmailService>>();
            var settingsMock = new Mock<IOptions<EmailSettings>>();
            var emailService = new EmailService(loggerMock.Object, settingsMock.Object);
            
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(EmailService))).Returns(emailService);

            // Act
            var service = _factory.GetService(NotificationTypeEnum.Email);

            // Assert
            Assert.IsInstanceOf<EmailService>(service);
        }

        [Test]
        public void GetService_Sms_ReturnsSmsService()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SmsNotificationService>>();
            var settingsMock = new Mock<IOptions<SmsSettings>>();
            settingsMock.Setup(s => s.Value).Returns(new SmsSettings { AccountSid = "AC123", AuthToken = "token" });
            
            var smsService = new SmsNotificationService(settingsMock.Object, loggerMock.Object);
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(SmsNotificationService))).Returns(smsService);

            // Act
            var service = _factory.GetService(NotificationTypeEnum.Sms);

            // Assert
            Assert.IsInstanceOf<SmsNotificationService>(service);
        }

        [Test]
        public void GetService_Telegram_ReturnsTelegramService()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TelegramNotificationService>>();
            var telegramService = new TelegramNotificationService(loggerMock.Object);
            
            _serviceProviderMock.Setup(sp => sp.GetService(typeof(TelegramNotificationService))).Returns(telegramService);

            // Act
            var service = _factory.GetService(NotificationTypeEnum.Telegram);

            // Assert
            Assert.IsInstanceOf<TelegramNotificationService>(service);
        }

        [Test]
        public void GetService_InvalidType_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var invalidType = (NotificationTypeEnum)999;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => _factory.GetService(invalidType));
        }
    }
}
