using System;

namespace EdgeAlignInspect
{
	[Serializable]
	public sealed class CirclePointRoiItem
	{
		public string Id { get; set; } = Guid.NewGuid().ToString("N");

		public string Name { get; set; } = "圆点基准1";

		public CircleRoiF Circle { get; set; }

		public bool UseTemplateTransform { get; set; } = true;

		public CaliperParameters Caliper { get; set; } = EdgeInspectJob.CreateDefaultCircleCaliper();

		public CirclePointRoiItem DeepClone()
		{
			return new CirclePointRoiItem
			{
				Id = Id,
				Name = Name,
				Circle = Circle,
				UseTemplateTransform = UseTemplateTransform,
				Caliper = (Caliper?.DeepClone() ?? EdgeInspectJob.CreateDefaultCircleCaliper())
			};
		}

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name) ? "圆点基准" : Name;
		}
	}
}
