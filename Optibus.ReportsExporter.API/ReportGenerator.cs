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
                page.DefaultTextStyle(x => x.FontSize(12));
                
                page.Content().Column(column =>
                {
                    column.Item().Text("Duty report")
                        .FontSize(20).Bold();
                    column.Spacing(10);

                    GenerateStartEndTimeStep(column, data);
                });
            });
        });

        using var memoryStream = new MemoryStream();
        document.GeneratePdf(memoryStream);
        return memoryStream.ToArray();
    }

    public DateTime ParseTime(string rawTime)
    {
        var parts = rawTime.Split('.');
        int dayOffset = int.Parse(parts[0]);
        TimeSpan timeOfDay = TimeSpan.Parse(parts[1]);
        return DateTime.Today.AddDays(dayOffset).Add(timeOfDay);
    }

    public void GenerateStartEndTimeStep(ColumnDescriptor column, ScheduleModel data)
    {
        column.Item().Text("Step 1 - Start and End Time")
                        .FontSize(16).Bold();
                        
                    column.Spacing(5);
                    
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1); 
                            columns.RelativeColumn(2); 
                            columns.RelativeColumn(2); 
                        });

                        // Cabeçalhos
                        table.Header(header =>
                        {
                            header.Cell().Text("Duty ID").Bold();
                            header.Cell().Text("Start Time").Bold();
                            header.Cell().Text("End Time").Bold();
                        });

                        // Dados
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

    public IEnumerable<VehicleEvent> GetVehicleEvents(ScheduleModel schedule, string vehicleId)
    {
        return schedule.Vehicles
            .Where(vehicle => vehicle.Id == vehicleId)
            .SelectMany(vehicle => vehicle.Events);
    }

    public DateTime GetDutyEventStartTime(ScheduleModel schedule, DutyEvent dutyEvent)
    {
        var rawStartTime = dutyEvent.Type switch
        {
            "vehicle_event" => GetVehicleEvents(schedule, dutyEvent.VehicleId)
                .First()
                .StartTime,
            "taxi" => dutyEvent.StartTime,
            "sign_on" => dutyEvent.StartTime,
            _ => throw new NotSupportedException($"Unsupported event type: {dutyEvent.Type}") 
        };
            
        return ParseTime(rawStartTime);
    }

    public DateTime GetDutyEventEndTime(ScheduleModel schedule, DutyEvent dutyEvent)
    {
        var rawEndTime = dutyEvent.Type switch
        {
            "vehicle_event" => GetVehicleEvents(schedule, dutyEvent.VehicleId)
                .Last()
                .EndTime,
            "taxi" => dutyEvent.EndTime,
            "sign_on" => dutyEvent.EndTime,
            _ => throw new NotSupportedException($"Unsupported event type: {dutyEvent.Type}") 
        };
            
        return ParseTime(rawEndTime);
    }
}