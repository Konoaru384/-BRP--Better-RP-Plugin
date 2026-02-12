using Exiled.API.Enums;
using Exiled.API.Interfaces;
using PlayerRoles;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace UnifiedSCPPlugin
{
    public class Config : IConfig
    {
        [Description("Enables or disables the entire plugin.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Enables debug logs for troubleshooting.")]
        public bool Debug { get; set; } = false;

        // ─────────────────────────────────────────────────────────────
        // CATEGORY: SCP ROOM ENTRY SYSTEM
        // ─────────────────────────────────────────────────────────────
        [Description("Settings related to SCP room entry detection and CASSIE messages.")]
        public ScpRoomEntrySettings ScpRoomEntry { get; set; } = new();

        // ─────────────────────────────────────────────────────────────
        // CATEGORY: SCP CONTAINMENT ANNOUNCEMENTS
        // ─────────────────────────────────────────────────────────────
        [Description("Settings for SCP containment announcements.")]
        public ContainmentSettings Containment { get; set; } = new();

        // ─────────────────────────────────────────────────────────────
        // CATEGORY: GATE SUMMON EVENTS
        // ─────────────────────────────────────────────────────────────
        [Description("Settings for gate summon events.")]
        public GateSummonSettings GateSummon { get; set; } = new();

        // ─────────────────────────────────────────────────────────────
        // CATEGORY: SCP-106 DOOR BLOCKING
        // ─────────────────────────────────────────────────────────────
        [Description("Settings related to SCP-106 door blocking and hints.")]
        public Scp106Settings Scp106 { get; set; } = new();

        // ─────────────────────────────────────────────────────────────
        // CATEGORY: SCP-173 AUTO-OPEN GATE
        // ─────────────────────────────────────────────────────────────
        [Description("Settings for automatically opening SCP-173's containment gate.")]
        public Gate173Settings Gate173 { get; set; } = new();

         // ─────────────────────────────────────────────────────────────
        // Blackout Hacking System
        // ─────────────────────────────────────────────────────────────

        [Description("Settings for the blackout hacking system.")]
        public BlackoutSettings Blackout { get; set; } = new();

        // ─────────────────────────────────────────────────────────────
        // CATEGORY: SCP DOOR & TELEPORTER IDS
        // ─────────────────────────────────────────────────────────────
        [Description("Customizable door and teleporter IDs for SCP containment rooms.")]
        public ScpDoorSettings ScpDoors { get; set; } = new();

        // ─────────────────────────────────────────────────────────────
        // CATEGORY: RESPAWN EVENT
        // ─────────────────────────────────────────────────────────────

        [Description("Settings for the Respawns Event.")]
        public UnifiedSystemSettings UnifiedSystem { get; set; } = new();

        // ─────────────────────────────────────────────────────────────
        // CATEGORY: HACKING SYSTEM
        // ─────────────────────────────────────────────────────────────

        [Description("Settings for the SCP-079 hacking system (.hack).")]
        public HackSettings Hack { get; set; } = new();

        // ─────────────────────────────────────────────────────────────
        // CATEGORY: 106 CUSTOM CONTAIMENT
        // ─────────────────────────────────────────────────────────────

        [Description("Settings for the SCP-106 Breach Handler system.")]
        public Scp106BreachHandlerSettings Breach106 { get; set; } = new();

        // ─────────────────────────────────────────────────────────────
        // CATEGORY: OPTIONAL CONFIGURATION FOR SCP ROOM ENTRY AND ROOM CHANGE DETECTION
        // ─────────────────────────────────────────────────────────────

        [Description("Optional configuration for SCP room entry and room change detection.")]
        public RMCH RMCH { get; set; } = new();

        // ─────────────────────────────────────────────────────────────
        // CATEGORY: CLASS D CELL DOOR ACCESS RESTRICTIONS ROUND START AND CARD EVENT
        // ─────────────────────────────────────────────────────────────

        [Description("Settings for Class-D cell door access restrictions.")]
        public ClassDCellAccessSettings ClassDCellAccess { get; set; } = new();

        // ─────────────────────────────────────────────────────────────
        // CATEGORY: SETTINGS FOR ROUND START WITH EVENT AND RP/HRP EVENTS
        // ─────────────────────────────────────────────────────────────

        [Description("Settings for round start behavior and HRP/RP logic.")]
        public RoundStartSettings RoundStart { get; set; } = new();

        // ─────────────────────────────────────────────────────────────
        // CATEGORY: SCP 3114 FLASHLIGHT BLINDNESS SYSTEM
        // ─────────────────────────────────────────────────────────────

        [Description("Settings for SCP-3114 flashlight blindness system.")]
        public Scp3114FlashSettings Scp3114Flash { get; set; } = new();

        // ─────────────────────────────────────────────────────────────
        // CATEGORY: IFINITE RESPAWN TOKEN MANAGER
        // ─────────────────────────────────────────────────────────────

        [Description("Settings for the infinite respawn token manager.")]
        public InfiniteRespawnSettings InfiniteRespawns { get; set; } = new();


    }



    // ─────────────────────────────────────────────────────────────
    // SCP ROOM ENTRY SETTINGS
    // ─────────────────────────────────────────────────────────────
    public class ScpRoomEntrySettings
    {
        [Description("Enables or disables the SCP room entry detection system.")]
        public bool Enable { get; set; } = true;

        [Description("Interval (in seconds) between room entry checks.")]
        public float CheckInterval { get; set; } = 1f;

        [Description("Duration of the initial red light phase.")]
        public float RedDuration { get; set; } = 5f;

        [Description("Duration of the white pulse light phase.")]
        public float WhitePulseDuration { get; set; } = 1f;

        [Description("Duration of the final red light phase.")]
        public float FinalRedDuration { get; set; } = 1f;

        [Description("Time before lights return to normal white state.")]
        public float ReturnToWhite { get; set; } = 1f;

        [Description("Chance (percentage) for StartupOgg1 to play when entering a room.")]
        public float StartupOgg1Chance { get; set; } = 50f;

        [Description("Chance (percentage) for ClassDCom18 audio to play.")]
        public float ClassDCom18Chance { get; set; } = 50f;

        [Description("Chance (percentage) for Class D keycard audio to play.")]
        public float ClassDCardChance { get; set; } = 50f;

        [Description("CASSIE messages played when an SCP breaches containment.")]
        public Dictionary<string, string> CassieBreachMessages { get; set; } = new()
        {
            { "Scp3114", "CUSTOMCASSIE OGG3114BREACH" },
            { "Scp173",  "CUSTOMCASSIE OGG173BREACH"  },
            { "Scp049",  "CUSTOMCASSIE OGG049BREACH"  },
            { "Scp106",  "CUSTOMCASSIE OGG106BREACH"  },
            { "Scp096",  "CUSTOMCASSIE OGG096BREACH"  },
            { "Scp939",  "CUSTOMCASSIE OGG939BREACH"  },
            { "Scp079",  "CUSTOMCASSIE OGG079BREACH"  },
            { "Scp066",  "CUSTOMCASSIE OGG066BREACH"  },
            { "Scp999",  "CUSTOMCASSIE OGG999BREACH"  },
            { "Scp008",  "CUSTOMCASSIE OGG008BREACH"  }
        };
    }

    // ─────────────────────────────────────────────────────────────
    // SCP CONTAINMENT SETTINGS
    // ─────────────────────────────────────────────────────────────
    public class ContainmentSettings
    {
        [Description("Enables or disables containment CASSIE announcements.")]
        public bool Enable { get; set; } = true;

        [Description("Custom CASSIE messages for each SCP when containment is completed.")]
        public Dictionary<string, string> CassieMessages { get; set; } = new()
    {
        { "Scp096", "containment complete . subject ninety six . terminated" },
        { "Scp173", "subject one seven three has been neutralized . facility secure" },
        { "Scp049", "instance zero four nine has ceased activity . containment successful" },
        { "Scp939", "containment confirmed for subject nine three nine . all units stand by" },
        { "Scp3114", "attention . subject three one one four has been contained . continue operations" },
        { "Scp066", "attention . subject zero six six has been silenced and contained" },
        { "Scp999", "containment complete . subject nine nine nine secured safely" },
        { "Scp008", "containment complete . subject zero zero eight secured safely" }
    };

        [Description("Default CASSIE message used when no SCP-specific message is found.")]
        public string DefaultCassieMessage { get; set; } = "containment procedure complete";

        // ─────────────────────────────────────────────────────────────
        // NEW: STANDARD DOORS (DoorType)
        // ─────────────────────────────────────────────────────────────
        [Description("Standard DoorType doors to lock for each SCP.")]
        public Dictionary<string, List<DoorType>> StandardDoors { get; set; } = new()
    {
        { "Scp173",  new() { DoorType.Scp173Gate } },
        { "Scp049",  new() { DoorType.Scp049Gate } },
        { "Scp3114", new() { DoorType.GR18Inner } },
        { "Scp106",  new() { DoorType.Scp106Primary, DoorType.Scp106Secondary } }
    };

        // ─────────────────────────────────────────────────────────────
        // NEW: CUSTOM MER DOOR IDS
        // ─────────────────────────────────────────────────────────────
        [Description("Custom MER door IDs to lock for each SCP.")]
        public Dictionary<string, List<string>> CustomDoorIds { get; set; } = new()
    {
        { "Scp939", new() { "4be19ae7" } },
        { "Scp173", new() { "a67a6d5c" } },
        { "Scp049", new() { "90e31d0e" } },
        { "Scp3114", new() { "008" } }
    };
    }


    // ─────────────────────────────────────────────────────────────
    // GATE SUMMON SETTINGS
    // ─────────────────────────────────────────────────────────────
    public class GateSummonSettings
    {
        [Description("Enables or disables gate summon events.")]
        public bool Enable { get; set; } = true;

        [Description("Enables debug logs for gate summon events.")]
        public bool Debug { get; set; } = false;
    }

    // ─────────────────────────────────────────────────────────────
    // SCP-106 SETTINGS
    // ─────────────────────────────────────────────────────────────
    public class Scp106Settings
    {
        [Description("Enables or disables SCP-106 door blocking behavior.")]
        public bool Enable { get; set; } = true;

        [Description("List of door names that SCP-106 is not allowed to pass through.")]
        public List<string> BlockedDoors { get; set; } = new()
        {
            "Scp106Primary",
            "Scp106Secondary"
        };

        [Description("Shows a hint to SCP-106 when attempting to pass through a blocked door.")]
        public bool ShowHint { get; set; } = true;

        [Description("Message displayed to SCP-106 when a blocked door is attempted.")]
        public string HintMessage { get; set; } = "<color=red>You cannot pass through this door!</color>";
    }

    // ─────────────────────────────────────────────────────────────
    // SCP-173 AUTO-OPEN GATE SETTINGS
    // ─────────────────────────────────────────────────────────────
    public class Gate173Settings
    {
        [Description("Enables or disables automatic opening of SCP-173's containment gate.")]
        public bool Enable { get; set; } = true;

        [Description("Name of the door to automatically open.")]
        public string DoorName { get; set; } = "Scp173Gate";

        [Description("Delay (in seconds) before the door is opened after the round starts.")]
        public float OpenDelay { get; set; } = 1f;
    }

    // ─────────────────────────────────────────────────────────────
    // BLACKOUT HACKING SYSTEM SETTINGS
    // ─────────────────────────────────────────────────────────────

    public class BlackoutSettings
    {
        [Description("Enables or disables the blackout hacking system.")] 
        public bool Enable { get; set; } = true;

        [Description("Room required to start the hacking process.")]
        public RoomType RequiredRoom { get; set; } = RoomType.EzUpstairsPcs;

        [Description("Teams allowed to start the blackout hack.")]
        public List<Team> AllowedTeams { get; set; } = new()
    {
        Team.ChaosInsurgency,
        Team.FoundationForces
    };

        [Description("Teams allowed to restore the lights after blackout.")]
        public List<Team> RestoreAllowedTeams { get; set; } = new()
    {
        Team.FoundationForces
    };

        [Description("Keywords required in the keycard name to start hacking.")]
        public List<string> RequiredKeycardKeywords { get; set; } = new()
    {
        "Chaos"
    };

        [Description("Duration of the hacking process in seconds.")]
        public float HackDuration { get; set; } = 200f;

        [Description("Maximum distance allowed from the hacking room.")]
        public float MaxDistance { get; set; } = 6f;

        [Description("Duration of the blackout in seconds.")]
        public float BlackoutDuration { get; set; } = 9999f;

        [Description("CASSIE message played when blackout is triggered.")]
        public string CassieMessage { get; set; } = "blackout";

        [Description("Broadcast message displayed when blackout starts.")]
        public string BlackoutBroadcastMessage { get; set; } =
            "<color=red><b>⚠ Blackout triggered! ⚠</b></color>";

        [Description("Duration of the blackout broadcast.")]
        public ushort BlackoutBroadcastDuration { get; set; } = 10;

        [Description("Hint shown when hacking is cancelled.")]
        public string HackCancelledHint { get; set; } =
            "<color=red>Hacking cancelled!</color>";

        [Description("Hint shown during hacking progress. {0} = percent.")]
        public string HackProgressHint { get; set; } =
            "<b><color=yellow>Generator sabotage in progress...</color></b>\nProgress: {0:0}%\n<color=red>Do not move away!</color>";

        [Description("Duration required to restore lights.")]
        public float RestoreDuration { get; set; } = 10f;

        [Description("Hint shown during light restoration.")]
        public string RestoreHint { get; set; } =
            "<color=yellow>Do not move for 10 seconds to restore power...</color>";

        [Description("Hint shown when restoration is cancelled.")]
        public string RestoreCancelledHint { get; set; } =
            "<color=red>Restoration cancelled, you moved!</color>";

        [Description("Zones affected when restoring lights.")]
        public ZoneType[] RestoreZones { get; set; } =
        {
        ZoneType.Entrance,
        ZoneType.HeavyContainment,
        ZoneType.LightContainment,
        ZoneType.Surface
    };

        [Description("Broadcast message displayed when lights are restored.")]
        public string RestoreBroadcastMessage { get; set; } =
            "<color=green><b>✅ Power restored!</b></color>";

        [Description("Duration of the restoration broadcast.")]
        public ushort RestoreBroadcastDuration { get; set; } = 10;
    }

    public class ScpDoorSettings
    {

        [Description("Door ID for SCP-939 containment.")]
        public string Scp939DoorId { get; set; } = "4be19ae7";

        [Description("Door ID for SCP-999 containment.")]
        public string Scp999DoorId { get; set; } = "90e31d0e";

        [Description("Door ID for SCP-173 MER room.")]
        public string Scp173MerDoorId { get; set; } = "ccd4a920";



        [Description("Door ID for SCP-096 containment.")]
        public string Scp096DoorId { get; set; } = "4be19a78";

        [Description("Teleport ID for SCP-096.")]
        public string Teleport096 { get; set; } = "11c66869";
    }

    // ─────────────────────────────────────────────────────────────
    // CUSTOM SPAWN WAVE SETTINGS FOR RESPWAN EVENT
    // ─────────────────────────────────────────────────────────────
    public class UnifiedSystemSettings
    {
        [Description("Duration of the early window (in seconds).")]
        public float EarlyWindowDuration { get; set; } = 600f;

        [Description("Minimum number of players required to activate the early window.")]
        public int MinPlayersForEarlyWindow { get; set; } = 7;

        [Description("Delay before SCP detection begins.")]
        public float ScpDetectionDelay { get; set; } = 5f;

        [Description("Interval between spectator conversion pulses.")]
        public float SpectatorPulseInterval { get; set; } = 150f;

        [Description("Broadcast duration for spectator conversion.")]
        public ushort SpectatorBroadcastDuration { get; set; } = 5;

        [Description("Broadcast text shown when spectators are converted.")]
        public string SpectatorBroadcastText { get; set; } =
            "<b><color=#FF8C00>New Class-D personnel have arrived on-site.</color></b>";

        [Description("Tokens given to Chaos during forced respawn.")]
        public int ChaosTokens { get; set; } = 2;

        [Description("Tokens given to MTF during forced respawn.")]
        public int NtfTokens { get; set; } = 0;

        [Description("Team name displayed for spectator cycles.")]
        public string SpectatorTeamName { get; set; } =
            "<color=#FFA500><b>CLASS D</b></color> 👥";

        [Description("Team name displayed for MTF.")]
        public string NtfTeamName { get; set; } =
            "<color=#00BFFF><b>MTF</b></color> ⚔️";

        [Description("Team name displayed for Chaos.")]
        public string ChaosTeamName { get; set; } =
            "<color=#32CD32><b>CHAOS</b></color> ⛔";

        [Description("Hint shown when Chaos arrives.")]
        public string ChaosArrivalHint { get; set; } =
            "<size=35><b><color=#32CD32>🚙 The van is arriving!</color></b></size>";

        [Description("Hint shown when MTF arrives.")]
        public string NtfArrivalHint { get; set; } =
            "<size=35><b><color=#00BFFF>🚁 The helicopter is arriving!</color></b></size>";

        [Description("Header for respawn hints.")]
        public string RespawnHeader { get; set; } =
            "<size=35><b><color=#FF4500>⚡ Respawn ⚡</color></b></size>";

        [Description("Label for team display.")]
        public string TeamLabel { get; set; } = "<size=28>Team:</size>";

        [Description("Label for countdown display.")]
        public string CountdownLabel { get; set; } = "<size=28>In:</size>";
    }

    // ─────────────────────────────────────────────────────────────
    // CASSIE HACKING SYSTEM SETTINGS
    // ─────────────────────────────────────────────────────────────
    public class HackSettings
    {
        [Description("Delay between hack progress ticks.")]
        public float HackTickDelay { get; set; } = 2.5f;

        [Description("Maximum distance before the hack is cancelled.")]
        public float ResetDistance { get; set; } = 22f;

        [Description("Distance at which corruption starts increasing.")]
        public float CorruptionRange { get; set; } = 6f;

        [Description("Corruption increase per tick when out of range.")]
        public float CorruptionRate { get; set; } = 12f;

        [Description("Corruption decrease per tick when inside range.")]
        public float CorruptionRecovery { get; set; } = 3f;

        [Description("Maximum corruption allowed before hack fails.")]
        public float MaxCorruption { get; set; } = 100f;

        [Description("Message shown when hack starts.")]
        public string HackStartMessage { get; set; } = "Hack started.";

        [Description("Message shown when hack is cancelled due to death.")]
        public string HackDeathMessage { get; set; } = "<color=red>You died. Hack cancelled.</color>";

        [Description("Message shown when player moves too far.")]
        public string HackTooFarMessage { get; set; } = "<color=red>You moved too far. Hack cancelled.</color>";

        [Description("Message shown when corruption reaches maximum.")]
        public string HackCorruptedMessage { get; set; } = "<color=red>Maximum corruption reached. Restart the hack.</color>";

        [Description("Hint shown during hack progress. {0}=progress, {1}=corruption.")]
        public string HackProgressHint { get; set; } =
            "<color=#00ffcc>💻 Hacking servers...</color>\n" +
            "<color=#e6edf3>Progress:</color> <b>{0}%</b>\n" +
            "<color=#ffa657>⚠️ Moving away increases corruption!</color>\n" +
            "<color=#ff5555>Corruption:</color> {1} / 100";

        [Description("Message shown when hack completes.")]
        public string HackCompleteMessage { get; set; } = "<color=#00FF00>Hack complete.</color>";

        [Description("Message shown when no spectator is available for SCP-079.")]
        public string NoCandidateMessage { get; set; } =
            "<color=orange>No available player for SCP-079. The next spectator/overwatch will be assigned.</color>";

        [Description("Message shown to the player who becomes SCP-079.")]
        public string Become079Message { get; set; } =
            "<color=#ffa657>You have become SCP-079 (hack successful).</color>";

        [Description("CASSIE message when SCP-079 is activated.")]
        public string Cassie079Message { get; set; } = "OGG079BREACH";

        [Description("Duration of the corridor chaos effect.")]
        public float CorridorChaosDuration { get; set; } = 30f;

        [Description("Initial interval between door toggles.")]
        public float CorridorChaosStartInterval { get; set; } = 2f;

        [Description("Final interval between door toggles.")]
        public float CorridorChaosEndInterval { get; set; } = 0.3f;

        [Description("Random jitter added to door toggle interval.")]
        public float CorridorChaosJitter { get; set; } = 0.2f;

        [Description("Initial toggle probability.")]
        public float CorridorChaosStartProbability { get; set; } = 0.3f;

        [Description("Final toggle probability.")]
        public float CorridorChaosEndProbability { get; set; } = 0.9f;

        [Description("Interval between light overcharges.")]
        public float OverchargeInterval { get; set; } = 10f;

        [Description("Excluded door IDs (MER doors).")]
        public List<string> ExcludedDoorIds { get; set; } = new()
    {
        "a67a6d5c",
        "4be19ae7",
        "90e31d0e",
        "008"
    };
    }

    // ─────────────────────────────────────────────────────────────
    // SCP-106 SETTINGS FOR CUSTOM UNCONTAINMENT BEHAVIOR
    // ─────────────────────────────────────────────────────────────

    public class Scp106BreachHandlerSettings
    {
        [Description("Enables or disables the SCP-106 breach handler system.")]
        public bool Enable { get; set; } = true;

        [Description("Number of generators required to unlock SCP-106 doors.")]
        public int GeneratorsRequired { get; set; } = 3;

        [Description("Hint shown when SCP-106 tries to open a locked door.")]
        public string LockedDoorHint { get; set; } =
            "<color=red>This door is locked until {0} generators are activated.</color>";

        [Description("Hint shown when SCP-106 tries to pass through a closed door.")]
        public string BlockedDoorHint { get; set; } =
            "<color=red>You cannot pass through these closed doors.</color>";

        [Description("Hint shown when SCP-106 is inside its containment room.")]
        public string InsideRoomHint { get; set; } =
            "<color=red>This room disables your abilities.</color>";

        [Description("Delay between runtime loop ticks.")]
        public float RuntimeTickDelay { get; set; } = 0.08f;

        [Description("Maximum distance from door center before pushback applies.")]
        public float MaxDistanceToCenter { get; set; } = 1.6f;

        [Description("Maximum distance from door plane before pushback applies.")]
        public float MaxDistanceToPlane { get; set; } = 1.0f;

        [Description("Pushback distance applied to SCP-106.")]
        public float PushDistance { get; set; } = 2.2f;
    }


    // ─────────────────────────────────────────────────────────────
    // BETTER CASSIE MESSAGES AND ROOM ENTRY SETTINGS
    // ─────────────────────────────────────────────────────────────


    public class RMCH
    {
        [Description("Enables or disables the SCP room entry and room change detection system.")]
        public bool Enable { get; set; } = true;

        [Description("Interval (in seconds) between room change checks.")]
        public float CheckInterval { get; set; } = 1f;

        [Description("Whether Cassie messages should be noisy.")]
        public bool CassieIsNoisy { get; set; } = false;

        [Description("Whether Cassie messages should be subtle.")]
        public bool CassieIsSubtle { get; set; } = false;

        [Description("Whether Cassie should auto-generate subtitles.")]
        public bool CassieAutoSubtitles { get; set; } = false;

        [Description("Whether Cassie should play background noise.")]
        public bool CassiePlayBackground { get; set; } = false;

        [Description("Cassie glitch scale (0 = none, 1 = heavy).")]
        public float CassieGlitchScale { get; set; } = 0f;

        [Description("CASSIE messages played when an SCP breaches containment.")]
        public Dictionary<string, string> CassieBreachMessages { get; set; } = new();

        [Description("CASSIE messages played when an SCP is terminated.")]
        public Dictionary<string, string> CassieTerminationMessages { get; set; } = new();
    }


    // ─────────────────────────────────────────────────────────────
    // RESTRICTIONS FOR CLASS-D CELL DOORS AND ROUND START CARD EVENTS
    // ─────────────────────────────────────────────────────────────


    public class ClassDCellAccessSettings
    {
        [Description("Enable or disable Class-D cell keycard restrictions.")]
        public bool Enable { get; set; } = true;

        [Description("If true, the player must HOLD the keycard. If false, any keycard in inventory works.")]
        public bool RequireHeldKeycard { get; set; } = true;

        [Description("List of allowed keycards for Class-D cell doors.")]
        public List<ItemType> AllowedKeycards { get; set; } = new()
    {
        ItemType.KeycardGuard,
        ItemType.KeycardMTFPrivate
    };

        [Description("Hint shown when a player tries to open a Class-D cell without permission.")]
        public string DeniedHint { get; set; } =
            "<color=red>You do not have the required keycard to open this cell.</color>";
    }


    // ─────────────────────────────────────────────────────────────
    // RP/HRP EVENT AND REBELION EVENT
    // ─────────────────────────────────────────────────────────────


    public class RoundStartSettings
    {
        [Description("Enable or disable the RoundHandler system.")]
        public bool Enable { get; set; } = true;

        [Description("Minimum number of players required to consider the round as RP (not HRP).")]
        public int MinPlayersForRP { get; set; } = 7;

        [Description("Chance (0.0 - 1.0) to trigger the special Class-D event.")]
        public float SpecialEventChance { get; set; } = 0.10f;

        [Description("Delay before playing the initial CASSIE message.")]
        public float InitialCassieDelay { get; set; } = 15f;

        [Description("CASSIE message played at round start (normal RP).")]
        public string CassieStartRP { get; set; } = "ur cassie message";

        [Description("CASSIE message played at round start (HRP).")]
        public string CassieStartHRP { get; set; } = "ur cassie message";

        [Description("CASSIE message played during the special event.")]
        public string CassieSpecialEvent { get; set; } = "ur cassie message";

        [Description("Broadcast shown when HRP mode is active.")]
        public string HrpBroadcast { get; set; } =
            "<color=red><b>❗ HRP Mode — Class-D cells have been opened!</b></color>";

        [Description("Duration of the HRP broadcast.")]
        public ushort HrpBroadcastDuration { get; set; } = 9999;

        [Description("Probability (0.0 - 1.0) that a Class-D receives a Guard keycard.")]
        public float ClassDKeycardChance { get; set; } = 0.45f;

        [Description("Whether Class-D cells should automatically open in HRP mode.")]
        public bool AutoOpenClassDCells { get; set; } = true;

        [Description("Whether to break plant windows at round start.")]
        public bool BreakPlantWindows { get; set; } = true;

        [Description("Whether to enable the HRP → RP transition broadcast.")]
        public bool EnableRPTransition { get; set; } = true;

        [Description("Broadcast shown when the round transitions from HRP to RP.")]
        public string RpTransitionBroadcast { get; set; } =
            "<color=red><b>💬 The round is now RP</b></color>";

        [Description("Delay before showing the RP transition broadcast.")]
        public float RpTransitionDelay { get; set; } = 5f;
    }

    public class Scp3114FlashSettings
    {
        [Description("Enable or disable the SCP-3114 flashlight blindness system.")]
        public bool Enable { get; set; } = true;

        [Description("Maximum distance at which a flashlight can blind SCP-3114.")]
        public float MaxDistance { get; set; } = 20f;

        [Description("Duration of the blindness effect (in seconds).")]
        public float BlindDuration { get; set; } = 2f;

        [Description("Interval between flashlight checks (in seconds).")]
        public float CheckInterval { get; set; } = 0.5f;

        [Description("Layer mask used for raycast detection.")]
        public LayerMask RaycastMask { get; set; } =
            ~LayerMask.GetMask("TransparentFX", "Hitbox", "InvisibleCollider", "Skybox");

        [Description("Whether debug logs should be printed.")]
        public bool Debug { get; set; } = false;
    }

    public class InfiniteRespawnSettings
    {
        [Description("Enable or disable the infinite respawn token manager.")]
        public bool Enable { get; set; } = true;

        [Description("How many tokens Chaos receives after an MTF respawn.")]
        public int ChaosTokensAfterMtf { get; set; } = 2;

        [Description("How many tokens MTF receives after a Chaos respawn.")]
        public int MtfTokensAfterChaos { get; set; } = 2;

        [Description("How many tokens mini-waves (NTF/Chaos) should always have.")]
        public int MiniWaveTokens { get; set; } = 0;

        [Description("Interval (in seconds) between token enforcement checks.")]
        public float EnforcementInterval { get; set; } = 2f;

        [Description("Enable debug logs for token manager.")]
        public bool Debug { get; set; } = false;
    }


}



