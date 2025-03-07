﻿using Buddy.Coroutines;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing.Avoidance;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trust.Data;
using Trust.Extensions;
using Trust.Helpers;

namespace Trust.Dungeons;

/// <summary>
/// Lv. 61: The Sirensong Sea dungeon logic.
/// </summary>
public class SirensongSea : AbstractDungeon
{
    /// <inheritdoc/>
    public override ZoneId ZoneId => Data.ZoneId.TheSirensongSea;

    /// <inheritdoc/>
    public override DungeonId DungeonId => DungeonId.TheSirensongSea;

    /// <inheritdoc/>
    protected override HashSet<uint> SpellsToFollowDodge { get; } = new() { EnemyAction.Hydroball };

    public override Task<bool> OnEnterDungeonAsync()
    {
        AvoidanceManager.AvoidInfos.Clear();

        AvoidanceManager.AddAvoidObject<EventObject>(() => Core.Player.InCombat, 6f, EnemyNpc.FirePuddle);
        AvoidanceManager.AddAvoidObject<EventObject>(() => Core.Player.InCombat, 6f, EnemyNpc.WaterPuddle);

        // Boss Arenas
        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.SpaeRock,
            () => ArenaCenter.Lugat,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.GloweringKrautz,
            () => ArenaCenter.TheGovernor,
            outerRadius: 90.0f,
            innerRadius: 19.0f,
            priority: AvoidancePriority.High);

        AvoidanceHelpers.AddAvoidDonut(
            () => Core.Player.InCombat && WorldManager.SubZoneId == (uint)SubZoneId.WardensDelight,
            () => ArenaCenter.Lorelei,
            outerRadius: 90.0f,
            innerRadius: 15.0f,
            priority: AvoidancePriority.High);

        return Task.FromResult(false);
    }

    /// <inheritdoc/>
    public override async Task<bool> RunAsync()
    {
        await FollowDodgeSpells();

        if (WorldManager.SubZoneId == (uint)SubZoneId.GloweringKrautz && Core.Player.InCombat)
        {
            BattleCharacter TheGovernorNPC = GameObjectManager.GetObjectsByNPCId<BattleCharacter>(EnemyNpc.TheGovernor)
                .FirstOrDefault(bc => bc.IsTargetable && bc.IsValid);
            if (EnemyAction.EnterNight.IsCasting() && TheGovernorNPC.TargetGameObject == Core.Me)
            {
                ff14bot.Helpers.Logging.WriteDiagnostic("Running away from Enger Night");
                while (!CommonBehaviors.IsLoading && !QuestLogManager.InCutscene && Core.Me.Location.Distance2D(ArenaCenter.TheGovernorArenaEdge) > 1)
                {
                    Navigator.PlayerMover.MoveTowards(ArenaCenter.TheGovernorArenaEdge);
                    await Coroutine.Yield();
                }

                await CommonTasks.StopMoving();
            }
        }

        return false;
    }

    private static class EnemyNpc
    {
        /// <summary>
        /// First Boss: Lugat.
        /// </summary>
        public const uint Lugat = 6071;

        /// <summary>
        /// Before second boss.
        /// </summary>
        public const uint FirePuddle = 2007809;

        /// <summary>
        /// Second Boss: The Governor.
        /// </summary>
        public const uint TheGovernor = 6072;

        /// <summary>
        /// Final Boss: Lorelei .
        /// </summary>
        public const uint Lorelei = 6074;

        /// <summary>
        /// Before Final boss.
        /// </summary>
        public const uint WaterPuddle = 2007808;
    }

    private static class ArenaCenter
    {
        /// <summary>
        /// First Boss: Lugat.
        /// </summary>
        public static readonly Vector3 Lugat = new(-1.791643f, -2.900793f, -215.6073f);

        /// <summary>
        /// Second Boss: The Governor.
        /// </summary>
        public static readonly Vector3 TheGovernor = new(-7.938193f, 4.440489f, 79.09968f);

        /// <summary>
        /// Second Boss: The Governor > Arena edge.
        /// </summary>
        public static readonly Vector3 TheGovernorArenaEdge = new(8.985318f, 4.437799f, 70.16875f);

        /// <summary>
        /// Third Boss: Lorelei.
        /// </summary>
        public static readonly Vector3 Lorelei = new(-44.54654f, 7.751197f, 465.0925f);
    }

    private static class EnemyAction
    {
        /// <summary>
        /// Lugat
        /// Hydroball
        /// Stack
        /// </summary>
        public const uint Hydroball = 8023;

        /// <summary>
        /// The Governor
        /// Enter Night
        /// Move to edge of arena to break tether
        /// </summary>
        public static readonly HashSet<uint> EnterNight = new() { 8032 };
    }
}
