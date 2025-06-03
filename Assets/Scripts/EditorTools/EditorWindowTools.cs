using UnityEditor;
using UnityEngine;

namespace EditorTools
{
	public class EditorWindowTools : MonoBehaviour
	{
		public static void DrawDivider(int thickness = 1, int padding = 30)
		{
			Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
			r.height = thickness;
			r.y += padding / 2;
			r.x -= 2;
			r.width += 6;
			EditorGUI.DrawRect(r, Color.grey);
		}

		public static void DrawHeader(string text)
		{
			GUIStyle style = EditorStyles.largeLabel;
			style.fontStyle = FontStyle.Bold;

			DrawLabel(text, style);
		}

		public static void DrawLabel(string text, GUIStyle style)
		{
			Vector2 size = style.CalcSize(new GUIContent(text));
			Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(size.y * 2));

			EditorGUI.LabelField(r, text, style);
		}
	}
}
