# MS.TSS .NET multithreading problem

The code which runs without any error on a single thread won't run multithreaded, even when using a new device/TPM2 instance for each thread.

With a real TPM it seems something between 5-6 threads can finish with success, but the rest of the 10 threads will fail, finally. Using the simulator, the test timeout of 10 seconds will interrupt the multithreaded test - probably a dead-lock somewhere?

The simulator logs an error code:

```
Receive error.  Error is 0x2746
```

The problems can be reproduced using .NET 6 and 8 (I didn't test 7).
