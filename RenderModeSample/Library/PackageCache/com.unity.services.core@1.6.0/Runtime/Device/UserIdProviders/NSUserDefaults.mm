#include <string>

#ifdef __cplusplus
extern "C" {
#endif

char* UOCPUserDefaultsGetString(const char *key) {
    if (!key) {
        return nil;
    }
    NSString* stringKey = [NSString stringWithUTF8String:key];
    NSString* stringValue = [[NSUserDefaults standardUserDefaults] stringForKey:stringKey];
    if (!stringValue) {
        return nil;
    }
    return strdup([stringValue UTF8String]);
}

void UOCPUserDefaultsSetString(const char *key, const char *value) {
    if (!key) {
        return;
    }
    NSString* stringKey = [NSString stringWithUTF8String:key];
    if (!value)
    {
        [[NSUserDefaults standardUserDefaults] removeObjectForKey:stringKey];
    } else {
        NSString* stringValue = [NSString stringWithUTF8String:value];
        [[NSUserDefaults standardUserDefaults] setValue:stringValue forKey:stringKey];
    }
    [[NSUserDefaults standardUserDefaults] synchronize];
}

#ifdef __cplusplus
}
#endif
