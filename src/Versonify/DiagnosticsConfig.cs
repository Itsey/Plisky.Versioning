namespace Versonify;

using System;
using System.Diagnostics;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;

public static class DiagnosticsConfig {
    public static void ConfigureTrace(VersonifyOptions options) {
        Console.WriteLine("Debug Mode, Adding Trace Handler");

        _ = Bilge.AddHandler(new ConsoleHandler(), HandlerAddOptions.SingleType);

        Bilge.SetConfigurationResolver((name, inLevel) => {
            var returnLvl = SourceLevels.Verbose;

            if ((options.Trace != null) && (options.Trace.ToLowerInvariant() == "info")) {
                returnLvl = SourceLevels.Information;
            }

            return name.Contains("Plisky-Versioning") || name.Contains("Versonify") ? returnLvl : inLevel;
        });
    }
}
