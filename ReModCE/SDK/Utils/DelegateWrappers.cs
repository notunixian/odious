using System;
using System.Linq;
using System.Reflection;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using ReMod.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ReModCE.SDK.Utils
{
	internal static class DelegateWrappers
    {
        internal delegate void ShowUiInputPopupAction(string title, string initialText, InputField.InputType inputType, bool isNumeric, string confirmButtonText, Il2CppSystem.Action<string, List<KeyCode>, Text> onComplete, Il2CppSystem.Action onCancel, string placeholderText = "Enter text...", bool closeAfterInput = true, Il2CppSystem.Action<VRCUiPopup> onPopupShown = null, bool unknownBool = false, int characterLimit = 0);

        internal static ShowUiInputPopupAction ourShowUiInputPopupAction;

        internal static ShowUiInputPopupAction ShowUiInputPopup
        {
            get
            {
                if (ourShowUiInputPopupAction != null)
                {
                    return ourShowUiInputPopupAction;
                }
                MethodInfo method = (from mb in typeof(VRCUiPopupManager).GetMethods()
                    where mb.Name.StartsWith("Method_Public_Void_String_String_InputType_Boolean_String_Action_3_String_List_1_KeyCode_Text_Action_String_Boolean_Action_1_VRCUiPopup_Boolean_Int32_") && XrefUtils.CheckMethod(mb, "UserInterface/MenuContent/Popups/InputPopup")
                    select mb).First();
                ourShowUiInputPopupAction = (ShowUiInputPopupAction)System.Delegate.CreateDelegate(typeof(ShowUiInputPopupAction), VRCUiPopupManager.prop_VRCUiPopupManager_0, method);
                return ourShowUiInputPopupAction;
            }
        }
    }
}
