using System;

namespace EdgeAlignInspect
{
	[Serializable]
	/// <summary>
	/// 检测 ROI 配置项。
	/// </summary>
	public sealed class DetectRoiItem
	{
		/// <summary>稳定标识，用于未来扩展保存、定位或外部映射。</summary>
		public string Id { get; set; } = Guid.NewGuid().ToString("N");

		/// <summary>界面和结果中显示的检测项名称。</summary>
		public string Name { get; set; } = "检测1";

		/// <summary>检测区域，使用旋转矩形表示。</summary>
		public RotRectF Roi { get; set; }

		/// <summary>是否使用绑定基准线判定；为 <c>false</c> 时使用检测 ROI 自身拟合线判定。</summary>
		public bool UseReferenceLine { get; set; } = true;

		/// <summary>当前检测 ROI 绑定的基准类型。</summary>
		public ReferenceBaseKind ReferenceBaseKind { get; set; } = ReferenceBaseKind.LineRoi;

		/// <summary>绑定线基准 ROI 的索引，作为 ID 失效时的回退值。</summary>
		public int BaseRoiIndex { get; set; } = 0;

		/// <summary>绑定线基准 ROI 的稳定 ID。</summary>
		public string BaseRoiId { get; set; } = "";

		/// <summary>绑定圆基准 ROI 的索引，作为 ID 失效时的回退值。</summary>
		public int CircleBaseRoiIndex { get; set; } = 0;

		/// <summary>绑定圆基准 ROI 的稳定 ID。</summary>
		public string CircleBaseRoiId { get; set; } = "";

		/// <summary>绑定圆点基准 ROI 的索引，作为 ID 失效时的回退值。</summary>
		public int CirclePointRoiIndex { get; set; } = 0;

		/// <summary>绑定圆点基准 ROI 的稳定 ID。</summary>
		public string CirclePointRoiId { get; set; } = "";

		/// <summary>检测 ROI 自身拟合线的角度参考方式。</summary>
		public DetectAngleReferenceMode AngleReference { get; set; } = DetectAngleReferenceMode.ParallelToBase;

		/// <summary>名义距离，单位为像素。</summary>
		public double NominalDistancePx { get; set; } = 0.0;

		/// <summary>毛刺判定公差，单位为像素；使用外部公差时由 SDK 运行参数覆盖。</summary>
		public double BurrTolerancePx { get; set; } = 2.0;

		/// <summary>凹陷判定公差，单位为像素。</summary>
		public double DentTolerancePx { get; set; } = 2.0;

		/// <summary>是否启用当前检测项。</summary>
		public bool Enabled { get; set; } = true;

		/// <summary>当前检测 ROI 的独立卡尺参数。</summary>
		public CaliperParameters Caliper { get; set; } = EdgeInspectJob.CreateDefaultDetectCaliper();

		/// <summary>创建当前配置项的深拷贝。</summary>
		public DetectRoiItem DeepClone()
		{
			return new DetectRoiItem
			{
				Id = Id,
				Name = Name,
				Roi = Roi,
				UseReferenceLine = UseReferenceLine,
				ReferenceBaseKind = ReferenceBaseKind,
				BaseRoiIndex = BaseRoiIndex,
				BaseRoiId = BaseRoiId,
				CircleBaseRoiIndex = CircleBaseRoiIndex,
				CircleBaseRoiId = CircleBaseRoiId,
				CirclePointRoiIndex = CirclePointRoiIndex,
				CirclePointRoiId = CirclePointRoiId,
				AngleReference = AngleReference,
				NominalDistancePx = NominalDistancePx,
				BurrTolerancePx = BurrTolerancePx,
				DentTolerancePx = DentTolerancePx,
				Enabled = Enabled,
				Caliper = (Caliper?.DeepClone() ?? EdgeInspectJob.CreateDefaultDetectCaliper())
			};
		}

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name) ? "检测" : Name;
		}
	}
}
