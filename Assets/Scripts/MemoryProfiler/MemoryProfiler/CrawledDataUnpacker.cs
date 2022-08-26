#if UNITY_5_3_OR_NEWER
using System.Collections.Generic;
using System.Linq;
using UnityEditor.MemoryProfiler;
using System;
using UnityEngine;

namespace MemoryProfilerWindow
{
    class CrawlDataUnpacker
    {
        public static CrawledMemorySnapshot Unpack(PackedCrawlerData packedCrawlerData, int natinum, ref Dictionary<int, NativeObject> allnative)
        {
            var packedSnapshot = packedCrawlerData.packedMemorySnapshot;
            var result = new CrawledMemorySnapshot
            {
                nativeObjects = packedSnapshot.nativeObjects.Select(r => UnpackNativeUnityEngineObject(packedSnapshot, r)).ToArray(),
                managedObjects = packedCrawlerData.managedObjects.Select(pm => UnpackManagedObject(packedSnapshot, pm)).ToArray(),
                gcHandles = packedSnapshot.gcHandles.Select(pgc => UnpackGCHandle(packedSnapshot)).ToArray(),
                staticFields = packedSnapshot.typeDescriptions.Where(t => t.staticFieldBytes != null & t.staticFieldBytes.Length > 0).Select(t => UnpackStaticFields(t)).ToArray(),
                typeDescriptions = packedSnapshot.typeDescriptions,
                managedHeap = packedSnapshot.managedHeapSections,
                nativeTypes = packedSnapshot.nativeTypes,
                virtualMachineInformation = packedSnapshot.virtualMachineInformation
            };
            foreach(var i in result.nativeObjects)
            {
                if (!allnative.ContainsKey(i.instanceID))
                {
                    NativeObject na = new NativeObject();
                    na.instanceID = i.instanceID;
                    na.references = new List<int>();
                    na.referencedBy = new List<int>();
                    allnative.Add(i.instanceID, na);
                }
            }
            result.FinishSnapshot();

            var referencesLists = MakeTempLists(result.allObjects.Length);
            var referencedByLists = MakeTempLists(result.allObjects.Length);
            try
            {
                int i = 0;
                foreach (var connection in packedCrawlerData.connections)
                {
                    if(natinum <= i)
                    {
                        referencesLists[connection.@from].Add(result.allObjects[connection.to]);
                        referencedByLists[connection.to].Add(result.allObjects[connection.@from]);
                    }
                    else
                    {

                        allnative[connection.@from].references.Add(connection.to);
                        allnative[connection.@to].referencedBy.Add(connection.from);
                    }
                    i++;
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }

            for (var i = 0; i != result.allObjects.Length; i++)
            {
                result.allObjects[i].references = referencesLists[i].ToArray();
                result.allObjects[i].referencedBy = referencedByLists[i].ToArray();
            }

            return result;
        }

        static List<ThingInMemory>[] MakeTempLists(int combined)
        {
            var referencesLists = new List<ThingInMemory>[combined];
            for (int i = 0; i != referencesLists.Length; i++)
                referencesLists[i] = new List<ThingInMemory>(4);
            return referencesLists;
        }

        static StaticFields UnpackStaticFields(TypeDescription typeDescription)
        {
            return new StaticFields()
                   {
                       typeDescription = typeDescription,
                       caption = "static fields of " + typeDescription.name,
                       size = typeDescription.staticFieldBytes.Length
                   };
        }

        static GCHandle UnpackGCHandle(PackedMemorySnapshot packedSnapshot)
        {
            return new GCHandle() { size = packedSnapshot.virtualMachineInformation.pointerSize, caption = "gchandle" };
        }

        static ManagedObject UnpackManagedObject(PackedMemorySnapshot packedSnapshot, PackedManagedObject pm)
        {
            var typeDescription = packedSnapshot.typeDescriptions[pm.typeIndex];
            return new ManagedObject() { address = pm.address, size = pm.size, typeDescription = typeDescription, caption = typeDescription.name };
        }

        static NativeUnityEngineObject UnpackNativeUnityEngineObject(PackedMemorySnapshot packedSnapshot, PackedNativeUnityEngineObject packedNativeUnityEngineObject)
        {
#if UNITY_5_6_OR_NEWER
            var classId = packedNativeUnityEngineObject.nativeTypeArrayIndex;
#else
            var classId = packedNativeUnityEngineObject.classId;
#endif
            var className = packedSnapshot.nativeTypes[classId].name;

            return new NativeUnityEngineObject()
                   {
                       instanceID = packedNativeUnityEngineObject.instanceId,
                       classID = classId,
                       className = className,
                       name = packedNativeUnityEngineObject.name,
                       caption = packedNativeUnityEngineObject.name + "(" + className + ")",
                       size = packedNativeUnityEngineObject.size,
                       isPersistent = packedNativeUnityEngineObject.isPersistent,
                       isDontDestroyOnLoad = packedNativeUnityEngineObject.isDontDestroyOnLoad,
                       isManager = packedNativeUnityEngineObject.isManager,
                       nativeObjectAddress = packedNativeUnityEngineObject.nativeObjectAddress,
                       hideFlags = packedNativeUnityEngineObject.hideFlags
                   };
        }
    }

    [System.Serializable]
    internal class PackedCrawlerData
    {
        public bool valid;
        public PackedMemorySnapshot packedMemorySnapshot;
        public StartIndices startIndices;
        public PackedManagedObject[] managedObjects;
        public TypeDescription[] typesWithStaticFields;
        public Connection[] connections;

        public PackedCrawlerData(PackedMemorySnapshot packedMemorySnapshot)
        {
            this.packedMemorySnapshot = packedMemorySnapshot;
            typesWithStaticFields = packedMemorySnapshot.typeDescriptions.Where(t => t.staticFieldBytes != null && t.staticFieldBytes.Length > 0).ToArray();
            startIndices = new StartIndices(this.packedMemorySnapshot.gcHandles.Length, this.packedMemorySnapshot.nativeObjects.Length, typesWithStaticFields.Length);
            valid = true;
        }
    }

    [System.Serializable]
    internal class StartIndices
    {
        [SerializeField]
        private int _gcHandleCount;
        [SerializeField]
        private int _nativeObjectCount;
        [SerializeField]
        private int _staticFieldsCount;

        public StartIndices(int gcHandleCount, int nativeObjectCount, int staticFieldsCount)
        {
            _gcHandleCount = gcHandleCount;
            _nativeObjectCount = nativeObjectCount;
            _staticFieldsCount = staticFieldsCount;
        }

        public int OfFirstGCHandle { get { return 0; } }
        public int OfFirstNativeObject { get { return OfFirstGCHandle + _gcHandleCount; } }
        public int OfFirstStaticFields { get { return OfFirstNativeObject + _nativeObjectCount; } }
        public int OfFirstManagedObject { get { return OfFirstStaticFields + _staticFieldsCount; } }
    }
}
#endif