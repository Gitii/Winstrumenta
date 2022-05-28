function Get-ScriptFolder() {
    $fp = $PSScriptRoot

    if ($fp) {
        return $fp
    }

    return $(split-path -parent $MyInvocation.MyCommand.Definition)
}

function New-TemporaryDirectory {
    $parent = [System.IO.Path]::GetTempPath()
    [string] $name = [System.Guid]::NewGuid()
    [string] $path = Join-Path $parent $name
    New-Item -ItemType Directory -Path $path | Out-Null

    return $path
}

function Extract-Zip($ArchivePath, $TargetDirectory) {
    Add-Type -Assembly "System.IO.Compression.Filesystem"
    $ar = [System.IO.Compression.ZipFile]::OpenRead($tempFile.FullName)

    $ar.Entries | ? { $_.Name -ne "" } | % {
        $entry = $_

        $directory = Split-Path $entry.FullName
        $fullTargetDirectory = Join-Path $TargetDirectory $directory

        New-Item -ItemType Directory $fullTargetDirectory -Force | Out-Null

        $fullTargetFilePath = Join-Path $fullTargetDirectory $entry.Name

        Write-Debug "Extracting $($entry.Name) to $fullTargetFilePath"

        $stream = New-Object -TypeName System.IO.FileStream -ArgumentList @($fullTargetFilePath, [System.IO.FileMode]::OpenOrCreate)

        $entryStream = $entry.Open()

        $entryStream.CopyTo($stream)

        $stream.Close()
        $entryStream.Close()
    }

    $ar.Dispose()

}

function Extract-Config($TargetDirectory) {
    $tempFile = New-TemporaryFile
    $tempFolder = New-TemporaryDirectory

    try {
        $url = 'https://github.com/Gitii/config/archive/refs/heads/main.zip' 

        Write-Host "Downloading..."
        Invoke-WebRequest -Uri $url -OutFile $tempFile.FullName
        
        Write-Host "Extracting to $tempFolder..."
        Extract-Zip -ArchivePath $tempFile.FullName -TargetDirectory $tempFolder

        $templateFolder = Join-Path $tempFolder "config-main" "templates"

        Write-Host "Copying templates to $TargetDirectory"
        Get-ChildItem -Path $templateFolder -Force | Copy-Item -Recurse -Destination $TargetDirectory -Force

    } finally {
        $tempFile.Delete()

        Remove-Item $tempFolder -Recurse -Force
    }
}

$targetDir = Get-Location

Extract-Config -TargetDirectory $targetDir
