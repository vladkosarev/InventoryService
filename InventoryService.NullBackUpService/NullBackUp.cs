using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryService.BackUpService;

namespace InventoryService.NullBackUpService
{
    public class NullBackUp:IBackUpService
    {
        public bool BackUp(string name, string content)
        {
            return true;
        }
    }
}
