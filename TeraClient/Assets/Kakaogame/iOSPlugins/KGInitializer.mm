#import <KakaoGame/KakaoGame.h>
#import "KGUnity.h"

/* SDK initialization with RAII */
class KGInitializer {
public:
	KGInitializer(){
		printf("Initialize KakaoGame.SDK\n");

		[KGSession initializeSDKWithAppDelegateClassName: @"UnityAppController"];
	}
} __kakaogame_initializer__;