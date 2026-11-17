# ---------- Stage 1: build the Angular app ----------
FROM node:22-alpine AS frontend-build
WORKDIR /app

# Copy only the package files first, so Docker can reuse the installed
# packages from cache when only source code changes.
COPY frontend/package*.json ./
RUN npm ci

COPY frontend/ ./
RUN npm run build

# ---------- Stage 2: publish the .NET API ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src

COPY backend/ ./
RUN dotnet publish VehicleExplorer.Api/VehicleExplorer.Api.csproj -c Release -o /publish

# ---------- Stage 3: the small image that actually runs ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=backend-build /publish ./
# The API serves the Angular files from wwwroot.
COPY --from=frontend-build /app/dist/vehicle-explorer/browser ./wwwroot

# Run as the non-root user that the base image provides.
USER $APP_UID

# ASP.NET Core listens on 8080 inside the container by default.
EXPOSE 8080
ENTRYPOINT ["dotnet", "VehicleExplorer.Api.dll"]