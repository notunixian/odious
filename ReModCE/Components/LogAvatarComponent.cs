using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;

namespace ReModCE.Components
{
    internal class LogAvatarComponent : ModComponent
    {
        private ConfigValue<bool> AvatarLogsEnabled;
        private ReMenuToggle _AvatarLogsToggle;
        private static MelonPreferences_Entry<bool> _LogAvi;

        public LogAvatarComponent()
        {
            AvatarLogsEnabled = new ConfigValue<bool>(nameof(AvatarLogsEnabled), false);
            AvatarLogsEnabled.OnValueChanged += () => _AvatarLogsToggle.Toggle(AvatarLogsEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            base.OnUiManagerInit(uiManager);

            var menu = uiManager.MainMenu.GetMenuPage("Logging");
            _AvatarLogsToggle = menu.AddToggle("Avatar Logs", "Enable whether avatar information (asset url, creator, avatar id, etc) in console.",  b => { AvatarLogsEnabled.SetValue(b); AvatarLog(b); }, AvatarLogsEnabled);
        }

        public void AvatarLog(bool enabled)
        {
            var category = MelonPreferences.GetCategory("ReModCE");
            _LogAvi = (MelonPreferences_Entry<bool>)category.GetEntry("LogAvi");

            if (enabled)
            {
                _LogAvi.Value = true;
                MelonPreferences.Save();
            }
            else
            {
                _LogAvi.Value = false;
                MelonPreferences.Save();
            }
        }
    }
}
