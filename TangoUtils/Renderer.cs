﻿using System;

/*
 * Copyright 2014 HobbiSoft. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.Apache.Org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.*/

namespace com.projecttango.tangoutils
{
    using Android.Views;
    using Matrix = Android.Opengl.Matrix;
	using Log = Android.Util.Log;
	using MotionEvent = Android.Views.MotionEvent;

	public class Renderer
	{

		protected internal const int FIRST_PERSON = 0;
		protected internal const int TOP_DOWN = 1;
		protected internal const int THIRD_PERSON = 2;
		protected internal const int THIRD_PERSON_FOV = 65;
		protected internal const int TOPDOWN_FOV = 65;
		protected internal const int MATRIX_4X4 = 16;

		protected internal const float CAMERA_FOV = 37.8f;
		protected internal const float CAMERA_NEAR = 0.01f;
		protected internal const float CAMERA_FAR = 200f;
		protected internal float mCameraAspect;
		protected internal float[] mProjectionMatrix = new float[MATRIX_4X4];
		private ModelMatCalculator mModelMatCalculator;
		private int viewId = 2;
		protected internal float[] mViewMatrix = new float[MATRIX_4X4];
		protected internal float[] mCameraPosition;
		protected internal float[] mLookAtPosition;
		protected internal float[] mCameraUpVector;
		private float[] mDevicePosition;
		private float mCameraOrbitRadius;
		private float mRotationX;
		private float mRotationY;
		private float mPreviousRotationX;
		private float mPreviousRotationY;
		private float mPreviousTouchX;
		private float mPreviousTouchY;
		private float mTouch1X, mTouch2X, mTouch1Y, mTouch2Y, mTouchStartDistance, mTouchMoveDistance, mStartCameraRadius;

		public Renderer()
		{
			mModelMatCalculator = new ModelMatCalculator();
			mRotationX = (float) Math.PI / 4;
			mRotationY = 0;
			mCameraOrbitRadius = 5.0f;
			mCameraPosition = new float[3];
			mCameraPosition[0] = 5f;
			mCameraPosition[1] = 5f;
			mCameraPosition[2] = 5f;
			mDevicePosition = new float[3];
			mDevicePosition[0] = 0;
			mDevicePosition[1] = 0;
			mDevicePosition[2] = 0;
		}

		/// <summary>
		/// Update the view matrix of the Renderer to follow the position of the
		/// device in the current perspective.
		/// </summary>
		public virtual void updateViewMatrix()
		{
			mDevicePosition = mModelMatCalculator.Translation;

			switch (viewId)
			{
			case FIRST_PERSON:
				float[] invertModelMat = new float[MATRIX_4X4];
				Matrix.SetIdentityM(invertModelMat, 0);

				float[] temporaryMatrix = new float[MATRIX_4X4];
				Matrix.SetIdentityM(temporaryMatrix, 0);

				Matrix.SetIdentityM(mViewMatrix, 0);
				Matrix.InvertM(invertModelMat, 0, mModelMatCalculator.ModelMatrix, 0);
				Matrix.MultiplyMM(temporaryMatrix, 0, mViewMatrix, 0, invertModelMat, 0);
				Array.Copy(temporaryMatrix, 0, mViewMatrix, 0, 16);
				break;
			case THIRD_PERSON:

				Matrix.SetLookAtM(mViewMatrix, 0, mDevicePosition[0] + mCameraPosition[0], mCameraPosition[1] + mDevicePosition[1], mCameraPosition[2] + mDevicePosition[2], mDevicePosition[0], mDevicePosition[1], mDevicePosition[2], 0f, 1f, 0f);
				break;
			case TOP_DOWN:
				// Matrix.SetIdentityM(mViewMatrix, 0);
				Matrix.SetLookAtM(mViewMatrix, 0, mDevicePosition[0] + mCameraPosition[0], mCameraPosition[1], mCameraPosition[2] + mDevicePosition[2], mDevicePosition[0] + mCameraPosition[0], mCameraPosition[1] - 5, mCameraPosition[2] + mDevicePosition[2], 0f, 0f, -1f);
				break;
			default:
				viewId = THIRD_PERSON;
				return;
			}
		}

