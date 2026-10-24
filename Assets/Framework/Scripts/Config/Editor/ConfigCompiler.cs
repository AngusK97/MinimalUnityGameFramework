using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace FrameworkConfig.Editor
{
    internal static partial class ConfigCompiler
    {
        private static readonly Regex Identifier = new Regex("\\A[A-Za-z_][A-Za-z0-9_]*\\z");
        private static readonly HashSet<string> ReservedFileNames = new HashSet<string>(
            "CON PRN AUX NUL COM1 COM2 COM3 COM4 COM5 COM6 COM7 COM8 COM9 LPT1 LPT2 LPT3 LPT4 LPT5 LPT6 LPT7 LPT8 LPT9".Split(' '),
            StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> Keywords = new HashSet<string>(
            ("abstract as base bool break byte case catch char checked class const continue decimal " +
             "default delegate do double else enum event explicit extern false finally fixed float for " +
             "foreach goto if implicit in int interface internal is lock long namespace new null object " +
             "operator out override params private protected public readonly ref return sbyte sealed short " +
             "sizeof stackalloc static string struct switch this throw true try typeof uint ulong " +
             "unchecked unsafe ushort using virtual void volatile while add alias and ascending async " +
             "await by descending dynamic equals file from get global group init into join let managed " +
             "nameof nint not notnull nuint on or orderby partial record remove required scoped select " +
             "set unmanaged value var when where with yield __arglist __makeref __reftype __refvalue").Split(' '),
            StringComparer.Ordinal);
        private static readonly HashSet<string> ObjectMembers = new HashSet<string>(
            "GetType ToString Equals GetHashCode ReferenceEquals MemberwiseClone Finalize".Split(' '),
            StringComparer.Ordinal);
        private static readonly HashSet<string> ModuleMembers = new HashSet<string>(
            ("Id GetName RecordAllResourceInfos GetResource AddResourceInfo LoadResources UnloadResources " +
             "Shutdown GetType ToString Equals GetHashCode ReferenceEquals MemberwiseClone Finalize").Split(' '),
            StringComparer.Ordinal);
        private static readonly Dictionary<string, string[]> SpecialFields = new Dictionary<string, string[]>
        {
            { "UI", new[] { "Id", "Name", "PrefabPath" } },
            { "Resource", new[] { "Id", "Name", "Type", "Module", "AssetPath" } },
            { "Scene", new[] { "Id", "Name", "ScenePath" } }
        };
        private static readonly Regex SourceTrivia = new Regex(
            "//[^\\r\\n]*|/\\*[\\s\\S]*?\\*/|@\"(?:\"\"|[^\"])*\"|\"(?:\\\\.|[^\"\\\\])*\"|'(?:\\\\.|[^'\\\\])*'");
        private static readonly Regex SourceTokens = new Regex("[A-Za-z_][A-Za-z0-9_]*|[{}.;(<]");

        public static bool ValidIdentifier(string value)
        {
            return !string.IsNullOrEmpty(value) && value.Length <= 80 && Identifier.IsMatch(value)
                && !value.Contains("__") && !Keywords.Contains(value) && !ReservedFileNames.Contains(value);
        }

        public static bool ValidNamespace(string value)
        {
            return !string.IsNullOrEmpty(value) && value.Length <= 160 && value.Split('.').All(ValidIdentifier);
        }

        public static ConfigTableModel Parse(SheetSnapshot source, List<ConfigIssue> issues)
        {
            var start = issues.Count;
            var model = new ConfigTableModel { Source = source };

            if (source == null)
            {
                AddIssue(model, issues, 0, "", "The sheet could not be read. Update config again.");
                return null;
            }

            foreach (var error in source.ReadErrors ?? new List<string>())
            {
                AddIssue(model, issues, 0, "", error);
            }

            if (!ValidIdentifier(source.Name))
            {
                AddIssue(model, issues, 0, "", "Table name must be an ASCII C# identifier, at most 80 characters, without keywords or double underscores. Rename the sheet.");
            }

            if (source.Rows == null || source.Rows.Count < 2)
            {
                AddIssue(model, issues, 1, "", "Add field names in row 1 and field types in row 2.");
                return null;
            }

            var headers = source.Rows[0] ?? new List<string>();
            var types = source.Rows[1] ?? new List<string>();
            var count = headers.Count;

            while (count > 0 && string.IsNullOrEmpty(headers[count - 1]))
            {
                count--;
            }

            if (count == 0 || headers[0] != "Id")
            {
                AddIssue(model, issues, 1, "Id", "The first field must be Id. Add an Id column first.");
            }

            var fieldNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var column = 0; column < count; column++)
            {
                var name = Cell(headers, column);
                var type = Cell(types, column);

                if (!ValidIdentifier(name) || ObjectMembers.Contains(name) || name == source.Name + "Config")
                {
                    AddIssue(model, issues, 1, "Column " + (column + 1), "Field '" + name + "' is not a usable C# property name. Use a short ASCII identifier that does not match a keyword or generated member.");
                }
                else if (!fieldNames.Add(name))
                {
                    AddIssue(model, issues, 1, name, "This field name is already used. Use unique names, including letter case.");
                }

                if (type != "string" && type != "int" && type != "float" && type != "bool"
                    && !(type.StartsWith("ref:", StringComparison.Ordinal) && ValidIdentifier(type.Substring(4))))
                {
                    AddIssue(model, issues, 2, name, "Unknown type '" + type + "'. Use string, int, float, bool, or ref:TableName.");
                }

                model.Fields.Add(new ConfigField { Name = name, Type = type });
            }

            CheckExtraCells(model, issues, types, count, 2);

            if (model.Fields.Count > 0 && model.Fields[0].Type != "string" && model.Fields[0].Type != "int")
            {
                AddIssue(model, issues, 2, "Id", "Id must use string or int.");
            }

            if (SpecialFields.TryGetValue(source.Name, out var required))
            {
                foreach (var field in model.Fields)
                {
                    if (!required.Contains(field.Name))
                    {
                        AddIssue(model, issues, 1, field.Name, "Unknown field. " + source.Name + " uses only " + string.Join(", ", required) + ".");
                    }

                    if (field.Type != "string")
                    {
                        AddIssue(model, issues, 2, field.Name, "This table uses string for every field.");
                    }
                }

                foreach (var name in required)
                {
                    if (!model.Fields.Any(field => field.Name == name))
                    {
                        AddIssue(model, issues, 1, name, "Required field is missing. Add the " + name + " column with type string.");
                    }
                }
            }

            if (HasErrors(issues, start))
            {
                return null;
            }

            var ids = new Dictionary<object, int>();

            for (var index = 2; index < source.Rows.Count; index++)
            {
                var cells = source.Rows[index] ?? new List<string>();

                if (cells.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                var rowNumber = index + 1;
                var values = new Dictionary<string, object>(StringComparer.Ordinal);
                CheckExtraCells(model, issues, cells, count, rowNumber);

                for (var column = 0; column < count; column++)
                {
                    var field = model.Fields[column];
                    var raw = Cell(cells, column);

                    if ((field.Name == "Id" || field.Type != "string") && string.IsNullOrWhiteSpace(raw))
                    {
                        AddIssue(model, issues, rowNumber, field.Name, "Required value is missing. Enter a " + field.Name + ".");
                        continue;
                    }

                    if (!TryValue(raw, field.Type, out var value))
                    {
                        AddIssue(model, issues, rowNumber, field.Name, "Expected " + TypeExample(field.Type) + ". Found '" + raw + "'.");
                        continue;
                    }

                    values[field.Name] = value;
                }

                if (values.TryGetValue("Id", out var id))
                {
                    if (ids.TryGetValue(id, out var originalRow))
                    {
                        AddIssue(model, issues, rowNumber, "Id", "Id '" + id + "' is already used in row " + originalRow + ". Use a unique Id.");
                    }
                    else
                    {
                        ids.Add(id, rowNumber);
                    }
                }

                if (SpecialFields.ContainsKey(source.Name))
                {
                    ValidateSpecialRow(model, issues, values, rowNumber);
                }

                model.Data.Add(values);
                model.SourceRows.Add(rowNumber);
            }

            return HasErrors(issues, start) ? null : model;
        }

        private static void ValidateSpecialRow(ConfigTableModel model, List<ConfigIssue> issues,
            Dictionary<string, object> values, int row)
        {
            var table = model.Source.Name;

            if (values.TryGetValue("Id", out var id) && !ValidIdentifier((string)id))
            {
                AddIssue(model, issues, row, "Id", "Id must be a short ASCII C# identifier, without keywords or double underscores.");
            }

            if (table == "UI" || table == "Scene")
            {
                if (id is string name && (name == table + "Name" || ObjectMembers.Contains(name)))
                {
                    AddIssue(model, issues, row, "Id", "Id conflicts with a generated member. Choose a different Id.");
                }

                if (table == "Scene" && string.IsNullOrWhiteSpace((string)values["ScenePath"]))
                {
                    AddIssue(model, issues, row, "ScenePath", "Required path is missing. Enter the saved scene's Assets/... path.");
                }

                return;
            }

            var module = (string)values["Module"];

            if (!ValidIdentifier(module))
            {
                AddIssue(model, issues, row, "Module", "Module must be a short ASCII C# identifier, for example Main.");
            }

            if (id is string member && (ModuleMembers.Contains(member) || member == module + "Resources"))
            {
                AddIssue(model, issues, row, "Id", "Id conflicts with a resource module member. Choose a different Id.");
            }

            var type = (string)values["Type"];

            if (type != "AudioClip" && type != "Prefab" && type != "Sprite")
            {
                AddIssue(model, issues, row, "Type", "Unknown resource type '" + type + "'. Use AudioClip, Prefab, or Sprite.");
            }

            if (string.IsNullOrWhiteSpace((string)values["AssetPath"]))
            {
                AddIssue(model, issues, row, "AssetPath", "Required path is missing. Enter the saved asset's Assets/... path.");
            }
        }

        public static bool ValidateNames(IReadOnlyDictionary<string, ConfigTableModel> models,
            string gameNamespace, List<ConfigIssue> issues, string gameRoot = null)
        {
            var start = issues.Count;

            if (!ValidNamespace(gameNamespace))
            {
                issues.Add(new ConfigIssue
                {
                    Table = "Config Source",
                    Field = "Namespace",
                    Message = "Use C# identifiers separated by dots, for example MyGame. Avoid keywords and double underscores."
                });
            }

            var names = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "GameConfig", "the shared configuration loader" }
            };

            foreach (var model in models.Values.OrderBy(item => item.Source.Name, StringComparer.Ordinal))
            {
                var generated = new List<string> { model.Source.Name + "Table", model.Source.Name + "Config" };

                if (model.Source.Name == "UI" || model.Source.Name == "Scene")
                {
                    generated.Add(model.Source.Name + "Name");
                }

                if (model.Source.Name == "Resource")
                {
                    generated.AddRange(model.Data.Select(row => (string)row["Module"] + "Resources").Distinct(StringComparer.Ordinal));
                }

                foreach (var name in generated)
                {
                    if (names.TryGetValue(name, out var owner))
                    {
                        AddIssue(model, issues, 0, "", "Generated type '" + name + "' conflicts with " + owner + ". Rename the table or resource module.");
                    }
                    else
                    {
                        names.Add(name, model.Source.Name);
                    }
                }
            }

            if (ValidNamespace(gameNamespace))
            {
                ValidateSourceTypes(gameNamespace, names, issues, gameRoot);
            }

            return !HasErrors(issues, start);
        }

        private static void ValidateSourceTypes(string gameNamespace, Dictionary<string, string> generatedNames,
            List<ConfigIssue> issues, string gameRoot)
        {
            foreach (var path in AssetDatabase.GetAllAssetPaths().Where(value => value.StartsWith("Assets/", StringComparison.Ordinal)
                         && value.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)).OrderBy(value => value, StringComparer.Ordinal))
            {
                if (!File.Exists(path))
                {
                    continue;
                }

                var source = File.ReadAllText(path);

                var owned = source.StartsWith(GeneratedHeader, StringComparison.Ordinal)
                    || source.StartsWith("// Generated by UITableGenerator. Do not edit.", StringComparison.Ordinal)
                    || source.StartsWith("// Generated by ResourceModuleTableGenerator. Do not edit.", StringComparison.Ordinal);

                if (owned && (gameRoot == null || path.StartsWith(gameRoot.TrimEnd('/') + "/Scripts/Generated/", StringComparison.Ordinal)))
                {
                    continue;
                }

                foreach (var declared in ReadTopLevelTypes(source))
                {
                    foreach (var generated in generatedNames)
                    {
                        var fullName = gameNamespace + "." + generated.Key;

                        if (fullName == declared || fullName.StartsWith(declared + ".", StringComparison.Ordinal)
                            || declared.StartsWith(fullName + ".", StringComparison.Ordinal))
                        {
                            issues.Add(new ConfigIssue
                            {
                                Table = generated.Value,
                                Message = "Generated type '" + fullName + "' conflicts with '" + declared
                                    + "' in " + path + ". Rename the table, resource module, namespace, or handwritten type."
                            });
                        }
                    }
                }
            }
        }

        // Read declarations, not text inside comments or strings. Keep nested types out of the namespace's type list.
        private static IEnumerable<string> ReadTopLevelTypes(string source)
        {
            var tokens = SourceTokens.Matches(SourceTrivia.Replace(source, " ")).Cast<Match>().Select(match => match.Value).ToArray();
            var scopes = new Stack<(string Namespace, bool InType)>();
            var currentNamespace = "";
            var inType = false;
            var pendingNamespace = (string)null;
            var pendingType = false;

            for (var index = 0; index < tokens.Length; index++)
            {
                var token = tokens[index];

                if (token == "namespace")
                {
                    var parts = new List<string>();

                    while (++index < tokens.Length && tokens[index] != "{" && tokens[index] != ";")
                    {
                        parts.Add(tokens[index]);
                    }

                    pendingNamespace = (currentNamespace.Length == 0 ? "" : currentNamespace + ".") + string.Concat(parts);
                    index--;
                }
                else if (token == "{")
                {
                    scopes.Push((currentNamespace, inType));
                    currentNamespace = pendingNamespace ?? currentNamespace;
                    inType = inType || pendingType;
                    pendingNamespace = null;
                    pendingType = false;
                }
                else if (token == "}")
                {
                    if (scopes.Count > 0)
                    {
                        var previous = scopes.Pop();
                        currentNamespace = previous.Namespace;
                        inType = previous.InType;
                    }

                    pendingNamespace = null;
                    pendingType = false;
                }
                else if (token == ";")
                {
                    currentNamespace = pendingNamespace ?? currentNamespace;
                    pendingNamespace = null;
                    pendingType = false;
                }
                else if (token == "class" || token == "struct" || token == "interface" || token == "enum" || token == "record")
                {
                    if (index + 1 >= tokens.Length || !Identifier.IsMatch(tokens[index + 1])
                        || tokens[index + 1] == "class" || tokens[index + 1] == "struct")
                    {
                        continue;
                    }

                    if (!inType)
                    {
                        yield return (currentNamespace.Length == 0 ? "" : currentNamespace + ".") + tokens[index + 1];
                    }

                    pendingType = true;
                }
            }
        }

        public static HashSet<string> ValidateReferences(IReadOnlyDictionary<string, ConfigTableModel> models,
            List<ConfigIssue> issues)
        {
            var invalid = new HashSet<string>(StringComparer.Ordinal);

            foreach (var model in models.Values.OrderBy(item => item.Source.Name, StringComparer.Ordinal))
            {
                foreach (var field in model.Fields.Where(item => item.Type.StartsWith("ref:", StringComparison.Ordinal)))
                {
                    var targetName = field.Type.Substring(4);

                    if (!models.TryGetValue(targetName, out var target))
                    {
                        AddIssue(model, issues, 2, field.Name, "Referenced table '" + targetName + "' is unavailable. Add or fix that table, or remove this reference.");
                        invalid.Add(model.Source.Name);
                        invalid.Add(targetName);
                        continue;
                    }

                    var targetType = target.Fields[0].Type;
                    var targetIds = new HashSet<object>(target.Data.Select(row => row["Id"]));

                    for (var index = 0; index < model.Data.Count; index++)
                    {
                        var raw = (string)model.Data[index][field.Name];

                        if (!TryValue(raw, targetType, out var value) || !targetIds.Contains(value))
                        {
                            AddIssue(model, issues, model.SourceRows[index], field.Name,
                                targetName + " Id '" + raw + "' is unavailable. Use an existing Id from that table.");
                            invalid.Add(model.Source.Name);
                            invalid.Add(targetName);
                        }
                    }
                }
            }

            return invalid;
        }

        public static bool ValidateAssets(ConfigTableModel model, List<ConfigIssue> issues)
        {
            if (!SpecialFields.ContainsKey(model.Source.Name))
            {
                return true;
            }

            var start = issues.Count;
            model.Addresses.Clear();
            var entries = new List<AddressableAssetEntry>();
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings != null)
            {
                settings.GetAllAssets(entries, false, group =>
                {
                    var schema = group.GetSchema<BundledAssetGroupSchema>();
                    return schema != null && schema.IncludeInBuild && schema.IncludeAddressInCatalog;
                });
            }

            var table = model.Source.Name;
            var pathField = table == "UI" ? "PrefabPath" : table == "Scene" ? "ScenePath" : "AssetPath";
            var uiPrefabs = new HashSet<string>(StringComparer.Ordinal);

            for (var index = 0; index < model.Data.Count; index++)
            {
                var row = model.Data[index];
                var sourceRow = model.SourceRows[index];
                var id = (string)row["Id"];
                var path = (string)row[pathField];
                model.Addresses[id] = "";

                if (!string.IsNullOrWhiteSpace(path) && !ValidAssetPath(path))
                {
                    AddIssue(model, issues, sourceRow, pathField, "Use a project path starting with Assets/, without backslashes, empty segments, or parent-folder segments.");
                    continue;
                }

                var asset = string.IsNullOrWhiteSpace(path) ? null : AssetDatabase.LoadMainAssetAtPath(path);

                if (asset == null)
                {
                    AddIssue(model, issues, sourceRow, pathField,
                        (table == "UI" ? "UI '" + id + "' prefab" : "Asset") + " was not found at '" + path
                        + "'. Add the asset at this path or fix the path, then update config again.", table == "UI");
                    continue;
                }

                if (table == "UI")
                {
                    if (!(asset is GameObject prefab) || !ValidPrefab(prefab, path)
                        || prefab.GetComponents<global::UI.UIBase>().Length != 1)
                    {
                        AddIssue(model, issues, sourceRow, pathField, "Expected a saved prefab with exactly one UIBase component on its root.");
                        continue;
                    }

                    if (!uiPrefabs.Add(path))
                    {
                        AddIssue(model, issues, sourceRow, pathField, "This prefab is already used by another UI row. Use a separate prefab.");
                        continue;
                    }
                }
                else if (table == "Scene")
                {
                    if (!(asset is SceneAsset) || !path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                    {
                        AddIssue(model, issues, sourceRow, pathField, "Expected a saved Unity scene (.unity). Choose a scene asset.");
                        continue;
                    }
                }
                else if (!ValidResourceAsset(asset, path, (string)row["Type"], out var error))
                {
                    AddIssue(model, issues, sourceRow, pathField, error);
                    continue;
                }

                var guid = AssetDatabase.AssetPathToGUID(path);
                var addresses = entries.Where(entry => entry.guid == guid && entry.AssetPath == path
                        && !string.IsNullOrWhiteSpace(entry.address))
                    .Select(entry => entry.address).Distinct(StringComparer.Ordinal).ToList();

                if (addresses.Count != 1)
                {
                    AddIssue(model, issues, sourceRow, pathField, addresses.Count == 0
                        ? "Asset has no loadable Addressables address. Check its group, Include in Build, and Include Addresses in Catalog."
                        : "Asset has multiple Addressables addresses. Keep one loadable address for this asset.");
                    continue;
                }

                var address = addresses[0];

                if (entries.Any(entry => entry.address == address && entry.guid != guid))
                {
                    AddIssue(model, issues, sourceRow, pathField, "Addressables address '" + address + "' is shared by another asset. Give each asset a unique address.");
                    continue;
                }

                model.Addresses[id] = address;
            }

            return !HasErrors(issues, start);
        }

        private static bool ValidResourceAsset(UnityEngine.Object asset, string path, string type, out string error)
        {
            error = "Expected a " + type + " asset. Fix Type or AssetPath.";

            if (type == "Prefab")
            {
                return asset is GameObject prefab && ValidPrefab(prefab, path);
            }

            if (type == "AudioClip")
            {
                return asset is AudioClip && !AssetDatabase.IsSubAsset(asset);
            }

            if (type == "Sprite")
            {
                if (asset is Sprite && !AssetDatabase.IsSubAsset(asset))
                {
                    return true;
                }

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null && importer.textureType == TextureImporterType.Sprite
                    && importer.spriteImportMode == SpriteImportMode.Single
                    && AssetDatabase.LoadAssetAtPath<Sprite>(path) != null)
                {
                    return true;
                }

                error = "Expected a standalone Sprite or a texture imported as Sprite (Single). Multiple sprites and atlas sub-assets are not supported.";
            }

            return false;
        }

        private static bool ValidPrefab(GameObject prefab, string path)
        {
            return PrefabUtility.IsPartOfPrefabAsset(prefab) && path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
                && prefab.transform.parent == null;
        }

        private static bool ValidAssetPath(string path)
        {
            return path.StartsWith("Assets/", StringComparison.Ordinal) && !path.Contains('\\')
                && !path.Any(char.IsControl) && path.Split('/').All(part => part.Length > 0 && part != "." && part != "..");
        }

        private static bool TryValue(string raw, string type, out object value)
        {
            value = null;

            switch (type)
            {
                case "int":
                    if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer))
                    {
                        value = integer;
                        return true;
                    }

                    return false;

                case "float":
                    if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
                        && !float.IsNaN(number) && !float.IsInfinity(number))
                    {
                        value = number;
                        return true;
                    }

                    return false;

                case "bool":
                    if (bool.TryParse(raw, out var boolean))
                    {
                        value = boolean;
                        return true;
                    }

                    return false;

                default:
                    value = raw;
                    return true;
            }
        }

        private static string TypeExample(string type)
        {
            return type == "int" ? "a whole number such as 60"
                : type == "float" ? "a finite number such as 1.5"
                : type == "bool" ? "true or false" : "a valid value";
        }

        private static string Cell(List<string> cells, int column)
        {
            return column < cells.Count ? cells[column] ?? "" : "";
        }

        private static void CheckExtraCells(ConfigTableModel model, List<ConfigIssue> issues,
            List<string> cells, int count, int row)
        {
            for (var column = count; column < cells.Count; column++)
            {
                if (!string.IsNullOrWhiteSpace(cells[column]))
                {
                    AddIssue(model, issues, row, "Column " + (column + 1), "This column has no field name. Add its header and type, or clear the extra value.");
                }
            }
        }

        private static bool HasErrors(List<ConfigIssue> issues, int start)
        {
            return issues.Skip(start).Any(issue => !issue.Warning);
        }

        private static void AddIssue(ConfigTableModel model, List<ConfigIssue> issues, int row,
            string field, string message, bool warning = false)
        {
            issues.Add(new ConfigIssue
            {
                Warning = warning,
                Table = model.Source == null ? "Config" : model.Source.WorkbookName + " > " + model.Source.Name,
                Row = row,
                Field = field,
                Message = message
            });
        }
    }
}
