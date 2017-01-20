using System;
using Android.Support.V7.Widget;

namespace Noteworthy
{
	public class SpanSizeLookup : GridLayoutManager.SpanSizeLookup
	{
		dynamic adapter;
		GridLayoutManager manager;
		public SpanSizeLookup(dynamic adapter, GridLayoutManager manager)
		{
			this.adapter = adapter;
			this.manager = manager;
		}

		public override int GetSpanSize(int position)
		{
			try
			{
				return 1;
				//return adapter.isHeader(position) ? manager.SpanCount : 1;
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler(Class.SimpleName, "GetSpanSize", ex);
			}
			return 1;
		}
	}
}

