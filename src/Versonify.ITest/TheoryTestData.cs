using System.Collections;
using System.Collections.Generic;
using Versonify;

namespace Versonify.ITest;

public class CanonicalLongOptionsTestData : IEnumerable<object[]> {
    private readonly List<object[]> data = new() {
        new object[] { Clargs.Build(new(ArgNames.Command, "passive"), new(ArgNames.VersionSource, "{VS}")) },
        new object[] { Clargs.Build(new(ArgNames.Unknown, "passive"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.Debug, string.Empty)) },
        new object[] { Clargs.Build(new(ArgNames.Unknown, "set"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.Digits, "0"), new(ArgNames.QuickValue, "9"), new(ArgNames.DryRun, string.Empty)) },
        new object[] { Clargs.Build(new(ArgNames.Unknown, "behaviour"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.Digits, "*")) },
        new object[] { Clargs.Build(new(ArgNames.Unknown, "updatefiles"), new(ArgNames.Root, "{ROOT}"), new(ArgNames.Increment, string.Empty), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.MinMatch, "*.zzz"), new(ArgNames.Output, "con"), new(ArgNames.NoError, string.Empty)) },
        new object[] { Clargs.Build(new(ArgNames.Unknown, "updatefiles"), new(ArgNames.Root, "{ROOT}"), new(ArgNames.Increment, string.Empty), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.MinMatch, "{MM}|StdFile"), new(ArgNames.NoOverride, string.Empty)) },
        new object[] { Clargs.Build(new(ArgNames.Unknown, "passive"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.Output, "con")) },
        new object[] { Clargs.Build(new(ArgNames.Unknown, "updatefiles"), new(ArgNames.Root, "{ROOT}"), new(ArgNames.Increment, string.Empty), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.MinMatch, "{MM}|StdFile")) },
        new object[] { Clargs.Build(new(ArgNames.Unknown, "override"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.QuickValue, "9.9.9")) },
        new object[] { Clargs.Build(new(ArgNames.Unknown, "set"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.Release, "Beta")) },
        new object[] { Clargs.Build(new(ArgNames.Unknown, "updatefiles"), new(ArgNames.Root, "{ROOT}"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.MinMatch, "{MM}|StdFile")) },
        new object[] { Clargs.Build(new(ArgNames.Unknown, "passive"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.Trace, "info")) },
        new object[] { Clargs.Build(new(ArgNames.Unknown, "passive"), new(ArgNames.VersionSource, "{VS}")) },
        new object[] { Clargs.Build(new(ArgNames.Unknown, "updatefiles"), new(ArgNames.Root, "{ROOT}"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.Increment, string.Empty), new(ArgNames.MinMatch, "{MM}|StdFile")) },
        new object[] { Clargs.Build(new(ArgNames.Unknown, "passive"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.DigitGroup, "default")) },
        new object[] { Clargs.Build(new(ArgNames.Unknown, "passive"), new(ArgNames.VersionSource, "{VS}"), new(ArgNames.PreRelease, string.Empty)) }
    };

    public IEnumerator<object[]> GetEnumerator() => data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
