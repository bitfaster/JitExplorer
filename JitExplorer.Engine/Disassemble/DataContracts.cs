using Iced.Intel;
using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace JitExplorer.Engine.Disassemble
{
    public abstract class SourceCode
    {
        public ulong InstructionPointer { get; set; }
    }

    public class Sharp : SourceCode
    {
        public string Text { get; set; }
        public string FilePath { get; set; }
        public int LineNumber { get; set; }
    }

    public class Asm : SourceCode
    {
        public Instruction Instruction { get; set; }
    }

    public class MonoCode : SourceCode
    {
        public string Text { get; set; }
    }

    public class Map
    {
        [XmlArray("Instructions")]
        [XmlArrayItem(nameof(SourceCode), typeof(SourceCode))]
        [XmlArrayItem(nameof(Sharp), typeof(Sharp))]
        [XmlArrayItem(nameof(Asm), typeof(Asm))]
        public SourceCode[] SourceCodes { get; set; }
    }

    public class DisassembledMethod
    {
        public string Name { get; set; }

        public ulong NativeCode { get; set; }

        public string Problem { get; set; }

        public Map[] Maps { get; set; }

        public string CommandLine { get; set; }

        public static DisassembledMethod Empty(string fullSignature, ulong nativeCode, string problem)
            => new DisassembledMethod
            {
                Name = fullSignature,
                NativeCode = nativeCode,
                Maps = Array.Empty<Map>(),
                Problem = problem
            };
    }

    public class DisassemblyResult
    {
        public DisassembledMethod[] Methods { get; set; }
        public string[] Errors { get; set; }
        public MutablePair[] SerializedAddressToNameMapping { get; set; }
        public uint PointerSize { get; set; }

        [XmlIgnore] // XmlSerializer does not support dictionaries ;)
        public Dictionary<ulong, string> AddressToNameMapping
            => _addressToNameMapping ?? (_addressToNameMapping = SerializedAddressToNameMapping.ToDictionary(x => x.Key, x => x.Value));

        [XmlIgnore]
        private Dictionary<ulong, string> _addressToNameMapping;

        public DisassemblyResult()
        {
            Methods = Array.Empty<DisassembledMethod>();
            Errors = Array.Empty<string>();
        }

        // KeyValuePair is not serializable, because it has read-only properties
        // so we need to define our own...
        [Serializable]
        [XmlType(TypeName = "Workaround")]
        public struct MutablePair
        {
            public ulong Key { get; set; }
            public string Value { get; set; }
        }
    }

    public static class DisassemblerConstants
    {
        public const string DisassemblerEntryMethodName = "__Jit";
    }

    public class Settings
    {
        public Settings(int processId, string typeName, string methodName, bool printSource, int maxDepth, string resultsPath, IEnumerable<string> filterMethods, SourceCodeProvider sourceCodeProvider)
        {
            ProcessId = processId;
            TypeName = typeName;
            MethodName = methodName;
            PrintSource = printSource;
            MaxDepth = methodName == DisassemblerConstants.DisassemblerEntryMethodName && maxDepth != int.MaxValue ? maxDepth + 1 : maxDepth;
            ResultsPath = resultsPath;
            FilterMethods = filterMethods;
            this.SourceCodeProvider = sourceCodeProvider;
        }

        internal int ProcessId { get; }
        internal string TypeName { get; }
        internal string MethodName { get; }
        internal bool PrintSource { get; }
        internal int MaxDepth { get; }
        internal string ResultsPath { get; }

        public IEnumerable<string> FilterMethods { get; }

        public SourceCodeProvider SourceCodeProvider { get; }
    }

    internal class State
    {
        internal State(ClrRuntime runtime)
        {
            Runtime = runtime;
            Todo = new Queue<MethodInfo>();
            HandledMethods = new HashSet<ClrMethod>(new ClrMethodComparer());
            AddressToNameMapping = new Dictionary<ulong, string>();
        }

        internal ClrRuntime Runtime { get; }
        internal Queue<MethodInfo> Todo { get; }
        internal HashSet<ClrMethod> HandledMethods { get; }
        internal Dictionary<ulong, string> AddressToNameMapping { get; }

        private sealed class ClrMethodComparer : IEqualityComparer<ClrMethod>
        {
            public bool Equals(ClrMethod x, ClrMethod y) => x.NativeCode == y.NativeCode;

            public int GetHashCode(ClrMethod obj) => (int)obj.NativeCode;
        }
    }

    internal readonly struct MethodInfo // I am not using ValueTuple here (would be perfect) to keep the number of dependencies as low as possible
    {
        internal ClrMethod Method { get; }
        internal int Depth { get; }

        internal MethodInfo(ClrMethod method, int depth)
        {
            Method = method;
            Depth = depth;
        }
    }
}
