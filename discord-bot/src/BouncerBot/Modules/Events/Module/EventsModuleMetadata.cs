using System;
using System.Collections.Generic;
using System.Text;

namespace BouncerBot.Modules.Events.Module;

internal static class EventsModuleMetadata
{
    public const string ModuleName = "events";
    public const string ModuleDescription = "Commands related to events";

    public static class MlialCommand
    {
        public const string Name = "mlial";
        public const string Description = "Get a log summary for MLIAL";
    }
}
