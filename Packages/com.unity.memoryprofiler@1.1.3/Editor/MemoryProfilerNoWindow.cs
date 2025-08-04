using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.MemoryProfiler.Editor.UI;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Unity.MemoryProfiler.Editor.Tests")]
namespace Unity.MemoryProfiler.Editor
{
    public class MemoryProfilerNoWindow
    {
        #region 自动化流程数据
        class SummaryDataClass
        {
            public string GroupName { get; set; }
            public string ItemName { get; set; }
            public ulong AllocatedSize { get; set; }
        }

        class UnityObjectClass
        {
            public string ObjectType { get; set; }
            public ulong AllocateSize { get; set; }
            public ulong NativeSize { get; set; }
            public ulong ManagedSize { get; set; }
            public ulong GraphicsSize { get; set; }
            public List<ObjectItemClass> ChildData { get; set; }
        }

        class ObjectItemClass
        {
            public string Name { get; set; }
            public ulong AllocateSize { get; set; }
            public ulong NativeSize { get; set; }
            public ulong ManagedSize { get; set; }
            public ulong GraphicsSize { get; set; }
        }

        class AllMemoryClass
        {
            public string GroupName { get; set; }
            public List<SubMemoryData> SubData { get; set; }
            public ulong AllocateSize { get; set; }  //当前节点名内存总占用
            public int Count { get; set; }
            public int ChildCount { get; set; }
        }

        class SubMemoryData
        {
            public string ItemName { get; set; }
            public ulong AllocateSize { get; set; }   //当前节点名内存总占用
            public int Count { get; set; }
            public int ChildCount { get; set; }
            public List<SubMemoryData> SubData { get; set; }
        }

        internal class ManagedObjectData
        {
            public string Type { get; set; }
            public string ItemName { get; set; }
            public ulong AllocateSize { get; set; }   //当前节点名内存总占用
            public List<string> Referencer { get; set; }

            internal ManagedObjectData(int refMaxDepth)
            {
                Referencer = new List<string>(refMaxDepth);
            }
        }

        class ManagedDateProcessor<T>
        {
            int threadCount;
            Task[] tasks;
            Action<List<T>, string, ManagedObjectData, StreamWriter, int, int> processDataAction;
            StreamWriter[] writers;

            internal ManagedDateProcessor(int threadCount, Action<List<T>, string, ManagedObjectData, StreamWriter, int, int> processDataAction, string filePath)
            {
                this.threadCount = threadCount;
                this.tasks = new Task[threadCount];
                this.processDataAction = processDataAction;
                this.writers = new StreamWriter[threadCount];

                string directory = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string fileExtension = Path.GetExtension(filePath);

                string uuid = Guid.NewGuid().ToString("N");
                for (int i = 0; i < threadCount; i++)
                {
                    string subfilePath = Path.Combine(directory, $"{fileName}-{uuid}-{i}{fileExtension}");
                    writers[i] = new StreamWriter(subfilePath, true);
                }
            }

            internal void RunProcess(List<T> managedObjects, string objectType)
            {
                int chunkSize = managedObjects.Count / threadCount;

                for (int i = 0; i < threadCount; i++)
                {
                    int start = i * chunkSize;
                    int end = (i == threadCount - 1) ? managedObjects.Count : start + chunkSize;

                    int fileIdx = i;
                    ManagedObjectData managedObject = new ManagedObjectData(RefMaxDepth);

                    tasks[i] = Task.Run(() => {
                        processDataAction(managedObjects, objectType, managedObject, writers[fileIdx], start, end);
                    });
                }
                Task.WaitAll(tasks);
            }

            internal void EndProcess()
            {
                foreach (var writer in writers)
                {
                    writer.Flush();
                    writer.Close();
                }
            }

        }
        #endregion
        bool m_WindowInitialized = false;

        SnapshotDataService m_SnapshotDataService;
        PlayerConnectionService m_PlayerConnectionService;

        MemoryProfilerViewController m_ProfilerViewController;

