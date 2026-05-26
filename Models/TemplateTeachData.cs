using System;

namespace EdgeAlignInspect
{
	[Serializable]
	/// <summary>
	/// 模板示教后保存的 HALCON 模型数据。
	/// </summary>
	public sealed class TemplateTeachData
	{
		/// <summary>是否包含有效模板。</summary>
		public bool HasTemplate { get; set; }

		/// <summary>导出的 HALCON shape model 字节数据。</summary>
		public byte[] ModelBytes { get; set; }

		/// <summary>示教时模板参考点行坐标。</summary>
		public double RefRow { get; set; }

		/// <summary>示教时模板参考点列坐标。</summary>
		public double RefCol { get; set; }

		/// <summary>示教时模板参考角度，单位为弧度。</summary>
		public double RefAngle { get; set; }

		/// <summary>清空模板数据。</summary>
		public void Clear()
		{
			HasTemplate = false;
			ModelBytes = null;
			RefRow = 0.0;
			RefCol = 0.0;
			RefAngle = 0.0;
		}

		/// <summary>创建当前模板数据的深拷贝。</summary>
		public TemplateTeachData DeepClone()
		{
			return new TemplateTeachData
			{
				HasTemplate = HasTemplate,
				ModelBytes = ((ModelBytes == null) ? null : ((byte[])ModelBytes.Clone())),
				RefRow = RefRow,
				RefCol = RefCol,
				RefAngle = RefAngle
			};
		}
	}
}
