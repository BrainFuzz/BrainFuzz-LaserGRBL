//Copyright (c) 2016-2021 Diego Settimi - https://github.com/arkypita/

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GPLv3 General Public License  along with this program; if not, write to the Free Software  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307,  USA. using System;

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LaserGRBL.UserControls {

  public enum FillStyles {
	Solid,
	Dashed
  }


  [Description("Color Progress Bar"), ToolboxBitmap(typeof(ProgressBar)), Designer(typeof(ColorProgressBarDesigner))]
  public partial class ColorProgressBar :System.Windows.Forms.UserControl {

	
	// Set default values
	private double _value   = 0;
	private double _minimum = 0;
	private double _maximum = 100;
	private double _step    = 10;
	private bool _reverse   = false;
	private bool _drawProgressString    = false;
	private int _progressStringDecimals = 0;

	private bool _throwException = false;

	private FillStyles _fillStyle = FillStyles.Dashed;
	private Color _fillColor      = Color.White;
	private Color _barColor       = Color.FromArgb(255, 128, 128);
	private Color _borderColor    = Color.Black;


	public ColorProgressBar() {
	  base.Size = new Size(150, 15);
	  SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer, true);
	}


	[Description("ColorProgressBar color"), Category("ColorProgressBar")]
	public Color BarColor {
	  get { return _barColor; }
	  set {
		if (value != _barColor) {
		  _barColor = value;
		  this.Invalidate();
		}
	  }
	}


	[Description("ColorProgressBar fill color"), Category("ColorProgressBar")]
	public Color FillColor {
	  get { return _fillColor; }
	  set {
		_fillColor = value;
		this.Invalidate();
	  }
	}


	[Description("Reverse Direction"), Category("ColorProgressBar")]
	public bool Reverse {
	  get { return _reverse; }
	  set {
		_reverse = value;
		this.Invalidate();
	  }
	}


	[Description("Throw exception on inconsistent value"), Category("ColorProgressBar")]
	public bool ThrowException {
	  get { return _throwException; }
	  set {
		_throwException = value;
		this.Invalidate();
	  }
	}


	[Description("ColorProgressBar fill style"), Category("ColorProgressBar"), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	public FillStyles FillStyle {
	  get { return _fillStyle; }
	  set {
		if (value != FillStyle) {
		  _fillStyle = value;
		  this.Invalidate();
		}
	  }
	}


	[Description("The current value for the ColorProgressBar, " + "in the range specified by the Minimum and Maximum properties."), Category("ColorProgressBar"), 
	  RefreshProperties(RefreshProperties.All)]
	public double Value {
	  // the rest of the Properties windows must be updated when this peroperty is changed.
	  get { return _value; }
	  set {

		if ((value != _value)) {
		  if (value < _minimum & ThrowException) {
			throw new ArgumentException("'" + value + "' is not a valid value for 'Value'.\r\n'Value' must be between 'Minimum' and 'Maximum'.");
		  }

		  if (value > _maximum & ThrowException) {
			throw new ArgumentException("'" + value + "' is not a valid value for 'Value'.\r\n'Value' must be between 'Minimum' and 'Maximum'.");
		  }

		  _value = value;
		  this.Invalidate();
		}
	  }
	}


	[Description("The lower bound of the range this ColorProgressbar is working with."), Category("ColorProgressBar"), RefreshProperties(RefreshProperties.All)]
	public double Minimum {
	  get { return _minimum; }
	  set {
		if (value != _minimum) {
		  if (value >= Maximum) {
			if (ThrowException)
			  throw new Exception("Maximum must be smaller then minimum");
		  }
		  _value = Math.Max(_value, value);
		  _minimum = value;
		  this.Invalidate();
		}
	  }
	}


	[Description("The upper bound of the range this ColorProgressbar is working with."), Category("ColorProgressBar"), RefreshProperties(RefreshProperties.All)]
	public double Maximum {
	  get { return _maximum; }
	  set {
		if (value != _maximum) {
		  if (value <= Minimum) {
			if (ThrowException)
			  throw new Exception("Maximum must be greater then minimum");
		  }
		  _value = Math.Min(_value, value);
		  _maximum = value;
		  this.Invalidate();
		}
	  }
	}


	[Description("The amount to jump the current value of the control by when the Step() method is called."), Category("ColorProgressBar")]
	public double Step {
	  get { return _step; }
	  set {
		_step = value;
		this.Invalidate();
	  }
	}


	[Description("The border color of ColorProgressBar"), Category("ColorProgressBar")]
	public Color BorderColor {
	  get { return _borderColor; }
	  set {
		_borderColor = value;
		this.Invalidate();
	  }
	}


	[Description("Draw progress string"), Category("ColorProgressBar")]
	public bool DrawProgressString {
	  get { return _drawProgressString; }
	  set {
		_drawProgressString = value;
		this.Invalidate();
	  }
	}


	[Description("Draw progress string decimal point"), Category("ColorProgressBar")]
	public int ProgressStringDecimals {
	  get { return _progressStringDecimals; }
	  set {
		_progressStringDecimals = value;
		this.Invalidate();
	  }
	}

	
	/// <summary>
	/// Call the PerformStep() method to increase the value displayed by the amount set in the Step property
	/// </summary>
	public void PerformStep() {
	  if (_value < _maximum) {
		_value += _step;
	  } else {
		_value = _maximum;
	  }
	  this.Invalidate();
	}


	/// <summary>
	/// Call the PerformStepBack() method to decrease the value displayed by the amount set in the Step property
	/// </summary>
	public void PerformStepBack() {
	  if (_value > _minimum) {
		_value -= _step;
	  } else {
		_value = _minimum;
	  }
	  this.Invalidate();
	}


	/// <summary>
	/// Call the Increment() method to increase the value displayed by an integer you specify
	/// </summary>
	/// <param name="value"></param>
	public void Increment(double value) {
	  if (_value < _maximum) {
        _value += value;
	  } else {
		_value = _maximum;
	  }
	  this.Invalidate();
	}


	/// <summary>
	/// Call the Decrement() method to decrease the value displayed by an integer you specify
	/// </summary>
	/// <param name="value"></param>
	public void Decrement(double value) {
	  if (_value > _minimum) {
		_value -= value;
	  } else {
		_value = _minimum;
	  }
	  this.Invalidate();
	}
	//Decrement

	protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
	  DrawBackground(e.Graphics);
	  DrawProgres(e.Graphics);
	  if (FillStyle == FillStyles.Dashed)
		DrawTick(e.Graphics);
	  if (DrawProgressString)
		DrawString(e.Graphics);
	  DrawBorder(e.Graphics);
	}


	protected virtual void DrawString(Graphics g) {
	  double pval = (Value - Minimum) / (Maximum - Minimum);
	  string pstr = null;
	  if ((double.IsNaN(pval))) {
		pstr = "";
	  } else {
		pstr = string.Format("{0:p" + ProgressStringDecimals.ToString() + "}", pval);
	  }
	  using (SolidBrush B = new SolidBrush(ForeColor)) {
		g.DrawString(pstr, Font, B, Convert.ToInt32((Width - g.MeasureString(pstr, Font).Width) / 2), Convert.ToInt32((Height - g.MeasureString(pstr, Font).Height) / 2));
	  }
	}


	protected virtual void DrawBorder(Graphics g) {
	  //SALVA HEIGHT E WIDTH PER CALCOLI PIU VELOCI
	  int W = this.ClientRectangle.Width;
	  int H = this.ClientRectangle.Height;

	  Point[] BordoGrigio = {
				new Point(0, 2),
				new Point(1, 1),
				new Point(2, 0),
				new Point(W - 3, 0),
				new Point(W - 2, 1),
				new Point(W - 1, 2),
				new Point(W - 1, H - 3),
				new Point(W - 2, H - 2),
				new Point(W - 3, H - 1),
				new Point(2, H - 1),
				new Point(1, H - 2),
				new Point(0, H - 3),
				new Point(0, 2)
			};
	  Point[] OmbraSopra = {
				new Point(1, 2),
				new Point(2, 1),
				new Point(3, 1),
				new Point(W - 4, 1),
				new Point(W - 3, 1),
				new Point(W - 2, 2)
			};

	  g.DrawCurve(Pens.Gray, BordoGrigio, 0);
	  //BordoGrigio
	  g.DrawCurve(Pens.LightGray, OmbraSopra, 0);
	  // Ombra Sopra
	  g.DrawLine(Pens.LightGray, 1, 2, 1, H - 3);
	  //Ombra Lato Sx
	}

	protected virtual void DrawBackground(Graphics g) {
	  //SALVA HEIGHT E WIDTH PER CALCOLI PIU VELOCI
	  int W = this.ClientRectangle.Width;
	  int H = this.ClientRectangle.Height;

	  //CREA IL PATH DELLA PARTE DA RIEMPIRE CON IL FILLCOLOR (INTERNA AL BORDO)
	  GraphicsPath path = new GraphicsPath();
	  path.AddLines(new Point[] {
				new Point(1, 2),
				new Point(2, 1),
				new Point(3, 0),
				new Point(W - 3, 0),
				new Point(W - 2, 1),
				new Point(W - 1, 2),
				new Point(W - 1, H - 3),
				new Point(W - 2, H - 2),
				new Point(W - 3, H - 1),
				new Point(3, H - 1),
				new Point(2, H - 2),
				new Point(1, H - 3),
				new Point(1, 2)
			});

	  Region reg = new Region(path);
	  //CREA LA REGION DA RIEMPIRE PARTENDO DAL PATH
	  Color col = default(Color);
	  if (this.Enabled) {
		col = this.FillColor;
	  } else {
		col = Color.DarkGray;
	  }
	  using (SolidBrush fillbrush = new SolidBrush(col)) {
		g.FillRegion(fillbrush, reg);
	  }
	}

	//L = Larghezza di una tacca
	protected int L = 6;
	//S = Spazio tra una tacca e l'altra
	protected int S = 2;

	protected virtual void DrawProgres(System.Drawing.Graphics g) {
	  if (this.Enabled) {
		DrawBar(g, ClientRectangle.Width, ClientRectangle.Height, Value, BarColor);
	  } else {
		DrawBar(g, ClientRectangle.Width, ClientRectangle.Height, Value, Color.LightGray);
	  }
	}

	protected void DrawBar(Graphics G, int W, int H, double V, Color C) {
	  int BarWidth = 0;

	  if (!double.IsNaN(V) && !double.IsInfinity(V) && !((Maximum - Minimum) == 0)) {
		BarWidth = Convert.ToInt32(Math.Floor(((W - 3) * (Math.Min(V, Maximum) - Minimum)) / (Maximum - Minimum)));
	  }


	  int BarHeight = H - 4;

	  //If FillStyle = FillStyles.Dashed Then BarWidth = ((CInt(BarWidth / SL)) * SL) - S

	  //BarWidth = Math.Min(W - 3, BarWidth)


	  if (!(BarWidth <= 0) & !(BarHeight <= 0)) {
		Rectangle ColoredBar = default(Rectangle);
		if (Reverse) {
		  ColoredBar = new Rectangle(W - BarWidth - 1, 2, BarWidth, BarHeight);
		} else {
		  ColoredBar = new Rectangle(2, 2, BarWidth, BarHeight);
		}


		using (LinearGradientBrush brush = new LinearGradientBrush(ColoredBar, this.FillColor, C, 90f)) {
		  float[] relativeIntensities = {
						0.1f,
						1f,
						1f,
						1f,
						1f,
						0.85f,
						0.1f
					};
		  float[] relativePositions = {
						0f,
						0.2f,
						0.5f,
						0.5f,
						0.5f,
						0.8f,
						1f
					};

		  // create a Blend object and assign it to brush
		  Blend blend     = new Blend();
		  blend.Factors   = relativeIntensities;
		  blend.Positions = relativePositions;
		  brush.Blend     = blend;

		  G.FillRectangle(brush, ColoredBar);
		}
	  }
	}


	protected virtual void DrawTick(Graphics g) {
	  //SALVA HEIGHT E WIDTH PER CALCOLI PIU VELOCI
	  int W = this.ClientRectangle.Width;
	  int H = this.ClientRectangle.Height;

	  int CurPos = L + 3;
	  while (CurPos < W) {
		using (Pen P = new Pen(this.FillColor, S)) {
		  g.DrawLine(P, (CurPos), 0, (CurPos), this.Height);
		}
		CurPos += S + L;
	  }
	}


	protected override void SetBoundsCore(int x, int y, int width, int height, System.Windows.Forms.BoundsSpecified specified) {
	  width = Math.Max(width, 20);
	  height = Math.Max(height, 6);

	  int calcwidth = width;
	  if (FillStyle == FillStyles.Dashed)
		calcwidth = (Convert.ToInt32(calcwidth / 8) * 8) + 1;
	  base.SetBoundsCore(x, y, calcwidth, height, specified);
	}
  }
}

//=======================================================
//Service provided by Telerik (www.telerik.com)
//Conversion powered by NRefactory.
//Twitter: @telerik
//Facebook: facebook.com/telerik
//=======================================================
