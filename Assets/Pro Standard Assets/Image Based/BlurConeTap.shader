// --------------------------------------------------------
// The blur iteration shader.
// Basically it just takes 4 texture samples and averages them.
// By applying it repeatedly and spreading out sample locations
// we get a Gaussian blur approximation.
// The alpha value in _Color would normally be 0.25 (to average 4 samples),
// however if we have glow amount larger than 1 then we increase this.

Shader "BlurConeTap" {
	Properties { _MainTex ("""", any) = """" {} }
	SubShader {
		Pass {
			ZTest Always Cull Off ZWrite Off Fog { Mode Off }
			SetTexture [_MainTex] {constantColor (0,0,0,0.25) combine texture * constant alpha}
			SetTexture [_MainTex] {constantColor (0,0,0,0.25) combine texture * constant + previous}
			SetTexture [_MainTex] {constantColor (0,0,0,0.25) combine texture * constant + previous}
			SetTexture [_MainTex] {constantColor (0,0,0,0.25) combine texture * constant + previous}
		}
	}
	Fallback Off
}