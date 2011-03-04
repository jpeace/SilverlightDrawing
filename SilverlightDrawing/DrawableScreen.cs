using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SilverlightDrawing
{
    public interface IScreen
    {
        double Width { get; set; }
        double Height { get; set; }

        void Draw();
    }
    
    public interface ITile
    {
        int Width { get; set; }
        int Height { get; set; }
        UIElement Draw();
    }

    public class ColoredTile : ITile
    {
        public int Width { get; set; }
        public int Height { get; set; }
        
        public ColoredTile(Color c)
        {
            Color = c;
        }
        public Color Color { get; set; }
        
        public UIElement Draw()
        {
            return new Rectangle {Width = Width, Height = Height, Fill = new SolidColorBrush(Color)};
        }
    }

    public class DrawableScreen : Canvas, IScreen
    {
        private readonly IList<UIElement> _elements;
        
        public DrawableScreen()
        {
            _elements = new List<UIElement>();
        }

        public void DrawLine(int x1, int y1, int x2, int y2)
        {
            var line = new Line {X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Stroke = new SolidColorBrush(Colors.Black)};
            _elements.Add(line);
        }

        public void DrawElementAt(int x, int y, UIElement element)
        {
            element.SetValue(LeftProperty, (double)x);
            element.SetValue(TopProperty, (double)y);
            _elements.Add(element);
        }
        public void DrawShapeAt(int x, int y, Shape shape)
        {
            DrawElementAt(x, y, shape);
        }

        public void Refresh()
        {
            Children.Clear();

            Draw();

            foreach (var e in _elements)
            {
                Children.Add(e);
            }
        }

        public virtual void Draw(){}
    }

    public class TiledScreen : DrawableScreen
    {
        private readonly TileGrid _tiles;

        public TiledScreen()
        {
            _tiles = new TileGrid {TileSize = 80};

            Width = 640;
            Height = 480;

            _tiles[0][0] = new ColoredTile(Colors.White);
            _tiles[5][7] = new ColoredTile(Colors.White);
            _tiles[2][2] = new ColoredTile(Colors.Red);
            _tiles[4][4] = new ColoredTile(Colors.Black);
        }

        public override void Draw()
        {
            int curX = 0;
            int curY = 0;

            foreach (var t in _tiles)
            {
                DrawElementAt(curX, curY, t.Draw());
                curX += _tiles.TileSize;
                if (curX >= Width)
                {
                    curX = 0;
                    curY += _tiles.TileSize;
                }
            }
        }
    }

    public class TileGrid : IEnumerable<ITile>
    {
        private ITile[][] _tiles;
        private int _numRows = -1;
        private int _numCols = -1;

        public Func<int, int, ITile> OnMissing { get; set; }

        public TileGrid()
        {
            OnMissing = (x, y) => new EmptyTile();
        }

        public int TileSize { get; set; }

        public ITile GetTileAt(int col, int row)
        {
            CheckAccess(ref col, ref row);
            if (_tiles[row][col] == null)
            {
                _tiles[row][col] = OnMissing(col, row);
            }
            return _tiles[row][col];
        }

        public void SetTileAt(ITile tile, int col, int row)
        {
            CheckAccess(ref col, ref row);
            tile.Width = TileSize;
            tile.Height = TileSize;
            _tiles[row][col] = tile;
        }

        private void CheckAccess(ref int col, ref int row)
        {
            if (col < 0) col = 0;
            if (row < 0) row = 0;

            if (row > _numRows)
            {
                _numRows = row + 1;
                _tiles = _tiles.Resize(_numRows);
            }

            if (col > _numCols)
            {
                _numCols = col + 1;
                for (var r = 0; r < _tiles.Length; ++r)
                {
                    if (_tiles[r] == null) _tiles[r] = new ITile[_numCols];
                    if (_tiles[r].Length < _numCols) _tiles[r] = _tiles[r].Resize(_numCols);
                }
            }
        }

        public TileGridRow this[int index]
        {
            get { return new TileGridRow(this, index); }
        }

        public int NumberOfRows { get { return _numRows; } }
        public int NumberOfColumns { get { return _numCols; } }

        public IEnumerator<ITile> GetEnumerator()
        {
            for (var row = 0 ; row < NumberOfRows ; ++row)
            {
                for (var col = 0 ; col < NumberOfColumns ; ++col)
                {
                    yield return GetTileAt(col, row);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class TileGridRow : IEnumerable<ITile>
    {
        private readonly TileGrid _grid;
        private readonly int _rowIndex;

        public TileGridRow(TileGrid grid, int rowIndex)
        {
            _grid = grid;
            _rowIndex = rowIndex;
        }

        public ITile this[int index]
        {
            get { return _grid.GetTileAt(index, _rowIndex); }
            set { _grid.SetTileAt(value, index, _rowIndex); }
        }

        public IEnumerator<ITile> GetEnumerator()
        {
            for (var i = 0 ; i < _grid.NumberOfColumns ; ++i)
            {
                yield return _grid.GetTileAt(i, _rowIndex);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class EmptyTile : ITile
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public UIElement Draw()
        {
            return new Canvas { Width = Width, Height = Height };
        }
    }

    public static class ArrayExtensions
    {
        public static T[] Resize<T>(this T[] a, int newSize)
        {
            Array.Resize(ref a, newSize);
            return a;
        }
    }
}