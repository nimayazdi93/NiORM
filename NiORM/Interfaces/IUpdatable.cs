using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiORM.Interfaces
{
    public interface IUpdatable
    {
        DateTime CreatedDateTime { get; set; }
        DateTime UpdatedDateTime { get; set; }
    }
}
