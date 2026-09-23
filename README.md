# Brows.Win32.PropertySystem

Win32 property system for .NET.

A small, dependency-free wrapper over the [Windows Property System](https://learn.microsoft.com/en-us/windows/win32/properties/windows-properties-system)
that lets you read and write file metadata — EXIF data, media tags, document properties, and
anything else exposed by a registered property handler.

## Install

```shell
dotnet add package Brows.Win32.PropertySystem
```

Targets `net462`, `net48`, `net8.0-windows`, and `net10.0-windows`.

Windows only. The library talks to `propsys.dll`, `ole32.dll`, `oleaut32.dll`, and `shell32.dll`
through COM interop, so it has no package dependencies of its own.

## Usage

Everything lives in the `Brows.Win32` namespace.

### Read a single property

```csharp
using Brows.Win32;

var dateTaken = PropertySystem.GetPropertyDescription("System.Photo.DateTaken");
var value = PropertySystem.GetPropertyValue(@"C:\photos\example.jpg", dateTaken, throwOnError: true);

Console.WriteLine(value.Display);  // "8/14/2025 5:03 AM" — formatted for the current locale
Console.WriteLine(value.Object);   // 8/14/2025 10:03:00 AM — a DateTime, in UTC
```

`Display` is the value formatted by the property's own rules; `Object` is the raw managed value.
The two can differ in more than formatting — date properties are stored in UTC but displayed in
local time, as above.

A property the file has never had set still returns a `PropertyValue`, with an empty `Display`
and a `null` `Object`.

### Read everything a file carries

```csharp
var descriptions = PropertySystem.EnumeratePropertyDescriptions(fileName, throwOnError: false);
foreach (var value in PropertySystem.EnumeratePropertyValues(fileName, descriptions, throwOnError: false)) {
    Console.WriteLine($"{value.Description.DisplayName}: {value.Display}");
}
```

Use `EnumeratePropertyValues` rather than repeated `GetPropertyValue` calls when you want several
properties from the same file — it opens the property store once.

### Write properties

```csharp
var title = PropertySystem.GetPropertyDescription("System.Title");
PropertySystem.SetPropertyValue(fileName, title, "Sunset over the harbour");
```

Or write several at once, committed as a single change:

```csharp
PropertySystem.SetPropertyValues(fileName, new[] {
    new KeyValuePair<PropertyDescription, object>(title, "Sunset over the harbour"),
    new KeyValuePair<PropertyDescription, object>(keywords, new[] { "sunset", "harbour" }),
}, ignoreError: null);
```

### Copy metadata between files

```csharp
PropertySystem.TransferPropertyValues(
    sourceFileName,
    destinationFileName,
    ignoreProperty: null,
    ignoreError: (property, hr) => true);
```

`TransferPropertyValues` copies *every* property it can read, including read-only and calculated
ones such as `System.Size`, so you will normally want an `ignoreError` callback to skip the ones
that cannot be written back.

### Match property names

```csharp
var wildcard = new PropertyWildcard("System.Photo.*");

wildcard.Matches("System.Photo.DateTaken");  // true
wildcard.Matches("System.Music.Artist");     // false
```

A `*` segment matches that segment and every segment after it. Without a `*`, the pattern is an
exact match — `System.Photo` does **not** match `System.Photo.DateTaken`. Comparison is
case-insensitive.

### Resolve legacy property names

```csharp
PropertyNameMap.GetCanonicalName("WhenTaken");   // "System.Photo.DateTaken"
PropertyNameMap.GetCanonicalNames("Copyright");  // ["System.Copyright", "System.Image.Copyright"]
```

Legacy names are matched case-insensitively, as Windows matches them. Unknown names return `null`;
pass `throwIfNotFound: true` to get an `ArgumentException` instead. This is a lookup for legacy
names only — a canonical name passed in returns `null`, since it needs no translation.

## API

| Type | Purpose |
| --- | --- |
| `PropertySystem` | Static entry point for reading and writing file metadata. |
| `PropertyDescription` | A property in the schema: its `CanonicalName` and `DisplayName`. |
| `PropertyValue` | A value read from a file: its `Description`, `Display`, and `Object`. |
| `PropertyWildcard` | Matches property names against a dot-separated pattern. |
| `PropertyNameMap` | Maps legacy property names onto the canonical names that replaced them. |

## Things to know

**Canonical names are case-sensitive.** `System.Photo.DateTaken` resolves; `system.photo.datetaken`
does not. `GetPropertyDescription` throws a `COMException` with `TYPE_E_ELEMENTNOTFOUND`
(`0x8002802B`) for a name that is not registered. Legacy names, by contrast, *are* matched
case-insensitively, both by Windows and by `PropertyNameMap`.

**Not every file type is writable.** Writing goes through the property handler registered for the
file's extension. Plain `.txt` files, for example, have no writable handler and fail with `E_FAIL`
(`0x80004005`). JPEG, MP3, and Office documents work. Read-only and calculated properties such as
`System.Size` fail with `WINCODEC_ERR_PROPERTYNOTSUPPORTED` (`0x88982F41`) regardless of file type.

**The enumeration methods are lazy.** `EnumeratePropertyDescriptions` and `EnumeratePropertyValues`
are iterators, so argument validation and I/O failures surface when you start enumerating, not when
you call the method. The `throwOnError` parameter controls whether a file that cannot be opened
throws or simply yields nothing.

**Legacy names are ambiguous in places.** Microsoft's published legacy-name table lists a few names
against more than one canonical name — `Copyright` and `Status` among them — so `PropertyNameMap`
maps each legacy name to a *list*. `GetCanonicalName` returns the first entry, which is the one
Windows' own alias resolution picks. The table also includes ten names, including the whole
`System.Fax.*` set, that are no longer registered on current versions of Windows; they are retained
for fidelity and marked in the source.

## Building

```shell
dotnet build
dotnet test
```

Requires the .NET 10 SDK (see `global.json`) and Windows. The test suite exercises the real property
system against real files, creating and deleting its own temporary directory as it runs.

## License

[MIT](LICENSE)
