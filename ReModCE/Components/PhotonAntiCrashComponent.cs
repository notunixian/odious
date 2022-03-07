using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using MelonLoader;
using Newtonsoft.Json;
using Photon.Realtime;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using UnhollowerBaseLib;
using VRC;

namespace ReModCE.Components
{
    internal class PhotonAntiCrashComponent : ModComponent
    {
        private ConfigValue<bool> PhotonAntiEnabled;
        private static ReMenuToggle _PhotonAntiToggled;
        private static MelonPreferences_Entry<bool> _PhotonProtection;


        public PhotonAntiCrashComponent()
        {
            PhotonAntiEnabled = new ConfigValue<bool>(nameof(PhotonAntiEnabled), true);
            PhotonAntiEnabled.OnValueChanged += () => _PhotonAntiToggled.Toggle(PhotonAntiEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var exploitsMenu = uiManager.MainMenu.GetCategoryPage("Safety").GetCategory("Photon Events");
            _PhotonAntiToggled = exploitsMenu.AddToggle("Photon Anti-Crash", "Attempts to prevent against maliciously crafted Photon events.",  b => { PhotonAntiEnabled.SetValue(b); PhotonAntiCrash(b); }, PhotonAntiEnabled);
        }

        // was thinking of doing requi's method of overriding functions but i'm too lazy to redo my patch for onevent
        public void PhotonAntiCrash(bool enabled)
        {
            var category = MelonPreferences.GetCategory("ReModCE");
            _PhotonProtection = (MelonPreferences_Entry<bool>)category.GetEntry("PhotonProtection");

            if (enabled)
            {
                _PhotonProtection.Value = true;
                MelonPreferences.Save();
            }
            else
            {
                _PhotonProtection.Value = false;
                MelonPreferences.Save();
            }
        }
    }
}
