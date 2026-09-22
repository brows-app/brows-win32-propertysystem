using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Brows.Win32;

/// <summary>
/// Maps the legacy property names Windows still accepts, such as <c>WhenTaken</c>, onto the
/// canonical names that replaced them, such as <c>System.Photo.DateTaken</c>.
/// </summary>
public static class PropertyNameMap {
    /// <summary>
    /// Gets every canonical name associated with <paramref name="propertyName"/>.
    /// </summary>
    /// <param name="propertyName">
    /// A legacy property name, or a canonical name (which maps to itself).
    /// </param>
    /// <returns>
    /// The associated canonical names, or <see langword="null"/> if <paramref name="propertyName"/>
    /// is neither a known legacy name nor a known canonical name. A handful of legacy names are
    /// documented against more than one canonical name, so the result can contain multiple entries.
    /// </returns>
    public static IReadOnlyList<string> GetCanonicalNames(string propertyName) {
        if (LegacyNameMap.TryGetValue(propertyName, out var value)) {
            return value;
        }
        if (CanonicalNameSet.Contains(propertyName)) {
            return [propertyName];
        }
        return null;
    }

    /// <summary>
    /// Gets the first canonical name associated with <paramref name="propertyName"/>.
    /// </summary>
    /// <param name="propertyName">
    /// A legacy property name, or a canonical name (which maps to itself).
    /// </param>
    /// <returns>
    /// The first associated canonical name, or <see langword="null"/> if <paramref name="propertyName"/>
    /// is neither a known legacy name nor a known canonical name. Use <see cref="GetCanonicalNames"/>
    /// for legacy names that are documented against more than one canonical name.
    /// </returns>
    public static string GetCanonicalName(string propertyName) {
        return GetCanonicalNames(propertyName)?[0];
    }

    private static readonly HashSet<string> CanonicalNameSet;

    private sealed class Map : IReadOnlyDictionary<string, IReadOnlyList<string>> {
        private readonly Dictionary<string, List<string>> Data = [];

        public void Add(string legacyName, string canonicalName) {
            if (Data.TryGetValue(legacyName, out var canonicalNames) == false) {
                Data[legacyName] = canonicalNames = [];
            }
            if (canonicalNames.Contains(canonicalName) == false) {
                canonicalNames.Add(canonicalName);
            }
        }

        public IReadOnlyList<string> this[string key] => Data[key];
        public IEnumerable<string> Keys => Data.Keys;
        public IEnumerable<IReadOnlyList<string>> Values => Data.Values;
        public int Count => Data.Count;
        public bool ContainsKey(string key) => Data.ContainsKey(key);

        public bool TryGetValue(string key, out IReadOnlyList<string> value) {
            var got = Data.TryGetValue(key, out var list);
            value = list;
            return got;
        }