		public virtual bool onTouchEvent(MotionEvent @event)
		{
			if (viewId == THIRD_PERSON)
			{
				int pointCount = @event.PointerCount;
				if (pointCount == 1)
				{
					switch (@event.Action)
					{
                        case MotionEventActions.Down:
                            {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float x = event.GetX();
						float x = @event.GetX();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float y = event.GetY();
						float y = @event.GetY();
						// Remember where we started
						mPreviousTouchX = x;
						mPreviousTouchY = y;
						mPreviousRotationX = mRotationX;
						mPreviousRotationY = mRotationY;
						break;
					}
					case MotionEventActions.Move:
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float x = event.GetX();
						float x = @event.GetX();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float y = event.GetY();
						float y = @event.GetY();
						// Calculate the distance moved
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float dx = mPreviousTouchX - x;
						float dx = mPreviousTouchX - x;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float dy = mPreviousTouchY - y;
						float dy = mPreviousTouchY - y;
						mRotationX = mPreviousRotationX + (float)(Math.PI * dx / 1900); // ScreenWidth
						mRotationY = mPreviousRotationY + (float)(Math.PI * dy / 1200); // Screen height
						if (mRotationY > (float) Math.PI)
						{
							mRotationY = (float) Math.PI;
						}
						if (mRotationY < 0)
						{
							mRotationY = 0.0f;
						}
						mCameraPosition[0] = (float)(mCameraOrbitRadius * Math.Sin(mRotationX));
						mCameraPosition[1] = (float)(mCameraOrbitRadius * Math.Cos(mRotationY));
						mCameraPosition[2] = (float)(mCameraOrbitRadius * Math.Cos(mRotationX));
						break;
					}
					}
				}
				if (pointCount == 2)
				{
					switch (@event.ActionMasked)
					{
					case MotionEventActions.Down:
                    case MotionEventActions.PointerDown:
                    {
						mTouch1X = @event.GetX(0);
						mTouch1Y = @event.GetY(0);
						mTouch2X = @event.GetX(1);
						mTouch2Y = @event.GetY(1);
						mTouchStartDistance = (float) Math.Sqrt(Math.Pow(mTouch1X - mTouch2X, 2) + Math.Pow(mTouch1Y - mTouch2Y, 2));
						mStartCameraRadius = mCameraOrbitRadius;
						break;
					}
					case MotionEventActions.Move:
					{
						mTouch1X = @event.GetX(0);
						mTouch1Y = @event.GetY(0);
						mTouch2X = @event.GetX(1);
						mTouch2Y = @event.GetY(1);
						mTouchMoveDistance = (float) Math.Sqrt(Math.Pow(mTouch1X - mTouch2X, 2) + Math.Pow(mTouch1Y - mTouch2Y, 2));
						float tmp = 0.05f * (mTouchMoveDistance - mTouchStartDistance);
						mCameraOrbitRadius = mStartCameraRadius - tmp;
						if (mCameraOrbitRadius < 1)
						{
							mCameraOrbitRadius = 1;
						}
						mCameraPosition[0] = (float)(mCameraOrbitRadius * Math.Sin(mRotationX));
						mCameraPosition[1] = (float)(mCameraOrbitRadius * Math.Cos(mRotationY));
						mCameraPosition[2] = (float)(mCameraOrbitRadius * Math.Cos(mRotationX));
						break;
					}
					case MotionEventActions.PointerUp:
					{
						int index = @event.ActionIndex == 0 ? 1 : 0;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float x = event.GetX(index);
						float x = @event.GetX(index);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float y = event.GetY(index);
						float y = @event.GetY(index);
						// Remember where we started
						mPreviousTouchX = x;
						mPreviousTouchY = y;
						mPreviousRotationX = mRotationX;
						mPreviousRotationY = mRotationY;
					}
				break;
					}
				}
			}
			else if (viewId == TOP_DOWN)
			{
				int pointCount = @event.PointerCount;
				if (pointCount == 1)
				{
					switch (@event.Action)
					{
                        case MotionEventActions.Down:
                    {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float x = event.GetX();
						float x = @event.GetX();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float y = event.GetY();
						float y = @event.GetY();
						// Remember where we started
						mPreviousTouchX = x;
						mPreviousTouchY = y;
						mPreviousRotationX = mCameraPosition[0];
						mPreviousRotationY = mCameraPosition[2];
						break;
					}
					case MotionEventActions.Move:
					{
                                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                                //ORIGINAL LINE: final float x = event.GetX();
                                float x = @event.GetX();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float y = event.GetY();
						float y = @event.GetY();
						// Calculate the distance moved
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float dx = mPreviousTouchX - x;
						float dx = mPreviousTouchX - x;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float dy = mPreviousTouchY - y;
						float dy = mPreviousTouchY - y;
						mCameraPosition[0] = mPreviousRotationX + dx / 190;
						mCameraPosition[2] = mPreviousRotationY + dy / 120;
						break;
					}
					}
				}
				if (pointCount == 2)
				{
					switch (@event.ActionMasked)
					{
                    case MotionEventActions.Down:
                        case MotionEventActions.PointerDown:
                    {
						mTouch1X = @event.GetX(0);
						mTouch1Y = @event.GetY(0);
						mTouch2X = @event.GetX(1);
						mTouch2Y = @event.GetY(1);
						mTouchStartDistance = (float) Math.Sqrt(Math.Pow(mTouch1X - mTouch2X, 2) + Math.Pow(mTouch1Y - mTouch2Y, 2));
						mStartCameraRadius = mCameraPosition[1];
						Log.Info("Start Radius is :", "" + mStartCameraRadius);
						break;
					}
                    case MotionEventActions.Move:
                    {
						mTouch1X = @event.GetX(0);
						mTouch1Y = @event.GetY(0);
						mTouch2X = @event.GetX(1);
						mTouch2Y = @event.GetY(1);
						mTouchMoveDistance = (float) Math.Sqrt(Math.Pow(mTouch1X - mTouch2X, 2) + Math.Pow(mTouch1Y - mTouch2Y, 2));
						float tmp = 0.05f * (mTouchMoveDistance - mTouchStartDistance);
						mCameraPosition[1] = mStartCameraRadius - tmp;
						break;
					}
                    case MotionEventActions.PointerUp:
                    {
						int index = @event.ActionIndex == 0 ? 1 : 0;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float x = event.GetX(index);
						float x = @event.GetX(index);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float y = event.GetY(index);
						float y = @event.GetY(index);
						// Remember where we started
						mPreviousTouchX = x;
						mPreviousTouchY = y;
						mPreviousRotationX = mCameraPosition[0];
						mPreviousRotationY = mCameraPosition[2];
					}
				break;
					}
				}
			}
			return true;
		}

