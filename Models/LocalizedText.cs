using System;
using System.Collections.Generic;
using System.Linq;

namespace EdgeAlignInspect
{
	public static class LocalizedText
	{
		private static readonly Dictionary<string, string> ZhToEn = new Dictionary<string, string>
		{
			{ "智茂检测", "Smart Inspection" },
			{ "操作", "Actions" },
			{ "参数", "Parameters" },
			{ "参数（当前：", "Parameters (Current: " },
			{ "参数（当前未选中检测ROI）", "Parameters (No Detect ROI Selected)" },
			{ "模板设置", "Template Settings" },
			{ "加载图片", "Load Image" },
			{ "删除选中ROI", "Delete ROI" },
			{ "运行", "Run" },
			{ "添加线基准", "Add Line Base" },
			{ "添加圆基准", "Add Circle Base" },
			{ "添加圆点基准", "Add Circle Point" },
			{ "添加检测ROI", "Add Detect ROI" },
			{ "保存(示教)", "Save Teach" },
			{ "确认配置", "Confirm" },
			{ "语言", "Language" },
			{ "中文", "Chinese" },
			{ "English", "English" },
			{ "当前选中检测ROI参数", "Selected Detect ROI" },
			{ "启用当前检测ROI", "Enable Detect ROI" },
			{ "关联基准ROI", "Base ROI" },
			{ "角度参考", "Angle Ref." },
			{ "标准距离(mm)", "Nominal(mm)" },
			{ "全局判定参数", "Global Judge" },
			{ "使用基准线判定", "Use Base Line" },
			{ "检测模式", "Detect Mode" },
			{ "模板匹配", "Template Match" },
			{ "启用模板匹配", "Enable Match" },
			{ "最小分数", "Min Score" },
			{ "角度起始", "Angle Start" },
			{ "角度范围", "Angle Range" },
			{ "找边极性", "Edge Polarity" },
			{ "基准极性", "Base Polarity" },
			{ "检测极性", "Detect Polarity" },
			{ "卡尺参数", "Caliper" },
			{ "基准", "Base" },
			{ "检测", "Detect" },
			{ "点数", "Count" },
			{ "阈值", "Threshold" },
			{ "圆外扩", "Circle Out" },
			{ "检测结果", "Result" },
			{ "运行后会在左侧图像显示线、点和偏差标注", "Lines, points and deviations are shown after running." },
			{ "错误信息已显示在左侧图像提示中", "Error details are shown on the image." },
			{ "关闭", "Off" },
			{ "模式", "Mode" },
			{ "判定方式", "Judge Mode" },
			{ "判定", "Judge" },
			{ "结果", "Result" },
			{ "失败ROI", "Failed ROI" },
			{ "失败项", "Failed Items" },
			{ "失败", "Failed" },
			{ "线基准", "Line Base" },
			{ "圆点", "Circle Point" },
			{ "局部Δ 最小/最大/平均", "Local Delta Min/Max/Mean" },
			{ "Δ最小", "Delta Min" },
			{ "Δ最大", "Delta Max" },
			{ "Δ平均", "Delta Mean" },
			{ "请检查模板 ROI、最小分数、角度范围和图像质量", "Check template ROI, min score, angle range, and image quality." },
			{ "请检查：模板ROI/最小分数/角度范围/图像质量", "Check: template ROI / min score / angle range / image quality" },
			{ "整体", "Overall" },
			{ "像素", "Pixel" },
			{ "标准", "Nominal" },
			{ "角差", "Angle Delta" },
			{ "未知", "Unknown" },
			{ "混合判定", "Mixed Judge" },
			{ "基准线判定", "Base Line Judge" },
			{ "自拟合线判定", "Self Fit Line Judge" },
			{ "两者都检测", "Both" },
			{ "只检测毛刺", "Burr Only" },
			{ "只检测凹陷", "Dent Only" },
			{ "毛刺+凹陷", "Burr + Dent" },
			{ "毛刺", "Burr" },
			{ "凹陷", "Dent" },
			{ "超边", "Over Edge" },
			{ "漏铜", "Copper Leak" },
			{ "平行于关联基准线", "Parallel to Base" },
			{ "平行于圆基准", "Parallel to Circle Base" },
			{ "平行于基准", "Parallel to Base" },
			{ "水平", "Horizontal" },
			{ "竖直", "Vertical" },
			{ "白找黑", "White to Black" },
			{ "黑找白", "Black to White" },
			{ "任意", "Any" },
			{ "外轮廓模板", "Outer Contour Template" },
			{ "只使用外轮廓创建模板", "Use outer contour only" },
			{ "边缘 Sigma", "Edge Sigma" },
			{ "低阈值", "Low Threshold" },
			{ "高阈值", "High Threshold" },
			{ "点最小间距", "Min Point Distance" },
			{ "角度分桶", "Angle Bins" },
			{ "擦除半径", "Erase Radius" },
			{ "特征点", "Feature Points" },
			{ "提取外轮廓点", "Extract Contour" },
			{ "擦除点", "Erase Points" },
			{ "重新提取", "Extract Again" },
			{ "创建模型", "Create Model" },
			{ "测试匹配", "Test Match" },
			{ "确定", "OK" },
			{ "取消", "Cancel" },
			{ "框选或调整模板 ROI 后提取外轮廓点。", "Select or adjust template ROI, then extract contour points." },
			{ "模板ROI", "Template ROI" },
			{ "模板", "Template" },
			{ "基准ROI", "Base ROI" },
			{ "检测ROI", "Detect ROI" },
			{ "圆基准", "Circle Base" },
			{ "圆点基准", "Circle Point" },
			{ "模板匹配失败", "Template match failed" },
			{ "基准失败", "Base failed" },
			{ "检测ROI失败", "Detect ROI failed" },
			{ "参数无效", "Invalid parameter" },
			{ "算法异常", "Algorithm exception" },
			{ "参数、各ROI独立卡尺参数与模板已保存到当前 Job。毛刺公差由上位机传入。", "Parameters, per-ROI calipers and template have been saved to the current Job. Burr tolerance is provided by host." },
			{ "参数与各ROI独立卡尺参数已保存（模板匹配关闭）。毛刺公差由上位机传入。", "Parameters and per-ROI calipers have been saved. Template matching is disabled. Burr tolerance is provided by host." },
			{ "模板设置已更新。", "Template settings updated." },
			{ "请先加载图片。", "Please load an image first." },
			{ "请加载图片", "Please load an image" },
			{ "错误", "Error" },
			{ "已保存", "Saved" },
			{ "提示", "Tip" },
			{ "保存失败", "Save Failed" },
			{ "确认配置失败", "Confirm Failed" },
			{ "结果为空", "Result is empty" },
			{ "参数已修改，请重新提取外轮廓点。", "Parameters changed. Please extract contour points again." },
			{ "已提取外轮廓特征点。", "Outer contour feature points extracted." },
			{ "没有提取到特征点，请调整 ROI 或阈值。", "No feature points extracted. Adjust ROI or thresholds." },
			{ "模型创建完成，后续匹配将使用外轮廓。", "Model created. Outer contour will be used for matching." },
			{ "模型创建完成，可点击测试匹配。", "Model created. You can test matching now." },
			{ "请先创建模型。", "Please create the model first." },
			{ "退出擦除", "Exit Erase" },
			{ "擦除模式：按住左键擦除外轮廓点。", "Erase mode: hold left button to erase contour points." },
			{ "已退出擦除模式。", "Erase mode exited." },
			{ "已擦除部分特征点，请重新创建模型。", "Some feature points were erased. Please recreate the model." },
			{ "正在创建模型...", "Creating model..." },
			{ "正在测试匹配...", "Testing match..." },
			{ "测试匹配失败", "Test Match Failed" },
			{ "提取失败", "Extract Failed" },
			{ "创建模型失败", "Create Model Failed" },
			{ "未找到匹配目标。", "No match target found." },
			{ "匹配成功，分数=", "Match succeeded, score=" },
			{ "需要：至少一个检测ROI；其中使用基准线判定的检测ROI还需要至少一个基准ROI。", "Required: at least one detect ROI; detect ROIs using base-line judging also require at least one base ROI." },
			{ "需要：至少一个检测ROI。", "Required: at least one detect ROI." },
			{ "Job 中未包含有效模板数据。", "Job does not contain valid template data." },
			{ "基准ROI为空", "Base ROI is empty" },
			{ "基准线拟合失败", "Base line fit failed" },
			{ "圆基准ROI为空", "Circle base ROI is empty" },
			{ "圆心连线无效", "Circle-center line is invalid" },
			{ "圆拟合失败", "Circle fit failed" },
			{ "圆点基准ROI为空", "Circle point base ROI is empty" },
			{ "圆点基准拟合失败", "Circle point base fit failed" },
			{ "检测ROI为空", "Detect ROI is empty" },
			{ "检测ROI拟合失败（检查：ROI/卡尺参数/极性）", "Detect ROI fit failed (check ROI/caliper/polarity)" },
			{ "关联圆点基准无效", "Linked circle-point base is invalid" },
			{ "关联基准线无效", "Linked base line is invalid" },
			{ "圆点基准索引", "Circle point base index" },
			{ "圆基准索引", "Circle base index" },
			{ "基准索引", "Base index" },
			{ "判定线无效", "Judge line is invalid" },
			{ "未取得检测点", "No detect points found" },
			{ "自拟合线局部判定", "Self-fit local judge" },
			{ "外部允差(mm)", "External tolerance(mm)" },
			{ "本地毛刺允差", "Local burr tolerance" },
			{ "本地凹陷允差", "Local dent tolerance" },
			{ "毛刺允差", "Burr tolerance" },
			{ "凹陷允差", "Dent tolerance" },
			{ "超边允差", "Over-edge tolerance" },
			{ "漏铜允差", "Copper-leak tolerance" },
			{ "解析度X", "Resolution X" },
			{ "解析度Y", "Resolution Y" },
			{ "最大毛刺", "Max burr" },
			{ "最大偏差", "Max delta" },
			{ "局部Δ(min/max/mean)", "Local delta(min/max/mean)" },
			{ "整体距离", "Overall distance" },
			{ "像素距离", "Pixel distance" },
			{ "超边NG", "Over-edge NG" },
			{ "漏铜NG", "Copper-leak NG" },
			{ "整体距离OK", "Overall distance OK" },
			{ "点到切割边距离", "Point to cut-edge distance" },
			{ "不可用", "Unavailable" },
			{ "卡尺测量点不足", "Insufficient caliper measure points" },
			{ "直线拟合失败", "Line fit failed" },
			{ "圆测量点不足（请检查圆ROI、圆外扩、阈值/极性）", "Insufficient circle measure points (check circle ROI, circle out, threshold/polarity)" },
			{ "两个圆心重合", "Two circle centers overlap" },
			{ "模板字节数据为空。", "Template byte data is empty." },
			{ "输入图像为空。", "Input image is null." },
			{ "Job 参数为空。", "Job parameter is null." },
			{ "公差参数为空。", "Tolerance options parameter is null." },
			{ "所有公差必须 >= 0。", "All tolerances must be >= 0." },
			{ "X 方向像素分辨率必须 > 0。", "Pixel resolution X must be > 0." },
			{ "Y 方向像素分辨率必须 > 0。", "Pixel resolution Y must be > 0." },
			{ "算法异常：", "Algorithm exception: " },
			{ "Input image is null.", "Input image is null." },
			{ "Job parameter is null.", "Job parameter is null." },
			{ "Tolerance options parameter is null.", "Tolerance options parameter is null." },
			{ "All tolerances must be >= 0.", "All tolerances must be >= 0." },
			{ "Pixel resolution X must be > 0.", "Pixel resolution X must be > 0." },
			{ "Pixel resolution Y must be > 0.", "Pixel resolution Y must be > 0." }
		};

