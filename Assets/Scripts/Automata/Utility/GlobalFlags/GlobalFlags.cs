using System.Collections.Generic;

namespace Automata.Utility
{
    internal class GlobalFlags
    {
        private static Dictionary<string, bool> flags = new Dictionary<string, bool>();

        // add new flag
        internal static void Add(string flagName, bool flagState)
        {
            // if flag name isn't null and doesn't exist
            if (flagName != "" && !flags.ContainsKey(flagName))
            {
                flags.Add(flagName, flagState);
            }
        }

        // get value of existing flag
        internal static bool Get(string flagName)
        {
            bool flagState = false;

            // get flag state
            flags.TryGetValue(flagName, out flagState);

            return flagState;
        }

        // set value of existing flag
        internal static void Set(string flagName, bool flagState)
        {
            // if flag exists
            if (flags.ContainsKey(flagName))
            {
                flags[flagName] = flagState;
            }
        }
    }
}