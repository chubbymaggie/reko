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

using Decompiler.Core.Code;
using Decompiler.Core.Types;
using System;

namespace Decompiler.Core.Operators
{
	public abstract class SignedIntOperator : ConditionalOperator
	{
	}

	public class LtOperator : SignedIntOperator
	{
		public override Constant ApplyConstants(Constant c1, Constant c2)
		{
			return Convert.ToInt32(c1.Value) < Convert.ToInt32(c2.Value)
				? Constant.True() 
				: Constant.False();
		}

		public override string ToString()
		{
			return " < ";
		}
	}

	public class GtOperator : SignedIntOperator
	{
		public override Constant ApplyConstants(Constant c1, Constant c2)
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return " > ";
		}
	}

	public class LeOperator : SignedIntOperator
	{
		public override Constant ApplyConstants(Constant c1, Constant c2)
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return " <= ";
		}
	}

	public class GeOperator : SignedIntOperator
	{
		public override Constant ApplyConstants(Constant c1, Constant c2)
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return " >= ";
		}
	}


}