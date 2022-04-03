using System.Collections;
using MelonLoader;
using ReMod.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReModCE.Components
{
    internal class UILoggerComponent : ModComponent
    {
		private static string ClientName = "Odious";

		private static string PrimaryColour = "#8C99E1";

		private static Color SecondaryColour = new Color(0f, 1f, 1f, 1f);

		private static Vector3 UIPosition = new Vector3(-20f, -300f, 0f);

		private static float TextSpacing = 20f;

		public static IEnumerator MakeUI()
		{
			while (RoomManager.field_Internal_Static_ApiWorld_0 == null)
			{
				yield return new WaitForSeconds(1f);
			}
			GameObject gameObject;
			GameObject GUI = (gameObject = GameObject.Find("/UserInterface").transform.Find("UnscaledUI/HudContent/Hud/AlertTextParent/Capsule").gameObject);
			gameObject.SetActive(value: true);
			GameObject text = GUI.transform.Find("Text").gameObject;
			yield return new WaitForEndOfFrame();
			GUI.transform.localPosition = UIPosition;
			Object.DestroyImmediate(GUI.transform.GetComponent<HorizontalLayoutGroup>());
			Object.DestroyImmediate(GUI.transform.GetComponent<ContentSizeFitter>());
			Object.DestroyImmediate(GUI.transform.GetComponent<ContentSizeFitter>());
			GUI.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			GUI.gameObject.AddComponent<VerticalLayoutGroup>().spacing = TextSpacing;
			TextMeshProUGUI component = text.GetComponent<TextMeshProUGUI>();
			component.color = SecondaryColour;
			component.alignment = TextAlignmentOptions.Left;
			component.enableWordWrapping = true;
			component.isOverlay = true;
			component.text = "<color=" + PrimaryColour + ">[" + ClientName + "]</color> ";
			yield return new WaitForEndOfFrame();
			text.SetActive(value: false);
		}

		public static void Msg(string Text, float Timer)
		{
			MelonCoroutines.Start(DoText(Text, 1, Timer));
		}

		public static void Error(string Text, float Timer)
		{
			MelonCoroutines.Start(DoText(Text, 2, Timer));
		}

		public static void Warn(string Text, float Timer)
		{
			MelonCoroutines.Start(DoText(Text, 3, Timer));
		}

		private static IEnumerator DoText(string Text, int TextType, float TimeBeforeDeletion)
		{
			GameObject textObj = Object.Instantiate(GameObject.Find("UserInterface/UnscaledUI/HudContent/Hud/AlertTextParent/Capsule/Text").gameObject, GameObject.Find("UserInterface/UnscaledUI/HudContent/Hud/AlertTextParent/Capsule").transform);
			TextMeshProUGUI component;
			string text = (component = textObj.GetComponent<TextMeshProUGUI>()).text;
			component.text = text + TextType switch
			{
				1 => "",
				2 => "<color=red>[ERROR]</color> ",
				3 => "<color=yellow>[Warning]</color> ",
				_ => "Waa waaa I broke something",
			} + Text;
			textObj.SetActive(value: true);
			yield return new WaitForSeconds(TimeBeforeDeletion);
			Object.Destroy(textObj);
		}
	}
}
