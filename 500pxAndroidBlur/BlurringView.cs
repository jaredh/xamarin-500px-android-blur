using System;

using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Renderscripts;

namespace FiveHundredPixels
{
	public class BlurringView : View
	{
		int downsampleFactor;
		int overlayColor;

		View blurredView;

		int blurredViewWidth;
		int blurredViewHeight;

		bool downsampleFactorChanged;

		Bitmap bitmapToBlur;
		Bitmap blurredBitmap;

		Canvas blurringCanvas;

		RenderScript renderScript;

		ScriptIntrinsicBlur blurScript;

		Allocation blurInput;
		Allocation blurOutput;


		public BlurringView (Context context) : base (context, null)
		{
		}

		public BlurringView (Context context, IAttributeSet attrs) : base (context, attrs)
		{

			var defaultBlurRadius = Context.Resources.GetInteger (Resource.Integer.default_blur_radius);
			var defaultDownsampleFactor = Context.Resources.GetInteger (Resource.Integer.default_downsample_factor);
			var defaultOverlayColor = Context.Resources.GetColor (Resource.Color.default_overlay_color);

			InitializeRenderScript (context);

			var a = context.ObtainStyledAttributes (attrs, Resource.Styleable.PxBlurringView);
			SetBlurRadius (a.GetInt(Resource.Styleable.PxBlurringView_blurRadius, defaultBlurRadius));
			SetDownsampleFactor (a.GetInt(Resource.Styleable.PxBlurringView_downsampleFactor, defaultDownsampleFactor));
			SetOverlayColor (a.GetInt (Resource.Styleable.PxBlurringView_overlayColor, defaultOverlayColor));
			a.Recycle ();
		}


		public void SetBlurredView (View blurredView)
		{
			this.blurredView = blurredView;
		}

		protected override void OnDetachedFromWindow ()
		{
			base.OnDetachedFromWindow ();

			if (renderScript != null)
				renderScript.Destroy ();
		}

		protected override void OnDraw (Android.Graphics.Canvas canvas)
		{
			base.OnDraw (canvas);

			if (blurredView != null) {
				if (Prepare ()) {
					// If the background of the blurred view is a color drawable, we use it to clear
					// the blurring canvas, which ensures that the edges of the child views are blurred
					// as well; otherwise we clear the blurring canvas with a transparent color.

					if (blurredView.Background != null && blurredView.Background is ColorDrawable)
						bitmapToBlur.EraseColor (blurredView.DrawingCacheBackgroundColor.ToArgb ());
					else
						bitmapToBlur.EraseColor (Color.Transparent);

					blurredView.Draw (blurringCanvas);
					Blur ();

					canvas.Save ();
					canvas.Translate (blurredView.GetX () - GetY (), blurredView.GetY () - GetY ());
					canvas.Scale (downsampleFactor, downsampleFactor);
					canvas.DrawBitmap (blurredBitmap, 0, 0, null);
					canvas.Restore ();
				}

				canvas.DrawColor (new Color(overlayColor));
			}
		}

		public void SetBlurRadius (int radius)
		{
			blurScript.SetRadius (radius);
		}

		public void SetDownsampleFactor (int factor)
		{
			if (factor <= 0)
				throw new ArgumentOutOfRangeException ("Downsample factor must be greater than 0.");

			if (downsampleFactor != factor) {
				downsampleFactor = factor;
				downsampleFactorChanged = true;
			}
		}

		public void SetOverlayColor (int color)
		{
			overlayColor = color;
		}

		void InitializeRenderScript (Context context)
		{
			renderScript = RenderScript.Create (context);
			blurScript = ScriptIntrinsicBlur.Create (renderScript, Element.U8_4 (renderScript));
		}

		protected bool Prepare ()
		{
			int width = blurredView.Width;
			int height = blurredView.Height;

			if (blurringCanvas == null || downsampleFactorChanged || blurredViewWidth != width || blurredViewHeight != height) {
				downsampleFactorChanged = false;

				blurredViewWidth = width;
				blurredViewHeight = height;

				int scaledWidth = width / downsampleFactor;
				int scaledHeight = height / downsampleFactor;

				// The following manipulation is to avoid some RenderScript artifacts at the edge
				scaledWidth = scaledWidth - scaledWidth % 4 + 4;
				scaledHeight = scaledHeight - scaledHeight % 4 + 4;

				if (blurredBitmap == null || blurredBitmap.Width != scaledWidth || blurredBitmap.Height != scaledHeight) {
					bitmapToBlur = Bitmap.CreateBitmap (scaledWidth, scaledHeight, Bitmap.Config.Argb8888);

					if (bitmapToBlur == null)
						return false;

					blurredBitmap = Bitmap.CreateBitmap (scaledWidth, scaledHeight, Bitmap.Config.Argb8888);

					if (blurredBitmap == null)
						return false;
				}

				blurringCanvas = new Canvas (bitmapToBlur);
				blurringCanvas.Scale (1f / downsampleFactor, 1f / downsampleFactor);
				blurInput = Allocation.CreateFromBitmap (renderScript, bitmapToBlur, Allocation.MipmapControl.MipmapNone, AllocationUsage.Script);
				blurOutput = Allocation.CreateTyped (renderScript, blurInput.Type);
			}

			return true;
		}

		protected void Blur ()
		{
			blurInput.CopyFrom (bitmapToBlur);	
			blurScript.SetInput (blurInput);
			blurScript.ForEach (blurOutput);
			blurOutput.CopyTo (blurredBitmap);
		}

	}
}

