using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Optibus.ReportsExporter.API;

namespace Optibus.ReportsExporter.API;

public static class ReportEndpoints
{
    public static void MapReportEndpoint(this WebApplication app)
    {
        app.MapPost("/generate-report", (ScheduleModel request, ReportGenerator reportGenerator) =>
        {
            try
            {
                var pdfBytes = reportGenerator.GeneratePdf(request);
                return Results.File(pdfBytes, "application/pdf", "report.pdf");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Results.StatusCode(500);
            }
        })
        .WithName("GenerateReport")
        .WithOpenApi();;
    }
}