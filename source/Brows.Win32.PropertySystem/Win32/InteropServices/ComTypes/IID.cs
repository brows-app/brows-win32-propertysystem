namespace Brows.Win32.InteropServices.ComTypes;

internal static class IID {
    public const string IShellItem = "43826D1E-E718-42EE-BC55-A1E261C37BFE";
    public const string IPropertyDescription = "6F79D558-3E96-4549-A1D1-7D75D2288814";
    public const string IPropertyDescriptionList = "1f9fc1d0-c39b-4b26-817f-011967d3440e";
    public const string IPropertyStore = "886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99";
    public const string IPropertySystem = "ca724e8a-c3e6-442b-88a4-6fb0db8035a3";

    public static class Managed {
        public static readonly Guid IPropertyDescription = new(IID.IPropertyDescription);
        public static readonly Guid IPropertyDescriptionList = new(IID.IPropertyDescriptionList);
        public static readonly Guid IPropertyStore = new(IID.IPropertyStore);
        public static readonly Guid IPropertySystem = new(IID.IPropertySystem);
    }
}
