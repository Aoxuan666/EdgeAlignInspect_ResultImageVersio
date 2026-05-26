using System;
using System.Drawing;

namespace EdgeAlignInspect
{
	[Serializable]
	/// <summary>
	/// 单个边缘点的偏差和缺陷判定结果。
	/// </summary>
	public sealed class EdgePointResult
	{
		/// <summary>所属检测 ROI 索引。</summary>
		public int DetectIndex { get; set; }

		/// <summary>所属检测 ROI 名称。</summary>
		public string DetectName { get; set; }

		/// <summary>点在所属检测 ROI 内的序号。</summary>
		public int Index { get; set; }

		/// <summary>边缘点图像坐标。</summary>
		public PointF Point { get; set; }

		/// <summary>相对判定线的原始有符号距离，单位为像素。</summary>
		public double SignedDistanceRawPx { get; set; }

		/// <summary>扣除名义距离后的有符号偏差，单位为像素。</summary>
		public double SignedDistancePx { get; set; }

		/// <summary>绝对偏差，单位为像素。</summary>
		public double DeltaPx { get; set; }

		/// <summary>扣除名义距离后的有符号偏差，单位为物理单位。</summary>
		public double SignedDistanceValue { get; set; }

		/// <summary>绝对偏差，单位为物理单位。</summary>
		public double DeltaValue { get; set; }

		/// <summary>该点是否判定为毛刺。</summary>
		public bool IsBurr { get; set; }

		/// <summary>该点是否判定为凹陷。</summary>
		public bool IsDent { get; set; }

		/// <summary>该点导致 NG 的原因；通常为毛刺、凹陷或无。</summary>
		public NgReason NgReasons { get; set; } = NgReason.None;

		/// <summary>该点 NG 原因的中文说明。</summary>
		public string NgReasonText => NgReasonHelper.ToText(NgReasons);
	}
}
