using System.Collections.Generic;

namespace EdgeAlignInspect
{
	public static class NgReasonHelper
	{
		public static string ToText(NgReason reasons)
		{
			return ToText(reasons, InspectionLanguage.Chinese);
		}

		public static string ToText(NgReason reasons, InspectionLanguage language)
		{
			return LocalizedText.ReasonText(reasons, language);
		}
	}
}
