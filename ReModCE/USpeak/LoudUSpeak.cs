using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReModCE.EvilEyeSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReModCE.USpeak
{
    class LoudUSpeak : ModComponent
    {
        public bool _LoudUSpeakEnabled;
        private static ReMenuToggle _LoudUSpeakToggled;
        public LoudUSpeak()
        {
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var exploitMenu = uiManager.MainMenu.GetCategoryPage("Exploits").GetCategory("USpeak");
            _LoudUSpeakToggled = exploitMenu.AddToggle("Loud Microphone", "Sets USpeaker volume to a high value, causing your microphone to become distorted/loud.", PerformLoud, _LoudUSpeakEnabled);
        }

        public void PerformLoud(bool value)
        {
            _LoudUSpeakEnabled = value;
            _LoudUSpeakToggled?.Toggle(value);

            if (_LoudUSpeakEnabled)
            {
                USpeaker.field_Internal_Static_Single_1 = float.MaxValue;
            }
            if (!_LoudUSpeakEnabled)
            {
                USpeaker.field_Internal_Static_Single_1 = 1;
            }
        }
    }
}
