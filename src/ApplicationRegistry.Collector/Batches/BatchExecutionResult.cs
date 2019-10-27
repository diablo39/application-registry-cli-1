namespace ApplicationRegistry.Collector
{
    struct BatchExecutionResult
    {
        public enum ExecutionResult
        {
            Success,
            Fail
        }

        public ExecutionResult Result { get; private set; }

        public static BatchExecutionResult CreateSuccessResult()
        {
            return new BatchExecutionResult { Result = ExecutionResult.Success };
        }

        public static BatchExecutionResult CreateFailResult()
        {
            return new BatchExecutionResult { Result = ExecutionResult.Fail };
        }
    }
}
