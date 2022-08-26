#if UNITY_5_3_OR_NEWER
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MemoryProfilerWindow;
using UnityEditor;
using UnityEditor.MemoryProfiler;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

public class SnapshotUtil
{
    public static void ConvertMemorySnapshotIntoJson(PackedMemorySnapshot packed, int frame, string case_uuid)
    {
        try
        {
            int nanum = packed.connections.Length;
            var crawled = new Crawler().Crawl(packed);
            Dictionary<int, NativeObject> allnative = new Dictionary<int, NativeObject>();
            CrawledMemorySnapshot _unpacked = CrawlDataUnpacker.Unpack(crawled, nanum, ref allnative);
            Dictionary<string, MemNativeRowObject> nativeobejcts = new Dictionary<string, MemNativeRowObject>();
            Dictionary<string, MemManageRowObject> manageobjects = new Dictionary<string, MemManageRowObject>();
            List<TopNativeObject> nativeobject = new List<TopNativeObject>();
            List<TopManagedObject> manageobject = new List<TopManagedObject>();
            Dictionary<long, string> allNamehash = new Dictionary<long, string>();
            #region 复用对象
            ThingInMemory item = new ThingInMemory();
            string groupName = string.Empty;
            string typeName = string.Empty;
            int refCount = 0;
            int[] referencedByIds = null;
            int[] referencesIds = null;
            #endregion

            #region 处理大类
            for (int i = 0; i < _unpacked.allObjects.Length; i++)
            {
                item = _unpacked.allObjects[i];
                groupName = MemUtil.GetGroupName(item);
                typeName = MemUtil.GetCategoryLiteral(item);
                refCount = item.referencedBy.Length;
                referencedByIds = item.referencedBy.Select(r => r.id).ToArray();
                referencesIds = item.references.Select(r => r.id).ToArray();

                #region 处理
                var allKey = groupName + typeName;
                if (MemUtil.GetCategoryLiteral(item) == "native")
                {
                   
                    var nativeobj = item as NativeUnityEngineObject;
                    MemNativeObject currentitem = new MemNativeObject();
                    currentitem.id = item.id;
                    currentitem.name_id = item.caption.GetHashCode();
                    if (!allNamehash.ContainsKey(currentitem.name_id))
                    {
                        allNamehash.Add(currentitem.name_id, item.caption);
                    }
                    currentitem.s = item.size;
                    currentitem.c = refCount;
                    currentitem.a = nativeobj.nativeObjectAddress;
                    currentitem.g = groupName;
                    currentitem.tn = typeName;
                    currentitem.inid = nativeobj.instanceID;
                    currentitem.isd = nativeobj.isDontDestroyOnLoad;
                    currentitem.isp = nativeobj.isPersistent;
                    currentitem.ism = nativeobj.isManager;
                    currentitem.refi = allnative[currentitem.inid].references.ToArray();
                    currentitem.refb = allnative[currentitem.inid].referencedBy.ToArray();
                    TopNativeObject top = new TopNativeObject();
                    top.name = item.caption;
                    top.type = groupName;
                    top.size = item.size;
                    nativeobject.Add(top);
                    if (nativeobejcts.ContainsKey(allKey))
                    {
                        nativeobejcts[allKey].size += item.size;
                        nativeobejcts[allKey].count += 1;
                        nativeobejcts[allKey].eachObject.Add(currentitem);
                    }
                    else
                    {
                        MemNativeRowObject memItem = new MemNativeRowObject();
                        memItem.size = item.size;
                        memItem.groupName = groupName;
                        memItem.typeName = typeName;
                        memItem.count = 1;
                        memItem.eachObject = new List<MemNativeObject>();
                        memItem.eachObject.Add(currentitem);
                        nativeobejcts.Add(allKey, memItem);
                    }
                }
                else if (MemUtil.GetCategoryLiteral(item) == "managed")
                {
                    var manageobj = item as ManagedObject;
                    MemManagedObject currentitem = new MemManagedObject();
                    currentitem.id = item.id;
                    currentitem.name_id = item.caption.GetHashCode();
                    if (!allNamehash.ContainsKey(currentitem.name_id))
                    {
                        allNamehash.Add(currentitem.name_id, item.caption);
                    }
                    currentitem.isa = manageobj.typeDescription.isArray;
                    currentitem.s = item.size;
                    currentitem.refc = refCount;
                    currentitem.a = manageobj.address;
                    currentitem.ab = manageobj.typeDescription.assembly;
                    currentitem.fields = manageobj.typeDescription.fields;
                    currentitem.g = groupName;
                    currentitem.tn = typeName;
                    currentitem.refi = referencesIds;
                    currentitem.refb = referencedByIds;
                    TopManagedObject top = new TopManagedObject();
                    top.address = currentitem.a;
                    top.type = groupName;
                    top.size = item.size;
                    manageobject.Add(top);
                    if (manageobjects.ContainsKey(allKey))
                    {
                        manageobjects[allKey].size += item.size;
                        manageobjects[allKey].count += 1;
                        manageobjects[allKey].eachObject.Add(currentitem);
                    }
                    else
                    {
                        MemManageRowObject memItem = new MemManageRowObject();
                        memItem.size = item.size;
                        memItem.groupName = groupName;
                        memItem.typeName = typeName;
                        memItem.count = 1;
                        memItem.eachObject = new List<MemManagedObject>();
                        memItem.eachObject.Add(currentitem);
                        manageobjects.Add(allKey, memItem);
                    }
                }
                else
                {
                    continue;
                }
                #endregion
            }
            #endregion

            #region 处理分类/保存
            List<TopNativeObject> nativeobject30 = new List<TopNativeObject>();
            List<TopManagedObject> manageobject30 = new List<TopManagedObject>();
            //nativeobject30
            nativeobject.Sort((x, y) =>
            {
                int res = 0;
                if (x.size < y.size) res = -1;
                else if (x.size > y.size) res = 1;
                return -res;
            });
            foreach (var ite in nativeobject)
            {
                nativeobject30.Add(ite);
                if (nativeobject30.Count == 30)
                {
                    break;
                }
            }
            //manggeobject30
            manageobject.Sort((x, y) =>
            {
                int res = 0;
                if (x.size < y.size) res = -1;
                else if (x.size > y.size) res = 1;
                return -res;
            });
            foreach (var itea in manageobject)
            {
                manageobject30.Add(itea);
                if (manageobject30.Count == 30)
                {
                    break;
                }
            }
            nativeobject.Clear();
            manageobject.Clear();
            #endregion
            Debug.Log("完成解析且保存数据");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogFormat("ConvertMemorySnapshotIntoJson() interrupted.");

        }
    }

    //private static string StringToMD5(string strText)
    //{
    //    MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
    //    byte[] result = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(strText));
    //    return BitConverter.ToString(result).Replace("-", "");
    //}
}
#endif