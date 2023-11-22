using Tpm2Lib;

namespace TPM_Tests
{
    [TestClass]
    public class SimulatorTests
    {
        [TestMethod, Timeout(10000)]
        public void MultiThreaded()
        {
            // Only a few tasks will be completed, before the others fail because of an invalid TPM2 state (?)
            List<Task> tasks = [];
            int completed = 0;
            for (int i = 0; i < 10; i++)
                tasks.Add(Task.Run(() =>
                {
                    // Use a new TPM2 instance for each thread
                    using TcpTpmDevice ttd = new("127.0.0.1", 2321);
                    ttd.Connect();
                    using Tpm2 tpm = new(ttd);
                    ttd.PowerCycle();
                    tpm.Startup(Su.Clear);
                    // Create a HMAC-SHA-384
                    TpmHandle handle = tpm.HashSequenceStart([], TpmAlgId.Sha384);
                    AuthSession session = tpm.StartAuthSessionEx(TpmSe.Hmac, TpmAlgId.Sha384);
                    try
                    {
                        tpm[session].SequenceUpdate(handle, [1, 2, 3]);
                        Assert.AreEqual(48, tpm[session].SequenceComplete(handle, [], TpmHandle.RhOwner, out _).Length);
                        Interlocked.Increment(ref completed);
                    }
                    finally
                    {
                        tpm.FlushContext(session);
                    }
                }));
            try
            {
                Task.WaitAll([.. tasks]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Completed threads: {completed}");
                Assert.Fail(ex.ToString());
            }
            Assert.AreEqual(10, completed);
        }

        [TestMethod, Timeout(10000)]
        public void SingleThreaded()
        {
            // This actually runs without any problems - but may fail, if the multithreaded tests fail before (connection to the simulator fails)
            using TcpTpmDevice ttd = new("127.0.0.1", 2321);
            ttd.Connect();
            using Tpm2 tpm = new(ttd);
            ttd.PowerCycle();
            tpm.Startup(Su.Clear);
            TpmHandle handle = tpm.HashSequenceStart([], TpmAlgId.Sha384);
            AuthSession session = tpm.StartAuthSessionEx(TpmSe.Hmac, TpmAlgId.Sha384);
            try
            {
                tpm[session].SequenceUpdate(handle, [1, 2, 3]);
                Assert.AreEqual(48, tpm[session].SequenceComplete(handle, [], TpmHandle.RhOwner, out _).Length);
            }
            finally
            {
                tpm.FlushContext(session);
            }
        }
    }
}
