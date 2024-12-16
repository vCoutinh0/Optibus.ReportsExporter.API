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
            Stops = new List<Stop>
            {
                new Stop { Id = "S1", Name = "Depot A" },
                new Stop { Id = "S2", Name = "Station B" },
                new Stop { Id = "S3", Name = "Station C" }
            },
            Vehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    Id = "V1",
                    Events = new List<VehicleEvent>
                    {
                        new VehicleEvent { Sequence = 0, DutyId = "D1", TripId = "T1", Type = "service_trip"},
                        new VehicleEvent { Sequence = 1, DutyId = "D1", TripId = "T2", Type = "service_trip"}
                    }
                }
            },
            Trips = new List<Trip>
            {
                new Trip { Id = "T1", OriginStopId = "S1", DestinationStopId = "S2", ArrivalTime = "0.07:30", DepartureTime = "0.07:00" },
                new Trip { Id = "T2", OriginStopId = "S2", DestinationStopId = "S3", ArrivalTime = "0.07:00", DepartureTime = "0.08:45" }
            },
            Duties = new List<Duty>
            {
                new Duty
                {
                    Id = "D1",
                    Events = new List<DutyEvent>
                    {
                        new DutyEvent { Type = "vehicle_event", VehicleId = "V1", VehicleEventSequence = 0}
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