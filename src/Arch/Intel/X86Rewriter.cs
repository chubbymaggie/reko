﻿#region License
/* 
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
#endregion

using Decompiler.Core;
using Decompiler.Core.Code;
using Decompiler.Core.Lib;
using Decompiler.Core.Machine;
using Decompiler.Core.Operators;
using Decompiler.Core.Types;
using System;
using System.Collections.Generic;
using System.Text;
using IEnumerable = System.Collections.IEnumerable;
using IEnumerator = System.Collections.IEnumerator;

namespace Decompiler.Arch.Intel
{
    public partial class X86Rewriter : Rewriter, Rewriter2
    {
        private IntelArchitecture arch;
        private Frame frame;
        private LookaheadEnumerator<DisassembledInstruction> dasm;
        private Emitter emitter;
        private OperandRewriter2 orw;
        private DisassembledInstruction di;
        private IntelRewriterState state;

        [Obsolete("Phasing out old rewriter")]
        public X86Rewriter(IProcedureRewriter prw)
            : base(prw)
        {
        }

        public X86Rewriter(IntelArchitecture arch, ImageReader rdr, Frame frame)
            : base(null)
        {
            this.arch = arch;
            this.frame = frame;
            this.dasm = new LookaheadEnumerator<DisassembledInstruction>(CreateDisassemblyStream(rdr, arch.WordWidth));
        }

        protected virtual IEnumerable<DisassembledInstruction> CreateDisassemblyStream(ImageReader rdr, PrimitiveType defaultWordSize)
        {
            var d = new IntelDisassembler(rdr, defaultWordSize);
            for (; ; )        //$REVIEW: this will walk off the edge of the program image; we're assuming the caller will deal.
            {
                var addr = d.Address;
                var instr = d.Disassemble();
                var length = (uint)(d.Address - addr);
                yield return new DisassembledInstruction(addr, instr, length);
            }
        }

        #region old Rewriter overrides; delete when done.
        public override void GrowStack(int bytes)
        {
            throw new NotImplementedException();
        }

        public override void ConvertInstructions(MachineInstruction[] instrs, Address[] addrs, uint[] deadOutFlags, Address addrEnd, CodeEmitter emitter)
        {
            throw new NotImplementedException();
        }

        public override void EmitCallAndReturn(Procedure callee)
        {
            throw new NotImplementedException();
        }
        #endregion

        public IEnumerator<RewrittenInstruction> GetEnumerator()
        {
            while (dasm.MoveNext())
            {
                emitter = new Emitter(frame);
                emitter.LinearAddress = dasm.Current.Address.Linear;
                orw = new OperandRewriter2(arch, frame);
                di = dasm.Current;
                switch (di.Instruction.code)
                {
                default:
                    throw new NotImplementedException(string.Format("Intel opcode {0} not supported yet.", di.Instruction.code));
                case Opcode.add: RewriteAddSub(BinaryOperator.Add); break;
                case Opcode.and: RewriteLogical(BinaryOperator.And); break;
                case Opcode.cmp: RewriteCmp(); break;
                case Opcode.mov: RewriteMov(); break;
                case Opcode.or: RewriteLogical(BinaryOperator.Or); break;
                case Opcode.push: RewritePush(); break;
                case Opcode.pop: RewritePop(); break;
                case Opcode.sub: RewriteAddSub(BinaryOperator.Sub); break;
                case Opcode.test: RewriteTest(); break;
                case Opcode.xor: RewriteLogical(BinaryOperator.Xor); break;
                }

                foreach (RewrittenInstruction ri in emitter.Instructions)
                {
                    yield return ri;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Breaks up very common case of x86:
        /// <code>
        ///		op [memaddr], reg
        /// </code>
        /// into the equivalent:
        /// <code>
        ///		tmp := [memaddr] op reg;
        ///		store([memaddr], tmp);
        /// </code>
        /// </summary>
        /// <param name="opDst"></param>
        /// <param name="src"></param>
        /// <param name="forceBreak">if true, forcibly splits the assignments in two if the destination is a memory store.</param>
        /// <returns></returns>
        public Assignment EmitCopy(MachineOperand opDst, Expression src, bool forceBreak)
        {
            Expression dst = SrcOp(opDst);
            Identifier idDst = dst as Identifier;
            Assignment ass;
            if (idDst != null || !forceBreak)
            {
                MemoryAccess acc = dst as MemoryAccess;
                if (acc != null)
                {
                    emitter.Store(acc, src);
                    ass = null;			//$REVIEW: is this ever used?
                }
                else
                {
                    ass = emitter.Assign(idDst, src);
                }
            }
            else
            {
                Identifier tmp = frame.CreateTemporary(opDst.Width);
                ass = emitter.Assign(tmp, src);
                MemoryAccess ea = orw.CreateMemoryAccess((MemoryOperand)opDst, state);
                emitter.Emit(new Store(ea, tmp));
            }
            return ass;
        }




        private Expression SrcOp(MachineOperand opSrc)
        {
            return orw.Transform(opSrc, opSrc.Width);
        }

        private Expression SrcOp(MachineOperand opSrc, PrimitiveType dstWidth)
        {
            return orw.Transform(opSrc, dstWidth);
        }


        private class Emitter : CodeEmitter2
        {
            private Frame frame;
            private List<RewrittenInstruction> ri;

            public Emitter(Frame frame)
            {
                this.frame = frame;
                this.ri = new List<RewrittenInstruction>();
            }

            public List<RewrittenInstruction> Instructions { get { return ri; } }

            public uint LinearAddress { get; set; }

            public override Statement Emit(Instruction instr)
            {
                var i = new RewrittenInstruction(LinearAddress, instr);
                ri.Add(i);
                return null; //$REview: if you really need Statement, get Block.LastSTatement
            }

            public override Frame Frame
            {
                get { return frame; }
            }

            public override Identifier Register(int i)
            {
                throw new NotImplementedException();
            }
        }
    }
}