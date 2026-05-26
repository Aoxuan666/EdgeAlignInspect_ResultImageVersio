using System;

namespace EdgeAlignInspect
{
	[Serializable]
	/// <summary>
	/// 检测 ROI 自身拟合线的角度参考方式。
	/// </summary>
	public enum DetectAngleReferenceMode
	{
		/// <summary>参考绑定基准线方向。</summary>
		ParallelToBase,
		/// <summary>参考图像水平方向。</summary>
		Horizontal,
		/// <summary>参考图像垂直方向。</summary>
		Vertical
	}
}
