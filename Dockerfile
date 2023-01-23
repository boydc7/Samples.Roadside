FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

ARG buildconfig

WORKDIR /usr/src/api

COPY . .

RUN dotnet restore && dotnet publish Samples.Roadside.sln -c ${buildconfig} -o publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

WORKDIR /opt/api

COPY --from=build-env /usr/src/api/publish /opt/api

ENV ASPNETCORE_URLS=http://+:8082
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0

EXPOSE 8082

ENTRYPOINT ["/opt/api/roadside"]
