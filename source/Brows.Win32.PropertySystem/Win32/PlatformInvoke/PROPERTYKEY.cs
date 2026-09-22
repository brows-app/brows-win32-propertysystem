using System.Runtime.InteropServices;

namespace Brows.Win32.PlatformInvoke;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct PROPERTYKEY {
    public Guid fmtid;
    public uint pid;
}
