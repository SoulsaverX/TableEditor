using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableReader
{
    class tblFile
    {
        public byte[] Header = new byte[0x20];
        public int Header_Count;
        public int Rows_Count;
        public List<int> HeadresTypes = new List<int>();
        public List<byte[]> HeadresNames = new List<byte[]>();
        public List<byte[]> HeadresNamesSize = new List<byte[]>();
        //public List<byte[]> RowSize = new List<byte[]>();
        public List<byte[]> RowValue = new List<byte[]>();

    }
}
