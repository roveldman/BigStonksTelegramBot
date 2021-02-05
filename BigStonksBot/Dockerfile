FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR ./app
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out
ENTRYPOINT ["dotnet", "out/BigStonksBot.dll"]
