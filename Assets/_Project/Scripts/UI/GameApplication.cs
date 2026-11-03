using UnityEngine;
using UnityEngine.InputSystem;
using static ZombieAttack.Draw2D;
namespace ZombieAttack
{
    public class GameApplication : MonoBehaviour
    {
        public RunManager Run { get; private set; }
        GameCatalog catalog;
        PrototypeAudio sound;
        Vector2 pointer=new Vector2(800,450);
        float scale=1,offsetX,offsetY,time,waveNotice;
        bool help,uiClick,allowButtons;
        int selectedSlot,shopTab;
        readonly string[] upgradeNames={"Dano","Cadência","Pente maior","Recarga rápida"};
        readonly string[] upgradeDescriptions={"+0,45 de dano por disparo","+15% disparos por segundo","+2 unidades por pente","-16% no tempo de recarga"};
        void Awake()
        {
            Application.targetFrameRate=60; catalog=Resources.Load<GameCatalog>("GameCatalog");
            if(catalog==null) { Debug.LogError("Catálogo ausente."); enabled=false; return; }
            Run=new RunManager(catalog,SaveSystem.Load()); sound=gameObject.AddComponent<PrototypeAudio>();
            Run.OnSound+=sound.Play; Run.OnSpatialSound+=sound.PlaySpatial; Run.OnWaveStarted+=_=>waveNotice=3.5f;
            if(Camera.main==null) { var obj=new GameObject("Camera"); var cam=obj.AddComponent<Camera>(); cam.orthographic=true; cam.clearFlags=CameraClearFlags.SolidColor; cam.backgroundColor=Ink; obj.tag="MainCamera"; obj.AddComponent<AudioListener>(); }
        }
        void Layout()
        {
            scale=Mathf.Max(.01f,Mathf.Min(Screen.width/1600f,Screen.height/900f)); offsetX=(Screen.width-1600*scale)*.5f; offsetY=(Screen.height-900*scale)*.5f;
        }
        bool CombatPoint(Vector2 p) => p.x>=0 && p.x<1600 && p.y>175 && p.y<790 && !(p.x>1300 && p.y<405);
        void Update()
        {
            if(Run==null) return; Layout(); sound.ListenerSide=Run.viewSide;
            if(Mouse.current!=null)
            {
                var p=Mouse.current.position.ReadValue(); pointer=new Vector2((p.x-offsetX)/scale,(Screen.height-p.y-offsetY)/scale);
                uiClick|=Mouse.current.leftButton.wasReleasedThisFrame;
                if(Run.state==RunState.Wave && !Run.paused && !help && !Run.demo)
                {
                    if(Mouse.current.leftButton.isPressed && CombatPoint(pointer)) Run.Shoot(pointer);
                    if(Mouse.current.rightButton.wasPressedThisFrame && CombatPoint(pointer)) Run.UseAbility(pointer);
                }
            }
            if(Keyboard.current!=null)
            {
                var key=Keyboard.current;
                if(key.escapeKey.wasPressedThisFrame)
                {
                    if(help) help=false;
                    else if(Run.state==RunState.Wave || Run.state==RunState.Shop) Run.paused=!Run.paused;
                }
                if(!help && !Run.paused)
                {
                    if(key.qKey.wasPressedThisFrame || key.eKey.wasPressedThisFrame || key.tabKey.wasPressedThisFrame) Run.SwitchSide();
                    if(key.spaceKey.wasPressedThisFrame) Run.UseAbility(pointer);
                    if(key.enterKey.wasPressedThisFrame && Run.state==RunState.Shop) Run.NextWave();
                }
            }
            if(!help) Run.Tick(Time.unscaledDeltaTime);
            if(!Run.paused && !help) { time+=Time.unscaledDeltaTime; waveNotice-=Time.unscaledDeltaTime; }
            Cursor.visible=Run.state!=RunState.Wave || Run.paused || help || Run.demo || !CombatPoint(pointer);
        }
        void OnApplicationFocus(bool focused) { if(!focused && Run!=null && !Run.demo && (Run.state==RunState.Wave || Run.state==RunState.Shop)) Run.paused=true; }
        void OnDestroy() { Cursor.visible=true; }
        void OnGUI()
        {
            if(Run==null) return; Layout(); Draw2D.Init(); GUI.matrix=Matrix4x4.identity; Rect(0,0,Screen.width,Screen.height,Ink);
            GUI.matrix=Matrix4x4.TRS(new Vector3(offsetX,offsetY,0),Quaternion.identity,new Vector3(scale,scale,1));
            GameArt.Background(Run.character.theme,Run.state==RunState.Menu?ViewSide.Front:Run.viewSide,time);
            if(Run.state==RunState.Menu) { allowButtons=!help; Menu(); }
            else
            {
                Battle(); allowButtons=!help && !Run.paused && Run.state==RunState.Wave; HUD();
                if(Run.state==RunState.Shop) { allowButtons=!help && !Run.paused; Shop(); }
                if(Run.state==RunState.GameOver) { allowButtons=!help; GameOver(); }
                if(Run.paused) { allowButtons=!help; Pause(); }
                if(Run.state==RunState.Wave && !Run.paused && !Run.demo && !help && CombatPoint(pointer)) Crosshair();
            }
            if(help) { allowButtons=true; Help(); }
            if(Event.current.type==EventType.Repaint) uiClick=false;
            GUI.matrix=Matrix4x4.identity; GUI.color=Color.white;
        }
        bool Button(string title,float x,float y,float w,float h,Color color,bool enabled=true,int size=18)
        {
            bool over=enabled && allowButtons && new Rect(x,y,w,h).Contains(pointer);
            Round(x-4,y-4,w+8,h+12,0,Hex("0B0C19")); Round(x,y,w,h,0,enabled?(over?Color.Lerp(color,Color.white,.18f):color):Hex("394158"));
            Rect(x+4,y,w-8,4,enabled?Color.Lerp(color,Color.white,.3f):Hex("586079"));
            Rect(x+4,y+h-4,w-8,4,enabled?Color.Lerp(color,Ink,.38f):Hex("22293A"));
            bool bright=color==Gold || color==Mint || color==Cream;
            Text(title,x+8,y,w-16,h,size,enabled?(bright?Ink:Cream):Hex("87958B"),true,TextAnchor.MiddleCenter);
            if(over && uiClick && Event.current.type==EventType.Repaint) { uiClick=false; return true; } return false;
        }
        void Panel(float x,float y,float w,float h)
        {
            Round(x-4,y-4,w+8,h+12,0,Hex("0C0D1C")); Round(x,y,w,h,0,Hex("46516C"));
            Round(x+4,y+4,w-8,h-8,0,new Color(.105f,.13f,.21f,.97f));
            Rect(x+8,y+4,w-16,4,Hex("59637A"));
        }
        void Tag(string title,float x,float y,float width,Color color)
        { Round(x,y,width,28,14,color); Text(title,x+8,y,width-16,28,12,Ink,true,TextAnchor.MiddleCenter); }
        void Shade() => Rect(0,0,1600,900,new Color(.01f,.035f,.055f,.76f));
        void Menu()
        {
            Rect(0,0,1600,900,new Color(.015f,.035f,.055f,.32f));
            Panel(40,28,1520,64); Text("DZ / DEFESA ZUMBI",62,48,430,30,21,Cream,true);
            Text("EDIÇÃO PIXEL  ·  VERSÃO 0.3",560,52,560,24,15,Mint);
            Text("RECORDE "+Run.profile.bestWave+"   /   "+Run.profile.gems+" GEMAS",1100,51,430,24,15,Gold,true,TextAnchor.UpperRight);
            Panel(40,120,732,285); Tag("FRENTE E COSTAS",64,144,215,Mint);
            Text("DEFESA ZUMBI",58,186,692,83,76,Gold,true);
            Text("Mantenha os dois lados sob controle.",66,277,660,31,24,Cream);
            Text("Pente vazio? Recarregue. Outra câmera? Fique de olho.",66,315,660,25,17,Hex("ACC2B7"));
            if(Button("COMEÇAR DEFESA  >",65,351,360,40,Gold)) { Run.Start(); shopTab=0; selectedSlot=0; }
            if(Button("COMO JOGAR",446,351,300,40,Hex("345452"),true,15)) help=true;
            Panel(799,120,761,285); GameArt.Portrait(Run.profile.selected,810,130,260,260);
            var c=Run.character;
            Text(c.title.ToUpperInvariant(),1080,142,450,40,31,c.accent,true);
            Text(c.mapTitle+"  /  "+c.health+" PV",1083,190,430,30,18,Cream);
            Text(c.abilityTitle,1083,242,430,28,22,Gold,true);
            Text(c.abilityDescription,1083,280,425,64,18,Hex("BED1C6"));
            Text(c.cooldown+" s de recarga da habilidade",1083,363,425,24,13,Hex("91ACA6"));
            Text("ESCOLHA SEU SOBREVIVENTE",46,424,950,29,17,Cream,true);
            Text("8 PERSONAGENS DISPONÍVEIS PARA TESTAR",1020,428,530,24,12,Mint,false,TextAnchor.UpperRight);
            for(int i=0;i<8;i++)
            {
                float x=40+(i%4)*384,y=467+(i/4)*166; var hero=catalog.characters[i]; bool selected=Run.profile.selected==i;
                Round(x,y,368,151,13,selected?hero.accent:Hex("344B4B")); Panel(x+2,y+2,364,147);
                GameArt.Portrait(i,x+8,y+9,111,111);
                Text(hero.title,x+128,y+14,224,26,22,Cream,true);
                Text(hero.weapon.title,x+128,y+48,223,43,14,Hex("B4CAC0"));
                Text(hero.health+" PV  /  "+hero.cooldown+" s",x+128,y+95,215,21,12,hero.accent,true);
                if(Button(selected?"SELECIONADO":"ESCOLHER",x+12,y+120,344,23,selected?Gold:Hex("31504E"),!selected,11)) Run.SelectCharacter(i);
            }
            if(Button("VER DEMONSTRAÇÃO",40,836,300,38,Hex("31504E"),true,14)) { Run.Start(true); shopTab=0; }
            Text("MOUSE disparar    Q / E trocar lado    ESPAÇO habilidade    ESC pausa",390,847,940,24,14,Cream);
            if(Button(sound.Muted?"SOM OFF":"SOM ON",1410,836,150,38,Hex("31504E"),true,13)) sound.Muted=!sound.Muted;
        }
        void Battle()
        {
            var sorted=new System.Collections.Generic.List<Zombie>(); foreach(var z in Run.zombies) if(z.side==Run.viewSide) sorted.Add(z);
            sorted.Sort((a,b)=>b.distance.CompareTo(a.distance));
            foreach(var z in sorted)
            {
                GameArt.ZombieArt(z,time);
                if(Run.rootTimers[RunManager.SideIndex(z.side)]>0)
                {
                    var p=z.Position; Line(new Vector2(p.x-30*z.Size,p.y),new Vector2(p.x+25*z.Size,p.y-45*z.Size),5*z.Size,Mint);
                    Line(new Vector2(p.x+30*z.Size,p.y),new Vector2(p.x-20*z.Size,p.y-35*z.Size),4*z.Size,Mint);
                }
            }
            for(int i=0;i<3;i++) { int index=Run.SlotIndex(i,Run.viewSide); GameArt.TowerArt(i,Run.towers[index]); GameArt.BarricadeArt(i,Run.barricades[index]); }
            if(Run.temporaryTurret>0 && Run.turretSide==Run.viewSide)
            {
                Round(1040,686,90,65,9,Hex("3A4F51")); Rect(1072,653,26,45,Gold); Text(Mathf.CeilToInt(Run.temporaryTurret)+"s",1040,759,90,23,15,Gold,true,TextAnchor.MiddleCenter);
            }
            if(Run.decoyTimers[RunManager.SideIndex(Run.viewSide)]>0)
            {
                Ellipse(746,608,108,32,new Color(0,0,0,.3f)); Round(768,545,64,74,15,Hex("E49B63")); Ellipse(751,530,42,42,Gold); Ellipse(808,530,42,42,Gold); Ellipse(774,545,55,53,Gold);
                Text("ISCA",743,627,120,26,15,Gold,true,TextAnchor.MiddleCenter);
            }
            foreach(var p in Run.projectiles) if(p.side==Run.viewSide) { var pos=p.Position; float r=p.Radius; Ellipse(pos.x-r-5,pos.y-r-5,r*2+10,r*2+10,new Color(.6f,.9f,.2f,.2f)); Ellipse(pos.x-r,pos.y-r,r*2,r*2,Hex("B4D950")); }
            foreach(var coin in Run.coins) if(coin.side==Run.viewSide) GameArt.CoinArt(coin,time);
            foreach(var e in Run.effects)
            {
                if(e.side!=Run.viewSide) continue;
                float t=1-e.life/e.duration; Color color=e.color; color.a*=1-t;
                if(e.kind==0) Text(e.text,e.position.x-105,e.position.y-55-t*35,210,45,e.text=="NA CABEÇA!"?17:23,color,true,TextAnchor.MiddleCenter);
                else if(e.kind==1) Line(e.origin,e.position,2,color);
                else if(e.kind==2) for(int i=0;i<6;i++) { float a=i*Mathf.PI/3; var v=new Vector2(Mathf.Cos(a),Mathf.Sin(a)); Line(e.position+v*(5+t*22),e.position+v*(14+t*39),4,color); }
                else if(e.kind==3)
                { float r=e.origin.x*(.2f+t*.8f); color.a*=.25f; Ellipse(e.position.x-r,e.position.y-r,r*2,r*2,color); for(int i=0;i<12;i++) { float a=i*Mathf.PI/6; var v=new Vector2(Mathf.Cos(a),Mathf.Sin(a)); Line(e.position+v*r*.7f,e.position+v*r,5,e.color); } }
            }
            GameArt.Weapon(pointer,Run,time);
            if(Run.smokeTimer>0) { Rect(0,130,1600,665,new Color(.38f,.4f,.55f,.24f)); Text("FUMAÇA: INVULNERÁVEL",515,212,570,36,20,Cream,true,TextAnchor.MiddleCenter); }
            if(Run.damageFlash>0) { var c=new Color(.95f,.19f,.1f,Run.damageFlash); Rect(0,0,1600,12,c); Rect(0,0,18,900,c); Rect(1582,0,18,900,c); Rect(0,886,1600,14,c); }
            if(Run.switchFlash>0) Rect(0,114,1600,680,new Color(.03f,.08f,.1f,Run.switchFlash));
        }
        void HUD()
        {
            Panel(25,22,430,84); GameArt.Portrait(Run.profile.selected,30,24,79,79);
            Text(Run.character.title.ToUpperInvariant(),120,34,220,24,14,Run.character.accent,true);
            Text(Mathf.CeilToInt(Run.health)+" / "+Run.character.health,319,34,115,25,17,Cream,true,TextAnchor.UpperRight);
            Round(120,72,312,13,6,Hex("354A49")); Round(120,72,312*Mathf.Clamp01(Run.health/Run.character.health),13,6,Run.health<30?Hex("F08C6D"):Mint);
            Panel(555,22,490,84); Text("ONDA "+Run.wave.ToString("00"),578,33,270,37,29,Cream,true);
            Text(Run.character.mapTitle.ToUpperInvariant(),581,78,340,19,10,Hex("ABC7BD"));
            Text(Run.Remaining.ToString("00"),931,32,85,36,29,Gold,true,TextAnchor.UpperRight); Text("RESTANTES",901,77,115,19,10,Cream,false,TextAnchor.UpperRight);
            Panel(1170,22,236,84); Text("$ "+Run.wallet.ToString("000"),1191,35,200,37,30,Gold,true); Text("MOEDAS DA PARTIDA",1195,79,191,17,10,Cream);
            if(Button("II",1421,22,65,84,Ink,true,27)) Run.paused=true;
            if(Button("?",1500,22,75,84,Ink,true,26)) help=true;
            if(Button(RunManager.SideName(Run.viewSide)+"   >   TROCAR [Q / E]",570,122,460,42,Run.viewSide==ViewSide.Front?Hex("31564E"):Hex("594344"),true,17)) Run.SwitchSide();
            Minimap();
            if(Run.otherAttackTimer>0 && Run.lastAttackSide!=Run.viewSide)
            {
                Round(430,180,740,64,10,Hex("9B3D32")); Text((Run.lastAttackSide==ViewSide.Back?"ATAQUE PELAS COSTAS!":"ATAQUE PELA FRENTE!")+"  TROQUE COM Q / E",444,190,712,44,21,Cream,true,TextAnchor.MiddleCenter);
            }
            else if(Run.ThreatsOnSide(Run.OtherSide)>0)
            {
                Round(480,181,640,45,10,Hex("7A4930")); Text("PERIGO NA OUTRA CÂMERA  /  "+RunManager.SideName(Run.OtherSide),491,188,618,31,17,Gold,true,TextAnchor.MiddleCenter);
            }
            else if(waveNotice>0 && Run.state==RunState.Wave)
            {
                Panel(540,182,520,57); Text(Run.wave%10==0?"COMANDANTE NA ÁREA":"DEFENDA OS DOIS LADOS",551,188,498,28,20,Gold,true,TextAnchor.MiddleCenter);
                Text("A outra câmera continua em movimento.",550,218,500,19,12,Cream,false,TextAnchor.MiddleCenter);
            }
            if(Run.spawned==Run.total && Run.zombies.Count==0 && Run.state==RunState.Wave) Text("COLETE AS ÚLTIMAS MOEDAS DOS DOIS LADOS",450,253,700,28,16,Gold,true,TextAnchor.MiddleCenter);
            Panel(25,807,493,75); Text(Run.weapon.title.ToUpperInvariant(),43,818,326,22,15,Cream,true);
            Text(Mathf.FloorToInt(Run.ammo)+" / "+Run.Capacity,367,816,130,28,25,Gold,true,TextAnchor.UpperRight);
            if(Run.Reloading)
            {
                Round(44,852,300,10,5,Hex("3F5550")); Round(44,852,300*Run.ReloadProgress,10,5,Gold); Text("RECARREGANDO "+Run.reloadTimer.ToString("0.0")+"s",45,868,301,13,9,Cream);
            }
            else
            {
                int segments=Run.Capacity; float w=(300-(segments-1)*2)/segments;
                for(int i=0;i<segments;i++) Round(44+i*(w+2),852,w,10,2,Run.ammo>=i+1?Gold:Hex("3F5550"));
                Text(Run.reserve==0?"SEM RESERVA · COMPRE NA LOJA":"RECARGA AO ESVAZIAR",44,867,310,14,9,Run.reserve==0?Hex("EF9B7D"):Hex("9AB8AD"));
            }
            Text("RESERVA "+Run.reserve,357,850,142,24,13,Mint,true,TextAnchor.UpperRight);
            Panel(1095,807,480,75); Text(Run.character.abilityTitle.ToUpperInvariant(),1113,821,322,25,17,Cream,true);
            Text("ESPAÇO / BOTÃO DIREITO",1115,855,305,21,10,Hex("9AB8AD"));
            Tag(Run.abilityCooldown>0?Mathf.CeilToInt(Run.abilityCooldown)+" s":"PRONTA",1454,832,104,Run.abilityCooldown>0?Hex("B1BBB0"):Mint);
            if(Run.focusTimer>0) Text("OLHO DE ÁGUIA  ·  DANO 2,5×",500,750,600,31,18,Gold,true,TextAnchor.MiddleCenter);
            if(Run.demo)
            {
                Tag("DEMONSTRAÇÃO",34,127,212,Gold);
                if(Button("ASSUMIR CONTROLE",34,167,212,34,Ink,true,13)) Run.Start();
            }
        }
        void Minimap()
        {
            Panel(1320,126,255,264); Text("RADAR / DOIS LADOS",1334,137,225,24,13,Cream,true,TextAnchor.MiddleCenter);
            float cx=1447,cy=265;
            Rect(1336,172,223,180,Hex("213839"));
            Rect(1336,Run.viewSide==ViewSide.Front?172:265,223,87,new Color(.45f,.75f,.62f,.13f));
            Line(new Vector2(cx,175),new Vector2(cx,352),1,Hex("57716B")); Line(new Vector2(1336,cy),new Vector2(1559,cy),2,Hex("6C7C70"));
            Text("FRENTE",1344,174,100,18,10,Mint,true); Text("COSTAS",1344,332,100,18,10,Gold,true);
            foreach(var z in Run.zombies)
            {
                float x=cx+z.lateral*93,y=cy+(z.side==ViewSide.Front?-1:1)*(15+z.distance*67); bool other=z.side!=Run.viewSide;
                Color color=z.distance<.22f?Hex("FF705C"):other?Gold:Mint; float r=z.data.kind==ZombieKind.Boss?6:4;
                if(z.distance<.22f) Ellipse(x-r-3,y-r-3,r*2+6,r*2+6,new Color(1,.3f,.2f,.25f+.2f*Mathf.Sin(time*8)));
                Ellipse(x-r,y-r,r*2,r*2,color);
            }
            foreach(var coin in Run.coins) if(coin.side!=Run.viewSide)
            { float x=cx+Mathf.Clamp((coin.position.x-800)/660,-1,1)*93; Ellipse(x-2,cy+(coin.side==ViewSide.Front?-37:37),4,4,Gold); }
            Ellipse(cx-6,cy-6,12,12,Cream); Triangle(new Vector2(cx-5,cy+(Run.viewSide==ViewSide.Front?-9:9)),new Vector2(cx+5,cy+(Run.viewSide==ViewSide.Front?-9:9)),new Vector2(cx,cy+(Run.viewSide==ViewSide.Front?-17:17)),Cream);
            Text("OUTRA CÂMERA: "+Run.CountOnSide(Run.OtherSide),1332,362,231,20,12,Gold,true,TextAnchor.MiddleCenter);
        }
        void Crosshair()
        {
            var coin=Run.CoinAt(pointer); var target=Run.ZombieAt(pointer); Color c=coin!=null?Gold:target!=null?Hex("F29B7A"):Cream;
            float x=pointer.x,y=pointer.y,gap=8+Run.recoil*7;
            Ellipse(x-2,y-2,4,4,c);
            Line(new Vector2(x-gap-9,y),new Vector2(x-gap,y),3,c); Line(new Vector2(x+gap,y),new Vector2(x+gap+9,y),3,c);
            Line(new Vector2(x,y-gap-9),new Vector2(x,y-gap),3,c); Line(new Vector2(x,y+gap),new Vector2(x,y+gap+9),3,c);
            if(coin!=null) Text("COLETAR",x+25,y-12,110,24,12,Gold,true);
        }
        void Shop()
        {
            Shade(); Panel(115,119,1370,671);
            Text("REORGANIZE A DEFESA",145,143,850,48,35,Cream,true);
            Text("Onda "+Run.completed+" vencida  /  bônus de sobrevivência +"+Run.waveBonus+" moedas",148,201,930,25,17,Mint);
            Text(Run.wallet+" MOEDAS",1140,145,308,35,26,Gold,true,TextAnchor.UpperRight);
            Text("Próxima onda em "+Mathf.CeilToInt(Run.shopTimer)+"s",1140,200,308,24,15,Cream,false,TextAnchor.UpperRight);
            string[] tabs={"ARMAS","MELHORIAS E MUNIÇÃO","TORRES E BARRICADAS"};
            for(int i=0;i<3;i++) if(Button(tabs[i],145+i*435,248,416,45,i==shopTab?Gold:Hex("304D4C"),true,16)) shopTab=i;
            if(shopTab==0) WeaponShop(); else if(shopTab==1) UpgradeShop(); else TowerShop();
            if(Button("MUNIÇÃO +"+Run.AmmoPackAmount+"  /  "+Run.AmmoPackPrice,146,698,390,48,Mint,Run.wallet>=Run.AmmoPackPrice,17)) Run.BuyAmmo();
            if(Button("CURA 30%  /  "+Run.HealPrice,553,698,365,48,Hex("3D6550"),Run.wallet>=Run.HealPrice && Run.health<Run.character.health,17)) Run.Heal();
            if(Button("PRÓXIMA ONDA  >",1035,698,412,48,Gold,true,20)) Run.NextWave();
            Text("Reservas: "+Run.reserve+"  ·  Pente: "+Mathf.FloorToInt(Run.ammo)+" / "+Run.Capacity,149,759,720,22,13,Hex("A9C7B8"));
            Text("ENTER para continuar",1040,758,402,22,13,Hex("A9C7B8"),false,TextAnchor.MiddleCenter);
        }
        void WeaponShop()
        {
            Text("O mesmo arsenal para todos. Cada compra inclui dois pentes de reserva.",147,315,1260,28,17,Hex("B6CEC2"));
            for(int i=0;i<catalog.shopWeapons.Length;i++)
            {
                var w=catalog.shopWeapons[i]; float x=145+i*262; bool owned=Run.ownedWeapons.Contains(w),equipped=Run.weapon==w;
                Round(x,363,245,279,12,Hex("243F3F"));
                Text(w.title,x+15,381,215,58,23,Cream,true);
                Text(w.description,x+15,450,215,45,15,Hex("B6CEC2"));
                Text(w.damage+" DANO   /   "+w.capacity+" TIROS",x+15,516,217,24,12,Gold,true);
                Text(w.fireRate+" / s   ·   recarga "+w.reloadSeconds+"s",x+15,550,217,24,12,Hex("ABC8BE"));
                if(Button(equipped?"EQUIPADA":owned?"EQUIPAR":w.price+" MOEDAS",x+12,590,221,37,equipped?Mint:Gold,!equipped && (owned || Run.wallet>=w.price),14)) Run.BuyWeapon(i);
            }
        }
        void UpgradeShop()
        {
            Text("Melhorias acompanham você ao trocar de arma nesta partida.",147,315,1260,28,17,Hex("B6CEC2"));
            for(int i=0;i<4;i++)
            {
                float x=145+i*327; Round(x,366,310,240,12,Hex("243F3F"));
                Text(upgradeNames[i],x+18,388,274,35,25,Cream,true); Text(upgradeDescriptions[i],x+18,442,274,45,17,Hex("B6CEC2"));
                Text("NÍVEL "+Run.upgrades[i]+" / 5",x+18,509,274,25,14,Mint,true);
                if(Button(Run.upgrades[i]>=5?"MÁXIMO":Run.UpgradePrice(i)+" MOEDAS",x+15,549,280,40,Gold,Run.upgrades[i]<5 && Run.wallet>=Run.UpgradePrice(i),17)) Run.BuyUpgrade(i);
            }
            Text("Recarga atual: "+Run.ReloadSeconds.ToString("0.00")+" s  /  Capacidade: "+Run.Capacity+"  /  Dano: "+Run.Damage.ToString("0.00"),149,630,1254,27,17,Gold);
        }
        void TowerShop()
        {
            if(Button("LADO: "+RunManager.SideName(Run.viewSide)+"  [Q/E]",145,312,372,37,Hex("3D6056"),true,16)) Run.SwitchSide();
            Text("ESPAÇO",559,320,140,22,13,Mint,true);
            for(int i=0;i<3;i++) if(Button((i+1).ToString(),696+i*83,312,70,37,selectedSlot==i?Gold:Hex("304D4C"),true,16)) selectedSlot=i;
            int index=Run.SlotIndex(selectedSlot,Run.viewSide); var tower=Run.towers[index]; var barrier=Run.barricades[index];
            Text("Horda > barricada > torre > você",982,321,465,23,14,Gold,true,TextAnchor.UpperRight);
            if(tower==null)
            {
                for(int i=0;i<3;i++)
                {
                    float x=145+i*435; var t=catalog.towers[i]; Round(x,378,415,254,12,Hex("243F3F"));
                    Text(t.title,x+20,398,375,35,26,Cream,true); Text(t.description,x+20,453,375,43,18,Hex("B6CEC2"));
                    Text(t.health+" DE INTEGRIDADE",x+20,514,375,24,14,Mint,true);
                    if(Button("INSTALAR  /  "+t.price,x+18,565,379,45,Gold,Run.wallet>=t.price,17)) Run.BuyTower(i,selectedSlot);
                }
                Text("Instale uma torre para liberar a barricada à frente dela.",150,651,1290,27,16,Gold);
            }
            else
            {
                Round(145,378,630,272,12,Hex("243F3F")); Text(tower.data.title+"  ·  NÍVEL "+tower.level,166,398,588,40,26,Cream,true);
                Text("Integridade "+Mathf.CeilToInt(tower.health)+" / "+tower.maxHealth,166,457,588,27,18,Mint);
                if(Button("CONSERTO / 12",166,555,272,47,Mint,Run.wallet>=12 && tower.health<tower.maxHealth,16)) Run.ServiceTower(selectedSlot,false);
                if(Button("MELHORAR / "+25*tower.level,454,555,298,47,Gold,Run.wallet>=25*tower.level && tower.level<4,16)) Run.ServiceTower(selectedSlot,true);
                Round(796,378,650,272,12,Hex("243F3F")); Text("BARRICADA À FRENTE",817,398,607,40,25,Cream,true);
                if(barrier==null)
                {
                    Text("Protege esta torre. Ocupa um espaço separado e recebe os ataques primeiro.",817,457,602,64,18,Hex("B6CEC2"));
                    if(Button("INSTALAR / "+Run.TowerPrice(3),818,555,606,47,Gold,Run.wallet>=Run.TowerPrice(3),17)) Run.BuyTower(3,selectedSlot);
                }
                else
                {
                    Text("Integridade "+Mathf.CeilToInt(barrier.health)+" / "+barrier.maxHealth,817,457,606,27,18,Mint);
                    if(Button("CONSERTO / 12",817,555,288,47,Mint,Run.wallet>=12 && barrier.health<barrier.maxHealth,16)) Run.ServiceTower(selectedSlot,false,true);
                    if(Button("REFORÇO / "+25*barrier.level,1123,555,300,47,Gold,Run.wallet>=25*barrier.level && barrier.level<4,16)) Run.ServiceTower(selectedSlot,true,true);
                }
            }
        }
        void Pause()
        {
            Shade(); Panel(540,230,520,438); Tag("PODE RESPIRAR",660,260,280,Mint);
            Text("DEFESA EM PAUSA",565,319,470,62,36,Cream,true,TextAnchor.MiddleCenter);
            if(Button("CONTINUAR",590,412,420,56,Gold)) Run.paused=false;
            if(Button("RECOMEÇAR PARTIDA",590,487,420,48,Hex("345451"),true,17)) Run.Start(Run.demo);
            if(Button("VOLTAR AO MENU",590,554,420,48,Hex("345451"),true,17)) { Run.state=RunState.Menu; Run.paused=false; }
            Text("ESC para continuar",590,628,420,25,14,Hex("ABC3AD"),false,TextAnchor.MiddleCenter);
        }
        void GameOver()
        {
            Shade(); Panel(410,181,780,544); Tag(Run.newRecord?"NOVO RECORDE!":"A HORDA PASSOU",650,213,300,Gold);
            Text("A defesa caiu.",460,271,680,75,55,Cream,true,TextAnchor.MiddleCenter);
            Text("A próxima tentativa começa com uma nova estratégia.",460,352,680,38,20,Hex("B5CAB2"),false,TextAnchor.MiddleCenter);
            string[] labels={"ONDAS VENCIDAS","ZUMBIS ELIMINADOS","GEMAS GANHAS"}; int[] values={Run.completed,Run.kills,Run.demo?0:Run.earnedGems};
            for(int i=0;i<3;i++) { float x=453+i*234; Round(x,419,220,115,12,Hex("294744")); Text(values[i].ToString(),x,431,220,60,43,i==2?Gold:Mint,true,TextAnchor.MiddleCenter); Text(labels[i],x,503,220,21,11,Cream,true,TextAnchor.MiddleCenter); }
            if(Button("DEFENDER DE NOVO",453,570,454,58,Gold)) Run.Start();
            if(Button("MENU",930,570,220,58,Hex("345451"))) Run.state=RunState.Menu;
            Text(Run.demo?"Demonstrações não alteram seu progresso.":"Recorde e gemas salvos. Equipamentos reiniciam por partida.",453,657,697,28,15,Hex("B5CAB2"),false,TextAnchor.MiddleCenter);
        }
        void Help()
        {
            Shade(); Panel(300,110,1000,680); Tag("MANUAL DE SOBREVIVÊNCIA",590,135,420,Mint);
            Text("Cuide dos dois lados",340,184,920,60,43,Cream,true,TextAnchor.MiddleCenter);
            string[] titles={"01  DUAS CÂMERAS","02  PENTE E RESERVA","03  MOEDAS E ARSENAL","04  TORRES PROTEGIDAS","05  SUA HABILIDADE"};
            string[] details={"Q, E, Tab ou botão troca frente/costas. O radar mostra os dois lados. Alertas vermelhos indicam ataques fora da câmera.","Clique ou segure para disparar. Só recarrega ao esvaziar o pente, usando a reserva. Compre munição e recarga mais rápida na loja.","Nem todo zumbi solta moedas. Clique para coletar em 4 s sem gastar munição. As armas da loja são iguais para todos.","Cada lado tem três torres. A barricada só pode ser instalada no espaço à frente de uma torre. Conserte e melhore na loja.","Espaço ou botão direito ativa a habilidade. Cada personagem tem poder, vida, arma inicial e cenário próprios. Esc pausa."};
            for(int i=0;i<5;i++) { float y=272+i*78; Text(titles[i],340,y,330,27,18,Gold,true); Text(details[i],704,y,549,64,17,Cream); }
            if(Button("ENTENDI. VAMOS DEFENDER!",516,708,568,52,Gold)) help=false;
        }
    }
}
