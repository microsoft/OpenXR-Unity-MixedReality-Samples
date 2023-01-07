---
uid: arfoundation-extensions
---
# Extending AR Foundation

In many cases, AR Foundation or its subsystems wrap some platform-specific SDK, such as ARCore or ARKit. If you know you're on a particular platform, you might want to access specific features of that SDK that aren't accessible via AR Foundation.

For many objects, AR Foundation provides a native pointer to platform-specific data. For example, the `XRSessionSubsystem` has a `nativePtr` property.

Each provider package defines what each native pointer points to. In general, a pointer points to a struct whose first member is an `int` that contains a version number followed by the raw pointer. Future versions of the package might add additional fields to this struct.

In C, the `XRSessionSubsystem.nativePtr` might point to a struct like this:

```c
typedef struct UnityXRNativeSessionPtr
{
    int version;
    void* session;
} UnityXRNativeSessionPtr;
```

Structure packing and alignment rules vary by platform, so the `void* session` pointer isn't necessarily at a 4 byte offset. On a 64-bit platform, for instance, the pointer might be offset by 8 bytes to ensure the pointer is on an 8 byte boundary.

All trackables (such as planes, tracked images, or faces) provide a native pointer. You can use these pointers to access things like the native frame, session, plane, anchor, and so on.
