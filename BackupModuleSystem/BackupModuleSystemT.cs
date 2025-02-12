using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupModuleSystem
{
    public class BackupModuleSystemT
    {
        public int id { get; set; }
        public DateTime time { get; set; }
        public string name { get; set; }

        public override string ToString()
        {
            return name;
        }
    }
}
