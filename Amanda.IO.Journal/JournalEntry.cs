using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amanda.IO.Journal
{
    public class JournalEntry<TRecordType>
    {
        public JournalAction Action { get; set; }
        public List<TRecordType> Records { get; set; }
    }
}