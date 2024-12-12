namespace TanksGame
{
    public class Tank
    {
        // Защищенные поля для наследников
        protected int _x;  // Позиция танка по X
        protected int _y;  // Позиция танка по Y
        protected Direction _direction;  // Направление танка
        protected bool _isPlayer;  // Флаг игрока
        protected readonly Dictionary<Direction, (char[,] body, char gun)> _tankSymbols;
        protected readonly int _size;

        // Константы
        public const int CELL_SIZE = 2;  // Размер клетки для танка

        // Конструктор танка
        protected Tank(int x, int y, bool isPlayer, char bodyChar = ' ', int size = 2)
        {
            _x = x * CELL_SIZE;
            _y = y * CELL_SIZE;
            _direction = Direction.Right;
            _size = size;
            _isPlayer = isPlayer;

            // Если символ не задан, используем разные символы для игрока и врагов
            if (bodyChar == ' ')
            {
                bodyChar = isPlayer ? '■' : '□';
            }

            // Задаем символы для пушки
            char gunVertical = isPlayer ? '║' : '│';
            char gunHorizontal = isPlayer ? '═' : '─';

            // Создаем тело танка
            var body = new char[size, size];
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    body[i, j] = bodyChar;

            // Создаем словарь с символами для разных направлений
            _tankSymbols = new Dictionary<Direction, (char[,], char)>
            {
                { Direction.Up, (body, gunVertical) },
                { Direction.Down, (body, gunVertical) },
                { Direction.Left, (body, gunHorizontal) },
                { Direction.Right, (body, gunHorizontal) }
            };
        }

        // Методы для получения и установки позиции
        public int GetX() => _x;
        public int GetY() => _y;
        public void SetX(int value) => _x = value;
        public void SetY(int value) => _y = value;

        // Виртуальные методы для переопределения в наследниках
        public virtual void Update(Map map, PlayerTank player = null)
        {
            // Базовая реализация пустая
        }

        // Метод для отрисовки танка
        public virtual void Draw(char[,] screen)
        {
            var (body, gun) = _tankSymbols[_direction];
            
            // Рисуем корпус танка
            for (int i = 0; i < _size; i++)
            {
                for (int j = 0; j < _size; j++)
                {
                    if (_y + i < screen.GetLength(0) && _x + j < screen.GetLength(1))
                    {
                        screen[_y + i, _x + j] = body[i, j];
                    }
                }
            }
            
            // Определяем позицию пушки
            int gunX = _x;
            int gunY = _y;

            switch (_direction)
            {
                case Direction.Up:
                    gunX += _size / 2;
                    gunY -= 1;
                    break;
                case Direction.Down:
                    gunX += _size / 2;
                    gunY += _size;
                    break;
                case Direction.Left:
                    gunX -= 1;
                    gunY += _size / 2;
                    break;
                case Direction.Right:
                    gunX += _size;
                    gunY += _size / 2;
                    break;
            }

            // Рисуем пушку если она в пределах экрана
            if (gunY >= 0 && gunY < screen.GetLength(0) && 
                gunX >= 0 && gunX < screen.GetLength(1))
            {
                screen[gunY, gunX] = gun;
            }
        }

        // Метод для получения символов танка
        public (char[,] body, char gun) GetTankSymbols()
        {
            return _tankSymbols[_direction];
        }
    }
} 