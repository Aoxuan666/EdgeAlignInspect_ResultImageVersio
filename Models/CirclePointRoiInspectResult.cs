using System;

namespace EdgeAlignInspect
{
	[Serializable]
	public sealed class CirclePointRoiInspectResult
	{
		public int Index { get; set; }

		public string Name { get; set; }

		public CircleRoiF CircleRoiCur { get; set; }

		public EdgeCircleFit Circle { get; set; } = new EdgeCircleFit();

		public CaliperParameters CaliperUsed { get; set; } = EdgeInspectJob.CreateDefaultCircleCaliper();

		public bool Success { get; set; }

		public string Message { get; set; } = "";
	}
}
