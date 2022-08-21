param(
	[Parameter(Mandatory=$true)][string] $IsoFilePath,
	[Parameter(Mandatory=$true)][string] $WindowsVersion,
    [Parameter(Mandatory=$true)][string] $OutputDirectoryPath
)

$env:Path += ";C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Deployment Tools\amd64\Oscdimg"

New-Item -Path $OutputDirectoryPath -ItemType "directory" -Force | Out-Null
    
& packer build --force -var "iso_url=$IsoFilePath" -var "switch_name=Internet" -var "output_directory=$OutputDirectoryPath" "windows_${WindowsVersion}.pkr.hcl"

Write-Host "Successfully build vm image and saved it to $OutputFilePath"
