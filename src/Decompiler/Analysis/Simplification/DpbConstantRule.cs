/* 
 * Copyright (C) 1999-2008 John K�ll�n.
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
using Decompiler.Core.Code;
using System;

namespace Decompiler.Analysis.Simplification
{
	public class DpbConstantRule
	{
		private DepositBits dpb;
		private Constant cSrc;
		private Constant cBits;

		public bool Match(DepositBits dpb)
		{
			this.dpb = dpb;
			cSrc = dpb.Source as Constant;
			if (cSrc == null)
				return false;
			cBits = dpb.InsertedBits as Constant;
			if (cBits != null)
				return true;
			return (Convert.ToInt32(cSrc.Value) == 0 && dpb.BitPosition == 0);
		}

		public Expression Transform(Statement stm)
		{
			if (cBits != null)
			{
				int bitMask = Mask(dpb.BitCount) << dpb.BitPosition;
				int maskedVal = Convert.ToInt32(cSrc.Value) & ~bitMask;
				int newBits = Convert.ToInt32(cBits.Value) << dpb.BitPosition;
				return new Constant(cSrc.DataType, maskedVal | (newBits & bitMask));
			}
			else if (dpb.BitPosition == 0 && Convert.ToInt32(cSrc.Value) == 0)
			{
				return new Cast(dpb.Source.DataType, dpb.InsertedBits);
			}
			else
				return dpb;
		}

		public int Mask(int bitCount)
		{
			return ((1 << bitCount) - 1);
		}
	}
}
