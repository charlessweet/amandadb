using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amanda.Data.Structured
{
    public interface IAmandaRecord<T>
    {
        T Key { get; }
    }
}
