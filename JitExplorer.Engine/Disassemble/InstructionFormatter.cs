using Iced.Intel;
using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Engine.Disassemble
{
    public static class InstructionFormatter
    {
        internal static string Format(Instruction instruction, Formatter formatter, bool printInstructionAddresses, uint pointerSize)
        {
            var output = new StringBuilderFormatterOutput();

            if (printInstructionAddresses)
            {
                FormatInstructionPointer(instruction, formatter, pointerSize, output);
            }

            formatter.Format(instruction, output);

            return output.ToString();
        }

        private static void FormatInstructionPointer(Instruction instruction, Formatter formatter, uint pointerSize, StringBuilderFormatterOutput output)
        {
            output.Write(FormatAddress(instruction, formatter, pointerSize), FormatterOutputTextKind.Text);
            output.Write(" ", FormatterOutputTextKind.Text);
        }

        public static string FormatAddress(Instruction instruction, Formatter formatter, uint pointerSize)
        {
            string ipFormat = formatter.Options.LeadingZeroes
                ? pointerSize == 4 ? "X8" : "X16"
                : "X";

            return instruction.IP.ToString(ipFormat);
        }
    }
}
