using System;

namespace EdgeAlignInspect
{
	[Serializable]
	public sealed class DetectRoiItem
	{
		public string Id { get; set; } = Guid.NewGuid().ToString("N");

		public string Name { get; set; } = "检测1";

		public RotRectF Roi { get; set; }

		public bool UseReferenceLine { get; set; } = true;

		public ReferenceBaseKind ReferenceBaseKind { get; set; } = ReferenceBaseKind.LineRoi;

		public int BaseRoiIndex { get; set; } = 0;

		public string BaseRoiId { get; set; } = "";

		public int CircleBaseRoiIndex { get; set; } = 0;

		public string CircleBaseRoiId { get; set; } = "";

		public int CirclePointRoiIndex { get; set; } = 0;

		public string CirclePointRoiId { get; set; } = "";

		public DetectAngleReferenceMode AngleReference { get; set; } = DetectAngleReferenceMode.ParallelToBase;

		public double NominalDistancePx { get; set; } = 0.0;

		public double BurrTolerancePx { get; set; } = 2.0;

		public double DentTolerancePx { get; set; } = 2.0;

		public bool Enabled { get; set; } = true;

		public CaliperParameters Caliper { get; set; } = EdgeInspectJob.CreateDefaultDetectCaliper();

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
