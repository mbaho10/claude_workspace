FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY BudgetPlanner.sln .
COPY src/BudgetPlanner.Api/BudgetPlanner.Api.csproj src/BudgetPlanner.Api/
COPY src/BudgetPlanner.Application/BudgetPlanner.Application.csproj src/BudgetPlanner.Application/
COPY src/BudgetPlanner.Domain/BudgetPlanner.Domain.csproj src/BudgetPlanner.Domain/
COPY src/BudgetPlanner.Infrastructure/BudgetPlanner.Infrastructure.csproj src/BudgetPlanner.Infrastructure/
RUN dotnet restore
COPY . .
RUN dotnet publish src/BudgetPlanner.Api/BudgetPlanner.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "BudgetPlanner.Api.dll"]
