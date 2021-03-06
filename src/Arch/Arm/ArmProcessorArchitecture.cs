﻿#region License
/* 
 * Copyright (C) 1999-2015 John Källén.
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

using Reko.Core;
using Reko.Core.Expressions;
using Reko.Core.Types;
using Reko.Core.Machine;
using Reko.Core.Rtl;
using Reko.Core.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reko.Core.Serialization;
using System.Globalization;
using Reko.Core.Operators;

namespace Reko.Arch.Arm
{
    public class ArmProcessorArchitecture : IProcessorArchitecture
    {
        public ArmProcessorArchitecture()
        {
        }

        #region IProcessorArchitecture Members

        public IEnumerable<MachineInstruction> CreateDisassembler(ImageReader imageReader)
        {
            return new ArmDisassembler(this, imageReader);
            //return new ArmDisassembler2(this, imageReader);
        }

        public ProcessorState CreateProcessorState()
        {
            return new ArmProcessorState(this);
        }

        public BitSet CreateRegisterBitset()
        {
            return new BitSet(16);
        }

        public IEnumerable<RtlInstructionCluster> CreateRewriter(ImageReader rdr, ProcessorState state, Frame frame, IRewriterHost host)
        {
            return new ArmRewriter(this, rdr, (ArmProcessorState)state, frame, host);
        }

        public IEnumerable<Address> CreatePointerScanner(ImageMap map, ImageReader rdr, IEnumerable<Address> knownAddresses, PointerScannerFlags flags)
        {
            var knownLinAddresses = knownAddresses.Select(a => a.ToUInt32()).ToHashSet();
            if (flags != PointerScannerFlags.Calls)
                throw new NotImplementedException(string.Format("Haven't implemented support for scanning for {0} yet.", flags));
            while (rdr.IsValid)
            {
                uint linAddrCall =  rdr.Address.ToUInt32();
                var opcode = rdr.ReadLeUInt32();
                if ((opcode & 0x0F000000) == 0x0B000000)         // BL
                {
                    int offset = ((int)opcode << 8) >> 6;
                    uint target = (uint)(linAddrCall + 8 + offset);
                    if (knownLinAddresses.Contains(target))
                        yield return Address.Ptr32(linAddrCall);
                }
            }
        }


        public Frame CreateFrame()
        {
            return new Frame(FramePointerType);
        }

        public ImageReader CreateImageReader(LoadedImage image, Address addr)
        {
            return new LeImageReader(image, addr);
        }

        public ImageReader CreateImageReader(LoadedImage image, ulong offset)
        {
            return new LeImageReader(image, offset);
        }

        public ProcedureSerializer CreateProcedureSerializer(ISerializedTypeVisitor<DataType> typeLoader, string defaultCc)
        {
            return new ArmProcedureSerializer(this, typeLoader, defaultCc);
        }

        public RegisterStorage GetRegister(int i)
        {
            return A32Registers.GpRegs[i];
        }

        public RegisterStorage GetRegister(string name)
        {
            if (name == null) throw new ArgumentNullException("name");
            RegisterStorage reg;
            if (!A32Registers.RegistersByName.TryGetValue(name, out reg))
                reg = null;
            return reg;
        }

        public Address MakeAddressFromConstant(Constant c)
        {
            return Address.Ptr32(c.ToUInt32());
        }

        public bool TryGetRegister(string name, out RegisterStorage reg)
        {
            throw new NotImplementedException();
        }

        public FlagGroupStorage GetFlagGroup(uint grf)
        {
            throw new NotImplementedException();
        }

        public FlagGroupStorage GetFlagGroup(string name)
        {
            throw new NotImplementedException();
        }

        public Expression CreateStackAccess(Frame frame, int cbOffset, DataType dataType)
        {
            return new MemoryAccess(new BinaryExpression(
                         Operator.IAdd, FramePointerType,
                         frame.EnsureRegister(StackRegister), Constant.Word32(cbOffset)),
                         dataType);
        }

        public Address ReadCodeAddress(int size, ImageReader rdr, ProcessorState state)
        {
            throw new NotImplementedException();
        }

        public int InstructionBitSize { get { return 32; } }

        public string GrfToString(uint grf)
        {
            StringBuilder s = new StringBuilder();
            if ((grf & (uint)FlagM.NF) != 0) s.Append('N');
            if ((grf & (uint)FlagM.ZF) != 0) s.Append('Z');
            if ((grf & (uint)FlagM.CF) != 0) s.Append('C');
            if ((grf & (uint)FlagM.VF) != 0) s.Append('V');
            return s.ToString();
        }

        public PrimitiveType FramePointerType
        {
            get { return StackRegister.DataType; }
        }

        public PrimitiveType PointerType
        {
            get { return PrimitiveType.Pointer32; }
        }

        public PrimitiveType WordWidth
        {
            get { return PrimitiveType.Word32; }
        }

        public RegisterStorage StackRegister
        {
            get { return A32Registers.sp; }
        }

        public uint CarryFlagMask
        {
            get { throw new NotImplementedException(); }
        }

        public bool TryParseAddress(string txtAddress, out Address addr)
        {
            return Address.TryParse32(txtAddress, out addr);
        }
        #endregion
    }

}