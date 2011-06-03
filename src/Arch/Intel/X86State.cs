#region License
/* 
 * Copyright (C) 1999-2011 John K�ll�n.
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

using Decompiler.Core;
using Decompiler.Core.Code;
using Decompiler.Core.Expressions;
using Decompiler.Core.Machine;
using Decompiler.Core.Rtl;
using Decompiler.Core.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Decompiler.Arch.Intel
{
	public class X86State : ProcessorState
	{
		private ulong [] regs;
		private bool [] valid;
		private Stack<Constant> stack;

        private const int StackItemSize = 2;

		public X86State()
		{
			regs = new ulong[(int)Registers.Max];
			valid = new bool[(int)Registers.Max];
			stack = new Stack<Constant>();
		}

		public X86State(X86State st)
		{
			regs = (ulong []) st.regs.Clone();
			valid = (bool []) st.valid.Clone();
            stack = new Stack<Constant>(st.stack);
            FpuStackItems = st.FpuStackItems;
		}

        public int FpuStackItems { get; set; }

		public Address AddressFromSegOffset(MachineRegister seg, uint offset)
		{
			Constant c = Get(seg);
			if (c.IsValid)
			{
				return new Address((ushort) c.ToUInt32(), offset & 0xFFFF);
			}
			else
				return null;
		}

		public Address AddressFromSegReg(MachineRegister seg, MachineRegister reg)
		{
			Constant c = Get(reg);
			if (c.IsValid)
			{
				return AddressFromSegOffset(seg, c.ToUInt32());
			}
			else 
				return null;
		}

		public ProcessorState Clone()
		{
			return new X86State(this);
		}

		public void Set(MachineRegister reg, Constant c)
		{
			if (c == null || !c.IsValid)
			{
				valid[reg.Number] = false;
			}
			else
			{
				reg.SetRegisterFileValues(regs, c.ToUInt32(), valid);	//$REVIEW: AsUint64 for x86-64?
			}
		}

		public void SetInstructionPointer(Address addr)
		{
			Set(Registers.cs, new Constant(PrimitiveType.Word16, addr.Selector));
		}

		public Constant Get(MachineRegister reg)
		{
			if (valid[reg.Number])
				return new Constant(reg.DataType, regs[reg.Number]);
			else
				return Constant.Invalid;
		}

        public void OnProcedureEntered()
        {
            FpuStackItems = 0;
            Set(Registers.D, Constant.False());
        }

        public void OnProcedureLeft(ProcedureSignature sig)
        {
            sig.FpuStackDelta = FpuStackItems;     
        }

        public CallSite OnBeforeCall(int returnAddressSize)
        {
            if (returnAddressSize > 0)
                Push(PrimitiveType.CreateWord(returnAddressSize), Constant.Invalid);
            return new CallSite(returnAddressSize, FpuStackItems);  
        }

        public void OnAfterCall(ProcedureSignature sig)
        {
            ShrinkStack(sig.StackDelta);
            ShrinkFpuStack(-sig.FpuStackDelta);
        }

        private void ShrinkStack(int shr)
        {
            while (shr > 0)
            {
                Pop(PrimitiveType.Word16);
                shr -= StackItemSize;
            }
        }

		public Constant Pop(PrimitiveType t)
		{
			try
			{
				switch (t.Size)
				{
				default:
					throw new ArgumentOutOfRangeException("t");
				case 2:
					if (stack.Count < 1)
					{
						Debug.WriteLine("Stack underflow from popping.");
						return Constant.Invalid;
					}
					return stack.Pop();
				case 4:
					if (stack.Count < 2)
					{
                        Debug.WriteLine("Stack underflow from popping.");
						return Constant.Invalid;
					}
					Constant v = stack.Pop();
					Constant v2 = stack.Pop();
					if (v.IsValid && v2.IsValid)
					{
						return new Constant(t, (v.ToUInt32() & 0x0000FFFF) | (v2.ToUInt32() << 16));
					}
					else
					{
						return Constant.Invalid;
					}
				}
			}
			catch (InvalidOperationException)
			{
				//$NYI:				Log.Warn("Stack underflow");
				return Constant.Invalid;
			}
		}

		public void Push(PrimitiveType t, Constant c)
		{
			switch (t.Size)
			{
			default:
				throw new ArgumentOutOfRangeException();
			case 1:
			case 2:
				stack.Push(c);
				break;
			case 4:
				if (!c.IsValid)
				{
					stack.Push(c);
					stack.Push(c);
				}				 
				else
				{
					//$REVIEW: we lose type information here, change stack model to sortedlist.
					//$BUG: 64-bit constants?
					stack.Push(new Constant(PrimitiveType.Word16, c.ToUInt32() >> 16));
					stack.Push(new Constant(PrimitiveType.Word16, c.ToUInt32() & 0xFFFF));
				}
				break;
			}
		}

        public bool HasSameValues(X86State st2)
        {
            for (int i = 0; i < valid.Length; ++i)
            {
                if (valid[i] != st2.valid[i])
                    return false;
                if (valid[i])
                {
                    MachineRegister reg = Registers.GetRegister(i);
                    ulong u1 = (ulong)(regs[reg.Number] & ((1UL << reg.DataType.BitSize) - 1UL));
                    ulong u2 = (ulong)(st2.regs[reg.Number] & ((1UL << reg.DataType.BitSize) - 1UL));
                    if (u1 != u2)
                        return false;
                }
            }
            return true;
        }


		public void GrowFpuStack(Address addrInstr)
		{
			++FpuStackItems;
			if (FpuStackItems > 7)
			{
				Debug.WriteLine(string.Format("Possible FPU stack overflow at address {0}", addrInstr));	//$BUGBUG: should be an exception
			}
		}

        public void ShrinkFpuStack(int cItems)
        {
            FpuStackItems -= cItems;
        }
    }
}