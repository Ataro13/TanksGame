namespace TanksGame
{
    public class PlayerTank : Tank
    {
        private readonly Game _game;

        public PlayerTank(int x, int y, Game game) : base(x, y, true, '■', 1)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
        }

        public override void Update(Map map, PlayerTank player = null)
        {
            // Игрок управляется через HandleInput
        }

        public void HandleInput(ConsoleKey key, Map map, Game game)
        {
            if (map == null) return;
            if (game == null) return;

            int newX = _x;
            int newY = _y;
            bool moved = false;

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    newY -= CELL_SIZE;
                    _direction = Direction.Up;
                    moved = true;
                    break;
                case ConsoleKey.DownArrow:
                    newY += CELL_SIZE;
                    _direction = Direction.Down;
                    moved = true;
                    break;
                case ConsoleKey.LeftArrow:
                    newX -= CELL_SIZE;
                    _direction = Direction.Left;
                    moved = true;
                    break;
                case ConsoleKey.RightArrow:
                    newX += CELL_SIZE;
                    _direction = Direction.Right;
                    moved = true;
                    break;
                case ConsoleKey.Spacebar:
                    FireBullet(map, game);
                    break;
            }

            if (moved && IsValidMove(newX, newY, map))
            {
                _x = newX;
                _y = newY;
            }
        }

        private bool IsValidMove(int newX, int newY, Map map)
        {
            for (int i = 0; i < CELL_SIZE; i++)
            {
                for (int j = 0; j < CELL_SIZE; j++)
                {
                    if (!map.IsValidPosition((newX + j) / CELL_SIZE, (newY + i) / CELL_SIZE))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void FireBullet(Map map, Game game)
        {
            int bulletX = _x;
            int bulletY = _y;

            switch (_direction)
            {
                case Direction.Up:
                    bulletX += CELL_SIZE / 2;
                    bulletY -= 1;
                    break;
                case Direction.Down:
                    bulletX += CELL_SIZE / 2;
                    bulletY += CELL_SIZE;
                    break;
                case Direction.Left:
                    bulletX -= 1;
                    bulletY += CELL_SIZE / 2;
                    break;
                case Direction.Right:
                    bulletX += CELL_SIZE;
                    bulletY += CELL_SIZE / 2;
                    break;
            }

            var bullet = new Bullet(bulletX, bulletY, _direction, true);
            game.AddPlayerBullet(bullet);
        }
    }
} 