using System;

namespace TanksGame
{
    public class BossTank : Tank
    {
        private readonly Random _random = new Random();
        private int _moveCounter = 0;
        private int _shootCounter = 0;
        private const int MOVE_DELAY = 5;
        private const int SHOOT_DELAY = 25;
        private const int DETECTION_RANGE = 15;
        private int _health = 5;
        private Map _currentMap = null!;
        private readonly Game _game;

        public BossTank(int x, int y, Game game) : base(x, y, false, '█', 3)
        {
            _game = game;
        }

        public override void Update(Map map, PlayerTank player)
        {
            if (player == null) return;
            _currentMap = map;

            _moveCounter++;
            _shootCounter++;

            UpdateDirectionToPlayer(player);

            if (_shootCounter >= SHOOT_DELAY && IsPlayerInSight(player))
            {
                _shootCounter = 0;
                FireDoubleCannon(map);
            }

            if (_moveCounter >= MOVE_DELAY)
            {
                _moveCounter = 0;
                MoveTowardsPlayer(player);
            }
        }

        private void UpdateDirectionToPlayer(PlayerTank player)
        {
            int dx = player.GetX() - _x;
            int dy = player.GetY() - _y;

            if (Math.Abs(dx) > Math.Abs(dy))
            {
                _direction = dx > 0 ? Direction.Right : Direction.Left;
            }
            else
            {
                _direction = dy > 0 ? Direction.Down : Direction.Up;
            }
        }

        private void MoveTowardsPlayer(PlayerTank player)
        {
            int newX = _x;
            int newY = _y;

            switch (_direction)
            {
                case Direction.Up: newY -= CELL_SIZE; break;
                case Direction.Down: newY += CELL_SIZE; break;
                case Direction.Left: newX -= CELL_SIZE; break;
                case Direction.Right: newX += CELL_SIZE; break;
            }

            if (IsValidMove(newX, newY))
            {
                _x = newX;
                _y = newY;
            }
        }

        private void FireDoubleCannon(Map map)
        {
            var gunPositions = _direction switch
            {
                Direction.Up => new[] { 
                    (_x + 1, _y - 1),
                    (_x + 2, _y - 1)
                },
                Direction.Down => new[] {
                    (_x + 1, _y + 3),
                    (_x + 2, _y + 3)
                },
                Direction.Left => new[] {
                    (_x - 1, _y + 1),
                    (_x - 1, _y + 2)
                },
                Direction.Right => new[] {
                    (_x + 3, _y + 1),
                    (_x + 3, _y + 2)
                },
                _ => new[] { (_x, _y), (_x, _y) }
            };

            foreach (var (bulletX, bulletY) in gunPositions)
            {
                var bullet = new Bullet(bulletX, bulletY, _direction, false);
                _game.AddBossBullet(bullet);
            }
        }

        private bool IsPlayerInSight(PlayerTank player)
        {
            if (_currentMap == null) return false;

            int enemyX = _x / CELL_SIZE;
            int enemyY = _y / CELL_SIZE;
            int playerX = player.GetX() / CELL_SIZE;
            int playerY = player.GetY() / CELL_SIZE;

            int distance = Math.Abs(enemyX - playerX) + Math.Abs(enemyY - playerY);
            if (distance > DETECTION_RANGE) return false;

            if (enemyX == playerX || enemyY == playerY)
            {
                int startX = Math.Min(enemyX, playerX);
                int endX = Math.Max(enemyX, playerX);
                int startY = Math.Min(enemyY, playerY);
                int endY = Math.Max(enemyY, playerY);

                if (enemyY == playerY)
                {
                    for (int x = startX + 1; x < endX; x++)
                    {
                        if (!_currentMap.CanShootThrough(x, enemyY)) return false;
                    }
                }
                else if (enemyX == playerX)
                {
                    for (int y = startY + 1; y < endY; y++)
                    {
                        if (!_currentMap.CanShootThrough(enemyX, y)) return false;
                    }
                }
                return true;
            }
            return false;
        }

        public bool CanShootPlayer(PlayerTank player)
        {
            return _shootCounter >= SHOOT_DELAY && IsPlayerInSight(player);
        }

        public bool TakeDamage()
        {
            _health--;
            return _health <= 0;
        }

        private bool IsValidMove(int newX, int newY)
        {
            if (_currentMap == null) return false;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (!_currentMap.IsValidPosition((newX + j) / CELL_SIZE, (newY + i) / CELL_SIZE))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override void Draw(char[,] screen)
        {
            // Рисуем тело босса
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (_y + i < screen.GetLength(0) && _x + j < screen.GetLength(1))
                    {
                        screen[_y + i, _x + j] = '█';
                    }
                }
            }

            var gunPositions = _direction switch
            {
                Direction.Up => new[] { 
                    (_x + 1, _y - 1),
                    (_x + 2, _y - 1)
                },
                Direction.Down => new[] {
                    (_x + 1, _y + 3),
                    (_x + 2, _y + 3)
                },
                Direction.Left => new[] {
                    (_x - 1, _y + 1),
                    (_x - 1, _y + 2)
                },
                Direction.Right => new[] {
                    (_x + 3, _y + 1),
                    (_x + 3, _y + 2)
                },
                _ => new[] { (_x, _y), (_x, _y) }
            };

            foreach (var (gunX, gunY) in gunPositions)
            {
                if (gunY >= 0 && gunY < screen.GetLength(0) && 
                    gunX >= 0 && gunX < screen.GetLength(1))
                {
                    screen[gunY, gunX] = _direction == Direction.Up || _direction == Direction.Down ? '║' : '═';
                }
            }
        }
    }
} 