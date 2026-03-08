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
using NUnit.Framework;

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
        
        // Setup configuration mocks
        _configurationMock.Setup(c => c["UserApi:BaseUrl"]).Returns("http://localhost:5000");
        _configurationMock.Setup(c => c["UserApiHash"]).Returns("test-hash");

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
        // Depending on implementation, if login fails, it might return an empty user object or null.
        // Looking at the code: var userEntity = new UserResponseDto(); ... return userEntity;
        // So it returns an empty object (Id=0, Name=null, etc.) if login fails (token is empty) or if GetById fails.
        
        // However, if Login fails, token is empty.
        // Then: if (!string.IsNullOrEmpty(token)) ... else it proceeds without Auth header?
        // The code says:
        // var token = await Login(cancellationToken);
        // if (!string.IsNullOrEmpty(token)) { ... }
        // var response = await _httpClient.GetAsync(...)
        
        // So if login fails, it still tries to call GetAsync but without the Bearer token.
        // The mock needs to handle the second call too, or the code will crash if the mock doesn't expect a second call?
        // Actually, if I use Setup instead of SetupSequence, it returns the same response for all calls.
        // But here we want to simulate Login failing.
        
        // If Login fails (returns Unauthorized), the code catches nothing, it just returns string.Empty for token.
        // Then it calls GetAsync.
        // So the mock MUST expect a second call (GetAsync) or I should use Setup to return Unauthorized for ANY call.
        
        // Let's use SetupSequence to be precise: Login fails, then GetAsync fails (or succeeds? usually if login fails, get fails).
        // Let's assume if login fails, the subsequent call also fails or returns 401.
        
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
        Assert.AreEqual(0, result2.Id);
        Assert.IsNull(result2.Name);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnEmptyUser_WhenGetUserFails()
    {
        // Arrange
        var userId = 1;
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
        Assert.AreEqual(0, result.Id);
        Assert.IsNull(result.Name);
    }
}
