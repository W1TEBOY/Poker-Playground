# syntax=docker/dockerfile:1.4

# ------------------------------------------------
# 1) DEV stage: full SDK + watch/run for local work
# ------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS dev
WORKDIR /src

# copy sln and csprojs for max cache-hit on restore
COPY *.sln ./
COPY Poker.Core/*.csproj Poker.Core/
COPY Poker.WebSockets/*.csproj Poker.WebSockets/
RUN dotnet restore

# Install Node.js 18.x LTS
RUN curl -fsSL https://deb.nodesource.com/setup_18.x | bash - \
 && apt-get install -y nodejs \
 && npm --version && node --version  # sanity check


# copy all source
COPY . .

# expose and start in watch mode
EXPOSE 80
ENTRYPOINT ["dotnet", "watch", "--project", "Poker.WebSockets", "run"]


# ------------------------------------------------
# 2) LINT stage: style & formatting checks
# ------------------------------------------------
FROM dev AS lint
# install the formatting tool
RUN dotnet tool install -g dotnet-format \
 && export PATH="$PATH:/root/.dotnet/tools" \
 # verify no formatting changes needed
 && dotnet format --verify-no-changes


# ------------------------------------------------
# 3) TEST stage: run your unit tests
# ------------------------------------------------
FROM dev AS test
# run tests (no rebuild thanks to cached dev layer)
RUN dotnet test Poker.Core/Poker.Core.csproj --no-build --logger:trx \
 && dotnet test Poker.WebSockets/Poker.WebSockets.csproj --no-build --logger:trx


# ------------------------------------------------
# 4) BUILD stage: publish trimmed release
# ------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# restore only the WebSockets project (caches faster)
COPY *.sln ./
COPY Poker.WebSockets/*.csproj Poker.WebSockets/
COPY Poker.Core/*.csproj Poker.Core/
RUN dotnet restore Poker.WebSockets/Poker.WebSockets.csproj

# copy & publish release
COPY . .
RUN dotnet publish Poker.WebSockets/Poker.WebSockets.csproj \
    -c Release \
    -o /app/publish \
    /p:TrimUnusedDependencies=true


# ------------------------------------------------
# 5) PROD stage: slim runtime image
# ------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS prod
WORKDIR /app
COPY --from=build /app/publish ./

EXPOSE 80
ENTRYPOINT ["dotnet", "Poker.WebSockets.dll"]


# ------------------------------------------------
# 6) SECURITY-SCAN stage: check for vulnerable deps
# ------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS security-scan
WORKDIR /src
COPY *.sln ./
COPY Poker.Core/*.csproj Poker.Core/
COPY Poker.WebSockets/*.csproj Poker.WebSockets/
RUN dotnet restore Poker.WebSockets/Poker.WebSockets.csproj

# list any known vulnerable packages
RUN dotnet list Poker.WebSockets/Poker.WebSockets.csproj package --vulnerable
