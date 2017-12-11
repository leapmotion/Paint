Shader "Portal/PortalSurfaceGrid"
{
	Properties
	{
    _Color ("Color", Color) = (1, 1, 1, 1)
    _GridSizeAndRowColCount ("Grid Size And Row Col Count", Vector) = (1, 1, 3, 3)
    _Offset ("Offset 2D", Vector) = (0, 0, 0, 0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
      #include "Assets/AppModules/Material Library/Resources/HandData.cginc"

      // Vert / Frag Structs

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
        float id : TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
        //uint id : TEXCOORD1; // ID debug
			};

      // Material Properties
			float4 _Color;
      float4 _GridSizeAndRowColCount;
      float4 _Offset;

			v2f vert (appdata v)
			{
        uint numRows = (uint)(_GridSizeAndRowColCount.z + 0.1),
             numCols = (uint)(_GridSizeAndRowColCount.w + 0.1);

        float gridW = _GridSizeAndRowColCount.x,
              gridH = _GridSizeAndRowColCount.y;
        float halfW = gridW / 2.0,
              halfH = gridH / 2.0;
        float cellW = gridW / numCols,
              cellH = gridH / numRows;

        // Get expected grid cell based on ID.
        // Grid placement is left-to-right, then top-to-bottom.
        uint id = (uint)(v.id.x + 0.1);
        uint gridX = id % numCols;
        uint gridY = id / numCols;

        // Get theoretical grid position to be clamped.
        float x = cellW * gridX;
        float y = cellH * gridY;

        // Remember original offset of input vertex from its grid point.
        float vertOffsetX = v.vertex.x - x;
        float vertOffsetY = v.vertex.y - (-y); // -y cheat makes X/Y modulus logic identical

        // Offset vertex position.
        x += _Offset.x;
        y += _Offset.y;
        
        {
          // Cell offset for wrapping.
          x += cellW / 2;
          y += cellH / 2;
        
          {
            // Flip handling when offsets are negative.
            int flipX = 1, flipY = 1;
            if (x < 0) {
              x = -x;
              flipX = -1;
            }
            if (y < 0) {
              y = -y;
              flipY = -1;
            }

            {
              // Apply X/Y wrapping
              x %= gridW;
              y %= gridH;
            }

            // Undo flip state, and if we were flipped, add an offset
            if (flipX < 0) {
              x -= gridW;
              x = flipX * x;
            }
            if (flipY < 0) {
              y -= gridH;
              y = flipY * y;
            }
          }
        
          // Undo cell offset.
          x -= cellW / 2;
          y -= cellH / 2;
        }

        // Convert final grid point back into final vertex position.
        v.vertex.x = x + vertOffsetX;
        v.vertex.y = (-y) + vertOffsetY; // see: -y cheat above

				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
        //o.id = id; // ID debug
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
        fixed4 color = _Color;

        // Color-based ID debug
        //if (i.id / 3 == 0) {
        //  color = fixed4(1, 0, 0, 1);
        //  
        //  if (i.id % 3 == 0) {
        //    color = fixed4(1, 0, 0, 1);
        //  }
        //  if (i.id % 3 == 1) {
        //    color = fixed4(0.66, 0, 0, 1);
        //  }
        //  if (i.id % 3 == 2) {
        //    color = fixed4(0.33, 0, 0, 1);
        //  }
        //}
        //if (i.id / 3 == 1) {
        //  color = fixed4(0, 1, 0, 1);
        //  
        //  if (i.id % 3 == 0) {
        //    color = fixed4(0, 1, 0, 1);
        //  }
        //  if (i.id % 3 == 1) {
        //    color = fixed4(0, 0.66, 0, 1);
        //  }
        //  if (i.id % 3 == 2) {
        //    color = fixed4(0, 0.33, 0, 1);
        //  }
        //}
        //if (i.id / 3 == 2) {
        //  color = fixed4(0, 0, 1, 1);
        //  
        //  if (i.id % 3 == 0) {
        //    color = fixed4(0, 0, 1, 1);
        //  }
        //  if (i.id % 3 == 1) {
        //    color = fixed4(0, 0, 0.66, 1);
        //  }
        //  if (i.id % 3 == 2) {
        //    color = fixed4(0, 0, 0.33, 1);
        //  }
        //}

				return color;
			}
			ENDCG
		}
	}
}
