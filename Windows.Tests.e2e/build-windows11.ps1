param(
	[string] $BasePath = "C:\Users\Germi\Downloads"
)

$IsoFilePath = "$BasePath\windows11-21h2.iso"
$OutputPath = "$BasePath\Downloads\"

if (-! $(Test-Path -Path $IsoFilePath -PathType Leaf)) {
    .\download-iso.ps1 -Win "Windows 11" -Rel "21H2 v1 (Build 22000.318 - 2021.11)" -Ed "Pro" -OutputFilePath $IsoFilePath
}

.\pack-image.ps1 -IsoFilePath $IsoFilePath -WindowsVersion 11 -OutputDirectoryPath $OutputPath