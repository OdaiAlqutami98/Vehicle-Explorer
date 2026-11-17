# Vehicle Explorer

Pick a car make and a model year (and optionally a vehicle type) to see the matching models in a
sortable table. Data comes from the free [NHTSA vPIC API](https://vpic.nhtsa.dot.gov/api/).

**Live demo:** http://13.63.185.205 (plain HTTP, hosted on AWS EC2 as a single Docker container)

- **Backend:** .NET 10 Web API (`backend/`)
- **Frontend:** Angular 21 + PrimeNG (`frontend/`)

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js 22](https://nodejs.org/) (includes npm)
- Internet access (the API calls NHTSA)

## Get the code

```bash
git clone https://github.com/OdaiAlqutami98/Vehicle-Explorer.git
cd Vehicle-Explorer
```

## Run with Docker

Needs [Docker](https://www.docker.com/) only (no .NET or Node).

```bash
docker compose up --build
```

Open http://localhost:8080. The API serves the frontend from the same port. Stop it with `Ctrl+C`,
then `docker compose down`.

## Run locally (without Docker)

**1. Start the backend** (terminal 1)

```bash
cd backend
dotnet run --project VehicleExplorer.Api
```

It runs on http://localhost:5080. Check http://localhost:5080/health shows `Healthy`.

**2. Start the frontend** (terminal 2)

```bash
cd frontend
npm ci
npm start
```

**3. Open the app** at http://localhost:4200

The frontend forwards `/api` calls to the backend on port 5080, so keep both terminals running.

## API endpoints

| Endpoint | Example | Returns |
|---|---|---|
| `GET /api/makes/GetMakes` | `/api/makes/GetMakes` | All makes, sorted by name |
| `GET /api/makes/GetVehicleTypes?makeId=` | `/api/makes/GetVehicleTypes?makeId=474` | Vehicle types of a make |
| `GET /api/makes/GetModels?makeId=&year=&vehicleType=` | `/api/makes/GetModels?makeId=474&year=2015&vehicleType=Passenger%20Car` | Models of a make for a year (`vehicleType` is optional) |
| `GET /health` | `/health` | `Healthy` |

Errors use ProblemDetails: 400 bad input, 429 too many requests, 502 NHTSA failed, 500 unexpected error.
More sample requests: `backend/VehicleExplorer.Api/VehicleExplorer.Api.http`.

