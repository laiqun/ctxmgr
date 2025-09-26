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
[x]查找 [x]查找下一个  [x]查找上一个 
[x]替换
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
[x]字体 字体颜色 前景色
[x]背景色
# 插入
[x]插入分割线
[x]插入日期
[x]插入时间日期分割线
[x]插入文本设置
# 工具
[x] 显示在最上层
[x] 设置
# 帮助
[x] 软件说明
[x] 快捷键列表
[x] 关于
# 设置项
[x]系统启动时自动运行  
[x]双击页面标题时的行为 
[x]更换显示/隐藏主窗口 快捷键  [x] 快捷键绑定
[x]插入文本片段
- [x] 分割线
- [x] 时间日期
- [x] 时间日期分割线
- [x] 自定义 添加/删除 修改
- [x] 根据自定义创建MenuItem，点击插入


# 标签的保存于加载
应用启动加载所有的标签页面
## 保存所有的标签页面
保存策略：不做全量保存，速度太慢
* 窗口关闭时 异步，但需要等待执行结束  未完成，必要性不大
* tab内容变化时，需要加节流，而且要异步，不能卡界面   已完成  去掉 改成失去焦点保存，它失去焦点保存自己； 整个窗口失去焦点，保存当前tab
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
# 要加的功能
* 集成IronPython，直接在软件内计算表达式，算术运算，字符串处理，日期处理等。  取消，Ironpython太慢
  我之前都是，win+r，输入 python，回车，然后输入表达式。或者打开浏览器，鼠标右键，审查元素，输入表达式
[x]表达式计算
[]表达式计算，增加hex和bin的支持，我经常用这两个函数
	[]增加一个计算器插件，集成到软件内，方便计算
	[]增加一个翻译插件，集成到软件内，方便翻译
	[]增加一个词典插件，集成到软件内，方便查词
	[]增加一个天气插件，集成到软件内，方便查天气
	[]增加一个日历插件，集成到软件内，方便查日期
	[]增加一个记事本插件，集成到软件内，方便记事
	[]增加一个待办事项插件，集成到软件内，方便管理待办事项
	[]增加一个番茄钟插件，集成到软件内，方便管理时间
	[]增加一个RSS阅读器插件，集成到软件内，方便阅读新闻
	[]增加一个股票行情插件，集成到软件内，方便查看股票行情
	[]增加一个汇率转换插件，集成到软件内，方便转换汇率
	[]增加一个单位转换插件，集成到软件内，方便转换单位
	[]增加一个二维码生成插件，集成到软件内，方便生成二维码
	[]增加一个条形码生成插件，集成到软件内，方便生成条形码
	[]增加一个图片查看器插件，集成到软件内，方便查看图片
	[]增加一个PDF查看器插件，集成到软件内，方便查看PDF文件
	[]增加一个Markdown预览插件，集成到软件内，方便预览Markdown文件
	[]增加一个HTML预览插件，集成到软件内，方便预览HTML文件
	[]增加一个JSON格式化插件，集成到软件内，方便格式化JSON数据
	[]增加一个XML格式化插件，集成到软件内，方便格式化XML数据
	[]增加一个CSV格式化插件，集成到软件内，方便格式化CSV数据
	[]增加一个正则表达式测试插件，集成到软件内，方便测试正则表达式

[x]快捷建 选择文件/文件夹
[x]快速把当前tab的内容粘贴出来  减少手动复制粘贴 * 快捷粘贴选中标签页的所有内容（减少手动复制粘贴）
[x] ctrl+q可以显示一个toast

[x]在线更新功能
[x]系统托盘  草稿本完成

[]集成LLM 调用 ，prompt优化，预定义的prompt，设置prompt在 头部，尾部 或者两个位置都有,降低优先级
[]文档（实时）同步
* [ ] 局域网内，公告板的功能  比如要发一个通知啊，局域网内的同事都能看到
* [ ] 通过网页链接分享tab内容

[]自定义插件系统 比如BMI计算   优先级不高

# 竞品
## minipad2
* 细节不太舒服，Ctrl+F 查找框打开后，按Esc不能关闭查找框。  已处理，但要注意以后新建的window也要支持
* 写长文章时，没写完就将软件关闭的情况下，重新打开时，软件会自动显示在你上次写到的地方，不必拉滚动条。 增加了activeTab和CaretIndex的保存与恢复
## flashpad
## eDiary