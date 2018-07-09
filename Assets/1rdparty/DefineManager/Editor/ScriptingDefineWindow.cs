using UnityEditor;
using UnityEngine;

namespace NeonCubeStudio.DefineManager
{
	public class ScriptingDefineWindow : EditorWindow
	{
		[MenuItem("Window/Platform Defines")]
		public static void Init()
		{
			GetWindow<ScriptingDefineWindow>(true, "Platform Defines", true);
		}

		private Editor m_Editor;
		private ScriptingDefineObject m_Asset;

		private void OnEnable()
		{
			m_Asset = ScriptableObject.CreateInstance<ScriptingDefineObject>();
			m_Editor = Editor.CreateEditor(m_Asset);
		}

		private void OnDisable()
		{
			Object.DestroyImmediate(m_Editor);
			Object.DestroyImmediate(m_Asset);
		}

		private void OnGUI()
		{
			m_Editor.OnInspectorGUI();
		}
	}
}
