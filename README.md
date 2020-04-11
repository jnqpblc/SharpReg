# SharpReg
SharpReg is a simple code set to interact with the Remote Registry service API using the same SMB process as reg.exe, which uses TCP port 445. This code is compatible with Cobalt Strike.

```
C:>SharpReg.exe

[-] Usage:
        --Query <Computer|local|hostname|ip> <KeyName|SOFTWARE\Microsoft\Policies> <ValueName|count|all|recurse|grep|ScriptBlockLogging> <grep search term>

        --Add <Computer|local|hostname|ip> <KeyName|SOFTWARE\Microsoft\Policies> <DataType|SZ|EXPAND_SZ|DWORD|QWORD|BINARY> <ValueName|YourValueName> <ValueData|YourValueData>

        --Delete <Computer|local|hostname|ip> <KeyName|SOFTWARE\Microsoft\Policies> <ValueName|all|ScriptBlockLogging>

```

## Examples

Using the all function to walk each key -- its like a simple dir of each key.
```
Z:\jnqpblc\SharpReg\SharpReg\bin\Debug>SharpReg.exe --Query local \ all


\\BCD00000000
\\HARDWARE
\\SAM
\\SECURITY
\\SOFTWARE
\\SYSTEM

Z:\jnqpblc\SharpReg\SharpReg\bin\Debug>SharpReg.exe --Query local SYSTEM all


SYSTEM\ActivationBroker
SYSTEM\ControlSet001
SYSTEM\DriverDatabase
SYSTEM\HardwareConfig
SYSTEM\Input
SYSTEM\Keyboard Layout
SYSTEM\Maps
SYSTEM\MountedDevices
SYSTEM\ResourceManager
SYSTEM\ResourcePolicyStore
SYSTEM\RNG
SYSTEM\Select
SYSTEM\Setup
SYSTEM\Software
SYSTEM\WPA
SYSTEM\CurrentControlSet

Z:\jnqpblc\SharpReg\SharpReg\bin\Debug>SharpReg.exe --Query local SYSTEM\CurrentControlSet all


SYSTEM\CurrentControlSet\Control
SYSTEM\CurrentControlSet\Enum
SYSTEM\CurrentControlSet\Hardware Profiles
SYSTEM\CurrentControlSet\Policies
SYSTEM\CurrentControlSet\Services
SYSTEM\CurrentControlSet\Software
```

Counting the subkey names -- fails on keys with a space, atm
```
Z:\jnqpblc\SharpReg\SharpReg\bin\Debug>SharpReg.exe --Query local SYSTEM\CurrentControlSet\Services count

There are 636 subkeys under HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services.

 [!] IOException: The specified registry key does not exist.
```

Recursively searching keys and values for a specific search term
```
Z:\jnqpblc\SharpReg\SharpReg\bin\Debug>SharpReg.exe --Query local SYSTEM\CurrentControlSet\Services grep Google


SYSTEM\CurrentControlSet\Services\GoogleChromeElevationService
SYSTEM\CurrentControlSet\Services\GoogleChromeElevationService

    ImagePath    REG_EXPANDSTRING    "C:\Program Files\Google\Chrome\Application\80.0.3987.163\elevation_service.exe"
SYSTEM\CurrentControlSet\Services\GoogleChromeElevationService

    DisplayName    REG_STRING    Google Chrome Elevation Service
SYSTEM\CurrentControlSet\Services\gupdate

    ImagePath    REG_EXPANDSTRING    "C:\Program Files\Google\Update\GoogleUpdate.exe" /svc
SYSTEM\CurrentControlSet\Services\gupdate

    DisplayName    REG_STRING    Google Update Service (gupdate)
SYSTEM\CurrentControlSet\Services\gupdate

    Description    REG_STRING    Keeps your Google software up to date. If this service is disabled or stopped, your Google software will not be kept up to date, meaning security vulnerabilities that may arise cannot be fixed and features may not work. This service uninstalls itself when there is no Google software using it.
SYSTEM\CurrentControlSet\Services\gupdatem

    ImagePath    REG_EXPANDSTRING    "C:\Program Files\Google\Update\GoogleUpdate.exe" /medsvc
SYSTEM\CurrentControlSet\Services\gupdatem

    DisplayName    REG_STRING    Google Update Service (gupdatem)
SYSTEM\CurrentControlSet\Services\gupdatem

    Description    REG_STRING    Keeps your Google software up to date. If this service is disabled or stopped, your Google software will not be kept up to date, meaning security vulnerabilities that may arise cannot be fixed and features may not work. This service uninstalls itself when there is no Google software using it.
```

Finding a specific binary on disk and getting the target value.
```
Z:\jnqpblc\SharpReg\SharpReg\bin\Debug>dir /s/b/a C:\Windows\calc.exe
C:\Windows\System32\calc.exe
C:\Windows\WinSxS\x86_microsoft-windows-calc_31bf3856ad364e35_10.0.14393.0_none_7b13d13279112b2e\calc.exe

Z:\jnqpblc\SharpReg\SharpReg\bin\Debug>SharpReg.exe --Query local SYSTEM\CurrentControlSet\Services\gupdatem ImagePath

    ImagePath    REG_EXPANDSTRING    "C:\Program Files\Google\Update\GoogleUpdate.exe" /medsvc
```

Overwriting the ImagePath of a service.
```
Z:\jnqpblc\SharpReg\SharpReg\bin\Debug>SharpReg.exe --Add local SYSTEM\CurrentControlSet\Services\gupdatem EXPAND_SZ ImagePath "C:\Windows\WinSxS\x86_microsoft-windows-calc_31bf3856ad364e35_10.0.14393.0_none_7b13d13279112b2e\calc.exe"

The add opetation of SYSTEM\CurrentControlSet\Services\gupdatem was successful.

Z:\jnqpblc\SharpReg\SharpReg\bin\Debug>SharpReg.exe --Query local SYSTEM\CurrentControlSet\Services\gupdatem ImagePath

    ImagePath    REG_EXPANDSTRING    C:\Windows\WinSxS\x86_microsoft-windows-calc_31bf3856ad364e35_10.0.14393.0_none_7b13d13279112b2e\calc.exe
```

Restoring the previous value.
```
Z:\jnqpblc\SharpReg\SharpReg\bin\Debug>SharpReg.exe --Add local SYSTEM\CurrentControlSet\Services\gupdatem EXPAND_SZ ImagePath "\"C:\Program Files\Google\Update\GoogleUpdate.exe\" /medsvc"

The add opetation of SYSTEM\CurrentControlSet\Services\gupdatem was successful.

Z:\jnqpblc\SharpReg\SharpReg\bin\Debug>SharpReg.exe --Query local SYSTEM\CurrentControlSet\Services\gupdatem ImagePath

    ImagePath    REG_EXPANDSTRING    "C:\Program Files\Google\Update\GoogleUpdate.exe" /medsvc

```
