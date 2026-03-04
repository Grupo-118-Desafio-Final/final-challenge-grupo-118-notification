using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Domain.User.Dtos;
using External.User.API.User;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace UnitTest;

public class UsersManagerTests
{
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private HttpClient _httpClient;
    private Mock<IConfiguration> _configurationMock;
    private UsersManager _usersManager;

    [SetUp]
    public void Setup()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["UserApi:BaseUrl"]).Returns("http://localhost:5000");

        _usersManager = new UsersManager(_httpClient, _configurationMock.Object);
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
        var userId = 1;
        var token = "test-token";
        var userResponse = new
        {
            data = new
            {
                name = "John",
                lastName = "Doe",
                email = "john.doe@example.com",
                token = token
            }
        };

        var loginResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(userResponse))
        };

        var getUserResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(userResponse))
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
        var userId = 1;
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
        Assert.AreEqual(0, result.Id);
        Assert.IsNull(result.Name);
        Assert.IsNull(result.LastName);
        Assert.IsNull(result.Email);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnEmptyUser_WhenGetUserFails()
    {
        // Arrange
        var userId = 1;
        var token = "test-token";
        var userResponse = new
        {
            data = new
            {
                token = token
            }
        };

        var loginResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(userResponse))
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
        Assert.AreEqual(0, result.Id);
        Assert.IsNull(result.Name);
        Assert.IsNull(result.LastName);
        Assert.IsNull(result.Email);
    }
}