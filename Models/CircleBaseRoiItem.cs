using System;

namespace EdgeAlignInspect
{
	[Serializable]
	public sealed class CircleBaseRoiItem
	{
		public string Id { get; set; } = Guid.NewGuid().ToString("N");

		public string Name { get; set; } = "圆基准1";

		public CircleRoiF Circle1 { get; set; }

		public CircleRoiF Circle2 { get; set; }

		public bool UseTemplateTransform { get; set; } = true;

		public CaliperParameters Caliper { get; set; } = EdgeInspectJob.CreateDefaultCircleCaliper();

		public CircleBaseRoiItem DeepClone()
		{
			return new CircleBaseRoiItem
			{
				Id = Id,
				Name = Name,
				Circle1 = Circle1,
				Circle2 = Circle2,
				UseTemplateTransform = UseTemplateTransform,
				Caliper = (Caliper?.DeepClone() ?? EdgeInspectJob.CreateDefaultCircleCaliper())
			};
		}

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name) ? "圆基准" : Name;
		}
	}
}
