using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class HelpTools
{
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
    /// 取得本地时间
    /// </summary>
    /// <returns></returns>
    public static DateTime GetLocalTime()
    {
        return DateTime.Now.ToUniversalTime();
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
