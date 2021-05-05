using Nez;
using Nez.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Wolf3D.Scenes
{
    public class MainMenuScene : Scene
    {
        // Nez UI stuff
        Entity uiEntity;
        UICanvas canvas;
        Table table; //main menu
        Table optionsTable; //options menu
        Table levelSelectTable; //temp simple level select menu with buttons?

        //main menu buttons
        Button startGame;
        Button levelSelect;
        Button options;
        Button exit;

        //options buttons
        Slider sfxSlider;
        Slider musicSlider;
        CheckBox fullscreenCheckbox;
        SelectBox<string> windowSizeSelect;
        Button optionsToMain;

        //level select
        SelectBoxList<string> levelSelectList;
        Button levelToMain;

        public override void Initialize()
        {
            base.Initialize();
            Input.MouseLockState = MouseLockState.Unlocked;
            Core.Instance.IsMouseVisible = true;
            ClearColor = Color.Black;
            AddRenderer(new DefaultRenderer());
            NezGame.PlayMusic(NezGame.mainMenuMusic);

            uiEntity = CreateEntity("UI");
            canvas = new UICanvas();
            canvas.IsFullScreen = false;
            uiEntity.AddComponent(canvas);
            canvas.RenderLayer = 10;

            SetupMainMenu(canvas.Stage);
            SetupOptionsMenu(canvas.Stage);
        }

        public override void Unload()
        {
            //remove all the button event refs
            startGame.OnClicked -= StartGame_OnClicked;
            options.OnClicked -= Options_OnClicked;
            levelSelect.OnClicked -= LevelSelect_OnClicked;
            exit.OnClicked -= Exit_OnClicked;

            sfxSlider.OnChanged -= SfxChanged;
            musicSlider.OnChanged -= MusicChanged;
            optionsToMain.OnClicked -= OptionsToMain_OnClicked;
            fullscreenCheckbox.OnChanged -= FullscreenCheckbox_OnChanged;

        }

        #region Main Menu
        void SetupMainMenu(Stage Stage)
        {
            table = Stage.AddElement(new Table());
            table.SetY(NezGame.designHeight / 2f);
            table.SetX(0);
            table.SetWidth(NezGame.designWidth);
            table.SetHeight(NezGame.designHeight - 50);

            var buttonStyle = TextButtonStyle.Create(Color.Black, Color.DarkGray, Color.DarkSlateGray);

            startGame = new TextButton("START GAME", buttonStyle);
            startGame.OnClicked += StartGame_OnClicked;
            table.Add(startGame);
            table.Row();

            levelSelect = new TextButton("LEVEL SELECT", buttonStyle);
            levelSelect.OnClicked += LevelSelect_OnClicked;
            table.Add(levelSelect);
            table.Row();

            options = new TextButton("OPTIONS", buttonStyle);
            options.OnClicked += Options_OnClicked;
            table.Add(options);
            table.Row();

            exit = new TextButton("EXIT", buttonStyle);
            exit.OnClicked += Exit_OnClicked;
            table.Add(exit);
            table.Row();
        }

        private void Exit_OnClicked(Button obj)
        {
            Core.Exit();
        }

        private void Options_OnClicked(Button obj)
        {
            table.SetVisible(false);
            optionsTable.SetVisible(true);
        }

        private void LevelSelect_OnClicked(Button obj)
        {
        }

        private void StartGame_OnClicked(Button obj)
        {
            Core.Scene = new WolfScene("tiled-test");
        }
        #endregion

        #region Options Menu
        void SetupOptionsMenu(Stage Stage)
        {
            optionsTable = Stage.AddElement(new Table());
            optionsTable.SetY(NezGame.designHeight * 0.2f);
            optionsTable.SetX(0);
            optionsTable.SetWidth(NezGame.designWidth);
            optionsTable.SetHeight(NezGame.designHeight * 0.8f);

            var text = new Label("OPTIONS", new LabelStyle(Color.White));
            text.SetAlignment(Align.Top);
            optionsTable.Add(text);
            optionsTable.Row();
            var blank = new Label("-----------");
            optionsTable.Add(blank);
            optionsTable.Row();

            var sfxTextComponent = new Label("SFX VOLUME:", new LabelStyle(Color.White));
            optionsTable.Add(sfxTextComponent);
            optionsTable.Row();
            sfxSlider = new Slider(0, 100, 1, false, SliderStyle.Create(Color.DarkSlateGray, Color.Gray));
            sfxSlider.SetValue(NezGame.gameSettings.sfxVolume);
            sfxSlider.OnChanged += SfxChanged;
            optionsTable.Add(sfxSlider);
            optionsTable.Row();

            var musicTextComponent = new Label("MUSIC VOLUME:", new LabelStyle(Color.White));
            optionsTable.Add(musicTextComponent);
            optionsTable.Row();
            musicSlider = new Slider(0, 100, 1, false, SliderStyle.Create(Color.DarkSlateGray, Color.Gray));
            musicSlider.SetValue(NezGame.gameSettings.musicVolume);
            musicSlider.OnChanged += MusicChanged;
            optionsTable.Add(musicSlider);
            optionsTable.Row();

            var checkboxStyle = new CheckBoxStyle(new PrimitiveDrawable( 20f, Color.DarkSlateGray), new PrimitiveDrawable(20f, Color.Green), null, Color.White);
            fullscreenCheckbox = new CheckBox("FULLSCREEN", checkboxStyle);
            fullscreenCheckbox.IsChecked = NezGame.gameSettings.isFullscreen;
            fullscreenCheckbox.OnChanged += FullscreenCheckbox_OnChanged;
            optionsTable.Add(fullscreenCheckbox);
            optionsTable.Row();

            var listboxstyle = new ListBoxStyle(Graphics.Instance.BitmapFont, Color.White, Color.DarkSlateGray, new PrimitiveDrawable(50f, Color.Red));
            listboxstyle.HoverSelection = new PrimitiveDrawable(50f, Color.Yellow);
            var scrollPaneStyle = new ScrollPaneStyle(new PrimitiveDrawable(25, Color.Black), new PrimitiveDrawable(25, Color.Yellow), new PrimitiveDrawable(25, Color.Purple), new PrimitiveDrawable(25, Color.Blue), new PrimitiveDrawable(25, Color.Orange));
            var selectStyle = new SelectBoxStyle(Graphics.Instance.BitmapFont, Color.White, new PrimitiveDrawable(25, Color.Black), scrollPaneStyle, listboxstyle);
            windowSizeSelect = new SelectBox<string>(selectStyle);
            windowSizeSelect.SetSize(50, 50f);
            var currentSize = $"{NezGame.gameSettings.WindowWidth}x{NezGame.gameSettings.WindowHeight}";
            string[] sizes = { "1920x1080", $"{NezGame.designWidth}x{NezGame.designHeight}" };
            windowSizeSelect.SetItems(sizes);
            windowSizeSelect.SetSelected(currentSize);
            windowSizeSelect.OnChanged = WindowSizeSelect_OnChanged;
            optionsTable.Add(windowSizeSelect);
            optionsTable.Row();

            optionsToMain = new TextButton("BACK", TextButtonStyle.Create(Color.Black, Color.DarkGray, Color.DarkSlateGray));
            optionsToMain.OnClicked += OptionsToMain_OnClicked;
            optionsTable.Add(optionsToMain);
            optionsTable.Row();

            optionsTable.SetVisible(false);
        }

        public void WindowSizeSelect_OnChanged(string val)
        {
            var vals = val.Split('x').ToArray();
            NezGame.gameSettings.WindowWidth = int.Parse(vals[0]);
            NezGame.gameSettings.WindowHeight = int.Parse(vals[1]);
            NezGame.ApplyScreenSettings();
            NezGame.SaveSettings(NezGame.gameSettings);
        }

        private void FullscreenCheckbox_OnChanged(bool val)
        {
            NezGame.gameSettings.isFullscreen = val;
            NezGame.ApplyScreenSettings();
            NezGame.SaveSettings(NezGame.gameSettings);
        }

        private void OptionsToMain_OnClicked(Button obj)
        {
            table.SetVisible(true);
            optionsTable.SetVisible(false);
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
        #endregion
    }
}
