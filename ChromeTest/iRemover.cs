using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitRemover.Logics.Remover
{
    public interface iRemover
    {
        string DefaultProfile { get; set; }
        string GetDefaultProfile();
        void StartCleaning();
        void RemoveExtension();
        void RemoveSearchEngine();
        void CleanSettings();
    }
}
