using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MPUIKIT.Editor
{
	public class EditorGUILayoutExtended : UnityEditor.Editor
	{
		private static readonly Type editorGUIType = typeof(EditorGUI);

		private static readonly Type RecycledTextEditorType =
			Assembly.GetAssembly(editorGUIType).GetType("UnityEditor.EditorGUI+RecycledTextEditor");

		private static readonly Type[] argumentTypes =
		{
			RecycledTextEditorType, typeof(Rect), typeof(Rect), typeof(int), typeof(float), typeof(string),
			typeof(GUIStyle), typeof(bool)
		};

		private static readonly MethodInfo doFloatFieldMethod = editorGUIType.GetMethod("DoFloatField",
			BindingFlags.NonPublic | BindingFlags.Static, null, argumentTypes, null);

		private static readonly FieldInfo fieldInfo =
			editorGUIType.GetField("s_RecycledEditor", BindingFlags.NonPublic | BindingFlags.Static);


		private static readonly object recycledEditor = fieldInfo != null ? fieldInfo.GetValue(null) : null;
		private static readonly GUIStyle style = EditorStyles.numberField;


		private static int s_DragControlID = 0;
		private static bool s_IsDragging = false;
		private static float s_StartValue;
		private static float s_AccumulatedDelta = 0f;
		private const float s_BaseSensitivity = 0.5f; 



		public static float FloatFieldExtended(Rect _position, float _value, Rect _dragHotZone)
		{
			int controlId = GUIUtility.GetControlID("EditorTextField".GetHashCode(), FocusType.Keyboard, _position);
			Event e = Event.current;

	
			EditorGUIUtility.AddCursorRect(_dragHotZone, MouseCursor.SlideArrow);

	
			if (e.type == EventType.MouseDown && e.button == 0 && _dragHotZone.Contains(e.mousePosition))
			{
				s_IsDragging = true;
				s_DragControlID = controlId;
				s_StartValue = _value;
				s_AccumulatedDelta = 0f;
				GUIUtility.hotControl = controlId;
				EditorGUIUtility.SetWantsMouseJumping(1); 
				e.Use();
			}
			else if (s_IsDragging && GUIUtility.hotControl == controlId)
			{
				if (e.type == EventType.MouseDrag)
				{
					float delta = e.delta.x;
					s_AccumulatedDelta += delta;
					
					float sensitivity = s_BaseSensitivity * (e.shift ? 0.1f : 1f);
					float newValue = s_StartValue + s_AccumulatedDelta * sensitivity;

					if (!Mathf.Approximately(newValue, _value))
					{
						_value = newValue;
						GUI.changed = true;
					}
					e.Use();
				}
				else if (e.type == EventType.MouseUp || e.rawType == EventType.MouseUp)
				{
					_EndDrag();
					e.Use();
				}
	
				else if (e.type == EventType.Layout) {  }
			}

			if (doFloatFieldMethod != null && recycledEditor != null)
			{
				object[] parameters = {recycledEditor, _position, _dragHotZone, controlId, _value, "g7", style, true};
				try
				{
					return (float) doFloatFieldMethod.Invoke(null, parameters);
				}
				catch
				{
				}
			}
			
			EditorGUI.BeginChangeCheck();
			float result = EditorGUI.FloatField(_position, _value);
			if (EditorGUI.EndChangeCheck())
			{
				if (s_IsDragging)
				{
					_EndDrag();
				}
			}

			return result;
		}

		private static void _EndDrag()
		{
			s_IsDragging = false;
			s_DragControlID = 0;
			GUIUtility.hotControl = 0;
			try
			{
				EditorGUIUtility.SetWantsMouseJumping(0);
			}
			catch
			{
			}
		}

		public static float FloatField(GUIContent _content, float _value, float _labelwidth,
			params GUILayoutOption[] _options)
		{
			Rect totalRect = EditorGUILayout.GetControlRect(_options);

			Rect labelRect = new Rect(totalRect.x, totalRect.y, _labelwidth, totalRect.height);
			Rect inputRect = new Rect(totalRect.x + _labelwidth, totalRect.y, totalRect.width - _labelwidth,
				totalRect.height);

			EditorGUI.LabelField(labelRect, _content);
			return FloatFieldExtended(inputRect, _value, labelRect);
		}
	}
}
