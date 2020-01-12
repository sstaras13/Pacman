using PacmanEngine.Components.Actors;
using System.Collections.Generic;
using PacmanEngine.Components.Base;
using PacmanEngine.Components.Graphics;

namespace Pacman
{
    public enum InitialData
    {
        Wall,
        Empty,
        SmallCoin,
        BigCoin,
        Pacman,
        Blinky,
        Pinky,
        Inky,
        Clyde
    }
    public enum StaticGameObjectType
    {
        BigCoin, SmallCoin, BackGrnd
    }
    class PointData
    {
        public Coordinate Coord { get; set; }
        public InitialData InitData { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Engine.Run(InitCollection());
        }
        private static IEnumerable<IGameObject> InitCollection()
        {
            var mazeData = ConfigurationManager.AppSettings["MazeData"];

            PointData[] InitData = mazeData.
                Split(' ').
                Select(x => x.Select(y => int.Parse(y.ToString())).ToArray()).
                Select((arr, Y) =>
                arr.Select((Number, X) => new PointData() { Coord = new Coordinate(X, Y), InitData = (InitialData)Number })).
                SelectMany(x => x).ToArray();

            var coll = new List<IGameObject>();

            coll.Add(new Pacman());
            coll.Add(BaseObject.CreateStaticObject(AnimationType.MazeBlue, 0, 0));

            coll.Add(BaseObject.CreateStaticObject(AnimationType.SmallCoin, 1, 2));
            coll.Add(BaseObject.CreateStaticObject(AnimationType.SmallCoin, 3, 4));
            coll.Add(BaseObject.CreateStaticObject(AnimationType.SmallCoin, 7, 4));
            coll.Add(BaseObject.CreateStaticObject(AnimationType.BigCoin, 8, 4));

            return coll;
        }

        static BaseObject CreateObject(PointData pt)
        {
            BaseObject result = null;

            switch (pt.InitData)
            {
                case InitialData.Pacman:
                    result = new Pacman();
                    result.Animation.Location = pt.Coord;
                    break;
                case InitialData.BigCoin:
                    BaseObject.CreateStaticObject(AnimationType.SmallCoin, pt.Coord.X, pt.Coord.Y);
            }

            {
                var pac = new Pacman();
                pac.Animation.Location = pt.Coord;
                return pac;
            }
        }

    }
           
    public class BaseObject : IGameObject
    {
        public Animation Animation { get; set; }

        public bool IsEnabled { get; set; } = true;

        public string Name { get; set; }

        public static BaseObject CreateStaticObject(AnimationType type, int xPos, int yPos)
        {
            BaseObject result = null;

            switch(type)
            {
                case AnimationType.MazeBlue:
                    result = new BaseObject()
                    {
                        Animation = AnimationFactory.CreateAnimation(AnimationType.MazeBlue)
                    };
                    break;
                case AnimationType.BigCoin:
                    result = new BaseObject()
                    {
                        Animation = AnimationFactory.CreateAnimation(AnimationType.BigCoin)
                    };
                    break;
                case AnimationType.SmallCoin:
                    result = new BaseObject()
                    {
                        Animation = AnimationFactory.CreateAnimation(AnimationType.SmallCoin),
                    };
                    break;
            }

            if (result != null)
            {
                result.Name = type.ToString();
                result.Animation.Location = new Coordinate(xPos, yPos);

            }
               
            return result;

        }

        public virtual void Update() { }
    }
    
    public class Pacman : BaseObject, IProtagonist
    {
        private DirectionKeys CurrentDirection = DirectionKeys.Right;

        public float Speed { get; set; } = 0.1f;

        public DirectionKeys PressedKeys { get; set; }

        public Pacman()
        {
            Animation = AnimationFactory.CreateAnimation(AnimationType.PacmanRight);
            Animation.Location = new Coordinate(2, 2);
        }
        public void Collide(IEnumerable<IGameObject> collisions)
        {
            foreach (var obj in collisions)
                if (obj.Name == AnimationType.SmallCoin.ToString())
                    obj.IsEnabled = false;
        }
        public override void Update()
        {
            DirectionKeys newDirection = DirectionKeys.None;
            if ((PressedKeys & DirectionKeys.Left) == DirectionKeys.Left)
                newDirection = DirectionKeys.Left;
            else if ((PressedKeys & DirectionKeys.Right) == DirectionKeys.Right)
                newDirection = DirectionKeys.Right;
            else if ((PressedKeys & DirectionKeys.Up) == DirectionKeys.Up)
                newDirection = DirectionKeys.Up;
            else if ((PressedKeys & DirectionKeys.Down) == DirectionKeys.Down)
                newDirection = DirectionKeys.Down;

            if (CurrentDirection != newDirection && newDirection != DirectionKeys.None)
            {
                Animation newAnimation;
                switch (newDirection)
                {
                    case DirectionKeys.Left:
                        newAnimation = AnimationFactory.CreateAnimation(AnimationType.PacmanLeft);
                        break;
                    case DirectionKeys.Right:
                        newAnimation = AnimationFactory.CreateAnimation(AnimationType.PacmanRight);
                        break;
                    case DirectionKeys.Up:
                        newAnimation = AnimationFactory.CreateAnimation(AnimationType.PacmanUp);
                        break;
                    default:
                        newAnimation = AnimationFactory.CreateAnimation(AnimationType.PacmanDown);
                        break;
                }

                newAnimation.Location = Animation.Location;
                Animation = newAnimation;
                CurrentDirection = newDirection;
            }

            switch (CurrentDirection)
            {
                case DirectionKeys.Left:
                    Animation.Location -= new Coordinate(Speed, 0);
                    break;
                case DirectionKeys.Right:
                    Animation.Location += new Coordinate(Speed, 0);
                    break;
                case DirectionKeys.Up:
                    Animation.Location -= new Coordinate(0, Speed);
                    break;
                default:
                    Animation.Location += new Coordinate(0, Speed);
                    break;
            }

        }

    }
}