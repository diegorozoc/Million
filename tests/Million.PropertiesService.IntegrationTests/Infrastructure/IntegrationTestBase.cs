using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Million.PropertiesService.Api.Models;
using Million.PropertiesService.Persistance;
using Newtonsoft.Json;

namespace Million.PropertiesService.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase
{
    protected TestWebApplicationFactory Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Factory = new TestWebApplicationFactory();
        Client = Factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Client?.Dispose();
        Factory?.Dispose();
    }

    [SetUp]
    public virtual async Task SetUp()
    {
        // Clean database before each test
        await CleanDatabaseAsync();
    }

    protected virtual async Task CleanDatabaseAsync()
    {
        try
        {
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PropertiesDbContext>();
            
            // Ensure database is created
            await dbContext.Database.EnsureCreatedAsync();
            
            // Remove all data
            if (dbContext.PropertyImages.Any())
                dbContext.PropertyImages.RemoveRange(dbContext.PropertyImages);
            if (dbContext.PropertyTraces.Any())
                dbContext.PropertyTraces.RemoveRange(dbContext.PropertyTraces);
            if (dbContext.Properties.Any())
                dbContext.Properties.RemoveRange(dbContext.Properties);
            if (dbContext.Owners.Any())
                dbContext.Owners.RemoveRange(dbContext.Owners);
            
            await dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            // If cleanup fails, we'll recreate the database
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PropertiesDbContext>();
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
        }
    }

    protected async Task<string> GetJwtTokenAsync(string email = "admin@million.com", string password = "admin123")
    {
        var loginRequest = new LoginRequest(email, password);
        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        
        response.Should().BeSuccessful();
        
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResponse!.Token;
    }

    protected void SetAuthorizationHeader(string token)
    {
        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    protected async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(content);
    }

    protected async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
    {
        var json = JsonConvert.SerializeObject(value);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await Client.PostAsync(requestUri, content);
    }

    protected async Task<HttpResponseMessage> PutAsJsonAsync<T>(string requestUri, T value)
    {
        var json = JsonConvert.SerializeObject(value);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await Client.PutAsync(requestUri, content);
    }

    protected Million.PropertiesService.Domain.Owners.Entities.Owner CreateTestOwner(
        string name = "Test Owner",
        string street = "123 Test St",
        string city = "Test City",
        string postalCode = "12345",
        string country = "Test Country")
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PropertiesDbContext>();
        
        var address = new Million.PropertiesService.Domain.Common.ValueObjects.Address(
            street, city, postalCode, country);
        var dateOfBirth = new Million.PropertiesService.Domain.Common.ValueObjects.DateOfBirth(
            new DateTime(1980, 1, 1));
        
        var owner = Million.PropertiesService.Domain.Owners.Entities.Owner.Create(
            name, address, dateOfBirth);
        
        dbContext.Owners.Add(owner);
        dbContext.SaveChanges();
        
        return owner;
    }
}