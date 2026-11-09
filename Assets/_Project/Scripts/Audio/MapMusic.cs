using UnityEngine;
namespace ZHold
{
    // Original short looping compositions: each map has its own mode, tempo and instrumentation.
    public static class MapMusic
    {
        static float S(float hz,float t)=>Mathf.Sin(2*Mathf.PI*hz*t);
        public static float[] Samples(MapTheme theme,out int rate)
        {
            rate=22050;int id=(int)theme;
            float[] bpms={112,96,120,88,106,116,92,82};float beat=60/bpms[id],length=beat*32;
            var samples=new float[Mathf.RoundToInt(length*rate)];var rng=new System.Random(492+id);
            int[][] motifs={new[]{0,0,7,3,0,5,3,7},new[]{0,7,10,12,10,7,3,5},new[]{0,3,0,6,7,6,3,0},new[]{0,7,12,10,7,3,2,0},new[]{0,3,5,7,10,7,5,3},new[]{0,4,7,12,11,7,4,2},new[]{0,5,7,3,0,3,5,7},new[]{0,2,7,9,7,2,0,-3}};
            float root= id==3?146.83f:id==7?130.81f:110*Mathf.Pow(2,id/12f);
            for(int i=0;i<samples.Length;i++)
            {
                float t=i/(float)rate;int step=Mathf.FloorToInt(t/(beat*.5f));float local=t%(beat*.5f),per=t%beat;
                int note=motifs[id][step%8];float hz=root*Mathf.Pow(2,note/12f);
                float pluck=(S(hz,local)+.33f*S(hz*2,local)+.13f*S(hz*3,local))*Mathf.Exp(-local*(id==3?12:8));
                if(id==1||id==7)pluck=S(hz,local)*Mathf.Sin(Mathf.PI*Mathf.Clamp01(local/(beat*.5f)))*.85f;
                if(id==2)pluck=(S(hz,local)+.3f*S(hz*2.76f,local))*Mathf.Exp(-local*17);
                float bass=S(root*.5f,per)*Mathf.Exp(-per*5)*.14f;
                float kick=S(52+80*Mathf.Exp(-per*35),per)*Mathf.Exp(-per*24)*.13f;
                float noise=(float)rng.NextDouble()*2-1;
                float hat=noise*Mathf.Exp(-local*150)*.025f;
                float snare=(step/2%4==1||step/2%4==3)?noise*Mathf.Exp(-per*40)*.06f:0;
                samples[i]=Mathf.Clamp(pluck*.10f+bass+kick+hat+snare,-.65f,.65f);
            }
            // Small boundary fades eliminate clicks while the beat grid remains continuous.
            for(int i=0;i<200;i++){float f=i/200f;samples[i]*=f;samples[samples.Length-1-i]*=f;}
            return samples;
        }
    }
}
