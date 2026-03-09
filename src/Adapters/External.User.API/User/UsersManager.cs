using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Domain.User.Dtos;
using Domain.User.Ports.In;
using External.User.API.Models;
using Microsoft.Extensions.Configuration;

namespace External.User.API.User;

public class UsersManager : IUserManager
{
    private readonly HttpClient _httpClient;
    private readonly string _userApiBaseUrl;
    private readonly string _userApiHash;

    public UsersManager(HttpClient httpClient, UserApiSettings userApiSettings)
    {
        _httpClient = httpClient;

        _userApiBaseUrl = userApiSettings.BaseUrl ?? throw new ArgumentNullException(nameof(userApiSettings.BaseUrl),
            "UserApi:BaseUrl configuration is missing.");

        _userApiHash = userApiSettings.UserApiHash ??
                       throw new ArgumentNullException(nameof(userApiSettings.UserApiHash),
                           "UserApiHash configuration is missing.");
    }

    public async Task<UserResponseDto?> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var userEntity = new UserResponseDto();

        _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _userApiHash);

        var response = await _httpClient.GetAsync($"{_userApiBaseUrl}/users/{id}", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            using var jsonDoc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var root = jsonDoc.RootElement;

            var name = root.GetProperty("name").GetString() ?? string.Empty;
            var lastName = root.GetProperty("lastName").GetString() ?? string.Empty;
            var email = root.GetProperty("email").GetString() ?? string.Empty;

            userEntity.Id = id;
            userEntity.Name = name;
            userEntity.LastName = lastName;
            userEntity.Email = email;
        }

        return userEntity;
    }
}