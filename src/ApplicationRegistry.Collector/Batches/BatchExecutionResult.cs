namespace ApplicationRegistry.Collector
{
    struct BatchExecutionResult
    {
        public enum ExecutionResult
        {
            Success,
            Error,
            Fail
        }

        public ExecutionResult Result { get; private set; }

        public static BatchExecutionResult CreateSuccessResult()
        {
            return new BatchExecutionResult { Result = ExecutionResult.Success };
        }

        public static BatchExecutionResult CreateErrorResult()
        {
            return new BatchExecutionResult { Result = ExecutionResult.Error };
        }

        public static BatchExecutionResult CreateFailResult()
        {
            return new BatchExecutionResult { Result = ExecutionResult.Fail };
        }
    }
}
