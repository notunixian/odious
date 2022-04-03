using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppSystem.Text.RegularExpressions;
using MelonLoader;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using ReModCE.Loader;
using UnityEngine;
using UnityEngine.UI;
using System.Windows.Forms;
using ReModCE.EvilEyeSDK;

namespace ReModCE.Components
{
    internal class WorldSpoofComponent : ModComponent
    {
        private ConfigValue<bool> WorldSpoofEnabled;
        private static ReMenuToggle _WorldSpoofToggled;
        private ConfigValue<bool> WorldSpoofNagEnabled;
        private static ReMenuToggle _WorldSpoofNagToggled;
        private static MelonPreferences_Entry<bool> _WorldSpoof;
        private static MelonPreferences_Entry<bool> _WorldSpoofWarn;
        private static MelonPreferences_Entry<string> _WorldIdToSpoof;
        private static MelonPreferences_Entry<string> _InstanceIdToSpoof;
        private ReMenuButton _WorldIDSpoof;
        private ReMenuButton _InstanceIDSpoof;
        private ReMenuButton _GetWorldID;


        public WorldSpoofComponent()
        {
            WorldSpoofEnabled = new ConfigValue<bool>(nameof(WorldSpoofEnabled), false);
            WorldSpoofEnabled.OnValueChanged += () => _WorldSpoofToggled.Toggle(WorldSpoofEnabled);

            WorldSpoofNagEnabled = new ConfigValue<bool>(nameof(WorldSpoofNagEnabled), true);
            WorldSpoofNagEnabled.OnValueChanged += () => _WorldSpoofNagToggled.Toggle(WorldSpoofNagEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var spoofingMenu = uiManager.MainMenu.GetCategoryPage("Spoofing").GetCategory("Worlds");
            _WorldSpoofToggled = spoofingMenu.AddToggle("World Spoof", "Will spoof your world to be something else than it actually is, (by default it is VRChat Home World w/ instance id 1337. Useful for tricking stalkers or for fun.", b => { WorldSpoofEnabled.SetValue(b); WorldSpoof(b); }, WorldSpoofEnabled);
            _WorldSpoofNagToggled = spoofingMenu.AddToggle("World Spoof Warn", "Will nag you about your world being spoofed every ~30 seconds.", b => { WorldSpoofNagEnabled.SetValue(b); WorldSpoofNag(b); }, WorldSpoofNagEnabled);


            _WorldIDSpoof = spoofingMenu.AddButton("World ID to Spoof",
                "Sets the world ID for world spoofing, by default is the VRChat home world.",
                () =>
                {
                    PopupTextInput(_WorldIDSpoof, _WorldIdToSpoof, false);
                });

            _InstanceIDSpoof = spoofingMenu.AddButton("Instance ID to Spoof",
                "Sets the instance ID for world spoofing, by default is 1337. Letters and Numbers ONLY for instance id.",
                () =>
                {
                    PopupTextInput(_InstanceIDSpoof, _InstanceIdToSpoof, true);
                });

            _GetWorldID = spoofingMenu.AddButton("Grab World ID",
                "Gets the World ID of the current world you are in. (useful for this feature.)",
                () =>
                {
                    if (Clipboard.ContainsText())
                    {
                        Clipboard.Clear();
                        if (VRC.Player.prop_Player_0.prop_APIUser_0.location != "")
                        {
                            Clipboard.SetText(VRC.Player.prop_Player_0.prop_APIUser_0.location);
                        }
                    }
                    if (VRC.Player.prop_Player_0.prop_APIUser_0.location != "")
                    {
                        Clipboard.SetText(VRC.Player.prop_Player_0.prop_APIUser_0.location);
                        ReLogger.Msg($"Copied current world ID {VRC.Player.prop_Player_0.prop_APIUser_0.location} to clipboard!");
                    }
                });
        }

        private void PopupTextInput(ReMenuButton button, MelonPreferences_Entry<string> configValue, bool isInstanceSpoof)
        {
            var category = MelonPreferences.GetCategory("ReModCE");
            _WorldIdToSpoof = (MelonPreferences_Entry<string>)category.GetEntry("WorldIdToSpoof");
            _InstanceIdToSpoof = (MelonPreferences_Entry<string>)category.GetEntry("InstanceIdToSpoof");

            VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Input World/Instance ID to spoof",
                $"", InputField.InputType.Standard, false, "Submit",
                (s, k, t) =>
                {

                    if (string.IsNullOrEmpty(s))
                        return;

                    if (s.Contains("worldId=") && s.Contains("&instanceId"))
                    {
                        var worldIdIndex = s.IndexOf("worldId=");
                        var instanceIdIndex = s.IndexOf("&instanceId=");
                        var worldId = s.Substring(worldIdIndex + "worldId=".Length, instanceIdIndex - (worldIdIndex + "worldId=".Length));
                        var instanceId = s.Substring(instanceIdIndex + "&instanceId=".Length);

                        if (configValue.Identifier.Contains("_WorldIdToSpoof"))
                        {
                            s = $"{worldId}".Trim().TrimEnd('\r', '\n');
                            ReLogger.Msg($"parsed vrc join link to {s}");
                        }
                        else
                        {
                            s = $"{instanceId}".Trim().TrimEnd('\r', '\n');
                            ReLogger.Msg($"parsed vrc join link to {s}");
                        }
                    }

                    if (isInstanceSpoof == true)
                    {
                        if (!Regex.IsMatch(s, "^[A-Za-z0-9]*$"))
                        {
                            ReLogger.Msg($"Not setting InstanceID due to instance id containing characters other than letters or numbers.");
                            return;
                        }
                    }
                    else
                    {
                        if (!s.Contains("wrld_"))
                        {
                            ReLogger.Msg($"Not setting WorldID due to it not containing wrld_");
                            return;
                        }
                    }

                    // do this check since the game does not really like me
                    if (!isInstanceSpoof)
                    {
                        _WorldIdToSpoof.Value = s;
                        MelonPreferences.Save();
                    }

                    if (isInstanceSpoof)
                    {
                        _InstanceIdToSpoof.Value = s;
                        MelonPreferences.Save();
                    }

                }, null);
        }

        public void WorldSpoof(bool enabled)
        {
            var category = MelonPreferences.GetCategory("ReModCE");
            _WorldSpoof = (MelonPreferences_Entry<bool>)category.GetEntry("WorldSpoof");

            if (enabled)
            {
                _WorldSpoof.Value = true;
                MelonPreferences.Save();
            }
            else
            {
                _WorldSpoof.Value = false;
                MelonPreferences.Save();
            }
        }

        public void WorldSpoofNag(bool enabled)
        {
            var category = MelonPreferences.GetCategory("ReModCE");
            _WorldSpoofWarn = (MelonPreferences_Entry<bool>)category.GetEntry("WorldSpoofWarn");

            if (enabled)
            {
                _WorldSpoofWarn.Value = true;
                MelonPreferences.Save();
            }
            else
            {
                _WorldSpoofWarn.Value = false;
                MelonPreferences.Save();
            }
        }
    }
}
