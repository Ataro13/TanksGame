namespace TanksGame
{
    public class EnemyTank : Tank
    {
        private readonly Random _random = new Random();
        private int _moveCounter = 0;
        private int _shootCounter = 0;
        private const int MOVE_DELAY = 10;
        private const int SHOOT_DELAY = 70;
        private const int SHOOT_CHANCE = 50;
        private const int CHASE_DISTANCE = 8;
        private int _targetX, _targetY;
        private Map _currentMap = null!;

        public EnemyTank(int x, int y) : base(x, y, false)
        {
        }

        public override void Update(Map map, PlayerTank player)
        {
            if (player == null || map == null) return;
            _currentMap = map;

            _moveCounter++;
            _shootCounter++;

            int distanceToPlayer = CalculateDistance(player);

            if (_shootCounter >= SHOOT_DELAY)
            {
                if (IsPlayerInSight(player))
                {
                    if (_random.Next(100) < SHOOT_CHANCE)
                    {
                        _shootCounter = 0;
                        Fire(player, map);
                    }
                }
            }

            if (_moveCounter >= MOVE_DELAY)
            {
                _moveCounter = 0;

                if (distanceToPlayer < CHASE_DISTANCE && IsPlayerInSight(player))
                {
                    _targetX = player.GetX();
                    _targetY = player.GetY();
                    MoveTowardsTarget();
                }
                else
                {
                    if (_random.Next(100) < 70)
                    {
                        RandomMove();
                        if (!TryMove())
                        {
                            _direction = (Direction)((_random.Next(3) + (int)_direction + 1) % 4);
                            TryMove();
                        }
                    }
                }
            }
        }

        private int CalculateDistance(PlayerTank player)
        {
            int dx = (player.GetX() - _x) / CELL_SIZE;
            int dy = (player.GetY() - _y) / CELL_SIZE;
            return Math.Abs(dx) + Math.Abs(dy);
        }

        private void MoveTowardsTarget()
        {
            int dx = _targetX - _x;
            int dy = _targetY - _y;

            if (Math.Abs(dx) > Math.Abs(dy))
            {
                _direction = dx > 0 ? Direction.Right : Direction.Left;
                TryMove();
            }
            else
            {
                _direction = dy > 0 ? Direction.Down : Direction.Up;
                TryMove();
            }

            if (!TryMove())
            {
                _targetX = _x;
                _targetY = _y;
            }
        }

        private bool TryMove()
        {
            if (_currentMap == null) return false;

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
                return true;
            }
            return false;
        }

        private void RandomMove()
        {
            _direction = (Direction)_random.Next(4);
            TryMove();
        }

        private bool IsPlayerInSight(PlayerTank player)
        {
            if (_currentMap == null) return false;

            int enemyX = _x / CELL_SIZE;
            int enemyY = _y / CELL_SIZE;
            int playerX = player.GetX() / CELL_SIZE;
            int playerY = player.GetY() / CELL_SIZE;

            if (enemyX == playerX)
            {
                int startY = Math.Min(enemyY, playerY);
                int endY = Math.Max(enemyY, playerY);
                _direction = enemyY > playerY ? Direction.Up : Direction.Down;

                for (int y = startY + 1; y < endY; y++)
                {
                    if (!_currentMap.CanShootThrough(enemyX, y)) return false;
                }
                return true;
            }
            else if (enemyY == playerY)
            {
                int startX = Math.Min(enemyX, playerX);
                int endX = Math.Max(enemyX, playerX);
                _direction = enemyX > playerX ? Direction.Left : Direction.Right;

                for (int x = startX + 1; x < endX; x++)
                {
                    if (!_currentMap.CanShootThrough(x, enemyY)) return false;
                }
                return true;
            }
            return false;
        }

        public bool CanShootPlayer(PlayerTank player)
        {
            return _shootCounter >= SHOOT_DELAY && IsPlayerInSight(player);
        }

        private bool IsValidMove(int newX, int newY)
        {
            if (_currentMap == null) return false;

            for (int i = 0; i < CELL_SIZE; i++)
            {
                for (int j = 0; j < CELL_SIZE; j++)
                {
                    if (!_currentMap.IsValidPosition((newX + j) / CELL_SIZE, (newY + i) / CELL_SIZE))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public Bullet Fire(PlayerTank player, Map map)
        {
            _currentMap = map;
            int bulletX = _x + CELL_SIZE / 2;
            int bulletY = _y + CELL_SIZE / 2;

            if (Math.Abs(player.GetX() - _x) > Math.Abs(player.GetY() - _y))
            {
                _direction = player.GetX() > _x ? Direction.Right : Direction.Left;
            }
            else
            {
                _direction = player.GetY() > _y ? Direction.Down : Direction.Up;
            }

            return new Bullet(bulletX, bulletY, _direction, false);
        }
    }
} 