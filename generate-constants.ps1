# generate-constants.ps1
#
# Connects to your Dynamics 365 / Dataverse environment and regenerates
# entity constant classes in the constants/ folder.
#
# Run this script once when setting up, and again whenever your Dataverse
# schema changes. Do NOT run it as part of normal program execution.
#
# Usage:
#   .\generate-constants.ps1

$scriptRoot   = $PSScriptRoot
$constantsDir = Join-Path $scriptRoot "constants"
$projectDir   = Join-Path (Join-Path $scriptRoot "scripts") "RetrieveConstants"

dotnet run --project $projectDir -- $constantsDir
