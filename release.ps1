param ([string]$NewVersion = $(throw "Need a version"))
set-strictmode -version 2.0
$ErrorActionPreference="Stop"

Add-Type -AssemblyName "System.IO.Compression.FileSystem"

function CheckError ($errormessage) {
    if( $LASTEXITCODE -ne 0)
    {
        throw $errormessage + ", Exitcode: " + $LASTEXITCODE 
    }    
}
function CopyFiles($fromDir,$toDir)
{
    if(-not [System.IO.Directory]::Exists($toDir))
    {
        [System.IO.Directory]::CreateDirectory($toDir)
    }
    $files = [System.IO.Directory]::GetFiles($fromDir)
    foreach ($file in $files)
    {
        $newfileName = [System.IO.Path]::GetFileName($file)
        $newfile = [System.IO.Path]::Combine($toDir,$newfileName)
        [System.IO.File]::Copy($file,$newfile)
    }
}

function CreatePackage ($target, $version) {
    
    dotnet restore tapi.sln -r $target -v q
    CheckError -errormessage "Unit Tests Failed"
    dotnet build tapi.sln -r $target  -v q
    CheckError -errormessage "Unit Tests Failed"
    dotnet publish tapi.sln -c release -r $target -v q
    CheckError -errormessage "Unit Tests Failed"


    $toDir = [System.IO.Path]::Combine("artifacts",$target)
    
    if([System.IO.Directory]::Exists($toDir))
    {
        [System.IO.Directory]::Delete($toDir,$TRUE)
    }
    
    $toAppDir = [System.IO.Path]::Combine($toDir,"app")
    $fromAppDir = [System.IO.Path]::Combine("out/bin/release/netcoreapp1.1",$target,"publish")
    CopyFiles -fromDir $fromAppDir -toDir $toAppDir

    $fromSampleDataDir = "sample/data";
    $toSampleDataDir = [System.IO.Path]::Combine($toDir,$fromSampleDataDir)
    CopyFiles -fromDir $fromSampleDataDir -toDir $toSampleDataDir
    
    $fromSampleTemplateDir = "sample/templates";
    $toSampleTemplateDir = [System.IO.Path]::Combine($toDir,$fromSampleTemplateDir)
    CopyFiles -fromDir $fromSampleTemplateDir -toDir $toSampleTemplateDir

    $fromSampleConfFile = "sample/config.xml";
    $toSampleConfFile = [System.IO.Path]::Combine($toDir,$fromSampleConfFile)
    [System.IO.File]::Copy($fromSampleConfFile,$toSampleConfFile)

    $fromSampleConfFile = "src/tapi.core/Model/tapi.xsd";
    $toSampleConfFile = [System.IO.Path]::Combine($toAppDir,"tapi.xsd")
    [System.IO.File]::Copy($fromSampleConfFile,$toSampleConfFile)


    if($target -eq "win10-x64")
    {
        $command = "app\tapi.exe -conf:sample/config.xml -data:sample/data -templates:sample/templates/ -out:sample/out/"
        $runFileName = [System.IO.Path]::Combine($toDir,"run.cmd")
        [System.IO.File]::WriteAllText($runFileName,$command)    
    }
    else 
    {
        $command = "app\tapi -conf:sample/config.xml -data:sample/data -templates:sample/templates/ -out:sample/out/"
        $runFileName = [System.IO.Path]::Combine($toDir,"run.sh")
        [System.IO.File]::WriteAllText($runFileName,$command)
    }

    $toFile = [System.IO.Path]::Combine("artifacts",$target + "_v"+ $version +".zip")
    [System.IO.File]::Delete($toFile)
    [System.IO.Compression.ZipFile]::CreateFromDirectory($toDir,$toFile)
    [System.IO.Directory]::Delete($toDir,$TRUE)
}

$starttime = [System.DateTime]::Now;
Write-Output "Starting to create a new release"

Write-Output "Update Version"
$VersionRegex = "\d+\.\d+\.\d+\.\d+"
$file = "src/tapi.core/properties.cs"
$filecontent = Get-Content($file)
$filecontent -replace $VersionRegex, $NewVersion | Out-File $file

Write-Output "Run unittests"
dotnet restore tapi.sln -v q
CheckError -errormessage "Unit Tests Failed"
dotnet build tapi.sln -v q
CheckError -errormessage "Unit Tests Failed"
dotnet test src/tapi.unittest/tapi.unittest.csproj --no-build -o ../../out/bin/debug -v q
CheckError -errormessage "Unit Tests Failed"

CreatePackage -target "win10-x64" -version $NewVersion
CreatePackage -target "osx.10.11-x64" -version $NewVersion
CreatePackage -target "ubuntu.14.04-x64" -version $NewVersion

Write-Output "Cleanup"
[System.IO.Directory]::Delete("out/bin/release",$TRUE)
dotnet restore tapi.sln -v q
CheckError -errormessage "Unit Tests Failed"

$time = [System.DateTime]::Now - $starttime;
Write-Output ""
Write-Output "Done " + $time.ToString()


