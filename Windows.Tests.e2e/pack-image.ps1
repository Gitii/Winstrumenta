param(
	[Parameter(Mandatory=$true)][string] $IsoFilePath,
	[Parameter(Mandatory=$true)][string] $WindowsVersion,
    [Parameter(Mandatory=$true)][string] $OutputDirectoryPath
)

function New-TemporaryDirectory {
    $parent = [System.IO.Path]::GetTempPath()
    [string] $name = [System.Guid]::NewGuid()
    New-Item -ItemType Directory -Path (Join-Path $parent $name)
}

function New-TemporaryFile2($Extension = ".tmp") {
    $parent = [System.IO.Path]::GetTempPath()
    [string] $name = [System.Guid]::NewGuid()
    New-Item -ItemType File -Path (Join-Path $parent "$name$Extension")
}

# Disable progress bar to speed up download
$ProgressPreference = 'SilentlyContinue'

$TempFolder = New-TemporaryDirectory
$TempFile   = New-TemporaryFile2 ".zip"
try {
    Write-Host "Downloading Windows (10 / 11) Desktop Templates For Packer..."
    Invoke-WebRequest "https://github.com/Gitii/packer-windows-desktop/archive/refs/heads/main.zip" -OutFile $TempFile.FullName
    
    Write-Host "Preparing workspace..."
    Expand-Archive -LiteralPath $TempFile.FullName -DestinationPath $TempFolder
    
    $Path = Join-Path $TempFolder "packer-windows-desktop-main"
    
    $ScriptPath = Join-Path $Path "build_windows_${WindowsVersion}.bat"
    
    $env:Path += ";C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Deployment Tools\amd64\Oscdimg"
    
    try {
        Push-Location $Path
        & packer build --force --only=hyperv-iso -var "iso_url=$IsoFilePath" -var "switch_name=Internet" "windows_${WindowsVersion}.json"
    }
    finally {
        Pop-Location
    }
} finally {
    Write-Host "Cleaning up"
    Remove-Item $TempFolder -Recurse -Force | Out-Null
    Remove-Item $TempFile -Force | Out-Null
}

Write-Host "Successfully build vm image and saved it to $OutputFilePath"
