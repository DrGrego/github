using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace VectorRedactor
{
    class Circle : Figure // класс КРУГ
    {
        protected int _xc, _yc, _r; // центр и радиус

        // конструктор класса
        public Circle(int xc, int yc, int r, Color ClrBrush, Color ClrPen, int penWidth, DashStyle ds) :
            // вызываем конструктор для базового класса, чтобы установить
            // параметры кисти и пера
            base(ClrBrush, ClrPen, penWidth, ds)
        {
            this._xc = xc;
            this._yc = yc;
            this._r = r;
        }

        public override void Paint(Graphics g)
        {
            // рисование заливки
            g.FillEllipse(b, _xc - _r, _yc - _r, 2 * _r, 2 * _r);

            // если перо невидимое, контур не рисуем
            if (IsStealthPen == true) return;
            // рисование контура
            g.DrawEllipse(p, _xc - _r, _yc - _r, 2 * _r, 2 * _r);
        }

        public override void Move(int dx, int dy)
        {
            _xc += dx;
            _yc += dy;
        }

        public override void Resize(int dx, int dy)
        {
            // следим, чтобы размер не стал меньше допустимого
            if (_r + dx >= BoundarySize) _r += dx;
        }

        // содержит ли круг заданную точку?
        public override bool Contains(int x, int y)
        {
            int dx = x - _xc, dy = y - _yc;

            // следует из уравнения для круга
            return (dx * dx + dy * dy <= _r * _r);
        }
        //Метод для генерации svg строки
        public override string SaveToSVG()
        {
            string clrStroke =ColorTranslator.ToHtml(p.Color);
            string clrFill = ColorTranslator.ToHtml(b.Color);
            string str = $"<circle cx=\"{_xc}\" cy=\"{_yc}\" r=\"{_r}\" stroke=\"{clrStroke}\" stroke-width=\"{p.Width}\" ";
            if (p.DashStyle == DashStyle.Solid) str+="";
            if (p.DashStyle == DashStyle.Dash) 
            {
                if (p.Width < 3) str += "stroke-dasharray=\"5,5\" ";
                else str += $"stroke-dasharray=\"{p.Width * 2},{p.Width * 2}\" ";
            }
            if (p.DashStyle == DashStyle.DashDot)
            {
                if (p.Width < 3) str += "stroke-dasharray=\"10,5,5,5\" ";
                else str += $"stroke-dasharray=\"{p.Width*4},{p.Width*2},{p.Width*2},{p.Width*2}\" ";
            }
            str += $"fill=\"{clrFill}\" ";
            str += "/>";
            return str;
        }
    }


    class Rectangle : Figure // класс ПРЯМОУГОЛЬНИК
    {
        protected int _xleft, _ytop; // левый верхний угол
        protected int _w, _h; // ширина и высота

        // конструктор класса
        public Rectangle(int xleft, int ytop, int w, int h, Color ClrBrush, Color ClrPen, int penWidth, DashStyle ds) :
            base(ClrBrush, ClrPen, penWidth, ds)
        {
            _xleft = xleft;
            _ytop = ytop;
            _w = w;
            _h = h;
        }

        public override void Paint(Graphics g)
        {
            // рисование заливки
            g.FillRectangle(b, _xleft, _ytop, _w, _h);

            if (IsStealthPen == true) return;
            // рисование контура
            g.DrawRectangle(p, _xleft, _ytop, _w, _h);
        }

        public override void Move(int dx, int dy)
        {
            _xleft += dx;
            _ytop += dy;
        }

        public override void Resize(int dx, int dy)
        {
            if (_w + dx >= BoundarySize && _h + dy >= BoundarySize)
            {
                _w += dx;
                _h += dy;
            }
        }

        // содержит ли прямоугольник заданную точку?
        public override bool Contains(int x, int y)
        {
            return (x >= _xleft && x <= _xleft + _w &&
                y >= _ytop && y <= _ytop + _h);
        }
        public override string SaveToSVG()
        {
            string clrStroke = ColorTranslator.ToHtml(p.Color);
            string clrFill = ColorTranslator.ToHtml(b.Color);
            string str = $"<rect x=\"{_xleft}\" y=\"{_ytop}\" width=\"{_w}\" height=\"{_h}\" stroke=\"{clrStroke}\" stroke-width=\"{p.Width}\" ";
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

    class Square : Rectangle // класс КВАДРАТ
    {
        // конструктор класса
        public Square(int xleft, int ytop, int w, Color ClrBrush, Color ClrPen, int penWidth, DashStyle ds) :
            base(xleft, ytop, w, w, ClrBrush, ClrPen, penWidth, ds)
        {
            // у квадрата нет дополнительных полей по сравнению с Rectangle,
            // тут ничего не делается
        }

        public override void Resize(int dx, int dy)
        {
            if (_w + dx >= BoundarySize)
            {
                _w += dx;
                _h += dx;
            }
        }
        public override string SaveToSVG()
        {
            string clrStroke = ColorTranslator.ToHtml(p.Color);
            string clrFill = ColorTranslator.ToHtml(b.Color);
            string str = $"<rect x=\"{_xleft}\" y=\"{_ytop}\" width=\"{_w}\" height=\"{_h}\" stroke=\"{clrStroke}\" stroke-width=\"{p.Width}\" ";
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

    class Pie : Circle // класс СЕКТОР
    {
        // стартовый и конечный угол в градусах (по часовой стрелке)
        double StartAngleGrad, EndAngleGrad;

        // конструктор класса
        public Pie(int xc, int yc, int r,
            double StartFiGrad, double DeltaFiGrad,
            Color ClrBrush, Color ClrPen, int penWidth, DashStyle ds) :
            base(xc, yc, r, ClrBrush, ClrPen, penWidth, ds)
        {
            this.StartAngleGrad = StartFiGrad;
            this.EndAngleGrad = StartFiGrad + DeltaFiGrad;
        }

        public override void Paint(Graphics g)
        {
            // угол сектора = конечный угол - начальный угол
            double DeltaAngle = EndAngleGrad - StartAngleGrad;

            // рисование заливки
            g.FillPie(b, _xc - _r, _yc - _r, 2 * _r, 2 * _r, (float)StartAngleGrad, (float)DeltaAngle);

            if (IsStealthPen == true) return;
            // рисование контура
            g.DrawPie(p, _xc - _r, _yc - _r, 2 * _r, 2 * _r, (float)StartAngleGrad, (float)DeltaAngle);
        }

        // находится ли угол fi в диапазоне [StartAngleGrad; EndAngleGrad]
        bool AngleInRange(double fi)
        {
            return (fi >= StartAngleGrad && fi <= EndAngleGrad);
        }

        // получить угол, соответствующий заданной точке
        double GetFiRad(double x, double y)
        {
            double r = Math.Sqrt(x * x + y * y);

            // угол (нормируем на (-Пи; Пи])
            // 1) для x = 0 или y = 0
            if (x == 0 || y == 0)
            {
                // при x = 0 угол либо -Пи/2, либо Пи/2
                if (x == 0)
                    return (y >= 0) ? 0.5 * Math.PI : (-0.5 * Math.PI);
                // при y = 0 угол либо 0, либо Пи
                return (x >= 0) ? 0 : Math.PI;
            }

            // 2) для ненулевых x, y

            // 1-ая или 4-ая четверть
            if (x > 0) return Math.Atan(y / x);

            // 2-ая или 3-я четверть
            if (y > 0) // 2-ая четверть
                return Math.PI + Math.Atan(y / x);
            return Math.Atan(y / x) - Math.PI;
        }

        // содержит ли сектор заданную точку?
        public override bool Contains(int x, int y)
        {
            // получаем угол, соответствующий точке, и его градусы
            double fi = RadToGrad(GetFiRad(x - _xc, y - _yc));

            // fi + 360: без этого будут проблемы, StartAngleGrad < 180,
            // EndAngleGrad > 180
            if (AngleInRange(fi) == false && AngleInRange(fi + 360) == false)
                return false;

            // если угол подходящий, это еще не значит, что точка внутри сектора;
            // проще всего для проверки использовать метод для класса MyCircle
            return base.Contains(x, y);
        }

        // вычисление точки по углу, данному в градусах
        PointF AngleToPoint(double fiGrad)
        {
            // переводим в радианы
            double fi = GradToRad(fiGrad);

            float x = (float)(_xc + _r * Math.Cos(fi));
            float y = (float)(_yc + _r * Math.Sin(fi));

            return new PointF(x, y);
        }
        public override string SaveToSVG()
        {
            PointF StartPoint = AngleToPoint(StartAngleGrad);
            PointF EndPoint = AngleToPoint(EndAngleGrad); 
            string clrStroke = ColorTranslator.ToHtml(p.Color);
            string clrFill = ColorTranslator.ToHtml(b.Color);
            string str = $"<path d=\"M {_xc} {_yc} L {StartPoint.X} {StartPoint.Y} A {_r} {_r} 0 0 1 {EndPoint.X} {EndPoint.Y} Z\" stroke=\"{clrStroke}\" stroke-width=\"{p.Width}\" ";
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