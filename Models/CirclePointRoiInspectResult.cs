using System;

namespace EdgeAlignInspect
{
	[Serializable]
	/// <summary>
	/// 单圆基准点 ROI 的运行结果，包含当前 ROI、拟合圆和找圆状态。
	/// </summary>
	public sealed class CirclePointRoiInspectResult
	{
		/// <summary>圆点基准 ROI 索引。</summary>
		public int Index { get; set; }

		/// <summary>圆点基准 ROI 名称。</summary>
		public string Name { get; set; }

		/// <summary>模板变换后的当前圆 ROI。</summary>
		public CircleRoiF CircleRoiCur { get; set; }

		/// <summary>在当前图像中拟合得到的圆；其圆心作为点到切割边距离的基准点。</summary>
		public EdgeCircleFit Circle { get; set; } = new EdgeCircleFit();

		/// <summary>本次找圆实际使用的卡尺参数。</summary>
		public CaliperParameters CaliperUsed { get; set; } = EdgeInspectJob.CreateDefaultCircleCaliper();

		/// <summary>圆点基准找圆是否成功。</summary>
		public bool Success { get; set; }

		/// <summary>圆点基准找圆说明或失败原因。</summary>
		public string Message { get; set; } = "";
	}
}
