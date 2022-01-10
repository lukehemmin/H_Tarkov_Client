# VSCode.ps1
# Copyright: © SPT-AKI 2021
# License: NCSA
# Authors:
# - Merijn Hendriks
# - waffle.lord

param (
        [switch]$VSBuilt,
        [string]$config = "Release"
    )

#setup variables
Write-Host "Running build for config: $($config)" -ForegroundColor Cyan
$buildDir = "Build/"
$launcherData = "./Aki.Launcher/Aki_Data/"
$publishSwitches = "--nologo --verbosity minimal --runtime win10-x64 --configuration $($config) -p:PublishSingleFile=true --no-self-contained"
$pathPrefix = "./"

if($VSBuilt) {
    $buildDir = "../Build"
    $launcherData = "../Aki.Launcher/Aki_Data"
    $pathPrefix = "../"

    if($config -eq "Release") {
        $publishSwitches += " --no-build"
    }
}

$publishSwitches += " --output ${buildDir}"
$publishArgs = "publish " + $pathPrefix + "Aki.Launcher/Aki.Launcher.csproj " + $publishSwitches
$publishCLIArgs = "publish " + $pathPrefix + "Aki.Launcher.CLI/Aki.Launcher.CLI.csproj " + $publishSwitches

#removes the Obj and Bin directories and their contents
function CleanBuild 
{
    [string[]]$delPaths = Get-ChildItem -Recurse | where {$_.FullName -like "*\bin" -or $_.FullName -like "*\obj"} | select -ExpandProperty FullName

    foreach ($path in $delPaths)
    {
        Write-Host "  Delete: $($path)"
        Remove-Item $path -Force -Recurse
    }
}

# build the project
Write-Host "Cleaning previous builds ..." -ForegroundColor Cyan

#remove build dir to avoid reminant data
if (Test-Path $buildDir) 
{
    Remove-Item $buildDir -Recurse -Force
}

#make sure bin and obj don't exist before cleaning or errors may occur from previous builds ran from VS
if(-not $VSBuilt)
{
    CleanBuild

    $cleanProcess = Start-Process "dotnet" -PassThru -NoNewWindow -ArgumentList "clean --nologo --verbosity quiet --runtime win10-x64"
    Wait-Process -InputObject $cleanProcess
    $cleanProcess.Dispose()
}

Write-Host "`nDone" -ForegroundColor Cyan

Write-Host "`nBuilding launcher ..." -ForegroundColor Cyan
$publishProcess = Start-Process "dotnet" -PassThru -NoNewWindow -ArgumentList $publishArgs
Wait-Process -InputObject $publishProcess
$publishProcess.Dispose()
Write-Host "`nDone" -ForegroundColor Cyan

Write-Host "`nBuilding launcher CLI..." -ForegroundColor Cyan
$publishProcess = Start-Process "dotnet" -PassThru -NoNewWindow -ArgumentList $publishCLIArgs
Wait-Process -InputObject $publishProcess
$publishProcess.Dispose()
Write-Host "`nDone" -ForegroundColor Cyan

if($config -eq "Release") {
    Remove-Item "$($buildDir)\Launcher.pdb"
    Remove-Item "$($buildDir)\LauncherCLI.pdb"
}

Remove-Item "$($buildDir)\Aki.ByteBanger.pdb"
Remove-Item "$($buildDir)\Aki.Launcher.Base.pdb"

#copy aki_data folder
Write-Host "`nCopying Aki_Data folder ... " -NoNewLine

Copy-Item -Path $launcherData -Destination "./${buildDir}/Aki_Data" -Recurse -Force -ErrorAction SilentlyContinue

if (Test-Path "$($buildDir)/Aki_Data") 
{
    Write-host "OK" -ForegroundColor Green
}
else 
{
    Write-host "Folder doesn't appear to have been copied.`nError: $($Error[0])" -ForegroundColor Red
}

# delete build waste
Write-Host "`nCleaning garbage produced by build..." -ForegroundColor Cyan

CleanBuild

Write-Host "`nDone" -ForegroundColor Cyan