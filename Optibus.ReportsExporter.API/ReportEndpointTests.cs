using System.Net.Http;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Optibus.ReportsExporter.API;
public class ReportEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ReportEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task GenerateReport_ShouldReturnPdf()
    {
        // Arrange
        var request = new ScheduleModel
        {
            Duties = new List<Duty>
            {
                new Duty
                {
                    Id = "D1",
                    Events = new List<DutyEvent>
                    {
                        new DutyEvent { Type = "taxi", StartTime = "0.06:00", EndTime = "0.07:00" }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/generate-report", content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
    }
}