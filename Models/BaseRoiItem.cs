using System;

namespace EdgeAlignInspect
{
	[Serializable]
	/// <summary>
	/// 线基准 ROI 配置项。
	/// </summary>
	public sealed class BaseRoiItem
	{
		/// <summary>稳定标识，用于检测 ROI 绑定基准时跨列表顺序变化定位对象。</summary>
		public string Id { get; set; } = Guid.NewGuid().ToString("N");

		/// <summary>界面和结果中显示的基准名称。</summary>
		public string Name { get; set; } = "基准1";

		/// <summary>线基准使用的旋转矩形 ROI。</summary>
		public RotRectF Roi { get; set; }

		/// <summary>当前线基准 ROI 的独立卡尺参数。</summary>
		public CaliperParameters Caliper { get; set; } = EdgeInspectJob.CreateDefaultBaseCaliper();

		/// <summary>创建当前配置项的深拷贝。</summary>
		public BaseRoiItem DeepClone()
		{
			return new BaseRoiItem
			{
				Id = Id,
				Name = Name,
				Roi = Roi,
				Caliper = (Caliper?.DeepClone() ?? EdgeInspectJob.CreateDefaultBaseCaliper())
			};
		}

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name) ? "基准" : Name;
		}
	}
}
