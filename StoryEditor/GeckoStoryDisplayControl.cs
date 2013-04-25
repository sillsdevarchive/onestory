using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;             // for LinkLabel

namespace OneStoryProjectEditor
{
	public abstract class GeckoStoryDisplayControl : OseGeckoWebBrowser
	{
		public const string CstrTextAreaPrefix = "ta";
		public const string CstrParagraphPrefix = "tp";
		public const string CstrButtonPrefix = "btn";

		internal LinkLabel LineNumberLink;

		internal string StrIdToScrollTo;

		public StoryEditor TheSe { get; set; }
		public virtual StoryData StoryData { get; set; }

		public abstract void LoadDocument();
	}
}
