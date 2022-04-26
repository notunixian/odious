using System;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReModCE.Loader;
using VRC;
using System.IO;

namespace ReModCE.Components
{
    internal class InfoLogsComponent : ModComponent
    {
        private ConfigValue<bool> JoinLeaveLogsEnabled;
        private ReMenuToggle _joinLeaveLogsToggle;
        private const string SavePath = "UserData/Odious/Logs";

        public InfoLogsComponent()
        {
            JoinLeaveLogsEnabled = new ConfigValue<bool>(nameof(JoinLeaveLogsEnabled), true);
            JoinLeaveLogsEnabled.OnValueChanged += () => _joinLeaveLogsToggle.Toggle(JoinLeaveLogsEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage("Logging");
            _joinLeaveLogsToggle = menu.AddToggle("Join/Leave Logs",
                "Enable whether player joins/leaves should be logged in console. This also logs to your Odious folder.", JoinLeaveLogsEnabled.SetValue,
                JoinLeaveLogsEnabled);
        }

        public override void OnPlayerJoined(Player player)
        {
            if (!JoinLeaveLogsEnabled) return;
            ReLogger.Msg(ConsoleColor.Cyan, $"{player.field_Private_APIUser_0.displayName} joined the instance.");
            ReModCE.LogDebug($"<color=green>[+]</color> {player.field_Private_APIUser_0.displayName}");
            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
                if (!File.Exists($"{SavePath}/Players.txt"))
                {
                    File.Create($"{SavePath}/Players.txt");
                }
            }

            using (StreamWriter writer = File.AppendText($"{SavePath}/Players.txt"))
            {
                // don't log the local player
                if (player.field_Private_APIUser_0.displayName == Player.prop_Player_0.field_Private_APIUser_0.displayName)
                {
                    return;
                }
                writer.WriteLine($"{player.field_Private_APIUser_0.displayName} | {player.field_Private_APIUser_0.id} | seen at {Player.prop_Player_0.prop_APIUser_0.location} \n");
            }
        }

        public override void OnPlayerLeft(Player player)
        {
            if (!JoinLeaveLogsEnabled) return;
            ReLogger.Msg(ConsoleColor.White, $"{player.field_Private_APIUser_0.displayName} left the instance.");
            ReModCE.LogDebug($"<color=red>[-]</color> {player.field_Private_APIUser_0.displayName}");
        }
    }
}
