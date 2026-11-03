using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace ZombieAttack
{
    // Original 5x7 bitmap lettering, cached per label, with Portuguese accents.
    public static class PixelFont
    {
        static readonly Dictionary<char,byte[]> glyphs=new Dictionary<char,byte[]>();
        static readonly Dictionary<string,Texture2D> labels=new Dictionary<string,Texture2D>();
        static void Add(char c,string rows)
        {var values=rows.Split(',');var data=new byte[7];for(int i=0;i<7;i++)data[i]=Convert.ToByte(values[i],16);glyphs[c]=data;}
        static void Init()
        {
            if(glyphs.Count>0)return;
            string[] letters={"0E,11,11,1F,11,11,11","1E,11,11,1E,11,11,1E","0F,10,10,10,10,10,0F","1E,11,11,11,11,11,1E","1F,10,10,1E,10,10,1F","1F,10,10,1E,10,10,10","0F,10,10,17,11,11,0F","11,11,11,1F,11,11,11","0E,04,04,04,04,04,0E","07,02,02,02,12,12,0C","11,12,14,18,14,12,11","10,10,10,10,10,10,1F","11,1B,15,15,11,11,11","11,19,15,13,11,11,11","0E,11,11,11,11,11,0E","1E,11,11,1E,10,10,10","0E,11,11,11,15,12,0D","1E,11,11,1E,14,12,11","0F,10,10,0E,01,01,1E","1F,04,04,04,04,04,04","11,11,11,11,11,11,0E","11,11,11,11,11,0A,04","11,11,11,15,15,1B,11","11,11,0A,04,0A,11,11","11,11,0A,04,04,04,04","1F,01,02,04,08,10,1F"};
            for(int i=0;i<26;i++)Add((char)('A'+i),letters[i]);
            string[] lower={"00,00,0E,01,0F,11,0F","10,10,1E,11,11,11,1E","00,00,0F,10,10,10,0F","01,01,0F,11,11,11,0F","00,00,0E,11,1F,10,0F","06,08,08,1E,08,08,08","00,0F,11,11,0F,01,0E","10,10,1E,11,11,11,11","04,00,0C,04,04,04,0E","02,00,06,02,02,12,0C","10,10,12,14,18,14,12","0C,04,04,04,04,04,0E","00,00,1A,15,15,15,15","00,00,1E,11,11,11,11","00,00,0E,11,11,11,0E","00,1E,11,11,1E,10,10","00,0F,11,11,0F,01,01","00,00,16,19,10,10,10","00,00,0F,10,0E,01,1E","08,08,1E,08,08,09,06","00,00,11,11,11,11,0F","00,00,11,11,11,0A,04","00,00,11,11,15,15,0A","00,00,11,0A,04,0A,11","00,11,11,11,0F,01,0E","00,00,1F,02,04,08,1F"};
            for(int i=0;i<26;i++)Add((char)('a'+i),lower[i]);
            string[] nums={"0E,11,13,15,19,11,0E","04,0C,04,04,04,04,0E","0E,11,01,02,04,08,1F","1E,01,01,0E,01,01,1E","02,06,0A,12,1F,02,02","1F,10,10,1E,01,01,1E","0E,10,10,1E,11,11,0E","1F,01,02,04,08,08,08","0E,11,11,0E,11,11,0E","0E,11,11,0F,01,01,0E"};
            for(int i=0;i<10;i++)Add((char)('0'+i),nums[i]);
            Add(' ',"00,00,00,00,00,00,00");Add('.',"00,00,00,00,00,0C,0C");Add(',',"00,00,00,00,00,04,08");Add(':',"00,0C,0C,00,0C,0C,00");
            Add(';',"00,0C,0C,00,04,04,08");Add('!',"04,04,04,04,04,00,04");Add('?',"0E,11,01,02,04,00,04");Add('-',"00,00,00,1F,00,00,00");
            Add('+',"00,04,04,1F,04,04,00");Add('/',"01,02,02,04,08,08,10");Add('>',"10,08,04,02,04,08,10");Add('<',"01,02,04,08,04,02,01");
            Add('[',"0E,08,08,08,08,08,0E");Add(']',"0E,02,02,02,02,02,0E");Add('(',"02,04,08,08,08,04,02");Add(')',"08,04,02,02,02,04,08");
            Add('$',"04,0F,14,0E,05,1E,04");Add('%',"19,19,02,04,08,13,13");Add('=',"00,00,1F,00,1F,00,00");Add('·',"00,00,00,04,00,00,00");
            Add('×',"00,11,0A,04,0A,11,00");Add('|',"04,04,04,04,04,04,04");Add('\'',"04,04,08,00,00,00,00");Add('"',"0A,0A,00,00,00,00,00");
        }
        static char Base(char c)
        {string d=c.ToString().Normalize(NormalizationForm.FormD);return glyphs.ContainsKey(d[0])?d[0]:'?';}
        static int Width(char c)
        {
            if(c==' ')return 3;var g=glyphs[Base(c)];int min=5,max=0;
            foreach(byte row in g)for(int x=0;x<5;x++)if((row&(1<<(4-x)))!=0){min=Mathf.Min(min,x);max=Mathf.Max(max,x);}
            return max-min+2;
        }
        static int Measure(string s) {int n=0;foreach(char c in s)n+=Width(c);return Mathf.Max(1,n-1);}
        static List<string> Wrap(string text,int width)
        {
            var lines=new List<string>();
            foreach(string paragraph in text.Split('\n'))
            {
                string line="";
                foreach(string word in paragraph.Split(' '))
                {string next=line.Length==0?word:line+" "+word;if(Measure(next)>width && line.Length>0){lines.Add(line);line=word;}else line=next;}
                lines.Add(line);
            }
            return lines;
        }
        static Texture2D Make(string text,int width)
        {
            var lines=Wrap(text,width);int tw=1;foreach(string line in lines)tw=Mathf.Max(tw,Measure(line));
            int th=lines.Count*11;var pixels=new Color32[tw*th];
            Action<int,int> put=(x,y)=>{if(x>=0 && x<tw && y>=0 && y<th)pixels[(th-1-y)*tw+x]=new Color32(255,255,255,255);};
            for(int row=0;row<lines.Count;row++)
            {
                int cursor=0;
                foreach(char c in lines[row])
                {
                    var g=glyphs[Base(c)];int left=5;
                    foreach(byte bits in g)for(int x=0;x<5;x++)if((bits&(1<<(4-x)))!=0)left=Mathf.Min(left,x);
                    if(c!=' ')
                    {
                        for(int y=0;y<7;y++)for(int x=0;x<5;x++)if((g[y]&(1<<(4-x)))!=0)put(cursor+x-left,row*11+2+y);
                        string d=c.ToString().Normalize(NormalizationForm.FormD);char accent=d.Length>1?d[1]:'\0';
                        if(accent=='\u0301'){put(cursor+2,row*11);put(cursor+1,row*11+1);}
                        if(accent=='\u0300'){put(cursor+1,row*11);put(cursor+2,row*11+1);}
                        if(accent=='\u0302'){put(cursor+2,row*11);put(cursor+1,row*11+1);put(cursor+3,row*11+1);}
                        if(accent=='\u0303'){put(cursor+1,row*11);put(cursor+3,row*11);put(cursor+2,row*11+1);}
                        if(accent=='\u0327'){put(cursor+2,row*11+9);put(cursor+1,row*11+10);}
                    }
                    cursor+=Width(c);
                }
            }
            // Remove empty ascent/descent so a seven-pixel caption remains readable in small windows.
            int first=th,last=-1;
            for(int i=0;i<pixels.Length;i++)if(pixels[i].a!=0){first=Mathf.Min(first,i/tw);last=Mathf.Max(last,i/tw);}
            if(last>=first){int croppedHeight=last-first+1;var cropped=new Color32[tw*croppedHeight];Array.Copy(pixels,first*tw,cropped,0,cropped.Length);pixels=cropped;th=croppedHeight;}
            var texture=new Texture2D(tw,th,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.HideAndDontSave};
            texture.SetPixels32(pixels);texture.Apply(false,true);return texture;
        }
        public static void Draw(string text,float x,float y,float w,float h,int size,Color color,bool bold,TextAnchor anchor)
        {
            if(string.IsNullOrEmpty(text) || w<=0 || h<=0)return;Init();
            // Font pixels occupy whole physical screen pixels, even in a narrow browser panel.
            float screenScale=Mathf.Max(.01f,GUI.matrix.m00);
            float scale=Mathf.Max(1,Mathf.Round(size/9f*screenScale))/screenScale;
            int logicalWidth=Mathf.Max(1,Mathf.FloorToInt(w/scale));string key=logicalWidth+":"+text;
            if(!labels.TryGetValue(key,out var texture))
            {
                if(labels.Count>=512){foreach(var cached in labels.Values)UnityEngine.Object.Destroy(cached);labels.Clear();}
                texture=Make(text,logicalWidth);labels[key]=texture;
            }
            scale=Mathf.Min(scale,w/texture.width,h/texture.height);
            if(scale*screenScale>=1)scale=Mathf.Floor(scale*screenScale+.001f)/screenScale;
            float dw=texture.width*scale,dh=texture.height*scale;
            int align=(int)anchor;if(align%3==1)x+=(w-dw)/2;else if(align%3==2)x+=w-dw;
            if(align/3==1)y+=(h-dh)/2;else if(align/3==2)y+=h-dh;
            x=(Mathf.Round(x*screenScale+GUI.matrix.m03)-GUI.matrix.m03)/screenScale;
            y=(Mathf.Round(y*screenScale+GUI.matrix.m13)-GUI.matrix.m13)/screenScale;
            if(bold){GUI.color=new Color(.04f,.03f,.09f,color.a*.7f);GUI.DrawTexture(new Rect(x+scale,y+scale,dw,dh),texture);}
            GUI.color=color;GUI.DrawTexture(new Rect(x,y,dw,dh),texture);GUI.color=Color.white;
        }
    }
}
