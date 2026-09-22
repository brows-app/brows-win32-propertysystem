using Brows.Win32.PlatformInvoke;
using System.Runtime.InteropServices;

namespace Brows.Win32.InteropServices.ComTypes;

[Guid(IID.IPropertyDescriptionList)]
[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPropertyDescriptionList {
    [PreserveSig]
    HRESULT GetCount([Out] out uint pcElem);

    [PreserveSig]
    HRESULT GetAt(
        [In] uint iElem,
        [In] ref Guid riid,
        [Out, MarshalAs(UnmanagedType.Interface, IidParameterIndex = 1)] out IPropertyDescription ppv);
};
