using System.Runtime.InteropServices;
using WinShield.Domain;

namespace WinShield.Platform.Windows;

public sealed class WindowsAmsiService : IAmsiService
{
    public Task<AmsiScanResult> ScanContentAsync(string contentName, string content, CancellationToken cancellationToken = default)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Task.FromResult(new AmsiScanResult(false, false, "AMSI requires Windows.", ThreatSeverity.Info));
        }

        var hr = AmsiInitialize("WinShield", out var context);
        if (hr != 0 || context == IntPtr.Zero)
        {
            return Task.FromResult(new AmsiScanResult(false, false, $"AMSI initialization failed: 0x{hr:X}", ThreatSeverity.Info));
        }

        try
        {
            var bytes = System.Text.Encoding.Unicode.GetBytes(content);
            hr = AmsiScanBuffer(context, bytes, (uint)bytes.Length, contentName, IntPtr.Zero, out var result);
            if (hr != 0)
            {
                return Task.FromResult(new AmsiScanResult(true, false, $"AMSI scan failed: 0x{hr:X}", ThreatSeverity.Info));
            }

            var detected = result >= 32768;
            return Task.FromResult(new AmsiScanResult(true, detected, $"AMSI result code {result}.", detected ? ThreatSeverity.High : ThreatSeverity.Info));
        }
        finally
        {
            AmsiUninitialize(context);
        }
    }

    [DllImport("amsi.dll", CharSet = CharSet.Unicode)]
    private static extern int AmsiInitialize(string appName, out IntPtr amsiContext);

    [DllImport("amsi.dll")]
    private static extern void AmsiUninitialize(IntPtr amsiContext);

    [DllImport("amsi.dll", CharSet = CharSet.Unicode)]
    private static extern int AmsiScanBuffer(IntPtr amsiContext, byte[] buffer, uint length, string contentName, IntPtr session, out int result);
}
