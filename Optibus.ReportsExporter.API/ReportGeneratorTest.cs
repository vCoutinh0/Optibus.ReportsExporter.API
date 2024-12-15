using FluentAssertions;
using Xunit;

namespace Optibus.ReportsExporter.API;

public class ReportGeneratorTests
{
    private readonly ReportGenerator _reportGenerator;

    public ReportGeneratorTests()
    {
        _reportGenerator = new ReportGenerator(); 
    }
    
    [Theory]
    [InlineData("0.06:30", 6, 30)]
    [InlineData("0.13:30", 13, 30)]
    [InlineData("1.00:00", 0, 0)]
    public void ParseTime_ShouldConvertCorrectly(string input, int expectedHour, int expectedMinute)
    {
        // Act
        var result = _reportGenerator.ParseTime(input);

        // Assert
        result.Hour.Should().Be(expectedHour);
        result.Minute.Should().Be(expectedMinute);
    }
    
    [Fact]
    public void GetVehicleEvents_ShouldReturnCorrectEvents()
    {
        // Arrange
        var schedule = new ScheduleModel
        {
            Vehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    Id = "V1",
                    Events = new List<VehicleEvent>
                    {
                        new VehicleEvent { StartTime = "0.06:00", EndTime = "0.06:30" },
                        new VehicleEvent { StartTime = "0.07:00", EndTime = "0.07:30" }
                    }
                },
                new Vehicle
                {
                    Id = "V2",
                    Events = new List<VehicleEvent>
                    {
                        new VehicleEvent { StartTime = "0.08:00", EndTime = "0.08:30" }
                    }
                }
            }
        };

        // Act
        var events = _reportGenerator.GetVehicleEvents(schedule, "V1");

        // Assert
        events.Should().HaveCount(2);
        events.First().StartTime.Should().Be("0.06:00");
        events.Last().EndTime.Should().Be("0.07:30");
    }
    
    [Fact]
    public void GetDutyEventStartTime_ShouldReturnCorrectTime_ForVehicleEvent()
    {
        // Arrange
        var schedule = new ScheduleModel
        {
            Vehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    Id = "V1",
                    Events = new List<VehicleEvent>
                    {
                        new VehicleEvent { StartTime = "0.06:00", EndTime = "0.06:30" }
                    }
                }
            }
        };

        var dutyEvent = new DutyEvent
        {
            Type = "vehicle_event",
            VehicleId = "V1"
        };

        // Act
        var startTime = _reportGenerator.GetDutyEventStartTime(schedule, dutyEvent);

        // Assert
        startTime.Hour.Should().Be(6);
        startTime.Minute.Should().Be(0);
    }
    
    [Fact]
    public void GetDutyEventEndTime_ShouldReturnCorrectTime_ForTaxiEvent()
    {
        // Arrange
        var schedule = new ScheduleModel();
        var dutyEvent = new DutyEvent
        {
            Type = "taxi",
            EndTime = "0.08:30"
        };

        // Act
        var endTime = _reportGenerator.GetDutyEventEndTime(schedule, dutyEvent);

        // Assert
        endTime.Hour.Should().Be(8);
        endTime.Minute.Should().Be(30);
    }    
}