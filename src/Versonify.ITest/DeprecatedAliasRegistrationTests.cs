using System.Collections;
using System.CommandLine;
using System.Reflection;
using Shouldly;

namespace Versonify.ITest;

/// <summary>
/// Guards against drift between the deprecated alias warning maps and the aliases actually registered
/// on the root command. An alias present in a map but missing from the command emits a deprecation
/// warning and then fails to parse, which is why these are verified together.
/// </summary>
public class DeprecatedAliasRegistrationTests {

    public static TheoryData<string> DeprecatedAliasKeys {
        get {
            var result = new TheoryData<string>();
            foreach (string alias in GetAllDeprecatedAliasKeys()) {
                result.Add(alias);
            }
            return result;
        }
    }

    [Theory]
    [MemberData(nameof(DeprecatedAliasKeys))]
    public void Every_deprecated_alias_key_is_registered_on_the_root_command(string deprecatedAlias) {
        var registeredAliases = GetRegisteredAliases();

        bool isRegistered = registeredAliases.Contains(deprecatedAlias);

        isRegistered.ShouldBeTrue($"Alias '{deprecatedAlias}' warns as deprecated but is not registered, so it cannot be parsed.");
    }

    [Fact]
    public void Deprecated_alias_maps_are_not_empty() {
        GetAllDeprecatedAliasKeys().Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Every_deprecated_alias_maps_to_a_canonical_option_name() {
        var registeredNames = GetRegisteredOptionNames();

        foreach (var entry in GetDeprecatedAliasMap("deprecatedAliasMapLower")) {
            registeredNames.ShouldContain(entry.Value, $"Canonical option '{entry.Value}' for alias '{entry.Key}' is not a registered option name.");
        }

        foreach (var entry in GetDeprecatedAliasMap("deprecatedAliasMapUpper")) {
            registeredNames.ShouldContain(entry.Value, $"Canonical option '{entry.Value}' for alias '{entry.Key}' is not a registered option name.");
        }
    }

    [Fact]
    public void Deprecated_aliases_are_absent_when_compatibility_is_disabled() {
        var registeredAliases = GetRegisteredAliases(includeDeprecatedAliases: false);

        foreach (string alias in GetAllDeprecatedAliasKeys()) {
            bool isRegistered = registeredAliases.Contains(alias);

            isRegistered.ShouldBeFalse($"Alias '{alias}' is deprecated and must not be registered when compatibility is disabled.");
        }
    }

    private static List<string> GetAllDeprecatedAliasKeys() {
        var result = new List<string>();

        foreach (var entry in GetDeprecatedAliasMap("deprecatedAliasMapLower")) {
            result.Add(entry.Key);
        }

        foreach (var entry in GetDeprecatedAliasMap("deprecatedAliasMapUpper")) {
            result.Add(entry.Key);
        }

        return result;
    }

    private static List<KeyValuePair<string, string>> GetDeprecatedAliasMap(string fieldName) {
        var field = typeof(CommandLineParser).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
        field.ShouldNotBeNull($"Expected a private static field named '{fieldName}' on CommandLineParser.");

        var rawMap = field!.GetValue(null) as IEnumerable;
        rawMap.ShouldNotBeNull($"Field '{fieldName}' did not contain an enumerable map.");

        var result = new List<KeyValuePair<string, string>>();
        foreach (object? entry in rawMap!) {
            var entryType = entry!.GetType();
            string key = (string)entryType.GetProperty("Key")!.GetValue(entry)!;
            string value = (string)entryType.GetProperty("Value")!.GetValue(entry)!;
            result.Add(new KeyValuePair<string, string>(key, value));
        }

        return result;
    }

    private static RootCommand BuildRootCommand(bool includeDeprecatedAliases) {
        var method = typeof(CommandLineParser).GetMethod("BuildRootCommand", BindingFlags.NonPublic | BindingFlags.Static);
        method.ShouldNotBeNull("Expected a private static BuildRootCommand method on CommandLineParser.");

        return (RootCommand)method!.Invoke(null, [includeDeprecatedAliases])!;
    }

    private static HashSet<string> GetRegisteredAliases(bool includeDeprecatedAliases = true) {
        // Alias lookup is case-insensitive here because the lower map stores lowercased probe keys.
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var option in BuildRootCommand(includeDeprecatedAliases).Options) {
            foreach (string alias in option.Aliases) {
                result.Add(alias);
            }
        }

        return result;
    }

    private static HashSet<string> GetRegisteredOptionNames() {
        var result = new HashSet<string>(StringComparer.Ordinal);

        foreach (var option in BuildRootCommand(true).Options) {
            result.Add(option.Name);
        }

        return result;
    }
}
