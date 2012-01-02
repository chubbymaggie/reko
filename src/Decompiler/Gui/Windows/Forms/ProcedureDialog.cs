#region License
/* 
 * Copyright (C) 1999-2012 John K�ll�n.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; see the file COPYING.  If not, write to
 * the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Decompiler.Gui.Windows.Forms
{
    public partial class ProcedureDialog : Form
    {
        public ProcedureDialog()
        {
            InitializeComponent();
        }

        public TextBox ProcedureName
        {
            get { return txtName; }
        }

        public TextBox Signature
        {
            get { return txtSignature; }
        }


        public ListView ArgumentList
        {
            get { return listArguments; }
        }

        public PropertyGrid ArgumentProperties
        {
            get { return propArgument; }
        }

        public TabControl TabControl
        {
            get { return tabControl1; }
        }

        public Button OkButton { get { return btnOK; } }


        private void txtComment_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }


    }
}