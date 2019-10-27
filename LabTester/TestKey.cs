namespace LabTester
{
    public struct TestKey
    {
        public int ProcessCount;
        public string TaskArgs;

        public TestKey(int processCount, string taskArgs)
        {
            ProcessCount = processCount;
            TaskArgs = taskArgs;
        }
    }
}