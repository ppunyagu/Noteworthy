using System;
using Android.Support.V7.Widget;

namespace Noteworthy
{
	public class GridSpacingItemDecoration : RecyclerView.ItemDecoration
	{

		private int spanCount;
		private int spacing;
		private bool includeEdge, isList;


		public GridSpacingItemDecoration(int spanCount, int spacing, bool includeEdge, bool isList)
		{
			this.spanCount = spanCount;
			this.spacing = spacing;
			this.includeEdge = includeEdge;
			this.isList = isList;
		}
		public GridSpacingItemDecoration()
		{
			this.spanCount = 2;
			this.spacing = 10;
			this.includeEdge = false;
		}

		public override void GetItemOffsets(global::Android.Graphics.Rect outRect, global::Android.Views.View view, RecyclerView parent, RecyclerView.State state)
		{
			try
			{
				int position = parent.GetChildAdapterPosition(view); // item position
				int column = position % spanCount; // item column

				if (includeEdge)
				{
					outRect.Left = spacing - column * spacing / spanCount; // spacing - column * ((1f / spanCount) * spacing)
					outRect.Right = (column + 1) * spacing / spanCount; // (column + 1) * ((1f / spanCount) * spacing)

					if (position < spanCount)
					{ // top edge
						outRect.Top = spacing;
					}
					outRect.Bottom = spacing; // item bottom
				}
				else {
					outRect.Left = column * spacing / spanCount; // column * ((1f / spanCount) * spacing)
					outRect.Right = spacing - (column + 1) * spacing / spanCount; // spacing - (column + 1) * ((1f /    spanCount) * spacing)
					if (isList)
					{
						if (position >= spanCount)
						{
							//For Vertical Space
							outRect.Top = spacing; // item top
						}
					}
				}
			}
			catch (Exception ex)
			{
				Utility.ExceptionHandler(Class.SimpleName, "GetItemOffsets", ex);
			}
		}
	}
}

