# SharpReg
SharpReg is a simple code set to interact with the Remote Registry service API using the same SMB process as reg.exe, which uses TCP port 445. This code is compatible with Cobalt Strike.

```
C:>SharpReg.exe

[-] Usage:
        --Query <Computer|local|hostname|ip> <KeyName|SOFTWARE\Microsoft\Policies> <ValueName|all|ScriptBlockLogging>

        --Add <Computer|local|hostname|ip> <KeyName|SOFTWARE\Microsoft\Policies> <DataType|SZ|DWORD|BINARY> <ValueName|YourValueName> <ValueData|YourValueData>

        --Delete <Computer|local|hostname|ip> <KeyName|SOFTWARE\Microsoft\Policies> <ValueName|all|ScriptBlockLogging>
```
