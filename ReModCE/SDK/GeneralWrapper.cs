using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib.Public.Patching;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using ReMod.Core;
using ReModCE.Loader;
using ReModCE.SDK.Utils;
using UnhollowerRuntimeLib.XrefScans;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using VRC;
using VRC.UI;
using VRC.UI.Elements.Menus;
using VRC.UserCamera;
using VRCSDK2;

namespace ReModCE.SDK
{
    internal class GeneralWrapper
    {
		private static Camera uiCamera;

		private static Camera photoCamera;

		private static PageAvatar pageAvatar;

		private static PageUserInfo pageUserInfo;

		private static PageWorldInfo pageWorldInfo;

		private static SelectedUserMenuQM selectedUserManager;

		private static VRC_SyncVideoPlayer worldVideoPlayer;

		internal static FastMethodInfo closeMenuFunction;

		internal static FastMethodInfo closePopupFunction;

		internal static FastMethodInfo alertActionFunction;

		internal static FastMethodInfo alertPopupFunction;

		internal static FastMethodInfo reloadPlayerAvatarFunction;

		internal static FastMethodInfo loadAvatarFunction;

		private static GameObject socialMenuModerationButton;

		private static GameObject avatarPreviewBase;

		private static GameObject reticleObj;

		internal static void InitializeWrappers()
		{
			closePopupFunction = new FastMethodInfo((from mb in typeof(VRCUiPopupManager).GetMethods()
													 where mb.Name.StartsWith("Method_Public_Void_") && mb.Name.Length <= 21 && !mb.Name.Contains("PDM") && XrefUtils.CheckMethod(mb, "POPUP")
													 select mb).First());
			closeMenuFunction = new FastMethodInfo((from mb in typeof(VRCUiManager).GetMethods()
													where mb.Name.StartsWith("Method_Public_Void_Boolean_Boolean_") && XrefUtils.CheckUsedBy(mb, "ChangeToSelectedAvatar")
													select mb).First());
			alertActionFunction = new FastMethodInfo((from mb in typeof(VRCUiPopupManager).GetMethods()
													  where mb.Name.StartsWith("Method_Public_Void_String_String_String_Action_String_Action_Action_1_VRCUiPopup_") && !mb.Name.Contains("PDM") && XrefUtils.CheckMethod(mb, "UserInterface/MenuContent/Popups/StandardPopup")
													  select mb).Last());
			alertPopupFunction = new FastMethodInfo((from mb in typeof(VRCUiPopupManager).GetMethods()
													 where mb.Name.StartsWith("Method_Public_Void_String_String_Single_") && XrefUtils.CheckMethod(mb, "UserInterface/MenuContent/Popups/InformationPopup")
													 select mb).First());
			reloadPlayerAvatarFunction = new FastMethodInfo((from mb in typeof(VRCPlayer).GetMethods()
															 where mb.Name.StartsWith("Method_Public_Static_Void_APIUser_")
															 select mb).Last());
			loadAvatarFunction = new FastMethodInfo(typeof(VRCPlayer).GetMethods().First((MethodInfo mi) => mi.Name.StartsWith("Method_Private_Void_Boolean_") && mi.Name.Length < 31 && mi.GetParameters().Any((ParameterInfo pi) => pi.IsOptional) && XrefScanner.UsedBy(mi).Any((XrefInstance instance) => instance.Type == XrefType.Method && instance.TryResolve() != null && instance.TryResolve().Name == "ReloadAvatarNetworkedRPC")));
		}

		internal static VRC_SyncVideoPlayer GetWorldVideoPlayer()
		{
			if (worldVideoPlayer == null)
			{
				VRC_SyncVideoPlayer[] array = Resources.FindObjectsOfTypeAll<VRC_SyncVideoPlayer>();
				if (array != null)
				{
					worldVideoPlayer = ((array.Length != 0) ? array[0] : null);
				}
			}
			return worldVideoPlayer;
		}

		internal static GameObject GetAvatarPreviewBase()
		{
			if (avatarPreviewBase == null)
			{
				avatarPreviewBase = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/AvatarPreviewBase");
			}
			return avatarPreviewBase;
		}

		internal static GameObject GetSocialMenuModerationButton()
		{
			if (socialMenuModerationButton == null)
			{
				socialMenuModerationButton = GameObject.Find("UserInterface/MenuContent/Screens/UserInfo/Buttons/RightSideButtons/RightLowerButtonColumn/ModerateButton");
			}
			return socialMenuModerationButton;
		}

		internal static PlayerManager GetPlayerManager()
		{
			return PlayerManager.field_Private_Static_PlayerManager_0;
		}

		internal static PageAvatar GetPageAvatar()
		{
			if (pageAvatar == null)
			{
				pageAvatar = GameObject.Find("UserInterface/MenuContent/Screens/Avatar").GetComponent<PageAvatar>();
			}
			return pageAvatar;
		}

