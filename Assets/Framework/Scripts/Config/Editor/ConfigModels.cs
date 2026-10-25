using System.Collections.Generic;
using Newtonsoft.Json;

namespace FrameworkConfig.Editor
{
    internal sealed class ConfigSource
    {
        public string Source = "";
        public string LocalFolder = "Editor/Tables/Source";
        public string FolderId = "";
        public string Namespace = "MyGame";

        // Settings written before local Excel support have no Source field.
        [JsonIgnore]
        public string Kind => string.IsNullOrEmpty(Source)
            ? (string.IsNullOrEmpty(FolderId) ? "LocalExcel" : "GoogleSheets") : Source;

        [JsonIgnore]
        public string Identity => Kind == "LocalExcel" ? "local:" + LocalFolder : "google:" + FolderId;
    }

    internal sealed class SheetSnapshot
    {
        public string WorkbookId = "";
        public string WorkbookName = "";
        public string Version = "";
        public int SheetId;
        public string Name = "";
        public List<List<string>> Rows = new();
        public List<string> ReadErrors = new();

        [JsonIgnore]
        public string Key => WorkbookId + ":" + SheetId;
    }

    internal sealed class ConfigIssue
    {
        public bool Warning;
        public string Table = "";
        public int Row;
        public string Field = "";
        public string Message = "";

        public override string ToString()
        {
            return (Warning ? "Warning: " : "Error: ") + Table
                + (Row > 0 ? " > Row " + Row : "")
                + (Field.Length > 0 ? " > " + Field : "") + "\n" + Message;
        }
    }

    internal sealed class ConfigField
    {
        public string Name;
        public string Type;
    }

    internal sealed class ConfigTableModel
    {
        public SheetSnapshot Source;
        public List<ConfigField> Fields = new();
        public List<Dictionary<string, object>> Data = new();
        public List<int> SourceRows = new();
        public Dictionary<string, string> Addresses = new();
    }

    internal sealed class AppliedTable
    {
        public SheetSnapshot Source;
        public string Hash;
        public int RowCount;
        public Dictionary<string, string> Files = new();
    }

    internal sealed class ConfigManifest
    {
        public const int CurrentVersion = 1;
        public int GeneratorVersion = CurrentVersion;
        public string Namespace;
        public string FolderId;
        public string SourceId;

        [JsonIgnore]
        public string Identity => !string.IsNullOrEmpty(SourceId) ? SourceId
            : (string.IsNullOrEmpty(FolderId) ? "" : "google:" + FolderId);

        public List<AppliedTable> Tables = new();
        public Dictionary<string, string> SharedFiles = new();
    }
}
