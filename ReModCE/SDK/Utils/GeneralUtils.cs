using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using VRC.Core;

namespace ReModCE.SDK
{
    internal class GeneralUtils
    {
        internal static readonly ApiAvatar robotAvatar = new ApiAvatar
        {
            id = "avtr_c38a1615-5bf5-42b4-84eb-a8b6c37cbd11",
            name = "Robot",
            releaseStatus = "public",
            assetUrl = "https://api.vrchat.cloud/api/1/file/file_3c521ce5-e662-4a5d-a2f1-d9088cfde086/1/file",
            version = 1,
            authorName = "vrchat",
            authorId = "8JoV9XEdpo",
            description = "Beep Boop",
            thumbnailImageUrl = "https://api.vrchat.cloud/api/1/file/file_0e8c4e32-7444-44ea-ade4-313c010d4bae/1/file"
        };

		internal static List<T> FindAllComponentsInGameObject<T>(GameObject gameObject, bool includeInactive = true, bool searchParent = true, bool searchChildren = true) where T : class
		{
			List<T> list = new List<T>();
			if (gameObject == null)
			{
				return list;
			}
			try
			{
				foreach (T component in gameObject.GetComponents<T>())
				{
					list.Add(component);
				}
				if (searchParent && gameObject.transform.parent != null)
				{
					foreach (T item in gameObject.GetComponentsInParent<T>(includeInactive))
					{
						list.Add(item);
					}
				}
				if (searchChildren && gameObject.transform.childCount > 0)
				{
					foreach (T componentsInChild in gameObject.GetComponentsInChildren<T>(includeInactive))
					{
						list.Add(componentsInChild);
					}
				}
			}
			catch (Exception e)
			{
				MelonLogger.Error("FindAllComponentsInGameObject", e);
			}
			return list;
		}

		internal static Vector3 GetNameplateOffset(bool open)
		{
            return open ? new Vector3(0f, 60f, 0f) : new Vector3(0f, 30f, 0f);
		}

		internal static int GetChildDepth(Transform child, Transform parent)
		{
			int num = 0;
			if (child == parent)
			{
				return num;
			}
			while (child.parent != null)
			{
				num++;
				if (child.parent == parent)
				{
					return num;
				}
				child = child.parent;
			}
			return -1;
		}

		internal static bool IsInvalid(Vector3 vector)
		{
			return float.IsNaN(vector.x) || float.IsInfinity(vector.x) || float.IsNaN(vector.y) || float.IsInfinity(vector.y) || float.IsNaN(vector.z) || float.IsInfinity(vector.z);
		}

		internal static bool IsInvalid(Quaternion quaternion)
		{
			return float.IsNaN(quaternion.x) || float.IsInfinity(quaternion.x) || float.IsNaN(quaternion.y) || float.IsInfinity(quaternion.y) || float.IsNaN(quaternion.z) || float.IsInfinity(quaternion.z) || float.IsNaN(quaternion.w) || float.IsInfinity(quaternion.w);
		}

		internal static int Clamp(int value, int min, int max)
		{
			if (value < min)
			{
				return min;
			}
			if (value > max)
			{
				return max;
			}
			return value;
		}

		internal static float Clamp(float value, float min, float max)
		{
			if (value < min)
			{
				return min;
			}
			if (value > max)
			{
				return max;
			}
			return value;
		}

		internal static short Clamp(short value, short min, short max)
		{
			if (value < min)
			{
				return min;
			}
			if (value > max)
			{
				return max;
			}
			return value;
		}

		internal static sbyte Clamp(sbyte value, sbyte min, sbyte max)
		{
			if (value < min)
			{
				return min;
			}
			if (value > max)
			{
				return max;
			}
			return value;
		}

		internal static bool IsBeyondLimit(Vector3 vector, float lowerLimit, float higherLimit)
		{
			if (vector.x < lowerLimit || vector.x > higherLimit || vector.y < lowerLimit || vector.y > higherLimit || vector.z < lowerLimit || vector.z > higherLimit)
			{
				return true;
			}
			return false;
		}

		internal static string RemoveCharacterFromString(string text, char character)
		{
			char[] array = new char[text.Length];
			int num = 0;
			foreach (char c in text)
			{
				if (c != character)
				{
					array[num] = c;
					num++;
				}
			}
			return new string(array, 0, num);
		}

        internal static void RemoveAvatarFromCache(string avatarId)
        {
            AssetBundleDownloadManager assetBundleDownloadManager = AssetBundleDownloadManager.prop_AssetBundleDownloadManager_0;
            for (int i = 0; i < assetBundleDownloadManager.field_Private_Queue_1_AssetBundleDownload_0.Count; i++)
            {
                AssetBundleDownload assetBundleDownload = assetBundleDownloadManager.field_Private_Queue_1_AssetBundleDownload_0.Dequeue();
                if (assetBundleDownload.field_Private_String_0 != avatarId)
                {
                    assetBundleDownloadManager.field_Private_Queue_1_AssetBundleDownload_0.Enqueue(assetBundleDownload);
                }
            }
            Resources.UnloadUnusedAssets();
        }
	}
}
