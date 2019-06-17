namespace NodeHostEnvironment
{
    public enum JsType
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
        Error
        // TODO: What about special objects like arrays & promises
    }
}