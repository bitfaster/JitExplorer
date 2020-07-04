using Iced.Intel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JitExplorer.Engine.Disassemble
{
    public static class DisassemblyPrettifier
    {
        public class Element
        {
            public string TextRepresentation { get; }
            public SourceCode Source { get; }

            public Element(string textRepresentation, SourceCode source)
            {
                TextRepresentation = textRepresentation;
                Source = source;
            }
        }

        public class Reference : Element
        {
            public string Id { get; }

            public Reference(string textRepresentation, string id, SourceCode source) : base(textRepresentation, source) => Id = id;
        }

        public class Label : Element
        {
            public string Id { get; }

            public Label(string label) : base(label, null) => Id = label;
        }

        public static IReadOnlyList<Element> Prettify(DisassembledMethod method, DisassemblyResult disassemblyResult, string labelPrefix, AsmFormat asmFormat)
        {
            

            var asmInstructions = method.Maps.SelectMany(map => map.SourceCodes.OfType<Asm>()).ToArray();

            // first of all, we search of referenced addresses (jump|calls)
            var referencedAddresses = new HashSet<ulong>();
            foreach (var asm in asmInstructions)
                if (ClrMdDisassembler.TryGetReferencedAddress(asm.Instruction, disassemblyResult.PointerSize, out ulong referencedAddress))
                    referencedAddresses.Add(referencedAddress);

            // for every IP that is referenced, we emit a uinque label
            var addressesToLabels = new Dictionary<ulong, string>();
            int currentLabelIndex = 0;
            foreach (var instruction in asmInstructions)
                if (referencedAddresses.Contains(instruction.InstructionPointer) && !addressesToLabels.ContainsKey(instruction.InstructionPointer))
                    addressesToLabels.Add(instruction.InstructionPointer, $"{labelPrefix}_L{currentLabelIndex++:00}");

            var formatterWithLabelsSymbols = asmFormat.GetFormatterWithSymbolSolver(addressesToLabels);
            var formatterWithGlobalSymbols = asmFormat.GetFormatterWithSymbolSolver(disassemblyResult.AddressToNameMapping);

            var prettified = new List<Element>();
            foreach (var map in method.Maps)
                foreach (var instruction in map.SourceCodes)
                {
                    if (instruction is Sharp sharp)
                    {
                        prettified.Add(new Element(sharp.Text, sharp));
                    }
                    else if (instruction is MonoCode mono)
                    {
                        prettified.Add(new Element(mono.Text, mono));
                    }
                    else if (instruction is Asm asm)
                    {
                        // this IP is referenced by some jump|call, so we add a label
                        if (addressesToLabels.TryGetValue(asm.InstructionPointer, out string label))
                        {
                            prettified.Add(new Label(label));
                        }

                        if (ClrMdDisassembler.TryGetReferencedAddress(asm.Instruction, disassemblyResult.PointerSize, out ulong referencedAddress))
                        {
                            // jump or a call within same method
                            if (addressesToLabels.TryGetValue(referencedAddress, out string translated))
                            {
                                prettified.Add(new Reference(InstructionFormatter.Format(asm.Instruction, formatterWithLabelsSymbols, asmFormat.PrintInstructionAddresses, disassemblyResult.PointerSize), translated, asm));
                                continue;
                            }

                            // call to a known method
                            if (disassemblyResult.AddressToNameMapping.ContainsKey(referencedAddress))
                            {
                                prettified.Add(new Element(InstructionFormatter.Format(asm.Instruction, formatterWithGlobalSymbols, asmFormat.PrintInstructionAddresses, disassemblyResult.PointerSize), asm));
                                continue;
                            }
                        }

                        prettified.Add(new Element(InstructionFormatter.Format(asm.Instruction, formatterWithGlobalSymbols, asmFormat.PrintInstructionAddresses, disassemblyResult.PointerSize), asm));
                    }
                }

            return prettified;
        }

        //internal static Formatter GetFormatterWithSymbolSolver(IReadOnlyDictionary<ulong, string> addressToNameMapping)
        //{
        //    var symbolSolver = new SymbolResolver(addressToNameMapping);

        //    var formatter = new MasmFormatter();
        //    formatter.Options.FirstOperandCharIndex = 10; // pad right the mnemonic
        //    formatter.Options.HexSuffix = default; // don't print "h" at the end of every hex address
        //    formatter.Options.TabSize = 0; // use spaces
            

        //    //switch (Formatter)
        //    //{
        //    //    case MasmFormatter masmFormatter:
        //    return new MasmFormatter(formatter.MasmOptions, symbolSolver);
        //    //    case NasmFormatter nasmFormatter:
        //    //        return new NasmFormatter(nasmFormatter.NasmOptions, symbolSolver);
        //    //    case GasFormatter gasFormatter:
        //    //        return new GasFormatter(gasFormatter.GasOptions, symbolSolver);
        //    //    case IntelFormatter intelFormatter:
        //    //        return new IntelFormatter(intelFormatter.IntelOptions, symbolSolver);
        //    //    default:
        //    //        // we don't know how to translate it so we just return the original one
        //    //        // it's better not to solve symbols rather than throw exception ;)
        //    //        return Formatter;
        //    //}
        //}
    }
}
