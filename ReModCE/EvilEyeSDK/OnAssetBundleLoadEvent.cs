using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ReModCE.EvilEyeSDK
{
	public interface OnAssetBundleLoadEvent
	{
		bool OnAvatarAssetBundleLoad(GameObject avatar, string avatarId);
	}
}
