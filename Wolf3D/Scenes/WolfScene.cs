using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using System;
using System.Linq;
using Wolf3D.Components;
using Wolf3D.Entities;
using Wolf3D.Renderers;
using Wolf3D.Util;
using System.IO;
using Newtonsoft.Json;
using Wolf3D.Weapons;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Wolf3D.Data;
using Wolf3D.Shaders;

namespace Wolf3D.Scenes
{

    public partial class WolfScene : Scene
    {
        bool alive = true;
        string levelName;
        int[,] worldMap;
        int[,] floorMap;
        int[,] ceilingMap;
        int[,] halfwallLR;
        int[,] halfwallUD;
        Data.Tiled.TiledObject[] objects;
        Vector2 spawnLocation;
        Direction spawnFacingDirection;

        PlayerEntity player;
        PlayerInfo playerInfo;
        PlayerState playerState;
        int w = NezGame.designWidth;
        int h = NezGame.h;
        int textureSize = 32;

        WolfSprite[] sprites;
        public WolfRenderer wolfRenderer;
        Sprite[] wallSprites;
        Color[][] SpriteColors;
        SpriteAnimator handsSprite;
        Stack<PaintersStripe> paintersStripes;

         Dictionary<string, string> levelToNextLevel;

        bool isEnded = false;


        Color[,] targetColors;

        TextComponent ammoCountText;
        TextComponent healthText;
        TextComponent restartText;
        SpriteRenderer hitEffectSprite;

        Entity playerUI;


        Dictionary<ValueTuple<int, int>, DoorMeta> doorDictionary;

        AsciiPostProcessor asciiPostProcessor;

        public WolfScene(string levelName)
        {
            this.levelName = levelName;

            levelToNextLevel = new Dictionary<string, string>();
            levelToNextLevel.Add("tiled-test", "tiled-test-2");
            levelToNextLevel.Add("tiled-test-2", "tiled-test");
            levelToNextLevel.Add("tiled-test-3", "tiled-test");

            playerInfo = new PlayerInfo(levelName);
        }
        public WolfScene(string levelName, PlayerInfo playerInfo)
        {
            this.levelName = levelName;

            levelToNextLevel = new Dictionary<string, string>();
            levelToNextLevel.Add("tiled-test", "tiled-test-2");
            levelToNextLevel.Add("tiled-test-2", "tiled-test-3");
            levelToNextLevel.Add("tiled-test-3", "tiled-test");

            this.playerInfo = playerInfo;
        }

        public override void Initialize()
        {
            Input.MouseLockState = MouseLockState.Locked;
            Core.Instance.IsMouseVisible = false;
            base.Initialize();
            this.ClearColor = new Color(90, 83, 83, 255);
            wolfRenderer = new WolfRenderer(0);
            AddRenderer(wolfRenderer);
            AddRenderer(new RenderLayerRenderer(1, 10));
            doorDictionary = new Dictionary<ValueTuple<int, int>, DoorMeta>();
            NezGame.PlayMusic(NezGame.world1Music);
            //asciiPostProcessor = new AsciiPostProcessor(999);
            //addPostProcessor(asciiPostProcessor);
        }


