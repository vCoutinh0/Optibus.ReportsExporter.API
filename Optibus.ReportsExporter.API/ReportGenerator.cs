namespace Optibus.ReportsExporter.API;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

public class ReportGenerator
{
    public byte[] GeneratePdf(ScheduleModel data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(8).LineHeight(2));
                
                page.Content().Column(column =>
                {
                    column.Item().Text("Duty report")
                        .FontSize(30).ExtraBold().LineHeight(3);
                    
                    GenerateStartEndTimeStep(column, data);

                    GenerateStartEndTimeNameStep(column, data);

                    GenerateBreaksStep(column, data);
                });
            });
        });

        using var memoryStream = new MemoryStream();
        document.GeneratePdf(memoryStream);
        return memoryStream.ToArray();
    }

    public void GenerateStartEndTimeStep(ColumnDescriptor column, ScheduleModel data)
    {
        column.Item().Text("Start and End Time")
                        .FontSize(16).LineHeight(3).Bold();
                        
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1); 
                            columns.RelativeColumn(2); 
                            columns.RelativeColumn(2); 
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Duty ID").Bold();
                            header.Cell().Text("Start Time").Bold();
                            header.Cell().Text("End Time").Bold();
                        });

                        foreach (var duty in data.Duties)
                        {
                            var startTime = GetDutyEventStartTime(data, duty.Events.First());
                            var endTime = GetDutyEventEndTime(data, duty.Events.Last());

                            table.Cell().Text(duty.Id);
                            table.Cell().Text(startTime.ToString("HH:mm"));
                            table.Cell().Text(endTime.ToString("HH:mm"));
                        }
                    });
    }
    
    public void GenerateStartEndTimeNameStep(ColumnDescriptor column, ScheduleModel data)
    {
        column.Item().Text("Start and End Stop Name")
            .FontSize(16).LineHeight(3).Bold();
                        
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1); 
                columns.RelativeColumn(1); 
                columns.RelativeColumn(1); 
                columns.RelativeColumn(3); 
                columns.RelativeColumn(3); 
            });

            table.Header(header =>
            {
                header.Cell().Text("Duty ID").Bold();
                header.Cell().Text("Start Time").Bold();
                header.Cell().Text("End Time").Bold();
                header.Cell().Text("Start stop description").Bold();
                header.Cell().Text("End stop description").Bold();
            });
            
            foreach (var duty in data.Duties)
            {
                var startTime = GetDutyEventStartTime(data, duty.Events.First());
                var endTime = GetDutyEventEndTime(data, duty.Events.Last());
                var startStopDescription = GetServiceTripStartStopDescription(data, duty);
                var endStopDescription = GetServiceTripEndStopDescription(data, duty);

                table.Cell().Text(duty.Id);
                table.Cell().Text(startTime.ToString("HH:mm"));
                table.Cell().Text(endTime.ToString("HH:mm"));
                table.Cell().Text(startStopDescription);
                table.Cell().Text(endStopDescription);
            }
        });
    }
        
    public void GenerateBreaksStep(ColumnDescriptor column, ScheduleModel data)
    {
        column.Item().Text("Breaks")
            .FontSize(16).LineHeight(3).Bold();
                        
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1); 
                columns.RelativeColumn(1); 
                columns.RelativeColumn(1); 
                columns.RelativeColumn(2); 
                columns.RelativeColumn(2); 
                columns.RelativeColumn(2); 
                columns.RelativeColumn(1); 
                columns.RelativeColumn(2); 

            });

            table.Header(header =>
            {
                header.Cell().Text("Duty ID").Bold();
                header.Cell().Text("Start Time").Bold();
                header.Cell().Text("End Time").Bold();
                header.Cell().Text("Start stop description").Bold();
                header.Cell().Text("End stop description").Bold();
                header.Cell().Text("Break start time").Bold();
                header.Cell().Text("Break duration").Bold();
                header.Cell().Text("Break stop name").Bold();
            });
            
            foreach (var duty in data.Duties)
            {
                var breaks = GetBreaks(data, duty);
                
                var startTime = GetDutyEventStartTime(data, duty.Events.First());
                var endTime = GetDutyEventEndTime(data, duty.Events.Last());
                var startStopDescription = GetServiceTripStartStopDescription(data, duty);
                var endStopDescription = GetServiceTripEndStopDescription(data, duty);
                
                foreach (var (breakDuration, breakStartTime, breakStopName) in breaks)
                {
                    table.Cell().Text(duty.Id);
                    table.Cell().Text(startTime.ToString("HH:mm"));
                    table.Cell().Text(endTime.ToString("HH:mm"));
                    table.Cell().Text(startStopDescription);
                    table.Cell().Text(endStopDescription);
                    table.Cell().Text(breakStartTime.ToString("HH:mm:ss"));
                    table.Cell().Text(breakDuration.TotalMinutes.ToString("F0"));
                    table.Cell().Text(breakStopName);
                }
            }
        });
    }
    
    public DateTime ParseTime(string rawTime)
    {
        var parts = rawTime.Split('.');
        int dayOffset = int.Parse(parts[0]);
        TimeSpan timeOfDay = TimeSpan.Parse(parts[1]);
        return DateTime.Today.AddDays(dayOffset).Add(timeOfDay);
    }
    
    public IEnumerable<VehicleEvent> GetVehicleEvents(ScheduleModel schedule, string vehicleId)
    {
        return schedule.Vehicles
            .Where(vehicle => vehicle.Id == vehicleId)
            .SelectMany(vehicle => vehicle.Events);
    }

    public string GetServiceTripEndStopDescription(ScheduleModel schedule, Duty duty)
    {
        var dutyEvent = duty.Events
            .Last(d => d.Type == DutyEventType.VehicleEvent);

        var lastServiceTripId = GetVehicleEvents(schedule, dutyEvent.VehicleId)
            .Last(evt => evt.Type == VehicleEventType.ServiceTrip && evt.DutyId == duty.Id)
            .TripId;
        
        var destinationStopId = schedule
            .Trips
            .Last(trip => trip.Id == lastServiceTripId)
            .DestinationStopId;

        return GetStopName(schedule, destinationStopId);
    }

    public string GetServiceTripStartStopDescription(ScheduleModel schedule, Duty duty)
    {
        var dutyEvent = duty.Events
            .First(d => d.Type == DutyEventType.VehicleEvent);

        var firstServiceTripId = GetVehicleEvents(schedule, dutyEvent.VehicleId)
            .First(evt => evt.Type == VehicleEventType.ServiceTrip && evt.DutyId == duty.Id)
            .TripId;
        
        var originStopId = schedule
            .Trips
            .First(trip => trip.Id == firstServiceTripId)
            .OriginStopId;
        
        return GetStopName(schedule, originStopId);
    }
    
    public string GetStopName(ScheduleModel schedule, string stopId)
    {
        return schedule.Stops
            .First(stop => stop.Id == stopId)
            .Name;
    }

    public DateTime GetDutyEventStartTime(ScheduleModel schedule, DutyEvent dutyEvent)
    {
        var rawStartTime = dutyEvent.Type switch
        {
            DutyEventType.VehicleEvent => GetRawVehicleEventRawStartTime(schedule, dutyEvent),
            DutyEventType.Taxi => dutyEvent.StartTime,
            DutyEventType.SignOn => dutyEvent.StartTime,
            _ => throw new NotSupportedException($"Unsupported event type: {dutyEvent.Type}") 
        };
            
        return ParseTime(rawStartTime);
    }

    public DateTime GetDutyEventEndTime(ScheduleModel schedule, DutyEvent dutyEvent)
    {
        var rawEndTime = dutyEvent.Type switch
        {
            DutyEventType.VehicleEvent => GetRawVehicleEventEndTime(schedule, dutyEvent),
            DutyEventType.Taxi => dutyEvent.EndTime,
            DutyEventType.SignOn=> dutyEvent.EndTime,
            _ => throw new NotSupportedException($"Unsupported event type: {dutyEvent.Type}") 
        };
            
        return ParseTime(rawEndTime);
    }

    public string GetRawVehicleEventRawStartTime(ScheduleModel schedule, DutyEvent dutyEvent)
    {
        var vehicleEvent = GetVehicleEvents(schedule, dutyEvent.VehicleId)
            .First(v => v.Sequence == dutyEvent.VehicleEventSequence);

        if (vehicleEvent.Type == VehicleEventType.ServiceTrip)
        {
            var tripId = vehicleEvent.TripId;
            
            var rawEventStartTime = schedule
                .Trips
                .First(trip => trip.Id == tripId)
                .DepartureTime;

            return rawEventStartTime;
        }
        
        return vehicleEvent.StartTime;
    }

    public string GetRawVehicleEventEndTime(ScheduleModel schedule, DutyEvent dutyEvent)
    {
        var vehicleEvent = GetVehicleEvents(schedule, dutyEvent.VehicleId)
            .First(v => v.Sequence == dutyEvent.VehicleEventSequence);

        if (vehicleEvent.Type == VehicleEventType.ServiceTrip)
        {
            var tripId = vehicleEvent.TripId;
            
            var rawEventStartTime = schedule
                .Trips
                .First(trip => trip.Id == tripId)
                .ArrivalTime;

            return rawEventStartTime;
        }
        
        return vehicleEvent.EndTime;
    }
    
    public string GetDestinationStopId(ScheduleModel schedule, DutyEvent dutyEvent)
    {
        if (dutyEvent.Type != DutyEventType.VehicleEvent)
            return dutyEvent.DestinationStopId;
        
        var vehicleEvent = GetVehicleEvents(schedule, dutyEvent.VehicleId)
            .First(v => v.Sequence == dutyEvent.VehicleEventSequence);

        if (vehicleEvent.Type == VehicleEventType.ServiceTrip)
        {
            var tripId = vehicleEvent.TripId;
            
            var destinationStopId = schedule
                .Trips
                .First(trip => trip.Id == tripId)
                .DestinationStopId;

            return destinationStopId;
        }
        
        return vehicleEvent.DestinationStopId;
    }
    
    public IEnumerable<(TimeSpan BreakDuration, DateTime BreakStartTime, string BreakStopName)> GetBreaks(ScheduleModel schedule, Duty duty)
    {
        var breaks = new List<(TimeSpan, DateTime, string)>();

        var events = duty.Events;

        for (int i = 0; i < events.Count - 1; i++)
        {
            var currentEvent = events[i];
            var nextEvent = events[i + 1];

            var currentEndTime = GetDutyEventEndTime(schedule, currentEvent);
            var nextStartTime = GetDutyEventStartTime(schedule, nextEvent);
            var breakDuration = nextStartTime - currentEndTime;

            if (breakDuration.TotalMinutes > 15)
            {
                var breakStopName = GetStopName(schedule, GetDestinationStopId(schedule, currentEvent));

                breaks.Add((breakDuration, currentEndTime, breakStopName));
            }
        }

        return breaks;
    }
}

