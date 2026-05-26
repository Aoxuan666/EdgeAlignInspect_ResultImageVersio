using System;
using System.Collections.Generic;
using System.Drawing;

namespace EdgeAlignInspect
{
	[Serializable]
	/// <summary>
	/// 单个检测 ROI 的检测结果。
	/// </summary>
	public sealed class DetectRoiInspectResult
	{
		/// <summary>检测 ROI 索引。</summary>
		public int Index { get; set; }

		/// <summary>检测 ROI 名称。</summary>
		public string Name { get; set; }

		/// <summary>该检测 ROI 是否启用。</summary>
		public bool Enabled { get; set; }

		/// <summary>模板变换后的当前 ROI。</summary>
		public RotRectF RoiCur { get; set; }

		/// <summary>是否使用绑定基准线判定。</summary>
		public bool UseReferenceLine { get; set; }

		/// <summary>绑定的基准类型。</summary>
		public ReferenceBaseKind ReferenceBaseKind { get; set; } = ReferenceBaseKind.LineRoi;

		/// <summary>绑定线基准 ROI 的索引。</summary>
		public int BaseRoiIndex { get; set; } = -1;

		/// <summary>绑定圆基准 ROI 的索引。</summary>
		public int CircleBaseRoiIndex { get; set; } = -1;

		/// <summary>绑定圆点基准 ROI 的索引。</summary>
		public int CirclePointRoiIndex { get; set; } = -1;

		/// <summary>自身拟合线判定时使用的角度参考方式。</summary>
		public DetectAngleReferenceMode AngleReference { get; set; } = DetectAngleReferenceMode.ParallelToBase;

		/// <summary>检测 ROI 内边缘点拟合得到的线。</summary>
		public EdgeLineFit FittedLine { get; set; } = new EdgeLineFit();

		/// <summary>最终用于判定偏差的参考线。</summary>
		public EdgeLineFit JudgeLine { get; set; } = new EdgeLineFit();

		/// <summary>本次检测实际使用的卡尺参数。</summary>
		public CaliperParameters CaliperUsed { get; set; } = EdgeInspectJob.CreateDefaultDetectCaliper();

		/// <summary>参考线角度，单位为弧度。</summary>
		public double RefAngleRad { get; set; }

		/// <summary>检测 ROI 自身拟合线角度，单位为弧度。</summary>
		public double FittedAngleRad { get; set; }

		/// <summary>检测线和参考线的角度差，单位为弧度。</summary>
		public double AngleDeltaRad { get; set; }

		/// <summary>检测线和参考线的角度差，单位为度。</summary>
		public double AngleDeltaDeg { get; set; }

		/// <summary>角度参考方式的显示文本。</summary>
		public string AngleReferenceText { get; set; } = "";

		/// <summary>名义距离，单位为像素。</summary>
		public double NominalDistancePx { get; set; }

		/// <summary>圆点基准拟合得到的基准点。</summary>
		public PointF ReferencePointCur { get; set; }

		/// <summary>基准点投影到切割边拟合线上的垂足。</summary>
		public PointF ReferenceFootPoint { get; set; }

		/// <summary>是否执行了整体距离判定；线基准、圆基准和圆点基准都会启用该判定。</summary>
		public bool HasOverallDistance { get; set; }

		/// <summary>兼容旧显示逻辑：是否执行了圆点到自拟合切割边的整体距离判定。</summary>
		public bool HasPointToLineDistance { get; set; }

		/// <summary>整体距离测量点；点线为圆心，线线/圆基准为检测线上的代表点。</summary>
		public PointF OverallMeasurePoint { get; set; }

		/// <summary>整体距离测量点投影到目标线上的垂足。</summary>
		public PointF OverallFootPoint { get; set; }

		/// <summary>整体距离判定使用的参考线；线线/圆基准为基准线，点线为检测拟合线。</summary>
		public EdgeLineFit OverallReferenceLine { get; set; } = new EdgeLineFit();

		/// <summary>整体实际距离，单位为像素。</summary>
		public double OverallDistancePx { get; set; }

