using System;
using UnityEngine;
using UnityEditor;

namespace ZHold.Editor
{
    public static class MapProgressionValidation
    {
        static int checks;
        static void Check(bool value,string message) { if(!value)throw new Exception("MAP/PROGRESSION FAILED: "+message);checks++; }
        static void Step(RunManager run,float seconds) { for(int i=0;i<Mathf.CeilToInt(seconds/.02f);i++)run.Tick(.02f); }
        static RunManager Lumberjack(GameCatalog catalog)
        {
            var p=new PlayerProfile{selected=6};p.unlocked[6]=true;
            var run=new RunManager(catalog,p,721,persistProgress:false);run.Start();run.spawnTimer=999;return run;
        }
        static Zombie Enemy(RunManager run,int lane,ViewSide side=ViewSide.Front)
        {
            var z=new Zombie{data=run.catalog.zombies[0],side=side,lateral=(lane-1)*.66f,distance=.5f,health=100,maxHealth=100,speed=0};
            run.zombies.Add(z);return z;
        }
        public static void Run(GameCatalog catalog)
        {
            checks=0;
            foreach(string side in new[]{"Front","Back"})
            {
                var texture=Resources.Load<Texture2D>("Art/Maps-v5/"+side);
                Check(texture!=null,"new map atlas "+side);
                Check(texture.width/2f>=512&&texture.height/4f>=280,"map cells retain sufficient pixel detail "+side);
                Check(Mathf.Abs(texture.width/(float)texture.height-8f/9)<.02f,"16:9 map cell aspect "+side);
                Check(texture.filterMode==FilterMode.Point&&!texture.isReadable,"pixel import and GPU-only texture "+side);
            }
            for(int i=0;i<8;i++)
            {
                DepthProjector.Configure((MapTheme)i);
                Check((DepthProjector.Project(0,1)-new Vector2(800,400)).sqrMagnitude<.01f,"spawn row unchanged "+i);
                Check((DepthProjector.Project(.66f,0)-new Vector2(1235.6f,760)).sqrMagnitude<.01f,"near lane unchanged "+i);
                var uv=GameArt.BackgroundUV(i);
                Check(uv.x>=i%2*.5f&&uv.xMax<(i%2+1)*.5f&&uv.y>=1-(i/2+1)*.25f&&uv.yMax<1-i/2*.25f,"map framing stays within its cell "+i);
                Check(i==0?catalog.characters[i].price==0:catalog.characters[i].price>0,"recruit free and other characters priced "+i);
            }
            var fresh=SaveSystem.Migrate(new PlayerProfile());
            Check(fresh.version==3&&fresh.unlocked[0]&&Array.FindAll(fresh.unlocked,v=>v).Length==1,"new game only owns recruit");
            var migrated=SaveSystem.Migrate(new PlayerProfile{version=2,gems=345,bestWave=9,selected=6,unlocked=new[]{true,true,true,true,true,true,true,true}});
            Check(migrated.gems==345&&migrated.bestWave==9&&migrated.selected==0&&Array.FindAll(migrated.unlocked,v=>v).Length==1,"preview save preserves currency/record and introduces purchases");
            var run=new RunManager(catalog,fresh,12,persistProgress:false);
            var expectedOrder=new[]{0,5,1,2,3,4,6,7};var menuOrder=run.CharacterMenuOrder();
            for(int i=0;i<menuOrder.Length;i++)Check(menuOrder[i]==expectedOrder[i],"menu ordered by price "+i);
            string beforePreview=JsonUtility.ToJson(fresh);var equippedWeapon=run.weapon;
            for(int i=0;i<catalog.characters.Length;i++)
            {
                Check(run.PreviewCharacter(i)&&run.PreviewedCharacter==catalog.characters[i],"any character can be inspected "+i);
                Check(JsonUtility.ToJson(fresh)==beforePreview&&run.character==catalog.characters[0]&&run.weapon==equippedWeapon,"preview changes no gems, unlocks or equipped character "+i);
            }
            Check(!run.PreviewCharacter(-1)&&!run.PreviewCharacter(8)&&run.PreviewCharacterIndex==7,"invalid preview preserves current details");
            Check(!run.SelectCharacter(6)&&fresh.selected==0,"locked character cannot be selected");
            Check(!run.BuyCharacter(-1)&&!run.BuyCharacter(8)&&!run.BuyCharacter(0),"invalid purchases rejected");
            fresh.gems=catalog.characters[1].price-1;
            Check(!run.BuyCharacter(1)&&fresh.gems==catalog.characters[1].price-1&&!run.OwnsCharacter(1),"insufficient gems do not spend");
            fresh.gems=catalog.characters[1].price+17;
            Check(run.BuyCharacter(1)&&fresh.gems==17&&run.OwnsCharacter(1)&&fresh.selected==1&&run.weapon==catalog.characters[1].weapon,"purchase debits once, unlocks and equips");
            Check(!run.BuyCharacter(1)&&fresh.gems==17,"repeat purchase cannot charge");
            Check(run.SelectCharacter(0)&&run.SelectCharacter(1)&&fresh.gems==17,"owned characters equip freely");
            var loaded=SaveSystem.Migrate(JsonUtility.FromJson<PlayerProfile>(JsonUtility.ToJson(fresh)));
            Check(loaded.version==3&&loaded.gems==17&&loaded.selected==1&&loaded.unlocked[1]&&!loaded.unlocked[6],"owned characters survive serialized save and migration");
            var malformed=SaveSystem.Migrate(new PlayerProfile{unlocked=new[]{false,true},selected=7,gems=-8,bestWave=-4});
            Check(malformed.unlocked.Length==8&&malformed.unlocked[0]&&malformed.unlocked[1]&&malformed.selected==0&&malformed.gems==0&&malformed.bestWave==0,"short/corrupt save is normalized without losing purchases");
            fresh.gems=999;run.Start();
            Check(!run.BuyCharacter(6)&&!run.SelectCharacter(0)&&fresh.gems==999,"purchases and selection restricted to main menu");
            Check(!run.PreviewCharacter(6)&&run.character==catalog.characters[1],"preview cannot change a running match");
            for(int lane=0;lane<3;lane++)
            {
                run=Lumberjack(catalog);var targets=new[]{Enemy(run,0),Enemy(run,1),Enemy(run,2)};var hidden=Enemy(run,lane,ViewSide.Back);
                run.UseAbility(targets[(lane+1)%3].Position);
                Check(run.choosingTreeLane&&run.abilityCooldown==0&&run.abilityUses==0&&run.treeTimer==0,"first space only selects "+lane);
                Step(run,.3f);Check(targets[lane].health==100&&run.choosingTreeLane,"selection has no automatic damage "+lane);
                run.UseAbility(targets[lane].Position);
                Check(run.choosingTreeLane&&run.selectedTreeLane==-1&&run.abilityCooldown==0,"space cannot confirm without a click "+lane);
                Check(!run.SelectTreeLane(TreeSelectionLayout.HitTest(new Vector2(800,400)))&&run.selectedTreeLane==-1,"click outside lower panel selects nothing "+lane);
                var rect=TreeSelectionLayout.Button(lane);float ammo=run.ammo;run.Shoot(rect.center);
                Check(run.ammo==ammo&&run.thrown.Count==0,"selection click never throws the equipped axe "+lane);
                Check(run.SelectTreeLane(TreeSelectionLayout.HitTest(rect.center))&&run.selectedTreeLane==lane,"lower column button records click "+lane);
                run.UseAbility(targets[(lane+1)%3].Position);
                Check(!run.choosingTreeLane&&run.treeLane==lane&&run.treeTimer>0&&run.abilityCooldown>0&&run.abilityUses==1,"confirmation uses clicked lane despite pointer movement "+lane);
                Step(run,1.2f);Check(!run.zombies.Contains(targets[lane])&&run.zombies.Contains(hidden)&&run.zombies.Contains(targets[(lane+1)%3]),"only confirmed lane and side are cleared "+lane);
                run.UseAbility(Vector2.zero);Check(!run.choosingTreeLane&&run.abilityUses==1,"cooldown prevents rearming "+lane);
            }
            run=Lumberjack(catalog);var target=Enemy(run,1);run.UseAbility(target.Position);run.SelectTreeLane(2);run.SelectTreeLane(0);
            Check(run.selectedTreeLane==0,"a second column click changes the pending choice");run.CancelTreeSelection();
            Check(!run.choosingTreeLane&&run.selectedTreeLane==-1&&run.abilityCooldown==0&&run.abilityUses==0,"cancel clears choice without cooldown");
            run.UseAbility(target.Position);run.SwitchSide();
            Check(!run.choosingTreeLane&&run.abilityCooldown==0&&run.viewSide==ViewSide.Back,"camera switch cancels pending selection");
            run.UseAbility(target.Position);run.SelectTreeLane(1);run.paused=true;run.UseAbility(target.Position);
            Check(run.treeTimer==0&&run.abilityCooldown==0,"pause cannot confirm tree");run.paused=false;
            run.Start();Check(!run.choosingTreeLane,"restart clears tree selection");
            run.spawned=run.total;run.spawnTimer=999;run.UseAbility(Vector2.zero);Step(run,.1f);
            Check(run.state==RunState.WaveEnd&&!run.choosingTreeLane,"wave completion cancels tree selection");
            run=Lumberjack(catalog);run.StartTutorial();run.AdvanceTutorial(6);run.ContinueTutorial();run.UseAbility(Vector2.zero);Step(run,.1f);
            Check(run.tutorialStep==6&&run.abilityUses==0,"tutorial requires actual tree confirmation");
            run.SelectTreeLane(1);run.UseAbility(Vector2.zero);Step(run,2.2f);Check(run.tutorialStep==7,"tutorial advances after clicked and confirmed tree animation");
            Debug.Log("MAP_PROGRESSION_TESTS_PASSED: "+checks+" checks");
        }
    }
}
