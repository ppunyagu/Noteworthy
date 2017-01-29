using Android.Graphics;
using Android.Content;

namespace Noteworthy
{
	public class FontFactory
	{
		public FontFactory()
		{
		}
		static Typeface t1;
		static Typeface t2;
		static Typeface t5;


		public static Typeface GetFontOpenSansSemibold(Context c)
		{
			if (t1 == null)
			{
				t1 = Typeface.CreateFromAsset(c.Assets, "OpenSans-Semibold.ttf");
			}
			return t1;
		}

		public static Typeface GetFontOpenSansRegular(Context c)
		{
			if (t2 == null)
			{
				t2 = Typeface.CreateFromAsset(c.Assets, "OpenSans-Regular.ttf");
			}
			return t2;
		}

		public static Typeface GetFontOpenSansBold(Context c)
		{
			if (t5 == null)
			{
				t5 = Typeface.CreateFromAsset(c.Assets, "OpenSans-Bold.ttf");
			}
			return t5;
		}
	}
}

