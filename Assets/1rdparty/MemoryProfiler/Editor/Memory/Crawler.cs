#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.MemoryProfiler;

namespace MemoryProfilerWindow
{
    internal enum PointerType
    {
        Reference,
        RawPointer
    }

    internal struct ThingToProfile
    {
        internal readonly PointerType type;
        internal readonly BytesAndOffset bytesAndOffset;
        internal readonly ulong objectPointer;
        internal readonly TypeDescription typeDescription;
        internal readonly bool useStaticFields;
        internal readonly int indexOfFrom;

        internal ThingToProfile(ulong objectPtr, int refIndexOfFrom)
        {
            type = PointerType.Reference;
            objectPointer = objectPtr;
            indexOfFrom = refIndexOfFrom;

            useStaticFields = true;
            typeDescription = new TypeDescription();
            bytesAndOffset = new BytesAndOffset();
        }

        internal ThingToProfile(TypeDescription typeDesc, BytesAndOffset inBytesAndOffset, bool inUseStaticFields, int inIndexOfFrom)
        {
            type = PointerType.RawPointer;
            typeDescription = typeDesc;
            bytesAndOffset = inBytesAndOffset;
            useStaticFields = inUseStaticFields;
            indexOfFrom = inIndexOfFrom;
            objectPointer = 0;
        }
    }

    internal class Crawler
    {
        private Dictionary<UInt64, TypeDescription> _typeInfoToTypeDescription;

        private Dictionary<int, UInt64> _pointer2Backups = new Dictionary<int, ulong>();
        private VirtualMachineInformation _virtualMachineInformation;
        private TypeDescription[] _typeDescriptions;
        private FieldDescription[][] _instanceFields;
        private FieldDescription[][] _staticFields;

        internal PackedCrawlerData Crawl(PackedMemorySnapshot input)
        {
            _typeInfoToTypeDescription = input.typeDescriptions.ToDictionary(td => td.typeInfoAddress, td => td);
            _virtualMachineInformation = input.virtualMachineInformation;
            _typeDescriptions = input.typeDescriptions;
            _instanceFields = new FieldDescription[_typeDescriptions.Length][];
            _staticFields = new FieldDescription[_typeDescriptions.Length][];

            foreach (TypeDescription type in _typeDescriptions)
            {
                _instanceFields[type.typeIndex] = TypeTools.AllFieldsOf(type, _typeDescriptions, TypeTools.FieldFindOptions.OnlyInstance).ToArray();
                _staticFields[type.typeIndex] = TypeTools.AllFieldsOf(type, _typeDescriptions, TypeTools.FieldFindOptions.OnlyStatic).ToArray();
            }

            PackedCrawlerData result = new PackedCrawlerData(input);

            List<PackedManagedObject> managedObjects = new List<PackedManagedObject>(result.startIndices.OfFirstManagedObject * 3);

            List<Connection> connections = new List<Connection>(managedObjects.Capacity * 3);
            //we will be adding a lot of connections, but the input format also already had connections. (nativeobject->nativeobject and nativeobject->gchandle). we'll add ours to the ones already there.
            connections.AddRange(input.connections);

            Stack<ThingToProfile> thingsToProfile = new Stack<ThingToProfile>();
            for(int i = 0; i < input.gcHandles.Length; ++i)
            {
                thingsToProfile.Push(new ThingToProfile(input.gcHandles[i].target, result.startIndices.OfFirstGCHandle + i));
            }

            for (int i = 0; i < result.typesWithStaticFields.Length; i++)
            {
                TypeDescription typeDescription = result.typesWithStaticFields[i];
                thingsToProfile.Push(new ThingToProfile(typeDescription, new BytesAndOffset { bytes = typeDescription.staticFieldBytes, offset = 0, pointerSize = _virtualMachineInformation.pointerSize }, true, result.startIndices.OfFirstStaticFields + i));
            }

            while (thingsToProfile.Count > 0)
            {
                ThingToProfile thingToProfile = thingsToProfile.Pop();
                if(thingToProfile.type == PointerType.Reference)
                {
                    CrawlPointerNonRecursive(input, result.startIndices, thingToProfile.objectPointer, thingToProfile.indexOfFrom, connections, managedObjects, thingsToProfile);
                }
                else
                {
                    CrawlRawObjectDataNonRecursive(input, result.startIndices, thingToProfile.bytesAndOffset, thingToProfile.typeDescription, thingToProfile.useStaticFields, thingToProfile.indexOfFrom, connections, managedObjects, thingsToProfile);
                }
            }

            result.managedObjects = managedObjects.ToArray();
            connections.AddRange(AddManagedToNativeConnectionsAndRestoreObjectHeaders(input, result.startIndices, result));
            result.connections = connections.ToArray();

            return result;
        }

