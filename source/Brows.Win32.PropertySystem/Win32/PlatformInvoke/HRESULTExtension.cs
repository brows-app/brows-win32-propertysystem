using System.Runtime.InteropServices;

namespace Brows.Win32.PlatformInvoke;

internal static class HRESULTExtension {
    public static void ThrowOnError(this HRESULT hresult) {
        var hr = (uint)hresult;
        switch (hr) {
            case 0:
                break;
            case 0x80270000:
                //if (Log.Info()) {
                //    Log.Info($"User canceled (HRESULT {hresult})");
                //}
                break;
            default:
                Marshal.ThrowExceptionForHR(unchecked((int)hresult));
                break;
        }
    }
}
