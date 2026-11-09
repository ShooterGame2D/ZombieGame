using System.Collections.Generic;
using UnityEngine;

namespace ZHold
{
    // Readable Google Fonts face, rendered once without shadow, outline or synthetic bold.
    public static class PixelFont
    {
        static Font font;
        static GUIStyle style;
        static readonly Dictionary<string,int> sizes=new Dictionary<string,int>();
        public static Font Face => font!=null?font:(font=Resources.Load<Font>("Fonts/AtkinsonHyperlegible-Regular"));
        static GUIStyle Style
        {
            get
            {
                if(style==null) style=new GUIStyle { font=Face,fontStyle=FontStyle.Normal,richText=false,
                    wordWrap=true,padding=new RectOffset(),margin=new RectOffset(),clipping=TextClipping.Clip };
                return style;
            }
        }
        public static int FitSize(string text,float width,float height,int preferred)
        {
            var label=new GUIContent(text); var s=Style;
            int candidate=Mathf.Max(10,preferred);
            // Fit in single-pixel increments; preserve clear accents and punctuation.
            for(;candidate>=8;candidate--)
            {
                s.fontSize=candidate;
                if(s.CalcHeight(label,width)<=height+.5f && LongestWordWidth(text,s)<=width+.5f) return candidate;
            }
            for(candidate=7;candidate>4;candidate--)
            {
                s.fontSize=candidate;
                if(s.CalcHeight(label,width)<=height+.5f && LongestWordWidth(text,s)<=width+.5f) return candidate;
            }
            return 4;
        }
        static float LongestWordWidth(string text,GUIStyle s)
        {
            float width=0;
            foreach(var word in text.Split(' ','\n')) width=Mathf.Max(width,s.CalcSize(new GUIContent(word)).x);
            return width;
        }
        public static void Draw(string text,float x,float y,float w,float h,int size,Color color,bool bold,TextAnchor anchor)
        {
            if(string.IsNullOrEmpty(text)||w<=0||h<=0||Face==null) return;
            var matrix=GUI.matrix;
            var origin=matrix.MultiplyPoint3x4(new Vector3(x,y,0));
            float scale=Mathf.Max(.01f,Mathf.Abs(matrix.m00));
            var rect=new Rect(Mathf.Round(origin.x),Mathf.Round(origin.y),Mathf.Round(w*scale),Mathf.Round(h*Mathf.Abs(matrix.m11)));
            int preferred=Mathf.Max(10,Mathf.RoundToInt(size*scale*1.18f));
            string key=rect.width+":"+rect.height+":"+preferred+":"+text;
            if(!sizes.TryGetValue(key,out int fitted))
            {
                if(sizes.Count>=1024) sizes.Clear();
                fitted=FitSize(text,rect.width,rect.height,preferred); sizes[key]=fitted;
            }
            var s=Style; s.fontSize=fitted; s.alignment=anchor; s.normal.textColor=color;
            var previousColor=GUI.color;
            // Render at physical resolution instead of shrinking a virtual-resolution label.
            GUI.matrix=Matrix4x4.identity; GUI.color=Color.white;
            GUI.Label(rect,text,s);
            GUI.matrix=matrix; GUI.color=previousColor;
        }
    }
}
