using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IWshRuntimeLibrary;


namespace ctxmgr.Utilities
{
    public class AutoStartHelper
    {
        // 获取当前用户的启动文件夹路径
        private static string StartupPath =>
            Environment.GetFolderPath(Environment.SpecialFolder.Startup);

        // 获取当前应用程序的完整路径
        private static string AppFullPath =>
            System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;

        // 快捷方式的名称（可以根据需要修改）
        private static string ShortcutName => "FlashPad";

        /// <summary>
        /// 设置开机自动启动
        /// </summary>
        /// <param name="enable">true:启用自启动, false:禁用自启动</param>
        /// <returns>操作是否成功</returns>
        public static bool SetAutoStart(bool enable)
        {
            try
            {
                string shortcutFullPath = Path.Combine(StartupPath, $"{ShortcutName}.lnk");

                if (enable)
                {
                    // 创建快捷方式到启动文件夹
                    return CreateShortcut(StartupPath, ShortcutName, AppFullPath);
                }
                else
                {
                    // 从启动文件夹删除快捷方式
                    if (System.IO.File.Exists(shortcutFullPath))
                    {
                        System.IO.File.Delete(shortcutFullPath);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                // 实际项目中可进行日志记录
                Console.WriteLine($"设置自启动时出错: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查是否已设置开机自启动
        /// </summary>
        public static bool IsAutoStartEnabled()
        {
            string shortcutFullPath = Path.Combine(StartupPath, $"{ShortcutName}.lnk");
            return System.IO.File.Exists(shortcutFullPath);
        }

        /// <summary>
        /// 创建快捷方式
        /// </summary>
        private static bool CreateShortcut(string directory, string shortcutName, string targetPath, string description = "")
        {
            try
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string shortcutPath = Path.Combine(directory, $"{shortcutName}.lnk");

                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);

                // 设置快捷方式属性
                shortcut.TargetPath = targetPath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
                shortcut.WindowStyle = (int)WshWindowStyle.WshMinimizedNoFocus;
                shortcut.Description = description;
                shortcut.IconLocation = targetPath; // 使用应用程序自身图标
                shortcut.Save();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"创建快捷方式时出错: {ex.Message}");
                return false;
            }
        }
    }
}
