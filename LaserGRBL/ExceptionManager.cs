//Copyright (c) 2016-2021 Diego Settimi - https://github.com/arkypita/

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GPLv3 General Public License  along with this program; if not, write to the Free Software  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307,  USA. using System;

using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LaserGRBL {
  public partial class ExceptionManager :Form {
	public static GrblCore grblCore;

	public bool UserClose = false;

	private ExceptionManager() {
	  InitializeComponent();
	}

	public static void RegisterHandler() {
	  AppDomain.CurrentDomain.UnhandledException += OnUnhandledThreadException;
	  Application.ThreadException += OnUnhandledMainException;
	}

	private static void OnUnhandledMainException(object sender, ThreadExceptionEventArgs e) {
	  CreateAndShow(e?.Exception, true, false);
	}

	private static void OnUnhandledThreadException(object sender, UnhandledExceptionEventArgs e) {
	  CreateAndShow(e?.ExceptionObject as Exception, false, false);
	}

	public static void CreateAndShow(Exception ex, bool canContinue, bool manual = true) {
	  bool close;
	  using (ExceptionManager exceptionManager = new ExceptionManager()) {
		StringBuilder stringBuilder = new StringBuilder();

		try {
		  stringBuilder.AppendFormat("LaserGrbl v{0}", typeof(GitHub).Assembly.GetName().Version);
		  stringBuilder.AppendLine();
		  stringBuilder.AppendFormat("{0} v{1}", grblCore?.Type, grblCore?.Configuration?.GrblVersion);
		  stringBuilder.AppendLine();
		  stringBuilder.AppendFormat("Wrapper: {0}", Settings.GetObject("ComWrapper Protocol", ComWrapper.WrapperType.UsbSerial));
		  stringBuilder.AppendLine();
		  stringBuilder.AppendFormat("{0} ({1})", Tools.OSHelper.GetOSInfo()?.Replace("|", ", "), Tools.OSHelper.GetBitFlag());
		  stringBuilder.AppendLine();
		  stringBuilder.AppendFormat("CLR: {0}", Tools.OSHelper.GetClrInfo());
		  stringBuilder.AppendLine();
		  stringBuilder.AppendLine();
		} catch { }

		AppendExceptionData(stringBuilder, ex);

		exceptionManager.TbExMessage.Text = stringBuilder.ToString();
		exceptionManager.BtnContinue.Visible = canContinue;
		exceptionManager.ShowDialog();
		close = exceptionManager.UserClose;
	  }

	  if (close)
		Application.Exit();
	}

	private void BtnAbort_Click(object sender, EventArgs e) {
	  UserClose = true;
	  Close();
	}

	private void BtnContinue_Click(object sender, EventArgs e) {
	  Close();
	}

	private static void AppendExceptionData(StringBuilder stringBuilder, Exception e) {
	  if (e != null) {
		try {
		  stringBuilder.AppendFormat("TypeOf exception  [{0}]", e.GetType());
		  stringBuilder.AppendLine();
		  stringBuilder.AppendFormat("Exception message [{0}]", e.Message);
		  stringBuilder.AppendLine();
		  stringBuilder.AppendFormat("Exception source  [{0}], thread [{1}]", e.Source, Thread.CurrentThread.Name);
		  stringBuilder.AppendLine();
		  stringBuilder.AppendFormat("Exception method  [{0}]", e.TargetSite);
		  stringBuilder.AppendLine();
		  stringBuilder.AppendFormat("");
		  stringBuilder.AppendLine();
		} catch { }

		try {
		  if (e.StackTrace != null) {
			stringBuilder.AppendFormat("   ----------- stack trace -----------");
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat(e.StackTrace);
			stringBuilder.AppendLine();
			stringBuilder.AppendFormat("");
		  }
		} catch { }

		try {
		  stringBuilder.AppendLine();
		  if (e.InnerException != null) {
			stringBuilder.AppendFormat("Inner exception data");
			stringBuilder.AppendLine();
			AppendExceptionData(stringBuilder, e.InnerException);
			stringBuilder.AppendLine();
		  }
		} catch { }
	  }
	}

	private void LblFormDescription_LinkClicked(object sender, LinkClickedEventArgs e) { Tools.Utils.OpenLink(e.LinkText); }
  }
}