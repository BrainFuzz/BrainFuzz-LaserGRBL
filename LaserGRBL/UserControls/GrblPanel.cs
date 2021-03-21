//Copyright (c) 2016-2021 Diego Settimi - https://github.com/arkypita/

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GPLv3 General Public License  along with this program; if not, write to the Free Software  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307,  USA. using System;

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using LaserGRBL;

namespace LaserGRBL.UserControls {
  public partial class GrblPanel :UserControl {
	private GrblCore _GrblCore;
	private Bitmap _bitMap;
	private System.Threading.Thread _thread;
	private Matrix _lastMatrix;
	private GPoint mLastWPos;
	private GPoint mLastMPos;
	private float mCurF;
	private float mCurS;
	private bool mFSTrig;

	public GrblPanel() {
	  InitializeComponent();

	  SetStyle(ControlStyles.UserPaint, true);
	  SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
	  SetStyle(ControlStyles.AllPaintingInWmPaint, true);
	  SetStyle(ControlStyles.ResizeRedraw, true);
	  mLastWPos = GPoint.Zero;
	  mLastMPos = GPoint.Zero;

	  forcez = Settings.GetObject("Enale Z Jog Control", false);
	  SettingsForm.SettingsChanged += SettingsForm_SettingsChanged;
	}

	private void SettingsForm_SettingsChanged(object sender, EventArgs e) {
	  bool newforce = Settings.GetObject("Enale Z Jog Control", false);
	  if (newforce != forcez) {
		forcez = newforce;
		Invalidate();
	  }
	}

	protected override void OnPaintBackground(PaintEventArgs e) {
	  e.Graphics.Clear(ColorScheme.PreviewBackColor);
	}

	bool forcez = false;
	protected override void OnPaint(PaintEventArgs e) {
	  try {


		if (_bitMap != null)
		  e.Graphics.DrawImage(_bitMap, 0, 0, Width, Height);

		if (_GrblCore != null) {
		  PointF point = MachineToDraw(mLastWPos.ToPointF());
		  e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		  e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

		  using (Pen pen = GetPen(ColorScheme.PreviewCross, 2f)) {
			e.Graphics.DrawLine(pen, (int) point.X, (int) point.Y - 5, (int) point.X, (int) point.Y - 5 + 10);
			e.Graphics.DrawLine(pen, (int) point.X - 5, (int) point.Y, (int) point.X - 5 + 10, (int) point.Y);
		  }

		  using (Brush brush = GetBrush(ColorScheme.PreviewText)) {
			Rectangle rectangle = ClientRectangle;
			rectangle.Inflate(-5, -5);
			StringFormat stringFormat = new StringFormat();

			//  II | I
			// ---------
			// III | IV
			GrblFile.CartesianQuadrant quadrant = _GrblCore != null && _GrblCore.LoadedFile != null ? _GrblCore.LoadedFile.Quadrant : GrblFile.CartesianQuadrant.Unknown;
			stringFormat.Alignment = quadrant == GrblFile.CartesianQuadrant.II || quadrant == GrblFile.CartesianQuadrant.III ? StringAlignment.Near : StringAlignment.Far;
			stringFormat.LineAlignment = quadrant == GrblFile.CartesianQuadrant.III || quadrant == GrblFile.CartesianQuadrant.IV ? StringAlignment.Far : StringAlignment.Near;

			String position = string.Format("X: {0:0.000} Y: {1:0.000}", _GrblCore != null ? mLastMPos.X : 0, _GrblCore != null ? mLastMPos.Y : 0);

			if (_GrblCore != null && (mLastWPos.Z != 0 || mLastMPos.Z != 0 || forcez))
			  position += string.Format(" Z: {0:0.000}", mLastMPos.Z);

			if (_GrblCore != null && _GrblCore.WorkingOffset != GPoint.Zero)
			  position = position + "\n" + string.Format("X: {0:0.000} Y: {1:0.000}", _GrblCore != null ? mLastWPos.X : 0, _GrblCore != null ? mLastWPos.Y : 0);

			if (_GrblCore != null && _GrblCore.WorkingOffset != GPoint.Zero && (mLastWPos.Z != 0 || mLastMPos.Z != 0 || forcez))
			  position += string.Format(" Z: {0:0.000}", mLastWPos.Z);

			if (mCurF != 0 || mCurS != 0 || mFSTrig) {
			  mFSTrig = true;
			  String fs = string.Format("F: {0:00000.##} S: {1:000.##}", _GrblCore != null ? mCurF : 0, _GrblCore != null ? mCurS : 0);
			  position = position + "\n" + fs;
			}

			e.Graphics.DrawString(position, Font, brush, rectangle, stringFormat);
		  }
		}
	  } catch (Exception ex) {
		Logger.LogException("GrblPanel Paint", ex);
	  }
	}


