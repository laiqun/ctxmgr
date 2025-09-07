
# 文件
[x] 从文件载入
[x] 保存到文件
[x] 隐藏窗口
[x] 退出


# 编辑
[x]撤销
[x]重做
[x]剪切
[x]复制
[x]粘贴
[]查找 []查找下一个  []查找上一个 
[]替换
[x]全选
# 页面
[x]新建
[x]删除
[x]编辑标题
[x]向左移动  还没有添加拖动 和快捷键
[x]向右移动  还没有添加拖动 和快捷键
[x]移动到最左边 还没有添加拖动 和快捷键
[x]移动到最右边 还没有添加拖动 和快捷键
# 格式
[x]浅色模式 
[x]深色模式 
[x]自动换行
[]字体 前景色
[]背景色
# 插入
[x]插入分割线
[x]插入日期
[x]插入时间日期分割线
[x]插入文本设置
# 工具
[x] 显示在最上层
[] 设置
# 帮助
[x] 软件说明
[] 快捷键列表
[x] 关于
# 设置项
[x]系统启动时自动运行  
[x]双击页面标题时的行为 
[]更换显示/隐藏主窗口 快捷键  [] 快捷键绑定
[x]插入文本片段
- [x] 分割线
- [x] 时间日期
- [x] 时间日期分割线
- [x] 自定义 添加/删除 修改
- [x]根据自定义创建MenuItem，点击插入


# 标签的保存于加载
应用启动加载所有的标签页面
## 保存所有的标签页面
保存策略：不做全量保存，速度太慢
* 窗口关闭时 异步，但需要等待执行结束  未完成，必要性不大
* tab内容变化时，需要加节流，而且要异步，不能卡界面   已完成
* 页面增加/删除时 需要异步    已完成
* tab名字发生变化时 需要异步   已完成
* 程序关闭时，可以只保存当前tab  异步，但需要等待执行结束  未完成，必要性可能不大

# 添加和取消开机启动 完成 开机启动时最小化窗口

# 给Menuitem添加访问键 完成

# 修复短时间快速多次模拟复制粘贴 COM crash的问题  完成，改成了监听全局剪切板事件
```
System.Runtime.InteropServices.COMException
  HResult=0x800401D0
  Message=OpenClipboard 失败 (0x800401D0 (CLIPBRD_E_CANT_OPEN))
  Source=System.Private.CoreLib
  StackTrace:
   at System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Int32 errorCode)
   at System.Windows.Clipboard.CriticalSetDataObject(Object data, Boolean copy)
   at ctxmgr.GlobalHotkeyManager.HwndHook(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, Boolean& handled) in C:\Users\Administrator\source\repos\ctxmgr\GlobalHotkeyManager.cs:line 78
   at System.Windows.Interop.HwndSource.PublicHooksFilterMessage(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, Boolean& handled)
   at MS.Win32.HwndWrapper.WndProc(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, Boolean& handled)
   at System.Windows.Threading.ExceptionWrapper.InternalRealCall(Delegate callback, Object args, Int32 numArgs)
   at System.Windows.Threading.ExceptionWrapper.TryCatchWhen(Object source, Delegate callback, Object args, Int32 numArgs, Delegate catchHandler)
   at System.Windows.Threading.Dispatcher.LegacyInvokeImpl(DispatcherPriority priority, TimeSpan timeout, Delegate method, Object args, Int32 numArgs)
   at MS.Win32.HwndSubclass.SubclassWndProc(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam)
   at MS.Win32.UnsafeNativeMethods.DispatchMessage(MSG& msg)
   at System.Windows.Threading.Dispatcher.PushFrameImpl(DispatcherFrame frame)
   at System.Windows.Application.RunDispatcher(Object ignore)
   at System.Windows.Application.RunInternal(Window window)
   at ctxmgr.App.Main()

```