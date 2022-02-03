using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRC;
using VRC.Core;
using VRC.SDKBase;
using VRC.Udon;
using ReModCE.Core;

namespace ReModCE.EvilEyeSDK
{
	internal class WorldWrapper
	{
		// Token: 0x06000102 RID: 258 RVA: 0x00007132 File Offset: 0x00005332
		public static string GetInstance()
		{
			return PlayerWrapper.LocalPlayer().GetAPIUser().instanceId;
		}

		// Token: 0x06000103 RID: 259 RVA: 0x00007143 File Offset: 0x00005343
		public static string GetID()
		{
			return WorldWrapper.CurrentWorld().id;
		}

		// Token: 0x06000104 RID: 260 RVA: 0x0000714F File Offset: 0x0000534F
		public static string GetLocation()
		{
			return PlayerWrapper.LocalPlayer().GetAPIUser().location;
		}

		// Token: 0x06000105 RID: 261 RVA: 0x00007160 File Offset: 0x00005360
		public static ApiWorld CurrentWorld()
		{
			return RoomManager.field_Internal_Static_ApiWorld_0;
		}

		// Token: 0x06000106 RID: 262 RVA: 0x00007167 File Offset: 0x00005367
		public static ApiWorldInstance CurrentWorldInstance()
		{
			return RoomManager.field_Internal_Static_ApiWorldInstance_0;
		}

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x06000107 RID: 263 RVA: 0x0000716E File Offset: 0x0000536E
		public static string GetWorldID
		{
			get
			{
				return PlayerWrapper.LocalPlayer().GetAPIUser().location;
			}
		}

		// Token: 0x06000108 RID: 264 RVA: 0x00007180 File Offset: 0x00005380
		public static void Init()
		{
			WorldWrapper.vrc_Pickups = UnityEngine.Object.FindObjectsOfType<VRC_Pickup>();
			WorldWrapper.udonBehaviours = UnityEngine.Object.FindObjectsOfType<UdonBehaviour>();
			WorldWrapper.vrc_Triggers = UnityEngine.Object.FindObjectsOfType<VRC_Trigger>();
			PlayerWrapper.PlayersActorID = new Dictionary<int, Player>();
			for (int i = 0; i < ReModCE.OnWorldInitEventArray.Length; i++)
			{
                ReModCE.OnWorldInitEventArray[i].OnWorldInit();
			}
		}

		// Token: 0x04000076 RID: 118
		public static VRC_Pickup[] vrc_Pickups;

		// Token: 0x04000077 RID: 119
		public static UdonBehaviour[] udonBehaviours;

		// Token: 0x04000078 RID: 120
		public static VRC_Trigger[] vrc_Triggers;
	}
}
