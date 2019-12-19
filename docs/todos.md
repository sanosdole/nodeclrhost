# Open issues

## Check context aware node module initialization

Do we need a context aware native node module or is `NAPI_MODULE` registration sufficient?
See <https://nodejs.org/api/addons.html#addons_context_aware_addons> for details.

## Marshall arrays

At least support creating Array objects

## Marshall anonymous types

JS often wants JSON objects as parameters. This would be easier if anonymous types could be marshalled:

```cs
dynamic jsObject = ...;
jsObject.someFunction(new { a: 5, b: { c: "string", d: anotherJsObject }});
```

## Access require

We want to call require from .NET.

## Optimize string marshalling