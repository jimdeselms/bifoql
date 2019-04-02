FROM microsoft/dotnet:2.1.301-sdk AS builder

WORKDIR /app

RUN curl -sL https://deb.nodesource.com/setup_10.x |  bash -
RUN apt-get install -y nodejs

COPY src/Bifoql/Bifoql.csproj src/Bifoql/Bifoql.csproj
COPY src/Bifoql.Playpen/Bifoql.Playpen.csproj src/Bifoql.Playpen/Bifoql.Playpen.csproj

RUN dotnet restore src/Bifoql/Bifoql.csproj
RUN dotnet restore src/Bifoql.Playpen/Bifoql.Playpen.csproj

COPY src/Bifoql src/Bifoql
COPY src/Bifoql.Playpen src/Bifoql.Playpen

RUN dotnet publish src/Bifoql.Playpen -c Release -o out --no-restore

FROM microsoft/dotnet:2.1.1-aspnetcore-runtime
WORKDIR /app
COPY --from=builder /app/src/Bifoql.Playpen/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "Bifoql.Playpen.dll"]