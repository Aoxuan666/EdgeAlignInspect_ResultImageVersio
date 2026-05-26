using System;
using System.Collections.Generic;

namespace EdgeAlignInspect
{
	[Serializable]
	/// <summary>
	/// 缺陷检测模式。
	/// </summary>
	public enum DefectDetectMode
	{
		/// <summary>同时检测毛刺和凹陷。</summary>
		Both,
		/// <summary>仅检测毛刺。</summary>
		BurrOnly,
		/// <summary>仅检测凹陷。</summary>
		DentOnly,
		/// <summary>测量基准点到切割边拟合直线的垂直距离。</summary>
		PointToCutEdgeDistance
	}

	/// <summary>
	/// NG 原因，可组合表示同一次检测中出现的多种问题。
	/// </summary>
	[Flags]
	public enum NgReason
	{
		/// <summary>无 NG 原因。</summary>
		None = 0,
		/// <summary>局部毛刺超出允许公差。</summary>
		Burr = 1,
		/// <summary>局部凹陷超出允许公差。</summary>
		Dent = 2,
		/// <summary>整体距离大于标准距离并超差。</summary>
		OverEdge = 4,
		/// <summary>整体距离小于标准距离并超差。</summary>
		CopperLeak = 8,
		/// <summary>模板匹配失败。</summary>
		TemplateMatchFailed = 16,
		/// <summary>基准 ROI 拟合或关联失败。</summary>
		BaseRoiFailed = 32,
		/// <summary>检测 ROI 找边或拟合失败。</summary>
		DetectRoiFailed = 64,
		/// <summary>输入参数无效。</summary>
		ParameterInvalid = 128,
		/// <summary>算法运行异常。</summary>
		AlgorithmException = 256
	}

	/// <summary>
	/// NG 原因枚举的显示文本转换工具。
	/// </summary>
	public static class NgReasonHelper
	{
		/// <summary>
		/// 将 NG 原因转换为中文说明。
		/// </summary>
		/// <param name="reasons">NG 原因标志。</param>
		/// <returns>中文原因说明；无 NG 时返回 OK。</returns>
		public static string ToText(NgReason reasons)
		{
			if (reasons == NgReason.None)
			{
				return "OK";
			}
			List<string> names = new List<string>();
			AddIf(names, reasons, NgReason.Burr, "毛刺");
			AddIf(names, reasons, NgReason.Dent, "凹陷");
			AddIf(names, reasons, NgReason.OverEdge, "超边");
			AddIf(names, reasons, NgReason.CopperLeak, "漏铜");
			AddIf(names, reasons, NgReason.TemplateMatchFailed, "模板匹配失败");
			AddIf(names, reasons, NgReason.BaseRoiFailed, "基准失败");
			AddIf(names, reasons, NgReason.DetectRoiFailed, "检测ROI失败");
			AddIf(names, reasons, NgReason.ParameterInvalid, "参数无效");
			AddIf(names, reasons, NgReason.AlgorithmException, "算法异常");
			return names.Count == 0 ? reasons.ToString() : string.Join("+", names);
		}

		private static void AddIf(List<string> names, NgReason reasons, NgReason flag, string text)
		{
			if ((reasons & flag) == flag)
			{
				names.Add(text);
			}
		}
	}
}
