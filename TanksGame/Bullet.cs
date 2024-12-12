namespace TanksGame
{
    public class Bullet
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        private readonly Direction _direction;
        public bool IsActive { get; private set; }
        public bool IsPlayerBullet { get; }

        public Bullet(int x, int y, Direction direction, bool isPlayerBullet)
        {
            X = x;
            Y = y;
            _direction = direction;
            IsActive = true;
            IsPlayerBullet = isPlayerBullet;
        }

        public void Update(Map map)
        {
            if (!IsActive) return;

            int newX = X;
            int newY = Y;

            switch (_direction)
            {
                case Direction.Up: newY--; break;
                case Direction.Down: newY++; break;
                case Direction.Left: newX--; break;
                case Direction.Right: newX++; break;
            }

            int mapX = newX / Tank.CELL_SIZE;
            int mapY = newY / Tank.CELL_SIZE;

            if (map.IsWall(mapX, mapY))
            {
                IsActive = false;
                return;
            }

            if (map.IsDestructible(mapX, mapY))
            {
                map.DamageWall(mapX, mapY);
                IsActive = false;
                return;
            }

            if (!map.CanShootThrough(mapX, mapY))
            {
                IsActive = false;
                return;
            }

            X = newX;
            Y = newY;
        }

        public bool CollidesWith(int targetX, int targetY)
        {
            int bulletMapX = X / Tank.CELL_SIZE;
            int bulletMapY = Y / Tank.CELL_SIZE;
            int targetMapX = targetX / Tank.CELL_SIZE;
            int targetMapY = targetY / Tank.CELL_SIZE;

            return bulletMapX == targetMapX && bulletMapY == targetMapY;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
} 