		internal static PageUserInfo GetPageUserInfo()
		{
			if (pageUserInfo == null)
			{
				pageUserInfo = GameObject.Find("UserInterface/MenuContent/Screens/UserInfo").GetComponent<PageUserInfo>();
			}
			return pageUserInfo;
		}

		internal static PageWorldInfo GetPageWorldInfo()
		{
			if (pageWorldInfo == null)
			{
				pageWorldInfo = GameObject.Find("UserInterface/MenuContent/Screens/WorldInfo").GetComponent<PageWorldInfo>();
			}
			return pageWorldInfo;
		}

		internal static SelectedUserMenuQM GetSelectedUserManager()
		{
			if (selectedUserManager == null)
			{
				selectedUserManager = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)/Container/Window/QMParent/Menu_SelectedUser_Local").GetComponent<SelectedUserMenuQM>();
			}
			return selectedUserManager;
		}

		internal static GameObject GetReticle()
		{
			if (reticleObj == null)
			{
				reticleObj = GameObject.Find("UserInterface/UnscaledUI/HudContent/Hud/ReticleParent");
			}
			return reticleObj;
		}

		internal static Camera GetPlayerCamera()
		{
			return VRCVrCamera.field_Private_Static_VRCVrCamera_0.field_Public_Camera_0;
		}

		internal static Camera GetUICamera()
		{
			if (uiCamera == null)
			{
				uiCamera = GetPlayerCamera().transform.Find("StackedCamera : Cam_InternalUI").GetComponent<Camera>();
			}
			return uiCamera;
		}

		internal static Camera GetPhotoCamera()
		{
			if (photoCamera == null)
			{
				photoCamera = UserCameraController.field_Internal_Static_UserCameraController_0.field_Public_GameObject_0.GetComponent<Camera>();
			}
			return photoCamera;
		}

		internal static VRCTrackingManager GetVRCTrackingManager()
		{
			return VRCTrackingManager.field_Private_Static_VRCTrackingManager_0;
		}

		internal static VRCUiManager GetVRCUiManager()
		{
			return VRCUiManager.prop_VRCUiManager_0;
		}

		internal static VRCUiPopupManager GetVRCUiPopupManager()
		{
			return VRCUiPopupManager.prop_VRCUiPopupManager_0;
		}

		internal static void EnableOutline(Renderer renderer, bool state)
		{
			HighlightsFX.Method_Public_Static_Void_Renderer_Boolean_PDM_0(renderer, state);
		}

        internal static void AlertPopup(string title, string text, float timeout = 10f)
		{
			MelonCoroutines.Start(AlertPopupEnumerator(title, text, timeout));
		}

		private static IEnumerator AlertPopupEnumerator(string title, string text, float timeout)
		{
			yield return new WaitForEndOfFrame();
			try
			{
				alertPopupFunction.Invoke(GetVRCUiPopupManager(), title, text, timeout);
			}
			catch (System.Exception e)
			{
				ReLogger.Error("AlertPopup", e);
			}
		}

		internal static void AlertAction(string title, string content, string button1Text, System.Action button1Action, string button2Text, System.Action button2Action)
		{
			try
			{
				alertActionFunction.Invoke(GetVRCUiPopupManager(), title, content, button1Text, (Il2CppSystem.Action)button1Action, button2Text, (Il2CppSystem.Action)button2Action, null);
			}
			catch (System.Exception e)
			{
                ReLogger.Error("AlertAction", e);
			}
		}

		internal static void CloseMenu()
		{
			try
			{
				closeMenuFunction.Invoke(GetVRCUiManager(), false, false);
			}
			catch (System.Exception e)
			{
                ReLogger.Error("CloseMenu", e);
			}
		}

		internal static void ClosePopup()
		{
			try
			{
				closePopupFunction.Invoke(GetVRCUiPopupManager(), null);
			}
			catch (System.Exception e)
			{
                ReLogger.Error("ClosePopup", e);
			}
		}

		internal static bool IsInVR()
		{
			return XRDevice.isPresent;
		}

		internal static void ShowInputPopup(string title, string initialText, InputField.InputType inputType, bool isNumeric, string confirmButtonText, System.Action<string, Il2CppSystem.Collections.Generic.List<KeyCode>, Text> onComplete, System.Action onCancel = null, string placeholderText = "Enter text...", bool closeAfterInput = true, System.Action<VRCUiPopup> onPopupShown = null, bool unknownBool = false, int characterLimit = 0)
		{
			DelegateWrappers.ShowUiInputPopup(title, initialText, inputType, isNumeric, confirmButtonText, onComplete, onCancel, placeholderText, closeAfterInput, onPopupShown, unknownBool, characterLimit);
		}
	}
}
