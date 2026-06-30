using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EdgeAlignInspect
{
	[Serializable]
	public sealed class TemplateEraseStroke
	{
		public PointF Center { get; set; }

		public double Radius { get; set; }

		public TemplateEraseStroke DeepClone()
		{
			return new TemplateEraseStroke
			{
				Center = Center,
				Radius = Radius
			};
		}
	}

	[Serializable]
	public sealed class TemplateTeachData
	{
		public bool HasTemplate { get; set; }

		public byte[] ModelBytes { get; set; }

		public List<PointF> FeaturePoints { get; set; } = new List<PointF>();

		public List<TemplateEraseStroke> EraseStrokes { get; set; } = new List<TemplateEraseStroke>();

		public double RefRow { get; set; }

		public double RefCol { get; set; }

		public double RefAngle { get; set; }

		public void Clear()
		{
			HasTemplate = false;
			ModelBytes = null;
			FeaturePoints?.Clear();
			EraseStrokes?.Clear();
			RefRow = 0.0;
			RefCol = 0.0;
			RefAngle = 0.0;
		}

		public TemplateTeachData DeepClone()
		{
			return new TemplateTeachData
			{
				HasTemplate = HasTemplate,
				ModelBytes = ((ModelBytes == null) ? null : ((byte[])ModelBytes.Clone())),
				FeaturePoints = (FeaturePoints == null ? new List<PointF>() : new List<PointF>(FeaturePoints)),
				EraseStrokes = (EraseStrokes == null ? new List<TemplateEraseStroke>() : EraseStrokes.Select(x => x?.DeepClone() ?? new TemplateEraseStroke()).ToList()),
				RefRow = RefRow,
				RefCol = RefCol,
				RefAngle = RefAngle
			};
		}
	}
}
