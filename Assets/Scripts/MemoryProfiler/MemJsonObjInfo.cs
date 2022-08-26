using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LitJson;
using UnityEditor.MemoryProfiler;

class MemObj
{
    public static void AllMemToJson(Dictionary<string, MemTypeNameObject> dicAllObjects, string resultSavePath)
    {
        //var jsonContent = "{{\"all\":[{0}]}}";
        var everyMemObject = "{{\"i\":{0}, \"n\":\"{1}\", \"s\":{2}, \"t\":\"{3}\", \"c\":{4}}}";

        var isFirst = true;
        MemTypeNameObject allItem = null;
        using (StreamWriter sw = new StreamWriter(resultSavePath))
        {
            sw.Write("{\"all\":[");
            foreach (var item in dicAllObjects)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sw.Write(",");

                allItem = item.Value;
                sw.Write(string.Format(everyMemObject, allItem.index, allItem.groupName, allItem.size, allItem.typeName, allItem.count));
            }
            sw.Write("]}");
        }
    }

    public static void SaveAsJson(Dictionary<string, MemTypeNameObject> result, string resultPath)
    {
        string resultJson = JsonMapper.ToJson(result);
        using (var sw = new StreamWriter(new FileStream(resultPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), new System.Text.UTF8Encoding(false)))
        {
            sw.Write(resultJson);
        }
    }

    public static void SaveAllMemJson(List<MemInstanceNameObject> instances, string resultPath)
    {
        string resultJson = JsonMapper.ToJson(instances);
        using (var sw = new StreamWriter(new FileStream(resultPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), new System.Text.UTF8Encoding(false)))
        {
            sw.Write(resultJson);
        }
    }

    public static void InstanceMemToJson(List<MemInstanceNameObject> instances, string eachSaveFullPath)
    {
        //var allContent = "{{\"each\":[{0}]}}";
        var eachArray = "{{\"n\":\"{0}\", \"s\":{1}, \"r\":{2}, \"a\":\"{3}\", \"t\":\"{4}\", \"h\":\"{5}\"}}";

        var isFirst = true;
        using (StreamWriter sw = new StreamWriter(eachSaveFullPath))
        {
            sw.Write("{\"each\":[");
            foreach (var item in instances)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sw.Write(",");

                sw.Write(string.Format(eachArray, item.groupName, item.size, item.refCount, item.address, item.typeName, item.md5));
            }
            sw.Write("]}");
        }
    }

    public static void InstanceMemToText(List<MemInstanceNameObject> instances, string saveFullPath, int frame = 0)
    {
        var sb = new StringBuilder();
        var stringFormat = "{0}@@{1}@@{2}@@{3}@@{4}@@{5}@@{6}@@{7}@@{8}@@{9}@@{10}";
        foreach (var item in instances)
        {
            sb.AppendLine(string.Format(stringFormat, frame, item.id, item.caption, item.typeName, item.groupName, item.size, String.IsNullOrEmpty(item.address) ? "null" : item.address, item.refCount,
                item.referencesIds.Length > 0 ? String.Join(",", item.referencesIds.Select(r => r.ToString()).ToArray()) : "null", item.referencedByIds.Length > 0 ? String.Join(",", item.referencedByIds.Select(r => r.ToString()).ToArray()) : "null", item.md5));
        }

        using (StreamWriter sw = new StreamWriter(saveFullPath))
        {
            sw.Write(sb);
        }
    }

    //public static void SaveSnapHash(Dictionary<long,string> allname, IMongoDatabase db)
    //{
    //    foreach (var item in allname)
    //    {
    //        FilterDefinition<SnapHashList> query = Builders<SnapHashList>.Filter.Eq("name", item.Value);
    //        UpdateDefinition<SnapHashList> update = Builders<SnapHashList>.Update.SetOnInsert("_id", item.Key).SetOnInsert("name", item.Value);
    //        DB.Instance.IntsertFunhash(db, query, update);
    //    }
    //}

    //public static void SaveRowToDB(string case_uuid, int frame, Dictionary<string, MemNativeRowObject> nativeobejcts, Dictionary<string, MemManageRowObject> manageobjects, List<TopNativeObject> nativeobject, List<TopManagedObject> manageobject, IMongoDatabase db)
    //{
    //    MemNativeRow native = new MemNativeRow();
    //    native.allobjects = new List<MemNativeRowObject>();
    //    MemManageRow manage = new MemManageRow();
    //    manage.allobjects = new List<MemManageRowObject>();
    //    MemFunRowTop memtop = new MemFunRowTop();
    //    memtop.case_uuid = case_uuid;
    //    memtop.frame_id = frame;
    //    memtop.topnative = nativeobject;
    //    memtop.topmanage = manageobject;

    //    #region native处理+排序
    //    foreach (var item in nativeobejcts.Values)
    //    {
    //        native.objectsCount += item.count;
    //        native.totaSize += item.size;
    //        native.allobjects.Add(item);
    //    }
    //    native.allobjects.Sort((x, y) =>
    //    {
    //        int res = 0;
    //        if (x.size < y.size) res = -1;
    //        else if (x.size > y.size) res = 1;
    //        return -res;
    //    });
    //    #endregion
    //    #region managed处理+排序
    //    foreach (var it in manageobjects.Values)
    //    {
    //        manage.objectsCount += it.count;
    //        manage.totaSize += it.size;
    //        manage.allobjects.Add(it);
    //    }
    //    manage.allobjects.Sort((x, y) =>
    //    {
    //        int res = 0;
    //        if (x.size < y.size) res = -1;
    //        else if (x.size > y.size) res = 1;
    //        return -res;
    //    });
    //    #endregion
    //    string nativeFilename = string.Format("native-{0}.json", frame);
    //    string manageFilename = string.Format("managed-{0}.json", frame);
    //    DB.Instance.UploadFile(native, nativeFilename, db, case_uuid);
    //    DB.Instance.UploadFile(manage, manageFilename, db, case_uuid);
    //    DB.Instance.Insert(memtop,db);
    //}
}

