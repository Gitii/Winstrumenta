param(
	[Parameter(Mandatory=$true)][string] $Win, # Specify Windows version (e.g. "Windows 11")
	[Parameter(Mandatory=$true)][string] $Rel, # Specify Windows release (e.g. "21H2 v1 (Build 22000.318 - 2021.11)")
	[Parameter(Mandatory=$true)][string] $Ed, # Specify Windows edition (e.g. "Pro")
    [Parameter(Mandatory=$true)][string] $OutputFilePath
)

# Disable progress bar to speed up download
$ProgressPreference = 'SilentlyContinue'

# Download fido
$FidoFile = New-TemporaryFile
try {
    $FidoFile.MoveTo([System.IO.Path]::ChangeExtension($FidoFile.FullName, ".ps1"))

    Write-Host "Downloading fido to $($FidoFile.FullName)"
    Invoke-WebRequest "https://raw.githubusercontent.com/pbatard/Fido/master/Fido.ps1" -OutFile $FidoFile.FullName

    Write-Host "Building iso download url"
    $DownloadUrl = $(& $FidoFile.FullName -Win $Win -Rel $Rel -Ed $Ed -Lang English -Arch x64 -GetUrl)

    Write-Host "Downloading iso for $Win $Rel $Ed"
    Invoke-WebRequest $DownloadUrl -OutFile $OutputFilePath

} finally {
    Write-Host "Cleaning up"
    Remove-Item $FidoFile | Out-Null
}

Write-Host "Successfully downloaded iso image to $OutputFilePath"