        private IEnumerable<Connection> AddManagedToNativeConnectionsAndRestoreObjectHeaders(PackedMemorySnapshot packedMemorySnapshot, StartIndices startIndices, PackedCrawlerData packedCrawlerData)
        {
            if (packedMemorySnapshot.typeDescriptions.Length == 0)
                yield break;

            TypeDescription unityEngineObjectTypeDescription = packedMemorySnapshot.typeDescriptions.First(td => td.name == "UnityEngine.Object");

            for (int i = 0; i != packedCrawlerData.managedObjects.Length; i++)
            {
                int managedObjectIndex = i + startIndices.OfFirstManagedObject;
                UInt64 address = packedCrawlerData.managedObjects[i].address;

                TypeDescription typeDescription = RestoreObjectHeader(packedMemorySnapshot.managedHeapSections, address, managedObjectIndex);

                if (!DerivesFrom(packedMemorySnapshot.typeDescriptions, typeDescription.typeIndex, unityEngineObjectTypeDescription.typeIndex))
                    continue;

                int indexOfNativeObject = -1;
#if UNITY_5_4_OR_NEWER
                // Since Unity 5.4, UnityEngine.Object no longer stores instance id inside when running in the player. Use cached ptr instead to find the index of native object
                int cachedPtrOffset = unityEngineObjectTypeDescription.fields.Single(f => f.name == "m_CachedPtr").offset;
                UInt64 cachedPtr = packedMemorySnapshot.managedHeapSections.Find(address + (UInt64)cachedPtrOffset, packedMemorySnapshot.virtualMachineInformation).ReadPointer();
                indexOfNativeObject = Array.FindIndex(packedMemorySnapshot.nativeObjects, no => (ulong)no.nativeObjectAddress == cachedPtr);
#else
                int instanceIDOffset = unityEngineObjectTypeDescription.fields.Single(f => f.name == "m_InstanceID").offset;
                int instanceID = packedMemorySnapshot.managedHeapSections.Find(address + (UInt64)instanceIDOffset, packedMemorySnapshot.virtualMachineInformation).ReadInt32();
                indexOfNativeObject = Array.FindIndex(packedMemorySnapshot.nativeObjects, no => no.instanceId == instanceID);
#endif

                if (indexOfNativeObject != -1)
                    yield return new Connection { @from = managedObjectIndex, to = indexOfNativeObject + startIndices.OfFirstNativeObject };
            }
        }

        private bool DerivesFrom(TypeDescription[] typeDescriptions, int typeIndex, int potentialBase)
        {
            if (typeIndex == potentialBase)
                return true;
            int baseIndex = typeDescriptions[typeIndex].baseOrElementTypeIndex;

            if (baseIndex == -1)
                return false;

            return DerivesFrom(typeDescriptions, baseIndex, potentialBase);
        }