	private Pen GetPen(Color color, float width) { return new Pen(color, width); }

	private Pen GetPen(Color color) { return new Pen(color); }

	private Brush GetBrush(Color color) { return new SolidBrush(color); }

	public void SetComProgram(GrblCore core) {
	  _GrblCore = core;
	  _GrblCore.OnFileLoading += OnFileLoading;
	  _GrblCore.OnFileLoaded += OnFileLoaded;
	}

	void OnFileLoading(long elapsed, string filename) {
	  AbortCreation();
	}

	void OnFileLoaded(long elapsed, string filename) {
	  RecreateBMP();
	}

	public void RecreateBMP() {
	  AbortCreation();

	  _thread = new System.Threading.Thread(DoTheWork);
	  _thread.Name = "GrblPanel Drawing Thread";
	  _thread.Start();
	}

	private void AbortCreation() {
	  if (_thread != null) {
		_thread.Abort();
		_thread = null;
	  }
	}

	protected override void OnSizeChanged(EventArgs e) {
	  base.OnSizeChanged(e);
	  RecreateBMP();
	}

	private void DoTheWork() {
	  try {
		Size wSize = Size;

		if (wSize.Width < 1 || wSize.Height < 1)
		  return;

		Bitmap bmp = new Bitmap(wSize.Width, wSize.Height);
		using (Graphics graphics = Graphics.FromImage(bmp)) {
		  graphics.SmoothingMode     = SmoothingMode.AntiAlias;
		  graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		  graphics.PixelOffsetMode   = PixelOffsetMode.HighQuality;
		  graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

		  if (_GrblCore != null /*&& Core.HasProgram*/)
			_GrblCore.LoadedFile.DrawOnGraphics(graphics, wSize);

		  _lastMatrix = graphics.Transform;
		}

		AssignBMP(bmp);
	  } catch (System.Threading.ThreadAbortException) {
		//standard condition for abort and recreation
	  } catch (Exception ex) {
		Logger.LogException("Drawing Preview", ex);
	  }
	}

	public PointF MachineToDraw(PointF point) {
	  if (_lastMatrix == null) {
		return point;
	  }

	  PointF[] pointArray = new PointF[] { point };
	  _lastMatrix.TransformPoints(pointArray);
	  return pointArray[0];
	}

	public PointF DrawToMachine(PointF p) {
	  if (_lastMatrix == null || !_lastMatrix.IsInvertible)
		return p;

	  Matrix mInvert = _lastMatrix.Clone();
	  mInvert.Invert();

	  PointF[] pointArray = new PointF[] { p };
	  mInvert.TransformPoints(pointArray);

	  return pointArray[0];
	}

	private void AssignBMP(Bitmap bitmap) {
	  lock (this) {
		if (_bitMap != null)
		  _bitMap.Dispose();

		_bitMap = bitmap;
	  }
	  Invalidate();
	}


	public void TimerUpdate() {
	  if (_GrblCore != null && (mLastWPos != _GrblCore.WorkPosition || mLastMPos != _GrblCore.MachinePosition || mCurF != _GrblCore.CurrentF || mCurS != _GrblCore.CurrentS)) {
		mLastWPos = _GrblCore.WorkPosition;
		mLastMPos = _GrblCore.MachinePosition;
		mCurF     = _GrblCore.CurrentF;
		mCurS     = _GrblCore.CurrentS;
		Invalidate();
	  }
	}


	internal void OnColorChange() {
	  RecreateBMP();
	}

	private void GrblPanel_MouseDoubleClick(object sender, MouseEventArgs e) {
	  if (Settings.GetObject("Click N Jog", true)) {
		PointF coord = DrawToMachine(new PointF(e.X, e.Y));
		_GrblCore.BeginJog(coord, e.Button == MouseButtons.Right);
	  }
	}
  }
}