﻿/* 
 * Copyright (C) 1999-2010 John Källén.
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

using Decompiler.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Decompiler.Core.Services
{
    public interface DecompilerEventListener
    {
        ICodeLocation CreateAddressNavigator(Address address);
        ICodeLocation CreateProcedureNavigator(Procedure proc);
        ICodeLocation CreateBlockNavigator(Block block);
        void AddDiagnostic(ICodeLocation location, Diagnostic d);

        void ShowStatus(string caption);
        void ShowProgress(string caption, int numerator, int denominator);
    }

    public class NullDecompilerEventListener : DecompilerEventListener
    {
        private static NullDecompilerEventListener e = new NullDecompilerEventListener();

        public static DecompilerEventListener Instance { get { return e; } }

        #region DecompilerEventListener Members

        public void AddErrorDiagnostic(Address address, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void AddWarningDiagnostic(Address address, string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void AddDiagnostic(Diagnostic d)
        {
            throw new NotImplementedException();
        }

        public void ShowStatus(string caption)
        {
            throw new NotImplementedException();
        }

        public void ShowProgress(string caption, int numerator, int denominator)
        {
            throw new NotImplementedException();
        }

        public ICodeLocation CreateAddressNavigator(Address address)
        {
            return new NullCodeLocation(address.ToString());
        }

        public ICodeLocation CreateProcedureNavigator(Procedure proc)
        {
            return new NullCodeLocation(proc.Name);
        }

        public ICodeLocation CreateBlockNavigator(Block block)
        {
            return new NullCodeLocation(block.Name);
        }

        public void AddDiagnostic(ICodeLocation location, Diagnostic d)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class NullCodeLocation : ICodeLocation
    {
        public NullCodeLocation(string text)
        {
            this.Text = text;
        }

        public string Text { get; private set; }

        public void NavigateTo()
        {
        }
    }
}