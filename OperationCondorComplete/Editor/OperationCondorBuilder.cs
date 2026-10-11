#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class OperationCondorBuilder
{
    const string Root = "Assets/OperationCondorComplete";
    const string Gen = Root + "/Generated";
    static Font font;

    [MenuItem("Tools/Operation Condor/Build Complete Demo")]
    public static void BuildCompleteDemo()
    {
        try
        {
            PrepareAssets();
            if (AssetDatabase.IsValidFolder(Gen)) AssetDatabase.DeleteAsset(Gen);
            AssetDatabase.CreateFolder(Root, "Generated");
            AssetDatabase.CreateFolder(Gen, "Animations");
            AssetDatabase.CreateFolder(Gen, "Prefabs");
            AssetDatabase.Refresh();
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            int groundLayer = EnsureLayer("Ground");
            var playerAnimator = BuildPlayerAnimator();
            var enemyAnimator = BuildEnemyAnimator();
            var bulletPrefab = BuildBulletPrefab();
            BuildLevel(playerAnimator, enemyAnimator, bulletPrefab, groundLayer);
            BuildMainMenu();

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu_Complete.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Level01_Complete.unity", true)
            };
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Operation Condor", "Listo. Se crearon MainMenu_Complete y Level01_Complete, además de animaciones, prefabs, audio, parallax, UI, victoria/derrota y pausa. Abre MainMenu_Complete y presiona Play.", "OK");
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("Operation Condor - Error", ex.Message + "\nRevisa Console.", "OK");
        }
    }

    static void PrepareAssets()
    {
        string[] pngs = Directory.GetFiles(Root + "/Art", "*.png", SearchOption.AllDirectories);
        AssetDatabase.Refresh();
        foreach (var full in pngs)
        {
            string path = full.Replace('\\','/');
            int idx = path.IndexOf("Assets/"); if (idx >= 0) path = path.Substring(idx);
            var ti = AssetImporter.GetAtPath(path) as TextureImporter; if (ti == null) continue;
            ti.textureType = TextureImporterType.Sprite; ti.spriteImportMode = SpriteImportMode.Single; ti.alphaIsTransparency = true; ti.mipmapEnabled = false; ti.filterMode = FilterMode.Point; ti.textureCompression = TextureImporterCompression.Uncompressed;
            if (path.Contains("bg_")) ti.spritePixelsPerUnit = 100;
            else if (path.Contains("Player") || path.Contains("Enemy")) ti.spritePixelsPerUnit = 96;
            else if (path.Contains("ground_tile") || path.Contains("goal")) ti.spritePixelsPerUnit = 128;
            else if (path.Contains("platform_tile")) ti.spritePixelsPerUnit = 128;
            else if (path.Contains("bullet")) ti.spritePixelsPerUnit = 64;
            else ti.spritePixelsPerUnit = 100;
            ti.SaveAndReimport();
        }
        AssetDatabase.Refresh();
    }

    static Sprite S(string rel) => AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/" + rel);
    static AudioClip A(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>(Root + "/Audio/" + name);

    static AnimationClip Clip(string assetName, Sprite[] sprites, float fps, bool loop)
    {
        var clip = new AnimationClip { frameRate = fps };
        var bind = new EditorCurveBinding { type = typeof(SpriteRenderer), path = "", propertyName = "m_Sprite" };
        var keys = new ObjectReferenceKeyframe[sprites.Length];
        for(int i=0;i<sprites.Length;i++) keys[i] = new ObjectReferenceKeyframe { time=i/fps, value=sprites[i] };
        AnimationUtility.SetObjectReferenceCurve(clip, bind, keys);
        var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = loop; AnimationUtility.SetAnimationClipSettings(clip, settings);
        string path = Gen + "/Animations/" + assetName + ".anim"; AssetDatabase.CreateAsset(clip,path); return clip;
    }

    static AnimatorController BuildPlayerAnimator()
    {
        var idle = Clip("Player_Idle", new[]{S("Art/Player/player_idle.png")}, 4, true);
        var run = Clip("Player_Run", Enumerable.Range(0,4).Select(i=>S($"Art/Player/player_run_{i}.png")).ToArray(), 10, true);
        var jump = Clip("Player_Jump", new[]{S("Art/Player/player_jump.png")}, 4, false);
        var fall = Clip("Player_Fall", new[]{S("Art/Player/player_jump.png")}, 4, false);
        var shoot = Clip("Player_Shoot", new[]{S("Art/Player/player_shoot.png"),S("Art/Player/player_idle.png")}, 12, false);
        var hurt = Clip("Player_Hurt", new[]{S("Art/Player/player_hurt.png"),S("Art/Player/player_idle.png")}, 9, false);
        var dead = Clip("Player_Dead", new[]{S("Art/Player/player_dead.png")}, 2, false);
        string p=Gen+"/Animations/Player.controller"; var c=AnimatorController.CreateAnimatorControllerAtPath(p);
        c.AddParameter("Speed",AnimatorControllerParameterType.Float); c.AddParameter("Grounded",AnimatorControllerParameterType.Bool); c.AddParameter("YVelocity",AnimatorControllerParameterType.Float); c.AddParameter("Shoot",AnimatorControllerParameterType.Trigger); c.AddParameter("Hurt",AnimatorControllerParameterType.Trigger); c.AddParameter("Dead",AnimatorControllerParameterType.Bool);
        var sm=c.layers[0].stateMachine; var stIdle=sm.AddState("Idle"); stIdle.motion=idle; sm.defaultState=stIdle; var stRun=sm.AddState("Run");stRun.motion=run; var stJump=sm.AddState("Jump");stJump.motion=jump; var stFall=sm.AddState("Fall");stFall.motion=fall; var stShoot=sm.AddState("Shoot");stShoot.motion=shoot; var stHurt=sm.AddState("Hurt");stHurt.motion=hurt; var stDead=sm.AddState("Dead");stDead.motion=dead;
        T(stIdle,stRun,false,"Speed",AnimatorConditionMode.Greater,.1f); T(stRun,stIdle,false,"Speed",AnimatorConditionMode.Less,.1f);
        var aj=sm.AddAnyStateTransition(stJump); aj.hasExitTime=false; aj.duration=.03f; aj.AddCondition(AnimatorConditionMode.IfNot,0,"Grounded"); aj.AddCondition(AnimatorConditionMode.Greater,.1f,"YVelocity");
        var af=sm.AddAnyStateTransition(stFall); af.hasExitTime=false; af.duration=.03f; af.AddCondition(AnimatorConditionMode.IfNot,0,"Grounded"); af.AddCondition(AnimatorConditionMode.Less,-.1f,"YVelocity");
        T(stJump,stFall,false,"YVelocity",AnimatorConditionMode.Less,0f); var ji=T(stJump,stIdle,false,"Grounded",AnimatorConditionMode.If,0); ji.AddCondition(AnimatorConditionMode.Less,.1f,"Speed"); var jr=T(stJump,stRun,false,"Grounded",AnimatorConditionMode.If,0); jr.AddCondition(AnimatorConditionMode.Greater,.1f,"Speed"); var fi=T(stFall,stIdle,false,"Grounded",AnimatorConditionMode.If,0); fi.AddCondition(AnimatorConditionMode.Less,.1f,"Speed"); var fr=T(stFall,stRun,false,"Grounded",AnimatorConditionMode.If,0); fr.AddCondition(AnimatorConditionMode.Greater,.1f,"Speed");
        var ashoot=sm.AddAnyStateTransition(stShoot); ashoot.hasExitTime=false; ashoot.duration=.02f; ashoot.AddCondition(AnimatorConditionMode.If,0,"Shoot"); Exit(stShoot,stIdle,.85f);
        var ah=sm.AddAnyStateTransition(stHurt); ah.hasExitTime=false; ah.duration=.02f; ah.AddCondition(AnimatorConditionMode.If,0,"Hurt"); Exit(stHurt,stIdle,.85f);
        var ad=sm.AddAnyStateTransition(stDead); ad.hasExitTime=false; ad.duration=0; ad.AddCondition(AnimatorConditionMode.If,0,"Dead");
        return c;
    }
    static AnimatorController BuildEnemyAnimator()
    {
        var idle=Clip("Enemy_Idle",new[]{S("Art/Enemy/enemy_idle.png")},4,true); var run=Clip("Enemy_Run",Enumerable.Range(0,4).Select(i=>S($"Art/Enemy/enemy_run_{i}.png")).ToArray(),9,true); var hurt=Clip("Enemy_Hurt",new[]{S("Art/Enemy/enemy_hurt.png"),S("Art/Enemy/enemy_idle.png")},8,false); var dead=Clip("Enemy_Dead",new[]{S("Art/Enemy/enemy_dead.png")},2,false);
        var c=AnimatorController.CreateAnimatorControllerAtPath(Gen+"/Animations/Enemy.controller"); c.AddParameter("Speed",AnimatorControllerParameterType.Float); c.AddParameter("Hurt",AnimatorControllerParameterType.Trigger); c.AddParameter("Dead",AnimatorControllerParameterType.Bool);
        var sm=c.layers[0].stateMachine; var i=sm.AddState("Idle");i.motion=idle;sm.defaultState=i;var r=sm.AddState("Run");r.motion=run;var h=sm.AddState("Hurt");h.motion=hurt;var d=sm.AddState("Dead");d.motion=dead;T(i,r,false,"Speed",AnimatorConditionMode.Greater,.1f);T(r,i,false,"Speed",AnimatorConditionMode.Less,.1f);var ah=sm.AddAnyStateTransition(h);ah.hasExitTime=false;ah.AddCondition(AnimatorConditionMode.If,0,"Hurt");Exit(h,r,.8f);var ad=sm.AddAnyStateTransition(d);ad.hasExitTime=false;ad.AddCondition(AnimatorConditionMode.If,0,"Dead");return c;
    }
    static AnimatorStateTransition T(AnimatorState from,AnimatorState to,bool exit,string param,AnimatorConditionMode mode,float threshold){var t=from.AddTransition(to);t.hasExitTime=exit;t.duration=.04f;t.AddCondition(mode,threshold,param);return t;}
    static void Exit(AnimatorState from,AnimatorState to,float exitTime){var t=from.AddTransition(to);t.hasExitTime=true;t.exitTime=exitTime;t.duration=.03f;}

    static GameObject BuildBulletPrefab()
    {
        var go=new GameObject("OC_Bullet"); var sr=go.AddComponent<SpriteRenderer>();sr.sprite=S("Art/World/bullet.png");sr.sortingOrder=8;go.transform.localScale=Vector3.one*.55f;var rb=go.AddComponent<Rigidbody2D>();rb.gravityScale=0;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;var col=go.AddComponent<CircleCollider2D>();col.isTrigger=true;col.radius=.18f;go.AddComponent<OC_Bullet>();
        var prefab=PrefabUtility.SaveAsPrefabAsset(go,Gen+"/Prefabs/OC_Bullet.prefab");UnityEngine.Object.DestroyImmediate(go);return prefab;
    }

    static void BuildLevel(AnimatorController pc,AnimatorController ec,GameObject bulletPrefab,int groundLayer)
    {
        EnsureScenesFolder(); var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single); scene.name="Level01_Complete";
        var camGO=new GameObject("Main Camera");camGO.tag="MainCamera";var cam=camGO.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=5.4f;cam.backgroundColor=new Color(.04f,.09f,.18f);camGO.AddComponent<AudioListener>();camGO.transform.position=new Vector3(-5,0,-10);
        CreateBackground(camGO.transform);
        CreateGround(groundLayer); CreatePlatforms(groundLayer);
        var player=CreatePlayer(pc,bulletPrefab,groundLayer); camGO.AddComponent<OC_CameraFollow>().SetTarget(player.transform);
        CreateEnemy(new Vector3(1.5f,-1.75f,0),ec); CreateEnemy(new Vector3(7.5f,-1.75f,0),ec); CreateEnemy(new Vector3(13.5f,-1.75f,0),ec);
        CreateGoal(new Vector3(18.5f,-1.6f,0));
        var gm=CreateLevelUI(); SetupAudio(gm);
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Level01_Complete.unity");
    }

    static void CreateBackground(Transform cam)
    {
        MakeBG("Sky",S("Art/World/bg_sky.png"),new Vector3(0,.2f,7),-30,cam,0f);
        MakeBG("Mountains",S("Art/World/bg_mountains.png"),new Vector3(0,.2f,6),-20,cam,.12f);
        MakeBG("JungleOutpost",S("Art/World/bg_jungle.png"),new Vector3(0,.2f,5),-10,cam,.28f);
    }
    static void MakeBG(string name,Sprite s,Vector3 pos,int order,Transform cam,float factor){var g=new GameObject(name);g.transform.position=pos;g.transform.localScale=new Vector3(1.65f,1.12f,1f);var sr=g.AddComponent<SpriteRenderer>();sr.sprite=s;sr.sortingOrder=order;var p=g.AddComponent<OC_Parallax>();p.Configure(cam,factor);}
    static void CreateGround(int groundLayer)
    {
        var root=new GameObject("Ground"); root.layer=groundLayer;root.transform.position=new Vector3(3,-3.2f,0);var col=root.AddComponent<BoxCollider2D>();col.size=new Vector2(38,1.05f);col.offset=new Vector2(0,-.1f);
        var spr=S("Art/World/ground_tile.png");for(int i=-19;i<=19;i++){var t=new GameObject("GroundTile");t.transform.SetParent(root.transform);t.transform.localPosition=new Vector3(i,0,0);var sr=t.AddComponent<SpriteRenderer>();sr.sprite=spr;sr.sortingOrder=1;}
    }
    static void CreatePlatforms(int groundLayer){Platform(new Vector2(-4,-.4f),5,groundLayer);Platform(new Vector2(5,.4f),4,groundLayer);Platform(new Vector2(12,-.1f),5,groundLayer);}
    static void Platform(Vector2 pos,int tiles,int groundLayer){var r=new GameObject("Platform"); r.layer=groundLayer;r.transform.position=pos;var c=r.AddComponent<BoxCollider2D>();c.size=new Vector2(tiles, .48f);var s=S("Art/World/platform_tile.png");for(int i=0;i<tiles;i++){var t=new GameObject("P");t.transform.SetParent(r.transform);t.transform.localPosition=new Vector3(i-(tiles-1)/2f,0,0);var sr=t.AddComponent<SpriteRenderer>();sr.sprite=s;sr.sortingOrder=2;}}
    static GameObject CreatePlayer(AnimatorController controller,GameObject bullet,int groundLayer)
    {
        var p=new GameObject("Player");p.transform.position=new Vector3(-9,-1.7f,0);var sr=p.AddComponent<SpriteRenderer>();sr.sprite=S("Art/Player/player_idle.png");sr.sortingOrder=6;var rb=p.AddComponent<Rigidbody2D>();rb.gravityScale=3.2f;rb.freezeRotation=true;rb.collisionDetectionMode=CollisionDetectionMode2D.Continuous;var col=p.AddComponent<CapsuleCollider2D>();col.size=new Vector2(.48f,.92f);col.offset=new Vector2(0,-.02f);var an=p.AddComponent<Animator>();an.runtimeAnimatorController=controller;var health=p.AddComponent<OC_PlayerHealth>();var ctrl=p.AddComponent<OC_PlayerController>();
        var gc=new GameObject("GroundCheck").transform;gc.SetParent(p.transform);gc.localPosition=new Vector3(0,-.5f,0);var fp=new GameObject("FirePoint").transform;fp.SetParent(p.transform);fp.localPosition=new Vector3(.6f,.08f,0);ctrl.Configure(bullet,fp,gc,1 << groundLayer);
        return p;
    }
    static void CreateEnemy(Vector3 pos,AnimatorController controller)
    {
        var e=new GameObject("Enemy_Commando");e.transform.position=pos;var sr=e.AddComponent<SpriteRenderer>();sr.sprite=S("Art/Enemy/enemy_idle.png");sr.sortingOrder=6;var rb=e.AddComponent<Rigidbody2D>();rb.gravityScale=3.2f;rb.freezeRotation=true;var col=e.AddComponent<CapsuleCollider2D>();col.size=new Vector2(.48f,.92f);var an=e.AddComponent<Animator>();an.runtimeAnimatorController=controller;e.AddComponent<OC_EnemyHealth>();e.AddComponent<OC_EnemyPatrol>();
    }
    static void CreateGoal(Vector3 pos){var g=new GameObject("ExtractionTerminal");g.transform.position=pos;var sr=g.AddComponent<SpriteRenderer>();sr.sprite=S("Art/World/goal.png");sr.sortingOrder=4;var c=g.AddComponent<BoxCollider2D>();c.isTrigger=true;c.size=new Vector2(.7f,1.15f);g.AddComponent<OC_Goal>();}

    static GameObject CreateLevelUI()
    {
        var gm=new GameObject("GameSystems");var manager=gm.AddComponent<OC_GameManager>();SetupAudio(gm);
        var canvas=NewCanvas("HUD_Canvas");
        var hud=new GameObject("HUD",typeof(RectTransform));hud.transform.SetParent(canvas.transform,false);var hrt=hud.GetComponent<RectTransform>();Stretch(hrt);
        var back=UIRect("HealthBack",hud.transform,new Color(.03f,.05f,.08f,.88f),new Vector2(30,-30),new Vector2(300,34),new Vector2(0,1),new Vector2(0,1),new Vector2(0,1));
        var fill=UIRect("HealthFill",back.transform,new Color(.15f,.9f,.45f,1),Vector2.zero,new Vector2(292,26),new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(.5f,.5f)).GetComponent<Image>();fill.type=Image.Type.Filled;fill.fillMethod=Image.FillMethod.Horizontal;fill.fillOrigin=0;
        var status=UIText("Status",hud.transform,"VIDA 5/5     J: DISPARAR     ESPACIO: SALTAR",22,TextAnchor.UpperRight,new Vector2(-30,-30),new Vector2(720,40),new Vector2(1,1));status.color=Color.white;
        var mission=UIText("Mission",hud.transform,"MISIÓN: ALCANZA EL TERMINAL DE EXTRACCIÓN",21,TextAnchor.UpperCenter,new Vector2(0,-30),new Vector2(620,40),new Vector2(.5f,1));mission.color=new Color(.55f,.95f,1f);
        var go=PanelWithButtons(canvas.transform,"GameOverPanel","MISIÓN FALLIDA",new Color(.12f,.02f,.02f,.94f),out var retry1,out var menu1);
        var win=PanelWithButtons(canvas.transform,"VictoryPanel","MISIÓN COMPLETADA",new Color(.02f,.12f,.08f,.94f),out var retry2,out var menu2);
        var pause=PanelWithButtons(canvas.transform,"PausePanel","PAUSA",new Color(.02f,.04f,.09f,.92f),out var resume,out var menu3,"REANUDAR");
        go.SetActive(false);win.SetActive(false);pause.SetActive(false);manager.Configure(go,win,pause,fill,status);
        UnityEventTools.AddPersistentListener(retry1.onClick,manager.Restart);UnityEventTools.AddPersistentListener(retry2.onClick,manager.Restart);UnityEventTools.AddPersistentListener(menu1.onClick,manager.Menu);UnityEventTools.AddPersistentListener(menu2.onClick,manager.Menu);UnityEventTools.AddPersistentListener(resume.onClick,manager.Resume);UnityEventTools.AddPersistentListener(menu3.onClick,manager.Menu);
        return gm;
    }
    static GameObject PanelWithButtons(Transform canvas,string name,string title,Color color,out Button primary,out Button menu,string primaryText="REINTENTAR")
    {
        var p=UIRect(name,canvas,color,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.one,new Vector2(.5f,.5f));var rt=p.GetComponent<RectTransform>();rt.offsetMin=rt.offsetMax=Vector2.zero;
        var t=UIText("Title",p.transform,title,54,TextAnchor.MiddleCenter,new Vector2(0,120),new Vector2(800,90),new Vector2(.5f,.5f));t.fontStyle=FontStyle.Bold;t.color=new Color(.75f,.97f,1f);
        primary=UIButton(primaryText,p.transform,new Vector2(0,10));menu=UIButton("MENÚ PRINCIPAL",p.transform,new Vector2(0,-70));return p;
    }

    static void SetupAudio(GameObject host)
    {
        var am=host.GetComponent<OC_AudioManager>()??host.AddComponent<OC_AudioManager>();var so=new SerializedObject(am);so.FindProperty("shootClip").objectReferenceValue=A("shoot.wav");so.FindProperty("hitClip").objectReferenceValue=A("hit.wav");so.FindProperty("jumpClip").objectReferenceValue=A("jump.wav");so.FindProperty("winClip").objectReferenceValue=A("win.wav");so.FindProperty("loseClip").objectReferenceValue=A("lose.wav");so.FindProperty("clickClip").objectReferenceValue=A("click.wav");so.FindProperty("musicClip").objectReferenceValue=A("music_loop.wav");so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void BuildMainMenu()
    {
        EnsureScenesFolder();var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);scene.name="MainMenu_Complete";var camGO=new GameObject("Main Camera");camGO.tag="MainCamera";var cam=camGO.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=5.4f;cam.backgroundColor=new Color(.02f,.05f,.1f);camGO.AddComponent<AudioListener>();camGO.transform.position=new Vector3(0,0,-10);MakeBG("Sky",S("Art/World/bg_sky.png"),new Vector3(0,0,4),-30,camGO.transform,0);MakeBG("Mountains",S("Art/World/bg_mountains.png"),new Vector3(0,0,3),-20,camGO.transform,.05f);MakeBG("Jungle",S("Art/World/bg_jungle.png"),new Vector3(0,0,2),-10,camGO.transform,.1f);
        var sys=new GameObject("MenuSystems");SetupAudio(sys);var menu=sys.AddComponent<OC_MainMenu>();var canvas=NewCanvas("MainMenuCanvas");var shade=UIRect("Shade",canvas.transform,new Color(0,0,0,.35f),Vector2.zero,Vector2.zero,Vector2.zero,Vector2.one,new Vector2(.5f,.5f));var sr=shade.GetComponent<RectTransform>();sr.offsetMin=sr.offsetMax=Vector2.zero;
        var plate=UIRect("TitlePlate",canvas.transform,new Color(.03f,.08f,.15f,.94f),new Vector2(0,220),new Vector2(900,180),new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(.5f,.5f));var title=UIText("Title",plate.transform,"OPERATION CONDOR",64,TextAnchor.MiddleCenter,Vector2.zero,new Vector2(850,120),new Vector2(.5f,.5f));title.fontStyle=FontStyle.Bold;title.color=new Color(.45f,.95f,1f);
        var play=UIButton("JUGAR",canvas.transform,new Vector2(0,30));var opt=UIButton("OPCIONES",canvas.transform,new Vector2(0,-55));var quit=UIButton("SALIR",canvas.transform,new Vector2(0,-140));
        var options=UIRect("OptionsPanel",canvas.transform,new Color(.02f,.06f,.12f,.96f),new Vector2(0,-10),new Vector2(680,250),new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(.5f,.5f));UIText("OptionsText",options.transform,"OPCIONES\n\nVolumen general: usa el control del sistema\nESC pausa durante la partida",24,TextAnchor.MiddleCenter,Vector2.zero,new Vector2(620,170),new Vector2(.5f,.5f));var close=UIButton("CERRAR",options.transform,new Vector2(0,-85));options.SetActive(false);menu.Configure(options);
        UnityEventTools.AddPersistentListener(play.onClick,menu.Play);UnityEventTools.AddPersistentListener(opt.onClick,menu.Options);UnityEventTools.AddPersistentListener(quit.onClick,menu.Quit);UnityEventTools.AddPersistentListener(close.onClick,menu.Options);
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/MainMenu_Complete.unity");
    }

    static Canvas NewCanvas(string name){var go=new GameObject(name,typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var c=go.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;var sc=go.GetComponent<CanvasScaler>();sc.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;sc.referenceResolution=new Vector2(1920,1080);sc.matchWidthOrHeight=.5f;var es=new GameObject("EventSystem");es.AddComponent<UnityEngine.EventSystems.EventSystem>();es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();return c;}
    static GameObject UIRect(string name,Transform parent,Color color,Vector2 pos,Vector2 size,Vector2 amin,Vector2 amax,Vector2 pivot){var g=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));g.transform.SetParent(parent,false);var rt=g.GetComponent<RectTransform>();rt.anchorMin=amin;rt.anchorMax=amax;rt.pivot=pivot;rt.anchoredPosition=pos;if(amin!=amax && size==Vector2.zero){rt.offsetMin=rt.offsetMax=Vector2.zero;}else rt.sizeDelta=size;g.GetComponent<Image>().color=color;return g;}
    static Text UIText(string name,Transform parent,string value,int size,TextAnchor align,Vector2 pos,Vector2 rect,Vector2 anchor){var g=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));g.transform.SetParent(parent,false);var rt=g.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=anchor;rt.pivot=anchor;rt.anchoredPosition=pos;rt.sizeDelta=rect;var t=g.GetComponent<Text>();t.text=value;t.font=font;t.fontSize=size;t.alignment=align;t.color=Color.white;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;return t;}
    static Button UIButton(string label,Transform parent,Vector2 pos){var g=UIRect("Button_"+label,parent,new Color(.05f,.3f,.42f,.95f),pos,new Vector2(330,62),new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(.5f,.5f));var b=g.AddComponent<Button>();b.targetGraphic=g.GetComponent<Image>();var colors=b.colors;colors.highlightedColor=new Color(.12f,.65f,.78f);colors.pressedColor=new Color(.02f,.18f,.28f);b.colors=colors;var t=UIText("Label",g.transform,label,26,TextAnchor.MiddleCenter,Vector2.zero,new Vector2(300,52),new Vector2(.5f,.5f));t.fontStyle=FontStyle.Bold;return b;}
    static void Stretch(RectTransform rt){rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;}
    static int EnsureLayer(string layerName)
    {
        for(int i=0;i<32;i++) if(LayerMask.LayerToName(i)==layerName) return i;
        var tagManager=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers=tagManager.FindProperty("layers");
        for(int i=8;i<32;i++)
        {
            var sp=layers.GetArrayElementAtIndex(i);
            if(string.IsNullOrEmpty(sp.stringValue)){sp.stringValue=layerName;tagManager.ApplyModifiedProperties();return i;}
        }
        return 0;
    }
    static void EnsureScenesFolder(){if(!AssetDatabase.IsValidFolder("Assets/Scenes"))AssetDatabase.CreateFolder("Assets","Scenes");}
}
#endif
