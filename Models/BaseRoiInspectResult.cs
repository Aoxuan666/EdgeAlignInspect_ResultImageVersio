using System;

namespace EdgeAlignInspect
{
	[Serializable]
	/// <summary>
	/// 单个线基准 ROI 的检测结果。
	/// </summary>
	public sealed class BaseRoiInspectResult
	{
		/// <summary>线基准 ROI 索引。</summary>
		public int Index { get; set; }

		/// <summary>线基准 ROI 名称。</summary>
		public string Name { get; set; }

		/// <summary>模板变换后的当前 ROI。</summary>
		public RotRectF RoiCur { get; set; }

		/// <summary>拟合得到的基准线。</summary>
		public EdgeLineFit Line { get; set; } = new EdgeLineFit();

		/// <summary>本次检测实际使用的卡尺参数。</summary>
		public CaliperParameters CaliperUsed { get; set; } = EdgeInspectJob.CreateDefaultBaseCaliper();

		/// <summary>该基准是否检测成功。</summary>
		public bool Success { get; set; }

		/// <summary>该基准的结果说明或失败原因。</summary>
		public string Message { get; set; } = "";
	}
}
