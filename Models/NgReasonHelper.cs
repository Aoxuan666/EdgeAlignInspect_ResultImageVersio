using System.Collections.Generic;

namespace EdgeAlignInspect
{
	public static class NgReasonHelper
	{
		public static string ToText(NgReason reasons)
		{
			if (reasons == NgReason.None)
			{
				return "OK";
			}
			List<string> list = new List<string>();
			AddIf(list, reasons, NgReason.Burr, "毛刺");
			AddIf(list, reasons, NgReason.Dent, "凹陷");
			AddIf(list, reasons, NgReason.OverEdge, "超边");
			AddIf(list, reasons, NgReason.CopperLeak, "漏铜");
			AddIf(list, reasons, NgReason.TemplateMatchFailed, "模板匹配失败");
			AddIf(list, reasons, NgReason.BaseRoiFailed, "基准失败");
			AddIf(list, reasons, NgReason.DetectRoiFailed, "检测ROI失败");
			AddIf(list, reasons, NgReason.ParameterInvalid, "参数无效");
			AddIf(list, reasons, NgReason.AlgorithmException, "算法异常");
			return (list.Count == 0) ? reasons.ToString() : string.Join("+", list);
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
