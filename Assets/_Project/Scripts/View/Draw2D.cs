using UnityEngine;
namespace ZombieAttack
{
    // Primitives use a four-unit pixel grid, including weapons and effects.
    public static class Draw2D
    {
        const float Pixel=4;
        static Texture2D circle;
        public static readonly Color Ink=Hex("141526"), Cream=Hex("F4EDD5"), Gold=Hex("F7C65B"), Mint=Hex("A8D969");
        public static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#"+hex,out var c); return c; }
        public static float Snap(float value)=>Mathf.Round(value/Pixel)*Pixel;
        public static void Init()
        {
            if(circle!=null) return;
            circle=new Texture2D(12,12,TextureFormat.RGBA32,false) {filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            for(int y=0;y<12;y++) for(int x=0;x<12;x++) circle.SetPixel(x,y,new Color(1,1,1,Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(6,6))<6?1:0));
            circle.Apply();
        }
        public static void Rect(float x,float y,float w,float h,Color c)
        {
            if(w<=0 || h<=0) return;
            GUI.color=c; GUI.DrawTexture(new Rect(Snap(x),Snap(y),Mathf.Max(Pixel,Snap(w)),Mathf.Max(Pixel,Snap(h))),Texture2D.whiteTexture); GUI.color=Color.white;
        }
        public static void Ellipse(float x,float y,float w,float h,Color c)
        { Init(); GUI.color=c; GUI.DrawTexture(new Rect(Snap(x),Snap(y),Mathf.Max(Pixel,Snap(w)),Mathf.Max(Pixel,Snap(h))),circle); GUI.color=Color.white; }
        public static void Round(float x,float y,float w,float h,float radius,Color c)
        {
            if(w<12 || h<12) { Rect(x,y,w,h,c); return; }
            Rect(x+Pixel,y,w-2*Pixel,h,c); Rect(x,y+Pixel,w,h-2*Pixel,c);
        }
        public static void Line(Vector2 a,Vector2 b,float width,Color c)
        {
            int steps=Mathf.Max(1,Mathf.CeilToInt(Mathf.Max(Mathf.Abs(b.x-a.x),Mathf.Abs(b.y-a.y))/Pixel));
            float thickness=Mathf.Max(Pixel,Snap(width));
            for(int i=0;i<=steps;i++) { Vector2 p=Vector2.Lerp(a,b,i/(float)steps); Rect(p.x-thickness/2,p.y-thickness/2,thickness,thickness,c); }
        }
        public static void Triangle(Vector2 a,Vector2 b,Vector2 c,Color color)
        {
            if(a.y>b.y) {var t=a;a=b;b=t;} if(b.y>c.y) {var t=b;b=c;c=t;} if(a.y>b.y) {var t=a;a=b;b=t;}
            if(c.y-a.y<.01f) return;
            for(float y=Snap(a.y);y<=c.y;y+=Pixel)
            {
                float center=y+Pixel/2;
                float left=Mathf.Lerp(a.x,c.x,Mathf.Clamp01((center-a.y)/(c.y-a.y)));
                float right=center<b.y && b.y>a.y?Mathf.Lerp(a.x,b.x,(center-a.y)/(b.y-a.y)):Mathf.Lerp(b.x,c.x,c.y==b.y?0:Mathf.Clamp01((center-b.y)/(c.y-b.y)));
                Rect(Mathf.Min(left,right),y,Mathf.Max(Pixel,Mathf.Abs(right-left)),Pixel,color);
            }
        }
        public static void Quad(Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color color) {Triangle(a,b,c,color);Triangle(a,c,d,color);}
        public static void Text(string text,float x,float y,float w,float h,int size,Color color,bool bold=false,TextAnchor anchor=TextAnchor.UpperLeft)
            => PixelFont.Draw(text,x,y,w,h,size,color,bold,anchor);
        public static void Sky() {for(int i=0;i<8;i++) Rect(0,i*60,1600,60,Color.Lerp(Hex("D59A68"),Hex("3A4868"),i/7f));}
    }
}
