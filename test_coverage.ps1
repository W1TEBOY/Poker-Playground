<#
.SYNOPSIS
  Clean everything, restore, build, run tests with Coverlet, then produce a per-branch HTML coverage report.

.DESCRIPTION
  1. Delete all old bin/ and obj/ folders under the repo.
  2. Delete any existing TestResults/ folders or coverage XMLs.
  3. Determine the current Git branch name.
  4. dotnet restore
  5. dotnet build
  6. dotnet test --collect:"XPlat Code Coverage"
  7. Locate the newly‐created coverage.cobertura.xml file(s).
  8. Install ReportGenerator if needed.
  9. Run ReportGenerator into `CoverageReport/<branchName>/…`.
 10. Open the HTML report for this branch in your browser.

.NOTES
  - Requires PowerShell 7+ (Core). Works on Windows, Linux, or macOS.
  - Assumes your test project lives at "./Poker.Core.Tests/Poker.Core.Tests.csproj". Change `-TestProject` if yours is elsewhere.
  - Make sure your test project references `<PackageReference Include="coverlet.collector" Version="3.*" />` in its .csproj.

.PARAMETER TestProject
  Path (relative or absolute) to your test project’s .csproj. Default = "./Poker.Core.Tests/Poker.Core.Tests.csproj".

.PARAMETER Configuration
  Build configuration. Default = "Debug". Set to "Release" if you want a Release build.

.EXAMPLE
  pwsh ./Generate-Coverage.ps1
  pwsh ./Generate-Coverage.ps1 -TestProject "./src/MyTests/MyProject.Tests.csproj" -Configuration Release
#>

[CmdletBinding()]
param (
    [Parameter(Mandatory = $false)]
    [string] $TestProject     = "./Poker.Core.Tests/Poker.Core.Tests.csproj",

    [Parameter(Mandatory = $false)]
    [ValidateSet("Debug","Release")]
    [string] $Configuration   = "Debug"
)

function Log([string] $msg, [ConsoleColor] $color = "White") {
    $time = (Get-Date).ToString("HH:mm:ss")
    Write-Host "[$time] $msg" -ForegroundColor $color
}

#-----------------------------
# 0) Basic checks
#-----------------------------
Log "Starting per-branch coverage script..." "Cyan"

# Ensure PowerShell 7+
if ($PSVersionTable.PSVersion.Major -lt 7) {
    Write-Error "This script requires PowerShell 7 or newer. You are running $($PSVersionTable.PSVersion)."
    exit 1
}

# Verify dotnet is installed
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "ERROR: 'dotnet' was not found in PATH. Please install .NET SDK."
    exit 1
}

# Verify Git is installed (to detect current branch)
if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Error "ERROR: 'git' was not found in PATH. Please ensure Git is installed so we can detect branch name."
    exit 1
}

# Verify TestProject file exists
if (-not (Test-Path $TestProject)) {
    Write-Error "ERROR: Test project file not found at '$TestProject'. Adjust the -TestProject parameter."
    exit 1
}

#-----------------------------
# 1) Delete ALL old bin/ and obj/ directories
#-----------------------------
Log "Deleting all old bin/ and obj/ folders..." "Yellow"
Get-ChildItem -Recurse -Directory -Force `
    | Where-Object { $_.Name -in 'bin','obj' } `
    | ForEach-Object {
        try {
            Remove-Item -Recurse -Force $_.FullName -ErrorAction Stop
            Log "  Removed: $_.FullName" "DarkYellow"
        } catch {
            Write-Warning "  Could not delete $_.FullName: $_"
        }
    }

#-----------------------------
# 2) Delete any old TestResults/ or coverage XMLs
#-----------------------------
Log "Deleting old TestResults/ and coverage XML files..." "Yellow"
if (Test-Path "./TestResults") {
    try {
        Remove-Item -Recurse -Force "./TestResults"
        Log "  Removed: ./TestResults" "DarkYellow"
    } catch {
        Write-Warning "  Could not delete ./TestResults: $_"
    }
}

Get-ChildItem -Recurse -Filter "coverage.cobertura.xml" -File |
    ForEach-Object {
        try {
            Remove-Item -Force $_.FullName
            Log "  Removed old coverage file: $($_.FullName)" "DarkYellow"
        } catch {
            Write-Warning "  Could not delete coverage XML $($_.FullName): $_"
        }
    }

#-----------------------------
# 3) Determine current Git branch
#-----------------------------
Log "Detecting current Git branch..." "Yellow"
$branchNameRaw = (git rev-parse --abbrev-ref HEAD).Trim()
if ([string]::IsNullOrWhiteSpace($branchNameRaw)) {
    Write-Error "ERROR: Unable to detect Git branch (got empty result)."
    exit 1
}

# Some CI/workflows might produce branch names with slashes (feature/foo). Replace '/' with '_' for a valid folder name.
$branchName = $branchNameRaw -replace '/', '_'
Log "  Current branch is: '$branchNameRaw' → using output folder 'CoverageReport/$branchName'" "Green"

