#if UNITY_EDITOR
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEditor;
using UnityEditor.MemoryProfiler;

#if UNITY_5_5_OR_NEWER
using Profiler = UnityEngine.Profiling.Profiler;
#else
using Profiler = UnityEngine.Profiler;
#endif

namespace MemoryProfilerWindow
{
    internal static class PackedMemorySnapshotUtility
    {

        private static string previousDirectory = null;

        internal static void SaveToFile(PackedMemorySnapshot snapshot)
        {
            string filePath = EditorUtility.SaveFilePanel("Save Snapshot", previousDirectory, "MemorySnapshot", "memsnap2");
            if (string.IsNullOrEmpty(filePath))
                return;

            previousDirectory = Path.GetDirectoryName(filePath);
            SaveToFile(filePath, snapshot);
        }

        private static void SaveToFile(string filePath, PackedMemorySnapshot snapshot)
        {
            // Saving snapshots using JsonUtility, instead of BinaryFormatter, is significantly faster.
            // I cancelled saving a memory snapshot that is saving using BinaryFormatter after 24 hours.
            // Saving the same memory snapshot using JsonUtility.ToJson took 20 seconds only.

            Debug.LogFormat("Saving...");
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            Profiler.BeginSample("PackedMemorySnapshotUtility.SaveToFile");
            stopwatch.Start();

            string json = JsonUtility.ToJson(snapshot);
            File.WriteAllText(filePath, json);

            stopwatch.Stop();
            Profiler.EndSample();
            Debug.LogFormat("Saving took {0}ms", stopwatch.ElapsedMilliseconds);
        }

        internal static PackedMemorySnapshot LoadFromFile()
        {
            string filePath = EditorUtility.OpenFilePanelWithFilters("Load Snapshot", previousDirectory, new[] { "Snapshots", "memsnap2,memsnap" });
            if (string.IsNullOrEmpty(filePath))
                return null;

            return LoadFromFile(filePath);
        }

        private static PackedMemorySnapshot LoadFromFile(string filePath)
        {
            Debug.LogFormat("Loading...");
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            PackedMemorySnapshot result;
            string fileExtension = Path.GetExtension(filePath);

            if (string.Equals(fileExtension, ".memsnap2", System.StringComparison.OrdinalIgnoreCase))
            {
                Profiler.BeginSample("PackedMemorySnapshotUtility.LoadFromFile(json)");
                stopwatch.Start();

                string json = File.ReadAllText(filePath);
                result = JsonUtility.FromJson<PackedMemorySnapshot>(json);

                stopwatch.Stop();
                Profiler.EndSample();
            }
            else if (string.Equals(fileExtension, ".memsnap", System.StringComparison.OrdinalIgnoreCase))
            {
                Profiler.BeginSample("PackedMemorySnapshotUtility.LoadFromFile(binary)");
                stopwatch.Start();

                BinaryFormatter binaryFormatter = new BinaryFormatter();
                using (Stream stream = File.Open(filePath, FileMode.Open))
                {
                    result = binaryFormatter.Deserialize(stream) as PackedMemorySnapshot;
                }

                stopwatch.Stop();
                Profiler.EndSample();
            }
            else
            {
                Debug.LogErrorFormat("MemoryProfiler: Unrecognized memory snapshot format '{0}'.", filePath);
                result = null;
            }

            Debug.LogFormat("Loading took {0}ms", stopwatch.ElapsedMilliseconds);
            return result;
        }
    }
}
#endif