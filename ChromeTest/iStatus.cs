using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitRemover.Logics
{
    public interface iStatus
    {
        string Status { get; set; }
        int MaxWork { get; set; }
        int CurrentWorkIndex { get; set; }
    }
}