        public override void OnStart()
        {
            base.OnStart();
            CreatePauseMenu();
            HitEffectPool.clearCache();
            // load level info
            using (StreamReader reader = new StreamReader($"content/levels/{levelName}.json"))
            {
                string json = reader.ReadToEnd();
                var tiledMap = JsonConvert.DeserializeObject<Data.Tiled.TiledMap>(json);
                tiledMap.FillLayerDictionary();
                tiledMap.FixAllObjectPositions();
                //walls
                var walls = tiledMap.layerDictionary["wall"];
                walls.FillData2D();
                worldMap = walls.data2D;
                //floors
                var floors = tiledMap.layerDictionary["floor"];
                floors.FillData2D();
                floorMap = floors.data2D;
                //ceilings
                var ceilings = tiledMap.layerDictionary["ceiling"];
                ceilings.FillData2D();
                ceilingMap = ceilings.data2D;
                //halfwallLR
                var halfwallsLR = tiledMap.layerDictionary["halfwall-0"];
                halfwallsLR.FillData2D();
                halfwallLR = halfwallsLR.data2D;
                //halfwallUD
                var halfwallsUD = tiledMap.layerDictionary["halfwall-1"];
                halfwallsUD.FillData2D();
                halfwallUD = halfwallsUD.data2D;
                //doors --> need to integrate this into the worldmap?

                var doors = tiledMap.layerDictionary["door"];
                doors.FillData2D();
                var doorMap = doors.data2D;
                for (int x = 0; x < worldMap.GetLength(0); x++)
                {
                    for (int y = 0; y < worldMap.GetLength(1); y++)
                    {
                        if (doorMap[x, y] != 0)
                        {
                            var doorEntity = new Entity("door");
                            worldMap[x, y] = doorMap[x, y] + 100;
                            var door = new DoorMeta();
                            door.x = x;
                            door.y = y;
                            doorDictionary.Add(new ValueTuple<int, int>(x, y), door);

                            var moveCollider = new BoxCollider(10, 10);
                            Nez.Flags.SetFlagExclusive(ref moveCollider.PhysicsLayer, (int)Constants.PhysicsLayer.Wall);
                            Flags.SetFlagExclusive(ref moveCollider.CollidesWithLayers, (int)Constants.PhysicsLayer.Player);
                            Flags.SetFlag(ref moveCollider.CollidesWithLayers, (int)Constants.PhysicsLayer.Enemy);
                            Flags.SetFlag(ref moveCollider.CollidesWithLayers, (int)Constants.PhysicsLayer.Gib);
                            moveCollider.LocalOffset = new Vector2((x * 10f) + 5f, (y * 10f) + 5f);

                            var triggerCollider = new BoxCollider(25, 25);
                            triggerCollider.IsTrigger = true;
                            Nez.Flags.SetFlagExclusive(ref triggerCollider.PhysicsLayer, (int)Constants.PhysicsLayer.DoorTrigger);
                            Flags.SetFlagExclusive(ref triggerCollider.CollidesWithLayers, (int)Constants.PhysicsLayer.Player);
                            Flags.SetFlag(ref triggerCollider.CollidesWithLayers, (int)Constants.PhysicsLayer.Enemy);
                            triggerCollider.LocalOffset = new Vector2((x * 10f) + 5f, (y * 10f) + 5f);
                            door.wallCollider = moveCollider;
                            doorEntity.AddComponent(moveCollider);
                            doorEntity.AddComponent(triggerCollider);
                            doorEntity.AddComponent(door);
                            AddEntity(doorEntity);
                        }
                    }
                }
                //objects
                var objs = tiledMap.layerDictionary["objects"];
                objects = objs.objects;

                //levelEnd
                var endObj = tiledMap.layerDictionary["levelend"].objects.First();
                var endEntity = CreateEntity("levelEnd");
                endEntity.Position = endObj.position / (float)tiledMap.tileheight * 10f;
                var endCollider = new BoxCollider(10f, 10f);
                endCollider.IsTrigger = true;
                Nez.Flags.SetFlagExclusive(ref endCollider.PhysicsLayer, (int)Constants.PhysicsLayer.DoorTrigger);
                Nez.Flags.SetFlagExclusive(ref endCollider.CollidesWithLayers, (int)Constants.PhysicsLayer.Player);
                var endTrigger = new Components.LevelEndTrigger();
                endEntity.AddComponent(endCollider);
                endEntity.AddComponent(endTrigger);

                //playerspawn
                var spawnObj = tiledMap.layerDictionary["playerspawn"].objects.First();
                spawnObj.FillPropDictionary();
                spawnLocation = ((spawnObj.position / (float)tiledMap.tileheight) * 10f) + new Vector2(5f);
                var spawnFacingString = spawnObj.propertiesDictionary["facing"];
                switch (spawnFacingString.ToLower())
                {
                    case "right":
                        spawnFacingDirection = Direction.Right;
                        break;
                    case "left":
                        spawnFacingDirection = Direction.Left;
                        break;
                    case "up":
                        spawnFacingDirection = Direction.Up;
                        break;
                    case "down":
                        spawnFacingDirection = Direction.Down;
                        break;
                    default:
                        spawnFacingDirection = Direction.Right;
                        break;
                }
            }

            CreateCollidersForMap(worldMap);

            targetColors = new Color[NezGame.designHeight, NezGame.designWidth];
            wolfRenderer.colorBuffer = targetColors;
            wolfRenderer.zBuffer = new float[h, w];
            paintersStripes = new Stack<PaintersStripe>();

            var wallTexture = Content.Load<Texture2D>("img/Spacetiles");
            wallSprites = Sprite.SpritesFromAtlas(wallTexture, 32, 32).ToArray();
            var wallTextureColors = SpriteHelpers.GetTextureColors(wallSprites[0]);
            SpriteColors = new Color[wallSprites.Length][];
            for (int i = 0; i < wallSprites.Length; i++)
            {
                SpriteColors[i] = SpriteHelpers.GetColors(wallSprites[i], wallTextureColors);
            }
            var blobTexture = Content.Load<Texture2D>("img/blobman");
            var soliderTexture = Content.Load<Texture2D>("img/BadGuy");
            var soliderSprites = Sprite.SpritesFromAtlas(soliderTexture, 48, 48);
            var pickupsTexture = Content.Load<Texture2D>("img/pickups");
            var pickupsSprites = Sprite.SpritesFromAtlas(pickupsTexture, 32, 32);
            var gibTexutre = Content.Load<Texture2D>("img/gibs");
            var gibSprites = Sprite.SpritesFromAtlas(gibTexutre, 32, 32);
            var chargerTexture = Content.Load<Texture2D>("img/KorbisCharger");
            var chargerSprites = Sprite.SpritesFromAtlas(chargerTexture, 48, 48);

            playerUI = CreateEntity("playerUi");

            hitEffectSprite = new PrototypeSpriteRenderer(NezGame.designWidth * 2f, NezGame.designHeight * 2f);
            hitEffectSprite.Color = Color.White;
            hitEffectSprite.RenderLayer = 10;
            hitEffectSprite.Enabled = false;
            playerUI.AddComponent(hitEffectSprite);

            var hudBGTex = Content.Load<Texture2D>("img/KorbisUI2");
            var hudBGSprite = new SpriteRenderer(hudBGTex);
            //hudBGSprite.origin = Vector2.Zero;
            //hudBGSprite.LocalOffset = new Vector2(0, NezGame.designHeight - 72);
            hudBGSprite.RenderLayer = 10;
            var playerHudTex = Content.Load<Texture2D>("img/korbisHud");
            var playerHudSprites = Sprite.SpritesFromAtlas(playerHudTex, 64, 72);
            var playerUiSprite = new SpriteAnimator(playerHudSprites[0]);
            playerUiSprite.RenderLayer = 10;
            playerUI.AddComponent(hudBGSprite);
            playerUI.AddComponent(playerUiSprite);
            playerUI.Position = new Vector2(NezGame.designWidth / 2, NezGame.designHeight - (72 / 2));

            var ammoLabel = new TextComponent(Graphics.Instance.BitmapFont, "AMMO", new Vector2(-88, -20), Color.White);
            var hpLabel = new TextComponent(Graphics.Instance.BitmapFont, "HEALTH", new Vector2(88, -20), Color.White);
            ammoCountText = new TextComponent(Graphics.Instance.BitmapFont, "0", new Vector2(-88, 10), Color.White);
            healthText= new TextComponent(Graphics.Instance.BitmapFont, "0", new Vector2(88, 10), Color.White);
            restartText = new TextComponent(Graphics.Instance.BitmapFont, "SHOOT to restart", new Vector2(0, -NezGame.designHeight / 2), Color.White);
            restartText.Enabled = false;
            playerUI.AddComponent(ammoCountText);
            playerUI.AddComponent(healthText);
            playerUI.AddComponent(ammoLabel);
            playerUI.AddComponent(hpLabel);
            playerUI.AddComponent(restartText);
            ammoLabel.RenderLayer = 10;
            ammoLabel.FollowsParentEntityScale = false;
            ammoLabel.Scale = new Vector2(2, 2);
            ammoLabel.SetVerticalAlign(VerticalAlign.Center);
            ammoLabel.SetHorizontalAlign(HorizontalAlign.Center);
            hpLabel.RenderLayer = 10;
            hpLabel.FollowsParentEntityScale = false;
            hpLabel.Scale = new Vector2(2, 2);
            hpLabel.SetVerticalAlign(VerticalAlign.Center);
            hpLabel.SetHorizontalAlign(HorizontalAlign.Center);
            healthText.RenderLayer = 10;
            healthText.FollowsParentEntityScale = false;
            healthText.Scale = new Vector2(3, 3);
            healthText.SetVerticalAlign(VerticalAlign.Center);
            healthText.SetHorizontalAlign(HorizontalAlign.Center);
            restartText.RenderLayer = 10;
            restartText.FollowsParentEntityScale = false;
            restartText.Scale = new Vector2(2);
            restartText.SetVerticalAlign(VerticalAlign.Center);
            restartText.SetHorizontalAlign(HorizontalAlign.Center);
            ammoCountText.RenderLayer = 10;
            ammoCountText.FollowsParentEntityScale = false;
            ammoCountText.Scale = new Vector2(3, 3);
            ammoCountText.SetVerticalAlign(VerticalAlign.Center);
            ammoCountText.SetHorizontalAlign(HorizontalAlign.Center);

            var playerHurtSound = Content.Load<SoundEffect>("sfx/player_hit");
            player = new PlayerEntity(worldMap, playerUiSprite, playerHudSprites, healthText, ammoCountText, wolfRenderer, hitEffectSprite, new SoundEffect[]{ playerHurtSound });
            player.Position = spawnLocation;
            AddEntity(player);
            playerState = player.GetComponent<PlayerState>();
            foreach(AmmoType key in playerInfo.Ammo.Keys)
            {
                var val = playerInfo.Ammo[key];
                playerState.SetAmmo(key, val);
            }
            player.controller.equippedWeaponIndex = (int) playerInfo.equippedWeapon;
            if (playerInfo.Heath > 100)
            {
                var diff = playerInfo.Heath - 100;
                player.Hit(-diff);// should just add hp on start
            }
            switch (spawnFacingDirection)
            {
                case Direction.Right:
                    break;
                case Direction.Down:
                    playerState.rotate(Mathf.PI / 2f);
                    break;
                case Direction.Up:
                    playerState.rotate(3f / 2f * Mathf.PI);
                    break;

            }
            HitEffectPool.warmCache(10, this, playerState);

            //loop objects to create enemies/scenery
            for (int i = 0; i < objects.Length; i++)
            {
                var obj = objects[i];

                switch (obj.name.ToLower())
                {
                    case "basic":
                        var basic = new BasicEnemyEntity(playerState, new Sprite(blobTexture), worldMap);
                        basic.Position = (obj.position / 32f * 10f) + new Vector2(5f);
                        AddEntity(basic);
                        break;
                    case "soldier":
                        var soliderHitSoundEffect = Content.Load<SoundEffect>("sfx/goblin-hurt-1");
                        var soliderDeadSoundEffect = Content.Load<SoundEffect>("sfx/goblin-dead");
                        var soliderGibSoundEffect = Content.Load<SoundEffect>("sfx/splat1");
                        var solider = new SoliderEnemyEntity(playerState, soliderSprites, worldMap, gibSprites, new List<SoundEffect>() { soliderHitSoundEffect, soliderDeadSoundEffect, soliderGibSoundEffect });
                        solider.Position = (obj.position / 32f * 10f) + new Vector2(5f);
                        AddEntity(solider);
                        break;
                    case "soldier2":
                        var solider2HitSoundEffect = Content.Load<SoundEffect>("sfx/goblin-hurt-1");
                        var solider2DeadSoundEffect = Content.Load<SoundEffect>("sfx/goblin-dead");
                        var solider2GibSoundEffect = Content.Load<SoundEffect>("sfx/splat1");
                        var solider2 = new ShotgunSoldierEnemyEntity(playerState, soliderSprites, worldMap, gibSprites, new List<SoundEffect>() { solider2HitSoundEffect, solider2DeadSoundEffect, solider2GibSoundEffect });
                        solider2.Position = (obj.position / 32f * 10f) + new Vector2(5f);
                        AddEntity(solider2);
                        break;
                    case "spiker":
                        var spikerHitSoundEffect = Content.Load<SoundEffect>("sfx/goblin-hurt-1");
                        var spikerDeadSoundEffect = Content.Load<SoundEffect>("sfx/goblin-dead");
                        var spikerGibSoundEffect = Content.Load<SoundEffect>("sfx/splat1");
                        var spiker = new SpikerEnemyEntity(playerState, soliderSprites, worldMap, gibSprites, new List<SoundEffect>() { spikerHitSoundEffect, spikerDeadSoundEffect, spikerGibSoundEffect });
                        spiker.Position = (obj.position / 32f * 10f) + new Vector2(5f);
                        AddEntity(spiker);
                        break;
                    case "charger":
                        var chargerHitSoundEffect = Content.Load<SoundEffect>("sfx/goblin-hurt-1");
                        var chargerDeadSoundEffect = Content.Load<SoundEffect>("sfx/goblin-dead");
                        var chargerGibSoundEffect = Content.Load<SoundEffect>("sfx/splat1");
                        var charger = new ChargerEntity(playerState, chargerSprites, worldMap, gibSprites, new List<SoundEffect>() { chargerHitSoundEffect, chargerDeadSoundEffect, chargerGibSoundEffect });
                        charger.Position = (obj.position / 32f * 10f) + new Vector2(5f);
                        AddEntity(charger);
                        break;

                    case "barrel":
                        var barrel = new SceneryEntity(SceneryType.barrel, wallTexture, playerState);
                        barrel.Position = (obj.position / 32f * 10f) + new Vector2(5f);
                        AddEntity(barrel);
                        break;
                    case "pistol-ammo":
                        var pistolPickupSound = Content.Load<SoundEffect>("sfx/cloth");
                        var pistolAmmoSprite = new WolfSprite(playerState);
                        pistolAmmoSprite.SetSprite(pickupsSprites.Skip(3).FirstOrDefault());
                        var pistolAmmo = new AmmoEntity(AmmoType.Pistol, 10, pistolAmmoSprite, pistolPickupSound);
                        pistolAmmo.Position = (obj.position / 32f * 10f) + new Vector2(5f);
                        AddEntity(pistolAmmo);
                        break;
                    case "shotgun-ammo":
                        var shotgunPickupSound = Content.Load<SoundEffect>("sfx/cloth");
                        var shotgunAmmoSprite = new WolfSprite(playerState);
                        shotgunAmmoSprite.SetSprite(pickupsSprites.Skip(2).FirstOrDefault());
                        var shotgunAmmo = new AmmoEntity(AmmoType.Shotgun, 10, shotgunAmmoSprite, shotgunPickupSound);
                        shotgunAmmo.Position = (obj.position / 32f * 10f) + new Vector2(5f);
                        AddEntity(shotgunAmmo);
                        break;
                    case "chaingun-ammo":
                        var ChaingunPickupSound = Content.Load<SoundEffect>("sfx/cloth");
                        var ChaingunAmmoSprite = new WolfSprite(playerState);
                        ChaingunAmmoSprite.SetSprite(pickupsSprites.Skip(5).FirstOrDefault());
                        var ChaingunAmmo = new AmmoEntity(AmmoType.MachineGun, 20, ChaingunAmmoSprite, ChaingunPickupSound);
                        ChaingunAmmo.Position = (obj.position / 32f * 10f) + new Vector2(5f);
                        AddEntity(ChaingunAmmo);
                        break;
                    case "explosive-ammo":
                        var explosivePickupSound = Content.Load<SoundEffect>("sfx/cloth");
                        var explosiveAmmoSprite = new WolfSprite(playerState);
                        explosiveAmmoSprite.SetSprite(pickupsSprites.Skip(5).FirstOrDefault());
                        var explosiveAmmo = new AmmoEntity(AmmoType.Explosive, 20, explosiveAmmoSprite, explosivePickupSound);
                        explosiveAmmo.Position = (obj.position / 32f * 10f) + new Vector2(5f);
                        AddEntity(explosiveAmmo);
                        break;
                    case "disc-ammo":
                        var discPickupSound = Content.Load<SoundEffect>("sfx/cloth");
                        var discAmmoSprite = new WolfSprite(playerState);
                        discAmmoSprite.SetSprite(pickupsSprites.Skip(5).FirstOrDefault());
                        var discAmmo = new AmmoEntity(AmmoType.Disc, 40, discAmmoSprite, discPickupSound);
                        discAmmo.Position = (obj.position / 32f * 10f) + new Vector2(5f);
                        AddEntity(discAmmo);
                        break;
                    case "health":
                        var healthPickupSound = Content.Load<SoundEffect>("sfx/cloth");
                        var healthSprite = new WolfSprite(playerState);
                        healthSprite.SetSprite(pickupsSprites.Skip(1).FirstOrDefault());
                        var health = new HealthEntity(10, healthSprite, healthPickupSound);
                        health.Position = (obj.position / 32f * 10f) + new Vector2(5f);
                        AddEntity(health);
                        break;

                }
            }


            sprites = this.Entities.FindComponentsOfType<WolfSprite>().ToArray();

            wolfRenderer.sprites = sprites;


            var handsTex = Content.Load<Texture2D>("img/hands");

            var playerHandsSprites = Sprite.SpritesFromAtlas(handsTex, 360, 48);

            var pistolIdle = new SpriteAnimation(new Sprite[] { playerHandsSprites[0] }, 12);

            var pistolShoot = new SpriteAnimation(playerHandsSprites.Skip(1).Take(2).Concat(new List<Sprite>() { playerHandsSprites[0] }).ToArray(), 12);

            var chaingunIdle = new SpriteAnimation(playerHandsSprites.Skip(8).Take(3).Reverse().ToArray(), 12);

            var chainGunShoot = new SpriteAnimation(playerHandsSprites.Skip(11).Take(2).ToArray(), 12);

            var shottyIdle = new SpriteAnimation(playerHandsSprites.Skip(3).Take(1).ToArray(), 12);

            var shottyShoot = new SpriteAnimation(playerHandsSprites.Skip(4).Take(4).Concat(new List<Sprite>() { playerHandsSprites[3] }).ToArray(), 12);

            var melee = new SpriteAnimation(playerHandsSprites.Skip(14).Take(6).ToArray(), 12);


            handsSprite = new SpriteAnimator();

            handsSprite.RenderLayer = 10;

            handsSprite.AddAnimation(Constants.HandStates.PISTOL_IDLE, pistolIdle);
            handsSprite.AddAnimation(Constants.HandStates.PISTOL_SHOOT, pistolShoot);

            handsSprite.AddAnimation(Constants.HandStates.CHAINGUN_IDLE, chaingunIdle);

            handsSprite.AddAnimation(Constants.HandStates.CHAINGUN_SHOOT, chainGunShoot);
            handsSprite.AddAnimation(Constants.HandStates.SHOTGUN_IDLE, shottyIdle);
            handsSprite.AddAnimation(Constants.HandStates.SHOTGUN_SHOOT, shottyShoot);
            handsSprite.AddAnimation(Constants.HandStates.MELEE_SHOOT, melee);

            handsSprite.LocalOffset = new Vector2(0, -87);

            handsSprite.Scale = new Vector2(2f);

            handsSprite.FollowsParentEntityScale = false;

            playerUI.AddComponent(handsSprite);

            player.SetHandsSprite(handsSprite);

        }

