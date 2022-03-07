using ExitGames.Client.Photon;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2CppSystem.IO;
using Photon.Pun;
using Photon.Realtime;
using Object = Il2CppSystem.Object;

namespace ReModCE.PhotonExtension
{
    internal static class PhotonExtensions
    {
		public static void OpRaiseEvent(byte code, object customObject, RaiseEventOptions RaiseEventOptions, SendOptions sendOptions)
		{
			Object Object = Serialization.FromManagedToIL2CPP<Il2CppSystem.Object>(customObject);
			PhotonExtensions.OpRaiseEvent(code, Object, RaiseEventOptions, sendOptions);
		}

		public static void OpRaiseEvent(byte code, Object customObject, RaiseEventOptions RaiseEventOptions, SendOptions sendOptions)
		{
			PhotonNetwork.Method_Private_Static_Boolean_Byte_Object_RaiseEventOptions_SendOptions_0(code, (Il2CppSystem.Object)customObject, RaiseEventOptions, sendOptions);
		}
	}
}
