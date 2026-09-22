using Brows.Win32.PlatformInvoke;
using System.Runtime.InteropServices;

namespace Brows.Win32.InteropServices.ComTypes;

[Guid(IID.IPropertyStore)]
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPropertyStore {
    [PreserveSig]
    HRESULT GetCount([Out] out uint cProps);

    [PreserveSig]
    HRESULT GetAt([In] uint iProp, [Out] out PROPERTYKEY pkey);

    [PreserveSig]
    HRESULT GetValue([In] ref PROPERTYKEY key, [In, Out] ref PROPVARIANT pv);

    [PreserveSig]
    HRESULT SetValue([In] ref PROPERTYKEY key, [In] ref PROPVARIANT propvar);

    [PreserveSig]
    HRESULT Commit();
}
