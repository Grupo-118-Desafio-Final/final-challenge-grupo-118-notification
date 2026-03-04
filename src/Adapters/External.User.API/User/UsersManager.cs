using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Domain.User.Dtos;
using Domain.User.Ports.In;
using Microsoft.Extensions.Configuration;

namespace External.User.API.User;


public class UsersManager : IUserManager
{
    private readonly HttpClient _httpClient;
    private readonly string _userApiBaseUrl;
    
    public UsersManager(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _userApiBaseUrl = configuration["UserApi:BaseUrl"] ??
                              throw new ArgumentNullException("UserApi:BaseUrl configuration is missing.");
    }
    public async Task<UserResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var userEntity = new UserResponseDto();
        var token = await Login(cancellationToken);
        
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await _httpClient.GetAsync($"{_userApiBaseUrl}/users/GetById/{id}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            using var jsonDoc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var root = jsonDoc.RootElement;
            if (root.TryGetProperty("data", out var dataElement))
            {
                var name = dataElement.GetProperty("name").GetString() ?? string.Empty;
                var lastName = dataElement.GetProperty("lastName").GetString() ?? string.Empty;
                var email = dataElement.GetProperty("email").GetString() ?? string.Empty;

                userEntity.Id = id;
                userEntity.Name = name;
                userEntity.LastName = lastName;
                userEntity.Email = email;
            }
        }
        
        return userEntity;
    }

    private async Task<string> Login(CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsync($"{_userApiBaseUrl}/users/login", null, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            using var jsonDoc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var root = jsonDoc.RootElement;
            if (root.TryGetProperty("data", out var dataElement))
            {
                return dataElement.GetProperty("token").GetString() ?? string.Empty;
            }
        }

        return string.Empty;
    }
}