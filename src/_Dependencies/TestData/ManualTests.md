# Manual Tests

## setup

cd x:\code\ghub\plisky.versioning\src
cd x:\code\ghub\plisky.versioning\src\versonify\bin\debug\net10.0

## Create Version



`versonify -Command=CreateVersion -VS=%TEMP%\bugrepro.store -Q="666.666.666.666" -Release=Buggy`

```
WARNING: '-command' is deprecated. Use '--command' instead.
WARNING: '-vs' is deprecated. Use '--version-source' instead.
WARNING: '-release' is deprecated. Use '--release' instead.
Performing Versioning Actions
Using Value From Command Line: 666.666.666.666
Setting Release From Command Line: Buggy
Creating New Version Store: 666.666.666.666
Saving 666.666.666.666
```

`versonify -Command=CreateVersion -v="%TEMP%\bugrepro.store" -Q="666.666.666.666" -Release=Buggy`

```
WARNING: '-command' is deprecated. Use '--command' instead.
WARNING: '-release' is deprecated. Use '--release' instead.
Performing Versioning Actions
Error >> Version store %TEMP%\bugrepro.store already exists.
Errors Occurred:
Use '--help' to display available options and commands, or '--get-md-help' to export documentation.
```

`versonify --command=CreateVersion -v="%TEMP%\bugrepro2.store" -Q="666.666.666.666" --release=Buggy`

Creates with no warnings.

`versonify --command=passive -v="%TEMP%\bugrepro2.store"`

`versonify passive -v="%TEMP%\bugrepro2.store"`

```
Performing Versioning Actions
Loaded [666.666.666.666]
666.666.666.666
```

`versonify passive -v="%TEMP%\bugrepro2.store -o=jcon`

```
versonify passive -v="%TEMP%\bugrepro2.store" -o=jcon
💖 Versioning -DEBUG- By Versonify 💖 (0.0.0.0).
{"MessageLevel":"information","MessageContent":"Performing Versioning Actions","Meta":{}}
{"MessageLevel":"information","MessageContent":"Loaded [666.666.666.666]","Meta":{}}
```

Set the new version

`versonify --command=set -v="%TEMP%\bugrepro2.store" -Q="1.2.3.4" `

```
Performing Versioning Actions
Set version to: 1.2.3.4
Saving Updated Digit Values
[1.2.3.4]
1.2.3.4
```
Change the prefix    
`versonify --command=prefix -v="%TEMP%\bugrepro2.store" -Q="A" -o=jcon -d=*`

```
Performing Versioning Actions
Setting prefix for all digits to: A
Saving updated digit prefixes
[1A2A3A4]
1A2A3A4
```
Set prefixes

```
versonify --command=prefix -v="%TEMP%\bugrepro2.store" -Q="A" -o=jcon -d=1
versonify --command=prefix -v="%TEMP%\bugrepro2.store" -Q="A" -o=jcon -d=2
```

```
Performing Versioning Actions
Setting prefix for digit(s) [1] to: .
Saving updated digit prefixes
[1.2.3A4]
1.2.3A4
```