        private TypeDescription GetTypeDescription(MemorySection[] heap, ulong objectAddress)
        {
            TypeDescription typeDescription;

            // IL2CPP has the class pointer as the first member of the object.
            if (!_typeInfoToTypeDescription.TryGetValue(objectAddress, out typeDescription))
            {
                // Mono has a vtable pointer as the first member of the object.
                // The first member of the vtable is the class pointer.
                BytesAndOffset vtable = heap.Find(objectAddress, _virtualMachineInformation);
                UInt64 vtableClassPointer = vtable.ReadPointer();
                typeDescription = _typeInfoToTypeDescription[vtableClassPointer];
            }

            return typeDescription;
        }

        private TypeDescription RestoreObjectHeader(MemorySection[] heaps, ulong address, int managedObjectIndex)
        {
            BytesAndOffset bo = heaps.Find(address, _virtualMachineInformation);
            UInt64 mask = this._virtualMachineInformation.pointerSize == 8 ? System.UInt64.MaxValue - 1 : System.UInt32.MaxValue - 1;
            UInt64 pointer = bo.ReadPointer();
            UInt64 typeInfoAddress = pointer & mask;
            bo.WritePointer(typeInfoAddress);

            UInt64 restoreValue = 0;
            _pointer2Backups.TryGetValue(managedObjectIndex, out restoreValue);
            bo.NextPointer().WritePointer(restoreValue);

            return GetTypeDescription(heaps, typeInfoAddress);
        }

        private void CrawlRawObjectDataNonRecursive(PackedMemorySnapshot packedMemorySnapshot, StartIndices startIndices, BytesAndOffset bytesAndOffset, TypeDescription typeDescription, bool useStaticFields, int indexOfFrom,
                                                        List<Connection> out_connections, List<PackedManagedObject> out_managedObjects, Stack<ThingToProfile> out_thingsToProfile)
        {

            // Do not crawl MemoryProfilerWindow objects
            if (typeDescription.name.StartsWith("MemoryProfilerWindow."))
                return;

            FieldDescription[] fields = useStaticFields ? _staticFields[typeDescription.typeIndex] : _instanceFields[typeDescription.typeIndex];

            for(int i = 0; i < fields.Length; ++i)
            {
                FieldDescription field = fields[i];
                TypeDescription fieldType = packedMemorySnapshot.typeDescriptions[field.typeIndex];
                BytesAndOffset fieldLocation = bytesAndOffset.Add(field.offset - (useStaticFields ? 0 : _virtualMachineInformation.objectHeaderSize));

                if(fieldType.isValueType)
                {
                    out_thingsToProfile.Push(new ThingToProfile(fieldType, fieldLocation, false, indexOfFrom));
                    continue;
                }
                else
                {
                    //temporary workaround for a bug in 5.3b4 and earlier where we would get literals returned as fields with offset 0. soon we'll be able to remove this code.
                    bool gotException = false;
                    try
                    {
                        fieldLocation.ReadPointer();
                    }
                    catch (ArgumentException)
                    {
                        UnityEngine.Debug.LogWarningFormat("Skipping field {0} on type {1}", field.name, typeDescription.name);
                        UnityEngine.Debug.LogWarningFormat("FieldType.name: {0}", fieldType.name);
                        gotException = true;
                    }

                    if (!gotException)
                    {
                        out_thingsToProfile.Push(new ThingToProfile(fieldLocation.ReadPointer(), indexOfFrom));
                    }
                }
            }
        }

