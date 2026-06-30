using System;
using System.Collections.Generic;
using System.Drawing;

namespace EdgeAlignInspect
{
	[Serializable]
	public sealed class EdgeCircleFit
	{
		public bool Success { get; set; }

		public PointF Center { get; set; }

		public double Radius { get; set; }

		public string Message { get; set; } = "";

		public List<PointF> MeasurePoints { get; } = new List<PointF>();
	}
}
