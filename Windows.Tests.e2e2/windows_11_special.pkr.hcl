
variable "cpus" {
  type    = string
  default = "2"
}

variable "disk_size" {
  type    = string
  default = "262144"
}

variable "headless" {
  type    = string
  default = "false"
}

variable "iso_checksum" {
  type    = string
  default = "none"
}

variable "iso_url" {
  type    = string
  default = "./iso/Windows_11.iso"
}

variable "memory" {
  type    = string
  default = "4096"
}

variable "switch_name" {
  type    = string
  default = "Default Switch"
}

variable "vm_name" {
  type    = string
  default = "windows_11"
}

variable "output_directory" {
  type = string
}

source "hyperv-iso" "win11" {
  boot_command                     = ["a<wait>a<wait>a"]
  boot_wait                        = "-1s"
  cd_files                         = ["./answer_files/11_efi_secure/Autounattend.xml", "./scripts/fixnetwork.ps1", "./scripts/disable-screensaver.ps1", "./scripts/disable-winrm.ps1", "./scripts/enable-winrm.ps1", "./scripts/microsoft-updates.bat", "./scripts/win-updates.ps1"]
  communicator                     = "winrm"
  configuration_version            = "10.0"
  cpus                             = "${var.cpus}"
  disk_size                        = "${var.disk_size}"
  enable_dynamic_memory            = false
  enable_mac_spoofing              = true
  enable_secure_boot               = true
  enable_virtualization_extensions = true
  generation                       = "2"
  guest_additions_mode             = "disable"
  iso_checksum                     = "${var.iso_checksum}"
  iso_url                          = "${var.iso_url}"
  memory                           = "${var.memory}"
  shutdown_command                 = "shutdown /s /t 10 /f /d p:4:1 /c \"Packer Shutdown\""
  switch_name                      = "${var.switch_name}"
  vm_name                          = "${var.vm_name}"
  winrm_password                   = "vagrant"
  winrm_timeout                    = "6h"
  winrm_username                   = "vagrant"
  output_directory                 = "${output_directory}"
}

build {
  sources = ["source.hyperv-iso.win11"]

  provisioner "windows-shell" {
    execute_command = "{{ .Vars }} cmd /c \"{{ .Path }}\""
    remote_path     = "/tmp/script.bat"
    scripts         = ["./scripts/enable-rdp.bat"]
  }

  provisioner "powershell" {
    scripts = ["./scripts/vm-guest-tools.ps1"]
  }

  provisioner "windows-restart" {
    restart_timeout = "5m"
  }

  provisioner "windows-shell" {
    execute_command = "{{ .Vars }} cmd /c \"{{ .Path }}\""
    remote_path     = "/tmp/script.bat"
    scripts         = ["./scripts/set-winrm-automatic.bat"]
  }

  provisioner "powershell" {
    scripts = ["./scripts/install-java11.ps1", "./scripts/install-android-sdk.ps1"]
  }
}
