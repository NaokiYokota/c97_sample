#ifndef C97_PARTICLE_INCLUDED
#define C97_PARTICLE_INCLUDED

// CustomDataのプロパティ設定
#define SET_CUSTOM_DATA_PROPERTY(name) uniform int name##U; \
        uniform int name##USwizzle; \
        uniform int name##V; \
        uniform int name##VSwizzle;
    
// 該当のCustomDataを取得    
#define GET_CUSTOM_DATA(name) half2(customData[name##U][name##USwizzle],customData[name##V][name##VSwizzle])

#endif