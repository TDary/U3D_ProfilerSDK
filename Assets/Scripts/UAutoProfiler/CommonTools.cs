using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CommonTools
{


    /// <summary>
    /// hash算法
    /// </summary>
    /// <param name="protoName"></param>
    /// <returns></returns>
    public static long UniqueHash(string str)
    {
        long hash = 0;
        int pLength = str.Length;

        for (int index = 0; index < pLength; ++index)
        {
            int tmp = str[index] - 31;
            hash = hash + tmp * (index + 1);
        }

        hash = hash * 100 + str[pLength - 3] - 31;
        if (pLength > 3)
        {
            hash = hash * 100 + str[pLength - 4] - 31;
        }
        if (pLength > 4)
        {
            hash = hash * 100 + str[pLength - 5] - 31;
        }

        hash = hash * 100 + pLength;

        return hash;
        //long hash = 5381;
        ////int c;
        //for (int i = 0; i < str.Length; i++)
        //{
        //    hash = ((hash << 5) + hash) + str[i]; /* hash * 33 + c */
        //}
        //return hash;
        //long hash = 0;
        //int pLength = str.Length;

        //for (int index = 0; index < pLength; ++index)
        //{
        //    int tmp = str[index] - 31;
        //    hash = hash + tmp * (index + 1);
        //}

        //hash = hash * 100 + str[pLength - 1] - 31;
        //hash = hash * 100 + pLength;


        //return hash;
    }
    //public static ulong hash(string str)
    //{
    //    ulong hash = 5381;
    //    int c;
    //    for (int i = 0; i < str.Length; i++)
    //    {
    //        hash = ((hash << 5) + hash) + str[i]; /* hash * 33 + c */
    //    }
    //    return hash;
    //}

    public static uint GenerateGUIDUint()
    {
        string shardField = Guid.NewGuid().ToString();
        uint code = 0;
        shardField = shardField.Trim();
        for (int i = 0; i < shardField.Length; i += 2)
        {
            code *= 16777619;
            code ^= shardField[i];
        }

        return code;
    }

    public static long GenerateGUIDLong()
    {
        uint h = CommonTools.GenerateGUIDUint();
        uint l = CommonTools.GenerateGUIDUint();
        long t = ((long)h << 32);
        long code = t + (long)l;

        return code;
    }



    /// <summary>
    /// 取得真实的本地时间(从格林威治19700101开始算) 单位秒
    /// </summary>
    /// <returns></returns>
    public static uint GetLocalTimeSeconds()
    {
        long nowtime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;  //真实的系统时间
        return (uint)nowtime;
    }

    /// <summary>
    /// 获得系统时间毫秒
    /// </summary>
    /// <returns></returns>
    public static long GetLocalTimeMS()
    {
        long nowtime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;  //真实的系统时间
        return nowtime;
    }

    /// <summary>
    /// 取得本地时间
    /// </summary>
    /// <returns></returns>
    public static DateTime GetLocalTime()
    {
        return DateTime.Now.ToUniversalTime();
    }

    /// <summary>
    /// unix时间戳转DateTime
    /// </summary>
    /// <param name="timeStamp">单位是秒</param>
    public static DateTime TimeStamp2DateTime(uint timeStamp)
    {
#pragma warning disable CS0618 // 类型或成员已过时
        System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
#pragma warning restore CS0618 // 类型或成员已过时
        DateTime dt = startTime.AddSeconds(timeStamp);

        return dt;
    }


    /// <summary>
    /// 返回当前时间戳的格式化字符串 类似2005-11-5 14:23:23
    /// </summary>
    /// <returns></returns>
    public static string GetLocalTimeFormat()
    {
        System.DateTime dateTime = TimeStamp2DateTime(GetLocalTimeSeconds());
        return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    /// <summary>
    /// 将格式化之后的日期再转回时间戳
    /// </summary>
    /// <param name="timeStr"></param>
    /// <returns></returns>
    public static DateTime ToDateTime(string timeStr)
    {
        DateTime dt = Convert.ToDateTime(timeStr);
        return dt;
    }



    /// <summary>
    /// 复制文件夹所有文件
    /// </summary>
    /// <param name="sourcePath">源目录</param>
    /// <param name="destPath">目的目录</param>
    public static void CopyFolder(string sourcePath, string destPath)
    {
        if (Directory.Exists(sourcePath))
        {
            if (!Directory.Exists(destPath))
            {
                //目标目录不存在则创建
                try
                {
                    Directory.CreateDirectory(destPath);
                }
                catch (Exception ex)
                {
                    throw new Exception("创建目标目录失败：" + ex.Message);
                }
            }
            //获得源文件下所有文件
            List<string> files = new List<string>(Directory.GetFiles(sourcePath));
            files.ForEach(c =>
            {
                string destFile = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                File.Copy(c, destFile, true);//覆盖模式
            });
            //获得源文件下所有目录文件
            List<string> folders = new List<string>(Directory.GetDirectories(sourcePath));
            folders.ForEach(c =>
            {
                string destDir = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                //采用递归的方法实现
                CopyFolder(c, destDir);
            });

        }
    }


}
