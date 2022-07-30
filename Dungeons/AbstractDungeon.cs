﻿using ff14bot.Managers;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Abstract starting point for implementing specialized dungeon logic.
/// </summary>
public abstract class AbstractDungeon
{
    /// <summary>
    /// Gets zone ID for this dungeon.
    /// </summary>
    public const ZoneId ZoneId = Data.ZoneId.NONE;

    /// <summary>
    /// Gets <see cref="DungeonId"/> for this dungeon.
    /// </summary>
    public abstract DungeonId DungeonId { get; }

    /// <summary>
    /// Gets a handle to signal the combat routine should not use certain features (e.g., prevent CR from moving).
    /// </summary>
    protected CapabilityManagerHandle CapabilityHandle { get; } = CapabilityManager.CreateNewHandle();

    /// <summary>
    /// Gets SideStep Plugin reference.
    /// </summary>
    protected PluginContainer SidestepPlugin { get; } = PluginHelpers.GetSideStepPlugin();

    /// <summary>
    /// Executes dungeon logic.
    /// </summary>
    /// <returns><see langword="true"/> if this behavior expected/handled execution.</returns>
    public abstract Task<bool> RunAsync();
}
