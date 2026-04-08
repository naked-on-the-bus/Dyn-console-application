# generate-constants.ps1
#
# Connects to your Dynamics 365 / Dataverse environment and regenerates
# Constants.cs in the project source directory.
#
# Run this script once when setting up, and again whenever your Dataverse
# schema changes. Do NOT run it as part of normal program execution.
#
# Usage:
#   .\generate-constants.ps1

$projectDir = Join-Path $PSScriptRoot "console"
dotnet run --project $projectDir -- --scaffold
