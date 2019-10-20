using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationRegistry.Collector
{
    class ProcessParameters
    {

    }

    class BatchContext
    {

    }

    interface IBatch
    {
        void Process(BatchContext context);
    }
}
