using System.Text.Json.Serialization;

namespace Optibus.ReportsExporter.API
{
    public class ScheduleModel
    {
        [JsonPropertyName("stops")]
        public List<Stop> Stops { get; set; }

        [JsonPropertyName("trips")]
        public List<Trip> Trips { get; set; }

        [JsonPropertyName("vehicles")]
        public List<Vehicle> Vehicles { get; set; }

        [JsonPropertyName("duties")]
        public List<Duty> Duties { get; set; }
    }

    public class Stop
    {
        [JsonPropertyName("stop_id")]
        public string Id { get; set; }

        [JsonPropertyName("stop_name")]
        public string Name { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("is_depot")]
        public bool IsDepot { get; set; }
    }

    public class Trip
    {
        [JsonPropertyName("trip_id")]
        public string Id { get; set; }

        [JsonPropertyName("route_number")]
        public string RouteNumber { get; set; }

        [JsonPropertyName("origin_stop_id")]
        public string OriginStopId { get; set; }

        [JsonPropertyName("destination_stop_id")]
        public string DestinationStopId { get; set; }

        [JsonPropertyName("departure_time")]
        public string DepartureTime { get; set; }

        [JsonPropertyName("arrival_time")]
        public string ArrivalTime { get; set; }
    }

    public class Vehicle
    {
        [JsonPropertyName("vehicle_id")]
        public string Id { get; set; }

        [JsonPropertyName("vehicle_events")]
        public List<VehicleEvent> Events { get; set; }
    }

    public class VehicleEvent
    {
        [JsonPropertyName("vehicle_event_sequence")]
        public int Sequence { get; set; }

        [JsonPropertyName("vehicle_event_type")]
        public string Type { get; set; }

        [JsonPropertyName("start_time")]
        public string StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public string EndTime { get; set; }

        [JsonPropertyName("origin_stop_id")]
        public string OriginStopId { get; set; }

        [JsonPropertyName("destination_stop_id")]
        public string DestinationStopId { get; set; }

        [JsonPropertyName("duty_id")]
        public string DutyId { get; set; }
    }

    public class Duty
    {
        [JsonPropertyName("duty_id")]
        public string Id { get; set; }

        [JsonPropertyName("duty_events")]
        public List<DutyEvent> Events { get; set; }
    }

    public class DutyEvent
    {
        [JsonPropertyName("duty_event_sequence")]
        public int Sequence { get; set; }

        [JsonPropertyName("duty_event_type")]
        public string Type { get; set; }

        [JsonPropertyName("vehicle_event_sequence")]
        public int VehicleEventSequence { get; set; }

        [JsonPropertyName("vehicle_id")]
        public string VehicleId { get; set; }
        
        [JsonPropertyName("start_time")]
        public string StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public string EndTime { get; set; }
    }
}
