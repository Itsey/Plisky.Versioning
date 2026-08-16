namespace Versonify;

using System;
using System.Diagnostics;
using Plisky.Diagnostics;
using Plisky.Diagnostics.Listeners;

public static class DiagnosticsConfig {

    public static void ConfigureTrace(VersonifyOptions options) {
        Console.WriteLine("Debug Mode, Adding Trace Handler");

        _ = Bilge.AddHandler(new ConsoleHandler(), HandlerAddOptions.SingleType);
#if DEBUG
        var hnd = new TCPHandler(new TCPHandlerOptions("127.0.0.1", 9060));
        hnd.SetFormatter(new FlimFlamV4Formatter());
        _ = Bilge.AddHandler(hnd, HandlerAddOptions.SingleType);
#endif
        Bilge.SetConfigurationResolver((name, inLevel) => {
            var returnLvl = SourceLevels.Verbose;

            if ((options.Trace != null) && string.Equals(options.Trace, "info", StringComparison.InvariantCultureIgnoreCase)) {
                returnLvl = SourceLevels.Information;
            }

            return name.Contains("Plisky-Versioning") || name.Contains("Versonify") ? returnLvl : inLevel;
        });
    }
}