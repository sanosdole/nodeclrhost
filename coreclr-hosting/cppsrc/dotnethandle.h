#ifndef __CORECLR_HOSTING_DOTNETHANDLE_H__
#define __CORECLR_HOSTING_DOTNETHANDLE_H__

#include <functional>

#include "jshandle.h"

typedef enum
{
    DotNetTypeUndefined,
    DotNetTypeNull,
    DotNetTypeBoolean,
    DotNetTypeInt32,
    DotNetTypeInt64,
    DotNetTypeDouble,
    DotNetTypeString, // Do we need encodings or do we assume UTF8?
    DotNetTypeJsHandle, // A handle that was received from node
    DotNetTypeFunction
} DotNetType;


extern "C" struct DotNetHandle
{
    DotNetType Type;
    void* Value;
    void (*ReleaseFunc)(DotNetHandle*);    

    void Release()
    {
        if (nullptr != ReleaseFunc)
            ReleaseFunc(this);        
    }

    Napi::Value ToValue(const Napi::Env& env, std::function<Napi::Function(DotNetHandle*)> function_factory)
    {
        if (Type == DotNetTypeNull)
            return env.Null();
        if (Type == DotNetTypeJsHandle)
            return reinterpret_cast<JsHandle*>(Value)->ToValue(env);
        if (Type == DotNetTypeString)
            return Napi::String::New(env, (const char*)Value); // TODO: string is copied, we could use char16_t to prevent a copy
        if (Type == DotNetTypeBoolean)
            return Napi::Boolean::New(env, Value != nullptr);
        if (Type == DotNetTypeInt32)
            return Napi::Number::New(env, (int)Value);

        if (Type == DotNetTypeFunction)
        {
            return function_factory(this);
            /*auto releaseFunc = ReleaseFunc;
            auto value = Value;
            ReleaseFunc = nullptr;

            // TODO: Check if using data instead of capture is better
            auto function = Napi::Function::New(env, [value, env](const Napi::CallbackInfo& info) {
                //context->SetThreadInstance();
                printf("Building arguments for Calling back to .NET\n");
                auto argc = info.Length();
                std::vector<JsHandle> arguments;
                for (auto c = 0; c<argc; c++)
                {
                    //arguments[c] = JsHandle::FromValue(info[c]);
                    arguments.push_back(JsHandle::FromValue(info[c]));
                    //printf("Arg %d is %d \n", c, argv[c].Type);
                }
                
                DotNetHandle resultIntern;
                void (*fptr)(int,JsHandle*, DotNetHandle&) = (void (*)(int, JsHandle*, DotNetHandle&))value;
                printf("Calling back to .NET\n");
                if (argc > 0)
                    (*fptr)(argc, arguments.data(), resultIntern);
                else
                    (*fptr)(0, nullptr, resultIntern);
                printf("Called back to .NET\n");

                return resultIntern.ToValue(env);
            });            

            auto releaseCopy = new DotNetHandle;
            releaseCopy->Type = DotNetTypeFunction;
            releaseCopy->Value = value;
            releaseCopy->ReleaseFunc = releaseFunc;

            napi_add_finalizer(env,
             function,
              (void*)releaseCopy,
               [](napi_env env, void* finalize_data, void* finalize_hintnapi_env) 
               {
                   auto toRelease = (DotNetHandle*)finalize_data;
                   toRelease->Release();
                   delete toRelease;
                                      
                },
                nullptr,
                 nullptr);
            
            return function;       */
        }
        
        // TODO: Support other types
        return Napi::Value::Value();
    }
};

#endif