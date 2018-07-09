using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NeonCubeStudio.DefinesManager
{
	[SuppressMessage("ReSharper", "NotAccessedField.Local")]
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public class ScriptingDefineObject : ScriptableObject
	{
		[SerializeField]
        private Compiler m_Compiler;

		[SerializeField]
        private BuildTargetGroup m_BuildTarget;

		[SerializeField]
        private string[] m_Defines;

		[SerializeField]
        private bool m_IsApplied;
	}
}
