
/* 
        Game Title:
            Vörb

        Creator:
            Maxence Roy - 1957042
  
        Latest update: 
            December 11th, 2019 - 11:57 PM
            Version 1.0.0.0

        Changes made in this update:
            Game finished.

        Problems to fix:
            (Nothing)

        Next to do:
            (Nothing)
                        
        Possible improvements:
            Reorganize and order the code segments. Use brackets to classify functions. Put functions in a clear order.
            Create a function that automatically centers a string. It would prevent always doing (screensize - stringsize) / 2.
            Create a MoveDirection function to avoid always making a switch statement for moving an object up, down, left or right.
            Make an Enemy class. So Crawler is both Enemy and MovingObject - https://stackoverflow.com/questions/11134832/inherit-from-two-classes-in-c-sharp
            If want to make the game multiple, could be useful to make player a class.
            If want to make multiple games in one program, could make a class "Game".
            Add buttons to press on the maps and open up paths.
            Add sounds (.wav files).
            Add Easter eggs (secrets).
            Create High Scores. At the end of a game, the player's score is saved and added to a list.
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameProject_Programming1
{
    // MAPS ARE LOCATED AT THE BOTTOM

    // Making the variable 'Choice' allows me to read inputs without having to rely on ConsoleKeys (that way, I can easily add other input devices). This variable is also used to designate directions (up, down, left, right)
    enum Choice 
    {
         Up, Down, Left, Right, Stop, Shoot, Quit
    }

    class Object // Object is class that is inherited (pass the contents) by all object-related (pellets, enemies, etc) classes
    {
        public int PosX, PosY;
        public Object(int assignedPosX, int assignedPosY) // This is a constructor
        {
            PosX = assignedPosX;
            PosY = assignedPosY;
        }
    }

    class MovingObject : Object
    {
        public int PreviousPosX, PreviousPosY;
        public int MoveDistance;
        public ulong Speed;
        public ulong Iteration = 0; // Serves as a way to measure time. 1 iteration = 1 loop of the game (about a frame)
        /*
            Iteration has a flaw: the speed is not consistent, as it depends on how fast the computer can go through the game loop. A slower computer will therefore have slower moving objects.
            I decided to use this over Thread.Sleep, however, because this method doesn't not temporarily stop the loop of the game, meaning bullets, for instance, will go a the same speed, but can be updated at different times.
            Another way to do this would be to make a "Task", which runs seperately from the main loop, but this could be unreliable as this does not guarantee the functions will be synchronized.
        */
        public void Move(Choice direction)
        {
            PreviousPosX = PosX;
            PreviousPosY = PosY;
            switch (direction)
            {
                case Choice.Up:
                    PosY -= MoveDistance;
                    break;
                case Choice.Down:
                    PosY += MoveDistance;
                    break;
                case Choice.Left:
                    PosX -= MoveDistance;
                    break;
                case Choice.Right:
                    PosX += MoveDistance;
                    break;
            }
        }
        public bool CanMove()
        {
            if (Iteration % Speed == 1) // Iteration starts with value 1
            {
                return true;
            }
            return false;
        }
        public MovingObject(int assignedPosX, int assignedPosY, int asssignedMoveDistance, ulong assignedSpeed) : base(assignedPosX, assignedPosY) // Inherits the properties of the constuctor (function Object()) of the Object class using 'base'
        {
            MoveDistance = asssignedMoveDistance;
            Speed = assignedSpeed;
        }
    }

    class Wall : Object
    {
        public Wall(int assignedPosX, int assignedPosY) : base(assignedPosX, assignedPosY) { }
    }

    class Bullet : MovingObject
    {
        public Choice Direction;
        public bool Particle, Friendly; // Particle is a completely different object. It only exists for visual effect and does not interact with anthing. I decided to make particles "bullets" because they behave the same way.
        public Bullet(int assignedPosX, int assignedPosY, int asssignedMoveDistance, ulong assignedSpeed, Choice assignedDirection, bool isFriendly, bool isParticle) : base(assignedPosX, assignedPosY, asssignedMoveDistance, assignedSpeed)
        {
            Direction = assignedDirection;
            Friendly = isFriendly;
            Particle = isParticle;
        }
    }

    class Pellet : Object
    {
        public Pellet(int assignedPosX, int assignedPosY) : base(assignedPosX, assignedPosY) { } 
    }

    class Crawler : MovingObject
    {
        public int Range;
        public int Health;
        public ulong InvTime;
        public ulong InvLimit;
        public bool Invincible = false;
        public Crawler(int assignedPosX, int assignedPosY, int asssignedMoveDistance, ulong assignedSpeed, int assignedRange, int assignedHealth, ulong assignedInvTime) : base(assignedPosX, assignedPosY, asssignedMoveDistance, assignedSpeed)
        {
            Range = assignedRange;
            Health = assignedHealth;
            InvTime = assignedInvTime;
        }
        public bool Damage(int healthLoss) // Remove health from the enemy
        {
            if (Iteration > InvLimit)
            {
                Invincible = false;
            }
            if (!Invincible)
            {
                Health -= healthLoss;
                Invincible = true; // The enemy gains temporary invincibility to prevent player from defeating it by repeatedly shooting bullets
                InvLimit = Iteration + InvTime; // The invincibility's duration is InvTime
                return true;
            }
            return false;
        }
    }

    class Shooter : Object
    {
        public ulong Iteration = 0;
        public int Health;
        public ulong InvTime;
        public ulong InvLimit;
        public bool Invincible = false;
        public Shooter(int assignedPosX, int assignedPosY, int assignedHealth, ulong assignedInvTime) : base(assignedPosX, assignedPosY)
        {
            Health = assignedHealth;
            InvTime = assignedInvTime;
        }
        public bool Damage(int healthLoss) // Remove health from the enemy
        {
            if (Iteration > InvLimit)
            {
                Invincible = false;
            }
            if (!Invincible)
            {
                Health -= healthLoss;
                Invincible = true; // The enemy gains temporary invincibility to prevent player from defeating it by repeatedly shooting bullets
                InvLimit = Iteration + InvTime; // The invincibility's duration is InvTime
                return true;
            }
            return false;
        }
    }


    class Program
    {
        // CONSTANTS
            // Screen-related constants
        const int DesiredScreenSizeX = 152, DesiredScreenSizeY = 52;
        const int LeftBorder = 1, UpBorder = 5, RightBorder = DesiredScreenSizeX - 2, DownBorder = DesiredScreenSizeY - 3;
        static readonly int MapXSize = RightBorder - LeftBorder + 1, MaxYSize = DownBorder - UpBorder + 1,
            MapArea = (MapXSize) * (MaxYSize); // Should be 150 x 45

            // Menu-related constants
        const bool TEST_MODE = false; // TEST_MODE removes all enemies and pellets, and allows the player to complete a level without doing anything
        const int NumLevels = 4;
        const ConsoleColor ColorRed = ConsoleColor.Red, ColorBlue = ConsoleColor.Blue, ColorGreen = ConsoleColor.Green, ColorYellow = ConsoleColor.Yellow, ColorMagenta = ConsoleColor.Magenta, ColorCyan = ConsoleColor.Cyan,
            ColorDarkRed = ConsoleColor.DarkRed, ColorDarkBlue = ConsoleColor.DarkBlue, ColorDarkGreen = ConsoleColor.DarkGreen, ColorDarkYellow = ConsoleColor.DarkYellow, ColorDarkMagenta = ConsoleColor.DarkMagenta, ColorDarkCyan = ConsoleColor.DarkCyan,
            ColorWhite = ConsoleColor.White, ColorGray = ConsoleColor.Gray, ColorDarkGray = ConsoleColor.DarkGray, ColorBlack = ConsoleColor.Black;
        static readonly ConsoleColor[] TitleColors = new ConsoleColor[] { ColorDarkGreen, ColorGreen, ColorGreen, ColorWhite, ColorCyan, ColorCyan, ColorDarkCyan };
        static readonly string[] Title = new string[]
        {
            "oooooo     oooo    o8o   o8o                   .o8      ",
            " `888.     .8'      `'    `'                  '888      ",
            "  `888.   .8'       .ooooo.      oooo d88b     888oooo. ",
            "   `888. .8'       d88' `88b     `888'''8P     d88' `88b",
            "    `888.8'        888   888      888          888   888",
            "     `888'         888   888      888          888   888",
            "      `8'          `Y8bod8P'     d888b         `Y8bod8P'"
        };
        const float TitleColorChangeTime = 20000, // in iterations
            BlackoutTime = 0.01f, // in seconds
            TransScreenTime = 1, // in seconds
            ScoreSectionTime = 0.5f, // in seconds
            ScoreCountTime = 0.01f; // in seconds
        static readonly int TitleScreenSizeX = 56, TitleScreenSizeY = Title.Length,
            TitleScreenPosX = (DesiredScreenSizeX - TitleScreenSizeX) / 2, TitleScreenPosY = (DesiredScreenSizeY - TitleScreenSizeY) - 33;
        const string Author = "A game made by Maxence Roy (2019)";
        static readonly int AuthorPosX = (DesiredScreenSizeX - Author.Length) / 2, AuthorPosY = 26;
        static readonly string[] MenuOptions = new string[] { "Enter one of the options.", "1. Play", "2. How to play", "3. Quit" };
        static readonly int MenuPosY = 30;
        static readonly ConsoleColor[] HowToPlayTextColors = new ConsoleColor[]
        {
            ColorWhite, // How To Play TitleX
            ColorDarkGray, ColorDarkGray, ColorDarkGray,
            ColorGreen, ColorGreen, ColorGreen, ColorGreen, // YOU section
            ColorDarkGray, ColorDarkGray,
            ColorCyan, ColorCyan, ColorCyan, ColorCyan, ColorCyan, // ITEMS section
            ColorDarkGray, ColorDarkGray,
            ColorRed, ColorRed, ColorRed, ColorRed, ColorRed, ColorRed, // ENEMIES seciton
            ColorDarkGray, ColorDarkGray,
            ColorYellow, ColorYellow, ColorYellow, ColorYellow, ColorYellow, ColorYellow, ColorYellow, ColorYellow, ColorYellow, // THE GOAL section
            ColorDarkGray, ColorDarkGray,
            ColorGray, ColorGray, // Other info
            ColorDarkGray, ColorDarkGray,
            ColorGray, // How to go back to menu
        };
        static readonly string[] HowToPlayText = new string[]
        {
            "H O W    T O    P L A Y",
            "",
            "",
            "",
            "YOU",
            "This is you: " + PlayerNormalAppearance,
            "Use the [WASD] or [Arrow Keys] to MOVE.",
            "Use the [Spacebar] or [E] to SHOOT a bullet.",
            "__________________________________________________________________________________________________________________________________",
            "",
            "ITEMS",
            "This is a PELLET: " + PelletAppearance,
            "This is the KEY: " + KeyAppearance,
            "This is the EXIT: " + ExitAppearance,
            "This is a HEALTH PACK, which will make you gain back health: " + HealthPackAppearance,
            "__________________________________________________________________________________________________________________________________",
            "",
            "ENEMIES",
            "This is a CRAWLER: " + CrawlerApperance,
            "Crawlers move towards you if you get too close, or if you stand in front of them.",
            "This is a SHOOTER: " + ShooterAppearance,
            "Shooters can't move, but they shoot bullets around themselves.",
            "Defeat enemies by shooting BULLETS at them.",
            "__________________________________________________________________________________________________________________________________",
            "",
            "THE GOAL",
            "To complete the GAME, complete every level.",
            "To complete a LEVEL, you have to collect HALF of the PELLETS and the KEY, and defeat a certain amount of ENEMIES.",
            "Then, you will be able to go through the EXIT. (All requirements on the top right corner of your screen will appear green.)",
            "Be careful! Enemies make you lose health.",
            "If you health reaches 0, you lose a life.",
            "If you run out of lives, it's GAME OVER.",
            "The levels get progressively harder, but they also become more rewarding!",
            "Your score will depend on many factors, such as the amount of enemies you defeated and how many pellets you have collected.",
            "__________________________________________________________________________________________________________________________________",
            "",
            "The game is designed around a specific screen size. Changing it at any time during the game will break it.",
            "You can LEAVE the game anytime by pressing the [ESCAPE] key.",
            "__________________________________________________________________________________________________________________________________",
            "",
            "Press any key to go back to the title screen.",
        };
        static readonly int HowToPlayTextSizeX = 130, HowToPlayTextSizeY = Title.Length,
            HowToPlayTextPosX = (DesiredScreenSizeX - HowToPlayTextSizeX) / 2, HowToPlayTextPosY = 5;
        static readonly string[] LevelTitles = new string[]
        {
            "Level 0", // should never appear
            "Garden Greens",
            "The Circuit",
            "Cloud Cavern",
            "FINAL LEVEL - The Catacombs of Darkness"
        };
        const string TransScreenTopTextConst = "LEVEL ",
            TransScreenBottomText = "Press any key to begin...",
            ScoreScreenBottomText = "Press any key to continue",
            TransScreenLivesText1 = "You have ",
            TransScreenLivesText2One = " life left!",
            TransScreenLivesText2Multi = " lives left",
            PointTo = " -> ",
            PointsText = " points",
            LevelEndScoreGameCompleteText = "GAME COMPLETE!!!",
            LevelEndScoreNoDeathBonus = "[BONUS] 0 Deaths",
            LevelEndScoreLevelCompleteText = "Level complete!",
            LevelEndScoreHealthText = "Health remaining: ",
            LevelEndScorePelletText = "Pellets collected: ",
            LevelEndScoreCrawlerText = "Crawlers killed: ",
            LevelEndScoreShooterText = "Shooters killed: ",
            LevelEndScoreTimeBonusText = "[BONUS] Quick Feet",
            LevelEndScoreHealthBonusText = "[BONUS] Juggernaut",
            LevelEndScoreCollectBonusText = "[BONUS] Completionist",
            LevelEndScoreKillBonusText = "[BONUS] Annihilator",
            LevelEndOldScoreText = "Your old score",
            LevelEndPointsText = "Points you got this level",
            LevelEndNewScoreText = "Your current score",
            DeathMessage = "YOU DIED!",
            GameEndScoreText = "Your final score is",
            GameEndLeaveText = "Press any key to go back to the title screen",
            OofStatus = "Oof...",
            EpicGamerStatus = "That's quite an epic gamer score!",
            Dot = ". ";
        static readonly string[] GameOverText = new string[] // Backslashes (\) need to doubled to be interpreted as one. What the string will actually look like is shown on the right.
        {           
            "   ______                                     ____                           ",            //    ______                                     ____                           
            "  / ____/  ____ _   ____ ___    ___          / __ \\   _   __   ___     ____  ",           //   / ____/  ____ _   ____ ___    ___          / __ \   _   __   ___     ____  
            " / / __   / __ `/  / __ `__ \\  / _ \\        / / / /  | | / /  / _ \\   / __/  ",         //  / / __   / __ `/  / __ `__ \  / _ \        / / / /  | | / /  / _ \   / __/  
            "/ /_/ /  / /_/ /  / / / / / / /  __/       / /_/ /   | |/ /  /  __/  / /   __",            // / /_/ /  / /_/ /  / / / / / / /  __/       / /_/ /   | |/ /  /  __/  / /   __
            "\\____/   \\__,_/  /_/ /_/ /_/  \\___/        \\____/    |___/   \\___/  /_/   /_/"        // \____/   \__,_/  /_/ /_/ /_/  \___/        \____/    |___/   \___/  /_/   /_/
        },
            GameWonText = new string[]
            {
                "__  __                        _       __                      __    __    __",         // __  __                        _       __                      __    __    __
                "\\ \\/ /  ____    __  __       | |     / /  ____     ____      / /   / /   / /",       // \ \/ /  ____    __  __       | |     / /  ____     ____      / /   / /   / /
                " \\  /  / __ \\  / / / /       | | /| / /  / __ \\   / __ \\    / /   / /   / / ",     //  \  /  / __ \  / / / /       | | /| / /  / __ \   / __ \    / /   / /   / / 
                " / /  / /_/ / / /_/ /        | |/ |/ /  / /_/ /  / / / /   /_/   /_/   /_/  ",         //  / /  / /_/ / / /_/ /        | |/ |/ /  / /_/ /  / / / /   /_/   /_/   /_/  
                "/_/   \\____/  \\__,_/         |__/|__/   \\____/  /_/ /_/   (_)   (_)   (_)   "       // /_/   \____/  \__,_/         |__/|__/   \____/  /_/ /_/   (_)   (_)   (_)   
            };
        static readonly int TransScreenTopTextPosX = (DesiredScreenSizeX - (TransScreenTopTextConst.Length + 1)) / 2, TransScreenTopTextPosY = 18,
            DeathMessagePosX = (DesiredScreenSizeX - (DeathMessage.Length + 1)) / 2,
            ScoreScreen1PosX = (DesiredScreenSizeX / 2) + 2, ScoreScreen1PosY = 6,
            ScoreScreenLineOffset = 2,
            ScoreScreen2YOffset = 4,
            ScoreScreenBottomTextOffset = 6,
            ScoreCountIterations = 100, // not a position
            TransScreenMiddleTextPosY = 20, // PosX will depend on the current level
            TransScreenBottomTextPosX = (DesiredScreenSizeX - TransScreenBottomText.Length) / 2, TransScreenBottomTextPosY = 36,
            GameOverTextSizeX = 81, GameOverTextSizeY = GameOverText.Length,
            GameOverTextPosX = (DesiredScreenSizeX - GameOverTextSizeX) / 2, GameOverTextPosY = (DesiredScreenSizeY - GameOverTextSizeY) - 40,
            GameWonTextSizeX = 76, GameWonTextSizeY = GameWonText.Length,
            GameWonTextPosX = (DesiredScreenSizeX - GameWonTextSizeX) / 2, GameWonTextPosY = (DesiredScreenSizeY - GameWonTextSizeY) - 40,
            GameEndScoreTextPosX = (DesiredScreenSizeX - GameEndScoreText.Length) / 2, GameEndScoreTextPosY = (DesiredScreenSizeY / 2) - 6,
            GameEndScoreDotsPosX = (DesiredScreenSizeX - 5) / 2,
            GameEndScoreSmallOffset = 3, GameEndScoreLargeOffset = 5,
            GameEndLeaveTextPosX = (DesiredScreenSizeX - GameEndLeaveText.Length) / 2, GameEndLeaveTextPosY = 42,
            OofStatusPosX = (DesiredScreenSizeX - OofStatus.Length) / 2,
            EpicGamerStatusPosX = (DesiredScreenSizeX - EpicGamerStatus.Length) / 2;

            // Ingame display constants
        static readonly ConsoleColor[] BackgroundColors = new ConsoleColor[] { ColorBlack, ColorDarkGreen, ColorDarkYellow, ColorDarkCyan, ColorBlack }, // In the following 2 arrays, the index is the level of the game (0 is the menu)
            ForegroundColors = new ConsoleColor[] { ColorWhite, ColorBlack, ColorGray, ColorMagenta, ColorDarkGray }; // Wall colors, basically
        const int LevelDisplayPosX = 20, LevelDisplayPosY = 1,
            ScoreDisplayPosX = 20, ScoreDisplayPosY = 3,
            HealthDisplayPosX = 50, HealthDisplayPosY = 1,
            LivesDisplayPosX = 50, LivesDisplayPosY = 3,
            PelletDisplayPosX = 80, PelletDisplayPosY = 1,
            PelletsCollectedDisplayPosX = 99, PelletsCollectedDisplayPosY = 1,
            KeyStatusDisplayPosX = 89, KeyStatusDisplayPosY = 3,
            GotKeyDisplayPosX = 80, GotKeyDisplayPosY = 3,
            CrawlersKilledDisplayPosX = 120, CrawlersKilledDisplayPosY = 1,
            CrawlersKilledNumDisplayPosX = 137, CrawlersKilledNumDisplayPosY = 1,
            ShootersKilledDisplayPosX = 120, ShootersKilledDisplayPosY = 3,
            ShootersKilledNumDisplayPosX = 137, ShootersKilledNumDisplayPosY = 3;
        const string LevelDisplayConstant = "Level: ",
            ScoreDisplayConstant = "Score: ",
            HealthDisplayConstant = "Health: ",
            LivesDisplayConstant = "Lives remaining: ",
            ParenthesisOpen = "(", Slash = "/", ParenthesisClose = ")",
            PelletsCollectedDisplayConstant = "Pellets collected: ",
            GotKeyDisplayConstant = "Got Key: ",
            KeyStatusPositive = "Yes", KeyStatusNegative = "NO",
            CrawlersKilledDisplayConstant = "Crawlers killed: ",
            ShootersKilledDisplayConstant = "Shooters killed: ";
        
            // Score-related Constants
        const int GameCompleteScore = 2000,
            LevelCompleteScoreFirstLevel = 400,
            LevelCompleteScoreFactor = 200,
            PelletScoreValue = 5,
            KeyScoreValue = 50,
            CrawlerScoreValue = 50,
            ShooterScoreValue = 30,
            HealthScoreValue = 2,
            TimeBonus = 200,
            HealthBonus = 250,
            CollectBonus = 200,
            KillBonus = 200,
            NoDeathBonus = 400,
            OofStatusRequirement = 0,
            EpicGamerStatusRequirement = 10000;
        const ulong TimeBonusRequirementFirstLevel = 4000000, // in iterations. Maximum value to obtain the bonus. 100000 is about 3 seconds, so this is very roughly around 120 seconds
            TimeBonusRequirementFactor = 1500000;

            // Key-related Constants. Make sure to change HowToPlayText if you change those
        static readonly ConsoleKey[] KeyStrokesForUpInput = new ConsoleKey[] { ConsoleKey.UpArrow, ConsoleKey.W },
            KeyStrokesForDownInput = new ConsoleKey[] { ConsoleKey.DownArrow, ConsoleKey.S },
            KeyStrokesForLeftInput = new ConsoleKey[] { ConsoleKey.LeftArrow, ConsoleKey.A },
            KeyStrokesForRightInput = new ConsoleKey[] { ConsoleKey.RightArrow, ConsoleKey.D },
            KeyStrokesForShootInput = new ConsoleKey[] { ConsoleKey.Spacebar, ConsoleKey.E },
            KeyStrokesForQuitInput = new ConsoleKey[] { ConsoleKey.Escape };

            // Object Appearance Constants
        const char EmptyAppearance = ' ',
            PlayerNormalAppearance = 'Ö',
            PlayerCollectingAppearance = 'ö',
            WallAppearance = '█',
            PelletAppearance = '.',
            KeyAppearance = 't',
            ExitAppearance = '@',
            CrawlerApperance = '+',
            ShooterAppearance = 'x',
            HealthPackAppearance = 'H',
            BulletHorAppearance = '—',
            BulletVerAppearance = '|',
            ParticleAppearance = '*';
        const ConsoleColor PositiveColor = ColorGreen,
            NegativeColor = ColorRed,
            HighlightColor = ColorYellow,
            PlayerColor = ColorWhite,
            PlayerDyingColor = ColorGray,
            PelletColor = ColorYellow,
            KeyColor = ColorWhite,
            ExitColor = ColorWhite,
            HealthPackColor = ColorGreen,
            EnemyColor = ColorRed,
            EnemyDyingColor = ColorDarkRed;

            // Object Stat Constants
        const Choice DefaultPlayerLookDirection = Choice.Up;
        const ulong DefaultPlayerInvTime = 30000, // Amount of time the player will be invincible after being damaged
            BulletSpeed = 1000, // Counter intuitive: lower = faster. Do not use speed 1, CanMove() is not designed for it
            PlayerShootDelay = 5000,
            DefaultCrawlerSpeed = 7000, CrawlerSpeedAdd = 500,
            DefaultEnemyInvTime = 10000,
            DefaultShooterBulletSpeed = 2000, ShooterBulletSpeedAdd = 100,
            DefaultShooterShootDelay = 100000, ShooterShootDelayAdd = 10000,
            ParticleMoveSpeedVer = 1500,
            ParticleMoveSpeedHor = 1000,
            HealthWarningBlinkDelay = 10000;
        const int ObjectSpawnDistanceFromPlayer = 8,
            DefaultPlayerWalkDistance = 1, // Be careful! If WalkDistance > 1, the player might be able to go over walls
            StartLives = 3,
            StartScore = 0,
            StartHealth = 100,
            BulletMoveDistance = 1, 
            PlayerBulletDamage = 1,
            NumPelletsDefault = 30,
            NumPelletAdd = 10, // How many pellets are added per level
            NumCrawlerDefault = 3, NumCrawlerAdd = 1, // How many crawlers are added per level
            DefaultCrawlerRange = 16, CrawlerRangeAdd = 1,
            DefaultCrawlerWalkDistance = 1,
            DefaultCrawlerHealth = 6, CrawlerHealthAdd = 2,
            DefaultCrawlerDamage = 8, CrawlerDamageAdd = 1,
            NumShooterDefault = 3, NumShooterAdd = 1,
            DefaultShooterHealth = 8, ShooterHealthAdd = 2,
            DefaultShooterBulletDamage = 10, ShooterBulletDamageAdd = 5,
            DefaultHealthPackGain = 20,
            HealthPackGainAdd = 10,
            ParticleStageLimitVer = 2,
            ParticleStageLimitHor = 3,
            ParticleMoveDistance = 1;
            

        // VARIABLES
        static Random RNG = new Random();

            // Outside the game
        static bool Quitting, GameComplete, LevelComplete, DiedOnce;
        static int Score, Lives, CurrentLevel;
        static ulong MenuIteration, Iteration; // Since iteration is updated every loop, I'm using the largest integer possible

            // Ingame - Player properties
        static bool GotKey, 
            GotHealthPack,
            CanExit, 
            PlayerPosSet,
            PlayerInvincible, 
            isCollecting, // if the player is currently standing on a collectable (key or pellet)
            HealthIsRed,
            LostHealth;
        static int ScoreGain,
            PreviousPlayerPosX, PreviousPlayerPosY, PlayerPosX, PlayerPosY,
            PlayerHealth,
            PlayerDyingHealth,
            PlayerWalkDistance = DefaultPlayerWalkDistance, // Player Speed = amount of 'spaces' a player moves in a single cycle
            PelletsCollected,
            PelletsNeeded,
            CrawlersKilled,
            ShootersKilled,
            CrawlerKillMin,
            ShooterKillMin;
        static Choice PlayerLookDirection = DefaultPlayerLookDirection;
        static ulong LastTimeShot,
            PlayerInvLimit;

            // Ingame - Objects
        static int NumPellets, NumCrawlers, NumShooters;
        static List<Wall> WallList = new List<Wall>();
        static List<Bullet> BulletAndParticleList = new List<Bullet>();
        static List<Pellet> PelletList = new List<Pellet>();
        static List<Crawler> CrawlerList = new List<Crawler>();
        static List<Shooter> ShooterList = new List<Shooter>();
        static bool KeyPosSet, ExitPosSet;
        static int KeyPosX, KeyPosY,
            ExitPosX, ExitPosY,
            HealthPackPosX, HealthPackPosY,
            CrawlerRange,
            CrawlerWalkDistance,
            CrawlerDamage,
            CrawlerHealth,
            CrawlerDyingHealth,
            ShooterBulletDamage,
            ShooterHealth,
            ShooterDyingHealth,
            HealthPackGain;
        static ulong EnemyInvTime,
            CrawlerSpeed,
            ShooterBulletSpeed,
            ShooterShootDelay,
            LastTimeShootersShot;

        // FUNCTIONS
        static void Main(string[] args) // The main function of the game: menu options.
        {
            PrepareGame();
            ConsoleKey userInput = ConsoleKey.D0; // Placeholder
            bool showMenu = true;
            MenuIteration = 1;
            do
            {
                MenuIteration++;
                if (showMenu || MenuIteration % TitleColorChangeTime == 1) // Immediately show the title screen if the player came back from another screen (like how to play), otherwise wait for the delay before changing the colors of the title screen.
                {
                    ShowTitleScreen(showMenu);
                }
                userInput = ReadKeyStrokeMenu();
                switch (userInput)
                {
                    case ConsoleKey.D1:
                        showMenu = true;
                        GameLoop();
                        break;
                    case ConsoleKey.D2:
                        showMenu = true;
                        ShowHowToPlay();
                        break;
                    case ConsoleKey.D3:
                    default:
                        showMenu = false;
                        //Console.Write("What you entered is not a valid option!");
                        break;
                }
            } while (userInput != ConsoleKey.D3);
        }

        // Returns a key that the user is pressing, otherwise the default value
        static ConsoleKey ReadKeyStrokeMenu()
        {
            if (Console.KeyAvailable) // Makes sure a key has been pressed
            {
                return Console.ReadKey(true).Key;
            }
            return default(ConsoleKey);
        }

        // Halts the program until a key is returned
        static void WaitForKeyStroke()
        {
            while(Console.KeyAvailable) // This loop removes whatever key value was stored -- Resolves problem: user can type before ReadKey() and it'll still be registered by the function.
            {
                Console.ReadKey(true);
            }
            Console.ReadKey(true);
        }

        static void GameLoop() // Function which contains the main loops of the game
        {
            Score = StartScore;
            Lives = StartLives;
            Quitting = false;
            GameComplete = false;
            DiedOnce = false;
            CurrentLevel = 1;
            bool playerDied = false;
            do // Loop for the game
            {
                LevelTransition(playerDied);
                PrepareLevel();
                Choice userInput;
                do // Loop for a single level
                {
                    Iteration++;
                    userInput = ReadKeyStrokeGame();
                    if (userInput != Choice.Quit)
                    {
                        bool movement = PlayerAct(userInput);
                        MoveOtherObjects();
                        BulletCollisionCheck();
                        CrawlerCollisionCheck();
                        UpdateScreen(movement);
                    }
                    else
                    {
                        Quitting = true; // Quitting returns to menu
                    }
                } while (!LevelOver(ref playerDied));
            } while (!GameOver());
            if (!Quitting)
            {
                if (GameComplete)
                {
                    ShowGameEndScreen(true);
                }
                else
                {
                    ShowGameEndScreen(false);
                }
            }
        }

        static void PrepareGame() // Set screen size and colors
        {
            Console.SetWindowSize(DesiredScreenSizeX, DesiredScreenSizeY); // ERROR IN CASE SCREEN SIZE CHANGES         
            Console.CursorVisible = false;
        }

        static void ShowTitleScreen(bool firstTime)
        {
            if (firstTime)
            {
                Quitting = false;
                CurrentLevel = 0;
                Console.BackgroundColor = BackgroundColors[CurrentLevel]; // Current level is 0 
                Console.Clear();
                Console.ForegroundColor = HighlightColor;
                Console.SetCursorPosition(AuthorPosX, AuthorPosY);
                Console.Write(Author);
                Console.ForegroundColor = ForegroundColors[CurrentLevel];
                for (int i = 0; i < MenuOptions.Length; i++)
                {
                    Console.SetCursorPosition((DesiredScreenSizeX - MenuOptions[i].Length) / 2, MenuPosY + i);
                    Console.Write(MenuOptions[i]);
                }
            }
            ulong offset = MenuIteration % (ulong)TitleScreenSizeY;
            for (int i = 0; i < TitleScreenSizeY; i++)
            {
                // Example: If offset is 2, what would normally be index 0 will be 2, and 5 will be 0. Even though we are dealing with single digit numbers, I must convert them to ulong so it is compatible with MenuIteration.
                Console.ForegroundColor = TitleColors[((ulong)i + offset) % (ulong)TitleScreenSizeY];
                Console.SetCursorPosition(TitleScreenPosX, TitleScreenPosY + i);
                Console.Write(Title[i]);
            }
            
        }

        static void ShowHowToPlay()
        {
            Console.Clear();
            for (int i = 0; i < HowToPlayText.Length; i++)
            {
                Console.ForegroundColor = HowToPlayTextColors[i];
                Console.SetCursorPosition(HowToPlayTextPosX, HowToPlayTextPosY + i);
                Console.Write(HowToPlayText[i]);
            }
            WaitForKeyStroke();
        }

        static void LevelTransition(bool died)
        {
            if (died)
            {
                Blackout(NegativeColor);
                Console.ForegroundColor = ColorWhite;
                Console.SetCursorPosition(DeathMessagePosX, TransScreenTopTextPosY);
                Console.Write(DeathMessage);
            }
            else
            {
                Console.BackgroundColor = BackgroundColors[CurrentLevel];
                Console.ForegroundColor = ForegroundColors[CurrentLevel];
                Console.Clear();
                Console.SetCursorPosition(TransScreenTopTextPosX, TransScreenTopTextPosY);
                Console.Write(TransScreenTopTextConst + CurrentLevel);
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(TransScreenTime)); // Wait for 1 second. This completely stops the application. Should not be used if you want other events to happen while it waits. (Create a "Task" instead).
                Console.SetCursorPosition((DesiredScreenSizeX - LevelTitles[CurrentLevel].Length) / 2, TransScreenMiddleTextPosY);
                Console.Write(LevelTitles[CurrentLevel]);
            } 
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(TransScreenTime));
            if (died)
            {
                if (Lives == 1)
                {
                    Console.SetCursorPosition(TransScreenBottomTextPosX + 1, TransScreenBottomTextPosY - 2);
                    Console.Write(TransScreenLivesText1 + Lives + TransScreenLivesText2One);
                }
                else
                {
                    Console.SetCursorPosition(TransScreenBottomTextPosX + 1, TransScreenBottomTextPosY - 2);
                    Console.Write(TransScreenLivesText1 + Lives + TransScreenLivesText2Multi);
                }
            }
            Console.SetCursorPosition(TransScreenBottomTextPosX, TransScreenBottomTextPosY);
            Console.Write(TransScreenBottomText);
            WaitForKeyStroke();
        }

        static void WriteScoreLine(string firstText, string theRest, ref int posY)
        {
            Console.SetCursorPosition(ScoreScreen1PosX - firstText.Length, posY);
            posY = posY + ScoreScreenLineOffset;
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(ScoreSectionTime));
            Console.Write(firstText + PointTo + theRest + PointsText);
        }

        static void Blackout(ConsoleColor background)
        {
            Console.BackgroundColor = background;
            for (int i = 0; i < Console.WindowHeight - 1; i++)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(BlackoutTime));
                Console.SetCursorPosition(0, i);
                Console.Write(new string(' ', Console.WindowWidth));
            }
            Console.Clear();
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(BlackoutTime));
        }
        
        // Set colors, title, score, borders, etc.
        static void PrepareLevel()
        {
            // Before drawing anything, reset and set values
            Iteration = 0;
            ScoreGain = 0;
            PlayerPosSet = false;
            KeyPosSet = false;
            ExitPosSet = false;
            PelletsCollected = 0;
            CrawlersKilled = 0;
            ShootersKilled = 0;  
            LevelComplete = false;
            PlayerHealth = StartHealth;
            PlayerDyingHealth = ((StartHealth - (StartHealth % 2)) / 2) - 10;
            PlayerInvincible = false;
            HealthIsRed = false;
            LostHealth = false;
            GotHealthPack = false;
            if (TEST_MODE)
            {
                CanExit = true;
                GotKey = true;
                NumPellets = 0;
                NumCrawlers = 0;
                NumShooters = 0;
            }
            else
            {
                CanExit = false;
                GotKey = false;
                NumPellets = NumPelletsDefault + ((CurrentLevel - 1) * NumPelletAdd);
                NumCrawlers = NumCrawlerDefault + ((CurrentLevel - 1) * NumCrawlerAdd);
                NumShooters = NumShooterDefault + ((CurrentLevel - 1) * NumShooterAdd);
            }
            PelletsNeeded = NumPellets / 2;
            CrawlerKillMin = CurrentLevel;
            ShooterKillMin = CurrentLevel;
            CrawlerRange = DefaultCrawlerRange + ((CurrentLevel - 1) * CrawlerRangeAdd);
            CrawlerWalkDistance = DefaultCrawlerWalkDistance;
            CrawlerSpeed = DefaultCrawlerSpeed - (((ulong)CurrentLevel - 1) * CrawlerSpeedAdd);
            CrawlerHealth = DefaultCrawlerHealth + ((CurrentLevel - 1) * CrawlerHealthAdd); 
            CrawlerDamage = DefaultCrawlerDamage + ((CurrentLevel - 1) * CrawlerDamageAdd);
            CrawlerDyingHealth = ((CrawlerHealth - (CrawlerHealth % 2)) / 2) - 1;
            ShooterShootDelay = DefaultShooterShootDelay - (((ulong)CurrentLevel - 1) * ShooterShootDelayAdd);
            ShooterHealth = DefaultShooterHealth + ((CurrentLevel - 1) * ShooterHealthAdd);
            ShooterBulletSpeed = DefaultShooterBulletSpeed - (((ulong)CurrentLevel - 1) * ShooterBulletSpeedAdd);
            ShooterBulletDamage = DefaultShooterBulletDamage + ((CurrentLevel - 1) * ShooterBulletDamageAdd);
            ShooterDyingHealth = ((ShooterHealth - (ShooterHealth % 2)) / 2) - 1;
            EnemyInvTime = DefaultEnemyInvTime;
            HealthPackGain = DefaultHealthPackGain + ((CurrentLevel - 1) * HealthPackGainAdd);
            WallList.Clear();
            BulletAndParticleList.Clear();
            PelletList.Clear();
            CrawlerList.Clear();
            ShooterList.Clear();
            // Draw objects
            DrawMap();
            UpdateLevelDisplay();
            UpdateScoreDisplay();
            UpdateLivesDisplay();
            UpdateHealthDisplay(false);
            DrawStat(PelletDisplayPosX, PelletDisplayPosY, ForegroundColors[CurrentLevel], PelletsCollectedDisplayConstant); // Only need to draw this once since it doesn't change
            UpdatePelletsCollectedDisplay();
            DrawStat(GotKeyDisplayPosX, GotKeyDisplayPosY, ForegroundColors[CurrentLevel], GotKeyDisplayConstant); 
            UpdateKeyStatusDisplay();
            DrawStat(CrawlersKilledDisplayPosX, CrawlersKilledDisplayPosY, ForegroundColors[CurrentLevel], CrawlersKilledDisplayConstant); 
            UpdateCrawlersKilledNumDisplay();
            DrawStat(ShootersKilledDisplayPosX, ShootersKilledDisplayPosY, ForegroundColors[CurrentLevel], ShootersKilledDisplayConstant);
            UpdateShootersKilledNumDisplay();
            GenerateAtRandom(out PlayerPosX, out PlayerPosY, ObjectSpawnDistanceFromPlayer); // Generate player
            UpdatePlayerDisplay();
            PlayerPosSet = true;
            for (int i = 0; i < NumPellets; i++) // Generate pellets
            {
                GenerateAtRandom(out int pelletPosX, out int pelletPosY, ObjectSpawnDistanceFromPlayer);
                Pellet newPellet = new Pellet(pelletPosX, pelletPosY);
                PelletList.Add(newPellet);
                DrawPellet(newPellet.PosX, newPellet.PosY);
            }
            GenerateAtRandom(out KeyPosX, out KeyPosY, ObjectSpawnDistanceFromPlayer); // Generate key
            DrawKey();
            KeyPosSet = true;
            GenerateAtRandom(out ExitPosX, out ExitPosY, ObjectSpawnDistanceFromPlayer); // Generate exit
            DrawExit();
            ExitPosSet = true;
            for (int i = 0; i < NumCrawlers; i++) // Generate enemies
            {
                GenerateAtRandom(out int crawlerPosX, out int crawlerPosY, CrawlerRange + 1);
                Crawler newCrawler = new Crawler(crawlerPosX, crawlerPosY, CrawlerWalkDistance, CrawlerSpeed, CrawlerRange, CrawlerHealth, EnemyInvTime);
                CrawlerList.Add(newCrawler);
                DrawCrawler(newCrawler.PosX, newCrawler.PosY, EnemyColor);
            }
            for (int i = 0; i < NumShooters; i++) // Generate enemies
            {
                GenerateAtRandom(out int shooterPosX, out int shooterPosY, ObjectSpawnDistanceFromPlayer);
                Shooter newShooter = new Shooter(shooterPosX, shooterPosY, ShooterHealth, EnemyInvTime);
                ShooterList.Add(newShooter);
                DrawShooter(newShooter.PosX, newShooter.PosY, EnemyColor);
            }
            GenerateAtRandom(out HealthPackPosX, out HealthPackPosY, ObjectSpawnDistanceFromPlayer); // Generate a health pack
            DrawHealthPack();
            SpawnParticles(PlayerPosX, PlayerPosY, true); // Start of game: indicate the position of the player with particles 
        }
        
        static void UpdateLevelDisplay()
        {
            DrawStat(LevelDisplayPosX, LevelDisplayPosY, ForegroundColors[CurrentLevel], LevelDisplayConstant + CurrentLevel);
        }

        static void UpdateScoreDisplay()
        {
            DrawStat(ScoreDisplayPosX, ScoreDisplayPosY, ForegroundColors[CurrentLevel], ScoreDisplayConstant + (Score + ScoreGain));
        }

        static void UpdateLivesDisplay()
        {
            DrawStat(LivesDisplayPosX, LivesDisplayPosY, ForegroundColors[CurrentLevel], LivesDisplayConstant + Lives);
        }

        static void UpdateHealthDisplay(bool changeHealth)
        {
            ConsoleColor color;
            color = ForegroundColors[CurrentLevel];
            if (changeHealth) // If change health is true, the color of health will change (blink).
            {
                if (HealthIsRed)
                {
                    HealthIsRed = false;
                }
                else
                {
                    color = NegativeColor;
                    HealthIsRed = true;
                }
            }
            else // If not change, the color will remain the same.
            {
                if (HealthIsRed)
                {
                    color = NegativeColor;
                }
            }
            DrawStat(HealthDisplayPosX, HealthDisplayPosY, color, HealthDisplayConstant + PlayerHealth);
        }

        static void UpdateCrawlersKilledNumDisplay()
        {
            ConsoleColor color;
            if (CrawlersKilled < CrawlerKillMin)
            {
                color = NegativeColor;
            }
            else
            {
                color = PositiveColor;
            }
            DrawStat(CrawlersKilledNumDisplayPosX, CrawlersKilledNumDisplayPosY, color, ParenthesisOpen + CrawlersKilled + Slash + NumCrawlers + ParenthesisClose);
        }

        static void UpdateShootersKilledNumDisplay()
        {
            ConsoleColor color;
            if (ShootersKilled < ShooterKillMin)
            {
                color = NegativeColor;
            }
            else
            {
                color = PositiveColor;
            }
            DrawStat(ShootersKilledNumDisplayPosX, ShootersKilledNumDisplayPosY, color, ParenthesisOpen + ShootersKilled + Slash + NumShooters + ParenthesisClose);
        }

        static void UpdatePelletsCollectedDisplay()
        {
            ConsoleColor color;
            if (PelletsCollected < PelletsNeeded)
            {
                color = NegativeColor;
            }
            else
            {
                color = PositiveColor;
            }
            DrawStat(PelletsCollectedDisplayPosX, PelletsCollectedDisplayPosY, color, ParenthesisOpen + PelletsCollected + Slash + NumPellets + ParenthesisClose);
        }

        static void UpdateKeyStatusDisplay()
        {
            string keyStatus;
            ConsoleColor color;
            if (GotKey)
            {
                keyStatus = KeyStatusPositive;
                color = PositiveColor;
            }
            else
            {
                keyStatus = KeyStatusNegative;
                color = NegativeColor;
            }
            DrawStat(KeyStatusDisplayPosX, KeyStatusDisplayPosY, color, keyStatus);
        }

        static void DrawStat(int posX, int posY, ConsoleColor color, string text)
        {
            Console.SetCursorPosition(posX, posY);
            Console.ForegroundColor = color;
            Console.Write(text + EmptyAppearance);
        }

        static void UpdatePlayerDisplay()
        {
            char playerAppearance;
            if (isCollecting)
            {
                playerAppearance = PlayerCollectingAppearance;
                isCollecting = false; // isCollecting is strictly used for appearance
            }
            else
            {
                playerAppearance = PlayerNormalAppearance;
            }
            DrawObject(PlayerPosX, PlayerPosY, GetPlayerColor(), playerAppearance);
        }

        static ConsoleColor GetPlayerColor()
        {
            if (PlayerHealth > PlayerDyingHealth)
            {
                return PlayerColor;
            }
            return PlayerDyingColor;
        }

        static void DrawPellet(int posX, int posY)
        {
            DrawObject(posX, posY, PelletColor, PelletAppearance);
        }

        static void DrawKey()
        {
            DrawObject(KeyPosX, KeyPosY, KeyColor, KeyAppearance);
        }

        static void DrawExit()
        {
            ConsoleColor color;
            if (CanExit)
            {
                color = HighlightColor;
            }
            else
            {
                color = ExitColor;
            }
            DrawObject(ExitPosX, ExitPosY, color, ExitAppearance);
        }

        static void DrawHealthPack()
        {
            DrawObject(HealthPackPosX, HealthPackPosY, HealthPackColor, HealthPackAppearance);
        }

        static void DrawCrawler(int posX, int posY, ConsoleColor color)
        {
            DrawObject(posX, posY, color, CrawlerApperance);
        }

        static void DrawShooter(int posX, int posY, ConsoleColor color)
        {
            DrawObject(posX, posY, color, ShooterAppearance);
        }

        static void DrawObject(int posX, int posY, ConsoleColor color, char appearance)
        {
            Console.SetCursorPosition(posX, posY);
            Console.ForegroundColor = color;
            Console.Write(appearance);
        }

        // Returns a random X and Y position that does not collide with any object
        static void GenerateAtRandom(out int posX, out int posY, int minDist)
        {
            bool validPos = false;
            do
            {
                posX = RNG.Next(LeftBorder + 1, RightBorder);
                posY = RNG.Next(UpBorder + 1, DownBorder);
                validPos = !CollisionCheck(posX, posY, minDist);
            } while (!validPos);
        }

        // Returns the distance between two points
        static int GetDistance(int posX1, int posY1, int posX2, int posY2)
        {
            return Math.Abs(posX1 - posX2) + Math.Abs(posY1 - posY2);
        }

        // Returns the distance between two points and the position that 1 should look at to reach 2
        static int GetDistanceWithDirection(int posX1, int posY1, int posX2, int posY2, out Choice direction, out bool sameXorY)
        {
            int xDist = Math.Abs(posX1 - posX2), yDist = Math.Abs(posY1 - posY2);
            int dist = xDist + yDist;
            sameXorY = false;
            if (posX1 == posX2 || posY1 == posY2)
            {
                sameXorY = true;
            }
            if (xDist > yDist) // If x and y distance are equal, x will be prioritized
            {
                if (posX1 > posX2)
                {
                    direction = Choice.Left;
                    return dist;
                }
                if (posX1 < posX2)
                {
                    direction = Choice.Right;
                    return dist;
                }
                direction = Choice.Stop;
            }
            else
            {
                if (posY1 > posY2)
                {
                    direction = Choice.Up;
                    return dist;
                }
                if (posY1 < posY2)
                {
                    direction = Choice.Down;
                    return dist;
                }
                direction = Choice.Stop;
            }
            return dist;

        }

        // Assures that position X and Y do not collide with other objects
        static bool CollisionCheck(int potentialPosX, int potentialPosY, int minDist)
        {
            if (PlayerPosSet)
            {
                // Makes sure objects don't spawn too close to player nor at any of the X or Y spawn position.
                if (GetDistance(PlayerPosX, PlayerPosY, potentialPosX, potentialPosY) < minDist || PlayerPosX == potentialPosX || PlayerPosY == potentialPosY)
                {
                    return true;
                }
            }
            if (IsTouchingWall(potentialPosX, potentialPosY))
            {
                return true;
            }
            if (IsTouchingPellet(potentialPosX, potentialPosY))
            {
                return true;
            }
            if (IsTouchingKey(potentialPosX, potentialPosY))
            {
                return true;
            }
            if (IsTouchingExit(potentialPosX, potentialPosY))
            {
                return true;
            }
            if (IsTouchingCrawler(potentialPosX, potentialPosY))
            {
                return true;
            }
            if (IsTouchingShooter(potentialPosX, potentialPosY))
            {
                return true;
            }
            return false;
        }

        // Returns whether or not X and Y is a wall
        static bool IsTouchingWall(int posX, int posY)
        {
            foreach(Wall wall in WallList)
            {
                if (wall.PosX == posX && wall.PosY == posY)
                {
                    return true;
                }
            }
            return false;
        }

        // Returns whether or not X and Y is a pellet
        static bool IsTouchingPellet(int posX, int posY)
        {
            foreach (Pellet pellet in PelletList)
            {
                if (pellet.PosX == posX && pellet.PosY == posY)
                {
                    return true;
                }
            }
            return false;
        }

        static bool IsTouchingKey(int posX, int posY)
        {
            if (KeyPosSet) // Make sure key pos has been defined, or else skip.
            {
                if (KeyPosX == posX && KeyPosY == posY)
                {
                    return true;
                }
            }
            return false;
        }

        static bool IsTouchingExit(int posX, int posY)
        {
            if (ExitPosSet)
            {
                if (ExitPosX == posX && ExitPosY == posY)
                {
                    return true;
                }
            }
            return false;
        }

        static bool IsTouchingCrawler(int posX, int posY)
        {
            foreach (Crawler crawler in CrawlerList)
            {
                if (crawler.PosX == posX && crawler.PosY == posY)
                {
                    return true;
                }
            }
            return false;
        }

        static bool IsTouchingShooter(int posX, int posY)
        {
            foreach (Shooter shooter in ShooterList)
            {
                if (shooter.PosX == posX && shooter.PosY == posY)
                {
                    return true;
                }
            }
            return false;
        }

        static bool IsTouchingHealthPack(int posX, int posY)
        {
            if(HealthPackPosX == posX && HealthPackPosY == posY) // Since Health Pack is the last object added, there is no need to check if the position has been set
            {
                return true;
            }   
            return false;
        }

        static Crawler GetCrawlerWithXandY(int posX, int posY)
        {
            foreach (Crawler crawler in CrawlerList)
            {
                if (crawler.PosX == posX && crawler.PosY == posY)
                {
                    return crawler;
                }
            }
            return null; // Not recommended to use the function if unsure that X and Y is an enemy. Will be inaccurate if allow enemies to be on the same position.
        }

        static Shooter GetShooterWithXandY(int posX, int posY)
        {
            foreach (Shooter shooter in ShooterList)
            {
                if (shooter.PosX == posX && shooter.PosY == posY)
                {
                    return shooter;
                }
            }
            return null;
        }

        // Returns a Choice variable (see enum Choice) according to the key pressed by the player
        static Choice ReadKeyStrokeGame()
        {
            ConsoleKey playerInput;
            if (Console.KeyAvailable) // Makes sure a key has been pressed
            {
                playerInput = Console.ReadKey(true).Key;
                foreach (ConsoleKey validInput in KeyStrokesForShootInput) // Check if Shoot Input
                {
                    if (playerInput == validInput)
                    {
                        return Choice.Shoot; // Important -- Since the fucntion only returns one input, therefore the player can't shoot AND move
                    }
                }
                foreach (ConsoleKey validInput in KeyStrokesForUpInput) // Check if Up Input
                {
                    if (playerInput == validInput)
                    {
                        return Choice.Up;
                    }
                }
                foreach (ConsoleKey validInput in KeyStrokesForDownInput) // Check if Down Input
                {
                    if (playerInput == validInput)
                    {
                        return Choice.Down;
                    }
                }
                foreach (ConsoleKey validInput in KeyStrokesForLeftInput) // Check if Left Input
                {
                    if (playerInput == validInput)
                    {
                        return Choice.Left;
                    }
                }
                foreach (ConsoleKey validInput in KeyStrokesForRightInput) // Check if Right Input
                {
                    if (playerInput == validInput)
                    {
                        return Choice.Right;
                    }
                }
                foreach (ConsoleKey validInput in KeyStrokesForQuitInput) // Check if Quit Input
                {
                    if (playerInput == validInput)
                    {
                        return Choice.Quit;
                    }
                }
            }
            return Choice.Stop; // Otherwise, the user has not enetered any valid key stroke.
        }

        // According to the input received, updates the position of the player within the GameBoard if it is not outside boundaries OR shoots. Also returns whether or not the player has actually moved.
        static bool PlayerAct(Choice direction)
        {
            int potentialPosX = PlayerPosX, potentialPosY = PlayerPosY; // Initially set as previous player pos, will get replaced if movement happens
            PreviousPlayerPosX = PlayerPosX;
            PreviousPlayerPosY = PlayerPosY;
            bool movement = false;
            switch (direction)
            {
                case Choice.Up:
                    potentialPosY = PlayerPosY - PlayerWalkDistance;
                    PlayerLookDirection = direction;
                    movement = true;
                    break;
                case Choice.Down:
                    potentialPosY = PlayerPosY + PlayerWalkDistance;
                    PlayerLookDirection = direction;
                    movement = true;
                    break;
                case Choice.Left:
                    potentialPosX = PlayerPosX - PlayerWalkDistance;
                    PlayerLookDirection = direction;
                    movement = true;
                    break;
                case Choice.Right:
                    potentialPosX = PlayerPosX + PlayerWalkDistance;
                    PlayerLookDirection = direction;
                    movement = true;
                    break;
                case Choice.Shoot: // Notice how Shoot does not make movement true
                    if (Iteration - LastTimeShot > PlayerShootDelay)
                    {
                        LastTimeShot = Iteration;
                        Bullet newPlayerBullet = new Bullet(PlayerPosX, PlayerPosY, BulletMoveDistance, BulletSpeed, PlayerLookDirection, true, false);
                        BulletAndParticleList.Add(newPlayerBullet);
                    }
                    break;
            }
            if (movement && IsTouchingWall(potentialPosX, potentialPosY)) // If the player wants to go in a direction, check if there's a wall
            {
                movement = false;
            }
            else
            {
                PlayerPosX = potentialPosX;
                PlayerPosY = potentialPosY;
            }
            // I still check if the player hit any pellet or keys, even if he hasn't moved. Maybe in the future those will move
            for (int i = PelletList.Count - 1; i >= 0; i--) // Collision check for pellets. The for loop is in reverse because deleting an element of an ArrayList reduces the other's index, which breaks the loop.
            {
                if (PelletList[i].PosX == PlayerPosX && PelletList[i].PosY == PlayerPosY) // Make positions match
                {
                    isCollecting = true;
                    PelletList.RemoveAt(i);
                    PelletsCollected++;
                    ScoreGain += PelletScoreValue;
                    UpdatePelletsCollectedDisplay();
                    UpdateScoreDisplay();
                    CheckCanExit();
                }
            }
            if (!GotKey && IsTouchingKey(PlayerPosX, PlayerPosY))
            {
                isCollecting = true;
                GotKey = true;
                ScoreGain += KeyScoreValue;
                UpdateKeyStatusDisplay();
                UpdateScoreDisplay();
                CheckCanExit();
            }
            if (!GotHealthPack && IsTouchingHealthPack(PlayerPosX, PlayerPosY))
            {
                GotHealthPack = true;
                PlayerHealth += HealthPackGain;
                UpdateHealthDisplay(false);
            }
            if (CanExit && IsTouchingExit(PlayerPosX, PlayerPosY))
            {
                LevelComplete = true;
            }
            return movement;
        }

        // Updates the positions of bullets and enemies
        static void MoveOtherObjects()
        {
            for (int i = BulletAndParticleList.Count - 1; i >= 0; i--) // Update bullets
            {
                Bullet bullet = BulletAndParticleList[i];
                bullet.Iteration++;
                if (bullet.CanMove())
                {
                    bullet.Move(bullet.Direction);
                    if (ParticleExpired(bullet) || IsTouchingWall(bullet.PosX, bullet.PosY)) // If bullet hits a wall, it disappears. If it is a particle, make it disppear after a certain time.
                    {
                        UpdateBulletAndParticleDisplay(bullet, true);
                        BulletAndParticleList.RemoveAt(i);
                    }
                }
            }
            foreach (Crawler crawler in CrawlerList)
            {
                // Crawlers only move when the player is within their range or if they have the same X or Y as them
                if (GetDistanceWithDirection(crawler.PosX, crawler.PosY, PlayerPosX, PlayerPosY, out Choice direction, out bool sameXorY) <= crawler.Range || sameXorY)
                {
                    crawler.Iteration++;
                    if (crawler.CanMove())
                    {
                        int potentialPosX = crawler.PosX, potentialPosY = crawler.PosY;
                        switch (direction)
                        {
                            case Choice.Up:
                                potentialPosY = crawler.PosY - crawler.MoveDistance;
                                break;
                            case Choice.Down:
                                potentialPosY = crawler.PosY + crawler.MoveDistance;
                                break;
                            case Choice.Left:
                                potentialPosX = crawler.PosX - crawler.MoveDistance;
                                break;
                            case Choice.Right:
                                potentialPosX = crawler.PosX + crawler.MoveDistance;
                                break;
                        }
                        if (IsTouchingWall(potentialPosX, potentialPosY) || IsTouchingCrawler(potentialPosX, potentialPosY)) // Check if there is a wall
                        {
                            break;
                        }
                        else
                        {
                            crawler.Move(direction);
                        }
                    }
                }
            }
            foreach (Shooter shooter in ShooterList)
            {
                shooter.Iteration++;
            }
            if (Iteration - LastTimeShootersShot > ShooterShootDelay)
            {
                LastTimeShootersShot = Iteration;
                foreach(Shooter shooter in ShooterList)
                {
                    Bullet newShooterBullet = new Bullet(shooter.PosX, shooter.PosY, BulletMoveDistance, ShooterBulletSpeed, Choice.Up, false, false);
                    BulletAndParticleList.Add(newShooterBullet);
                    newShooterBullet = new Bullet(shooter.PosX, shooter.PosY, BulletMoveDistance, ShooterBulletSpeed, Choice.Down, false, false);
                    BulletAndParticleList.Add(newShooterBullet);
                    newShooterBullet = new Bullet(shooter.PosX, shooter.PosY, BulletMoveDistance, ShooterBulletSpeed, Choice.Left, false, false);
                    BulletAndParticleList.Add(newShooterBullet);
                    newShooterBullet = new Bullet(shooter.PosX, shooter.PosY, BulletMoveDistance, ShooterBulletSpeed, Choice.Right, false, false);
                    BulletAndParticleList.Add(newShooterBullet);
                }
            }
        }

        static bool ParticleExpired(Bullet particle)
        {
            if (particle.Particle)
            {
                if (particle.Direction == Choice.Up || particle.Direction == Choice.Down)
                {
                    if (particle.Iteration > (ParticleStageLimitVer * particle.Speed))
                    {
                        return true;
                    }
                }
                else
                {
                    if (particle.Iteration > (ParticleStageLimitHor * particle.Speed))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // This function verifies if bullets collide with any object and affect those object accordingly
        static void BulletCollisionCheck()
        {
            foreach (Bullet bullet in BulletAndParticleList.ToList())
            {
                if (!bullet.Particle) // Particles do not affect other objects
                {
                    if (bullet.Friendly) // If the bullet belongs to the player...
                    {
                        for (int i = CrawlerList.Count - 1; i >= 0; i--)
                        {
                            Crawler crawler = CrawlerList[i];
                            if (bullet.PosX == crawler.PosX && bullet.PosY == crawler.PosY)
                            {
                                bool success = crawler.Damage(PlayerBulletDamage);
                                if (success)
                                {
                                    SpawnParticles(crawler.PosX, crawler.PosY, true);
                                }
                                if (crawler.Health <= 0) // Check if dead. If so, remove from game.
                                {
                                    UpdateCrawlerDisplay(crawler, true);
                                    CrawlerList.RemoveAt(i);
                                    CrawlersKilled++;
                                    UpdateCrawlersKilledNumDisplay();
                                    ScoreGain += CrawlerScoreValue;
                                    UpdateScoreDisplay();
                                    CheckCanExit();
                                }
                            }
                        }
                        for (int i = ShooterList.Count - 1; i >= 0; i--)
                        {
                            Shooter shooter = ShooterList[i];
                            if (bullet.PosX == shooter.PosX && bullet.PosY == shooter.PosY)
                            {
                                bool success = shooter.Damage(PlayerBulletDamage);
                                if (success)
                                {
                                    SpawnParticles(shooter.PosX, shooter.PosY, true);
                                }
                                if (shooter.Health <= 0) // Check if dead. If so, remove from game.
                                {
                                    FillCell(shooter.PosX, shooter.PosY, false, true, true, true, true, true, true);
                                    ShooterList.RemoveAt(i);
                                    ShootersKilled++;
                                    UpdateShootersKilledNumDisplay();
                                    ScoreGain += ShooterScoreValue;
                                    UpdateScoreDisplay();
                                    CheckCanExit();
                                }
                            }
                        }
                    }
                    else // If the bullet belongs to the enemy...
                    {
                        if (bullet.PosX == PlayerPosX && bullet.PosY == PlayerPosY)
                        {
                            DamagePlayer(ShooterBulletDamage);
                        }
                    }
                }
            }
        }

        // Works like bullets, but with crawlers instead
        static void CrawlerCollisionCheck()
        {
            foreach (Crawler crawler in CrawlerList)
            {
                if (crawler.PosX == PlayerPosX && crawler.PosY == PlayerPosY)
                {
                    DamagePlayer(CrawlerDamage);
                }
            }
        }

        // Decrease the player's health
        static void DamagePlayer(int healthLoss)
        {
            if (Iteration > PlayerInvLimit)
            {
                PlayerInvincible = false;
            }
            if (!PlayerInvincible)
            {
                LostHealth = true;
                PlayerHealth -= healthLoss;
                PlayerInvincible = true;
                PlayerInvLimit = Iteration + DefaultPlayerInvTime;
                UpdateHealthDisplay(false);
                SpawnParticles(PlayerPosX, PlayerPosY, false);
            }
        }

        // Make particles appear around a point. Particles are mostly used to give feedback or make an object stand out
        static void SpawnParticles(int posX, int posY, bool positive)
        {
            Bullet newParticle = new Bullet(posX, posY, ParticleMoveDistance, ParticleMoveSpeedVer, Choice.Up, positive, true);
            BulletAndParticleList.Add(newParticle);
            newParticle = new Bullet(posX, posY, ParticleMoveDistance, ParticleMoveSpeedVer, Choice.Down, positive, true);
            BulletAndParticleList.Add(newParticle);
            newParticle = new Bullet(posX, posY, ParticleMoveDistance, ParticleMoveSpeedHor, Choice.Left, positive, true);
            BulletAndParticleList.Add(newParticle);
            newParticle = new Bullet(posX, posY, ParticleMoveDistance, ParticleMoveSpeedHor, Choice.Right, positive, true);
            BulletAndParticleList.Add(newParticle);
        }

        // Updates the value of CanExit, which determines whether or not the player can go through the exit
        static void CheckCanExit()
        {
            if (CanExit || PelletsCollected < PelletsNeeded || !GotKey || CrawlersKilled < CrawlerKillMin || ShootersKilled < ShooterKillMin)
            {
                return;
            }
            CanExit = true;
            DrawExit();
            SpawnParticles(ExitPosX, ExitPosY, true);
        }

        // Displays the newest version of the GameBoard
        static void UpdateScreen(bool movement)
        {
            
            foreach (Bullet bullet in BulletAndParticleList)
            {
                UpdateBulletAndParticleDisplay(bullet, false);
            }
            if (movement) // No need to clear the previous position and rewrite the player if the they haven't moved
            {
                UpdatePlayerDisplay();
                FillCell(PreviousPlayerPosX, PreviousPlayerPosY, false, true, true, true, true, true, true);
            }
            foreach (Crawler crawler in CrawlerList)
            {
                UpdateCrawlerDisplay(crawler, false);
            }
            if ((PlayerHealth <= PlayerDyingHealth) && Iteration % HealthWarningBlinkDelay == 1) // Make the Health Display blink red if health is low.
            {
                UpdateHealthDisplay(true);
            }
        }

        // Update bullets position and redraw whatever was at their previous position
        static void UpdateBulletAndParticleDisplay(Bullet bullet, bool lastIteration)
        {
            if (bullet.CanMove())
            {
                if (!lastIteration) 
                {
                    if (bullet.Friendly)
                    {
                        Console.ForegroundColor = PlayerColor;
                    }
                    else
                    {
                        Console.ForegroundColor = EnemyColor;
                    }
                    Console.SetCursorPosition(bullet.PosX, bullet.PosY);
                    if (bullet.Particle)
                    {
                        Console.Write(ParticleAppearance);
                    }
                    else
                    {
                        switch (bullet.Direction)
                        {
                            case Choice.Up:
                                Console.Write(BulletVerAppearance);
                                break;
                            case Choice.Down:
                                Console.Write(BulletVerAppearance);
                                break;
                            case Choice.Left:
                                Console.Write(BulletHorAppearance);
                                break;
                            case Choice.Right:
                                Console.Write(BulletHorAppearance);
                                break;
                        }
                    }
                }
                FillCell(bullet.PreviousPosX, bullet.PreviousPosY, true, true, true, true, true, true, true);
            }
        }

        static void UpdateCrawlerDisplay(Crawler crawler, bool lastIteration)
        {
            if (crawler.CanMove())
            {
                if (!lastIteration)
                {
                    DrawCrawler(crawler.PosX, crawler.PosY, GetCrawlerColor(crawler));
                }
                FillCell(crawler.PreviousPosX, crawler.PreviousPosY, true, true, true, true, true, true, true);
            }
        }

        static ConsoleColor GetCrawlerColor(Crawler crawler)
        {
            return GetEnemyColor(crawler.Invincible, crawler.Health, CrawlerDyingHealth);
        }

        static ConsoleColor GetShooterColor(Shooter shooter)
        {
            return GetEnemyColor(shooter.Invincible, shooter.Health, ShooterDyingHealth);
        }

        static ConsoleColor GetEnemyColor(bool invincible, int health, int dyingHealth)
        {
            if (health > dyingHealth)
            {
                return EnemyColor;
            }
            return EnemyDyingColor;
        }

        static void FillCell(int posX, int posY, bool playerOverwrite, bool pelletOverwrite, bool keyOverwrite, bool exitOverwrite, bool healthPackOverwrite, bool crawlerOverwrite, bool shooterOverwrite)
        {
            if (playerOverwrite && posX == PlayerPosX && posY == PlayerPosY)
            {
                UpdatePlayerDisplay();
                return;
            }
            if (pelletOverwrite && IsTouchingPellet(posX, posY))
            {
                DrawPellet(posX, posY);
                return;
            }
            if (!GotKey && keyOverwrite && IsTouchingKey(posX, posY))
            {
                DrawKey();
                return;
            }
            if (exitOverwrite && IsTouchingExit(posX, posY))
            {
                DrawExit();
                return;
            }
            if (!GotHealthPack && healthPackOverwrite && IsTouchingHealthPack(posX, posY))
            {
                DrawHealthPack();
                return;
            }
            if (crawlerOverwrite && IsTouchingCrawler(posX, posY))
            {
                Crawler crawler = GetCrawlerWithXandY(posX, posY);
                DrawCrawler(posX, posY, GetCrawlerColor(crawler));
                return;
            }
            if (shooterOverwrite && IsTouchingShooter(posX, posY))
            {
                Shooter shooter = GetShooterWithXandY(posX, posY);
                DrawShooter(posX, posY, GetShooterColor(shooter));
                return;
            }
            Console.SetCursorPosition(posX, posY);
            Console.Write(EmptyAppearance); // If no objects in previous positions, then the cell was empty
        }

        // Displays a map according to the level and create wall array
        static void DrawMap()
        {
            string CurrentMap = Maps[CurrentLevel];
            Console.BackgroundColor = BackgroundColors[CurrentLevel];
            Console.ForegroundColor = ForegroundColors[CurrentLevel];
            Console.Clear();
            for (int i = 0; i < MapArea; i++) // Goes through all characters
            {
                if (CurrentMap[i] == WallAppearance) // If character is a wall
                {
                    int wallX = (i % MapXSize) + LeftBorder; // Uses modulo to get x position, then adds offset
                    int wallY = ((int)Math.Floor((float)i / (float)MapXSize)) + UpBorder; // Uses the quotient of division to get y position, the adds offset
                    Wall newWall = new Wall(wallX, wallY);
                    WallList.Add(newWall);
                    Console.SetCursorPosition(wallX, wallY);
                    Console.Write(WallAppearance);
                }
            }
            /* This draws an empty map without the need of a "Map" string. Though the collisions won't be checked
            for (int i = UpBorder - 1; i < DownBorder; i++)
            {
                Console.SetCursorPosition(LeftBorder, i + 1);
                Console.Write(WallAppearance);
                Console.SetCursorPosition(RightBorder, i + 1);
                Console.Write(WallAppearance);
            }
            for (int i = LeftBorder - 1; i < RightBorder; i++)
            {
                Console.SetCursorPosition(i + 1, UpBorder);
                Console.Write(WallAppearance);
                Console.SetCursorPosition(i + 1, DownBorder);
                Console.Write(WallAppearance);
            }
            */
        }

        // Returns whether the LEVEL has ended or not and changes game variables accordingly. Ways to end a level: quitting, dying or completing the level
        static bool LevelOver(ref bool playerDied)
        {
            if (LevelComplete)
            {
                playerDied = false;
                // Show scores
                Blackout(ColorBlack);
                if (CurrentLevel != 0) 
                {
                    int posY = ScoreScreen1PosY;
                    int equationScoreInt; // used to avoid repeating large calculations
                    ulong equationScoreLong;
                    Console.ForegroundColor = ColorWhite;
                    if (CurrentLevel == NumLevels)
                    {
                        ScoreGain += GameCompleteScore;
                        WriteScoreLine(LevelEndScoreGameCompleteText, GameCompleteScore.ToString(), ref posY);
                        if (!DiedOnce)
                        {
                            Score += NoDeathBonus;
                            WriteScoreLine(LevelEndScoreNoDeathBonus, NoDeathBonus.ToString(), ref posY);
                        }
                    }
                    equationScoreInt = LevelCompleteScoreFirstLevel + ((CurrentLevel - 1) * LevelCompleteScoreFactor);
                    ScoreGain += equationScoreInt;
                    WriteScoreLine(LevelEndScoreLevelCompleteText, (equationScoreInt + KeyScoreValue).ToString(), ref posY);
                    ScoreGain += (HealthScoreValue * PlayerHealth);
                    WriteScoreLine(LevelEndScoreHealthText + PlayerHealth, (HealthScoreValue * PlayerHealth).ToString(), ref posY);
                    WriteScoreLine(LevelEndScorePelletText + PelletsCollected,  (PelletScoreValue * PelletsCollected).ToString(), ref posY);
                    WriteScoreLine(LevelEndScoreCrawlerText + CrawlersKilled, (CrawlerScoreValue * CrawlersKilled).ToString(), ref posY);
                    WriteScoreLine(LevelEndScoreShooterText + ShootersKilled, (ShooterScoreValue * ShootersKilled).ToString(), ref posY);
                    equationScoreLong = TimeBonusRequirementFirstLevel + (((ulong)CurrentLevel - 1) * TimeBonusRequirementFactor);
                    if (Iteration <= equationScoreLong)
                    {
                        ScoreGain += TimeBonus;
                        WriteScoreLine(LevelEndScoreTimeBonusText, TimeBonus.ToString(), ref posY);
                    }
                    if (!LostHealth)
                    {
                        ScoreGain += HealthBonus;
                        WriteScoreLine(LevelEndScoreHealthBonusText, HealthBonus.ToString(), ref posY);
                    }
                    if (PelletsCollected == NumPellets)
                    {
                        ScoreGain += CollectBonus;
                        WriteScoreLine(LevelEndScoreCollectBonusText, CollectBonus.ToString(), ref posY);
                    }
                    if (CrawlersKilled == NumCrawlers && ShootersKilled == NumShooters)
                    {
                        ScoreGain += KillBonus;
                        WriteScoreLine(LevelEndScoreKillBonusText, KillBonus.ToString(), ref posY);
                    }
                    posY += ScoreScreen2YOffset;
                    WriteScoreLine(LevelEndOldScoreText, Score.ToString(), ref posY);
                    WriteScoreLine(LevelEndPointsText, ScoreGain.ToString(), ref posY);
                    int posX = ScoreScreen1PosX - LevelEndNewScoreText.Length;
                    for (int i = 0; i <= ScoreCountIterations; i++)
                    {
                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(ScoreCountTime));
                        Console.SetCursorPosition(posX, posY);
                        // Multiply first. That way, the final division will return the proper result without any approximation. If division is first, the result will be approximate. And that imperfect result will be multiplied. 
                        Console.Write(LevelEndNewScoreText + PointTo + (Score + ((ScoreGain * i) / ScoreCountIterations)) + PointsText);
                    }
                    Score += ScoreGain;
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(ScoreSectionTime));
                    Console.SetCursorPosition(TransScreenBottomTextPosX, TransScreenBottomTextPosY + ScoreScreenBottomTextOffset);
                    Console.Write(ScoreScreenBottomText);
                    WaitForKeyStroke();
                }
                if (CurrentLevel == NumLevels) // Is the current level the last level? If so, then the player won the game.
                {
                    GameComplete = true; 
                }
                else
                {
                    CurrentLevel++; 
                }
                return true;
            }
            if (PlayerHealth <= 0)
            {
                playerDied = true;
                Lives--;
                return true;
            }
            if (Quitting)
            {
                DiedOnce = true;
                playerDied = false;
                return true;
            }
            return false;
        }

        // Returns whether the GAME has ended or not. Ways to end a level: quitting, running out of lives or completing every level
        static bool GameOver()
        {
            if (Quitting || Lives <= 0 || GameComplete)
            {
                return true;
            }
            return false;
        }

        static void ShowGameEndScreen(bool gameWon)
        {
            Console.ForegroundColor = ColorBlack;
            if (gameWon)
            {
                Blackout(ColorWhite);
                for (int i = 0; i < GameWonTextSizeY; i++)
                {
                    Console.SetCursorPosition(GameWonTextPosX, GameWonTextPosY + i);
                    Console.Write(GameWonText[i]);
                }
            }
            else
            {
                Score += ScoreGain; // Whatever was collected before dying gets counted.
                Blackout(NegativeColor);
                for (int i = 0; i < GameOverTextSizeY; i++)
                {
                    Console.SetCursorPosition(GameOverTextPosX, GameOverTextPosY + i);
                    Console.Write(GameOverText[i]);
                }
            }
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(ScoreSectionTime));
            int posY = GameEndScoreTextPosY;
            Console.SetCursorPosition(GameEndScoreTextPosX, posY);
            Console.Write(GameEndScoreText);
            posY += GameEndScoreSmallOffset;
            for (int i = 0; i < 3; i++) // I know I want 3 dots and this will not ever change under any circumstance so no need to make a constant for 3.
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(ScoreSectionTime));
                Console.SetCursorPosition(GameEndScoreDotsPosX + (Dot.Length * i), posY);
                Console.Write(Dot);
            }
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(ScoreSectionTime));
            posY += GameEndScoreSmallOffset;
            for (int i = 0; i <= ScoreCountIterations; i++)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(ScoreCountTime));
                Console.SetCursorPosition((DesiredScreenSizeX - Score.ToString().Length) / 2, posY); // Not using a static or const for X since the Score since varies throughout the game
                Console.Write(Score * i / ScoreCountIterations);
            }
            if (Score <= OofStatusRequirement)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(ScoreSectionTime));
                Console.SetCursorPosition(OofStatusPosX, posY + GameEndScoreLargeOffset);
                Console.Write(OofStatus);
            }
            if (Score >= EpicGamerStatusRequirement)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(ScoreSectionTime));
                Console.SetCursorPosition(EpicGamerStatusPosX, posY + GameEndScoreLargeOffset);
                Console.Write(EpicGamerStatus);
            }
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(ScoreSectionTime));
            Console.SetCursorPosition(GameEndLeaveTextPosX, GameEndLeaveTextPosY);
            Console.Write(GameEndLeaveText);
            WaitForKeyStroke();
        }


        // Maps (All maps are 150 x 45, changing those number will break the game)
        static readonly string[] Maps = new string[]
        {   
            // Default Map, Not used
            "██████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "█                                                                                                                                                    █" +
            "██████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████",

            // Map 1
            "██████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████" +
            "█                                           █                                                            █                                           █" +
            "█                              █            █                               █                         █  █             █                             █" +
            "█     █                                     █            █                                               █                                    █      █" +
            "█                                           █                                                            █                                           █" +
            "█                                   █       █                                             █              █                                           █" +
            "█                █                          █                          █                                 █      █                █       █           █" +
            "█  █                                        █                                                            █                                           █" +
            "█                                           █       █                                             █      █                                           █" +
            "█        █████           █                  █              █                                             █                              █████        █" +
            "█        █                                  █                                          █                 █                 █                █        █" +
            "█        █                                  █                            █                               █                                  █   █    █" +
            "█        █                       █                                                                                                   █      █        █" +
            "█        █                                           █                                                       █                              █        █" +
            "█                █                                                                                                      █                            █" +
            "█                                                                               █              █                                                     █" +
            "█      █                                    █                                                                                       █                █" +
            "█                                                               █                                                                                    █" +
            "█                           █                                                                                   █                                    █" +
            "█                                                                 ████████████████████           █                                                █  █" +
            "█                     █                       █                   █                  █                                                     █         █" +
            "█   █                                                             █                  █                                       █                       █" +
            "█                                      ████████████████████████████                  ██████████████████████████                                      █" +
            "█             █                                                   █                  █                                             █                 █" +
            "█                                             █                   █                  █                              █                                █" +
            "█                                                                 ██████        ██████                █                                              █" +
            "█                               █                                                                                                      █             █" +
            "█                   █                              █                                                                                                 █" +
            "█     █                                                                          █                                                                   █" +
            "█                                                                                                                          █                         █" +
            "█                                 █                                                                          █                                 █     █" +
            "█        █                                                     █                                █                                           █        █" +
            "█        █                                                                                                                                  █        █" +
            "█        █              █                   █                                                            █                                  █        █" +
            "█        █                                  █            █                                               █               █                  █        █" +
            "█        █████                              █                                            █               █                              █████        █" +
            "█                                           █                                                            █                                           █" +
            "█             █                             █                                                            █      █                   █                █" +
            "█                                           █                          █                                 █                                       █   █" +
            "█                                     █     █                                                            █                                           █" +
            "█                                           █                                                            █            █                              █" +
            "█                    █                      █          █                                                 █                                           █" +
            "█  █                                        █                                                       █    █                       █                   █" +
            "█                                           █                                                            █                                           █" +
            "██████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████",

            // Map 2
            "██████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████" +
            "███████████                                                                                                                                ███████████" +
            "██████████                                                                                                                                  ██████████" +
            "█████████                                                                                                                                    █████████" +
            "████████                                                                                                                                      ████████" +
            "███████                                                                                                                                        ███████" +
            "██████                              █████████████████████████████████████████████████          ███████████████████                              ██████" +
            "█████                               █████████████████████████████████████████████████                            █      ████                     █████" +
            "████                                ████████████████████████████████████████████████                             █      ████                      ████" +
            "███                                 ███████████████████████████████████████████████          █████████████████████                                 ███" +
            "██                                  █                                                       ██████████████████████                    ████          ██" +
            "█        ███████     ███████        █                                                      ███████████████████████                    ████           █" +
            "█        █                 █        ████████████████████████████████████████████          ████████████████████████  ████                             █" +
            "█        █                 █        ███████████████████████████████████████████                                  █  ████                             █" +
            "█        █                 █        ██████████████████████████████████████████                                   █                                   █" +
            "█        █                 █        █████████████████████████████████████████          ███████████████████████████           ████                    █" +
            "█        █                 █        █                                                 ████████████████████████████           ████             ████   █" +
            "█        ███████     ███████        █                                                █████████████████████████████                            ████   █" +
            "█                                   ██████████████████████████████████████          ██████████████████████████████                                   █" +
            "█                                   █████████████████████████████████████          ███████████████████████████████    ████                           █" +
            "█                                   ████████████████████████████████████          ████████████████████████████████    ████            ████           █" +
            "█   █   █   █   █   █   █   █   █   █                                                                            █                    ████           █" +
            "█   █   █   █   █   █   █   █   █   █                                                                            █                                   █" +
            "█   █   █   █   █   █   █   █   █   █                                                                            █                                   █" +
            "█   █   █   █   █   █   █   █   █   ████████████████████████████████          ████████████████████████████████████       ████            ████        █" +
            "█   █   █   █   █   █   █   █   █   ███████████████████████████████          █████████████████████████████████████       ████            ████        █" +
            "█   █   █   █   █   █   █   █   █   ██████████████████████████████          ██████████████████████████████████████                                   █" +
            "█   █   █   █   █   █   █   █   █   █████████████████████████████                                                █                                   █" +
            "█   █   █   █   █   █   █   █   █   ████████████████████████████                                                 █                             ████  █" +
            "█                                   ███████████████████████████          █████████████████████████████████████████      ████                   ████  █" +
            "█   █   █   █   █   █   █   █   █   █                                   ██████████████████████████████████████████      ████                         █" +
            "█   █   █   █   █   █   █   █   █   █                                  ███████████████████████████████████████████                 ████              █" +
            "█       █   █   █   █   █   █   █   ████████████████████████          ████████████████████████████████████████████                 ████              █" +
            "█       █   █   █   █   █   █   █   ███████████████████████                                                      █                                   █" +
            "██      █   █   █   █   █   █   █   ██████████████████████                                                       █  ████                            ██" +
            "███         █   █   █   █   █   █   █████████████████████          ███████████████████████████████████████████████  ████               ████        ███" +
            "████        █   █   █   █   █   █   █                             ████████████████████████████████████████████████                     ████       ████" +
            "█████       █   █   █   █   █   █   █                            █████████████████████████████████████████████████                               █████" +
            "██████                              ███████████████████          █████████████████████████████████████████████████                              ██████" +
            "███████                                                                                                                                        ███████" +
            "████████                                                                                                                                      ████████" +
            "█████████                                                                                                                                    █████████" +
            "██████████                                                                                                                                  ██████████" +
            "███████████                                                                                                                                ███████████" +
            "██████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████",

            // Map 3
            "██████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████" +
            "██                                  █             █          ███                      ███          █             █                                  ██" +
            "█                                                             █                        █                                                             █" +
            "█                       █                                     █           ██           █                                     █                       █" +
            "█                                                                         ██                                                                         █" +
            "██                                  █             █                      ████                      █             █                                  ██" +
            "█████████████████████████           ██████████████████████████████████████████████████████████████████████████████           █████████████████████████" +
            "████████████████████████             ███████████████                                              ███████████████             ████████████████████████" +
            "██                                               ██                                                █                                                ██" +
            "█                                                 █                                                █                                                 █" +
            "█                                                 █     ██████████████████████████████████████     █                                                 █" +
            "█                              █████              █    ██                                    ██    █              ██████                             █" +
            "██                            ██                  █    █                                      █    █                   █                            ██" +
            "███████████   ██████████      █                   █    █                                      █    █                   █      ██████████   ███████████" +
            "██                     ██     █                  ██    █                                      █    ██                  █     ██                     ██" +
            "█                       █     █       █████████████    █                                      █    █████████████       █     █                       █" +
            "█                       █     █                   █    █                                      █    █                   █     █                       █" +
            "█                       █     █                                                                                        █     █                       █" +
            "█                       █     █                                                                                        █     █                       █" +
            "█           █           █     ██                  █    █                                      █    █                   █     █           █           █" +
            "█          ███          █      ████████████████████    █                                      █    █████████████████████     █          ███          █" +
            "█          ███          █                        ██    █                                      █    ██                        █          ███          █" +
            "█          ███          █                         █    █                                      █    █                         █          ███          █" +
            "█          ███          █                         █    █                                      █    █                         █          ███          █" +
            "█          ███          █                        ██    ██                                    ██    ██                        █          ███          █" +
            "█          ███          █      ████████████████████     ██████████████████████████████████████     ████████████████████      █          ███          █" +
            "█          ███          █     ██                  █                      ████                      █                  ██     █          ███          █" +
            "█          ███          █     █                                           ██                                           █     █          ███          █" +
            "█          ███          █     █                                           ██                                           █     █          ███          █" +
            "█          ███          █     █                                           ██                                           █     █          ███          █" +
            "██        █████        ██     ██                  █                      ████                      █                  ██     ██        █████        ██" +
            "████████████████████████       ████████████████████████████████████████████████████████████████████████████████████████       ████████████████████████" +
            "█                                                ███                     ████                     ███                                                █" +
            "█                                                 █                       ██                       █                                                 █" +
            "█████████████████████████     █                   █                      ████                      █                   █     █████████████████████████" +
            "██                            █                   █         ██████████████████████████████         █                   █                            ██" +
            "█                             █                                                                                        █                             █" +
            "█          ████████████       █                                                                                        █       ████████████          █" +
            "█                     ██      █         █████████████████████████████████████████████████████████████████████          █      ██                     █" +
            "█                      █      █                                          ████                                          █      █                      █" +
            "█                     ██      █                                           ██                                           █      ██                     █" +
            "█     █████████████████       █                   █                       ██                       █                   █       █████████████████     █" +
            "█                             █                   █                                                █                   █                             █" +
            "██                           ███                 ███                                              ███                 ███                           ██" +
            "██████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████",

            // Map 4 [FINAL]. Fun fact: each corner represents a level
            "██████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████" +
            "██████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████" +
            "██████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████" +
            "███████████                                                                                                                                ███████████" +
            "██████████                █               █                   █        ████████                                                             ██████████" +
            "█████████         █              █                █                 █  █      █                                                              █████████" +
            "████████                               █                  █            █      █      ████████████████████         ████████████████            ████████" +
            "████████                  █                         █            █     █      █     ██     ███                   ██              ██           ████████" +
            "████████            █          █             █                         █      █     █       █                                                 ████████" +
            "████████                 █                                   █         █      █     █                █                                        ████████" +
            "████████                                █               █          █   █      █     █                ██         ███              ██           ████████" +
            "████████       █                  █                █                   █      █     █████████        █████████████████████████████            ████████" +
            "████████              █                                     █          █      █            ██        ██                                       ████████" +
            "████████                                 █          █                  █      █             █        █                                        ████████" +
            "████████        █        ██████████                     █        █    ██      ██            █        █             ██████████                 ████████" +
            "████████              ███          ███         █                    ██          ██          █        █          ███          ███              ████████" +
            "████████            ██                ██            █                                                         ██                ██            ████████" +
            "████████           █                    █     █                 ██                  ██                       █                    █           ████████" +
            "████████          █                      █                    ██                      ██                    █                      █          ████████" +
            "████████         █                        ████████████████████                          ████████████████████                        █         ████████" +
            "████████       █████                                                                                                              █████       ████████" +
            "████████                     ██                                           ██                                           ██                     ████████" +
            "████████                   ██████                                       ██████                                       ██████                   ████████" +
            "████████                     ██                                           ██                                           ██                     ████████" +
            "████████       █████                                                                                                              █████       ████████" +
            "████████         █                        ████████████████████                          ████████████████████                        █         ████████" +
            "████████          █                      █                    ██                      ██                    █                      █          ████████" +
            "████████           █                    █                       ██                  ██                       █                    █           ████████" +
            "████████            ██                ██                                                                      ██                ██            ████████" +
            "████████              ███          ███                              ██          ██                              ███          ███              ████████" +
            "████████                 ██████████                                   ██      ██                                   ██████████                 ████████" +
            "████████                                    ████████     ████████      █      █                                                               ████████" +
            "████████                                    ████████     ████████      █      █                          ██  ██                               ████████" +
            "████████         ████████     ████████      ████████     ████████      █      █                       ███      ███                            ████████" +
            "████████         ████████     ████████      ████████     ████████      █      █                     ██            ██                          ████████" +
            "████████         ████████     ████████                                 █      █         ██         █                █         ██              ████████" +
            "████████         ████████     ████████                                 █      █       ██████               ██               ██████            ████████" +
            "████████         ████████     ████████      ████████     ████████      █      █         ██         █                █         ██              ████████" +
            "████████         ████████     ████████      ████████     ████████      █      █                     ██            ██                          ████████" +
            "█████████                                   ████████     ████████      █      █                       ███      ███                           █████████" +
            "██████████                                  ████████     ████████      ████████                          ██  ██                             ██████████" +
            "███████████                                                                                                                                ███████████" +
            "██████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████" +
            "██████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████" +
            "██████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████████",
        };
    }
}
