using MelonLoader;
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
    class LowBitRateUSpeak : ModComponent
    {
        public bool _LowBitRateUSpeakEnabled;
        private static ReMenuToggle _LowBitRateUSpeakToggled;
        public LowBitRateUSpeak()
        {
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var exploitMenu = uiManager.MainMenu.GetCategoryPage("Exploits").GetCategory("USpeak");
            _LowBitRateUSpeakToggled = exploitMenu.AddToggle("Low BitRate Microphone", "Sets USpeaker bitrate to be 8kpb/s instead of the default 24kpbs/s, causing your microphone to sound low quality.", PerformLowBitRate, _LowBitRateUSpeakEnabled);
        }

        public void PerformLowBitRate(bool value)
        {
            _LowBitRateUSpeakEnabled = value;
            _LowBitRateUSpeakToggled?.Toggle(value);

            if (_LowBitRateUSpeakEnabled)
            {
                PlayerWrapper.LocalPlayer().GetUspeaker().field_Public_BitRate_0 = BitRate.BitRate_8K;
            }
            if (!_LowBitRateUSpeakEnabled)
            {
                PlayerWrapper.LocalPlayer().GetUspeaker().field_Public_BitRate_0 = BitRate.BitRate_24K;
            }
        }
    }
}
