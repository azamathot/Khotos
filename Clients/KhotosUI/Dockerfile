# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Clients/KhotosUI/KhotosUI.csproj", "Clients/KhotosUI/"]
COPY ["Shared/SharedModels/SharedModels.csproj", "Shared/SharedModels/"]
RUN dotnet restore "./Clients/KhotosUI/KhotosUI.csproj"
COPY . .
WORKDIR "/src/Clients/KhotosUI"
RUN dotnet build "KhotosUI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./KhotosUI.csproj" -c Release -o /app/publish

# Final stage
FROM nginx:alpine
EXPOSE 8080
#EXPOSE 443
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY Clients/KhotosUI/nginx.conf /etc/nginx/conf.d/default.conf
#ENTRYPOINT ["nginx"]
#docker build -t khotosui -f clients/khotosui/Dockerfile .
#docker run -p 7000:80 --name khotosui khotosui