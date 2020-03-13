namespace NodeHostEnvironment.InProcess
{
    internal enum JsType
    {
        // napi_value_type
        Undefined,
        Null,
        Boolean,
        Number,
        String,
        Symbol,
        Object,
        Function,
        External,

        // Custom types
        Error // Not an JS error, but a value that indicates an internal error (Exception)
    }
}