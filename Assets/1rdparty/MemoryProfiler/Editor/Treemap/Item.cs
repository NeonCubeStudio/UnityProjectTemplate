#if UNITY_EDITOR
using System;
using MemoryProfilerWindow;
using UnityEditor;
using UnityEngine;

namespace Treemap
{
    internal class Item : IComparable<Item>, ITreemapRenderable
    {
        internal Group _group;
        internal Rect _position;

        internal ThingInMemory _thingInMemory;

        internal long memorySize { get { return _thingInMemory.ignored ? 0 : _thingInMemory.size; } }
        internal string name { get { return _thingInMemory.caption; } }
        internal Color color { get { return _group.color; } }

        internal Item(ThingInMemory thingInMemory, Group group)
        {
            _thingInMemory = thingInMemory;
            _group = group;
        }

        public int CompareTo(Item other)
        {
            return (int)(_group != other._group ? other._group.totalMemorySize - _group.totalMemorySize : other.memorySize - memorySize);
        }

        public Color GetColor()
        {
            return _group.color;
        }

        public Rect GetPosition()
        {
            return _position;
        }

        public string GetLabel()
        {
            string row1 = _group._name;
            string row2 = EditorUtility.FormatBytes(memorySize);
            return row1 + "\n" + row2;
        }
    }
}
#endif