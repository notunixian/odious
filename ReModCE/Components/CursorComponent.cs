using ReMod.Core;
using ReMod.Core.Managers;
using ReMod.Core.UI.QuickMenu;
using ReMod.Core.Unity;
using ReMod.Core.VRChat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ReModCE.Components
{
    internal class CursorColorComponent : ModComponent
    {
        public ConfigValue<bool> _CursorColorEnabled;
        private static ReMenuToggle _CursorColorToggled;

        private ConfigValue<Color> CursorColor;
        private ReMenuButton _CursorColorButton;

        public CursorColorComponent()
        {
            Color color = new Color(0, 0.848f, 1, 1);
            CursorColor = new ConfigValue<Color>(nameof(CursorColor), color);

            _CursorColorEnabled = new ConfigValue<bool>(nameof(_CursorColorEnabled), false);
            _CursorColorEnabled.OnValueChanged += () => _CursorColorToggled.Toggle(_CursorColorEnabled);
        }

        public override void OnUiManagerInit(UiManager uiManager)
        {
            var visualMenu = uiManager.MainMenu.GetCategoryPage("Visuals").GetCategory("Cursor");
            _CursorColorToggled = visualMenu.AddToggle("Cursor Color", "Allows you to set a VRChat custom cursor color.", b => { _CursorColorEnabled.SetValue(b); PerformCursorColor(b); }, _CursorColorEnabled);

            _CursorColorButton = visualMenu.AddButton($"<color=#{CursorColor.Value.ToHex()}>Cursor</color> Color",
                $"Set your VRChat cursor color",
                () =>
                {
                    PopupColorInput(_CursorColorButton, CursorColor);
                }, ResourceManager.GetSprite("remodce.palette"));
        }

        private void PopupColorInput(ReMenuButton button, ConfigValue<Color> configValue)
        {
            VRCUiPopupManager.prop_VRCUiPopupManager_0.ShowInputPopupWithCancel("Input hex color code",
                $"#{configValue.Value.ToHex()}", InputField.InputType.Standard, false, "Submit",
                (s, k, t) =>
                {
                    if (string.IsNullOrEmpty(s))
                        return;

                    if (!ColorUtility.TryParseHtmlString(s, out var color))
                        return;

                    configValue.SetValue(color);

                    button.Text = $"<color=#{configValue.Value.ToHex()}>Cursor</color> Color";
                }, null);
        }

        public void PerformCursorColor(bool value)
        {
            if(value)
            {
                GameObject Cursor = GameObject.Find("_Application/CursorManager/MouseArrow/VRCUICursorIcon");
                MelonLoader.MelonLogger.Log(Cursor.GetComponent<SpriteRenderer>().color.ToString());
                Cursor.GetComponent<SpriteRenderer>().color = CursorColor;
            }
            
            if(!value)
            {
                GameObject Cursor = GameObject.Find("_Application/CursorManager/MouseArrow/VRCUICursorIcon");

                MelonLoader.MelonLogger.Msg($"[PerformColorCursor] cursor being reset!");
                MelonLoader.MelonLogger.Log($"[PerformColorCursor] Color before: {Cursor.GetComponent<SpriteRenderer>().color.ToString()}");

                // unity told me this was the original one, so fuck you unity if this is wrong.
                Cursor.GetComponent<SpriteRenderer>().color = new Color(0, 0.848f, 1, 1);
                MelonLoader.MelonLogger.Log($"[PerformColorCursor] Color after reset: {Cursor.GetComponent<SpriteRenderer>().color.ToString()}");
            }
        }

        public override void OnPreferencesSaved()
        {
            if(!_CursorColorEnabled)
            {
                return;
            }

            GameObject Cursor = GameObject.Find("_Application/CursorManager/MouseArrow/VRCUICursorIcon");
            MelonLoader.MelonLogger.Msg($"[OnPreferencesSaved] Setting color cursor since preferences have changed!");
            Cursor.GetComponent<SpriteRenderer>().color = CursorColor;
        }
    }
}
