using System.Globalization;
using System.Runtime.CompilerServices;

namespace ILSpy.Chinese.Plugin
{
	/// <summary>
	/// ILSpy 汉化插件的入口点。
	/// 插件程序集被 ILSpy 载入时，运行时会自动调用 <see cref="Initialize"/>，
	/// 把进程的 UI 区域性切换为简体中文；之后 ILSpy 自带的资源管理器
	/// 就会从 zh-Hans\ILSpy.resources.dll 读取译文。
	/// </summary>
	/// <remarks>
	/// 设计约束（改动本文件前请先确认以下前提仍然成立）：
	/// <list type="number">
	/// <item>不修改 ILSpy 的源码、XAML 与主程序集，因此只能通过插件机制接入。</item>
	/// <item>ILSpy 只加载文件名形如 *.Plugin.dll 的程序集（AppComposition.LoadPlugins），
	/// 且加载发生在主窗口创建之前，这个时机足以影响整个界面。</item>
	/// <item>ILSpy 的插件没有“启动钩子”导出点，<see cref="ModuleInitializerAttribute"/>
	/// 是唯一不需要宿主配合、又能保证早于界面初始化的入口。</item>
	/// <item>译文不放在本插件里，而是作为标准 .NET 卫星程序集，由 ILSpy 自己的
	/// <c>ResourceManager</c> 按 UI 区域性自动加载，连资源查找逻辑都不需要复刻。</item>
	/// </list>
	/// </remarks>
	internal static class PluginInitializer
	{
		/// <summary>目标 UI 区域性：zh-Hans 表示简体中文。</summary>
		private const string ChineseCultureName = "zh-Hans";

		/// <summary>
		/// 模块初始化器：程序集载入时立即执行，且在整个进程内只执行一次。
		/// </summary>
		/// <remarks>
		/// 这里只改“UI 区域性”，不改 <see cref="CultureInfo.CurrentCulture"/>，
		/// 以免影响 ILSpy 反编译结果中的数字、日期等格式化输出。
		/// 若以后 ILSpy 提供自己的语言设置入口，宿主设置的区域性会覆盖这里的值，
		/// 届时需要改为由用户在 ILSpy 内选择中文。
		/// </remarks>
		[ModuleInitializer]
		internal static void Initialize()
		{
			var culture = CultureInfo.GetCultureInfo(ChineseCultureName);

			// 影响之后新建的所有线程（ILSpy 的界面线程是在插件加载之后才启动的）。
			CultureInfo.DefaultThreadCurrentUICulture = culture;

			// 影响当前线程：加载插件的正是启动线程，它随后会创建主窗口。
			// 该属性是线程静态的，所以这里必须单独赋值一次。
			if (CultureInfo.CurrentUICulture.Name != culture.Name)
				CultureInfo.CurrentUICulture = culture;
		}
	}
}
