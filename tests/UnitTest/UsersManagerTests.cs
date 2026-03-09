using System;
using System.Linq;
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

        // Response for GetById
        var userResponseData = new
        {
            name = "John",
            lastName = "Doe",
            email = "john.doe@example.com"
        };

        var getUserResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(userResponseData))
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString() == $"http://localhost:5000/users/{userId}" &&
                    req.Headers.Contains("X-Api-Key") &&
                    req.Headers.GetValues("X-Api-Key").Contains("test-hash")
                ),
                ItExpr.IsAny<CancellationToken>()
            )
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
    public async Task GetByIdAsync_ShouldReturnEmptyUser_WhenApiCallFails()
    {
        // Arrange
        var userId = "123";
        var getUserResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString() == $"http://localhost:5000/users/{userId}"
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(getUserResponse);

        // Act
        var result = await _usersManager.GetByIdAsync(userId, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(null, result.Id);
        Assert.IsNull(result.Name);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnEmptyUser_WhenGetUserFails()
    {
        // Arrange
        var userId = "234";
        var getUserResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri.ToString() == $"http://localhost:5000/users/{userId}"
                ),
                ItExpr.IsAny<CancellationToken>()
            )
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