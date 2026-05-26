namespace EdgeAlignInspect
{
	public struct CanvasRoiSelection
	{
		public CanvasRoiKind Kind;

		public int Index;

		public bool IsValid => Kind != CanvasRoiKind.None && Index >= 0;

		public static CanvasRoiSelection None => new CanvasRoiSelection
		{
			Kind = CanvasRoiKind.None,
			Index = -1
		};

		public override string ToString()
		{
			if (Kind == CanvasRoiKind.None)
			{
				return "None";
			}
			return $"{Kind}[{Index}]";
		}
	}
}
