FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src
COPY MyWishlist.sln ./
COPY Wishlist.Domain/Wishlist.Domain.csproj Wishlist.Domain/
COPY Wishlist.Contracts/Wishlist.Contracts.csproj Wishlist.Contracts/
COPY Wishlist.Application/Wishlist.Application.csproj Wishlist.Application/
COPY Wishlist.Infrastructure/Wishlist.Infrastructure.csproj Wishlist.Infrastructure/
COPY Wishlist.Api/Wishlist.Api.csproj Wishlist.Api/
RUN dotnet restore Wishlist.Api/Wishlist.Api.csproj
COPY . .
RUN dotnet publish Wishlist.Api/Wishlist.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Wishlist.Api.dll"]
