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

Install-Script -Force Install-Git
Install-Git.ps1

Append-ToPathEnv "C:\Program Files\Git\usr\bin"
