FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app
COPY *.sln ./
COPY IT.Scrapper.Domain.Core/*.csproj IT.Scrapper.Domain.Core/
COPY IT.Scrapper.Domain.Contracts/*.csproj IT.Scrapper.Domain.Contracts/
COPY IT.Scrapper.Domain.Parser/*.csproj IT.Scrapper.Domain.Parser/
COPY IT.Scrapper.Domain.Strategies/*.csproj IT.Scrapper.Domain.Strategies/
COPY IT.Scrapper.Infra.TelegraphClient/*.csproj IT.Scrapper.Infra.TelegraphClient/
COPY IT.Scrapper.WorkerService/*.csproj IT.Scrapper.WorkerService/
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out --no-restore

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 as base
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build-env /app/out .
RUN mkdir -p logs
EXPOSE 80 443
ENTRYPOINT ["dotnet", "IT.Scrapper.WorkerService.dll"]
