using System;

namespace EdgeAlignInspect
{
	[Serializable]
	/// <summary>
	/// 检测 ROI 可绑定的基准类型。
	/// </summary>
	public enum ReferenceBaseKind
	{
		/// <summary>绑定线基准 ROI。</summary>
		LineRoi,
		/// <summary>绑定由两个圆心形成的圆基准。</summary>
		CirclePair,
		/// <summary>绑定单圆拟合圆心作为基准点。</summary>
		CirclePoint
	}
}
