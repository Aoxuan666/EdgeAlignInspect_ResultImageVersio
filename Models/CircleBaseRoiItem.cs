using System;

namespace EdgeAlignInspect
{
	[Serializable]
	/// <summary>
	/// 圆基准 ROI 配置项，由两个圆 ROI 组成。
	/// </summary>
	public sealed class CircleBaseRoiItem
	{
		/// <summary>稳定标识，用于检测 ROI 绑定圆基准时跨列表顺序变化定位对象。</summary>
		public string Id { get; set; } = Guid.NewGuid().ToString("N");

		/// <summary>界面和结果中显示的圆基准名称。</summary>
		public string Name { get; set; } = "圆基准1";

		/// <summary>第一个圆 ROI。</summary>
		public CircleRoiF Circle1 { get; set; }

		/// <summary>第二个圆 ROI。</summary>
		public CircleRoiF Circle2 { get; set; }

		/// <summary>当前圆基准 ROI 的独立卡尺参数。</summary>
		public CaliperParameters Caliper { get; set; } = EdgeInspectJob.CreateDefaultCircleCaliper();

		/// <summary>创建当前配置项的深拷贝。</summary>
		public CircleBaseRoiItem DeepClone()
		{
			return new CircleBaseRoiItem
			{
				Id = Id,
				Name = Name,
				Circle1 = Circle1,
				Circle2 = Circle2,
				Caliper = (Caliper?.DeepClone() ?? EdgeInspectJob.CreateDefaultCircleCaliper())
			};
		}

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name) ? "圆基准" : Name;
		}
	}
}
