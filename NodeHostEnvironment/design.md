# NodeHostEnvironment Design

```plantuml
hide empty member

class NodeHost

package BridgeApi {
    interface IBridgeToNode
}

package InProcess {
    class NodeBridge
    class JsDynamicObject
    interface IHostInProcess
    class JsValue
    class DotNetValue
    enum DotNetType
    enum JsType
}

package NativeHost {
    class InProcessNativeHost
    class InProcessNativeHost
    class DynamicLibraryLoader
    class NodeTaskScheduler
}

NodeHost .l.|> IBridgeToNode
NodeHost o-l-> IBridgeToNode
NodeHost --> NodeBridge

NodeBridge .u.|> IBridgeToNode
NodeBridge -l-> JsDynamicObject
NodeBridge --> IHostInProcess

JsDynamicObject --> IHostInProcess

IHostInProcess --> JsValue
IHostInProcess --> DotNetValue

JsValue o--> JsType
DotNetValue o--> DotNetType

InProcessNativeHost .l.|> IHostInProcess
NodeHost --> InProcessNativeHost
InProcessNativeHost ..> DynamicLibraryLoader
InProcessNativeHost o--> NodeTaskScheduler

```
