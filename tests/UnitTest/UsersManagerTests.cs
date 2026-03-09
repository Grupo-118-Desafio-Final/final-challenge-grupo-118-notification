using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Domain.User.Dtos;
using External.User.API.Models;
using External.User.API.User;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace UnitTest;

public class UsersManagerTests
{
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private HttpClient _httpClient;
    private Mock<IConfiguration> _configurationMock;
    private UsersManager _usersManager;
    private UserApiSettings _userApiSettings;

    [SetUp]
    public void Setup()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _userApiSettings = new UserApiSettings
        {
            BaseUrl = "http://localhost:5000",
            UserApiHash = "test-hash"
        };

        _usersManager = new UsersManager(_httpClient, _userApiSettings);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnUser_WhenApiCallIsSuccessful()
    {
        // Arrange
        var userId = "123";
        var token = "test-token";

        // Response for Login
        var loginResponseData = new
        {
            data = new
            {
                token = token
            }
        };

        // Response for GetById
        var userResponseData = new
        {
            data = new
            {
                name = "John",
                lastName = "Doe",
                email = "john.doe@example.com"
            }
        };

        var loginResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(loginResponseData))
        };

        var getUserResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(userResponseData))
        };

        _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(loginResponse)
            .ReturnsAsync(getUserResponse);

        // Act
        var result = await _usersManager.GetByIdAsync(userId, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(userId, result.Id);
        Assert.AreEqual("John", result.Name);
        Assert.AreEqual("Doe", result.LastName);
        Assert.AreEqual("john.doe@example.com", result.Email);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnEmptyUser_WhenLoginFails()
    {
        // Arrange
        var userId = "123";
        var loginResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(loginResponse);

        // Act
        var result = await _usersManager.GetByIdAsync(userId, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);

        var getUserResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);

        _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(loginResponse)
            .ReturnsAsync(getUserResponse);

        // Act
        var result2 = await _usersManager.GetByIdAsync(userId, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result2);
        Assert.AreEqual(null, result2.Id);
        Assert.IsNull(result2.Name);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnEmptyUser_WhenGetUserFails()
    {
        // Arrange
        var userId = "234";
        var token = "test-token";
        var loginResponseData = new
        {
            data = new
            {
                token = token
            }
        };

        var loginResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(loginResponseData))
        };

        var getUserResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(loginResponse)
            .ReturnsAsync(getUserResponse);

        // Act
        var result = await _usersManager.GetByIdAsync(userId, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(null, result.Id);
        Assert.IsNull(result.Name);
    }

    [Test]
    public async Task
        InstantiateUserManager_WithoutDefinedUSerProperties_ShouldThrowArgumentNullExceptionOnBaseUrlCheck()
    {
        // Arrange
        var invalidSettings = new UserApiSettings
        {
            BaseUrl = null,
            UserApiHash = "test-hash"
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UsersManager(_httpClient, invalidSettings));
    }

    [Test]
    public async Task
        InstantiateUserManager_WithoutDefinedUSerProperties_ShouldThrowArgumentNullExceptionOnApiHashCheck()
    {
        // Arrange
        var invalidSettings = new UserApiSettings
        {
            BaseUrl = "http://localhost:5000",
            UserApiHash = null
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UsersManager(_httpClient, invalidSettings));
    }
}