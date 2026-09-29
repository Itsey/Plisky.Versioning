using System.Collections;

namespace Versonify.ITest;

public class CanonicalLongOptionsTestData : IEnumerable<object[]> {

    private readonly List<object[]> data = [
        [Clargs.Build(new(ArgNames.Command, "passive"), new(ArgNames.VersionSource, "{VS}"))],
        [Clargs.Build(new(ArgNames.Unknown, "passive"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.Debug, string.Empty))],
        [Clargs.Build(new(ArgNames.Unknown, "set"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.Digits, "0"), new(ArgNames.QuickValue, "9"), new(ArgNames.DryRun, string.Empty))],
        [Clargs.Build(new(ArgNames.Unknown, "behaviour"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.Digits, "*"))],
        [Clargs.Build(new(ArgNames.Unknown, "updatefiles"), new(ArgNames.Root, "{ROOT}"), new(ArgNames.Increment, string.Empty), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.MinMatch, "*.zzz"), new(ArgNames.Output, "con"), new(ArgNames.NoError, string.Empty))],
        [Clargs.Build(new(ArgNames.Unknown, "updatefiles"), new(ArgNames.Root, "{ROOT}"), new(ArgNames.Increment, string.Empty), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.MinMatch, "{MM}|StdFile"), new(ArgNames.NoOverride, string.Empty))],
        [Clargs.Build(new(ArgNames.Unknown, "passive"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.Output, "con"))],
        [Clargs.Build(new(ArgNames.Unknown, "updatefiles"), new(ArgNames.Root, "{ROOT}"), new(ArgNames.Increment, string.Empty), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.MinMatch, "{MM}|StdFile"))],
        [Clargs.Build(new(ArgNames.Unknown, "override"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.QuickValue, "9.9.9"))],
        [Clargs.Build(new(ArgNames.Unknown, "set"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.Release, "Beta"))],
        [Clargs.Build(new(ArgNames.Unknown, "updatefiles"), new(ArgNames.Root, "{ROOT}"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.MinMatch, "{MM}|StdFile"))],
        [Clargs.Build(new(ArgNames.Unknown, "passive"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.DryRun, string.Empty))],
        [Clargs.Build(new(ArgNames.Unknown, "passive"), new(ArgNames.VersionSource, "{VS}"))],
        [Clargs.Build(new(ArgNames.Unknown, "updatefiles"), new(ArgNames.Root, "{ROOT}"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.Increment, string.Empty), new(ArgNames.MinMatch, "{MM}|StdFile"))],
        [Clargs.Build(new(ArgNames.Unknown, "passive"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.DigitGroup, "default"))],
        [Clargs.Build(new(ArgNames.Unknown, "passive"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.PreRelease, string.Empty))]
    ];

    public IEnumerator<object[]> GetEnumerator() => data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}