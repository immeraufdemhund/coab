using System.Diagnostics;
using GoldBox.Classes;

namespace GoldBox.Engine
{
    [DebuggerDisplay("{Name} size: {Size.ToString(\"X2\")}")]
    public class CmdItem
    {
        public delegate void CmdDelegate();

        public int Size { get; }
        public string Name { get; }
        CmdDelegate _cmd;

        public CmdItem(int size, string name, CmdDelegate cmd)
        {
            Size = size;
            Name = name;
            _cmd = cmd;
        }

        public void Run()
        {
            _cmd();
        }

        internal void Skip()
        {
            VmLog.WriteLine("SKIPPING: {0}", Name);

            if (Size == 0)
                gbl.ecl_offset += 1;
            else
                ovr008.vm_LoadCmdSets(Size);
        }
    }
}
