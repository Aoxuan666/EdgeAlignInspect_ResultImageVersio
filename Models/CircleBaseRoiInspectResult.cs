using System;

namespace EdgeAlignInspect
{
	[Serializable]
	/// <summary>
	/// 单个圆基准 ROI 组的检测结果。
	/// </summary>
	public sealed class CircleBaseRoiInspectResult
	{
		/// <summary>圆基准 ROI 组索引。</summary>
		public int Index { get; set; }

		/// <summary>圆基准 ROI 组名称。</summary>
		public string Name { get; set; }

		/// <summary>模板变换后的第一个圆 ROI。</summary>
		public CircleRoiF Circle1RoiCur { get; set; }

		/// <summary>模板变换后的第二个圆 ROI。</summary>
		public CircleRoiF Circle2RoiCur { get; set; }

		/// <summary>第一个圆的拟合结果。</summary>
		public EdgeCircleFit Circle1 { get; set; } = new EdgeCircleFit();

		/// <summary>第二个圆的拟合结果。</summary>
		public EdgeCircleFit Circle2 { get; set; } = new EdgeCircleFit();

		/// <summary>两个拟合圆心连成的基准线。</summary>
		public EdgeLineFit Line { get; set; } = new EdgeLineFit();

		/// <summary>本次检测实际使用的卡尺参数。</summary>
		public CaliperParameters CaliperUsed { get; set; } = EdgeInspectJob.CreateDefaultCircleCaliper();

		/// <summary>该圆基准是否检测成功。</summary>
		public bool Success { get; set; }

		/// <summary>该圆基准的结果说明或失败原因。</summary>
		public string Message { get; set; } = "";
	}
}
