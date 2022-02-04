using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReModCE.EvilEyeSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReModCE.Components
{
    class HideSelfComponent : ModComponent
    {
        private string pastAviID;
        public bool _HideSelfEnabled;
        private static ReMenuToggle _HideSelfToggled;
        public HideSelfComponent()
        {
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var aviMenu = uiManager.MainMenu.GetMenuPage("Avatars");
            _HideSelfToggled = aviMenu.AddToggle("Hide Yourself", "Hides your avatar, so you can load crasher avatars or anything else.", PerformHide, _HideSelfEnabled);
        }

        public void PerformHide(bool value)
        {
            _HideSelfEnabled = value;
            _HideSelfToggled?.Toggle(value);

            if (_HideSelfEnabled)
            {
                pastAviID = PlayerWrapper.LocalVRCPlayer().GetAPIAvatar().id;
                PlayerWrapper.LocalPlayer().SetHide(true);
            }
            if (!_HideSelfEnabled)
            {
                PlayerWrapper.ChangeAvatar(pastAviID);
                PlayerWrapper.LocalPlayer().SetHide(false);
            }
        }
    }
}
