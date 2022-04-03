using System;
using System.Collections.Generic;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using ReModCE.EvilEyeSDK;
using UnityEngine;
using UnityEngine.UI;
using VRC.Core;
using VRC.SDKBase;

namespace ReModCE.SDK
{
	internal class WorldUtils
	{
		private static GameObject voidClubAnnoyingEntryDoor;

		private static GameObject blindHud;

		private static GameObject flashbangHud;

		private static GameObject basementDoor;

		private static GameObject kitchenDoor;

		private static GameObject kitchenDoorChrome;

		private static GameObject kitchenDoorGlass;

		private static GameObject kitchenDoorCollider;

		private static GameObject annoyingIntro;

		internal static bool isAmongUsGame;

		internal static bool isJustBClub;

		internal static bool isFBTHeaven;

		internal static void GoToWorld(string roomID, Il2CppSystem.Collections.Generic.List<KeyCode> pressedKeys, Text text)
		{
			GoToWorld(roomID);
		}

		internal static void GoToWorld(string roomID, bool logErrorToHud = true)
		{
			string roomIDSanitized = roomID.Trim();
			string[] array = roomIDSanitized.Split(':');
			if (array.Length != 2)
			{
                return;
			}
			if (!IsInstanceValid(array[1], logErrorToHud))
			{
                return;
			}
			int num = array[1].IndexOf('~');
			string worldInstanceId = ((num != -1) ? array[1].Substring(0, num) : array[1]);
			IsWorldValid(array[0], worldInstanceId, delegate (ApiWorld world, string instanceId)
			{
				if (Networking.GoToRoom(roomIDSanitized))
				{
					MelonLogger.Msg("Joining world: " + world.id + " (Instance: " + instanceId + ")");
				}
				else
				{
                    MelonLogger.Msg("Failed joining world: " + world.id + " (Instance: " + instanceId + ")");
                }
			}, delegate (string reason)
			{
                MelonLogger.Msg("Can't go to world (" + reason + ")");
            });
		}

		internal static bool IsInstanceValid(string instanceId, bool logErrorToHud)
		{
			if (instanceId.Length > 200)
			{
                MelonLogger.Msg("Can't go to world due to invalid instance id");
                return false;
			}
			return true;
		}

		internal static bool IsWorldValid(string worldId, string worldInstanceId, System.Action<ApiWorld, string> onSuccess, System.Action<string> onFailed = null)
		{
			if (string.IsNullOrEmpty(worldId))
			{
				onFailed?.Invoke("WorldID is null or empty");
				return false;
			}
			if (string.IsNullOrEmpty(worldInstanceId))
			{
				onFailed?.Invoke("WorldInstanceID is null or empty");
				return false;
			}
			if (worldId.Length != 41 || !worldId.StartsWith("wrld_") || worldId[13] != '-' || worldId[18] != '-' || worldId[23] != '-' || worldId[28] != '-')
			{
				onFailed?.Invoke("WorldID failed sanity check");
				return false;
			}
			API.Fetch<ApiWorld>(worldId, (System.Action<ApiContainer>)delegate (ApiContainer apiContainer)
			{
				onSuccess(apiContainer.Model.Cast<ApiWorld>(), worldInstanceId);
			}, (System.Action<ApiContainer>)delegate (ApiContainer apiContainer)
			{
				onFailed?.Invoke(apiContainer.Error);
			});
			return true;
		}

		internal static void DropPortal(string worldId, string instanceId, int playerCount = 0, float portalTime = 30f)
        {
            var player = PlayerWrapper.LocalVRCPlayer();
			Vector3 position = player.transform.position + player.transform.forward * 2f;
			Quaternion rotation = player.transform.rotation;
			DropPortal(worldId, instanceId, playerCount, portalTime, position, rotation);
		}

		internal static void DropPortal(string worldId, string instanceId, int playerCount, float portalTime, Vector3 position, Quaternion rotation)
		{
			GameObject gameObject = Networking.Instantiate(VRC_EventHandler.VrcBroadcastType.Always, "Portals/PortalInternalDynamic", position, rotation);
			Networking.RPC(RPC.Destination.AllBufferOne, gameObject, "ConfigurePortal", new Il2CppSystem.Object[3]
			{
				(Il2CppSystem.String)worldId,
				(Il2CppSystem.String)instanceId,
				new Il2CppSystem.Int32
				{
					m_value = playerCount
				}.BoxIl2CppObject()
			});
			gameObject.GetComponent<PortalInternal>().field_Private_Single_1 = 0f - portalTime - 30f;
		}

		internal static ApiWorld GetCurrentWorld()
		{
			return RoomManager.field_Internal_Static_ApiWorld_0;
		}

		internal static ApiWorldInstance GetCurrentInstance()
		{
			return RoomManager.field_Internal_Static_ApiWorldInstance_0;
		}

		internal static void CacheAnnoyingGameObjects()
		{
			voidClubAnnoyingEntryDoor = GameObject.Find("Cyber_Entrance/Cyberpunk_street0.9/Building_lift.006");
			blindHud = GameObject.Find("Game Logic/Player HUD/Blind HUD Anim");
			flashbangHud = GameObject.Find("Game Logic/Player HUD/Flashbang HUD Anim");
			basementDoor = GameObject.Find(" - Props/Props (Static) - Hallways - First Floor/door-private");
			kitchenDoor = GameObject.Find("great_pug/kitchen_door");
			kitchenDoorChrome = GameObject.Find("great_pug/kitchen_door_chrome");
			kitchenDoorGlass = GameObject.Find("great_pug/Cube_022  (GLASS)");
			kitchenDoorCollider = GameObject.Find("great_pug/door-frame_004");
			annoyingIntro = GameObject.Find("Lobby/Entrance Corridor/Cancer Spawn");
		}

		internal static void FixVoidClubAnnoyingEntryDoor(bool enableFix)
		{
			if (voidClubAnnoyingEntryDoor != null)
			{
				voidClubAnnoyingEntryDoor.SetActive(!enableFix);
			}
		}

		internal static void FixMurderAntiKillscreen(bool enableFix)
		{
			if (blindHud != null)
			{
				blindHud.transform.localScale = (enableFix ? new Vector3(0f, 0f, 0f) : new Vector3(1f, 1f, 1f));
			}
			if (flashbangHud != null)
			{
				flashbangHud.transform.localScale = (enableFix ? new Vector3(0f, 0f, 0f) : new Vector3(1f, 1f, 1f));
			}
		}

		internal static void FixUnnecessaryDoorsInTheGreatPug(bool enableFix)
		{
			if (basementDoor != null)
			{
				basementDoor.SetActive(!enableFix);
			}
			if (kitchenDoor != null)
			{
				kitchenDoor.SetActive(!enableFix);
			}
			if (kitchenDoorChrome != null)
			{
				kitchenDoorChrome.SetActive(!enableFix);
			}
			if (kitchenDoorGlass != null)
			{
				kitchenDoorGlass.SetActive(!enableFix);
			}
			if (kitchenDoorCollider != null)
			{
				kitchenDoorCollider.GetComponent<BoxCollider>().enabled = !enableFix;
			}
		}

		internal static void FixAnnoyingIntroInJustBClub(bool enableFix)
		{
			if (annoyingIntro != null)
			{
				annoyingIntro.SetActive(!enableFix);
			}
		}
	}
}
