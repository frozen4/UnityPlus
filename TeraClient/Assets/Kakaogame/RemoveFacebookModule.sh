# Copy android appcompat library to KakaoGameSDK.libs
cp ../FacebookSDK/Plugins/Android/libs/appcompat-v7-23.4.0.aar Kakaogame/AndroidPlugins/KakaoGameSDK.libs/

# Remove FacebookSDK
rm -rf ../FacebookSDK
rm -rf ./AndroidPlugins/KakaoGameSDK.libs/KakaoGameSDK_IDP_Facebook.jar
