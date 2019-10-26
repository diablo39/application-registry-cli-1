using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationRegistry.Collector.Batches
{
    class ResultToFileSaveBatch : IBatch
    {
        public BatchResult Process(BatchContext context)
        {
            return BatchResult.CreateSuccessResult();
        }
    }
}