        public override void Update()
        {
            base.Update();
            if (!alive)
            {
                if(Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Space) || Input.LeftMouseButtonPressed)
                {
                    RestartLevel();
                }
            }
            if (Nez.Input.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                Pause();
            }
            if (alive && Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Z))
            {
                wolfRenderer.Tween("verticalOffset", -25f, 0.1f).Start();
                //wolfRenderer.verticalOffset = -25f;
            }
            else if(alive)
            {
                wolfRenderer.verticalOffset = Mathf.Approach(wolfRenderer.verticalOffset, 0f, 25f * Time.DeltaTime);
            }

            float offsetPositionX = playerState.RayCasterPosition.X + (wolfRenderer.screenShakeOffset.X / 10f);
            float offsetPositionY = playerState.RayCasterPosition.Y + (wolfRenderer.screenShakeOffset.Y / 10f);


            //FLOOR CASTING
            for (int y = 0; y < h; y++)
            {
                // rayDir for leftmost ray (x = 0) and rightmost ray (x = w)
                float rayDirX0 = playerState.Direction.X - playerState.Plane.X;
                float rayDirY0 = playerState.Direction.Y - playerState.Plane.Y;
                float rayDirX1 = playerState.Direction.X + playerState.Plane.X;
                float rayDirY1 = playerState.Direction.Y + playerState.Plane.Y;

                bool isFloor = y > h / 2;
                // Current y position compared to the center of the screen (the horizon)
                int p = isFloor ? (y - h / 2) : (h / 2 - y);

                // Vertical position of the camera.
                float posZ = isFloor ? (0.5f * h + wolfRenderer.totalVerticalOffset) : (0.5f * h - wolfRenderer.totalVerticalOffset);

                // Horizontal distance from the camera to the floor for the current row.
                // 0.5 is the z position exactly in the middle between floor and ceiling.
                float rowDistance = posZ / p;

                // calculate the real world step vector we have to add for each x (parallel to camera plane)
                // adding step by step avoids multiplications with a weight in the inner loop
                float floorStepX = rowDistance * (rayDirX1 - rayDirX0) / w;
                float floorStepY = rowDistance * (rayDirY1 - rayDirY0) / w;

                // real world coordinates of the leftmost column. This will be updated as we step to the right.
                float floorX = offsetPositionX + rowDistance * rayDirX0;
                float floorY = offsetPositionY + rowDistance * rayDirY0;

                for (int x = 0; x < w; ++x)
                {
                    // the cell coord is simply got from the integer parts of floorX and floorY
                    int cellX = (int)floorX;
                    int cellY = (int)floorY;

                    // get the texture coordinate from the fractional part
                    int tx = (int)(textureSize * (floorX - cellX)) & (textureSize - 1);
                    int ty = (int)(textureSize * (floorY - cellY)) & (textureSize - 1);

                    floorX += floorStepX;
                    floorY += floorStepY;

                    // choose texture and draw the pixel
                    int floorTexture = 5;
                    int ceilingTexture = 3;

                    if (isFloor && cellX >= 0 && cellX < floorMap.GetLength(0) && cellY >= 0 && cellY < floorMap.GetLength(1))
                    {
                        floorTexture = floorMap[cellX, cellY] - 1;
                    }
                    else if (!isFloor && cellX >= 0 && cellX < ceilingMap.GetLength(0) && cellY >= 0 && cellY < ceilingMap.GetLength(1))
                    {
                        ceilingTexture = ceilingMap[cellX, cellY] - 1;
                    }

                    Color color = Color.White;
                    if (isFloor)
                    {
                        // floor
                        color = SpriteColors[floorTexture][textureSize * ty + tx];
                        targetColors[y, x] = color;
                        wolfRenderer.zBuffer[y, x] = 1000; //default the z buffer values to WAY out there (no need to put on separate loop & use more cpu)
                    }
                    else
                    {
                        //ceiling
                        color = SpriteColors[ceilingTexture][textureSize * ty + tx];
                        targetColors[y, x] = color;
                        wolfRenderer.zBuffer[y, x] = 1000; //default the z buffer values to WAY out there (no need to put on separate loop & use more cpu)
                    }
                }
            }
            
