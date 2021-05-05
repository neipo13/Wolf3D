struct v2f {
    float4 pos : SV_POSITION;
    float2 uv  : TEXCOORD0;
};

sampler2D _MainTex;
sampler2D _CharTex;
float _tilesX;
float _tilesY;
float _tilesW;
float _tilesH;
float _brightness;
int screenHeight;
int screenWidth;
int _charCount;
 
float4 frag(v2f i) : COLOR{
    float2 newCoord = float2(saturate(floor(_tilesX * i.uv.x) / (_tilesX)), saturate(floor(_tilesY * i.uv.y) / (_tilesY)));
    float4 col = tex2D(_MainTex,newCoord);
    float gray = saturate((col.r + col.g + col.b)/3.0f);
    int charIndex = round(gray * (_charCount-1));
    float2 charCoord =float2(((screenWidth * i.uv.x) % _tilesW + (_tilesW-1)*charIndex)/ ((_tilesW - 1)* _charCount), saturate(((int)(screenHeight * i.uv.y) % _tilesH) / (_tilesH-1)));
    float4 charCol = tex2D(_CharTex, charCoord);

    if (charCol.r > .8f) {
            return col;
    }
    else {
        return col * _brightness;
    }					
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 frag();
    }
}