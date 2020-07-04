using Iced.Intel;
using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine.Disassemble
{
    public enum AsmSyntax
    {
        Masm,
        Nasm,
        Att,
        Intel
    }

    public class AsmFormat
    {
        private readonly AsmSyntax asmSyntax;
        bool printInstructionAddresses;

        public AsmFormat(AsmSyntax asmSyntax = AsmSyntax.Masm, bool printInstructionAddresses = false)
        {
            this.asmSyntax = asmSyntax;
            this.printInstructionAddresses = printInstructionAddresses;
        }

        public bool PrintInstructionAddresses => this.printInstructionAddresses;

        public Formatter GetFormatterWithSymbolSolver(IReadOnlyDictionary<ulong, string> addressToNameMapping)
        {
            var symbolSolver = new SymbolResolver(addressToNameMapping);

            switch (this.asmSyntax)
            {
                case AsmSyntax.Masm:
                    var formatter = new MasmFormatter();
                    SetOptions(formatter);
                    return new MasmFormatter(formatter.MasmOptions, symbolSolver);
                case AsmSyntax.Nasm:
                    var nformatter = new NasmFormatter();
                    SetOptions(nformatter);
                    return new NasmFormatter(nformatter.NasmOptions, symbolSolver);
                case AsmSyntax.Att:
                    var gformatter = new GasFormatter();
                    SetOptions(gformatter);
                    return new GasFormatter(gformatter.GasOptions, symbolSolver);
                default:
                    var iformatter = new IntelFormatter();
                    SetOptions(iformatter);
                    return new IntelFormatter(iformatter.IntelOptions, symbolSolver);
            }
        }

        private static void SetOptions(Formatter formatter)
        {
            formatter.Options.FirstOperandCharIndex = 10; // pad right the mnemonic
            formatter.Options.HexSuffix = default; // don't print "h" at the end of every hex address
            formatter.Options.TabSize = 0; // use spaces
        }
    }
}
