namespace JitExplorer
{
    public static class Signal
    {
        public static void __Jit()
        {
            using (var sPipe = new System.IO.Pipes.NamedPipeServerStream("MyTest.Pipe", System.IO.Pipes.PipeDirection.InOut))
            {
                sPipe.WaitForConnection();
                sPipe.ReadByte(); // wait for signal that code is dissassembled
            }
        }
    }
}