		private static readonly Dictionary<string, string> EnToZh = ZhToEn
			.GroupBy(p => p.Value)
			.ToDictionary(g => g.Key, g => g.First().Key);

		public static string Ui(string text, InspectionLanguage language)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}
			return language == InspectionLanguage.English ? ReplaceAll(text, ZhToEn) : ReplaceAll(text, EnToZh);
		}

		public static string Message(string text, InspectionLanguage language)
		{
			if (language != InspectionLanguage.English || string.IsNullOrEmpty(text))
			{
				return text;
			}
			return ReplaceAll(text, ZhToEn);
		}

		public static string ReasonText(NgReason reasons, InspectionLanguage language)
		{
			if (reasons == NgReason.None)
			{
				return "OK";
			}
			List<string> names = new List<string>();
			AddIf(names, reasons, NgReason.Burr, language == InspectionLanguage.English ? "Burr" : "毛刺");
			AddIf(names, reasons, NgReason.Dent, language == InspectionLanguage.English ? "Dent" : "凹陷");
			AddIf(names, reasons, NgReason.OverEdge, language == InspectionLanguage.English ? "Over Edge" : "超边");
			AddIf(names, reasons, NgReason.CopperLeak, language == InspectionLanguage.English ? "Copper Leak" : "漏铜");
			AddIf(names, reasons, NgReason.TemplateMatchFailed, language == InspectionLanguage.English ? "Template Match Failed" : "模板匹配失败");
			AddIf(names, reasons, NgReason.BaseRoiFailed, language == InspectionLanguage.English ? "Base Failed" : "基准失败");
			AddIf(names, reasons, NgReason.DetectRoiFailed, language == InspectionLanguage.English ? "Detect ROI Failed" : "检测ROI失败");
			AddIf(names, reasons, NgReason.ParameterInvalid, language == InspectionLanguage.English ? "Invalid Parameter" : "参数无效");
			AddIf(names, reasons, NgReason.AlgorithmException, language == InspectionLanguage.English ? "Algorithm Exception" : "算法异常");
			return names.Count == 0 ? reasons.ToString() : string.Join("+", names);
		}

		public static void ApplyToResult(EdgeInspectResult result, InspectionLanguage language)
		{
			if (result == null)
			{
				return;
			}
			result.Language = language;
			result.Message = Message(result.Message, language);
			foreach (BaseRoiInspectResult item in result.BaseResults)
			{
				if (item != null)
				{
					item.Message = Message(item.Message, language);
					if (item.Line != null)
					{
						item.Line.Message = Message(item.Line.Message, language);
					}
				}
			}
			foreach (CircleBaseRoiInspectResult item in result.CircleBaseResults)
			{
				if (item != null)
				{
					item.Message = Message(item.Message, language);
					if (item.Circle1 != null) item.Circle1.Message = Message(item.Circle1.Message, language);
					if (item.Circle2 != null) item.Circle2.Message = Message(item.Circle2.Message, language);
					if (item.Line != null) item.Line.Message = Message(item.Line.Message, language);
				}
			}
			foreach (CirclePointRoiInspectResult item in result.CirclePointResults)
			{
				if (item != null)
				{
					item.Message = Message(item.Message, language);
					if (item.Circle != null) item.Circle.Message = Message(item.Circle.Message, language);
				}
			}
			foreach (DetectRoiInspectResult item in result.DetectResults)
			{
				if (item != null)
				{
					item.Language = language;
					item.Message = Message(item.Message, language);
					item.AngleReferenceText = Message(item.AngleReferenceText, language);
					if (item.FittedLine != null) item.FittedLine.Message = Message(item.FittedLine.Message, language);
					if (item.JudgeLine != null) item.JudgeLine.Message = Message(item.JudgeLine.Message, language);
				}
			}
			for (int i = 0; i < result.FailedItems.Count; i++)
			{
				result.FailedItems[i] = Message(result.FailedItems[i], language);
			}
		}

		private static void AddIf(List<string> names, NgReason reasons, NgReason flag, string text)
		{
			if ((reasons & flag) == flag)
			{
				names.Add(text);
			}
		}

		private static string ReplaceAll(string text, Dictionary<string, string> map)
		{
			string result = text;
			foreach (KeyValuePair<string, string> pair in map.OrderByDescending(p => p.Key.Length))
			{
				result = result.Replace(pair.Key, pair.Value);
			}
			return result;
		}
	}
}