#-----------------------------
# 4) dotnet restore
#-----------------------------
Log "Restoring all projects..." "Yellow"
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "ERROR: 'dotnet restore' failed (exit code $LASTEXITCODE)."
    exit 1
}

#-----------------------------
# 5) dotnet build
#-----------------------------
Log "Building solution (Configuration = $Configuration)..." "Yellow"
dotnet build --configuration $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Error "ERROR: 'dotnet build' failed (exit code $LASTEXITCODE)."
    exit 1
}

#-----------------------------
# 6) Run tests with Coverlet
#-----------------------------
Log "Running tests with Coverlet code coverage..." "Yellow"
dotnet test $TestProject `
    --configuration $Configuration `
    --collect:"XPlat Code Coverage"
if ($LASTEXITCODE -ne 0) {
    Write-Error "ERROR: 'dotnet test' (with coverage) failed (exit code $LASTEXITCODE)."
    exit 1
}

#-----------------------------
# 7) Locate coverage.cobertura.xml
#-----------------------------
Log "Locating Cobertura XML file(s) under TestResults/..." "Yellow"
$coverageFiles = Get-ChildItem -Recurse -Filter "coverage.cobertura.xml" -File |
                 Select-Object -ExpandProperty FullName

if (-not $coverageFiles) {
    Write-Error "ERROR: No 'coverage.cobertura.xml' files found under ./TestResults after test run."
    exit 1
}

Log "Found coverage files:" "Green"
foreach ($f in $coverageFiles) {
    Write-Host "   $f" -ForegroundColor DarkGreen
}

#-----------------------------
# 8) Install ReportGenerator CLI if missing
#-----------------------------
Log "Checking for ReportGenerator CLI..." "Yellow"
if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
    Log "ReportGenerator not found. Installing global tool..." "DarkYellow"
    dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.1.10
    if ($LASTEXITCODE -ne 0) {
        Write-Error "ERROR: Failed to install ReportGenerator (exit code $LASTEXITCODE)."
        exit 1
    }

    # On *nix, ensure ~/.dotnet/tools is in PATH for this session
    if ($IsLinux -or $IsMacOS) {
        $toolsPath = "$HOME/.dotnet/tools"
        if (-not ($env:PATH -split [System.IO.Path]::PathSeparator | Where-Object { $_ -eq $toolsPath })) {
            Log "Adding '$toolsPath' to PATH temporarily..." "DarkYellow"
            $env:PATH = "$($env:PATH)$([System.IO.Path]::PathSeparator)$toolsPath"
        }
    }

    Log "ReportGenerator installed successfully." "Green"
}
else {
    $rgLoc = (Get-Command reportgenerator).Source
    Log "ReportGenerator already installed: $rgLoc" "Green"
}

#-----------------------------
# 9) Generate per-branch HTML report
#-----------------------------
$topCoverageDir = Join-Path (Get-Location) "CoverageReport"
$branchReportDir = Join-Path $topCoverageDir $branchName

# Create top-level CoverageReport/ if missing
if (-not (Test-Path $topCoverageDir)) {
    New-Item -ItemType Directory -Path $topCoverageDir | Out-Null
    Log "Created folder: $topCoverageDir" "DarkYellow"
}

# If the branch-specific folder already exists, clear it
if (Test-Path $branchReportDir) {
    Log "Clearing existing branch‐report folder '$branchReportDir'..." "DarkYellow"
    Remove-Item -Recurse -Force $branchReportDir
}

# Create a fresh folder for this branch
New-Item -ItemType Directory -Path $branchReportDir | Out-Null
Log "Using branch report folder: $branchReportDir" "DarkYellow"

# Join multiple coverage XMLs with semicolons
$reportsArg = $coverageFiles -join ";"

Log "Running ReportGenerator to emit HTML into '$branchReportDir'..." "Yellow"
reportgenerator `
    -reports:$reportsArg `
    -targetdir:$branchReportDir `
    -reporttypes:"Html;HtmlSummary"

if ($LASTEXITCODE -ne 0) {
    Write-Error "ERROR: ReportGenerator failed (exit code $LASTEXITCODE)."
    exit 1
}

Log "✔ HTML coverage report generated in: $branchReportDir" "Green"
Log "Open '$branchReportDir/index.htm' in your browser to inspect coverage for branch '$branchName'." "Cyan"

#-----------------------------
# 10) Open the HTML report automatically
#-----------------------------
$indexFile = Join-Path $branchReportDir "index.htm"
if (Test-Path $indexFile) {
    Log "Opening coverage report in default browser..." "Yellow"

    if ($IsWindows) {
        Start-Process -FilePath $indexFile
    }
    elseif ($IsMacOS) {
        & open $indexFile
    }
    elseif ($IsLinux) {
        & xdg-open $indexFile
    }
    else {
        Log "Couldn’t detect OS; please open $indexFile manually." "DarkYellow"
    }
}
else {
    Log "index.htm not found (expected at: $indexFile)" "Red"
}
