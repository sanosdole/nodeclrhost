namespace NodeHostEnvironment
{
    public enum DotNetType
    {
        Undefined,
        Null,
        Boolean,
        Int32,
        Int64,
        Double,
        String, // Do we need encodings or do we assume UTF8?
        JsHandle, // A handle that was received from node
        Function, 
    }
}