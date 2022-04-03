using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.VRChat;
using ReModCE.EvilEyeSDK;
using ReModCE.SDK;
using ReModCE.SDK.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ReModCE.Components
{
    internal class CloneByComponent : ModComponent
    {
        private ReMenuButton CloneByID;
        public CloneByComponent()
        {
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var aviMenu = uiManager.MainMenu.GetMenuPage("Avatars");
            CloneByID = aviMenu.AddButton("Clone By ID", "Clones an avatar by the Avatar ID.", () =>
            {
                GeneralWrapper.ShowInputPopup("Enter Avatar ID", string.Empty, InputField.InputType.Standard, false,
                    "Confirm", ChangeAvatarByID);
            });
        }

        private void ChangeAvatarByID(string avatarId, Il2CppSystem.Collections.Generic.List<KeyCode> pressedKeys, Text text)
        {
            PlayerUtils.ChangePlayerAvatar(avatarId.Trim(), logErrorOnHud: true);
        }
    }
}
