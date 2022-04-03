using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using RealisticEyeMovements;
using ReModCE.Loader;
using RootMotion.FinalIK;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.Management;
using VRC.SDKBase;
using VRC.UI;
using VRCSDK2;

namespace ReModCE.SDK.Utils
{
    internal class PlayerUtils
    {

        internal static void IsAvatarValid(string avatarId, Action<ApiAvatar> onSuccess, Action<string> onFailed = null)
        {
            if (string.IsNullOrEmpty(avatarId))
            {
                onFailed?.Invoke("Empty Avatar Id");
                return;
            }
            if (avatarId.Length != 41 || !avatarId.StartsWith("avtr_") || avatarId[13] != '-' || avatarId[18] != '-' || avatarId[23] != '-' || avatarId[28] != '-')
            {
                onFailed?.Invoke("Invalid Avatar Id");
                return;
            }
            ApiAvatar apiAvatar = new ApiAvatar();
            apiAvatar.id = avatarId;
            apiAvatar.Get((Action<ApiContainer>)delegate (ApiContainer x)
            {
                onSuccess(x.Model.Cast<ApiAvatar>());
            }, (Action<ApiContainer>)delegate (ApiContainer x)
            {
                onFailed?.Invoke(x.Error);
            });
        }

        internal static void ChangePlayerAvatar(string avatarId, bool logErrorOnHud)
        {
            IsAvatarValid(avatarId, delegate (ApiAvatar avatar)
            {
                GeneralWrapper.GetPageAvatar().field_Public_SimpleAvatarPedestal_0.field_Internal_ApiAvatar_0 = avatar;
                GeneralWrapper.GetPageAvatar().ChangeToSelectedAvatar();
            }, delegate (string error)
            {
                ReLogger.Msg("Failed to switch to avatar: (" + error + ")");
                if (logErrorOnHud)
                {
                    GeneralWrapper.AlertPopup("Reuploader", "Failed to switch to avatar: (" + error + ")");
                }
            });
        }
	}
}