        public IEnumerator<KeyValuePair<string, IReadOnlyList<string>>> GetEnumerator() {
            foreach (var item in Data) {
                yield return new KeyValuePair<string, IReadOnlyList<string>>(item.Key, item.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    // Transcribed verbatim from the legacy-name table published in the Remarks of
    // PSGetPropertyDescriptionByName:
    // https://learn.microsoft.com/en-us/windows/win32/api/propsys/nf-propsys-psgetpropertydescriptionbyname#remarks
    //
    // Ten of the canonical names below are documented by Microsoft but are no longer
    // registered in the property-schema cache on current versions of Windows, and have no
    // corresponding PKEY_ in the Windows SDK propkey.h. They are marked "obsolete" inline.
    // Resolving any of them (for example via PSGetPropertyDescriptionByName) fails with
    // TYPE_E_ELEMENTNOTFOUND (0x8002802B), so callers must handle that result rather than
    // assume every name returned by GetCanonicalName is resolvable.
    //
    // The table also lists several legacy names more than once. Those duplicates are not a
    // transcription error, so each legacy name maps to a list of canonical names rather than to a
    // single one. Where a duplicate is ambiguous, Windows' own alias resolution picks
    // "System.Copyright" for "Copyright" and "System.Media.Status" for "Status", which is the
    // order the rows appear in below.
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> LegacyNameMap = new Map {
        { "Access", "System.DateAccessed" },
        { "Album", "System.Music.AlbumTitle" },
        { "AllocSize", "System.FileAllocationSize" },
        { "Aperture", "System.Photo.Aperture" },
        { "Artist", "System.Music.Artist" },
        { "Attrib", "System.FileAttributes" },
        { "Attributes", "System.FileAttributes" },
        { "AttributesDescription", "System.FileAttributesDisplay" },
        { "Audio Format", "System.Audio.Format" },
        { "Audio Sample Size", "System.Audio.SampleSize" },
        { "BitDepth", "System.Image.BitDepth" },
        { "Bitrate", "System.Audio.EncodingBitrate" },
        { "CameraModel", "System.Photo.CameraModel" },
        { "Capacity", "System.Capacity" },
        { "Channels", "System.Audio.ChannelCount" },
        { "ColorSpace", "System.Image.ColorSpace" },
        { "Company", "System.Company" },
        { "Compression", "System.Video.Compression" },
        { "Compression", "System.Video.Compression" },
        { "Copyright", "System.Copyright" },
        { "Copyright", "System.Copyright" },
        { "Copyright", "System.Image.Copyright" },
        { "Create", "System.DateCreated" },
        { "CSCStatus", "System.OfflineStatus" },
        { "Data Rate", "System.Video.EncodingBitrate" },
        { "DateDeleted", "System.Recycle.DateDeleted" },
        { "DeletedFrom", "System.Recycle.DeletedFrom" },
        { "Dimensions", "System.Image.Dimensions" },
        { "Directory", "System.ItemFolderNameDisplay" },
        { "Distance", "System.Photo.SubjectDistance" },
        { "DocAppName", "System.ApplicationName" },
        { "DocAuthor", "System.Author" },
        { "DocByteCount", "System.Document.ByteCount" },
        { "DocCategory", "System.Category" },
        { "DocCharCount", "System.Document.CharacterCount" },
        { "DocComments", "System.Comment" },
        { "DocCompany", "System.Company" },
        { "DocCreatedTm", "System.Document.DateCreated" },
        { "DocEditTime", "System.Document.TotalEditingTime" },
        { "DocHiddenCount", "System.Document.HiddenSlideCount" },
        { "DocKeywords", "System.Keywords" },
        { "DocLastAuthor", "System.Document.LastAuthor" },
        { "DocLastPrinted", "System.Document.DatePrinted" },
        { "DocLastSavedTm", "System.Document.DateSaved" },
        { "DocLineCount", "System.Document.LineCount" },
        { "DocManager", "System.Document.Manager" },
        { "DocNoteCount", "System.Document.NoteCount" },
        { "DocPageCount", "System.Document.PageCount" },
        { "DocParaCount", "System.Document.ParagraphCount" },
        { "DocPresentationTarget", "System.Document.PresentationFormat" },
        { "DocRevNumber", "System.Document.RevisionNumber" },
        { "DocSlideCount", "System.Document.SlideCount" },
        { "DocSubject", "System.Subject" },
        { "DocTemplate", "System.Document.Template" },
        { "DocTitle", "System.Title" },
        { "DocWordCount", "System.Document.WordCount" },
        { "DRM Description", "System.DRM.Description" },
        { "Duration", "System.Media.Duration" },
        { "EquipMake", "System.Photo.CameraManufacturer" },
        { "ExposureBias", "System.Photo.ExposureBias" },
        { "ExposureProg", "System.Photo.ExposureProgram" },
        { "ExposureTime", "System.Photo.ExposureTime" },
        { "FaxCallerID", "System.Fax.CallerID" },               // obsolete: no System.Fax property set
        { "FaxCSID", "System.Fax.CSID" },                       // obsolete: no System.Fax property set
        { "FaxRecipientName", "System.Fax.RecipientName" },     // obsolete: no System.Fax property set
        { "FaxRecipientNumber", "System.Fax.RecipientNumber" }, // obsolete: no System.Fax property set
        { "FaxRouting", "System.Fax.Routing" },                 // obsolete: no System.Fax property set
        { "FaxSenderName", "System.Fax.SenderName" },           // obsolete: no System.Fax property set
        { "FaxTime", "System.Fax.Time" },                       // obsolete: no System.Fax property set
        { "FaxTSID", "System.Fax.TSID" },                       // obsolete: no System.Fax property set
        { "FileDescription", "System.FileDescription" },
        { "FileSystem", "System.Volume.FileSystem" },
        { "FileType", "System.Image.FileType" },                // obsolete: unregistered; closest current equivalent is System.ItemTypeText
        { "FileVersion", "System.FileVersion" },
        { "Flash", "System.Photo.Flash" },
        { "FlashEnergy", "System.Photo.FlashEnergy" },
        { "FNumber", "System.Photo.FNumber" },
        { "FocalLength", "System.Photo.FocalLength" },
        { "Frame Rate", "System.Video.FrameRate" },
        { "FrameCount", "System.Media.FrameCount" },
        { "FreeSpace", "System.FreeSpace" },
        { "Genre", "System.Music.Genre" },
        { "ImageX", "System.Image.HorizontalSize" },
        { "ImageY", "System.Image.VerticalSize" },
        { "ISOSpeed", "System.Photo.ISOSpeed" },
        { "LightSource", "System.Photo.LightSource" },
        { "LinksUpToDate", "System.Document.LinksDirty" },
        { "LinkTarget", "System.Link.TargetParsingPath" },
        { "Lyrics", "System.Music.Lyrics" },
        { "Manager", "System.Document.Manager" },
        { "MeteringMode", "System.Photo.MeteringMode" },
        { "MMClipCount", "System.Document.MultimediaClipCount" },
        { "Name", "System.ItemNameDisplay" },
        { "Owner", "System.FileOwner" },
        { "Play Count", "System.DRM.PlayCount" },
        { "Play Expires", "System.DRM.DatePlayExpires" },
        { "Play Starts", "System.DRM.DatePlayStarts" },
        { "PresentationTarget", "System.Document.PresentationFormat" },
        { "ProductName", "System.Software.ProductName" },
        { "ProductVersion", "System.Software.ProductVersion" },
        { "Project", "System.Media.Project" },                  // obsolete: unregistered; propkey.h defines PKEY_Project as System.Project
        { "Protected", "System.DRM.IsProtected" },
        { "Rank", "System.Search.Rank" },
        { "Rating", "System.Rating" },
        { "ResolutionX", "System.Image.HorizontalResolution" },
        { "ResolutionY", "System.Image.VerticalResolution" },
        { "Sample Rate", "System.Audio.SampleRate" },
        { "Scale", "System.Document.Scale" },
        { "ShutterSpeed", "System.Photo.ShutterSpeed" },
        { "Size", "System.Size" },
        { "Software", "System.SoftwareUsed" },
        { "Status", "System.Media.Status" },
        { "Status", "System.Status" },
        { "Stream Name", "System.Video.StreamName" },
        { "SyncCopyIn", "System.Sync.CopyIn" },
        { "Track", "System.Music.TrackNumber" },
        { "Type", "System.ItemTypeText" },
        { "Video Sample Size", "System.Video.SampleSize" },
        { "WhenTaken", "System.Photo.DateTaken" },
        { "Write", "System.DateModified" },
        { "Year", "System.Media.Year" },
    };

    static PropertyNameMap() {
        CanonicalNameSet = [.. LegacyNameMap.Values.SelectMany(names => names)];
    }
}
