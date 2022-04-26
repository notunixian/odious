using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReModCE.EvilEyeSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReMod.Core.VRChat;

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
            _HideSelfToggled = aviMenu.AddToggle("Hide Self", "Prevents download manager, meaning no avatars will load until you turn this off. (fuck you charlie)", PerformHide, _HideSelfEnabled);
        }

        public void PerformHide(bool value)
        {
            _HideSelfEnabled = value;
            _HideSelfToggled?.Toggle(value);

            if (_HideSelfEnabled)
            {
                AssetBundleDownloadManager.field_Private_Static_AssetBundleDownloadManager_0.gameObject.SetActive(false);
                PlayerWrapper.LocalVRCPlayer().prop_VRCAvatarManager_0.gameObject.SetActive(false);
            }

            if (!_HideSelfEnabled)
            {
                AssetBundleDownloadManager.field_Private_Static_AssetBundleDownloadManager_0.gameObject.SetActive(true);
                PlayerWrapper.LocalVRCPlayer().prop_VRCAvatarManager_0.gameObject.SetActive(true);
                PlayerExtensions.ReloadAvatar(PlayerWrapper.LocalVRCPlayer());
            }
        }
    }
}
