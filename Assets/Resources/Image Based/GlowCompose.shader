// --------------------------------------------------------
// The final composition shader:
//   adds (glow color * glow alpha * amount) to the original image.
// In the combiner glow amount can be only in 0..1 range; we apply extra
// amount during the blurring phase.

Shader "GlowCompose" {
	Properties {
		_Color (""Glow Amount"", Color) = (1,1,1,1)
		_MainTex ("""", RECT) = ""white"" {}
	}
	SubShader {
		Pass {
			ZTest Always Cull Off ZWrite Off Fog { Mode Off }
			Blend One One
			SetTexture [_MainTex] {constantColor [_Color] combine constant * texture DOUBLE}
		}
	}
	Fallback Off
}