﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Panoramas.Logger;

namespace TransformatorExample {
  public partial class BaseForm : Form {
    public void LogTime(String operation_info, Procedure proc) {
      this.Cursor = Cursors.WaitCursor;
      Logger.LogTime(operation_info, proc);
      this.Cursor = Cursors.Default;
    }
  }
}
