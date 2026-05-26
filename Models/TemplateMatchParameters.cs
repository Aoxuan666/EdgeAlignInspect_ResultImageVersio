using System;

namespace EdgeAlignInspect
{
	[Serializable]
	/// <summary>
	/// HALCON 形状模板匹配参数。
	/// </summary>
	public sealed class TemplateMatchParameters
	{
		/// <summary>是否启用模板匹配和运行时 ROI 坐标变换。</summary>
		public bool Enabled { get; set; } = true;

		/// <summary>模板金字塔层数。</summary>
		public int NumLevels { get; set; } = 5;

		/// <summary>匹配起始角度，单位为弧度。</summary>
		public double AngleStart { get; set; } = -0.3;

		/// <summary>匹配角度范围，单位为弧度。</summary>
		public double AngleExtent { get; set; } = 0.6;

		/// <summary>最小匹配分数。</summary>
		public double MinScore { get; set; } = 0.5;
	}
}
