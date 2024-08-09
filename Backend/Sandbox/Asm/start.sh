#!/bin/sh

touch /tmp/asm ; tail -n+1 -f /tmp/asm & dotnet Sharp.Backend.Sandbox.Asm.dll > /dev/null 2>&1
