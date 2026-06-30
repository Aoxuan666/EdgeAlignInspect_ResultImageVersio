using System;

namespace EdgeAlignInspect
{
	[Serializable]
	public sealed class BaseRoiItem
	{
		public string Id { get; set; } = Guid.NewGuid().ToString("N");

		public string Name { get; set; } = "基准1";

		public RotRectF Roi { get; set; }

		public CaliperParameters Caliper { get; set; } = EdgeInspectJob.CreateDefaultBaseCaliper();

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
