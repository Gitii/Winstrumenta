# Disable progress bar to speed up download
$ProgressPreference = 'SilentlyContinue'

function Get-SystemEnv ([string] $Key) {
    return (Get-ItemProperty -Path 'Registry::HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Session Manager\Environment' -Name $Key).$Key
}

function Set-SystemEnv ([string] $Key, [string] $Value) {
    Set-ItemProperty -Path 'Registry::HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Session Manager\Environment' -Name $Key -Value $Value | Out-Null

    New-Item -Name $Key -Value $Value -ItemType Variable -Path Env: -Force | Out-Null
}

function Append-ToPathEnv ([string] $NewPathItem) {
    $oldpath = Get-SystemEnv -Key PATH
    $newpath = "$oldpath;$NewPathItem"
    Set-SystemEnv -Key PATH -Value $newPath
}

$clt = New-TemporaryFile
try {
    $clt.MoveTo([System.IO.Path]::ChangeExtension($clt.FullName, ".zip"))

    Write-Host "Downloading commandline tools..."
    Invoke-WebRequest "https://dl.google.com/android/repository/commandlinetools-win-8512546_latest.zip" -OutFile $clt.FullName

    Write-Host "Extracting tools"

    $androidHome = "C:\Android\Sdk"
    Remove-Item -Path $androidHome -Recurse -Force -ErrorAction SilentlyContinue | Out-Null
    New-Item -Path $androidHome -ItemType "directory" -Force | Out-Null
    New-Item -Path "$androidHome/cmdline-tools/latest" -ItemType "directory" -Force | Out-Null
    Expand-Archive -LiteralPath $clt.FullName -DestinationPath "$androidHome" -Force

    Move-Item -Path "$androidHome/cmdline-tools/source.properties" -Destination "$androidHome/cmdline-tools/latest" -Force | Out-Null
    Move-Item -Path "$androidHome/cmdline-tools/bin" -Destination "$androidHome/cmdline-tools/latest" -Force | Out-Null
    Move-Item -Path "$androidHome/cmdline-tools/lib" -Destination "$androidHome/cmdline-tools/latest" -Force | Out-Null

    Set-SystemEnv -Key "ANDROID_HOME" -Value $androidHome

    Append-ToPathEnv "$androidHome/cmdline-tools/latest/bin;$androidHome/emulator;$androidHome/tools;$androidHome/tools/bin;$androidHome/platform-tools"

    Write-Host "Installing sdk and platform tools..."
    echo "y`ny`ny`ny`ny`ny`ny`ny`ny`ny`ny`ny`ny`ny`ny`n" | sdkmanager --licenses --sdk_root=${ENV:ANDROID_HOME}

    sdkmanager --update
    sdkmanager --install "build-tools;28.0.3" "platform-tools" "platforms;android-28" "system-images;android-30;google_apis;x86_64" "tools"
    echo "y`ny`ny`ny`ny`ny`ny`ny`ny`ny`ny`ny`ny`ny`ny`n" | sdkmanager --licenses --sdk_root=${ENV:ANDROID_HOME}
    
    Write-Host "Creating emulator..."
    echo "no`n" | avdmanager create avd -n Android30 -k "system-images;android-30;google_apis;x86_64"

} finally {
    Write-Host "Cleaning up"
    Remove-Item $clt | Out-Null
}

Write-Host "Successfully installed android sdk"
