FROM microsoft/dotnet as build

WORKDIR /app

COPY . ./

RUN dotnet publish test/Vp.Services.Web

FROM microsoft/dotnet

WORKDIR /app

COPY --from=build /app/test/Vp.Services.Web/bin/Debug/netcoreapp2.1/publish .

EXPOSE 80

ENTRYPOINT [ "dotnet", "./Vp.Services.Web.dll" ]