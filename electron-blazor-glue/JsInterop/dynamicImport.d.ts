// Type declarations for the plain-JS dynamicImport helper (see dynamicImport.js).
// Keep in sync with the .js implementation. TypeScript resolves this declaration
// while webpack bundles the matching .js file.
export declare const nativeImport: (specifier: string) => Promise<any>;