# Mac support

## DllImport prepends lib

## Release calls break

Release callbacks get wrong pointers:

- Releasing string breaks
- Release for call arguments breaks (only when invoked is called from a dotnet callback)
- Releasing callbacks use unknown pointer
