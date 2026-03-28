using System.Text.Json.Nodes;

namespace NugetSkills.Services;

public static class JsonHelpers
{
    public static JsonObject LoadOrCreateObject(string path)
    {
        if (!File.Exists(path))
            return new JsonObject();

        try
        {
            var existing = JsonNode.Parse(File.ReadAllText(path));
            return existing as JsonObject ?? new JsonObject();
        }
        catch
        {
            return new JsonObject();
        }
    }

    public static void MergeHookEntry(JsonObject root, string eventName, JsonObject hookEntry)
    {
        if (root.TryGetPropertyValue("hooks", out var existingHooks) && existingHooks is JsonObject hooksObj)
        {
            if (hooksObj.TryGetPropertyValue(eventName, out var arr) && arr is JsonArray array)
            {
                if (!ContainsNugetSkillsHook(array))
                    array.Add(hookEntry);
            }
            else
            {
                hooksObj[eventName] = new JsonArray { hookEntry };
            }
        }
        else
        {
            root["hooks"] = new JsonObject { [eventName] = new JsonArray { hookEntry } };
        }
    }

    public static void WriteJson(string path, JsonObject root)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, root.ToJsonString(Constants.JsonOptions));
    }

    private static bool ContainsNugetSkillsHook(JsonArray array)
    {
        return array.Any(entry =>
        {
            if (entry is not JsonObject obj)
                return false;

            // Direct command field (Cursor format)
            if (obj.TryGetPropertyValue("command", out var cmd) &&
                cmd?.GetValue<string>().Contains(Constants.HookIdentifier) == true)
                return true;

            // Nested hooks array (Claude format)
            if (obj.TryGetPropertyValue("hooks", out var h) && h is JsonArray innerArr)
                return innerArr.Any(hook =>
                    hook?.AsObject().TryGetPropertyValue("command", out var innerCmd) == true &&
                    innerCmd?.GetValue<string>().Contains(Constants.HookIdentifier) == true);

            return false;
        });
    }
}
