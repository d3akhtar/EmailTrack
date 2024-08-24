FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# copy everything from csproj to current workdir (would be /app)
COPY *.csproj ./
# basically pulls down the packages contained in csproj
RUN dotnet restore

# copy everything else
COPY . ./

RUN dotnet publish -c Release -o out

# runtime 
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT [ "dotnet", "EmailSender.dll" ]