public class MemNativeRow
{
    public long totaSize { get; set; }
    public int objectsCount { get; set; }
    public  List<MemNativeRowObject> allobjects { get; set; }
}

public class MemManageRow
{
    public long totaSize { get; set; }
    public int objectsCount { get; set; }
    public List<MemManageRowObject> allobjects { get; set; }
}

public class MemFunRowTop
{
    public string case_uuid { get; set; }
    public int frame_id { get; set; }
    public List<TopNativeObject> topnative { get; set; }
    public List<TopManagedObject> topmanage { get; set; }
}


[System.Serializable]
public class MemTypeNameObject
{
    public int index = 0;
    public string groupName = string.Empty;
    public long size = -1;
    public string typeName = string.Empty;
    public int count = -1;
    public List<MemInstanceNameObject> eachObject { get; set; }
}

[System.Serializable]
public class MemInstanceNameObject
{
    public int id;
    public string caption = string.Empty;
    public int size = -1;
    public int refCount = -1;
    public string address = string.Empty;
    public string groupName = string.Empty;
    public string typeName = string.Empty;
    public string md5 = string.Empty;
    public int[] referencesIds;
    public int[] referencedByIds;
}

[System.Serializable]
public class MemNativeRowObject
{
    public string groupName = string.Empty;
    public long size = -1;
    public string typeName = string.Empty;
    public int count = -1;
    public List<MemNativeObject> eachObject { get; set; }
}

[System.Serializable]
public class MemManageRowObject
{
    public string groupName = string.Empty;
    public long size = -1;
    public string typeName = string.Empty;
    public int count = -1;
    public List<MemManagedObject> eachObject { get; set; }
}

public class MemManagedObject
{
    public int id { get; set; }
    public int name_id { get; set; }   //name=>name_id
    public long s { get; set; }    //size=>s
    public bool isa { get; set; }  //isArray=>isa
    public int refc { get; set; }  //refCount=>refc
    public FieldDescription[] fields { get; set; }
    public ulong a { get; set; } //address=>a
    public string ab { get; set; }  //assembly=>ab
    public string g { get; set; }  //groupName=>g
    public string tn { get; set; }  //typeName=>tn
    public int[] refi { get; set; }  //referencesIds=>ref
    public int[] refb { get; set; }  //referencedByIds=>refb
}

public class MemNativeObject
{
    public int id { get; set; }
    public int name_id { get; set; }   //name=>name_id
    public long s { get; set; }   //size=>s
    public int c { get; set; }  //refCount=>c
    public long a { get; set; }  //address=>a
    public string g { get; set; }  //groupName=>g
    public string tn { get; set; }  //typeName=>tn
    public int inid { get; set; }  //instanceid=>inid
    public bool isp { get; set; } //isPersistent=>isp
    public bool ism { get; set; } //isManager=>ism
    public bool isd { get; set; } //isDontDestroyOnLoad=>isd
    public int[] refi { get; set; } //referencesIds=>refi
    public int[] refb { get; set; } //referencedByIds=>refb
}

public class TopNativeObject
{
    public string name { get; set; }
    public string type { get; set; }
    public long size { get; set; }
}

public class TopManagedObject
{
    public ulong address { get; set; }
    public string type { get; set; }
    public long size { get; set; }
}