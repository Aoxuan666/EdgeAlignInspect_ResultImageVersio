using System;

namespace EdgeAlignInspect
{
	[Serializable]
	public sealed class BaseRoiInspectResult
	{
		public int Index { get; set; }

		public string Name { get; set; }

		public RotRectF RoiCur { get; set; }

		public EdgeLineFit Line { get; set; } = new EdgeLineFit();

		public CaliperParameters CaliperUsed { get; set; } = EdgeInspectJob.CreateDefaultBaseCaliper();

		public bool Success { get; set; }

		public string Message { get; set; } = "";
	}
}