		public virtual void setFirstPersonView()
		{
			viewId = FIRST_PERSON;
			Matrix.PerspectiveM(mProjectionMatrix, 0, CAMERA_FOV, mCameraAspect, CAMERA_NEAR, CAMERA_FAR);
		}

		public virtual void setThirdPersonView()
		{
			viewId = THIRD_PERSON;
			mCameraPosition[0] = 5;
			mCameraPosition[1] = 5;
			mCameraPosition[2] = 5;
			mRotationX = mRotationY = (float)(Math.PI / 4);
			mCameraOrbitRadius = 5.0f;
			Matrix.PerspectiveM(mProjectionMatrix, 0, THIRD_PERSON_FOV, mCameraAspect, CAMERA_NEAR, CAMERA_FAR);
		}

		public virtual void setTopDownView()
		{
			viewId = TOP_DOWN;
			mCameraPosition[0] = 0;
			mCameraPosition[1] = 5;
			mCameraPosition[2] = 0;
			Matrix.PerspectiveM(mProjectionMatrix, 0, TOPDOWN_FOV, mCameraAspect, CAMERA_NEAR, CAMERA_FAR);
		}

		public virtual void resetModelMatCalculator()
		{
			mModelMatCalculator = new ModelMatCalculator();
		}

		public virtual ModelMatCalculator ModelMatCalculator
		{
			get
			{
				return mModelMatCalculator;
			}
		}

		public virtual float[] ViewMatrix
		{
			get
			{
				return mViewMatrix;
			}
		}

		public virtual float[] ProjectionMatrix
		{
			get
			{
				return mProjectionMatrix;
			}
		}

	}

}