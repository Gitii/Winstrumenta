<p align="center">
  <a href="https://github.com/Gitii/Winstrumenta">
    <img src="Assets/LogoWithClosedPackage.png" alt="Logo" width="256px">
  </a>

<h3 align="center">Winstrumenta<br/>A collection of tools for WSL & WSA 🥳</h3>

<p align="center">
    The `Winstrumenta` toolset focuses on Windows Subsystem for Linux (WSL) and Android (WSA). 
    <br />
    <a href="https://github.com/Gitii/Winstrumenta"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    <a href="https://github.com/Gitii/Winstrumenta/issues">Report Bug</a> ·
    <a href="https://github.com/Gitii/Winstrumenta/issues">Request Feature</a>
  </p>
</p>

---

### Winstrumenta - A collection of tools for WSL & WSA

The `Winstrumenta` toolset focuses on Windows Subsystem for Linux (WSL) and Android (WSA). The goal is to make the life of developers and users easier by providing quality of life tools for every day use cases.

The main tool right now is `Package Manager`.

# Package Manager

<p align="center">
  <img src="Assets/Package-Screenshot.png" alt="PM-Logo" width="600">
</p>

`Package Manager` is graphical use interface for managing `deb`, `rpm` and `apk` packages. It integrates into seamlessy in the windows explorer.

- `deb`, `rpm` and `apk` (WSA) packages are supported

- Easy selection of target distribution (if there are multiple ones)

- Uses native package managers of the selected distribution for compability

- Easy up- or downgrade of packages

- Launch installed packages or uninstall them

## Getting started with `Package Manager`

Package Manager is distributed through the `Microsoft Store`:  
<a href="https://www.microsoft.com/store/apps/9N9MX3J3F4G0">
<img title="Download from Microsoft-Store" src="Assets/ms-store-badge.png" alt="Download from Microsoft-Store" width="100">
</a>

Please note that `WSL 2` needs to be activated and running. Managing apps in WSL 1 should work but hasn't been tested.

---

## Roadmap

- Support dependencies: Currently `dpkg` and `rpm` are used to install packages. Both doesn't support dependency resolution.

- Support more package formats:

  - Alpine packages (`apk`)

  - Android packages (`apk`) for physical devices and emulators

  - Arch packages (`ar`)

- Advanced package management:

  - Extract/View instead of install or uninstall

---

# ⚠️ License

Winstrumenta is free and open-source software licensed under MIT.

# 🔒 Data Privacy

## Information collected

All apps in the by Winstrumenta toolset do not collect any personal identiying information.

## Use of information

No information is collected or shared over the wire. We reserve the right to
make changes to this policy. Any changes to this policy will be updated.

<br />
