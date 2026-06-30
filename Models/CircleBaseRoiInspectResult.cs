using System;

namespace EdgeAlignInspect
{
	[Serializable]
	public sealed class CircleBaseRoiInspectResult
	{
		public int Index { get; set; }

		public string Name { get; set; }

		public CircleRoiF Circle1RoiCur { get; set; }

		public CircleRoiF Circle2RoiCur { get; set; }

		public EdgeCircleFit Circle1 { get; set; } = new EdgeCircleFit();

		public EdgeCircleFit Circle2 { get; set; } = new EdgeCircleFit();

		public EdgeLineFit Line { get; set; } = new EdgeLineFit();

		public CaliperParameters CaliperUsed { get; set; } = EdgeInspectJob.CreateDefaultCircleCaliper();

		public bool Success { get; set; }

		public string Message { get; set; } = "";
	}
}
