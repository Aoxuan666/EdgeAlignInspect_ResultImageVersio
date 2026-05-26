using System;
using System.Collections.Generic;
using System.Drawing;

namespace EdgeAlignInspect
{
	[Serializable]
	/// <summary>
	/// 一次检测的总结果。
	/// </summary>
	public sealed class EdgeInspectResult
	{
		/// <summary>整体检测是否成功。存在检测项 NG 时该值通常为 <c>false</c>。</summary>
		public bool Success { get; set; }

		/// <summary>整体结果说明或失败原因。</summary>
		public string Message { get; set; } = "";

		/// <summary>本次检测是否启用了模板匹配。</summary>
		public bool TemplateMatchEnabled { get; set; }

		/// <summary>模板匹配是否成功。</summary>
		public bool TemplateMatchOk { get; set; }

		/// <summary>模板匹配分数。</summary>
		public double TemplateMatchScore { get; set; }

		/// <summary>模板 ROI 在当前图像中的位置。</summary>
		public RotRectF TemplateRoiCur { get; set; }

		/// <summary>线基准检测结果列表。</summary>
		public List<BaseRoiInspectResult> BaseResults { get; } = new List<BaseRoiInspectResult>();

		/// <summary>圆基准检测结果列表。</summary>
		public List<CircleBaseRoiInspectResult> CircleBaseResults { get; } = new List<CircleBaseRoiInspectResult>();

		/// <summary>单圆基准点检测结果列表；拟合圆心会供点到自拟合切割边的整体距离判定使用。</summary>
		public List<CirclePointRoiInspectResult> CirclePointResults { get; } = new List<CirclePointRoiInspectResult>();

		/// <summary>检测 ROI 结果列表。</summary>
		public List<DetectRoiInspectResult> DetectResults { get; } = new List<DetectRoiInspectResult>();

		/// <summary>失败项说明列表，便于上位机汇总展示。</summary>
		public List<string> FailedItems { get; } = new List<string>();

		/// <summary>兼容旧版单基准结果的当前基准 ROI。</summary>
		public RotRectF BaseRoiCur { get; set; }

		/// <summary>兼容旧版单检测结果的当前检测 ROI。</summary>
		public RotRectF DetectRoiCur { get; set; }

		/// <summary>兼容旧版单基准结果的基准线。</summary>
		public EdgeLineFit BaseLine { get; set; } = new EdgeLineFit();

		/// <summary>所有检测 ROI 的边缘点结果汇总。</summary>
		public List<EdgePointResult> Points { get; } = new List<EdgePointResult>();

		/// <summary>本次检测导致 NG 的原因，可同时包含多种原因。</summary>
		public NgReason NgReasons { get; set; } = NgReason.None;

		/// <summary>本次检测 NG 原因的中文说明。</summary>
		public string NgReasonText => NgReasonHelper.ToText(NgReasons);

		/// <summary>毛刺点数量。</summary>
		public int BurrCount { get; set; }

		/// <summary>凹陷点数量。</summary>
		public int DentCount { get; set; }

		/// <summary>整体超边数量。</summary>
		public int OverEdgeCount { get; set; }

		/// <summary>整体漏铜数量。</summary>
		public int CopperLeakCount { get; set; }

		/// <summary>有符号偏差最小值，单位为物理单位。</summary>
		public double SignedMin { get; set; }

		/// <summary>有符号偏差最大值，单位为物理单位。</summary>
		public double SignedMax { get; set; }

		/// <summary>有符号偏差平均值，单位为物理单位。</summary>
		public double SignedMean { get; set; }

		/// <summary>绝对偏差最小值，单位为物理单位。</summary>
		public double DeltaMin { get; set; }

		/// <summary>绝对偏差最大值，单位为物理单位。</summary>
		public double DeltaMax { get; set; }

		/// <summary>绝对偏差平均值，单位为物理单位。</summary>
		public double DeltaMean { get; set; }

		/// <summary>名义距离，单位为像素。</summary>
		public double Nominal { get; set; }

		/// <summary>毛刺公差，单位为像素。</summary>
		public double BurrTolerance { get; set; }

		/// <summary>凹陷公差，单位为像素。</summary>
		public double DentTolerance { get; set; }

		/// <summary>本次检测使用的缺陷检测模式。</summary>
		public DefectDetectMode DetectMode { get; set; } = DefectDetectMode.Both;

		/// <summary>兼容旧版单检测项的基准线判定标志。</summary>
		public bool UseReferenceLine { get; set; }

		/// <summary>是否包含使用基准线判定的检测 ROI。</summary>
		public bool HasReferenceLineItems { get; set; }

		/// <summary>是否包含使用自身拟合线判定的检测 ROI。</summary>
		public bool HasSelfFitLineItems { get; set; }

		/// <summary>是否同时存在基准线判定和自身拟合线判定。</summary>
		public bool IsMixedJudgeMode => HasReferenceLineItems && HasSelfFitLineItems;

		/// <summary>是否使用 SDK 运行时传入的外部毛刺公差。</summary>
		public bool UseExternalBurrTolerance { get; set; }

		/// <summary>SDK 运行时传入的外部毛刺公差。</summary>
		public double ExternalBurrTolerance { get; set; }

		/// <summary>SDK 按需返回给上位机的检测结果图；未请求返回图时为 <c>null</c>。</summary>
		public Bitmap ResultImage { get; set; }

		/// <summary>SDK 运行时传入的外部凹陷公差，单位为毫米。</summary>
		public double ExternalDentTolerance { get; set; }

		/// <summary>SDK 运行时传入的外部超边公差，单位为毫米。</summary>
		public double ExternalOverEdgeTolerance { get; set; }

		/// <summary>SDK 运行时传入的外部漏铜公差，单位为毫米。</summary>
		public double ExternalCopperLeakTolerance { get; set; }

		/// <summary>X 方向单像素代表的物理尺寸。</summary>
		public double PixelResolutionX { get; set; } = 1.0;

		/// <summary>Y 方向单像素代表的物理尺寸。</summary>
		public double PixelResolutionY { get; set; } = 1.0;

		/// <summary>最大正向偏差，单位为像素。</summary>
		public double MaxPositiveDeltaPx { get; set; }

		/// <summary>最大正向偏差，单位为物理单位。</summary>
		public double MaxPositiveDeltaValue { get; set; }
	}
}
