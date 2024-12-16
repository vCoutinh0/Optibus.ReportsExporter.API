using FluentAssertions;
using Xunit;

namespace Optibus.ReportsExporter.API;

public class ReportGeneratorTests
{
    private readonly ReportGenerator _reportGenerator;
    private readonly ScheduleModel _schedule;
    
    public ReportGeneratorTests()
    {
        _reportGenerator = new ReportGenerator(); 
        
        _schedule = new ScheduleModel
        {
            Vehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    Id = "V1",
                    Events = new List<VehicleEvent>
                    {
                        new VehicleEvent { Sequence = 1, StartTime = "0.05:00", EndTime = "0.06:00" },
                        new VehicleEvent { Sequence = 2, StartTime = "0.07:00", EndTime = "0.07:30" },
                        new VehicleEvent { Sequence = 2, StartTime = "0.09:00", EndTime = "0.10:30" },
                        new VehicleEvent { Sequence = 3, StartTime = "0.11:00", EndTime = "0.12:30" }
                    }
                }
            }
        };
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
    public void GetGetDutyEventStartTime_ShouldReturnStartTimeForVehicleEvent()
    {
        // Arrange
        var dutyEvent = new DutyEvent
        {
            Type = "vehicle_event",
            VehicleId = "V1",
            VehicleEventSequence = 2
        };

        var reportGenerator = new ReportGenerator();

        // Act
        var endTime = reportGenerator.GetDutyEventStartTime(_schedule, dutyEvent);

        // Assert
        endTime.Hour.Should().Be(7);
        endTime.Minute.Should().Be(00);
    }

    [Fact]
    public void GetDutyEventStartTime_ShouldReturnStartTimeForTaxiEvent()
    {
        // Arrange
        var dutyEvent = new DutyEvent
        {
            Type = "taxi",
            StartTime = "0.08:00"
        };

        var reportGenerator = new ReportGenerator();

        // Act
        var endTime = reportGenerator.GetDutyEventStartTime(_schedule, dutyEvent);

        // Assert
        endTime.Hour.Should().Be(8);
        endTime.Minute.Should().Be(0);
    }

    [Fact]
    public void GetDutyEventStartTime_ShouldReturnStartTimeForSignOnEvent()
    {
        // Arrange
        var dutyEvent = new DutyEvent
        {
            Type = "sign_on",
            StartTime = "0.09:00"
        };

        var reportGenerator = new ReportGenerator();

        // Act
        var endTime = reportGenerator.GetDutyEventStartTime(_schedule, dutyEvent);

        // Assert
        endTime.Hour.Should().Be(9);
        endTime.Minute.Should().Be(0);
    }

    [Fact]
    public void GetDutyEventStartTime_ShouldThrowExceptionForUnsupportedEventType()
    {
        // Arrange
        var dutyEvent = new DutyEvent
        {
            Type = "unsupported_event",
            EndTime = "0.10:00"
        };

        var reportGenerator = new ReportGenerator();

        // Act & Assert
        Action act = () => reportGenerator.GetDutyEventStartTime(_schedule, dutyEvent);
        act.Should().Throw<NotSupportedException>()
           .WithMessage("Unsupported event type: unsupported_event");
    }
    
    [Fact]
    public void GetDutyEventEndTime_ShouldReturnEndTimeForVehicleEvent()
    {
        // Arrange
        var dutyEvent = new DutyEvent
        {
            Type = "vehicle_event",
            VehicleId = "V1",
            VehicleEventSequence = 2
        };

        var reportGenerator = new ReportGenerator();

        // Act
        var endTime = reportGenerator.GetDutyEventEndTime(_schedule, dutyEvent);

        // Assert
        endTime.Hour.Should().Be(10);
        endTime.Minute.Should().Be(30);
    }

    [Fact]
    public void GetDutyEventEndTime_ShouldReturnEndTimeForTaxiEvent()
    {
        // Arrange
        var dutyEvent = new DutyEvent
        {
            Type = "taxi",
            EndTime = "0.08:00"
        };

        var reportGenerator = new ReportGenerator();

        // Act
        var endTime = reportGenerator.GetDutyEventEndTime(_schedule, dutyEvent);

        // Assert
        endTime.Hour.Should().Be(8);
        endTime.Minute.Should().Be(0);
    }

    [Fact]
    public void GetDutyEventEndTime_ShouldReturnEndTimeForSignOnEvent()
    {
        // Arrange
        var dutyEvent = new DutyEvent
        {
            Type = "sign_on",
            EndTime = "0.09:00"
        };

        var reportGenerator = new ReportGenerator();

        // Act
        var endTime = reportGenerator.GetDutyEventEndTime(_schedule, dutyEvent);

        // Assert
        endTime.Hour.Should().Be(9);
        endTime.Minute.Should().Be(0);
    }

    [Fact]
    public void GetDutyEventEndTime_ShouldThrowExceptionForUnsupportedEventType()
    {
        // Arrange
        var dutyEvent = new DutyEvent
        {
            Type = "unsupported_event",
            EndTime = "0.10:00"
        };

        var reportGenerator = new ReportGenerator();

        // Act & Assert
        Action act = () => reportGenerator.GetDutyEventEndTime(_schedule, dutyEvent);
        act.Should().Throw<NotSupportedException>()
           .WithMessage("Unsupported event type: unsupported_event");
    }
}