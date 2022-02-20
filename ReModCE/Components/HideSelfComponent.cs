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
            _HideSelfToggled = aviMenu.AddToggle("Hide Self", "Prevents download manager, meaning no avatars will load until you turn this off. (fuck you charlie)", PerformHide, _HideSelfEnabled);
        }
        
        // thanks area51
        public static void ClearAssets()
        {
            AssetBundleDownloadManager.field_Private_Static_AssetBundleDownloadManager_0.field_Private_Cache_0.ClearCache();
            AssetBundleDownloadManager.field_Private_Static_AssetBundleDownloadManager_0.field_Private_Queue_1_AssetBundleDownload_0.Clear();
            AssetBundleDownloadManager.field_Private_Static_AssetBundleDownloadManager_0.field_Private_Queue_1_AssetBundleDownload_1.Clear();
        }

        public void PerformHide(bool value)
        {
            _HideSelfEnabled = value;
            _HideSelfToggled?.Toggle(value);

            if (_HideSelfEnabled)
            {
                AssetBundleDownloadManager.field_Private_Static_AssetBundleDownloadManager_0.gameObject.SetActive(false);
                ClearAssets();
            }

            if (!_HideSelfEnabled)
            {
                ClearAssets();
                AssetBundleDownloadManager.field_Private_Static_AssetBundleDownloadManager_0.gameObject.SetActive(true);
            }
        }
    }
}
