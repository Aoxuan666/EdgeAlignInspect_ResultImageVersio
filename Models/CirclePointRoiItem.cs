using System;

namespace EdgeAlignInspect
{
	[Serializable]
	/// <summary>
	/// 单圆基准点 ROI 配置；第四种检测模式会拟合该圆，并使用圆心作为点到线距离的基准点。
	/// </summary>
	public sealed class CirclePointRoiItem
	{
		/// <summary>稳定标识，用于检测 ROI 绑定后在列表顺序变化时仍能定位该圆点基准。</summary>
		public string Id { get; set; } = Guid.NewGuid().ToString("N");

		/// <summary>界面和结果中显示的圆点基准名称。</summary>
		public string Name { get; set; } = "圆点基准1";

		/// <summary>用于找圆的单圆 ROI。</summary>
		public CircleRoiF Circle { get; set; }

		/// <summary>该圆点基准独立使用的圆卡尺参数。</summary>
		public CaliperParameters Caliper { get; set; } = EdgeInspectJob.CreateDefaultCircleCaliper();

		/// <summary>创建当前圆点基准配置的深拷贝。</summary>
		public CirclePointRoiItem DeepClone()
		{
			return new CirclePointRoiItem
			{
				Id = Id,
				Name = Name,
				Circle = Circle,
				Caliper = (Caliper?.DeepClone() ?? EdgeInspectJob.CreateDefaultCircleCaliper())
			};
		}

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name) ? "圆点基准" : Name;
		}
	}
}
