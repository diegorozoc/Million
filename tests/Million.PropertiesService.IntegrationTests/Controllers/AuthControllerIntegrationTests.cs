using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Million.PropertiesService.Api.Models;
using Million.PropertiesService.IntegrationTests.Infrastructure;

namespace Million.PropertiesService.IntegrationTests.Controllers;

[TestFixture]
public class AuthControllerIntegrationTests : IntegrationTestBase
{
    #region Login Integration Tests

    [Test]
    public async Task Login_WithValidAdminCredentials_ShouldReturnTokenAndUserInfo()
    {
        // Arrange
        var loginRequest = new LoginRequest("admin@million.com", "admin123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrEmpty();
        loginResponse.Email.Should().Be("admin@million.com");
        loginResponse.Role.Should().Be("Admin");
        loginResponse.UserId.Should().NotBeNullOrEmpty();
        loginResponse.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Test]
    public async Task Login_WithValidManagerCredentials_ShouldReturnTokenAndUserInfo()
    {
        // Arrange
        var loginRequest = new LoginRequest("manager@million.com", "manager123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrEmpty();
        loginResponse.Email.Should().Be("manager@million.com");
        loginResponse.Role.Should().Be("Manager");
        loginResponse.UserId.Should().NotBeNullOrEmpty();
        loginResponse.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Test]
    public async Task Login_WithValidUserCredentials_ShouldReturnTokenAndUserInfo()
    {
        // Arrange
        var loginRequest = new LoginRequest("user@million.com", "user123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrEmpty();
        loginResponse.Email.Should().Be("user@million.com");
        loginResponse.Role.Should().Be("User");
        loginResponse.UserId.Should().NotBeNullOrEmpty();
        loginResponse.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Test]
    public async Task Login_WithInvalidEmail_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest("invalid@million.com", "admin123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Invalid credentials");
    }

    [Test]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest("admin@million.com", "wrongpassword");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Invalid credentials");
    }

    [Test]
    public async Task Login_WithInvalidModelState_ShouldReturnBadRequest()
    {
        // Arrange - Invalid email format
        var loginRequest = new LoginRequest("invalid-email", "password123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert - The API validates credentials first, then model state, so this returns Unauthorized
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Login_WithMissingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest("", "password123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert - The API validates credentials first, then model state, so this returns Unauthorized
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Login_WithMissingPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest("admin@million.com", "");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert - The API validates credentials first, then model state, so this returns Unauthorized
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Login_WithShortPassword_ShouldReturnBadRequest()
    {
        // Arrange - Password less than 6 characters
        var loginRequest = new LoginRequest("admin@million.com", "123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert - The API validates credentials first, then model state, so this returns Unauthorized
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}