        // Api exposed for testing purposes
        internal PlayerConnectionService PlayerConnectionService => m_PlayerConnectionService;
        internal SnapshotDataService SnapshotDataService => m_SnapshotDataService;
        internal MemoryProfilerViewController ProfilerViewController => m_ProfilerViewController;

        #region 解析模块数据并输出json
        AllMemorySummaryModelBuilder m_SummaryModelBuilder;
        ManagedMemorySummaryModelBuilder m_ManagedMemoryModelBuilder;
        UnityObjectsMemorySummaryModelBuilder m_UnityObjectsModelBuilder;
        ResidentMemorySummaryModelBuilder m_ResidentModelBuilder;
        UnityObjectsModelBuilder m_AllUnityObjectsModelBuilder;
        AllTrackedMemoryModelBuilder m_AllMemoryModelBuilder;
        List<TreeViewItemData<AllTrackedMemoryModel.ItemData>> stackDatas;
        static int RefMaxDepth = 5;   //最大引用深度
        static int ManagedThreadCount = 4;
        public string BuildSummaryData()
        {
            try
            {
                m_SummaryModelBuilder = new AllMemorySummaryModelBuilder(m_SnapshotDataService.Base, null);
                m_ManagedMemoryModelBuilder = new ManagedMemorySummaryModelBuilder(m_SnapshotDataService.Base, null);
                m_UnityObjectsModelBuilder = new UnityObjectsMemorySummaryModelBuilder(m_SnapshotDataService.Base, null);
                m_ResidentModelBuilder = new ResidentMemorySummaryModelBuilder(m_SnapshotDataService.Base, null);
                MemorySummaryModel summaryModel = m_SummaryModelBuilder.Build();  //Summary里的
                MemorySummaryModel managedDataModel = m_ManagedMemoryModelBuilder.Build();  //Summary里的
                MemorySummaryModel unityobjModel = m_UnityObjectsModelBuilder.Build();  //Summary里的
                MemorySummaryModel residentModel = m_ResidentModelBuilder.Build();
                List<SummaryDataClass> result = new List<SummaryDataClass>(30);
                SummaryDataClass amd = new SummaryDataClass();   //Allocated Memory Distribution
                amd.GroupName = summaryModel.Title;
                amd.ItemName = "";
                amd.AllocatedSize = summaryModel.TotalA;
                result.Add(amd);
                foreach (var row in summaryModel.Rows)
                {
                    SummaryDataClass sc = new SummaryDataClass();
                    sc.GroupName = summaryModel.Title;
                    sc.ItemName = row.Name;
                    sc.AllocatedSize = row.BaseSize.Committed;
                    result.Add(sc);
                }

                SummaryDataClass managedHeap = new SummaryDataClass();   //Managed Heap Utilization
                managedHeap.GroupName = managedDataModel.Title;
                managedHeap.ItemName = "";
                managedHeap.AllocatedSize = managedDataModel.TotalA;
                result.Add(managedHeap);
                foreach (var row in managedDataModel.Rows)
                {
                    SummaryDataClass sc = new SummaryDataClass();
                    sc.GroupName = managedDataModel.Title;
                    sc.ItemName = row.Name;
                    sc.AllocatedSize = row.BaseSize.Committed;
                    result.Add(sc);
                }

                SummaryDataClass TopunityData = new SummaryDataClass();   //Top Unity Objects Categories
                TopunityData.GroupName = unityobjModel.Title;
                TopunityData.ItemName = "";
                TopunityData.AllocatedSize = unityobjModel.TotalA;
                result.Add(TopunityData);
                foreach (var row in unityobjModel.Rows)
                {
                    SummaryDataClass sc = new SummaryDataClass();
                    sc.GroupName = unityobjModel.Title;
                    sc.ItemName = row.Name;
                    sc.AllocatedSize = row.BaseSize.Committed;
                    result.Add(sc);
                }

                SummaryDataClass memUOD = new SummaryDataClass();   //Memory Usage On Device
                memUOD.GroupName = residentModel.Title;
                memUOD.ItemName = "";
                memUOD.AllocatedSize = residentModel.TotalA;
                result.Add(memUOD);
                foreach (var row in residentModel.Rows)
                {
                    SummaryDataClass sc = new SummaryDataClass();
                    sc.GroupName = residentModel.Title;
                    sc.ItemName = row.Name;
                    sc.AllocatedSize = row.BaseSize.Resident;
                    result.Add(sc);
                }
                return JsonConvert.SerializeObject(result);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }
        public string BuildUnityObjectsData()
        {
            try
            {
                m_AllUnityObjectsModelBuilder = new UnityObjectsModelBuilder();
                IScopedFilter<string> searchStringFilter = null;
                ITextFilter unityObjectNameFilter = null;
                ITextFilter unityObjectTypeNameFilter = null;
                IInstancIdFilter unityObjectInstanceIdFilter = null;
                bool flattenHierarchy = false;
                bool potentialDuplicatesFilter = false;
                bool disambiguateByInstanceId = false;
                var args = new UnityObjectsModelBuilder.BuildArgs(
                    searchStringFilter,
                    unityObjectNameFilter,
                    unityObjectTypeNameFilter,
                    unityObjectInstanceIdFilter,
                    flattenHierarchy,
                    potentialDuplicatesFilter,
                    disambiguateByInstanceId,
                    ProcessUnityObjectItemSelectedInvoke);
                var model = m_AllUnityObjectsModelBuilder.Build(m_SnapshotDataService.Base, args);
                List<UnityObjectClass> result = new List<UnityObjectClass>();
                var rootNodes = model.RootNodes;
                if (rootNodes.Count <= 0)
                    return null;
                foreach (var node in rootNodes)
                {
                    var data = node.data;
                    UnityObjectClass element = new UnityObjectClass();
                    element.ObjectType = data.Name;
                    element.AllocateSize = data.TotalSize.Committed;
                    element.NativeSize = data.NativeSize.Committed;
                    element.ManagedSize = data.ManagedSize.Committed;
                    element.GraphicsSize = data.GpuSize.Committed;
                    element.ChildData = new List<ObjectItemClass>();
                    if (node.hasChildren)
                    {
                        foreach (var child in node.children)
                        {
                            var childData = child.data;
                            ObjectItemClass oic = new ObjectItemClass();
                            oic.Name = string.IsNullOrEmpty(childData.Name) ? "<No Name>" : childData.Name;
                            oic.AllocateSize = childData.TotalSize.Committed;
                            oic.NativeSize = childData.NativeSize.Committed;
                            oic.ManagedSize = childData.ManagedSize.Committed;
                            oic.GraphicsSize = childData.GpuSize.Committed;
                            element.ChildData.Add(oic);
                        }
                    }
                    result.Add(element);
                }
                return JsonConvert.SerializeObject(result);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }

        }
        public string BuildAllMemoryData()
        {
            try
            {
                m_AllMemoryModelBuilder = new AllTrackedMemoryModelBuilder();
                IScopedFilter<string> searchFilter = null;
                ITextFilter itemNameFilter = null;
                IEnumerable<ITextFilter> itemPathFilter = null;
                bool excludeAll = false;
                bool disambiguateUnityObjects = false;
                AllTrackedMemoryTableMode m_TableMode = AllTrackedMemoryTableMode.OnlyCommitted;
                var args = new AllTrackedMemoryModelBuilder.BuildArgs(
                    searchFilter,
                    itemNameFilter,
                    itemPathFilter,
                    excludeAll,
                    MemoryProfilerSettings.ShowReservedMemoryBreakdown,
                    disambiguateUnityObjects,
                    m_TableMode == AllTrackedMemoryTableMode.OnlyCommitted,
                    ProcessObjectSelected);
                var model = m_AllMemoryModelBuilder.Build(m_SnapshotDataService.Base, args);
                List<AllMemoryClass> result = new List<AllMemoryClass>();
                var rootNodes = model.RootNodes;
                if (rootNodes.Count <= 0)
                    return null;
                foreach (var node in rootNodes)
                {
                    var data = node.data;
                    switch (data.Name)
                    {
                        case "Untracked*":
                            GetUntrackedData(node, ref result);
                            break;
                        case "Executables & Mapped":
                            GetExecutablesMappedData(node, ref result);
                            break;
                        case "Native":
                            GetNativeData(node, ref result);
                            break;
                        case "Graphics (Estimated)":
                            GetGraphicsData(node, ref result);
                            break;
                        case "Managed":
                            GetManagedData(node, ref result);
                            break;
                    }
                }
                return JsonConvert.SerializeObject(result);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }
        public void BuildStackReferenceData(in string resFilePath)
        {
            var processor = new ManagedDateProcessor<TreeViewItemData<AllTrackedMemoryModel.ItemData>>(ManagedThreadCount, ProcessManagedDatas, resFilePath);
            foreach (var parentItem in stackDatas)
            {
                var managedObjects = parentItem.children.ToList();

                processor.RunProcess(managedObjects, parentItem.data.Name);
            }
            processor.EndProcess();
        }
        void GetUntrackedData(TreeViewItemData<AllTrackedMemoryModel.ItemData> node, ref List<AllMemoryClass> result)
        {
            AllMemoryClass element = new AllMemoryClass();
            element.GroupName = node.data.Name;
            element.AllocateSize = node.data.Size.Committed;
            element.Count = 1;
            element.ChildCount = node.data.ChildCount;
            element.SubData = new List<SubMemoryData>();
            foreach (var childNode in node.children)
            {
                SubMemoryData subData = new SubMemoryData();
                subData.ItemName = childNode.data.Name;
                subData.AllocateSize = childNode.data.Size.Committed;
                subData.Count = 1;
                subData.ChildCount = childNode.data.ChildCount;
                subData.SubData = new List<SubMemoryData>();
                element.SubData.Add(subData);
            }
            result.Add(element);
        }
        void GetExecutablesMappedData(TreeViewItemData<AllTrackedMemoryModel.ItemData> node, ref List<AllMemoryClass> result)
        {
            // 需要统计dxcache的内存大小， 其他dll总大小
            var dxCaches = node.children.Where(item => item.data.Name.Contains("DXCache")).ToList();
            ulong dxSize = 0;
            foreach (var childNode in dxCaches)
            {
                dxSize += childNode.data.Size.Committed;
            }

            var data = node.data;
            AllMemoryClass element = new AllMemoryClass();
            element.GroupName = data.Name;
            element.AllocateSize = data.Size.Committed;
            element.ChildCount = data.ChildCount;
            element.Count = 1;
            element.SubData = new List<SubMemoryData>();
            SubMemoryData subData1 = new SubMemoryData();
            subData1.ItemName = "DxCache";
            subData1.AllocateSize = dxSize;
            subData1.Count = 1;
            subData1.ChildCount = dxCaches.Count;
            subData1.SubData = new List<SubMemoryData>();
            element.SubData.Add(subData1);
            SubMemoryData subData2 = new SubMemoryData();
            subData2.ItemName = "Other Dll";
            subData2.AllocateSize = data.Size.Committed - dxSize;
            subData2.Count = 1;
            subData2.ChildCount = data.ChildCount - dxCaches.Count;
            subData2.SubData = new List<SubMemoryData>();
            element.SubData.Add(subData2);
            result.Add(element);
        }
        void GetNativeData(TreeViewItemData<AllTrackedMemoryModel.ItemData> node, ref List<AllMemoryClass> result)
        {
            var data = node.data;
            AllMemoryClass element = new AllMemoryClass();
            element.GroupName = data.Name;
            element.AllocateSize = data.Size.Committed;
            element.ChildCount = data.ChildCount;
            element.Count = 1;
            element.SubData = new List<SubMemoryData>();
            foreach (var subItemData in node.children)
            {
                SubMemoryData sub = new SubMemoryData();
                sub.Count = 1;
                sub.AllocateSize = subItemData.data.Size.Committed;
                sub.ChildCount = subItemData.data.ChildCount;
                sub.SubData = new List<SubMemoryData>();
                sub.ItemName = subItemData.data.Name;
                if (sub.ChildCount != 0)
                {
                    foreach (var item in subItemData.children)
                    {
                        SubMemoryData su = new SubMemoryData();
                        su.Count = 1;
                        su.AllocateSize = item.data.Size.Committed;
                        su.ChildCount = item.data.ChildCount;
                        su.SubData = new List<SubMemoryData>();
                        su.ItemName = item.data.Name;
                        if (item.data.Name == "Managers")
                        {
                            foreach (var subItem in item.children)
                            {
                                if (subItem.data.Name == "IL2CPPMemoryAllocator")
                                {
                                    SubMemoryData il2cppData = new SubMemoryData();
                                    il2cppData.Count = 1;
                                    il2cppData.AllocateSize = subItem.data.Size.Committed;
                                    il2cppData.SubData = new List<SubMemoryData>();
                                    il2cppData.ItemName = subItem.data.Name;
                                    il2cppData.ChildCount = subItem.data.ChildCount;
                                    su.SubData.Add(il2cppData);
                                    break;
                                }
                            }
                        }
                        else if (item.data.Name == "UnsafeUtility")
                        {
                            foreach (var subItem in item.children)
                            {
                                if (subItem.data.Name == "Malloc(Persistent)")
                                {
                                    SubMemoryData Malloc = new SubMemoryData();
                                    Malloc.Count = 1;
                                    Malloc.AllocateSize = subItem.data.Size.Committed;
                                    Malloc.SubData = new List<SubMemoryData>();
                                    Malloc.ItemName = subItem.data.Name;
                                    Malloc.ChildCount = subItem.data.ChildCount;
                                    su.SubData.Add(Malloc);
                                    break;
                                }
                            }
                        }
                        else if (item.data.Name == "Rendering") //Rendering
                        {
                            foreach (var subItem in item.children)
                            {
                                if (subItem.data.Name == "ComputeBuffers")
                                {
                                    SubMemoryData ComputeBu = new SubMemoryData();
                                    ComputeBu.Count = 1;
                                    ComputeBu.AllocateSize = subItem.data.Size.Committed;
                                    ComputeBu.SubData = new List<SubMemoryData>();
                                    ComputeBu.ItemName = subItem.data.Name;
                                    ComputeBu.ChildCount = subItem.data.ChildCount;
                                    su.SubData.Add(ComputeBu);
                                    break;
                                }
                            }
                        }
                        sub.SubData.Add(su);
                    }
                }
                element.SubData.Add(sub);
            }
            result.Add(element);
        }
        void GetGraphicsData(TreeViewItemData<AllTrackedMemoryModel.ItemData> node, ref List<AllMemoryClass> result)
        {
            // 导出 gfx 和 computerbuffers
            var data = node.data;
            AllMemoryClass element = new AllMemoryClass();
            element.GroupName = data.Name;
            element.Count = 1;
            element.AllocateSize = data.Size.Committed;
            element.ChildCount = data.ChildCount;
            element.SubData = new List<SubMemoryData>();
            if (node.data.ChildCount != 0)
            {
                foreach (var item in node.children)
                {
                    if (item.data.Name == "")  //获取数据 "Rendering:ComputeBuffers"
                    {
                        SubMemoryData noNameData = new SubMemoryData();
                        noNameData.Count = 1;
                        noNameData.SubData = new List<SubMemoryData>();
                        noNameData.AllocateSize = item.data.Size.Committed;
                        noNameData.ChildCount = item.data.ChildCount;
                        noNameData.ItemName = "<No Name>";
                        SubMemoryData elementComputeBuffer = new SubMemoryData();
                        int count = 0;
                        foreach (var itemData in item.children)
                        {
                            if (itemData.data.Name == "Rendering:ComputeBuffers")
                            {
                                elementComputeBuffer.ItemName = itemData.data.Name;
                                elementComputeBuffer.AllocateSize += itemData.data.Size.Committed;
                                count += 1;
                            }
                            else
                            {
                                SubMemoryData idata = new SubMemoryData();
                                idata.Count = 1;
                                idata.SubData = new List<SubMemoryData>();
                                idata.AllocateSize = itemData.data.Size.Committed;
                                idata.ChildCount = itemData.data.ChildCount;
                                idata.ItemName = itemData.data.Name;
                                noNameData.SubData.Add(idata);
                            }
                        }
                        if (count != 0)
                        {
                            elementComputeBuffer.Count = count;
                            elementComputeBuffer.ChildCount = 0;
                            elementComputeBuffer.SubData = new List<SubMemoryData>();
                            noNameData.SubData.Add(elementComputeBuffer);
                        }
                        element.SubData.Add(noNameData);
                    }
                    else
                    {
                        SubMemoryData su = new SubMemoryData();
                        su.Count = 1;
                        su.SubData = new List<SubMemoryData>();
                        su.AllocateSize = item.data.Size.Committed;
                        su.ItemName = item.data.Name;
                        su.ChildCount = item.data.ChildCount;
                        foreach (var itemChild in item.children)
                        {
                            SubMemoryData ic = new SubMemoryData();
                            ic.Count = 1;
                            ic.SubData = new List<SubMemoryData>();
                            ic.AllocateSize = itemChild.data.Size.Committed;
                            ic.ItemName = itemChild.data.Name;
                            ic.ChildCount = itemChild.data.ChildCount;
                            su.SubData.Add(ic);
                        }
                        element.SubData.Add(su);
                    }
                }
            }
            result.Add(element);
        }
        void GetManagedData(TreeViewItemData<AllTrackedMemoryModel.ItemData> node, ref List<AllMemoryClass> result)
        {
            stackDatas = new List<TreeViewItemData<AllTrackedMemoryModel.ItemData>>();
            AllMemoryClass element = new AllMemoryClass();
            element.GroupName = node.data.Name;
            element.AllocateSize = node.data.Size.Committed;
            element.Count = 1;
            element.ChildCount = node.data.ChildCount;
            element.SubData = new List<SubMemoryData>();
            foreach (var nodeChild in node.children)
            {
                if (nodeChild.data.Name == "Reserved")
                {
                    var data = nodeChild.data;
                    SubMemoryData subData = new SubMemoryData();
                    subData.ItemName = data.Name;
                    subData.AllocateSize = data.Size.Committed;
                    subData.ChildCount = data.ChildCount;
                    subData.Count = 1;
                    subData.SubData = new List<SubMemoryData>();
                    element.SubData.Add(subData);
                }
                else if (nodeChild.data.Name == "Managed Objects")
                {
                    var data = nodeChild.data;
                    SubMemoryData subData = new SubMemoryData();
                    subData.ItemName = data.Name;
                    subData.AllocateSize = data.Size.Committed;
                    subData.ChildCount = data.ChildCount;
                    subData.SubData = new List<SubMemoryData>();
                    //提炼Top20子元素数据
                    //int getCount = 0;
                    foreach (var child in nodeChild.children)
                    {
                        SubMemoryData sub = new SubMemoryData();
                        sub.ItemName = child.data.Name;
                        sub.AllocateSize = child.data.Size.Committed;
                        sub.ChildCount = child.data.ChildCount;
                        sub.Count = 1;
                        sub.SubData = new List<SubMemoryData>();
                        subData.SubData.Add(sub);
                        stackDatas.Add(child);
                        // getCount += 1;
                        // if (getCount>=20)
                        // {
                        //     break;
                        // }
                    }
                    subData.Count = 1;
                    element.SubData.Add(subData);
                }
            }
            result.Add(element);
        }
        internal void ProcessManagedDatas(List<TreeViewItemData<AllTrackedMemoryModel.ItemData>> managedObjects, string managedType, ManagedObjectData managedObject, StreamWriter writer, int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                var item = managedObjects[i];

                managedObject.Type = managedType;
                managedObject.ItemName = item.data.Name;
                managedObject.AllocateSize = item.data.Size.Committed;
                managedObject.Referencer.Clear();

                if (FindFirstReferencer(item.data.Source, out var firstReferencer))
                {
                    string name = "";
                    if (firstReferencer.isNative)
                    {
                        name = GetTypeNameOfNativeObject(firstReferencer);
                    }

                    if (firstReferencer.isManaged)
                    {
                        name = GetTypeNameOfManagedObject(firstReferencer);
                    }
                    managedObject.Referencer.Add(name);

                    SetOtherDepthRef(firstReferencer, 1, RefMaxDepth, managedObject.Referencer);
                }

                writer.WriteLine(JsonConvert.SerializeObject(managedObject));
            }
        }

        private bool FindFirstReferencer(CachedSnapshot.SourceIndex sourceIndex, out ObjectData firstReferencer)
        {
            var referencer = ObjectConnection.GetAllReferencingObjects(m_SnapshotDataService.Base, sourceIndex);
            if (referencer.Length == 0)
            {
                firstReferencer = new ObjectData();
                return false;
            }

            firstReferencer = referencer[0];
            return true;
        }

        private bool FindFirstReferencer(ObjectData obj, out ObjectData firstReferencer)
        {
            var referencer = ObjectConnection.GetAllReferencingObjects(m_SnapshotDataService.Base, obj.displayObject);
            if (referencer.Length == 0)
            {
                firstReferencer = new ObjectData();
                return false;
            }

            firstReferencer = referencer[0];
            return true;
        }

        private string GetTypeNameOfNativeObject(ObjectData obj)
        {
            return m_SnapshotDataService.Base.NativeTypes.TypeName[
                m_SnapshotDataService.Base.NativeObjects.NativeTypeArrayIndex[
                    obj.displayObject.nativeObjectIndex]];
        }

        private string GetTypeNameOfManagedObject(ObjectData obj)
        {
            return m_SnapshotDataService.Base.TypeDescriptions.TypeDescriptionName[
                    obj.displayObject.managedTypeIndex];
        }

        void SetOtherDepthRef(ObjectData obj, int depth, int maxDepth, List<string> referencers)
        {
            if (depth >= maxDepth)
            {
                return;
            }
            string name = "";

            if (FindFirstReferencer(obj, out var firstReferencer))
            {
                if (firstReferencer.isNative)
                {
                    name = GetTypeNameOfNativeObject(firstReferencer);
                }

                if (firstReferencer.isManaged)
                {
                    name = GetTypeNameOfManagedObject(firstReferencer);
                }

                referencers.Add(name);
                SetOtherDepthRef(firstReferencer, depth + 1, maxDepth, referencers);
            }
        }

        void ProcessObjectSelected(int itemId, AllTrackedMemoryModel.ItemData itemData)
        {

        }
        void ProcessUnityObjectItemSelectedInvoke(int itemId, UnityObjectsModel.ItemData itemData)
        {

        }

        public void LoadedSnapshot(string filePath)
        {
            m_SnapshotDataService.Load(filePath);
        }

        public bool IsLoadSuccess()
        {
            return m_SnapshotDataService.Base.Valid;
        }

        public void UnloadSnapshot(string filePath)
        {
            m_SnapshotDataService.Unload(filePath);
        }
        #endregion

        public void Init()
        {
            m_WindowInitialized = true;

            m_SnapshotDataService = new SnapshotDataService();
            m_PlayerConnectionService = new PlayerConnectionService(m_SnapshotDataService);

            // Analytics
            MemoryProfilerAnalytics.EnableAnalytics();

            m_ProfilerViewController = new MemoryProfilerViewController(m_PlayerConnectionService, m_SnapshotDataService);
        }

        void OnDisable()
        {
            m_WindowInitialized = false;

            m_ProfilerViewController?.Dispose();
            m_ProfilerViewController = null;

            m_PlayerConnectionService?.Dispose();
            m_PlayerConnectionService = null;

            m_SnapshotDataService?.Dispose();
            m_SnapshotDataService = null;

            MemoryProfilerAnalytics.DisableAnalytics();
        }
    }
}
