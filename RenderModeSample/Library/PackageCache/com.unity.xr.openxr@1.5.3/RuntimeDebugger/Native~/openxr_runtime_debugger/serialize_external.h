#pragma once

#if XR_USE_GRAPHICS_API_D3D11
//struct ID3D11Texture2D;
template <>
void SendToCSharp<>(const char* fieldname, LUID t)
{
    StartStruct(fieldname, "LUID");
    char buf[32];
    snprintf(buf, 32, "0x%x", t.LowPart);
    SendToCSharp("LowPart", buf);
    snprintf(buf, 32, "0x%x", t.HighPart);
    SendToCSharp("HighPart", buf);
    EndStruct();
}

template <>
void SendToCSharp<>(const char* fieldname, D3D_FEATURE_LEVEL t)
{
    char buf[32];
    snprintf(buf, 32, "0x%x", t);
    SendToCSharp(fieldname, buf);
}
#endif