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
  "duties": [
    {
      "id": "D1",
      "events": [
        { "sequence": 1, "type": "vehicle_event", "vehicle_id": "V1" },
        { "sequence": 2, "type": "vehicle_event", "vehicle_id": "V1" }
      ]
    }
  ],
  "vehicles": [
    {
      "id": "V1",
      "events": [
        { "sequence": 1, "type": "service_trip", "trip_id": "T1" },
        { "sequence": 2, "type": "deadhead" }
      ]
    }
  ],
  "trips": [
    { "id": "T1", "origin_stop_id": "S1", "destination_stop_id": "S2", "arrival_time": "0.08:00", "departure_time": "0.08:15" }
  ],
  "stops": [
    { "id": "S1", "name": "Depot A" },
    { "id": "S2", "name": "Station B" }
  ]
}
```
