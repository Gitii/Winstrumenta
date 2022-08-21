# Disable progress bar to speed up download
$ProgressPreference = 'SilentlyContinue'

try {
    Write-Host "Downloading java 11 jdk..."
    New-Item -Path "C:\Temp" -ItemType "directory" | Out-Null
    Invoke-WebRequest https://api.adoptopenjdk.net/v3/installer/latest/11/ga/windows/x64/jdk/hotspot/normal/adoptopenjdk?project=jdk -OutFile C:\Temp\openjdk11.msi

    Write-Host "Installing java 11 jdk..."
    Start-Process -Wait -FilePath msiexec -ArgumentList /i, "C:\Temp\openjdk11.msi", 'ADDLOCAL="FeatureMain,FeatureEnvironment,FeatureJarFileRunWith,FeatureJavaHome"', 'INSTALLDIR="C:\Program Files\Java\jdk-11\"', /quiet, /norestart -Verb RunAs

} finally {
    Write-Host "Cleaning up"
    Remove-Item "C:\Temp\openjdk11.msi" | Out-Null
}

Write-Host "Successfully installed Java 11 jdk"
