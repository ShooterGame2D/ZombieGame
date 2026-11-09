using UnityEngine;
using static ZHold.Draw2D;
namespace ZHold
{
    public partial class GameApplication
    {
        bool showHome=true,settings,mouseCaptured;
        void Home()
        {
            Rect(0,0,1600,900,new Color(.02f,.025f,.06f,.5f));
            Panel(120,138,820,610);
            Tag("EDIÇÃO PIXEL / DUAS FRENTES",160,176,450,Mint);
            Text("Z-HOLD",155,240,740,210,98,Gold,true);
            Text("O perigo vem dos dois lados.\nSua defesa começa aqui.",164,467,715,72,27,Cream);
            if(Button("JOGAR",160,574,330,62,Gold,true,27))showHome=false;
            if(Button("TUTORIAL",516,574,330,62,Mint,true,25))Run.StartTutorial();
            if(Button("CONFIGURAÇÕES",160,660,686,49,Hex("345452"),true,22))settings=true;
            GameArt.Portrait(Run.profile.selected,990,214,440,475);
            Text("Recorde: "+Run.profile.bestWave+" ondas",1035,704,435,38,25,Cream,true,TextAnchor.MiddleCenter);
            Text("M1 atirar   ·   M2 coletar   ·   R recarregar   ·   Espaço especial",185,804,1230,38,23,Cream,false,TextAnchor.MiddleCenter);
        }
        float Setting(string label,float value,float min,float max,float y,string suffix="%")
        {
            Text(label,368,y,435,35,25,Cream,true);
            Text(suffix=="x"?value.ToString("0.00")+"×":Mathf.RoundToInt(value*100)+"%",1080,y,135,35,24,Gold,true,TextAnchor.MiddleRight);
            if(Button("−",815,y,50,38,Hex("345452"),true,24)) value=Mathf.Max(min,value-(max-min)/20);
            if(Button("+",1015,y,50,38,Hex("345452"),true,24)) value=Mathf.Min(max,value+(max-min)/20);
            // Native slider uses the system pointer while gameplay capture is released.
            value=GUI.HorizontalSlider(new UnityEngine.Rect(882,y+13,116,24),value,min,max);
            return value;
        }
        void SettingsPanel()
        {
            Shade();Panel(320,115,960,680);Tag("AJUSTE DO SEU JEITO",605,142,390,Mint);
            Text("CONFIGURAÇÕES",365,197,870,52,40,Cream,true);
            GameSettings.Brightness=Setting("Brilho da tela",GameSettings.Brightness,.65f,1.35f,280);
            GameSettings.Sensitivity=Setting("Sensibilidade do mouse",GameSettings.Sensitivity,.25f,3,340,"x");
            GameSettings.Master=Setting("Volume geral",GameSettings.Master,0,1,400);
            GameSettings.Effects=Setting("Efeitos sonoros",GameSettings.Effects,0,1,460);
            GameSettings.Music=Setting("Música do cenário",GameSettings.Music,0,1,520);
            GameSettings.Shake=Setting("Tremor de câmera",GameSettings.Shake,0,1,580);
            Text("A sensibilidade vale para a mira durante a partida. ESC pausa.",368,642,850,34,21,Hex("BBCBC8"));
            if(Button("SALVAR E VOLTAR",540,708,520,53,Gold,true,24)) {GameSettings.Save();settings=false;}
        }
        void Brightness()
        {
            if(GameSettings.Brightness<1)Rect(0,110,1600,690,new Color(0,0,0,1-GameSettings.Brightness));
            else if(GameSettings.Brightness>1)Rect(0,110,1600,690,new Color(1,.97f,.88f,(GameSettings.Brightness-1)*.4f));
            if(Run.focusTimer>0&&Run.state==RunState.Wave)
            {
                Rect(0,110,1600,690,new Color(1,.36f,.035f,.22f));
                Rect(0,110,24,690,new Color(.8f,.27f,.025f,.65f));Rect(1576,110,24,690,new Color(.8f,.27f,.025f,.65f));
            }
        }
        void LanePreview()
        {
            int lane=Run.selectedTreeLane;
            string[] names={"1 · ESQUERDA","2 · CENTRO","3 · DIREITA"};
            Panel(186,660,1228,128);
            string instruction=lane<0?"CLIQUE NA COLUNA DESEJADA · ESC: CANCELAR":"COLUNA "+(lane+1)+" SELECIONADA · ESPAÇO: DERRUBAR · ESC: CANCELAR";
            Text(instruction,210,674,1180,28,23,Gold,true,TextAnchor.MiddleCenter);
            for(int i=0;i<3;i++)
            {
                var box=TreeSelectionLayout.Button(i);bool hover=box.Contains(pointer);
                Color color=lane==i?Gold:hover?Hex("466964"):Hex("293C48");
                Rect(box.x-3,box.y-3,box.width+6,box.height+6,lane==i?Cream:Hex("62757F"));
                Rect(box.x,box.y,box.width,box.height,color);
                Text(names[i]+(lane==i?" · OK":""),box.x+6,box.y+4,box.width-12,box.height-8,25,lane==i?Ink:Cream,true,TextAnchor.MiddleCenter);
            }
        }
        void ExpandedHUD()
        {
            var boss=Run.MainBoss;
            if(boss!=null)
            {
                Panel(390,178,820,96);
                GUI.color=Color.white;GUI.DrawTextureWithTexCoords(new UnityEngine.Rect(410,184,68,80),Resources.Load<Texture2D>("Art/Pixel/Zombies"),boss.data.artUV);
                Text((boss.primaryBoss?"CHEFÃO: ":"ELITE: ")+boss.BossName.ToUpperInvariant()+" · "+RunManager.SideName(boss.side),500,188,680,31,23,Gold,true);
                Rect(500,233,570,19,Hex("552D37"));Rect(500,233,570*Mathf.Clamp01(boss.health/boss.maxHealth),19,Hex("ED6A4D"));
                Text(Mathf.CeilToInt(boss.health)+" / "+Mathf.CeilToInt(boss.maxHealth),1080,226,110,32,17,Cream,true,TextAnchor.MiddleRight);
            }
            string warning=null;
            if(Run.damageFlash>0)warning=Run.lastAttackSide!=Run.viewSide?"ATAQUE "+(Run.lastAttackSide==ViewSide.Back?"PELAS COSTAS":"PELA FRENTE")+"!  TROQUE A CÂMERA!":"VOCÊ ESTÁ SENDO ATACADO!";
            else if(Run.otherAttackTimer>0&&Run.lastAttackSide!=Run.viewSide)warning="ATAQUE NA OUTRA CÂMERA!  Q / E / SCROLL";
            else if(Run.bossNotice>0)warning="CHEFÃO NA ÁREA — PREPARE SUA DEFESA!";
            else if(Run.ThreatsOnSide(Run.OtherSide)>0)warning="PERIGO NA OUTRA CÂMERA: "+RunManager.SideName(Run.OtherSide);
            if(warning!=null)
            {
                Panel(345,290,910,57);Rect(350,295,900,47,Hex("AA372D"));Text(warning,367,302,866,35,27,Cream,true,TextAnchor.MiddleCenter);
            }
            if(Run.state==RunState.Wave&&!Run.Reloading&&Run.ammo<=Mathf.Max(2,Run.Capacity*.25f))
            {
                Rect(490,759,620,37,Hex("A33C2D"));
                Text(Run.ammo==0&&Run.reserve==0?"SEM MUNIÇÃO — USE O ESPECIAL!":"MUNIÇÃO ACABANDO!  [R] RECARREGAR",500,764,600,27,23,Cream,true,TextAnchor.MiddleCenter);
            }
            if(Run.state==RunState.WaveEnd)
            {
                Panel(495,320,610,223);Text("ONDA "+Run.wave+" CONCLUÍDA",520,344,560,43,35,Mint,true,TextAnchor.MiddleCenter);
                Text(Mathf.CeilToInt(Run.waveEndTimer).ToString(),610,391,380,91,76,Gold,true,TextAnchor.MiddleCenter);
                Text("A loja abre em instantes",540,491,520,30,24,Cream,false,TextAnchor.MiddleCenter);
            }
            if(Run.choosingTreeLane&&Run.state==RunState.Wave&&!Run.paused&&!Run.tutorialWaiting)LanePreview();
        }
        static readonly string[] LessonTitles={"BEM-VINDO À DEFESA","DISPARE COM M1","MIRE NA CABEÇA","RECARREGUE COM R","COLETE COM M2","TROQUE DE CÂMERA","USE SEU ESPECIAL","SUA PRIMEIRA MINI-ONDA","VISITE A LOJA","TREINO CONCLUÍDO"};
        static readonly string[] Lessons={
            "Este treino não gasta seu progresso. O jogo pausa a cada explicação. Você pode aprender sem perder vida.",
            "Mova o mouse para mirar. Clique ou segure o botão esquerdo para atingir o zumbi parado.",
            "Acerte a cabeça do zumbi. O tiro na cabeça causa mais dano e ignora a armadura.",
            "Aperte R antes de esvaziar o pente. As balas que sobraram são preservadas; a reserva completa o pente.",
            "Mire na moeda dourada e clique com o botão direito. Coletar não dispara e funciona durante a recarga.",
            "Use Q, E ou a rodinha do mouse. Observe o radar: ele indica os inimigos da outra câmera.",
            "Aperte Espaço. Lenhador: clique em uma das três colunas na parte inferior e aperte Espaço novamente para derrubar a árvore. ESC cancela.",
            "Elimine os quatro alvos dos dois lados. Você não perde vida. Depois, haverá um contador de 3 segundos.",
            "Compre munição no botão verde. Explore as abas: armas, melhorias e defesas. Barricadas ficam à frente das torres.",
            "Você aprendeu a atirar, acertar a cabeça, recarregar, coletar, trocar de lado, usar o especial e comprar na loja."};
        void TutorialPanel()
        {
            int step=Run.tutorialStep;
            if(Run.tutorialWaiting)
            {
                Shade();Panel(345,252,910,360);Tag("TUTORIAL · "+Mathf.Min(step+1,10)+" / 10",610,277,380,Mint);
                Text(LessonTitles[step],380,330,840,48,35,Gold,true,TextAnchor.MiddleCenter);
                Text(Lessons[step],392,397,816,102,27,Cream,false,TextAnchor.MiddleCenter);
                if(Button(step==9?"VOLTAR AO INÍCIO":"PRATICAR",510,532,580,53,Gold,true,24))
                {
                    if(step==9){Run.tutorialStep=-1;Run.tutorialWaiting=false;Run.state=RunState.Menu;showHome=true;}
                    else Run.ContinueTutorial();
                }
            }
            else if(step!=8)
            {
                Panel(354,112,892,63);Text(LessonTitles[step]+"  ·  ESC para pausar",370,126,860,33,26,Gold,true,TextAnchor.MiddleCenter);
            }
        }
        void TowerShop()
        {
            if(Button("LADO: "+RunManager.SideName(Run.viewSide)+" [Q/E]",145,312,372,37,Hex("3D6056"),true,20))Run.SwitchSide();
            Text("COLUNA",550,319,130,27,20,Mint,true);
            for(int i=0;i<3;i++)if(Button((i+1).ToString(),696+i*83,312,70,37,selectedSlot==i?Gold:Hex("304D4C"),true,20))selectedSlot=i;
            int index=Run.SlotIndex(selectedSlot,Run.viewSide);var tower=Run.towers[index];var barrier=Run.barricades[index];
            Text("Barricada > torre > você",1000,319,440,27,22,Gold,true,TextAnchor.UpperRight);
            for(int i=0;i<3;i++)
            {
                float x=145+i*435;var t=catalog.towers[i];bool same=tower!=null&&tower.data==t;
                Round(x,373,415,121,0,Hex("243F3F"));Text(t.title,x+14,383,220,30,25,Cream,true);
                Text(t.description,x+14,419,385,25,19,Hex("B6CEC2"));
                if(Button(same?"INSTALADA":(tower==null?"INSTALAR / ":"SUBSTITUIR / ")+t.price,x+13,452,389,31,same?Mint:Gold,!same&&Run.wallet>=t.price,19))Run.BuyTower(i,selectedSlot);
            }
            if(tower!=null)
            {
                Text(tower.data.title+" · "+(tower.level==4?"NÍVEL MÁXIMO":"NÍVEL "+tower.level)+" · "+Mathf.CeilToInt(tower.health)+" PV",150,511,650,28,23,Cream,true);
                if(Button("REPARAR / 12",150,550,270,38,Mint,tower.health<tower.maxHealth&&Run.wallet>=12,20))Run.ServiceTower(selectedSlot,false);
                if(Button(tower.level>=4?"NÍVEL MÁXIMO":"MELHORAR / "+25*tower.level,438,550,300,38,Gold,tower.level<4&&Run.wallet>=25*tower.level,20))Run.ServiceTower(selectedSlot,true);
                Text("BARRICADA · "+(barrier==null?"ESPAÇO LIVRE":barrier.level==4?"NÍVEL MÁXIMO":"NÍVEL "+barrier.level),820,511,615,28,23,Cream,true);
                if(barrier==null)
                {if(Button("INSTALAR / "+Run.TowerPrice(3),820,550,610,38,Gold,Run.wallet>=Run.TowerPrice(3),20))Run.BuyTower(3,selectedSlot);}
                else
                {
                    if(Button("REPARAR / 12",820,550,280,38,Mint,barrier.health<barrier.maxHealth&&Run.wallet>=12,20))Run.ServiceTower(selectedSlot,false,true);
                    if(Button(barrier.level==4?"NÍVEL MÁXIMO":"REFORÇAR / "+25*barrier.level,1120,550,310,38,Gold,barrier.level<4&&Run.wallet>=25*barrier.level,20))Run.ServiceTower(selectedSlot,true,true);
                }
            }
            Text(tower==null?"Instale uma torre para liberar sua barricada.":"Substituir custa o preço completo e reinicia o nível da torre. A barricada permanece.",150,624,1280,36,22,Gold);
        }
    }
}
