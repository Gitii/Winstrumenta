using System.Diagnostics;
using System.Net.Sockets;

namespace Winstrumenta.Package.Tests.HostEnvironment;

abstract class BaseHostEnvironment : IHostEnvironment
{
    private string? _tempDisk = null;
    private string _refernceImage = "/media/images/disk-11-21h2.qcow2";
    private string _vmName = "vm-" + (Guid.NewGuid()).ToString().Substring(0, 10);
    private int _port = 10022;
    private int _daemonPid = 0;

    protected abstract Task<(string stdOut, string stdErr)> ExecuteShellAsync(string command);

    protected abstract Task<(string stdOut, string stdErr)> ExecuteAsync(
        string command,
        params string[] arguments
    );

    public async Task StartAsync()
    {
        await StopAsync().ConfigureAwait(false);

        await CreateQemuDiskAsync().ConfigureAwait(false);

        await BootQemuEmulatorAsync().ConfigureAwait(false);
    }

    private async Task BootQemuEmulatorAsync()
    {
        if (string.IsNullOrEmpty(_tempDisk))
        {
            throw new Exception("Create disk first");
        }

        await ExecuteAsync(
                "qemu-system-x86_64",
                "-name",
                _vmName,
                "-machine",
                "type=q35,accel=kvm",
                "-bios",
                "/usr/share/ovmf/OVMF.fd",
                "-cpu",
                "host",
                "-m",
                "7G",
                "-device",
                "virtio-scsi-pci,id=scsi0",
                "-device",
                "scsi-hd,bus=scsi0.0,drive=drive0",
                "-drive",
                $"if=none,file={_tempDisk},id=drive0,cache=unsafe,discard=unmap,format=qcow2",
                "-device",
                "qemu-xhci",
                "-device",
                "virtio-tablet",
                "-vga",
                "qxl",
                "-vnc",
                "0.0.0.0:0",
                "-netdev",
                $"user,id=net0,hostfwd=tcp::{_port}-:22",
                "-device",
                "e1000,netdev=net0",
                "-daemonize",
                "-device",
                "virtio-serial-pci",
                "-chardev",
                "socket,path=/tmp/packer-windows-qga.sock,server,nowait,id=qga0",
                "-device",
                "virtserialport,chardev=qga0,name=org.qemu.guest_agent.0",
                "-daemonize"
            )
            .ConfigureAwait(false);

        await DetermineDaemonPidAsync().ConfigureAwait(false);

        await WaitForVmToBecomeReadyAsync().ConfigureAwait(false);
    }

    private async Task WaitForVmToBecomeReadyAsync()
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();
        try
        {
            var timoutInMs = TimeSpan.FromMinutes(3).TotalMilliseconds;

            while (watch.ElapsedMilliseconds < timoutInMs)
            {
                Console.WriteLine("Checking if vm is ready...");

                if (await IsPortOpenAsync("localhost", _port).ConfigureAwait(false))
                {
                    // vm is ready
                    Console.WriteLine("vm is ready :)");
                    return;
                }

                if (await IsProcessRunningAsync(_daemonPid).ConfigureAwait(false) is false)
                {
                    throw new Exception("Couldn't find running vm. Has it crashed?");
                }

                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }

            throw new Exception("Vm couldn't boot up propably :(");
        }
        finally
        {
            watch.Stop();
        }
    }

    private async Task<bool> IsProcessRunningAsync(int pid)
    {
        try
        {
            await ExecuteAsync("kill", "-0", pid.ToString()).ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task DetermineDaemonPidAsync()
    {
        var (output, _) = await ExecuteShellAsync("ps axo \"pid,args\" | grep qemu | grep vm-test")
            .ConfigureAwait(false);

        if (!output.Contains("qemu-system-x86_64"))
        {
            throw new Exception("Couldn't determine vm daemon process id. Has it crashed?");
        }

        var parts = output.Split(" ", 1);
        if (parts.Length != 1)
        {
            throw new Exception($"Invalid ps output: {output}");
        }

        if (!int.TryParse(parts[0], out _daemonPid))
        {
            throw new Exception($"Invalid pid output: {parts[0]}");
        }
    }

    async Task<bool> IsPortOpenAsync(string host, int port)
    {
        try
        {
            using var client = new TcpClient();
            client.ReceiveTimeout = 1000;
            client.SendTimeout = 1000;
            await client.ConnectAsync(host, port).ConfigureAwait(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task CreateQemuDiskAsync()
    {
        if (!string.IsNullOrEmpty(_tempDisk))
        {
            return;
        }

        (_tempDisk, _) = await ExecuteAsync("mktemp", ".qcow2").ConfigureAwait(false);

        await ExecuteAsync("qemu-img", "create", "-f", "qcow2", "-b", _refernceImage, _tempDisk)
            .ConfigureAwait(false);
    }

    public async Task StopAsync()
    {
        if (!string.IsNullOrEmpty(_tempDisk))
        {
            await ExecuteAsync("rm", "-f", _tempDisk).ConfigureAwait(false);
            _tempDisk = null;
        }

        if (_daemonPid > 0)
        {
            if (await IsProcessRunningAsync(_daemonPid).ConfigureAwait(false))
            {
                await KillProcessAsync(_daemonPid).ConfigureAwait(false);
            }
        }
    }

    private Task KillProcessAsync(int pid)
    {
        return ExecuteAsync("kill", pid.ToString());
    }

    public async Task ResetAsync()
    {
        await StopAsync().ConfigureAwait(false);
        await StartAsync().ConfigureAwait(false);
    }
}
