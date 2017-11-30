using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Xml;
using System.IO;

namespace RobotTank
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //enemies
        List<EnemyTank> enemies = new List<EnemyTank>();
        List<AdvancedEnemy> advancedEnemies = new List<AdvancedEnemy>();
        List<EnemySoldier> soldiers = new List<EnemySoldier>();
        Texture2D enemyTankTexture, enemyTankTurretTex, advancedTank, advancedTankTurret, soldierTex, soldierBulletTex;

        const int WrenchRepairAmount = 10;
        Texture2D background;
        Texture2D splashBG;
        Texture2D gameOverOptionsBackground;
        Texture2D bodyTex;
        Texture2D turretTex;
        Texture2D rocketTex;
        Texture2D treadsTex;
        float lifeUpMessageTimer;
        const int LifeUpMessageMax = 1500;
        Texture2D displayTex;

        //waves 
        int curWave;
        const int MaxWaves = 5;
        int[] MaxEnemies = new int[MaxWaves]            { 2, 4, 6, 8, 10 };   //amount of each enemy on each wave
        int[] MaxAdvancedEnemies = new int[MaxWaves]    { 2, 2, 3, 4, 6 };
        int[] MaxSoldiers = new int[MaxWaves]           { 4, 6, 10, 10, 15 };

        //sounds
        Song menusMusic; //play this music when not playing the game (menus,instructions,options,gameover)
        Song gameMusic; //play this music when the game is being played
        SoundEffect explosion; //other sound effects
        SoundEffect soldierDeath;
        SoundEffect click; //for clicking buttons
        SoundEffect soldierFire;
        SoundEffect rocketFire;
        bool playMusic; //play music if this is true otherwise dont
        bool playSounds; //play sounds if this is true otherwise dont

        //fonts
        SpriteFont font2;
        SpriteFont bigFont;
        SpriteFont lifeUpFont;
        SpriteFont font;

        //lighting
        Texture2D whiteSquare;
        Texture2D lightMask;
        RenderTarget2D mainSceneRT;
        RenderTarget2D lightMaskRT;
        Effect lightingEffect;
        Color worldColour, nightColour = new Color(70,70,70), dayColour = Color.White;
        float nightDayRate;
        float timer;
        float transitionTimer;
        const int MillisecondsPerDay = 10000; //time for day
        const int MillisecondsPerNight = 5000; //time for night 
        const int MillisecondsTransition = 15000; //time to change from one to the other
        bool changeTime; //if true then transistion into night/day time
        bool night; //if the last time was night or day if false
        const int LIGHTOFFSET = 115;


        //other
        List<Stain> bloodStains = new List<Stain>();
        List<Stain> explosionStains = new List<Stain>();
        Random rnd = new Random();
        enum Mode { Game, GameOver, Pause, Splash, Options };
        List<Sprite> explosions = new List<Sprite>();
        const int ExpSpriteWidth = 100;
        const int ExpSpriteHeight = 100;
        Texture2D healthBG, currentHealth,  explosionStainTex, bloodStainTex;
        const int ScreenWidth = 1024, ScreenHeight = 720;
        const int WorldWidth = 4 * ScreenWidth, WorldHeight = 4 * ScreenHeight;

        //GUI
        const int MaxButtons = 8;
        Button[] buttons = new Button[MaxButtons];
        Mode currentMode = Mode.Splash;
        const int ButtonX = 250;
        const int ButtonYSeperation = 30;
        bool showInfo;
        KeyboardState previousKeyboardState = Keyboard.GetState();
        MouseState previousMouseState = Mouse.GetState();
        bool showContinue;
        Texture2D infoTexture;
        Rectangle infoRect;
        Scorekeeper scoreKeeper;
        int start = 0, continueB = 1, options = 2, info = 3, exit = 4, music = 5, sounds = 6, mainMenu = 7;
        const float PauseOpacity = 0.8f;


        //player
        int deathTimer;//when the player dies these are used to make only one explosion and to wait a while before going to the next mode
        bool enableDeathTimer;
        bool playerDeathExplode;
        Texture2D explosionSpriteSheet;
        bool nameEntered;
        string playerName;
        PlayerTank player;
        const int PlayerColDamage = 30;

        //map
        Map map;
        const float MapScale = 0.07f;

        //drops
        Drop wrenchDrop; //Wrench object that drops randomly from enemies and the player repairs armour with it
        Drop droneDrop;
        Drone healDrone;
        float wrenchSpawnProb = 0.25f; //chance of a wrench spawning when an enemy dies
        float droneSpawnProb = 0.1f; //chance of a drone spawning when an enemy dies
        //drone object the player can get to heal them over time

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = ScreenWidth;
            graphics.PreferredBackBufferHeight = ScreenHeight;
            graphics.ApplyChanges();
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp; //so dont have to use window size that is power of 2
            IsMouseVisible = true;
            //create objects
            for (int i = 0; i < MaxButtons; i++)
                buttons[i] = new Button();
            scoreKeeper = new Scorekeeper(ScreenWidth);
            map = new Map(ScreenWidth, ScreenHeight, WorldWidth, WorldHeight, MapScale);
            wrenchDrop = new Drop();
            droneDrop = new Drop();
            healDrone = new Drone();
            infoRect = new Rectangle(ScreenWidth / 10, 20 + ScreenHeight / 10, ScreenWidth - ScreenWidth / 5, ScreenHeight - ScreenHeight / 5);
            LoadHighScores(); //load any highscores from the xml file from previous saves
            SetUpGame();
            base.Initialize();
        }

        private void SetUpGame()
        {
            //setup camera
            Camera.GraphicDevice = GraphicsDevice;
            Camera.Zoom = 1f;

            //day/night 
            worldColour = Color.White;
            changeTime = false;
            night = false;
            timer = 0;
            transitionTimer = 0;

            //set up gui
            SetButtonName();
            SetButtonPos();
            showInfo = false;
            showContinue = false;
            nameEntered = false;
            playerName = "";

            //set up sounds
            playMusic = true;
            playSounds = true;

            //player death variables
            enableDeathTimer = false;
            deathTimer = 80;
            playerDeathExplode = true;

            curWave = 0;
            lifeUpMessageTimer = LifeUpMessageMax;

        }

        private void RestartGame()
        { //restart the game by clearing all list, calling objects reset methods and reseting some variables int setupgame
            SetUpGame();
            scoreKeeper.Reset();
            player.Reset();
            enemies.Clear();
            advancedEnemies.Clear();
            soldiers.Clear();            
            explosions.Clear();
            wrenchDrop.Reset();
            explosionStains.Clear();
            bloodStains.Clear();
            droneDrop.Reset();
            healDrone.Reset();
            SpawnWave();
        }

        private void SetButtonName()
        {
            buttons[start].Text = "Start";
            buttons[continueB].Text = "Continue";
            buttons[options].Text = "Options";
            buttons[info].Text = "Info";
            buttons[exit].Text = "Exit";
            buttons[music].Text = "Music";
            buttons[sounds].Text = "Sounds";
            buttons[mainMenu].Text = "Main Menu";
        }

        private void SetButtonPos()
        { //set the pos of buttons depending on current mode
            if (currentMode == Mode.Splash)
            {
                buttons[start].Position = new Vector2(ButtonX - 165, 190);//start
                buttons[info].Position = new Vector2(ButtonX - 165, 270);//info
                buttons[options].Position = new Vector2(ButtonX - 165, 350);//options
                buttons[exit].Position = new Vector2(ButtonX - 165, 430);//exit
                buttons[music].Position = new Vector2(ButtonX, 140);//music
                buttons[continueB].Position = new Vector2(ButtonX, 230); //continue
                buttons[sounds].Position = new Vector2(ButtonX, 80);//sounds
                buttons[mainMenu].Position = new Vector2(345, 15);  //main menu
                
            }
            else if (currentMode == Mode.Game)
            {

            }
            else if (currentMode == Mode.Pause)
            {
                buttons[continueB].Position = new Vector2(385, 230);
                buttons[mainMenu].Position = new Vector2(385, 310); 
                buttons[options].Position = new Vector2(385, 390); 
                buttons[exit].Position = new Vector2(385, 470); 
            }
            else if (currentMode == Mode.Options)
            {
                buttons[sounds].Position = new Vector2(ButtonX - 165, 90);
                buttons[music].Position = new Vector2(ButtonX - 165, 170);
                if (showContinue)
                {
                    buttons[continueB].Position = new Vector2(ButtonX - 165, 250); 
                }
                else
                {
                    buttons[mainMenu].Position = new Vector2(ButtonX - 165, 250);
                }
            }
            else //game over mode
            {
                buttons[mainMenu].Position = new Vector2(ButtonX - 165, 295); 
            }
        }
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //lighting
            whiteSquare = Content.Load<Texture2D>("whitesquare");
            lightMask = Content.Load<Texture2D>("lightmask");
            lightingEffect = Content.Load<Effect>("lighting");
            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            mainSceneRT = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);
            lightMaskRT = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight);

            //map
            map.LoadContent(Content, "map", "dot");
            //backgrounds
            background = Content.Load<Texture2D>("background");
            splashBG = Content.Load<Texture2D>("splashbackground1");
            gameOverOptionsBackground = Content.Load<Texture2D>("gameoveroptionsbg");

            //tanks
            bodyTex = Content.Load<Texture2D>("tank_body");
            turretTex = Content.Load<Texture2D>("turret");
            rocketTex = Content.Load<Texture2D>("rocket");
            treadsTex = Content.Load<Texture2D>("tankTreads");
            advancedTank = Content.Load<Texture2D>("redbody");
            advancedTankTurret = Content.Load<Texture2D>("redturret");
            enemyTankTexture = Content.Load<Texture2D>("greenbody");
            enemyTankTurretTex = Content.Load<Texture2D>("greenturret");

            //soldier 
            soldierTex = Content.Load<Texture2D>("soldier");
            soldierBulletTex = Content.Load<Texture2D>("bullet");

            //fonts
            font2 = Content.Load<SpriteFont>("tempFont");
            font = Content.Load<SpriteFont>("font");
            bigFont = Content.Load<SpriteFont>("bigFont");
            lifeUpFont = Content.Load<SpriteFont>("lifeUpFont");

            //gui
            displayTex = Content.Load<Texture2D>("highScoreTex");
            healthBG = Content.Load<Texture2D>("healthbackground");
            currentHealth = Content.Load<Texture2D>("healthbar");
            Texture2D scoreboardTex = Content.Load<Texture2D>("scoreboard");
            Texture2D lifeImage = Content.Load<Texture2D>("lifeImage");
            scoreKeeper.LoadContent(scoreboardTex, font, bigFont, displayTex, healthBG, currentHealth, lifeImage);
            infoTexture = Content.Load<Texture2D>("infoTexture");
            Texture2D button = Content.Load<Texture2D>("buttonTex");
            SpriteFont buttonFont = Content.Load<SpriteFont>("buttonFont");
            foreach (Button b in buttons)
                b.LoadContent(button, buttonFont);


            //drops
            Texture2D particleTex = Content.Load<Texture2D>("particle");
            droneDrop.LoadContent(this.Content, "healDroneGlow");
            wrenchDrop.LoadContent(this.Content, "wrench");
            healDrone.LoadContent(this.Content, "healDrone", particleTex);

            bloodStainTex = Content.Load<Texture2D>("bloodStain");
            explosionStainTex = Content.Load<Texture2D>("explosionMark");
            explosionSpriteSheet = Content.Load<Texture2D>("explosionspritesheet");

            //load sounds
            menusMusic = Content.Load<Song>("music2");
            gameMusic = Content.Load<Song>("music1");
            click = Content.Load<SoundEffect>("click");
            explosion = Content.Load<SoundEffect>("explosion");
            soldierDeath = Content.Load<SoundEffect>("soldierdeath");
            soldierFire = Content.Load<SoundEffect>("soldierfire");
            rocketFire = Content.Load<SoundEffect>("rocketfire");
            if (playMusic)
                MediaPlayer.Play(menusMusic); //start with menu music
            MediaPlayer.IsRepeating = true; //music will always be repeating

            //player
            player = new PlayerTank(WorldWidth, WorldHeight, bodyTex, rocketTex, rocketFire);
        }

        private void CheckButtons()
        {
            foreach (Button b in buttons)
            {
                if (b.Clicked)
                { //if a button was clicked, check which one it was and do the needed action
                    if (playSounds)
                        click.Play();
                    if (currentMode == Mode.Splash)
                    {
                        if (b == buttons[start])
                        {
                            currentMode = Mode.Game;
                            if (playMusic)
                                MediaPlayer.Play(gameMusic);
                            RestartGame(); //reset the game

                        }
                        else if (b == buttons[options])
                        {
                            currentMode = Mode.Options;
                        }
                        else if (b == buttons[info])
                        {
                            showInfo = true;
                        }
                        else if (b == buttons[exit])
                        {
                            Exit();
                        }
                        else if (showInfo && b == buttons[mainMenu]) //in info screen
                        {
                            showInfo = false;
                        }
                    }
                    else if (currentMode == Mode.Game)
                    {

                    }
                    else if (currentMode == Mode.Options)
                    {
                        if (b == buttons[mainMenu] && showContinue == false)
                        {
                            currentMode = Mode.Splash;
                        }
                        else if (b == buttons[music])
                        {
                            playMusic = !playMusic; //toggle the music to the opposite of what it currently is. (if false it will now be set to true)

                            if (playMusic)
                                MediaPlayer.Play(menusMusic); //start music if playMusic is true
                            else
                                MediaPlayer.Stop(); //stop the music if playMusic is now false
                        }
                        else if (b == buttons[sounds])
                        {
                            playSounds = !playSounds; //toggle the sounds on or off
                        }
                        else if (b == buttons[continueB] && showContinue)
                        {
                            currentMode = Mode.Game; //if the continue button has been clicked then continue the game
                            if (playMusic)
                                MediaPlayer.Play(gameMusic);
                        }
                    }
                    else if (currentMode == Mode.Pause)
                    {
                        if (b == buttons[mainMenu])
                        {
                            currentMode = Mode.Splash;
                            if (playMusic)
                                MediaPlayer.Play(menusMusic);
                        }
                        else if (b == buttons[options])
                        {
                            currentMode = Mode.Options;
                            showContinue = true; //can continue the game if in entering the options from the pause mode
                            if (playMusic)
                                MediaPlayer.Play(menusMusic);
                        }
                        else if (b == buttons[continueB])
                        {
                            currentMode = Mode.Game; //if the continue button has been clicked then continue the game
                            SetButtonPos();
                        }

                        else if (b == buttons[exit])
                        {
                            Exit(); //exit the game
                        }
                    }
                    else  //game over
                    {
                        if (b == buttons[mainMenu])
                            currentMode = Mode.Splash;
                    }
                            
                    SetButtonPos(); 
                }
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            foreach (Button b in buttons)
                b.CheckClicked(previousMouseState);  //check if any buttons were clicked
            CheckButtons(); //check what buttons were clicked and the current mode and do something

            switch (currentMode)
            {
                case Mode.Splash: //if the game mode is currently set to the splash mode
                    UpdateSplash(gameTime); //update the splash mode only
                    break;
                case Mode.Game:
                    UpdateGame(gameTime);
                    break;
                case Mode.GameOver:
                    UpdateGameOver(gameTime);
                    break;
                case Mode.Options:
                    UpdateOptions(gameTime);
                    break;
                case Mode.Pause:
                    UpdatePause(gameTime);
                    break;
            }

            //set the current states to be the new previous states
            previousMouseState = Mouse.GetState();
            previousKeyboardState = Keyboard.GetState();
            base.Update(gameTime);
        }

        private void UpdateSplash(GameTime gameTime)
        {//do this while in the splash mode
            if (!Keyboard.GetState().IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyDown(Keys.Escape) && showInfo)
            {
                showInfo = false; //leave the information screen if the escape key was pressed while inside it
            }
        }

        private void UpdateGame(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (!Keyboard.GetState().IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyDown(Keys.Escape))
            {
                currentMode = Mode.Pause; //pause the game when the escape key is pressed
                SetButtonPos();
            }
            if (!Keyboard.GetState().IsKeyDown(Keys.S) && previousKeyboardState.IsKeyDown(Keys.S))
            {
                TakeScreenShot();
            }

            if (enableDeathTimer) //if the player has just died
            {
                deathTimer--; //decrease this timer 
                if (deathTimer < 0) //when it gets below 0
                {
                    if (scoreKeeper.Lives == 0)
                    {

                        if (playMusic)
                            MediaPlayer.Play(menusMusic); //change music
                        nameEntered = false; //reset nameEntered to false
                        playerName = ""; //reset the player name to an empty string
                        currentMode = Mode.GameOver;// go into game over mode
                    }
                    else
                    {
                        deathTimer = 80;
                        enableDeathTimer = false;
                        playerDeathExplode = true;
                        scoreKeeper.ResetHealth();
                        player.Reset();
                    }

                }
            }
            if (player.Alive)
                player.Update(gameTime, new Vector2(bodyTex.Bounds.Center.X, bodyTex.Bounds.Center.Y));
            foreach (EnemyTank e in enemies)
                e.Update(gameTime, player.Position, player.Rotation, playSounds); //update enemies
            foreach (AdvancedEnemy aEnemy in advancedEnemies)
                aEnemy.Update(gameTime, player.Position, player.Rotation, playSounds); //update advanced enemies
            foreach (EnemySoldier s in soldiers)
                s.Update(gameTime, player.Position);

            DetectCollisions();

            wrenchDrop.Update(gameTime);
            droneDrop.Update(gameTime);
            healDrone.Update(new Vector2(player.Position.X, player.Position.Y), gameTime);
            if (healDrone.Alive == false)
                scoreKeeper.DisableRegen();

            scoreKeeper.UpdateHealth(gameTime);
            RemoveObjects();
            IsGameOver(); //check if the gameover conditions have occured


            timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (night) //if its night time
            {
                if (timer > MillisecondsPerNight)
                {  //out of time
                    changeTime = true;
                }
                if (changeTime)
                {
                    transitionTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    nightDayRate = transitionTimer / MillisecondsTransition;
                    worldColour = Color.Lerp(nightColour, dayColour, nightDayRate);
                    if (transitionTimer > MillisecondsTransition)
                    {
                        night = false;
                        changeTime = false;
                        timer = 0;
                        transitionTimer = 0;
                    }
                }
            }
            else  //day
            {
                if (timer > MillisecondsPerDay)
                {  //out of time
                    changeTime = true;
                }  
                  if (changeTime)
                {
                    transitionTimer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    nightDayRate = transitionTimer / MillisecondsTransition;
                    worldColour = Color.Lerp(dayColour, nightColour, nightDayRate);
                    if (transitionTimer > MillisecondsTransition)
                    {
                        night = true;
                        changeTime = false;
                        timer = 0;
                        transitionTimer = 0;
                    }
                }
            }

            



            base.Update(gameTime);
        }

        private void TakeScreenShot()
        {
            Stream stream = File.Create("mask.png");
            Texture2D light = lightMaskRT;
            light.SaveAsPng(stream, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);            
            stream.Dispose();
        }
        private void UpdateOptions(GameTime gameTime)
        {
        }

        private void UpdatePause(GameTime gameTime)
        {
            if (!Keyboard.GetState().IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyDown(Keys.Escape))
            {
                currentMode = Mode.Game; //continue the game
                SetButtonPos();
            }
        }

        private void UpdateGameOver(GameTime gameTime)
        {
            if (!Keyboard.GetState().IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyDown(Keys.Escape) && nameEntered)
            {
                currentMode = Mode.Splash; //return to the splash screen
                SetButtonPos();
            }

            if (!Keyboard.GetState().IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyDown(Keys.Enter) && nameEntered)
            {
                currentMode = Mode.Game; //return to the splash screen
                RestartGame(); //reset the game
                if (playMusic)
                    MediaPlayer.Play(gameMusic);
            }
            

            if (nameEntered == false)
            {
                GetUserInput(); //get the name being entered by the player
            }

            if (!Keyboard.GetState().IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyDown(Keys.Enter) && nameEntered == false)
            { //when the player presses enter 
                if (playerName == null || playerName == "")
                    playerName = "Default"; //if the player didnt enter anything or they have scored 0
                nameEntered = true; //name has now been entered
                scoreKeeper.UpdateHighScores(playerName, curWave + 1); //pass this name and waves to scorekeeper to be added to the highscore table
                SaveHighScores(); //save the new table to an xml file
            }

            CheckButtons(); //check if any buttons have being clicked
            previousKeyboardState = Keyboard.GetState();
            previousMouseState = Mouse.GetState();
        }

        private void SaveHighScores()
        { //save the highscores to a text and xml file
            try
            {
                //XML:
                XmlTextWriter writer = new XmlTextWriter("../../../../SaveFiles/xmlFile.xml", null);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteComment("XML save file of the highscores in Joint Project 2");

                writer.WriteStartElement("JointProject2"); //root node

                scoreKeeper.WriteXML(writer); //scorekeepr writes itself

                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        private void LoadHighScores()
        {//load the highscores from the xml file
            try
            {
                //XML:
                XmlTextReader reader = new XmlTextReader("../../../../SaveFiles/xmlFile.xml");
                scoreKeeper.ReadXML(reader);    //object loads its own highscores           
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }


        private void GetUserInput()
        { //checks what keys the user has pressed and add them onto playerName
            Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();
            foreach (Keys key in pressedKeys) 
            {
                if (previousKeyboardState.IsKeyUp(key)) //if this key was released previously but is now pressed
                {
                    if (key == Keys.Back && playerName.Length > 0) // overflows
                        playerName = playerName.Remove(playerName.Length - 1, 1); //delete the last char at the end of this string
                    else if (key >= Keys.A && key <= Keys.Z && playerName.Length < 8) //if the the key is a character
                    {
                        bool caps = false;
                        foreach (Keys checkKey in pressedKeys) //loop to check if the right of left shift have been pressed to turn on caps
                        {
                            if (checkKey == Keys.LeftShift || checkKey == Keys.RightShift)
                                caps = true;
                        }
                        if (caps)
                            playerName += key.ToString(); //add the pressed keys to the string as capitals
                        else
                            playerName += key.ToString().ToLower(); //add the pressed keys to the string as small letters

                    }// end if else 
                } //end foreach
            }
        }

        private void IsGameOver()
        { //checks to see if the game has ended

            player.Alive = scoreKeeper.CheckPlayerAlive();
            if (!player.Alive)
            {
                if (playerDeathExplode)
                {
                    playerDeathExplode = false; //make this false so only one explosion is made
                    explosions.Add(new Sprite(explosionSpriteSheet, new Vector2(player.Position.X, player.Position.Y), 1f, ExpSpriteWidth, ExpSpriteHeight, true, 100));
                    explosionStains.Add(new Stain(new Vector2(player.Position.X, player.Position.Y)));
                    scoreKeeper.Lives--;
                }
                enableDeathTimer = true; //start the timer that will count down until the gamemode changes
            }

        }

        private void SpawnWave()
        { //spawn all required enemies for this wave

            for (int i = 0; i < MaxEnemies[curWave]; i++)  //put this in spawn waves?
            {
                EnemyTank newEnemyTank = new EnemyTank(WorldWidth, WorldHeight, player.Position, enemyTankTexture, enemyTankTurretTex, rocketTex);
                newEnemyTank.LoadContent(font, healthBG, currentHealth, rocketFire);
                enemies.Add(newEnemyTank);
            }

            for (int i = 0; i < MaxAdvancedEnemies[curWave]; i++)  //put this in spawn waves?
            {
                AdvancedEnemy newAdvancedEnemy = new AdvancedEnemy(WorldWidth, WorldHeight, player.Position, advancedTank, advancedTankTurret, rocketTex);
                newAdvancedEnemy.LoadContent(font, healthBG, currentHealth, rocketFire);
                advancedEnemies.Add(newAdvancedEnemy);
            }
            for (int i = 0; i < MaxSoldiers[curWave]; i++)  //put this in spawn waves?
            {
                EnemySoldier newSoldier = new EnemySoldier(WorldWidth, WorldHeight, player.Position, soldierTex, soldierBulletTex);
                newSoldier.LoadContent(font, healthBG, currentHealth, soldierFire);
                soldiers.Add(newSoldier);
            }

            
        }

        private void RemoveObjects()
        { //remove any objects from lists which are no longer alive

            for (int i = 0; i < enemies.Count; i++)   //remove enemies
            {
                if (enemies[i].Alive == false)
                {
                    explosions.Add(new Sprite(explosionSpriteSheet, new Vector2(enemies[i].Position.X, enemies[i].Position.Y), 1f, ExpSpriteWidth, ExpSpriteHeight, true, 100)); //make a new explosion
                    explosionStains.Add(new Stain(new Vector2(enemies[i].Position.X, enemies[i].Position.Y)));
                    if (playSounds)
                        explosion.Play();

                    if (rnd.NextDouble() < wrenchSpawnProb) //spawn wrench if less than this at same place explosion spawns
                        wrenchDrop.Activate(new Vector2(enemies[i].Position.X, enemies[i].Position.Y));
                    if (rnd.NextDouble() < droneSpawnProb)
                        droneDrop.Activate(new Vector2(enemies[i].Position.X, enemies[i].Position.Y));
                    scoreKeeper.ChangeScore(enemies[i].ScoreAmount * (curWave + 1));
                    scoreKeeper.EnemiesKilled++;

                    enemies.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < advancedEnemies.Count; i++)   //remove advanced enemies
            {
                if (advancedEnemies[i].Alive == false)
                {
                    explosions.Add(new Sprite(explosionSpriteSheet, new Vector2(advancedEnemies[i].Position.X, advancedEnemies[i].Position.Y), 1f, ExpSpriteWidth, ExpSpriteHeight, true, 100)); //make a new explosion
                    explosionStains.Add(new Stain(new Vector2(advancedEnemies[i].Position.X, advancedEnemies[i].Position.Y)));
                    if (playSounds)
                        explosion.Play();

                    if (rnd.NextDouble() < wrenchSpawnProb * 2) //double the chance of wrench from advanced enemies
                        wrenchDrop.Activate(new Vector2(advancedEnemies[i].Position.X, advancedEnemies[i].Position.Y));
                    if (rnd.NextDouble() < droneSpawnProb)
                        droneDrop.Activate(new Vector2(advancedEnemies[i].Position.X, advancedEnemies[i].Position.Y));

                    scoreKeeper.ChangeScore(advancedEnemies[i].ScoreAmount * (curWave + 1));
                    scoreKeeper.EnemiesKilled++;

                    advancedEnemies.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < soldiers.Count; i++)   //remove soldiers
            {
                if (soldiers[i].Alive == false)
                {

                    bloodStains.Add(new Stain(new Vector2(soldiers[i].Position.X, soldiers[i].Position.Y)));
                    if (playSounds)
                        soldierDeath.Play();

                    if (rnd.NextDouble() < wrenchSpawnProb)
                        wrenchDrop.Activate(new Vector2(soldiers[i].Position.X, soldiers[i].Position.Y));
                    //soldiers dont drop drones

                    scoreKeeper.ChangeScore(soldiers[i].ScoreAmount * (curWave + 1));
                    scoreKeeper.EnemiesKilled++;

                    soldiers.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < explosions.Count; i++) //remove explosions that have finished exploding and their alive is false
            {
                if (explosions[i].Alive == false)
                {
                    explosions.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < bloodStains.Count; i++) //remove blood stains that haved faded out
            {
                if (bloodStains[i].Dead)
                {
                    bloodStains.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < explosionStains.Count; i++) //remove explosion stains that haved faded out
            {
                if (explosionStains[i].Dead)
                {
                    explosionStains.RemoveAt(i);
                    i--;
                }
            }

            if (soldiers.Count == 0 && enemies.Count == 0 && advancedEnemies.Count == 0)  //check if all enemies dead and not on final wave
            {
                if (curWave + 1 == MaxWaves) //game over
                {
                    currentMode = Mode.GameOver;
                    if (playMusic)
                        MediaPlayer.Play(menusMusic);
                    SetButtonPos();
                }
                else
                {
                    curWave++;  //then increase to next wave
                    SpawnWave();
                }
            }

        }

        private void DrawGameMain(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(mainSceneRT);
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, Camera.Transform);
            int xPos = 0, yPos = 0;
            while (xPos < WorldWidth)
            {
                while (yPos < WorldHeight)
                {
                    spriteBatch.Draw(background, new Rectangle(xPos, yPos, background.Width, background.Height), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1f);
                    yPos += background.Height;
                }
                xPos += background.Width;
                yPos = 0;
            }
            spriteBatch.End();


            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, Camera.Transform);
            foreach (Stain eS in explosionStains)
                eS.Draw(spriteBatch, explosionStainTex, gameTime);
            foreach (Stain bS in bloodStains)
                bS.Draw(spriteBatch, bloodStainTex, gameTime);
            wrenchDrop.Draw(spriteBatch);
            droneDrop.Draw(spriteBatch);

            foreach (Sprite exp in explosions)
                  exp.Draw(spriteBatch, gameTime);
            spriteBatch.End();


            if (player.Alive) //only draw the player if they are alive
                player.Draw(spriteBatch, turretTex, treadsTex);

            foreach (EnemyTank e in enemies)
                e.Draw(spriteBatch, treadsTex, gameTime);

            foreach (AdvancedEnemy aEnemy in advancedEnemies)
                aEnemy.Draw(spriteBatch, treadsTex, gameTime);

            foreach (EnemySoldier s in soldiers)
                s.Draw(spriteBatch, gameTime);

            spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, null, null, null, Matrix.CreateTranslation(player.Position) * Camera.Transform);
            healDrone.Draw(spriteBatch);
            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);
        }

        private void DrawGameLightMask(GameTime gameTime)
        { 
            GraphicsDevice.SetRenderTarget(lightMaskRT);
            GraphicsDevice.Clear(Color.Black);

            // Create a Background from the whitesquare
            spriteBatch.Begin();
            spriteBatch.Draw(whiteSquare, new Vector2(0, 0), new Rectangle(0, 0, ScreenWidth, ScreenHeight), worldColour);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null, Camera.Transform);

            player.DrawLight(spriteBatch, lightMask);

            foreach (EnemyTank e in enemies)
                e.DrawLight(spriteBatch, lightMask);

            foreach (AdvancedEnemy aEnemy in advancedEnemies)
                aEnemy.DrawLight(spriteBatch, lightMask);

            foreach (EnemySoldier s in soldiers)
                s.DrawLight(spriteBatch, lightMask);

            float scale = (float)ExpSpriteWidth / lightMask.Width;
            scale *= 1.5f; //make it 50% bigger than the width of the texture
            foreach (Sprite exp in explosions) //draw light at each explosion
            {
                float opacity = 1f;  //fade in and fade out with this
                if (exp.CurrentFrame < 5)
                    opacity = ((float)exp.CurrentFrame / 5);
                spriteBatch.Draw(lightMask, exp.Position, null, Color.White * opacity, 0f, new Vector2(lightMask.Width / 2, lightMask.Height / 2), scale, SpriteEffects.None, 1f);
            }

            wrenchDrop.DrawLight(spriteBatch, lightMask);
            droneDrop.DrawLight(spriteBatch, lightMask);

            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);


        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        ///
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            switch (currentMode)
            {
                case Mode.Splash: //if the current gamemode is set to splash 
                    DrawSplash(); //draw the splash screen
                    break;
                case Mode.Game:
                    DrawGame(gameTime);
                    break;
                case Mode.Pause:
                    DrawGame(gameTime);
                    DrawPause();
                    break;
                case Mode.GameOver:
                    DrawGameOver();
                    break;
                case Mode.Options:
                    DrawOptions();
                    break;

            }
            base.Draw(gameTime);
        }

        private void DrawSplash()
        {//during splash mode
            spriteBatch.Begin();
            spriteBatch.Draw(splashBG, new Rectangle(0, 0, ScreenWidth, ScreenHeight), Color.White);
            if (showInfo) //if player clicked the show info button
            {
                spriteBatch.Draw(infoTexture, infoRect, Color.White); //draw info box which contains instructions
                buttons[mainMenu].Draw(spriteBatch);
            }
            else //not in the info screen
            {
                buttons[start].Draw(spriteBatch);
                buttons[options].Draw(spriteBatch);
                buttons[exit].Draw(spriteBatch);
                buttons[info].Draw(spriteBatch);
            }
            spriteBatch.End();
        }

        private void DrawGame(GameTime gameTime)
        {//draw this during game play

            //http://blog.josack.com/2011/07/xna-2d-dynamic-lighting.html
            GraphicsDevice.Clear(Color.CornflowerBlue);
            // TODO: Add your drawing code here

            DrawGameMain(gameTime);
            DrawGameLightMask(gameTime);

            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            lightingEffect.Parameters["lightMask"].SetValue(lightMaskRT);
            lightingEffect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(mainSceneRT, new Vector2(0, 0), Color.White);
            spriteBatch.End();


            spriteBatch.Begin();
            scoreKeeper.Draw(spriteBatch);
            //positions of enemies to be passed in to map:
            List<Vector2> enemiesPos = new List<Vector2>();
            List<Vector2> advancedEnemiesPos = new List<Vector2>();
            List<Vector2> soldiersPos = new List<Vector2>();
            foreach (EnemyTank e in enemies)
                enemiesPos.Add(new Vector2(e.Position.X, e.Position.Y));
            foreach (AdvancedEnemy aEnemy in advancedEnemies)
                advancedEnemiesPos.Add(new Vector2(aEnemy.Position.X, aEnemy.Position.Y));
            foreach (EnemySoldier s in soldiers)
                soldiersPos.Add(new Vector2(s.Position.X, s.Position.Y));

            map.Draw(spriteBatch, enemiesPos, advancedEnemiesPos, soldiersPos, new Vector2(player.Position.X, player.Position.Y));
            if (scoreKeeper.DisplayLifeUpMessage) //scorekeeper doesnt update so this has to be here and not in scorekeeper
            {
                lifeUpMessageTimer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                spriteBatch.DrawString(lifeUpFont, "Life Up!", new Vector2(ScreenWidth / 2 - 60, ScreenHeight / 2), Color.Yellow);
                if (lifeUpMessageTimer < 0)
                {
                    lifeUpMessageTimer = LifeUpMessageMax;
                    scoreKeeper.DisplayLifeUpMessage = false;
                }
            }
            spriteBatch.DrawString(font, "Wave: " + (curWave + 1), new Vector2(230, 10), Color.White);
            spriteBatch.End();
            
        }

        private void DrawPause()
        { //draw this during pause
            spriteBatch.Begin();  //draw necessary buttons
            buttons[continueB].Draw(spriteBatch, PauseOpacity);
            buttons[mainMenu].Draw(spriteBatch, PauseOpacity);
            buttons[options].Draw(spriteBatch, PauseOpacity);
            buttons[exit].Draw(spriteBatch, PauseOpacity);
            spriteBatch.End();
        }


        private void DrawOptions()
        {

            spriteBatch.Begin();
            string onOrOff = ""; //string to show if music/sounds is off or on

            spriteBatch.Draw(gameOverOptionsBackground, new Rectangle(0, 0, ScreenWidth, ScreenHeight), Color.White);//background
            //draw buttons
            buttons[music].Draw(spriteBatch);
            buttons[sounds].Draw(spriteBatch);
            scoreKeeper.DrawHighScores(spriteBatch);

            if (showContinue) //if player can continue (came from pause menu)
            {
                buttons[continueB].Draw(spriteBatch);
            }
            else //came from main menu (splash)
            {//change the position of the main menu button for this screen
                buttons[mainMenu].Draw(spriteBatch);
            }

            if (playMusic)
                onOrOff = "On"; //music can be played
            else
                onOrOff = "Off";
            spriteBatch.DrawString(font2, onOrOff, new Vector2(buttons[music].Position.X + 180, buttons[music].Position.Y + 21), Color.White);
            if (playSounds)
                onOrOff = "On"; //sounds can be played
            else
                onOrOff = "Off";
            spriteBatch.DrawString(font2, onOrOff, new Vector2(buttons[sounds].Position.X + 180, buttons[sounds].Position.Y + 21), Color.White);
            spriteBatch.End();
        }

        private void DrawGameOver()
        {//draw this when the mode is gameover

            spriteBatch.Begin();
            spriteBatch.Draw(gameOverOptionsBackground, new Rectangle(0, 0, ScreenWidth, ScreenHeight), Color.White); //draw backgrund


            spriteBatch.Draw(displayTex, new Rectangle(125, 160, 190, 90), Color.White * 0.8f); //draw the infoTexture where the strings will be drawn on top of
            if (playerName != null) //error checking
                spriteBatch.DrawString(font, "Name: " + playerName, new Vector2(135, 165), Color.SteelBlue); //display the players name
            //display the score the player got
            spriteBatch.DrawString(font, "Score: " + String.Format("{0:D4}", scoreKeeper.Score), new Vector2(135, 195), Color.SteelBlue);
            spriteBatch.DrawString(font, "Wave: " + (curWave + 1), new Vector2(135, 225), Color.SteelBlue);

            if (nameEntered == false) //if the player has entered their name yet
            { //tell the player to enter their name
                spriteBatch.Draw(displayTex, new Rectangle(130, 85, 160, 45), Color.White * 0.8f);
                spriteBatch.DrawString(bigFont, "Enter Name: ", new Vector2(140, 95), Color.SteelBlue); //display instrcutons
                buttons[mainMenu].Draw(spriteBatch, 0.5f); //draw at 50% opacity
            }
            else //name entered
            {
                buttons[mainMenu].Draw(spriteBatch);
            }

            scoreKeeper.DrawHighScores(spriteBatch);// highscore table draws itself
            spriteBatch.End();
        }

        private void DetectCollisions()
        {
            //enemies collisions
            for (int i = 0; i < enemies.Count; i++)
            {
                if (player.BS.Intersects(enemies[i].BS))    //player and enemies colliding
                {
                    if (IntersectsRects(player.RRect, enemies[i].RRect))
                    {
                        enemies[i].Alive = false;
                        scoreKeeper.ChangeHealth(-PlayerColDamage); //lose health
                    }
                }
                for (int j = 0; j < enemies.Count; j++)
                {
                    if (i != j)
                    {
                        if (enemies[i].NextBS.Intersects(enemies[j].BS))  //enemy and enemy collisions
                        {
                            enemies[i].Intersect1 = true;
                            break;
                        }
                        else
                        {
                            enemies[i].Intersect1 = false;
                        }
                    }
                }

                for (int j = 0; j < advancedEnemies.Count; j++)
                {
                    if (enemies[i].NextBS.Intersects(advancedEnemies[j].BS))  // enemies and advanced enemy collisions
                    {
                        enemies[i].Intersect2 = true;
                        break;
                    }
                    else
                    {
                        enemies[i].Intersect2 = false;
                    }
                }

                for (int j = 0; j < player.Bullets.Count; j++) //player bullets and enemy collisions
                {
                    if (enemies[i].BS.Intersects(player.Bullets[j].BS))
                    {
                        enemies[i].ChangeHealth(-player.BulletDamage);
                        player.Bullets[j].Alive = false;
                        scoreKeeper.ChangeScore(enemies[i].ScoreAmount*(curWave+1) / 5); //higher score for higher waves
                    }
                }

                for (int j = 0; j < enemies[i].Bullets.Count; j++) //loop through all bullets in each enemy
                {
                    if (enemies[i].Bullets[j].BS.Intersects(player.BS)) //if the bullet is colliding with the player
                    {
                        scoreKeeper.ChangeHealth(-enemies[i].BulletDamage);
                        enemies[i].Bullets[j].Alive = false;
                    }
                }
            }

            //advanced enemies collisons
            for (int i = 0; i < advancedEnemies.Count; i++)
            {
                if (player.BS.Intersects(advancedEnemies[i].BS))    //player and enemies colliding
                {
                    if (IntersectsRects(player.RRect, advancedEnemies[i].RRect))
                    {
                        advancedEnemies[i].Alive = false;
                        scoreKeeper.ChangeHealth(-PlayerColDamage); //lose health
                    }
                }


                for (int j = 0; j < advancedEnemies.Count; j++)
                {
                    if (i != j)
                    {
                        if (advancedEnemies[i].NextBS.Intersects(advancedEnemies[j].BS))  //advanced enemies and advanced enemy collisions
                        {
                            advancedEnemies[i].Intersect1 = true;
                            break;
                        }
                        else
                        {
                            advancedEnemies[i].Intersect1 = false;
                        }
                    }
                }
                for (int j = 0; j < enemies.Count; j++)
                {
                    if (advancedEnemies[i].NextBS.Intersects(enemies[j].BS))  // advanced enemies and  enemy collisions
                    {
                        advancedEnemies[i].Intersect2 = true;
                        break;
                    }
                    else
                    {
                        advancedEnemies[i].Intersect2 = false;
                    }
                }

                for (int j = 0; j < soldiers.Count; j++)
                {
                    if (advancedEnemies[i].NextBS.Intersects(soldiers[j].BS))  //advanced enemies and soldiers collisions
                    {
                        advancedEnemies[i].Intersect3 = true;
                        break;
                    }
                    else
                    {
                        advancedEnemies[i].Intersect3 = false;
                    }
                }


                for (int j = 0; j < player.Bullets.Count; j++) //player bullets and enemy collisions
                {
                    if (advancedEnemies[i].BS.Intersects(player.Bullets[j].BS))
                    {
                        advancedEnemies[i].ChangeHealth(-player.BulletDamage);
                        player.Bullets[j].Alive = false;
                        scoreKeeper.ChangeScore(advancedEnemies[i].ScoreAmount * (curWave + 1) / 5);
                    }
                }

                for (int j = 0; j < advancedEnemies[i].Bullets.Count; j++) //loop through all bullets in each enemy
                {
                    if (advancedEnemies[i].Bullets[j].BS.Intersects(player.BS)) //if the bullet is colliding with the player
                    {
                        scoreKeeper.ChangeHealth(-advancedEnemies[i].BulletDamage);
                        advancedEnemies[i].Bullets[j].Alive = false;
                    }
                }
            }

            //soldier collisions
            for (int i = 0; i < soldiers.Count; i++)
            {
                if (player.BS.Intersects(soldiers[i].BS))    //player and soldiers colliding
                {
                    soldiers[i].Alive = false;
                    scoreKeeper.ChangeHealth(-PlayerColDamage / 3); //lose health
                }
                for (int j = 0; j < soldiers.Count; j++)
                {
                    if (i != j)
                    {
                        if (soldiers[i].NextBS.Intersects(soldiers[j].BS))  //soldiers and soldiers collisions
                        {
                            soldiers[i].Intersect1 = true;
                            break;
                        }
                        else
                        {
                            soldiers[i].Intersect1 = false;
                        }
                    }
                }

                for (int j = 0; j < enemies.Count; j++)
                {
                    if (soldiers[i].NextBS.Intersects(enemies[j].BS))  // soldiers and enemy collisions
                    {
                        soldiers[i].Intersect2 = true;
                        break;
                    }
                    else
                    {
                        soldiers[i].Intersect2 = false;
                    }
                }

                for (int j = 0; j < advancedEnemies.Count; j++)
                {
                    if (soldiers[i].NextBS.Intersects(advancedEnemies[j].BS))  // soldiers and advanced enemy collisions
                    {
                        soldiers[i].Intersect3 = true;
                        break;
                    }
                    else
                    {
                        soldiers[i].Intersect3 = false;
                    }
                }

                for (int j = 0; j < player.Bullets.Count; j++) //player bullets and soldiers collisions
                {
                    if (soldiers[i].BS.Intersects(player.Bullets[j].BS))
                    {
                        soldiers[i].ChangeHealth(-player.BulletDamage);
                        player.Bullets[j].Alive = false;
                        scoreKeeper.ChangeScore(soldiers[i].ScoreAmount * (curWave + 1) / 5);
                    }
                }

                for (int j = 0; j < soldiers[i].Bullets.Count; j++) //loop through all bullets in each enemy
                {
                    if (soldiers[i].Bullets[j].BS.Intersects(player.BS)) //if the bullet is colliding with the player
                    {
                        scoreKeeper.ChangeHealth(-soldiers[i].BulletDamage);
                        soldiers[i].Bullets[j].Alive = false;
                    }
                }
            }


            //check if the player has collided with the wrench object
            if (wrenchDrop.BS.Intersects(player.BS) && wrenchDrop.Alive)
            {
                wrenchDrop.Reset();
                scoreKeeper.ChangeHealth(WrenchRepairAmount); //increase armour by this amount
            }

            if (droneDrop.BS.Intersects(player.BS) && droneDrop.Alive)
            { //has collided with droneDrop
                droneDrop.Reset();
                healDrone.Activate(); //activate the drone
                scoreKeeper.EnableRegen(); //enable regen
            }     


        }

        //collison
        private bool IntersectsRects(RotatedRectangle a, RotatedRectangle b)
        { //http://www.gamedev.net/page/resources/_/technical/game-programming/2d-rotated-rectangle-collision-r2604
            //http://gamedevelopment.tutsplus.com/tutorials/collision-detection-using-the-separating-axis-theorem--gamedev-169
            //4 axis required for 2 rectangles 
            Vector2 axis1 = a.UpperRight - a.UpperLeft;
            Vector2 axis2 = a.UpperRight - a.LowerRight;
            Vector2 axis3 = b.UpperLeft - b.LowerLeft;
            Vector2 axis4 = b.UpperRight - b.UpperLeft;
            if (CheckAxis(axis1, a, b) || CheckAxis(axis2, a, b) ||
                CheckAxis(axis2, a, b) || CheckAxis(axis4, a, b)) //if any axis is seperated then 
                return false;                                   //not colliding
            else
                return true;
        }

        private Vector2 GetMinMax(Vector2 axis, RotatedRectangle r)
        {
            Vector2 projUR = (Vector2.Dot(r.UpperRight, axis) / axis.LengthSquared()) * axis;
            Vector2 projUL = (Vector2.Dot(r.UpperLeft, axis) / axis.LengthSquared()) * axis;
            Vector2 projLR = (Vector2.Dot(r.LowerRight, axis) / axis.LengthSquared()) * axis;
            Vector2 projLL = (Vector2.Dot(r.LowerLeft, axis) / axis.LengthSquared()) * axis;

            float dotProjUR = Vector2.Dot(axis, projUR);
            float dotProjUL = Vector2.Dot(axis, projUL);
            float dotProjLR = Vector2.Dot(axis, projLR);
            float dotProjLL = Vector2.Dot(axis, projLL);

            float min = Math.Min(Math.Min(dotProjUL, dotProjUR), Math.Min(dotProjLL, dotProjLR));
            float max = Math.Max(Math.Max(dotProjUL, dotProjUR), Math.Max(dotProjLL, dotProjLR));

            return new Vector2(min, max);
        }

        private bool CheckAxis(Vector2 axis, RotatedRectangle a, RotatedRectangle b)
        {
            Vector2 aMinMax = GetMinMax(axis, a);
            Vector2 bMinMax = GetMinMax(axis, b);
            //x is min and y is max
            if (bMinMax.X >= aMinMax.Y || bMinMax.Y <= aMinMax.X)
                return true; //seperated ( not colliding)
            else
                return false;
        }
    }
}
