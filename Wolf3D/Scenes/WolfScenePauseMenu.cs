using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wolf3D.Scenes
{
    public partial class WolfScene
    {
        public bool paused = false;

        // Nez UI stuff
        Entity uiEntity;
        UICanvas canvas;
        Table table;
        Table areYouSureTable;

        Button exitButton;
        Slider sfxSlider;
        Slider musicSlider;

        Button notSureButton;
        Button amSureButton;

        public void CreatePauseMenu()
        {
            //setup the canvas
            uiEntity = CreateEntity("UI");
            canvas = new UICanvas();
            uiEntity.AddComponent(canvas);
            canvas.RenderLayer = 10;
            var Stage = canvas.Stage;
            //setup the table
            table = Stage.AddElement(new Table());
            table.SetY(25);
            table.SetX(150);
            table.SetWidth(NezGame.designWidth);
            table.SetHeight(h - 50);
            table.SetWidth(w - 300);
            
            table.SetBackground(new PrimitiveDrawable(Color.Black));

            var text = new Label("/2PAUSED/2", new LabelStyle(Color.White));
            text.SetAlignment(Align.Top);
            table.Add(text);
            table.Row();
            var blank = new Label("-----------");
            table.Add(blank);
            table.Row();

            var sfxText = new Label("SFX Volume:", new LabelStyle(Color.White));
            table.Add(sfxText);
            table.Row();
            sfxSlider = new Slider(0, 100, 1, false, SliderStyle.Create(Color.DarkSlateGray, Color.Gray));
            sfxSlider.SetValue(NezGame.gameSettings.sfxVolume);
            sfxSlider.OnChanged += SfxChanged;
            table.Add(sfxSlider);
            table.Row();

            var musicTextComponent = new Label("Music Volume:", new LabelStyle(Color.White));
            table.Add(musicTextComponent);
            table.Row();
            musicSlider = new Slider(0, 100, 1, false, SliderStyle.Create(Color.DarkSlateGray, Color.Gray));
            musicSlider.SetValue(NezGame.gameSettings.musicVolume);
            musicSlider.OnChanged += MusicChanged;
            table.Add(musicSlider);
            table.Row();

            var returnToGameButton = new TextButton("RETURN TO GAME", TextButtonStyle.Create(Color.Black, Color.DarkGray, Color.DarkSlateGray));
            returnToGameButton.OnClicked += ReturnToGame;
            table.Add(returnToGameButton).SetMinWidth(100).SetMinHeight(25);
            table.Row();
            
            //need some ARE YOU SURE box here
            exitButton = new TextButton("EXIT TO MAIN MENU", TextButtonStyle.Create(Color.Black, Color.DarkGray, Color.DarkSlateGray));
            exitButton.OnClicked += Exit;
            table.Add(exitButton).SetMinWidth(100).SetMinHeight(25);


            table.SetVisible(false);
            table.ToBack();



            areYouSureTable = Stage.AddElement(new Table());
            areYouSureTable.SetY(25);
            areYouSureTable.SetX(150);
            areYouSureTable.SetWidth(NezGame.designWidth);
            areYouSureTable.SetHeight(h - 50);
            areYouSureTable.SetWidth(w - 300);

            areYouSureTable.SetBackground(new PrimitiveDrawable(Color.Black));

            var uSureText = new Label("EXIT AND LOSE /1UNSAVED PROGRESS/1??", new LabelStyle(Color.White));
            areYouSureTable.Add(uSureText);
            areYouSureTable.Row();

            notSureButton = new TextButton("NO NOT MY PROGRESS!!", TextButtonStyle.Create(Color.Black, Color.DarkGray, Color.DarkSlateGray));
            notSureButton.OnClicked += NoReturn;
            areYouSureTable.Add(notSureButton).SetMinWidth(100).SetMinHeight(25);
            areYouSureTable.Row();

            amSureButton = new TextButton("YEAH I'M SURE", TextButtonStyle.Create(Color.Black, Color.DarkGray, Color.DarkSlateGray));
            amSureButton.OnClicked += YesExit;
            areYouSureTable.Add(amSureButton).SetMinWidth(100).SetMinHeight(25);
            areYouSureTable.Row();


            areYouSureTable.SetVisible(false);
            areYouSureTable.ToBack();
        }

        public override void Unload()
        {
            exitButton.OnClicked -= Exit;
            sfxSlider.OnChanged -= SfxChanged;
            musicSlider.OnChanged -= MusicChanged;
            notSureButton.OnClicked -= NoReturn;
            amSureButton.OnClicked -= YesExit;
        }

        public void SfxChanged(float volume)
        {
            NezGame.gameSettings.sfxVolume = (int)volume;
            NezGame.SaveSettings(NezGame.gameSettings);
        }
        public void MusicChanged(float volume)
        {
            NezGame.gameSettings.musicVolume = (int)volume;
            NezGame.ApplySoundSettings();
            NezGame.SaveSettings(NezGame.gameSettings);
        }

        public void ReturnToGame(Button btn)
        {
            Pause();
        }

        public void Exit(Button btn)
        {
            table.SetVisible(false);
            areYouSureTable.SetVisible(true);
        }

        public void YesExit(Button btn)
        {
            Core.Scene = new MainMenuScene();
        }

        public void NoReturn(Button btn)
        {
            table.SetVisible(true);
            areYouSureTable.SetVisible(false);
        }

        public void Pause()
        {
            if (!paused)
            {
                //pause the game
                player.controller.Enabled = false;
                Time.TimeScale = 0;
                paused = true;
                table.SetVisible(true);
                Nez.Input.MouseLockState = MouseLockState.Unlocked;
                Core.Instance.IsMouseVisible = true;
                areYouSureTable.SetVisible(false);
            }
            else
            {
                //un-pause the game
                paused = false;
                player.controller.Enabled = true;
                table.SetVisible(false);
                // gotta figure out how to fix this for fullscreen stretched stuff
                if (!Screen.IsFullscreen)
                {
                    // move the mouse to the middle of the screen so you dont jump around after getting back into the game
                    var w = Core.GraphicsDevice.Viewport.Width / 2;
                    var h = Core.GraphicsDevice.Viewport.Height / 2;
                    Mouse.SetPosition(w, h);
                }
                Nez.Input.MouseLockState = MouseLockState.Locked;
                Core.Instance.IsMouseVisible = false;
                Time.TimeScale = 1;
                areYouSureTable.SetVisible(false);
            }
        }
    }
}
