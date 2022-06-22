using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace VectorRedactor 
{ 
    class TrueNAngle : Figure
    {
        PointF[] _Vertices; // координаты вершин
        double _r; // радиус описанной окружности
        int _xc, _yc; // центр описанной окружности
        double _startFi; // угол, соответствующей 1-ой вершине

        // функция расчета координат вершин
        void GetByRAndCenter(int n)
        {
            // угол для текущей вершине
            double fi = _startFi;

            // шаг угла
            double delta = 2.0 * Math.PI / n;

            // идем с заданным шагом угла
            for (int i = 0; i < n; i++)
            {
                // координаты текущей вершины - на основе параметрического
                // уравнения окружности
                float x = (float)(_xc + _r * Math.Cos(fi));
                float y = (float)(_yc + _r * Math.Sin(fi));

                _Vertices[i] = new PointF(x, y);

                fi += delta;
            }
        }

        // конструктор класса
        public TrueNAngle(int xc, int yc,
            double r, // радиус описанной окружности
            int n, // число вершин
            // угол 1-ой вершины (по час. стрелке от оси X, в град.)
            double startFi, 
            Color ClrBrush, Color ClrPen, int penWidth, DashStyle ds) :
            base(ClrBrush, ClrPen, penWidth, ds)
        {
            this._xc = xc;
            this._yc = yc;
            this._r = r;

            // переводим градусы в радианы
            this._startFi = GradToRad(startFi);
            
            // отводим память под вершины
            _Vertices = new PointF[n];
            GetByRAndCenter(n); // вычисляем вершины
        }

        public override void Paint(Graphics g)
        {
            // рисование заливки
            g.FillPolygon(b, _Vertices);

            // если перо невидимое, контур не рисуем
            if (IsStealthPen == true) return;
            // рисование контура
            g.DrawPolygon(p, _Vertices);
        }

        public override void Move(int dx, int dy)
        {
            // перемещаем центр
            _xc += dx;
            _yc += dy;

            // перемещаем вершины
            for (int i = 0; i < _Vertices.Length; i++)
                // PointF - структурный тип, нельзя просто _Vertices[i].X += dx
                // если бы PointF был классом, можно было бы и так
                _Vertices[i] = new PointF(_Vertices[i].X + dx,
                    _Vertices[i].Y + dy);
        }

        public override void Resize(int dx, int dy)
        {
            // следим, чтобы размер не стал меньше допустимого
            if (_r + dx >= BoundarySize)
            {
                _r += dx;

                // пересчитываем координаты вершин
                GetByRAndCenter(_Vertices.Length);
            }
        }

        // функция расчёта положения точки D(xd, yd) относительно прямой AB
        double pPosition(double xa, double ya, double xb, double yb, double xd, double yd)
        {
            return (xd - xa) * (yb - ya) - (yd - ya) * (xb - xa);
        }

        // Лежат ли точки C и D с одной стороны прямой (AB)
        // (или лежит ли одна из них на прямой)?
        bool ByOneSide(double xa, double ya, double xb, double yb, double xc, double yc, double xd, double yd)
        {
            return (pPosition(xa, ya, xb, yb, xc, yc) * pPosition(xa, ya, xb, yb, xd, yd) >= 0);
        }

        // лежит ли точка D внутри или на стороне треуг. ABC?
        bool InTriangle(double xa, double ya, double xb, double yb, double xc, double yc, double xd, double yd)
        {
            // C и D должны быть по одну сторону от AB,
            // A и D должны быть по одну сторону от BC,
            // B и D должны быть по одну сторону от AC
            return (ByOneSide(xa, ya, xb, yb, xc, yc, xd, yd) == true &&
                ByOneSide(xb, yb, xc, yc, xa, ya, xd, yd) == true &&
                ByOneSide(xc, yc, xa, ya, xb, yb, xd, yd) == true);
        }

        // содержит ли многоугольник заданную точку?
        public override bool Contains(int x, int y)
        {
            int n = _Vertices.Length;

            // проверяем, лежит ли точка внутри или на стороне
            // одного из треугольников P0P1P2 ... P0Pn-1Pn
            for (int i = 1; i < n - 1; i++)
            {
                // если точка внутри или на стороне одного
                // из таких треугольников
                if (InTriangle(_Vertices[0].X, _Vertices[0].Y,
                    _Vertices[i].X, _Vertices[i].Y,
                    _Vertices[i + 1].X, _Vertices[i + 1].Y, x, y) == true)
                    return true;
            }

            // если точка не попадает ни в один из треугольников
            return false;
        }
        public override string SaveToSVG()
        {
            string clrStroke = ColorTranslator.ToHtml(p.Color);
            string clrFill= ColorTranslator.ToHtml(b.Color);
            string str = $"<path d=\"M {_Vertices[0].X} {_Vertices[0].Y} ";
            for (int i = 1; i < _Vertices.Length; i++) str += $"L {_Vertices[i].X} {_Vertices[i].Y} ";
            str += $"Z\" stroke=\"{clrStroke}\" stroke-width=\"{p.Width}\" ";
            if (p.DashStyle == DashStyle.Solid) str += "";
            if (p.DashStyle == DashStyle.Dash)
            {
                if (p.Width < 3) str += "stroke-dasharray=\"5,5\" ";
                else str += $"stroke-dasharray=\"{p.Width * 2},{p.Width * 2}\" ";
            }
            if (p.DashStyle == DashStyle.DashDot)
            {
                if (p.Width < 3) str += "stroke-dasharray=\"10,5,5,5\" ";
                else str += $"stroke-dasharray=\"{p.Width * 4},{p.Width * 2},{p.Width * 2},{p.Width * 2}\" ";
            }
            str += $"fill=\"{clrFill}\" ";
            str += "/>";
            return str;
        }
    }
}