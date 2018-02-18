using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PatternsEntities
{
    public enum TracingStatus : byte
    {
        Invalid = 0,
        Live = 1,
        Unplugged = 2,
        Recovery = 3,
        Error = 4,
        Late = 5
    }

    public enum EpisodeStatus
    {
        Unknown,
        Admitted,
        Discharged,
        ToHistory
    }

    public enum PluginsAction
    {
        Invalid = 0,
        UpdateIntervalDuration = 1
    }
}
