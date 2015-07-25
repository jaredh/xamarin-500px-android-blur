using System;

using Android.App;
using Android.Animation;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Android.OS;

using FiveHundredPixels;

namespace BlurSample
{
	[Activity (Label = "BlurSample", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		int count = 1;


		BlurringView mBlurringView;

		int[] mImageIds = {
			Resource.Drawable.p0, Resource.Drawable.p1, Resource.Drawable.p2, Resource.Drawable.p3, Resource.Drawable.p4,
			Resource.Drawable.p5, Resource.Drawable.p6, Resource.Drawable.p7, Resource.Drawable.p8, Resource.Drawable.p9
		};

		ImageView[] mImageViews = new ImageView[9];
		int mStartIndex;

		Random mRandom = new Random();

		bool mShifted;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			mBlurringView = FindViewById<BlurringView> (Resource.Id.blurring_view);
			View blurredView = FindViewById(Resource.Id.blurred_view);

			mBlurringView.SetBlurredView (blurredView);

			mImageViews [0] = FindViewById<ImageView> (Resource.Id.image0);
			mImageViews [1] = FindViewById<ImageView> (Resource.Id.image1);
			mImageViews [2] = FindViewById<ImageView> (Resource.Id.image2);
			mImageViews [3] = FindViewById<ImageView> (Resource.Id.image3);
			mImageViews [4] = FindViewById<ImageView> (Resource.Id.image4);
			mImageViews [5] = FindViewById<ImageView> (Resource.Id.image5);
			mImageViews [6] = FindViewById<ImageView> (Resource.Id.image6);
			mImageViews [7] = FindViewById<ImageView> (Resource.Id.image7);
			mImageViews [8] = FindViewById<ImageView> (Resource.Id.image8);

			// Get our button from the layout resource,
			// and attach an event to it

			Button shuffleButton = FindViewById<Button> (Resource.Id.shuffle_button);
			Button shiftButton = FindViewById<Button> (Resource.Id.shift_button);


			shiftButton.Click += (sender, e) => { 
				if (!mShifted) {
					foreach (ImageView imageView in mImageViews) {
						ObjectAnimator tx = ObjectAnimator.OfFloat (imageView, "translationX", (float)((mRandom.NextDouble () - 0.5f) * 500));
						tx.Update += (s, ea) => mBlurringView.Invalidate();
						ObjectAnimator ty = ObjectAnimator.OfFloat (imageView, "translationY", (float)((mRandom.NextDouble () - 0.5f) * 500));
						tx.Update += (s, ea) => mBlurringView.Invalidate();
						AnimatorSet set = new AnimatorSet();
						set.PlayTogether(tx, ty);
						set.SetDuration(3000);
						set.SetInterpolator(new OvershootInterpolator());
						set.AnimationStart += (s, ea) => imageView.SetLayerType(LayerType.Hardware, null);
						set.AnimationEnd += (s, ea) => imageView.SetLayerType(LayerType.None, null);
						set.AnimationCancel += (s, ea) => imageView.SetLayerType (LayerType.None, null);
						set.Start();
					}
					mShifted = true;
				} else {
					foreach (ImageView imageView in mImageViews) {
						ObjectAnimator tx = ObjectAnimator.OfFloat (imageView, "translationX", 0);
						tx.Update += (s, ea) => mBlurringView.Invalidate();
						ObjectAnimator ty = ObjectAnimator.OfFloat (imageView, "translationY", 0);
						tx.Update += (s, ea) => mBlurringView.Invalidate();
						AnimatorSet set = new AnimatorSet();
						set.PlayTogether(tx, ty);
						set.SetDuration(3000);
						set.SetInterpolator(new OvershootInterpolator());
						//					set.AddListener(new AnimationEndListener(imageView));
						set.AnimationStart += (s, ea) => imageView.SetLayerType(LayerType.Hardware, null);
						set.AnimationEnd += (s, ea) => imageView.SetLayerType(LayerType.None, null);
						set.AnimationCancel += (s, ea) => imageView.SetLayerType (LayerType.None, null);
						set.Start();
					}
					mShifted = false;
				}
			};

			shuffleButton.Click += (sender, e) => {
				int newStartIndex;

				do {
					newStartIndex = mImageIds[mRandom.Next(mImageIds.Length)];
				} while (newStartIndex == mStartIndex);
				mStartIndex = newStartIndex;

				for (int i = 0; i < mImageViews.Length; i++) {
					int drawableId = mImageIds[(mStartIndex +i) % mImageIds.Length];
					mImageViews[i].SetImageDrawable(ApplicationContext.Resources.GetDrawable(drawableId));
				}

				mBlurringView.Invalidate();
			};

		}

	}
}


