using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using System.IO;
using System;
using UnityEditorInternal;

namespace ProfileMemory
{
  
    public class ExtractMemoryInfo:EditorWindow
    {
        private static ExtractMemoryInfo Instance;
        [MenuItem("Tool/GetMemoryDetailed")]
        public static void GetMemoryDetails()
        {
            if (Instance == null)
            {
                Instance = ScriptableObject.CreateInstance<ExtractMemoryInfo>();
            }
            Instance.Show();
        }
        private void OnGUI()
        {
            string connectIP = string.Empty;
            GUILayout.Label("请输入要连接的IP：");
            connectIP = GUILayout.TextField(connectIP);
            if (GUILayout.Button("获取一次内存数据"))
            {
                if (!string.IsNullOrEmpty(connectIP))
                {
                    ExtractMemoryDetailed(connectIP);
                }
            }
        }
        public static void ExtractMemoryDetailed(string ip)
        {
            //打开Profiler窗口
            EditorApplication.ExecuteMenuItem("Window/Analysis/Profiler");
            ProfilerDriver.DirectIPConnect(ip);   //连接机器ip，需开启游戏
            var ProfilerWindow = typeof(EditorWindow).Assembly.GetType("UnityEditor.ProfilerWindow");
            var MemoryProfilerModule = typeof(EditorWindow).Assembly.GetType("UnityEditorInternal.Profiling.MemoryProfilerModule");
#if UNITY_2020_1_OR_NEWER
            var ProfilerWindows = ProfilerWindow.GetField("s_ProfilerWindows", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetField).GetValue(null) as IList;
#else
            var ProfilerWindows = ProfilerWindow.GetField("m_ProfilerWindows", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetField).GetValue(null) as IList;
#endif
            if (ProfilerWindows.Count == 0)
            {
                Debug.LogError("Profile窗口未打开");
                return;
            }
            var GetProfilerModule = ProfilerWindows[0].GetType().GetMethod("GetProfilerModule", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(MemoryProfilerModule);
            var memoryModule = GetProfilerModule.Invoke(ProfilerWindows[0], new System.Object[] { ProfilerArea.Memory });
            //截取一帧，Take Sample Editor
            var RefreshMemoryData = memoryModule.GetType().GetMethod("RefreshMemoryData", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            RefreshMemoryData.Invoke(memoryModule, new System.Object[] { });
            //内存数据在m_MemoryListView里面
            var m_MemoryListView = MemoryProfilerModule.GetField("m_MemoryListView", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).GetValue(memoryModule);
            var m_Root = m_MemoryListView.GetType().GetField("m_Root", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField).GetValue(m_MemoryListView);

            MemoryElement data = MemoryElement.Create(m_Root, -1);
            string dirName = "MemoryDetailed";
            string fileName = string.Format("MemoryDetailed{0:yyyy_MM_dd_HH_mm_ss}.txt", DateTime.Now);
            string outputPath = string.Format("{0}/{1}/{2}", System.Environment.CurrentDirectory, dirName, fileName);
            string dir = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            StreamWriter writer = new StreamWriter(outputPath);
            WriteMemoryDetail(writer, data);
            writer.Flush();
            writer.Close();
            Debug.Log(string.Format("提取Profile内存数据完成（{0}）", outputPath));
        }

        public static void WriteMemoryDetail(StreamWriter writer, MemoryElement root)
        {
            if (root == null) return;
            writer.WriteLine(root.ToString());
            foreach (var memoryElement in root.children)
            {
                if (memoryElement != null)
                {
                    WriteMemoryDetail(writer, memoryElement);
                }
            }
        }
    }
    public class MemoryElement
    {
        private int depth;
        public string name;
        public int totalChildCount;
        public long totalMemory;
        public List<MemoryElement> children = new List<MemoryElement>();
        public static MemoryElement Create(object root, int depth)
        {
            if (root == null) return null;
            MemoryElement memoryElement = new MemoryElement { depth = depth };
            memoryElement.name = (string)root.GetType().GetField("name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField).GetValue(root);
            memoryElement.totalMemory = (long)root.GetType().GetField("totalMemory", BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField).GetValue(root);
            memoryElement.totalChildCount = (int)root.GetType().GetField("totalChildCount", BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField).GetValue(root);
            var children = root.GetType().GetField("children", BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField).GetValue(root) as IList;
            if (children != null)
            {
                foreach (var child in children)
                {
                    if (child == null) continue;
                    //递归遍历
                    MemoryElement t_memoryElement = Create(child, depth + 1);
                    memoryElement.children.Add(t_memoryElement);
                }
            }
            return memoryElement;
        }
        public override string ToString()
        {
            if (depth < 0)
            {
                return string.Format("totalMemory:{0}B", totalMemory);
            }
            if (children.Count > 0)
            {
                string resultString = string.Format(new string('\t', depth) + "{0},({1}),{2}B", name, totalChildCount, totalMemory);
                return resultString;
            }
            else
            {
                string resultString = string.Format(new string('\t', depth) + "{0},{1}B", name, totalMemory);
                return resultString;
            }
        }
    }
}