		/// <summary>整体实际距离，单位为物理单位。</summary>
		public double OverallDistanceValue { get; set; }

		/// <summary>整体实际距离相对标准距离的偏差，单位为像素；实际大于标准为正。</summary>
		public double OverallDeltaPx { get; set; }

		/// <summary>整体实际距离相对标准距离的偏差，单位为物理单位；实际大于标准为正。</summary>
		public double OverallDeltaValue { get; set; }

		/// <summary>整体距离是否判定为超边；实际距离大于标准距离且超过允许公差时为真。</summary>
		public bool IsOverEdge { get; set; }

		/// <summary>整体距离是否判定为漏铜；实际距离小于标准距离且超过允许公差时为真。</summary>
		public bool IsCopperLeak { get; set; }

		/// <summary>兼容旧显示逻辑：基准点到切割边拟合线的垂直距离，单位为像素。</summary>
		public double PointToLineDistancePx
		{
			get { return OverallDistancePx; }
			set { OverallDistancePx = value; }
		}

		/// <summary>兼容旧显示逻辑：基准点到切割边拟合线的垂直距离，单位为物理单位。</summary>
		public double PointToLineDistanceValue
		{
			get { return OverallDistanceValue; }
			set { OverallDistanceValue = value; }
		}

		/// <summary>兼容旧显示逻辑：基准点到切割边距离相对名义距离的偏差，单位为像素。</summary>
		public double PointToLineDeltaPx
		{
			get { return OverallDeltaPx; }
			set { OverallDeltaPx = value; }
		}

		/// <summary>兼容旧显示逻辑：基准点到切割边距离相对名义距离的偏差，单位为物理单位。</summary>
		public double PointToLineDeltaValue
		{
			get { return OverallDeltaValue; }
			set { OverallDeltaValue = value; }
		}

		/// <summary>兼容旧显示逻辑：实际距离大于标准距离并超差。</summary>
		public bool IsPointToLineTooMuch
		{
			get { return IsOverEdge; }
			set { IsOverEdge = value; }
		}

		/// <summary>兼容旧显示逻辑：实际距离小于标准距离并超差。</summary>
		public bool IsPointToLineTooLittle
		{
			get { return IsCopperLeak; }
			set { IsCopperLeak = value; }
		}

		/// <summary>毛刺公差，单位为像素。</summary>
		public double BurrTolerancePx { get; set; }

		/// <summary>凹陷公差，单位为像素。</summary>
		public double DentTolerancePx { get; set; }

		/// <summary>是否使用 SDK 运行时传入的外部毛刺公差。</summary>
		public bool UseExternalBurrTolerance { get; set; }

		/// <summary>SDK 运行时传入的外部毛刺公差。</summary>
		public double ExternalBurrTolerance { get; set; }

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

		/// <summary>本检测 ROI 使用的缺陷检测模式。</summary>
		public DefectDetectMode DetectMode { get; set; } = DefectDetectMode.Both;

		/// <summary>检测 ROI 内每个有效边缘点的判定结果。</summary>
		public List<EdgePointResult> Points { get; } = new List<EdgePointResult>();

		/// <summary>该检测 ROI 导致 NG 的原因，可同时包含毛刺、凹陷、超边、漏铜等。</summary>
		public NgReason NgReasons { get; set; } = NgReason.None;

		/// <summary>该检测 ROI 的 NG 原因中文说明。</summary>
		public string NgReasonText => NgReasonHelper.ToText(NgReasons);

		/// <summary>毛刺点数量。</summary>
		public int BurrCount { get; set; }

		/// <summary>凹陷点数量。</summary>
		public int DentCount { get; set; }

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

		/// <summary>最大正向偏差，单位为像素。</summary>
		public double MaxPositiveDeltaPx { get; set; }

		/// <summary>最大正向偏差，单位为物理单位。</summary>
		public double MaxPositiveDeltaValue { get; set; }

		/// <summary>该检测 ROI 是否通过。</summary>
		public bool Success { get; set; }

		/// <summary>该检测 ROI 的结果说明或失败原因。</summary>
		public string Message { get; set; } = "";
	}
}
