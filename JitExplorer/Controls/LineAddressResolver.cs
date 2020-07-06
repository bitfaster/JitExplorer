using System;
using System.Collections.Generic;
using System.Text;

namespace JitExplorer.Controls
{
    public class LineAddressResolver
    {

        public string GetAddress(int line)
        {
            var linestr = line.ToString();

            var addr = "7FFED9580410";

            return addr.Substring(0, addr.Length - linestr.Length) + linestr;

            //return "7FFED9580410" + line.ToString();
        }
    }
}
