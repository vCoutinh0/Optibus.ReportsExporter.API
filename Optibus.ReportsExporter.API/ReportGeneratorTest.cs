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
                        new VehicleEvent { Sequence = 1, StartTime = "0.05:00", EndTime = "0.06:00" },
                        new VehicleEvent { Sequence = 2, StartTime = "0.07:00", EndTime = "0.07:30" },
                        new VehicleEvent { Sequence = 3, Type = "service_trip", TripId = "T1" },
                        new VehicleEvent { Sequence = 4, Type = "dead_run", EndTime = "0.07:00", DestinationStopId = "S3"}
                    },
                },
                new Vehicle
                {
                    Id = "V2",
                    Events = new List<VehicleEvent>
                    {
                        new VehicleEvent { Type = "service_trip", TripId = "T1", DutyId = "D1" },
                        new VehicleEvent { Type = "service_trip", TripId = "T2", DutyId = "D1" },
                    }
                },
                new Vehicle
                {
                    Id = "V3",
                    Events = new List<VehicleEvent>
                    {
                        new VehicleEvent { Sequence = 0, Type = "service_trip", TripId = "T1", },
                        new VehicleEvent { Sequence = 1, Type = "service_trip", TripId = "T2", }
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
                        new DutyEvent { Type = "vehicle_event", VehicleId = "V2" }
                    },
                },
                new Duty
                {
                    Id = "D2",
                    Events = new List<DutyEvent>
                    {
                        new DutyEvent { Sequence = 0, Type = "vehicle_event", VehicleId = "V3" },
                        new DutyEvent { Sequence = 1, Type = "vehicle_event", VehicleId = "V3" }
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
        // Act
        var events = _reportGenerator.GetVehicleEvents(_schedule, "V1");

        // Assert
        events.Should().HaveCount(4);
        events.First().StartTime.Should().Be("0.05:00");
        events.Last().EndTime.Should().Be("0.12:30");
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
    
    [Fact]
    public void GetServiceTripStartStopDescription_ShouldReturnCorrectStartStopName()
    {
        // Arrange
        var duty = _schedule.Duties.First(d => d.Id == "D1");
        var reportGenerator = new ReportGenerator();

        // Act
        var startStopName = reportGenerator.GetServiceTripStartStopDescription(_schedule, duty);

        // Assert
        startStopName.Should().Be("Depot A");
    }
    
    [Fact]
    public void GetServiceTripEndStopDescription_ShouldReturnCorrectEndStopName()
    {
        // Arrange
        var duty = _schedule.Duties.First(d => d.Id == "D1");
        var reportGenerator = new ReportGenerator();

        // Act
        var endStopName = reportGenerator.GetServiceTripEndStopDescription(_schedule, duty);

        // Assert
        endStopName.Should().Be("Station C");
    }

    [Fact]
    public void GetRawVehicleEventEndTime_ShouldReturnArrivalTime_ForServiceTrip()
    {
        // Arrange
        var dutyEvent = new DutyEvent { VehicleId = "V1", VehicleEventSequence = 3, Type = "vehicle_event" };
        var reportGenerator = new ReportGenerator();

        // Act
        var endTime = reportGenerator.GetRawVehicleEventEndTime(_schedule, dutyEvent);

        // Assert
        endTime.Should().Be("0.07:30");
    }
    
    [Fact]
    public void GetRawVehicleEventEndTime_ShouldReturnEndTime_ForNonServiceTrip()
    {
        // Arrange
        var dutyEvent = new DutyEvent { VehicleId = "V1", VehicleEventSequence = 4, Type = "vehicle_event" };
        var reportGenerator = new ReportGenerator();

        // Act
        var endTime = reportGenerator.GetRawVehicleEventEndTime(_schedule, dutyEvent);

        // Assert
        endTime.Should().Be("0.07:00"); 
    }
    
    [Fact]
    public void GetDestinationStopId_ShouldReturnStopId_ForNonVehicleEvent()
    {
        // Arrange
        var dutyEvent = new DutyEvent { Type = "taxi", DestinationStopId = "S1" };
        var reportGenerator = new ReportGenerator();

        // Act
        var stopId = reportGenerator.GetDestinationStopId(_schedule, dutyEvent);

        // Assert
        stopId.Should().Be("S1");
    }
    
    [Fact]
    public void GetDestinationStopId_ShouldReturnStopId_ForServiceTrip()
    {
        // Arrange
        var dutyEvent = new DutyEvent { VehicleId = "V1", VehicleEventSequence = 3, Type = "vehicle_event" };
        var reportGenerator = new ReportGenerator();

        // Act
        var stopId = reportGenerator.GetDestinationStopId(_schedule, dutyEvent);

        // Assert
        stopId.Should().Be("S2");
    }
    
    [Fact]
    public void GetDestinationStopId_ShouldReturnStopId_ForNonServiceTrip()
    {
        // Arrange
        var dutyEvent = new DutyEvent { VehicleId = "V1", VehicleEventSequence = 4, Type = "vehicle_event" };
        var reportGenerator = new ReportGenerator();

        // Act
        var stopId = reportGenerator.GetDestinationStopId(_schedule, dutyEvent);

        // Assert
        stopId.Should().Be("S3");
    }

    [Fact]
    public void GetBreaks_ShouldReturnABreak_WhenBreakExceeds15Minutes()
    {
        // Arrange
        var duty = _schedule.Duties[1];
        var reportGenerator = new ReportGenerator();

        // Act
        var breaks = reportGenerator.GetBreaks(_schedule, duty);

        // Assert
        breaks.Should().HaveCount(1);

        var breakInfo = breaks.First();
        breakInfo.BreakDuration.TotalMinutes.Should().Be(30); 
        breakInfo.BreakStartTime.ToString("HH:mm:ss").Should().Be("06:30:00");
        breakInfo.BreakStopName.Should().Be("Station B");       // Nome da parada correspondente
    }
    
    [Fact]
    public void GetBreaks_ShouldReturnEmpty_WhenNoBreakExceeds15Minutes()
    {
        // Arrange
        var duty = new Duty
        {
            Id = "D3",
            Events = new List<DutyEvent>
            {
                new DutyEvent { Sequence = 0, Type = "vehicle_event", VehicleId = "V3" },
                new DutyEvent { Sequence = 1, Type = "vehicle_event", VehicleId = "V3" }
            }
        };

        var reportGenerator = new ReportGenerator();

        // Act
        var breaks = reportGenerator.GetBreaks(_schedule, duty);

        // Assert
        breaks.Should().BeEmpty();
    }
    
}