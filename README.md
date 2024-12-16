# Duty Report API

## Overview
The **Duty Report API** is a backend service designed to generate PDF reports based on scheduling data. It processes details about vehicles, trips, stops, and duties, identifying key metrics such as start/end times, breaks, and vehicle events. The generated reports are exported as PDF files, making it easy to analyze and share.

## Features
- Generate PDF reports with detailed duty information.
- Extract and display:
    - **Step 1:** Duty start and end times.
    - **Step 2:** Service trip start and end Stop Name 
    - **Step 3:** Breaks longer than 15 minutes with details.
- Modular design for ease of extension and maintainability.

## Technologies Used
- **C#**
- **.NET 8**
- **Minimal APIs** for endpoint definition.
- **QuestPDF** for generating PDF documents.
- **xUnit** and **FluentAssertions** for unit testing.

## Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/your-repo/duty-report-api.git
   ```
2. Navigate to the project directory:
   ```bash
   cd duty-report-api
   ```
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Build the project:
   ```bash
   dotnet build
   ```

## Usage
### Running the API
1. Start the application:
   ```bash
   dotnet run
   ```
2. Access the Swagger UI at:
   ```
   https://localhost:<port>/swagger
   ```

### API Endpoint
#### **POST /generate-report**
- **Description:** Accepts scheduling data in JSON format and generates a PDF report.
- **Request Body:**
  ```json
  {
    "duties": [...],
    "vehicles": [...],
    "trips": [...],
    "stops": [...]
  }
  ```
- **Response:** Returns the generated PDF file.

## Key Components
### 1. `ScheduleModel`
Represents the data structure used in the report generation process. Includes:
- **Duties**: List of duty events.
- **Vehicles**: Vehicle events tied to trips.
- **Trips**: Links stops with scheduled times.
- **Stops**: Names and locations of stops.

### 2. `ReportGenerator`
Contains the logic for processing scheduling data and generating the PDF.
- Methods include:
    - `GeneratePdf`: Creates the report.
    - `GetBreaks`: Identifies breaks longer than 15 minutes.
    - `GetDutyEventStartTime`/`GetDutyEventEndTime`: Calculates start and end times of duty events.

### 3. Testing
- Unit tests are implemented using **xUnit**.
- Key scenarios include:
    - Validating break detection.
    - Ensuring correct PDF generation for each step.
    - Testing helper methods like `GetStopName` and `GetDestinationStopId`.

Run tests:
```bash
dotnet test
```

## Example JSON Input
```json
{
  "stops": [
    { "stop_id": "S1", "stop_name": "Depot A", "latitude": 34.053788, "longitude": -118.243691, "is_depot": true },
    { "stop_id": "S2", "stop_name": "Station B", "latitude": 34.045356, "longitude": -118.260212, "is_depot": false }
  ],
  "vehicles": [
    {
      "vehicle_id": "V1",
      "vehicle_events": [
        { "vehicle_event_sequence": 0, "vehicle_event_type": "service_trip", "trip_id": "T1", "duty_id": "1" },
        { "vehicle_event_sequence": 1, "vehicle_event_type": "deadhead", "trip_id": "T2", "duty_id": "1", "start_time":"0.09:00" , "end_time":"0.11.00" }
      ]
    }
  ],
  "trips": [
    { "trip_id": "T1", "origin_stop_id": "S1", "destination_stop_id": "S2", "departure_time": "0.07:00", "arrival_time": "0.07:45" },
    { "trip_id": "T2", "origin_stop_id": "S2", "destination_stop_id": "S1", "departure_time": "0.08:00", "arrival_time": "0.08:45" }
  ],
  "duties": [
    {
      "duty_id": "1",
      "duty_events": [
        { "duty_event_type": "vehicle_event", "vehicle_id": "V1", "vehicle_event_sequence": 0 },
        { "duty_event_type": "vehicle_event", "vehicle_id": "V1", "vehicle_event_sequence": 1 }
      ]
    }
  ]
}
```
