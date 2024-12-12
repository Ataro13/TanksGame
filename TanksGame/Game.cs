using System;
using System.Text;
using System.Runtime.InteropServices;

namespace TanksGame
{
    public class Game
    {
        private Map _map;
        private readonly List<Tank> _tanks;
        private readonly List<Bullet> _bullets;
        private readonly PlayerTank _player;
        private bool _isGameActive;
        private readonly StringBuilder _buffer;
        private int _currentLevel = 1;
        private const int MAX_LEVEL = 5;
        private int _score = 0;
        private const int POINTS_PER_KILL = 10;
        private const int WINDOW_WIDTH = 80;
        private const int WINDOW_HEIGHT = 35;
        private readonly string[] _screenBuffer;
        private readonly object _lockObject = new object();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);
        
        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int lpMode);
        
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, int dwMode);

        public Game()
        {
#pragma warning disable CA1416 // Validate platform compatibility
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            
            try 
            {
                Console.WindowWidth = WINDOW_WIDTH;
                Console.WindowHeight = WINDOW_HEIGHT;
                Console.BufferWidth = WINDOW_WIDTH;
                Console.BufferHeight = WINDOW_HEIGHT;
            }
            catch
            {
                Console.WriteLine("Не удалось установить размер окна");
            }
            
            Console.CursorVisible = false;
            Console.Clear();

            _screenBuffer = new string[WINDOW_HEIGHT];
            for (int i = 0; i < WINDOW_HEIGHT; i++)
            {
                _screenBuffer[i] = new string(' ', WINDOW_WIDTH);
            }

            try
            {
                var handle = GetStdHandle(-11);
                GetConsoleMode(handle, out int mode);
                SetConsoleMode(handle, mode | 0x0004);
            }
            catch { }

            _map = new Map(1);
            _tanks = new List<Tank>();
            _bullets = new List<Bullet>();
            _buffer = new StringBuilder(WINDOW_WIDTH * WINDOW_HEIGHT);
            
            _player = new PlayerTank(2, 2, this);
            InitializeGame();
#pragma warning restore CA1416
        }

        private void InitializeGame()
        {
            try
            {
                _tanks.Clear();
                _bullets.Clear();
                _map = new Map(_currentLevel);
                
                int playerStartX = 2;
                int playerStartY = 2;
                _player.SetX(playerStartX);  
                _player.SetY(playerStartY);
                
                SpawnEnemies();
                
                for (int i = 0; i < WINDOW_HEIGHT; i++)
                {
                    _screenBuffer[i] = new string(' ', WINDOW_WIDTH);
                }
                Console.Clear();
                
                _isGameActive = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in InitializeGame: {ex.Message}");
                Console.ReadKey(true);
            }
        }

        private void SpawnEnemies()
        {
            _tanks.Clear();
            
            if (_currentLevel == 5) 
            {
                var boss = new BossTank(15, 5, this);
                _tanks.Add(boss);
                return;
            }

            int enemyCount = _currentLevel;
            
            var positions = _currentLevel switch
            {
                1 => new List<(int x, int y)>
                {
                    (15, 1), (1, 1), (8, 1)
                },
                2 => new List<(int x, int y)>
                {
                    (17, 8), (17, 1), (15, 4), (12, 8)
                },
                3 => new List<(int x, int y)>
                {
                    (17, 8), (17, 1), (1, 8), (8, 8), (15, 4)
                },
                4 => new List<(int x, int y)>
                {
                    (18, 1), (18, 8), (15, 5), (12, 8)
                },
                _ => new List<(int x, int y)>()
            };

            for (int i = 0; i < enemyCount && i < positions.Count; i++)
            {
                var pos = positions[i];
                var enemy = new EnemyTank(pos.x, pos.y);
                _tanks.Add(enemy);
            }
        }

        private void ShowLevelStartScreen()
        {
            Console.Clear();
            Console.SetCursorPosition(0, WINDOW_HEIGHT / 2 - 3);
            Console.ForegroundColor = _currentLevel == 5 ? ConsoleColor.Red : ConsoleColor.Yellow;
            
            if (_currentLevel == 5)
            {
                Console.WriteLine(@$"
                    BOSS LEVEL!

                    DEFEAT THE MEGA TANK BOSS!
                    BOSS HEALTH: 5
                    CURRENT SCORE: {_score}

                    Press any key to start boss fight...
        ");
            }
            else
            {
                Console.WriteLine($@"
                    LEVEL {_currentLevel}

                    ENEMIES TO DEFEAT: {_currentLevel}
                    CURRENT SCORE: {_score}

                    Press any key to start level...
        ");
            }
            
            Console.ResetColor();
            Console.ReadKey(true);
            Console.Clear();
        }

        private void NextLevel()
        {
            _currentLevel++;
            if (_currentLevel <= MAX_LEVEL)
            {
                ShowLevelStartScreen();
                InitializeGame();
            }
            else
            {
                ShowGameCompleteScreen();
                _isGameActive = false;
            }
        }

        private void ShowGameCompleteScreen()
        {
            Console.Clear();
            Console.SetCursorPosition(0, WINDOW_HEIGHT / 2 - 5);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@$"
                    ╔═══════════════════════════════╗
                    ║        CONGRATULATIONS!       ║
                    ╚═══════════════════════════════╝

                    You've completed all levels!
                    You defeated the MEGA TANK BOSS!
                    Final Score: {_score}
                    You are the Tank Master!

                    ╔═══════════════════════════════╗
                    ║     Press ENTER - New Game    ║
                    ║     Press ESC   - Exit        ║
                    ╚═══════════════════════════════╝
    ");
            Console.ResetColor();

            while (true)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Enter)
                {
                    _currentLevel = 1;
                    _score = 0;
                    _isGameActive = true;
                    InitializeGame();
                    return;
                }
                if (key == ConsoleKey.Escape)
                {
                    Environment.Exit(0);
                }
            }
        }

        public void Start()
        {
            bool exitGame = false;
            while (!exitGame)
            {
                ShowStartScreen();
                _currentLevel = 1;
                _score = 0;
                _isGameActive = true;

                while (_currentLevel <= MAX_LEVEL && _isGameActive)
                {
                    ShowLevelStartScreen();
                    InitializeGame();
                    
                    while (_isGameActive)
                    {
                        ProcessInput();
                        UpdateGame();
                        DrawGame();
                        Thread.Sleep(50);
                    }
                }
                exitGame = ShowGameOverMenu();
            }
        }

        private bool ShowGameOverMenu()
        {
            Console.Clear();
            Console.SetCursorPosition(0, WINDOW_HEIGHT / 2 - 5);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@$"
                    ╔═══════════════════════════════╗
                    ║          GAME  OVER           ║
                    ╚═══════════════════════════════╝

                         FINAL SCORE: {_score}

                    ╔═══════════════════════════════╗
                    ║     Press ENTER - New Game    ║
                    ║     Press ESC   - Exit        ║
                    ╚═══════════════════════════════╝
    ");
            Console.ResetColor();

            while (true)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Enter)
                {
                    Console.Clear();
                    return false; // Продолжаем игру
                }
                if (key == ConsoleKey.Escape)
                {
                    Console.Clear();
                    return true; // Выходим из игры
                }
            }
        }

        private void ShowStartScreen()
        {
            Console.Clear();
            Console.SetCursorPosition(0, WINDOW_HEIGHT / 2 - 8);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@"
                    ╔═══════════════════════════════╗
                    ║         TANKS BATTLE          ║
                    ╚═══════════════════════════════╝

                              Controls:
                    ╔═══════════════════════════════╗
                    ║    [↑] - Move Up              ║
                    ║    [↓] - Move Down            ║
                    ║    [←] - Move Left            ║
                    ║    [→] - Move Right           ║
                    ║    [Space] - Fire             ║
                    ║    [Esc] - Exit               ║
                    ╚═══════════════════════════════╝

                         Press any key to start

                         @github.com/Ataro13
    ");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        private void ProcessInput()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Escape)
                {
                    _isGameActive = false;
                    return;
                }
                _player.HandleInput(key, _map, this);
            }
        }

        private void ShowVictoryScreen()
        {
            Console.Clear();
            Console.SetCursorPosition(0, _map.Height / 2);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"
            VICTORY!
            
            You are the Tank Master!
        ");
            Console.ResetColor();
            Thread.Sleep(3000);
        }

        private void UpdateGame()
        {
            foreach (var bullet in _bullets.ToList())
            {
                bullet.Update(_map);
                
                if (bullet.IsActive)
                {
                    if (!bullet.IsPlayerBullet && bullet.CollidesWith(_player.GetX(), _player.GetY()))
                    {
                        _isGameActive = false;
                        return;
                    }

                    foreach (var tank in _tanks.ToList())
                    {
                        if (bullet.IsPlayerBullet && bullet.CollidesWith(tank.GetX(), tank.GetY()))
                        {
                            if (_currentLevel == 5 && tank is BossTank bossTank)
                            {
                                if (bossTank.TakeDamage())
                                {
                                    _tanks.Remove(tank);
                                    _score += POINTS_PER_KILL * 5;
                                    if (_tanks.Count == 0)
                                    {
                                        _isGameActive = false;
                                        ShowGameCompleteScreen();
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                _tanks.Remove(tank);
                                _score += POINTS_PER_KILL;
                                if (_tanks.Count == 0)
                                {
                                    if (_currentLevel < MAX_LEVEL)
                                    {
                                        NextLevel();
                                    }
                                    else
                                    {
                                        _isGameActive = false;
                                        ShowGameCompleteScreen();
                                    }
                                    return;
                                }
                            }
                            bullet.Deactivate();
                            break;
                        }
                    }
                }
            }

            _bullets.RemoveAll(b => !b.IsActive);

            foreach (var tank in _tanks)
            {
                tank.Update(_map, _player);
                
                if (tank is BossTank bossTank && bossTank.CanShootPlayer(_player))
                {
                    bossTank.Update(_map, _player);
                }
                else if (tank is EnemyTank enemyTank && enemyTank.CanShootPlayer(_player))
                {
                    var bullet = enemyTank.Fire(_player, _map);
                    if (bullet != null)
                    {
                        _bullets.Add(bullet);
                    }
                }
            }
        }

        public void AddPlayerBullet(Bullet bullet)
        {
            if (bullet != null && _bullets != null)
            {
                _bullets.Add(bullet);
            }
        }

        private void DrawGame()
        {
            try
            {
                _buffer.Clear();
                char[,] display = new char[_map.Height * Map.CELL_SIZE, _map.Width * Map.CELL_SIZE];

                // Заполняем пустое пространство
                for (int i = 0; i < display.GetLength(0); i++)
                    for (int j = 0; j < display.GetLength(1); j++)
                        display[i, j] = ' ';

                // Отрисовка карты
                for (int i = 0; i < _map.Height; i++)
                {
                    for (int j = 0; j < _map.Width; j++)
                    {
                        char tile = _map.GetMapArray()[i, j];
                        if (tile != ' ')
                        {
                            for (int di = 0; di < Map.CELL_SIZE; di++)
                                for (int dj = 0; dj < Map.CELL_SIZE; dj++)
                                    display[i * Map.CELL_SIZE + di, j * Map.CELL_SIZE + dj] = tile;
                        }
                    }
                }

                // Отрисовка игрока и врагов
                _player.Draw(display);
                foreach (var tank in _tanks)
                    tank.Draw(display);

                // Отрисовка пуль
                foreach (var bullet in _bullets.Where(b => b.IsActive))
                {
                    if (bullet.Y >= 0 && bullet.Y < display.GetLength(0) && 
                        bullet.X >= 0 && bullet.X < display.GetLength(1))
                    {
                        display[bullet.Y, bullet.X] = '*';
                    }
                }

                // Вывод на экран
                for (int i = 0; i < display.GetLength(0); i++)
                {
                    for (int j = 0; j < display.GetLength(1); j++)
                    {
                        _buffer.Append(display[i, j]);
                    }
                    _buffer.AppendLine();
                }

                _buffer.AppendLine();

                if (_currentLevel == 5)
                {
                    _buffer.AppendLine($"     BOSS LEVEL      Score: {_score,6}      Enemies: {_tanks.Count,2}");
                }
                else
                {
                    _buffer.AppendLine($"     Level: {_currentLevel,-2}      Score: {_score,6}     Enemies: {_tanks.Count,2}");
                }

                Console.SetCursorPosition(0, 0);
                Console.Write(_buffer.ToString());
            }
            catch (Exception) { }
        }

        private string CenterText(string text)
        {
            int padding = (WINDOW_WIDTH - text.Length) / 2;
            return $"{new string(' ', padding)}{text}{new string(' ', WINDOW_WIDTH - text.Length - padding)}";
        }

        public void AddBossBullet(Bullet bullet)
        {
            if (bullet != null)
            {
                _bullets.Add(bullet);
            }
        }
    }
} 