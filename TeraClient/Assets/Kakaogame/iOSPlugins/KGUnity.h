#ifndef KGUnity_h
#define KGUnity_h

@interface KGUnity : NSObject
+ (void)sendMessage:(NSString*)message;
@end

@implementation KGUnity

+ (void)sendMessage:(NSString *)message {
    UnitySendMessage("KakaoGameSDK", "OnResponse", [message UTF8String]);
}

@end

#endif /* KGUnity_h */