            //Wall Casting
            for (int x = 0; x < w; x++)
            {
                paintersStripes.Clear();
                //calculate ray position and direction
                float cameraX = 2f * x / w - 1f; //x-coordinate in camera space
                float rayDirX = playerState.Direction.X + playerState.Plane.X * cameraX;
                float rayDirY = playerState.Direction.Y + playerState.Plane.Y * cameraX;
                //which box of the map we're in
                int mapX = (int)offsetPositionX;
                int mapY = (int)offsetPositionY;

                if (mapX < 0) mapX = 0;
                if (mapX >= w) mapX = w - 1;
                if (mapY < 0) mapY = 0;
                if (mapY >= h) mapY = h - 1;

                //length of ray from current position to next x or y-side
                float sideDistX;
                float sideDistY;

                //length of ray from one x or y-side to next x or y-side
                float deltaDistX = Mathf.Sqrt(1f + (rayDirY * rayDirY) / (rayDirX * rayDirX));
                float deltaDistY = Mathf.Sqrt(1f + (rayDirX * rayDirX) / (rayDirY * rayDirY));
                float perpWallDist;

                //what direction to step in x or y-direction (either +1 or -1)
                int stepX;
                int stepY;

                int hit = 0; //was there a wall hit?
                int side = 0; //was a NS or a EW wall hit?

                //calculate step and initial sideDist
                if (rayDirX < 0)
                {
                    stepX = -1;
                    sideDistX = (offsetPositionX - mapX) * deltaDistX;
                }
                else
                {
                    stepX = 1;
                    sideDistX = (mapX + 1.0f - offsetPositionX) * deltaDistX;
                }
                if (rayDirY < 0)
                {
                    stepY = -1;
                    sideDistY = (offsetPositionY - mapY) * deltaDistY;
                }
                else
                {
                    stepY = 1;
                    sideDistY = (mapY + 1.0f - offsetPositionY) * deltaDistY;
                }
                int mapVal = 0;
                bool isDoor = false;
                bool isSolid = true;
                PaintersStripe paintersStripe;
                //perform DDA
                while (hit == 0)
                {
                    paintersStripe = new PaintersStripe();
                    //jump to next map square, OR in x-direction, OR in y-direction
                    if (sideDistX < sideDistY)
                    {
                        sideDistX += deltaDistX;
                        mapX += stepX;
                        side = 0;
                    }
                    else
                    {
                        sideDistY += deltaDistY;
                        mapY += stepY;
                        side = 1;
                    }
                    if (mapX < 0) mapX = 0;
                    if (mapX >= w) mapX = w - 1;
                    if (mapY < 0) mapY = 0;
                    if (mapY >= h) mapY = h - 1;

                    mapVal = worldMap[mapX, mapY];
                    int halfUDVal = halfwallUD[mapX, mapY];
                    int halfLRVal = halfwallLR[mapX, mapY];
                    //Check if ray has hit a wall
                    if (mapVal > 0)
                    {
                        isDoor = mapVal >= 100;
                        isSolid = !isDoor;//later we will want this for non-solid textures
                        if (mapVal == 7) isSolid = false;
                        paintersStripe.mapX = mapX;
                        paintersStripe.mapY = mapY;
                        paintersStripe.sideDistX = sideDistX;
                        paintersStripe.sideDistY = sideDistY;
                        paintersStripe.side = side;
                        paintersStripe.textureMapValue = mapVal;
                        paintersStripe.isSolid = isSolid;
                        paintersStripe.isDoor = isDoor;
                        paintersStripes.Push(paintersStripe);

                        hit = isSolid ? 1 : 0;
                    }
                    else if (halfUDVal > 0)
                    {
                        isDoor = false;
                        isSolid = true;//later we will want this for non-solid textures
                        if (halfUDVal == 7) isSolid = false;
                        paintersStripe.mapX = mapX;
                        paintersStripe.mapY = mapY;
                        paintersStripe.sideDistX = sideDistX;
                        paintersStripe.sideDistY = sideDistY;
                        paintersStripe.side = side;
                        paintersStripe.textureMapValue = halfUDVal;
                        paintersStripe.isSolid = isSolid;
                        paintersStripe.isDoor = isDoor;
                        paintersStripe.isHalfWall = true;
                        paintersStripes.Push(paintersStripe);

                        hit = isSolid ? 1 : 0;
                    }
                    else if (halfLRVal > 0)
                    {
                        isDoor = false;
                        isSolid = true;//later we will want this for non-solid textures
                        if (halfLRVal == 7) isSolid = false;
                        paintersStripe.mapX = mapX;
                        paintersStripe.mapY = mapY;
                        paintersStripe.sideDistX = sideDistX;
                        paintersStripe.sideDistY = sideDistY;
                        paintersStripe.side = side;
                        paintersStripe.textureMapValue = halfLRVal;
                        paintersStripe.isSolid = isSolid;
                        paintersStripe.isDoor = isDoor;
                        paintersStripe.isHalfWall = true;
                        paintersStripes.Push(paintersStripe);

                        hit = isSolid ? 1 : 0;
                    }
                    // door stuff thanks to https://gist.github.com/Powersaurus/196aa4a9bc3bdffc24453c51830ce8a7 
                    // and https://medium.com/@Powersaurus/pico-8-raycaster-doors-cd8de9d943b
                    if (paintersStripe.isDoor || paintersStripe.isHalfWall)
                    {
                        hit = 0;
                        int mapX2 = paintersStripe.mapX;
                        int mapY2 = paintersStripe.mapY;
                        if ((int)offsetPositionX < mapX2) mapX2 -= 1;
                        if ((int)offsetPositionY > mapY2) mapY2 += 1;
                        float adj = 1;
                        float ray_mult = 1f;
                        if (paintersStripe.side == 1)
                        {
                            adj = mapY2 - offsetPositionY;
                            ray_mult = adj / rayDirY;
                        }
                        else
                        {
                            adj = mapX2 - offsetPositionX + 1;
                            ray_mult = adj / rayDirX;
                        }

                        float rxe = offsetPositionX + (rayDirX * ray_mult);
                        float rye = offsetPositionY + (rayDirY * ray_mult);
                        // vertical door (north/south)
                        bool pop = false;
                        if (paintersStripe.side == 0)
                        {
                            float ystep2 = Mathf.Sqrt((deltaDistX * deltaDistX) - 1);
                            int tempY = (int)(rye + (stepY * ystep2) / 2);
                            if (tempY != paintersStripe.mapY)
                            {
                                pop = true;
                            }
                        }
                        else
                        {
                            float xstep2 = Mathf.Sqrt((deltaDistY * deltaDistY) - 1);
                            int tempX = (int)(rxe + (stepX * xstep2) / 2);

                            if (tempX != paintersStripe.mapX)
                            {
                                pop = true;
                            }
                        }
                        if (pop)
                        {
                            paintersStripe = new PaintersStripe();
                            paintersStripes.Pop();
                        }
                    }
                }
                int count = paintersStripes.Count;
                int counter = 0;
                while (counter < count)
                {
                    counter++;
                    var stripe = paintersStripes.Pop();
                    //texturing calculations
                    if (stripe.isDoor)
                    {
                        stripe.textureMapValue -= 100;
                    }
                    int texNum = stripe.textureMapValue - 1; //1 subtracted from it so that texture 0 can be used
                    if (texNum < 0) texNum = 0; //why?


                    //Calculate distance projected on camera direction (Euclidean distance will give fisheye effect!)
                    if (stripe.side == 0)
                    {
                        float doorStep = 0f;
                        if (stripe.isDoor || stripe.isHalfWall) doorStep += stepX / 2f;
                        perpWallDist = ((stripe.mapX + doorStep) - offsetPositionX + (1f - stepX) / 2f) / rayDirX;
                    }
                    else
                    {
                        float doorStep = 0f;
                        if (stripe.isDoor || stripe.isHalfWall) doorStep += stepY / 2f;
                        perpWallDist = ((stripe.mapY + doorStep) - offsetPositionY + (1f - stepY) / 2f) / rayDirY;
                    }

                    //Calculate height of line to draw on screen
                    int lineHeight = (int)(h / perpWallDist);

                    //calculate lowest and highest pixel to fill in current stripe
                    int drawStart = -lineHeight / 2 + h / 2 + (int)(wolfRenderer.totalVerticalOffset / perpWallDist);
                    if (drawStart < 0) drawStart = 0;
                    int drawEnd = lineHeight / 2 + h / 2 + (int)(wolfRenderer.totalVerticalOffset / perpWallDist);
                    if (drawEnd >= h) drawEnd = h - 1;

                    //choose wall color
                    Color color = Color.White;


                    //calculate value of wallX
                    float wallX; //where exactly the wall was hit
                    if (stripe.side == 0) wallX = offsetPositionY + perpWallDist * rayDirY;
                    else wallX = offsetPositionX + perpWallDist * rayDirX;
                    wallX -= Mathf.Floor(wallX);

                    //x coordinate on the texture
                    int texX = (int)(wallX * textureSize);
                    if (stripe.side == 0 && rayDirX > 0) texX = textureSize - texX - 1;
                    if (stripe.side == 1 && rayDirY < 0) texX = textureSize - texX - 1;

                    bool doorHack = false;
                    if (stripe.isDoor)
                    {
                        ////WHY ARE DOOR TEXTURES ALL GOOFY NOW WTF?
                        var door = doorDictionary[new ValueTuple<int, int>(stripe.mapX, stripe.mapY)];
                        var pct = door.closedPct;
                        texX += (int)((1f - pct) * textureSize);
                        if (texX > textureSize - 1)
                        {
                            doorHack = true;
                        }
                    }
                    if (!doorHack)
                    {
                        // How much to increase the texture coordinate per screen pixel
                        float step = 1.0f * textureSize / lineHeight;
                        // Starting texture coordinate
                        float texPos = (drawStart - (wolfRenderer.totalVerticalOffset / perpWallDist) - h / 2 + lineHeight / 2) * step;
                        for (int y = drawStart; y < drawEnd; y++)
                        {
                            // Cast the texture coordinate to integer, and mask with (texHeight - 1) in case of overflow
                            int texY = (int)texPos & (textureSize - 1);
                            texPos += step;
                            color = SpriteColors[texNum][textureSize * texY + texX];
                            if (color.A != Color.Transparent.A)
                            {
                                //give x and y sides different brightness
                                if (side == 1)
                                {
                                    ColorExt.Lerp(color, Color.Black, 0.25f);
                                }
                                targetColors[y, x] = color;
                                wolfRenderer.zBuffer[y, x] = perpWallDist;
                            }

                        }

                    }

                }

            }


