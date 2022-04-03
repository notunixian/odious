using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MelonLoader;
using Newtonsoft.Json;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using ReModCE.Loader;
using ReModCE.SDK;
using UnhollowerBaseLib;
using VRC;
using UnityEngine;

namespace ReModCE.Components
{
    internal class HWIDSpoofComponent : ModComponent
    {
        private ConfigValue<bool> HWIDSpoofEnabled;
        private static ReMenuToggle _HWIDSpoofToggled;
        private static MelonPreferences_Entry<bool> _HWIDSpoof;

        public HWIDSpoofComponent()
        {
            var category = MelonPreferences.GetCategory("ReModCE");
            _HWIDSpoof = (MelonPreferences_Entry<bool>)category.GetEntry("HWIDSpoof");

            HWIDSpoofEnabled = new ConfigValue<bool>(nameof(HWIDSpoofEnabled), true);
            HWIDSpoofEnabled.OnValueChanged += () => _HWIDSpoofToggled.Toggle(HWIDSpoofEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var spoofMenu = uiManager.MainMenu.GetCategoryPage("Spoofing").GetCategory("Hardware");
            _HWIDSpoofToggled = spoofMenu.AddToggle("HWID Spoof", "Spoofs your hardware identifier to prevent bans, this regenerates automatically every restart if enabled.", b => { HWIDSpoofEnabled.SetValue(b); HWIDSpoof(b); }, HWIDSpoofEnabled);
        }

        public void HWIDSpoof(bool enabled)
        {
            if (enabled)
            {
                _HWIDSpoof.Value = true;
                MelonPreferences.Save();

                GeneralWrapper.AlertAction("Notice", "This will apply on next restart, Restart now?", "Restart",
                    delegate
                    {
                        try
                        {
                            Process.Start(Environment.CurrentDirectory + "/VRChat.exe", Environment.CommandLine);
                        }
                        catch (Exception e)
                        {
                            ReLogger.Error($"Unable to restart process, VRChat will not restart!", e);
                        }

                        try
                        {
                            Process.GetCurrentProcess().Kill();
                        }
                        catch (Exception e)
                        {
                            ReLogger.Error($"Unable to kill current process, this current VRChat will remain open unless closed by the user!", e);
                        }

                    }, "Restart Later", delegate
                    {
                        GeneralWrapper.ClosePopup();
                    });
            }
            else
            {
                _HWIDSpoof.Value = false;
                MelonPreferences.Save();
            }
        }
    }
}
