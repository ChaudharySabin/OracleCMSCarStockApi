################################
# 1) Build your app
################################
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy just the csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and publish
COPY . ./
RUN dotnet publish -c Release -o /app/out

################################
# 2) Create the runtime image
################################
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy the published output from build stage
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:80

# Expose port 80 and run
EXPOSE 80
ENTRYPOINT ["dotnet", "api.dll"]