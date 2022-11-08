# Limitations

## graphics card processing time
When generating big textures with the texture resampler it is required to increase the "TdrDdiDelay" and "TdrDelay" register entries in Computer\HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\GraphicsDrivers</BR>
The values in these entries must be higher then the amount of seconds to generate all the textures, otherwise the program will be terminated by windows.
You can use the script [TdrDelay_registerFix.reg](TdrDelay_registerFix.reg) to change these values.

## ram memory usage
with 8gb of vram:
<ul>
  <li>60 textures of 4096x4096 can be held in dedicated memory (tested with the bussines card holder scene)</li>
  <li>Producing more textures wil result in a sharp decrease in frame rate as shared memory is used.</li>
  <li>Going over 160 of these textures might crash the entire program, as the shared memory is filled.</li>
</ul>