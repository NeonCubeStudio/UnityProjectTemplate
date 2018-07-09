using UnityEngine;
using Automata.Debugging;
using Automata.Utility;

namespace Automata
{
    [System.Serializable]
    internal struct Properties
    {
        internal bool RestartRequired;
    }

    [DisallowMultipleComponent]
    internal class AutomataManager : ManagerBehaviour
    {
        internal Properties Properties = new Properties();
        
        // singleton
        private static AutomataManager instance = null;
        public static AutomataManager Instance
        {
            get
            {
                // find instance
                if (instance == null)
                {
                    instance = FindObjectOfType<AutomataManager>();
                }

                // if it still doesn't exist, instance does not exist
                if (instance == null)
                {
                    Debug.LogError("AutomataManager could not be found");
                }

                return instance;
            }
            set
            {
                instance = value;
            }
        }

        private void Awake()
        {
            Initialize();
        }

        internal override void Initialize()
        {
            GlobalFlags.Add("DebugGlobal", true);
        }

        internal T GetManager<T>() where T : ManagerBehaviour
        {
            return GetComponent<T>();
        }
    }
}