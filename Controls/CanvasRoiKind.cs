namespace EdgeAlignInspect
{
	/// <summary>
	/// 画布上可编辑或可选中的 ROI 类型。
	/// </summary>
	public enum CanvasRoiKind
	{
		/// <summary>未选中任何 ROI。</summary>
		None,
		/// <summary>模板匹配 ROI。</summary>
		Template,
		/// <summary>线基准 ROI。</summary>
		Base,
		/// <summary>检测 ROI。</summary>
		Detect,
		/// <summary>双圆基准中的第一个圆。</summary>
		CircleBase1,
		/// <summary>双圆基准中的第二个圆。</summary>
		CircleBase2,
		/// <summary>单圆基准点 ROI。</summary>
		CirclePoint
	}
}
