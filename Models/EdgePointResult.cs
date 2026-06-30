using System;
using System.Drawing;

namespace EdgeAlignInspect
{
	[Serializable]
	public sealed class EdgePointResult
	{
		public int DetectIndex { get; set; }

		public string DetectName { get; set; }

		public int Index { get; set; }

		public PointF Point { get; set; }

		public double SignedDistanceRawPx { get; set; }

		public double SignedDistancePx { get; set; }

		public double DeltaPx { get; set; }

		public double SignedDistanceValue { get; set; }

		public double DeltaValue { get; set; }

		public bool IsBurr { get; set; }

		public bool IsDent { get; set; }

		public NgReason NgReasons { get; set; } = NgReason.None;

		public string NgReasonText => NgReasonHelper.ToText(NgReasons);
	}
}