            wolfRenderer.Update(); // updating screenshake & sprite ordering stuff

        }


        public void CreateCollidersForMap(int[,] map)
        {
            var wallsEntity = new Entity("wall");
            int test = -1;

            // create a collider for this location if it is a non-zero
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    var mapVal = map[i, j];
                    var halfWallLR = halfwallLR[i, j];
                    var halfWallUD = halfwallUD[i, j];
                    if (mapVal > 0 && mapVal < 100)
                    {
                        var x = i * 10f;
                        var y = j * 10f;
                        var collider = new BoxCollider(10f, 10f);
                        collider.LocalOffset = new Vector2(x, y) + new Vector2(5f); // additional 5f,5f is to deal with the center origin
                        Nez.Flags.SetFlagExclusive(ref collider.PhysicsLayer, (int)Constants.PhysicsLayer.Wall);
                        if (mapVal == 11) // need to change this to more elaborate semi-solid checks
                        {
                            Nez.Flags.SetFlagExclusive(ref collider.PhysicsLayer, (int)Constants.PhysicsLayer.SemiSolidWall);
                        }
                        Flags.SetFlagExclusive(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.Player);
                        Flags.SetFlag(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.Enemy);
                        Flags.SetFlag(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.Gib);
                        Flags.SetFlag(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.EnemyShot);
                        test = collider.PhysicsLayer;
                        wallsEntity.AddComponent(collider);
                    }
                    else if (halfWallLR > 0)
                    {
                        var x = i * 10f;
                        var y = j * 10f;
                        var collider = new BoxCollider(5f, 10f);
                        collider.LocalOffset = new Vector2(x, y) + new Vector2(5f); // additional 5f,5f is to deal with the center origin
                        Nez.Flags.SetFlagExclusive(ref collider.PhysicsLayer, (int)Constants.PhysicsLayer.Wall);
                        if (halfWallLR == 11) // need to change this to more elaborate semi-solid checks
                        {
                            Nez.Flags.SetFlagExclusive(ref collider.PhysicsLayer, (int)Constants.PhysicsLayer.SemiSolidWall);
                        }
                        Flags.SetFlagExclusive(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.Player);
                        Flags.SetFlag(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.Enemy);
                        test = collider.PhysicsLayer;
                        wallsEntity.AddComponent(collider);
                    }
                    else if (halfWallUD > 0)
                    {
                        var x = i * 10f;
                        var y = j * 10f;
                        var collider = new BoxCollider(10f, 5f);
                        collider.LocalOffset = new Vector2(x, y) + new Vector2(5f); // additional 5f,5f is to deal with the center origin
                        Nez.Flags.SetFlagExclusive(ref collider.PhysicsLayer, (int)Constants.PhysicsLayer.Wall);
                        if (halfWallUD == 11) // need to change this to more elaborate semi-solid checks
                        {
                            Nez.Flags.SetFlagExclusive(ref collider.PhysicsLayer, (int)Constants.PhysicsLayer.SemiSolidWall);
                        }
                        Flags.SetFlagExclusive(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.Player);
                        Flags.SetFlag(ref collider.CollidesWithLayers, (int)Constants.PhysicsLayer.Enemy);
                        test = collider.PhysicsLayer;
                        wallsEntity.AddComponent(collider);
                    }
                }
            }
            //Console.WriteLine($"WALL: {Nez.Flags.BinaryStringRepresentation(test)}");

            AddEntity(wallsEntity);
        }

        public void PlayerDied()
        {
            //have some fade to black with a prototype sprite
            // after fade pull up some text that says shoot to try again or escape to quit?

            hitEffectSprite.Color = Color.Transparent;
            Color fadeColor = new Color(128, 0, 0, 75);
            hitEffectSprite.Enabled = true;
            hitEffectSprite.TweenColorTo(fadeColor, 0.5f).SetCompletionHandler((c) => restartText.Enabled = true).Start();
            wolfRenderer.Tween("verticalOffset", -50f, 0.5f).Start();
            Core.Schedule(0.5f, (t) => alive = false);
        }

        public void RestartLevel()
        {
            Core.Scene = new WolfScene(levelName);
        }

        public void EndLevel()
        {
            if (isEnded) return;
            isEnded = true;
            foreach(AmmoType type in Enum.GetValues(typeof(AmmoType)))
            {
                var val = playerState.GetAmmo(type);
                if(val > -1)
                {
                    var exists = playerInfo.Ammo.ContainsKey(type);
                    if (exists)
                    {
                        playerInfo.Ammo[type] = val;
                    }
                    else
                    {
                        playerInfo.Ammo.Add(type, val);
                    }
                }
            }
            playerInfo.Heath = player.hp;

            playerInfo.equippedWeapon = (AmmoType)(player.controller.equippedWeaponIndex);
            if (levelToNextLevel.ContainsKey(levelName))
            {
                Core.Scene = new WolfScene(levelToNextLevel[levelName], playerInfo);
            }
        }


    }
}