        private void CrawlPointerNonRecursive(PackedMemorySnapshot packedMemorySnapshot, StartIndices startIndices, ulong pointer, int indexOfFrom, List<Connection> out_connections, List<PackedManagedObject> out_managedObjects, Stack<ThingToProfile> out_thingsToProfile)
        {
            BytesAndOffset bo = packedMemorySnapshot.managedHeapSections.Find(pointer, _virtualMachineInformation);
            if (!bo.IsValid)
                return;

            UInt64 typeInfoAddress;
            int indexOfObject;
            bool wasAlreadyCrawled;
            try
            {
                ParseObjectHeader(startIndices, packedMemorySnapshot.managedHeapSections, pointer, out typeInfoAddress, out indexOfObject, out wasAlreadyCrawled, out_managedObjects);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarningFormat("Exception parsing object header. Skipping. {0}", e);
                return;
            }

            out_connections.Add(new Connection() { from = indexOfFrom, to = indexOfObject });

            if (wasAlreadyCrawled)
                return;

            TypeDescription typeDescription = _typeInfoToTypeDescription[typeInfoAddress];

            if (!typeDescription.isArray)
            {
                out_thingsToProfile.Push(new ThingToProfile(typeDescription, bo.Add(_virtualMachineInformation.objectHeaderSize), false, indexOfObject));
                return;
            }

            int arrayLength = ArrayTools.ReadArrayLength(packedMemorySnapshot.managedHeapSections, pointer, typeDescription, _virtualMachineInformation);
            TypeDescription elementType = packedMemorySnapshot.typeDescriptions[typeDescription.baseOrElementTypeIndex];
            BytesAndOffset cursor = bo.Add(_virtualMachineInformation.arrayHeaderSize);
            for (int i = 0; i != arrayLength; i++)
            {
                if (elementType.isValueType)
                {
                    out_thingsToProfile.Push(new ThingToProfile(elementType, cursor, false, indexOfObject));
                    cursor = cursor.Add(elementType.size);
                }
                else
                {
                    out_thingsToProfile.Push(new ThingToProfile(cursor.ReadPointer(), indexOfObject));
                    cursor = cursor.NextPointer();
                }
            }
        }

        int SizeOfObjectInBytes(TypeDescription typeDescription, BytesAndOffset bo, MemorySection[] heap, ulong address)
        {
            if (typeDescription.isArray)
                return ArrayTools.ReadArrayObjectSizeInBytes(heap, address, typeDescription, _typeDescriptions, _virtualMachineInformation);

            if (typeDescription.name == "System.String")
                return StringTools.ReadStringObjectSizeInBytes(bo, _virtualMachineInformation);

            //array and string are the only types that are special, all other types just have one size, which is stored in the typedescription
            return typeDescription.size;
        }

        private void ParseObjectHeader(StartIndices startIndices, MemorySection[] heap, ulong originalHeapAddress, out ulong typeInfoAddress, out int indexOfObject, out bool wasAlreadyCrawled, List<PackedManagedObject> outManagedObjects)
        {
            BytesAndOffset bo = heap.Find(originalHeapAddress, _virtualMachineInformation);

            UInt64 pointer1 = bo.ReadPointer();
            BytesAndOffset pointer2 = bo.NextPointer();

            if (HasMarkBit(pointer1) == 0)
            {
                TypeDescription typeDescription = GetTypeDescription(heap, pointer1);

                wasAlreadyCrawled = false;
                indexOfObject = outManagedObjects.Count + startIndices.OfFirstManagedObject;
                typeInfoAddress = typeDescription.typeInfoAddress;

                int size = SizeOfObjectInBytes(typeDescription, bo, heap, originalHeapAddress);

                outManagedObjects.Add(new PackedManagedObject() { address = originalHeapAddress, size = size, typeIndex = typeDescription.typeIndex });

                //okay, we gathered all information, now lets set the mark bit, and store the index for this object in the 2nd pointer of the header, which is rarely used.
                bo.WritePointer(pointer1 | 1);
                pointer2.WritePointer((ulong)indexOfObject);
                return;
            }

            //give typeinfo address back without the markbit
            typeInfoAddress = ClearMarkBit(pointer1);
            wasAlreadyCrawled = true;
            //read the index for this object that we stored in the 2ndpointer field of the header
            indexOfObject = (int)pointer2.ReadPointer();
        }

        private static ulong HasMarkBit(ulong pointer1)
        {
            return pointer1 & 1;
        }

        private static ulong ClearMarkBit(ulong pointer1)
        {
            return pointer1 & ~(1UL);
        }
    }
}
#endif